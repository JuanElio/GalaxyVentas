
# Proyecto Ventas
- ASP.NET Core MVC sobre .NET 10, escrita en C# con vistas Razor y jQuery, Clean Architecture.
- Requerimientos: `Docs/requerimientos-funcionales.md` es la fuente de verdad (RF, RN, RNF y supuestos P-xx). Prototipos navegables en `Docs/prototipos/`.
- Esquema de BD: `Docs/database/BDVentas.sql` (tablas) y `Docs/database/StoreProcedure.sql` (procedimientos de referencia). No inventar tablas ni columnas.
- Domain no depende de nada. Application solo depende de Domain.
- Usar DTOs / view models; nunca exponer entidades de EF en los controllers ni en las vistas.
- Validaciones con FluentValidation; los errores de las llamadas AJAX se devuelven como ProblemDetails (RFC 7807).
- Autenticación con cookie de ASP.NET Core y token antifalsificación en todo POST.
- Métodos async con CancellationToken.
- Cada servicio nuevo debe tener pruebas unitarias en tests/Ventas.UnitTests.
- Comentarios y mensajes de error en español.

## Contexto
- El operador  ingresa a la plataforma del DOP con usuario y contraseña.
- Realiza la busqueda del cliente o registra un nuevo cliente
- Asigna los articulos, la cantidad que esta disponible para la venta
- En base al monton de la venta registra el medio del pago que puede ser con Tarjeta de credito, tarjeta de debito, deposito o efectivo.
- Genera el comprobante factura o boleta.
- Al fianl puede generar su reporte de registro de venta.

## Stack

ASP.NET Core MVC sobre .NET 10 (`net10.0`) con vistas Razor y jQuery, autenticación por cookie, EF Core 10 con SQL Server, FluentValidation. OpenAPI + Scalar (ya referenciados en el proyecto web) solo documentan las acciones JSON que invoca jQuery. Los tests usan xUnit, Moq y FluentAssertions; los tests de integración usan `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`).

## Comandos

Ejecutar desde la raíz del repositorio (`Ventas.slnx` incluye los seis proyectos; los nuevos se registran con `dotnet sln Ventas.slnx add <ruta>.csproj`).

```bash
dotnet build
dotnet test                                         # todos los proyectos de test
dotnet run --project src/Ventas.Api                 # http://localhost:5070 / https://localhost:7205
dotnet test tests/Ventas.UnitTests
dotnet test tests/Ventas.IntegrationTests
dotnet test tests/Ventas.UnitTests --filter "FullyQualifiedName~NombreClase.NombreMetodo"   # un solo test
```

No usar migraciones de EF Core (`dotnet ef migrations` / `database update`): la base de datos ya existe y su esquema no se modifica.

## Arquitectura

Clean Architecture con cuatro capas en `src/`. Las dependencias apuntan hacia el dominio y están impuestas por las referencias entre proyectos:
Ventas/
├── src/
│   ├── Ventas.Domain          → Entidades y reglas de negocio puras (sin dependencias)
│   ├── Ventas.Application     → Casos de uso, DTOs, interfaces, validaciones
│   ├── Ventas.Infrastructure  → EF Core, DbContext, repositorios, SQL Server
│   └── Ventas.Api             → Capa web MVC: controllers, vistas Razor, jQuery (wwwroot), autenticación, middleware
└── tests/
    ├── Ventas.UnitTests
    └── Ventas.IntegrationTests

```
Web (Ventas.Api) ──> Application ──> Domain
 └──────────────> Infrastructure ──> Application
```

El proyecto web referencia Infrastructure **solo** para registrar servicios en `Program.cs`; los controllers nunca usan el `DbContext` ni repositorios directamente. El proyecto conserva el nombre `Ventas.Api` de la plantilla, aunque es una aplicación MVC.

### Principios

1. **Regla de dependencia**: una capa solo conoce a las que están más al centro. Domain no sabe que existe una base de datos ni HTTP; Application no sabe que se usa EF Core ni SQL Server.
2. **Inversión de dependencias**: cuando Application necesita algo externo (leer la BD, obtener un correlativo, calcular stock, saber quién es el usuario de la sesión) declara una interfaz y la implementa Infrastructure o la capa web. En ejecución, la DI inyecta la implementación concreta.
3. **El esquema heredado queda encerrado en Infrastructure**: los nombres `SGP_*`, `vch*`, `num*`, los tipos incoherentes y los flags `'S'/'N'` solo aparecen en las configuraciones de EF. El resto del código trabaja con nombres limpios y tipos correctos (`bool`, `decimal`, `long`).
4. **Las reglas de negocio viven en Domain**: cálculo de totales, IGV e invariantes son métodos de las entidades, testeables sin BD ni mocks. Application orquesta (carga, valida, llama al dominio, persiste) pero no calcula.

```csharp
// Application/Ventas — declara lo que necesita
public interface IVentaRepository
{
    Task AgregarAsync(Venta venta, CancellationToken ct);
}

// Infrastructure/Ventas — implementa con EF Core
public class VentaRepository(VentasDbContext db) : IVentaRepository
{
    public async Task AgregarAsync(Venta venta, CancellationToken ct) =>
        await db.Ventas.AddAsync(venta, ct);
}

// Infrastructure/Persistence/Configurations — único lugar con nombres reales
builder.ToTable("SGP_CABECERA_TRANSACCION_BS");
builder.Property(v => v.Id).HasColumnName("numCodTransaccion");
```

### Entidades de dominio ↔ tablas

| Entidad (Domain) | Tabla real |
|---|---|
| `Venta` / `DetalleVenta` | `SGP_CABECERA_TRANSACCION_BS` / `SGP_DETALLE_TRANSACCION_BS` |
| `Comprobante` / `DetalleComprobante` | `SGP_COMPROBANTE_BS` / `SGP_DETALLE_COMPROBANTE_BS` |
| `MovimientoComprobante` | `SGP_MOVIMIENTO_TRANSACCION_BS` |
| `MedioPago` / `PagoComprobante` | `SGP_MEDIO_PAGO_BS` / `SGP_MEDIO_PAGO_COMPROBANTE_BS` |
| `MovimientoKardex` | `PGA_TRANSACCION_KARDEX` |
| `Cliente`, `Articulo`, `Tarifa`, `TarifaCanal`, `PuntoVenta`, `Impresora`, `Banco`, `Usuario`, `TipoOrden` | `SGP_CLIENTE`, `SGP_ARTICULO`, `SGP_TARIFA`, `SGP_TARIFA_CANAL`, `SGP_PUNTO_VENTA`, `SGP_IMPRESORA`, `SGP_BANCO`, `SGP_USUARIO`, `SGP_TIPO_ORDEN` |
| `ItemCatalogo` | `SGP_DETALLE_TABLA` (filtrando `chrSituacionRegistro = 'A'`) |

`Venta` es la raíz del agregado: sus `DetalleVenta` solo se crean y modifican a través de ella, igual que `Comprobante` con sus detalles.

### Responsabilidades por capa

| Capa | Contiene | No debe contener |
|---|---|---|
| **Domain** | Entidades con nombres limpios en C# (`Venta`, `DetalleVenta`, `Comprobante`, `MedioPago`...), cálculo de totales e IGV (0.18), invariantes (cantidad > 0, total = Σ líneas), excepciones de dominio. | Atributos o referencias de EF, nombres de columnas `vch`/`num`, DTOs. |
| **Application** | Servicios de caso de uso (`VentaService`, `ComprobanteService`, `MedioPagoService`, `ClienteService`, `AutenticacionService`, `ReporteVentasService`), DTOs de entrada/salida y su mapeo, validadores FluentValidation, interfaces que implementan otras capas (repositorios, `IUnitOfWork`, `IStockService`, `ICorrelativoService`, `IUsuarioActual`). | EF Core, SQL, `HttpContext`, cookies. |
| **Infrastructure** | `VentasDbContext`, una `IEntityTypeConfiguration<T>` por tabla con el mapeo a nombres reales (`ToTable("SGP_COMPROBANTE_BS")`, `HasColumnName("vchNumSerie")`, conversiones de tipo y de flags `'S'/'N'`), repositorios, unit of work, cálculo de stock sobre `PGA_TRANSACCION_KARDEX`, obtención del correlativo en `SGP_IMPRESORA`, verificación de la contraseña contra `SGP_USUARIO`. | Reglas de negocio. |
| **Web (`Ventas.Api`)** | Controllers MVC delgados (reciben el modelo, llaman al servicio, devuelven una vista o JSON), vistas Razor y view models, jQuery en `wwwroot`, autenticación por cookie y `IUsuarioActual` (lee los claims de la sesión), antifalsificación, manejo global de excepciones → ProblemDetails en las llamadas AJAX, composición de DI. | Lógica de negocio, entidades de EF en vistas o respuestas. |

### Convenciones de organización

- Carpetas por funcionalidad dentro de cada capa: `Ventas/`, `Comprobantes/`, `Pagos/`, `Maestros/` (clientes, artículos, tarifas, catálogos), `Seguridad/` (inicio de sesión), `Reportes/`.
- Cada capa expone su registro de DI con un método de extensión: `services.AddApplication()` y `services.AddInfrastructure(configuration)`, invocados desde `Program.cs`.
- Los validadores se ejecutan en el servicio de Application (inyectando `IValidator<T>`); un fallo lanza una excepción de validación. En una llamada AJAX la capa web la traduce a ProblemDetails 400; en un formulario la muestra en la misma vista. Excepciones de dominio → 422/409, no encontrado → 404, otro punto de venta → 403.
- **Sesión** (RF-18, RF-20): cookie `HttpOnly`, `Secure` y `SameSite=Strict`, válida 8 horas. Guarda en claims el login, el nombre y el punto de venta (`SGP_USUARIO.vchCodPuntoVenta`), nunca la contraseña. Todos los controllers llevan `[Authorize]` salvo el de inicio de sesión. Sin sesión, una página redirige al login y una llamada AJAX recibe 401.
- **Antifalsificación**: todo formulario usa el token de Razor y toda llamada jQuery por POST envía la cabecera del token; validar con `[ValidateAntiForgeryToken]` o un filtro global.
- **Usuario y punto de venta** se obtienen siempre de `IUsuarioActual` (claims de la sesión), nunca de los datos de la petición (RN-19).
- Las columnas de auditoría (`vchCreador`, `dtmFechaCreacion`, `vchModificador`, `dtmFechaModificacion`) se rellenan en Infrastructure (interceptor de `SaveChanges`, con el usuario de `IUsuarioActual`), no en cada servicio.

### ¿Dónde va cada cosa?

| Necesito... | Capa / ubicación |
|---|---|
| Una regla o cálculo de negocio (total, IGV, "no vender cantidad 0") | Domain, como método de la entidad |
| Una nueva pantalla o acción | Servicio en Application + controller MVC y vista Razor (o acción JSON si la invoca jQuery) en `Ventas.Api` |
| Validar el formato de una petición | Validador FluentValidation en Application |
| Leer o grabar una tabla | Interfaz en Application, implementación + configuración EF en Infrastructure |
| Convertir un flag `'S'/'N'` o un tipo incoherente | `ValueConverter` en la configuración EF (Infrastructure) |
| Saber quién es el usuario o su punto de venta | `IUsuarioActual` (interfaz en Application, implementación en la capa web) |
| Cambiar el formato de error de las llamadas AJAX | Manejador global de excepciones en la capa web |
| Replicar cómo se graba un dato | Revisar el procedimiento de referencia en `StoreProcedure.sql` |

### Flujo de registrar una venta

```
POST /Ventas/Registrar (jQuery, JSON RegistrarVentaDto + token antifalsificación, cookie de sesión)
  → VentasController (MVC, [Authorize])
  → VentaService
      1. Validar DTO (FluentValidation)
      2. Usuario y punto de venta desde IUsuarioActual; tipo de orden V01
      3. Cargar cliente, artículos y tarifa vigente; verificar stock (IStockService)
      4. Construir la entidad Venta → Domain calcula importes (convención heredada, ver reglas)
      5. IUnitOfWork: abrir transacción
           SGP_CABECERA_TRANSACCION_BS (chrSituacionRegistro 'I') + SGP_DETALLE_TRANSACCION_BS
           correlativo SGP_IMPRESORA (serie = 1er carácter del tipo de comprobante + punto de venta)
             → SGP_COMPROBANTE_BS + detalle + SGP_MOVIMIENTO_TRANSACCION_BS
           PGA_TRANSACCION_KARDEX ('VENTA', cantidad negativa)
           cabecera → numCodComprobante y chrSituacionRegistro 'A'
           SGP_MEDIO_PAGO_BS (un solo medio, por el total) + SGP_MEDIO_PAGO_COMPROBANTE_BS
             → SGP_COMPROBANTE_BS.vchFlagMedioPago = 'S'
         commit (o rollback completo ante cualquier error)
  ← 201 con VentaRegistradaDto (JSON) → jQuery redirige a la vista del comprobante
```

### Tests

- `Ventas.UnitTests` referencia solo Domain + Application: probar cálculos de Domain sin mocks y servicios de Application con Moq sobre las interfaces.
- `Ventas.IntegrationTests` referencia el proyecto web y levanta el host con `WebApplicationFactory`, incluidas la autenticación por cookie y la antifalsificación.

Estado actual: `Ventas.Api` ya es MVC (`Comun/RegistroServiciosWeb.cs`: cookie, antifalsificación global, política de autorización por defecto y `ManejadorExcepciones` → ProblemDetails; jQuery 3.7.1 local en `wwwroot/lib` y `wwwroot/js/site.js`, que envía la cabecera `RequestVerificationToken`). Domain tiene los agregados `Venta` y `Comprobante`, `MedioPago` y los cálculos de importes. Inicio de sesión listo (`CuentaController`, `AutenticacionService`, `IUsuarioActual` en `Ventas.Api/Seguridad`); la cookie se revalida contra `SGP_USUARIO` en cada petición. La contraseña es la propiedad de sombra `Clave` de `UsuarioConfiguration` y solo la lee `VerificadorClave`. Las columnas de auditoría son propiedades de sombra (`MapearAuditoriaSgp()` / `MapearAuditoriaPga()`) que rellena `AuditoriaInterceptor`. Los tests de integración usan `AplicacionPrueba` (usuarios en memoria, sin BD).

## Esquema de base de datos

`Docs/database/BDVentas.sql` es el esquema de referencia de la base de datos existente (32 tablas, solo estructura, sin datos). **Hay que adaptarse al esquema tal cual**: no renombrar ni alterar tablas o columnas; las entidades de dominio pueden tener nombres limpios en C#, pero la configuración de EF (Fluent API en Infrastructure) mapea cada propiedad a su tabla/columna real.

- Prefijos: `SGP_` tablas de negocio, `PGA_` tablas técnicas/integración (`PGA_TRANSACCION_KARDEX`, `PGA_PARAMETRO`, `PGA_LOG`, `PGA_ORDEN_PAGO_DIGITAL`). Las `PGA_` no usan notación húngara (`CodArticulo`, `SituacionRegistro`...).
- Columnas en notación húngara (`vch` varchar, `chr` char, `dtm` datetime, `num`/`int` numéricos) y auditoría común: `vchCreador`, `dtmFechaCreacion`, `vchModificador`, `dtmFechaModificacion`, `chrSituacionRegistro`.
- `chrSituacionRegistro` es borrado lógico (`'A'` = activo); no hacer `DELETE` físico.
- Solo hay 8 FKs declaradas; el resto de relaciones (p. ej. cabecera→cliente, comprobante→cliente, medio de pago→transacción) son implícitas y se configuran solo en el modelo de EF.
- Tipos incoherentes entre columna y la clave referenciada (convertir en el mapeo, no en la BD): `SGP_CABECERA_TRANSACCION_BS.numCodCliente` es `int` vs `numeric(18,0)` en `SGP_CLIENTE`; `numCodComprobante` es `numeric(9,4)` vs `numeric(18,0)` (se desborda con códigos mayores a 99999, P-12); `vchCodAlmacen`, `vchTipoOrden`, `vchCodTarifaCanal` con longitudes distintas; `SGP_TARIFA` usa `nvarchar` y `SGP_ARTICULO` `varchar`.
- `SGP_TIPO_LIQUIDACION` no tiene PK.
- Flags mezclan `'S'/'N'` y `'0'/'1'`; mapear cada uno explícitamente.

### Procedimientos almacenados de referencia

`Docs/database/StoreProcedure.sql` contiene los procedimientos del sistema actual. **Son la referencia de cómo se graban los datos**: el sistema nuevo debe producir los mismos valores (para ser compatible con la información existente), aunque no esté obligado a invocarlos. Los principales:

| Procedimiento | Qué define |
|---|---|
| `pga_cabecera_transaccion_hemeroteca_agregar` | Alta de la cabecera de la venta: `chrSituacionRegistro = 'I'`, tipo de liquidación desde `SGP_TIPO_ORDEN`, normalización de `vchCodTarifaCanal` (si tiene 2 caracteres se antepone un espacio). |
| `pga_hemeroteca_emitir_comprobante` | Comprobante, serie y correlativo, detalle, `SGP_MOVIMIENTO_TRANSACCION_BS`, kardex y paso de la cabecera a `'A'`. |
| `pga_hemeroteca_comprobante_cancelar` | Registro del medio de pago («cancelar» = pagar): llama a la emisión y graba un medio de pago por el total. |
| `pga_mediopago_tipo_tarjeta_listar` | Tipos de tarjeta (catálogo `numCodigo = 114`). No se usa: débito y crédito son medios de pago distintos (P-09). |
| `pga_cliente_obtener_por_clave`, `pga_cliente_listar` | Lectura de clientes. |

Los procedimientos no abren una transacción; el sistema nuevo sí (RN-15). `pga_obtener_impuesto_iva` lee `SGP_DETALLE_IVA`, que no existe en `BDVentas.sql`: no usarlo.

## Alcance y reglas de negocio

El detalle está en `Docs/requerimientos-funcionales.md`; esta sección resume lo necesario para implementar.

Alcance: **inicio de sesión → búsqueda o registro del cliente → venta de artículos → generación del comprobante → registro del medio de pago**, más consulta de ventas y comprobantes y reporte de ventas. Tablas implicadas:

| Paso | Tablas |
|---|---|
| Sesión | `SGP_USUARIO` |
| Cliente | `SGP_CLIENTE` |
| Venta | `SGP_CABECERA_TRANSACCION_BS` (1) → `SGP_DETALLE_TRANSACCION_BS` (N) |
| Comprobante | `SGP_COMPROBANTE_BS` (1) → `SGP_DETALLE_COMPROBANTE_BS` (N); `SGP_MOVIMIENTO_TRANSACCION_BS` enlaza ítems de comprobante con líneas de transacción |
| Pago | `SGP_MEDIO_PAGO_BS` (un registro por venta); `SGP_MEDIO_PAGO_COMPROBANTE_BS` lo asigna al comprobante |
| Stock | `PGA_TRANSACCION_KARDEX` |
| Maestros | `SGP_ARTICULO`, `SGP_TARIFA` / `SGP_TARIFA_CANAL`, `SGP_PUNTO_VENTA`, `SGP_IMPRESORA` (serie y correlativo), `SGP_BANCO`, `SGP_TIPO_ORDEN` |

Reglas confirmadas:

- **Punto de venta**: el de `SGP_USUARIO.vchCodPuntoVenta` del usuario de la sesión; nunca se ingresa. El operador solo vende y consulta en ese punto de venta (403 si no).
- **IGV = 0.18**. `SGP_IVA` no tiene columna de tasa.
- **Importes** (convención de los procedimientos de referencia; el subtotal de la línea **incluye** el IGV), 4 decimales (`numeric(15,4)`). La tarifa **ya incluye el IGV** (P-15), así que se desglosa (difiere del procedimiento, que lo suma):
  - Línea: `numSubTotal = numMontoBruto = Cantidad × Tarifa`; `ValorVenta = round(numSubTotal / 1.18, 4)`; `numIGV = numSubTotal − ValorVenta`.
  - Cabecera de venta y comprobante: `numMontoBruto = Σ numSubTotal`; `numSubTotal = Σ (numSubTotal − numIGV)`; `numMontoIGV = Σ numIGV`; `numMontoTotal = numSubTotal + numMontoIGV`; descuento y recargo = 0.
  - Detalle del comprobante: `numPrecioUnitario = numTarifa`; `numMontoBruto = numSubTotal + recargo − descuento`.
  - Se graban con 4 decimales y se muestran con 2 en el comprobante y el reporte.
- **Moneda**: siempre soles (PEN), tipo de cambio 1.
- **Catálogos** (`SGP_TABLA` / `SGP_DETALLE_TABLA`, códigos de forma de pago, tipo de comprobante, medio de pago, tipo de tarjeta, estados...): usar solo registros con `chrSituacionRegistro = 'A'`.
- **Stock por artículo + punto de venta** (sin canal): se calcula sumando `PGA_TRANSACCION_KARDEX.Cantidad` por `CodArticulo` + `CodPuntoVenta`, comparando sin espacios; no se filtra por `CodTarifaCanal` porque el procedimiento de referencia lo graba en NULL. Los movimientos nuevos sí graban `CodTarifaCanal = '01'`. No hay tabla de saldos. No se permite vender más que el stock. `vchCodAlmacen` de la cabecera no determina el stock. El kardex está vacío en la BD actual: hace falta cargar stock para poder vender.
- **Kardex**: un movimiento por línea con `TipoTransaccion = 'VENTA'`, `Cantidad` negativa, `FechaTransaccion` sin hora, `CodTransaccion = numCodTransaccion`, `Descripcion = 'Salida por venta ' + serie + '-' + número`, `Estado` y `Proceso` vacíos.
- **Precio**: `SGP_TARIFA` por artículo + canal, vigente cuando la fecha de venta está entre `dtmFecInicio` y `dtmFecCaducidad`.
- **Canal**: fijo `01` (no lo elige el operador); en la cabecera se graba normalizado como `' 01'`.
- **Almacén** (`vchCodAlmacen`): primer almacén activo (menor código) del punto de venta en `SGP_PUNTO_VENTA_ALMACEN`; sin almacén, se rechaza la venta.
- **Tipo de orden**: `V01`; `vchTipLiquidacion` = `SGP_TIPO_ORDEN.intCodTipoLiquidacion`. Cabecera con `intEstado = 1`.
- **Numeración del comprobante**: serie = `F` (factura) o `B` (boleta) + código del punto de venta sin espacios (`' 001'` → `B001`, cabe en `varchar(4)`); la impresora se busca solo por `vchNumSerie` porque su `vchTipComprobante` no es fiable, buscada en `SGP_IMPRESORA.vchNumSerie`. `vchCorrelativo` guarda el **próximo** número: el comprobante lo toma y la impresora queda con el valor + 1. `vchNumComprobante` con 7 dígitos y ceros a la izquierda. Incremento atómico dentro de la transacción.
- **Estados**: cabecera `chrSituacionRegistro` `'I'` → `'A'` al emitir; comprobante `vchEstado = '2'` (cancelado), `chrFlagFacturado = 'S'`, `vchFlagMedioPago = 'S'`; medio de pago `vchEstado = '2'`.
- **Pago**: al contado, **un solo medio de pago por el total** (el operador no ingresa el monto). `vchMedioPago`: `1` efectivo, `2` cheque, `3` tarjeta de débito, `4` tarjeta de crédito, `5` boleta de depósito; `vchTipoTarjeta` en NULL. Cheque, tarjeta y depósito exigen banco y número de operación.
- **Contraseña**: `SGP_USUARIO.vchPassword` está en texto plano; comparar en tiempo constante y nunca registrarla en logs.
- **Cliente**: se busca por número de documento; si no existe se puede registrar (DNI 8 dígitos, RUC 11). No puede haber dos clientes activos con el mismo tipo y número de documento. Factura exige RUC.
- Venta, comprobante, kardex y medio de pago se graban en una única transacción de BD.

Pendiente de confirmar (preguntar antes de implementar; detalle en la sección 7 de los requerimientos): selección de tarifa por escala de `numCantidad` (P-06), si se redondea cada línea o solo los totales al mostrar 2 decimales (P-11), pago mixto (P-21) y valores heredados de tipo de cambio y `numMontoPagado` (P-22).

## Configuración

La cadena de conexión `ConnectionStrings:DefaultConnection` (SQL Server LocalDB, base de datos `BDVenta`) se guarda en **User Secrets** de `Ventas.Api`, no en `appsettings.json` (que mantiene un valor vacío). Para configurarla en un equipo nuevo:

```bash
cd src/Ventas.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<cadena de conexión>"
```

Las claves de protección de datos que firman la cookie de sesión tampoco van en el repositorio.

## Convenciones

- Responder y documentar en español.
- Mensajes de commit en inglés usando Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`).
- Remoto: https://github.com/JuanElio/GalaxyVentas.git, rama `main`.

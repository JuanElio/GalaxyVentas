
# Proyecto Ventas
- API REST en ASP.NET Core .NET 10, Clean Architecture.
- Esquema de BD: Docs/database/BDVentas.sql (SQL Server). No inventar tablas ni columnas.
- Domain no depende de nada. Application solo depende de Domain.
- Usar DTOs; nunca exponer entidades de EF en los controllers.
- Validaciones con FluentValidation; errores con ProblemDetails (RFC 7807).
- Métodos async con CancellationToken.
- Cada servicio nuevo debe tener pruebas unitarias en tests/Ventas.UnitTests.
- Comentarios y mensajes de error en español.



## Stack

API Web ASP.NET Core sobre .NET 10 (`net10.0`), EF Core 10 con SQL Server, FluentValidation, OpenAPI + Scalar. Los tests usan xUnit, Moq y FluentAssertions; los tests de integración usan `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`).

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

Clean Architecture con cuatro capas en `src/`. Dirección de dependencias (impuesta por las referencias entre proyectos):

```
Api ──> Application ──> Domain
 └────> Infrastructure ──> Application
```

- **Domain**: entidades y reglas de negocio; sin dependencias.
- **Application**: casos de uso, interfaces (p. ej. repositorios) y validadores de FluentValidation. Define las abstracciones que implementa Infrastructure.
- **Infrastructure**: EF Core (`Microsoft.EntityFrameworkCore.SqlServer`), DbContext e implementaciones de repositorios.
- **Api**: controladores y raíz de composición de DI (`Program.cs`). Contiene `Microsoft.EntityFrameworkCore.Design` para las herramientas `dotnet ef` (solo scaffolding/consultas, no migraciones).

Los tests unitarios solo referencian Domain + Application (Infrastructure se simula con Moq). Los tests de integración referencian Api y levantan el host completo.

El proyecto sigue siendo la plantilla inicial (marcadores `Class1.cs`, `WeatherForecast`, `UnitTest1.cs`); ninguna capa tiene código real todavía, y `Program.cs` aún no registra los servicios de Application/Infrastructure, el DbContext ni `app.MapScalarApiReference()`.

## Esquema de base de datos

`Docs/database/BDVentas.sql` es el esquema de referencia de la base de datos existente (32 tablas, solo estructura, sin datos). **Hay que adaptarse al esquema tal cual**: no renombrar ni alterar tablas o columnas; las entidades de dominio pueden tener nombres limpios en C#, pero la configuración de EF (Fluent API en Infrastructure) mapea cada propiedad a su tabla/columna real.

- Prefijos: `SGP_` tablas de negocio, `PGA_` tablas técnicas/integración (`PGA_TRANSACCION_KARDEX`, `PGA_PARAMETRO`, `PGA_LOG`, `PGA_ORDEN_PAGO_DIGITAL`). Las `PGA_` no usan notación húngara (`CodArticulo`, `SituacionRegistro`...).
- Columnas en notación húngara (`vch` varchar, `chr` char, `dtm` datetime, `num`/`int` numéricos) y auditoría común: `vchCreador`, `dtmFechaCreacion`, `vchModificador`, `dtmFechaModificacion`, `chrSituacionRegistro`.
- `chrSituacionRegistro` es borrado lógico (`'A'` = activo); no hacer `DELETE` físico.
- Solo hay 8 FKs declaradas; el resto de relaciones (p. ej. cabecera→cliente, comprobante→cliente, medio de pago→transacción) son implícitas y se configuran solo en el modelo de EF.
- Tipos incoherentes entre columna y la clave referenciada (convertir en el mapeo, no en la BD): `SGP_CABECERA_TRANSACCION_BS.numCodCliente` es `int` vs `numeric(18,0)` en `SGP_CLIENTE`; `numCodComprobante` es `numeric(9,4)` vs `numeric(18,0)`; `vchCodAlmacen`, `vchTipoOrden`, `vchCodTarifaCanal` con longitudes distintas; `SGP_TARIFA` usa `nvarchar` y `SGP_ARTICULO` `varchar`.
- `SGP_TIPO_LIQUIDACION` no tiene PK.
- Flags mezclan `'S'/'N'` y `'0'/'1'`; mapear cada uno explícitamente.

## Alcance y reglas de negocio

Alcance: **venta de artículos → generación del comprobante → registro del medio de pago**. Tablas implicadas:

| Paso | Tablas |
|---|---|
| Venta | `SGP_CABECERA_TRANSACCION_BS` (1) → `SGP_DETALLE_TRANSACCION_BS` (N) |
| Comprobante | `SGP_COMPROBANTE_BS` (1) → `SGP_DETALLE_COMPROBANTE_BS` (N); `SGP_MOVIMIENTO_TRANSACCION_BS` enlaza ítems de comprobante con líneas de transacción |
| Pago | `SGP_MEDIO_PAGO_BS` (por transacción); `SGP_MEDIO_PAGO_COMPROBANTE_BS` asigna montos de cada pago a cada comprobante |
| Stock | `PGA_TRANSACCION_KARDEX` |
| Maestros | `SGP_CLIENTE`, `SGP_ARTICULO`, `SGP_TARIFA` / `SGP_TARIFA_CANAL`, `SGP_PUNTO_VENTA`, `SGP_IMPRESORA` (serie y correlativo), `SGP_BANCO`, `SGP_USUARIO` |

Reglas confirmadas:

- **IGV = 0.18** sobre el subtotal. `SGP_IVA` no tiene columna de tasa.
- Totales (cabecera, detalle y comprobante): `SubTotal = MontoBruto − MontoDescuento + MontoRecargo`, `MontoIGV = SubTotal × 0.18`, `MontoTotal = SubTotal + MontoIGV`; la cabecera es la suma de sus líneas. Importes con 4 decimales (`numeric(15,4)`).
- **Catálogos** (`SGP_TABLA` / `SGP_DETALLE_TABLA`, códigos de forma de pago, tipo de comprobante, medio de pago, moneda, estados...): usar solo registros con `chrSituacionRegistro = 'A'`.
- **Stock por punto de venta + canal**: se calcula sumando `PGA_TRANSACCION_KARDEX.Cantidad` por `CodArticulo` + `CodPuntoVenta` + `CodTarifaCanal`; no hay tabla de saldos. Cada venta registra su movimiento enlazado por `CodTransaccion` (= `numCodTransaccion`). `vchCodAlmacen` de la cabecera no determina el stock.
- Precio: `SGP_TARIFA` por artículo + canal, vigente cuando la fecha de venta está entre `dtmFecInicio` y `dtmFecCaducidad`.
- Numeración del comprobante: `vchNumSerie` + `vchNumComprobante`, tomando el correlativo de `SGP_IMPRESORA` (incremento atómico, dentro de la misma transacción de BD).
- Venta, comprobante, kardex y medios de pago se graban en una única transacción de BD.

Pendiente de confirmar (preguntar antes de implementar): selección de tarifa por escala de `numCantidad`, códigos de `intEstado`/`vchEstado` y de `chrTipDscto0X`, signo de `Cantidad` en el kardex según `TipoTransaccion`, y regla de redondeo (`numMontoRedondeo`).

## Configuración

La cadena de conexión `ConnectionStrings:DefaultConnection` (SQL Server LocalDB, base de datos `BDVenta`) se guarda en **User Secrets** de `Ventas.Api`, no en `appsettings.json` (que mantiene un valor vacío). Para configurarla en un equipo nuevo:

```bash
cd src/Ventas.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<cadena de conexión>"
```

## Convenciones

- Responder y documentar en español.
- Mensajes de commit en inglés usando Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`).
- Remoto: https://github.com/JuanElio/GalaxyVentas.git, rama `main`.

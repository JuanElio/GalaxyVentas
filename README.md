# GalaxyVentas

Sistema web de ventas para punto de venta: el operador inicia sesión, busca o registra al cliente, agrega los artículos disponibles, emite la boleta o factura y registra el medio de pago, todo en una sola operación. También permite consultar ventas y comprobantes, obtener el reporte de ventas y cambiar la contraseña.

Trabaja sobre la base de datos existente `BDVenta` (SQL Server) **sin modificar su esquema** y graba los datos con las mismas convenciones que los procedimientos almacenados del sistema actual.

## Funcionalidades

| Módulo | Qué hace |
|---|---|
| Inicio de sesión | Usuario y contraseña de `SGP_USUARIO`; sesión por cookie segura de 8 horas, limitada al punto de venta del usuario. |
| Registrar venta | Búsqueda y alta de clientes, artículos agrupados por categoría con precio, IGV y stock, cálculo de importes en el servidor, comprobante con serie y correlativo, kardex y pago en una única transacción. |
| Consultas | Venta por número de transacción o comprobante por tipo y número; últimas 20 ventas; impresión del comprobante. |
| Reporte de ventas | Ventas del punto de venta por rango de fechas (máximo 31 días), con filtros y totales por medio de pago. |
| Mis datos | Datos del usuario de la sesión y cambio de contraseña. |

## Tecnología

- ASP.NET Core MVC sobre **.NET 10** (C#), vistas Razor y jQuery 3.7.1.
- EF Core 10 con SQL Server, FluentValidation, OpenAPI + Scalar (solo las acciones JSON).
- Pruebas: xUnit, Moq, FluentAssertions y `Microsoft.AspNetCore.Mvc.Testing`.

## Estructura

```
Ventas/
├── src/
│   ├── Ventas.Domain          Entidades y reglas de negocio (sin dependencias)
│   ├── Ventas.Application     Casos de uso, DTOs, validadores e interfaces
│   ├── Ventas.Infrastructure  EF Core, mapeo al esquema heredado, repositorios
│   └── Ventas.Api             Aplicación web MVC (controllers, vistas, jQuery)
├── tests/
│   ├── Ventas.UnitTests
│   └── Ventas.IntegrationTests
└── Docs/                      Requerimientos, casos de uso, manuales, BD y prototipos
```

Detalle en el [documento de arquitectura](Docs/arquitectura.md).

## Puesta en marcha

### Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server o LocalDB con la base de datos `BDVenta` restaurada (esquema en [Docs/database/BDVentas.sql](Docs/database/BDVentas.sql))
- Un usuario activo y vigente en `SGP_USUARIO` con punto de venta asignado

### 1. Cadena de conexión

La cadena de conexión no se guarda en el repositorio: va en *User Secrets* del proyecto web.

```bash
cd src/Ventas.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=BDVenta;Trusted_Connection=True;TrustServerCertificate=True"
```

### 2. Datos para poder vender

La venta usa el tipo de orden `V41` y el canal `30`. Con los datos originales hace falta asignar categorías y cargar stock. Ambos scripts muestran una vista previa y no graban nada hasta ejecutarlos con `@Confirmar = 1`:

1. [Docs/database/AsignarCategoriasV41.sql](Docs/database/AsignarCategoriasV41.sql): asigna artículos con tarifa en el canal 30 a sus categorías para `V41`.
2. [Docs/database/CargaStockInicial.sql](Docs/database/CargaStockInicial.sql): carga stock de prueba de esos artículos en cada punto de venta.

### 3. Compilar, probar y ejecutar

Desde la raíz del repositorio:

```bash
dotnet build
dotnet test
dotnet run --project src/Ventas.Api --launch-profile https
```

La aplicación queda en https://localhost:7205 (y http://localhost:5070). En desarrollo, la documentación de las acciones JSON está en `/scalar`.

> No se usan migraciones de EF Core: la base de datos ya existe y su esquema no se modifica.

## Pruebas

```bash
dotnet test tests/Ventas.UnitTests
dotnet test tests/Ventas.IntegrationTests
dotnet test --filter "Categoria=BaseDatos"     # solo las que usan la BD real
```

Las pruebas con `Categoria=BaseDatos` usan la BD de la cadena de conexión y deshacen sus cambios; si no hay cadena de conexión, terminan sin hacer nada. Estrategia y cobertura en el [plan de pruebas](Docs/plan-de-pruebas.md).

## Documentación

| Documento | Contenido |
|---|---|
| [Requerimientos funcionales](Docs/requerimientos-funcionales.md) | RF, reglas de negocio, requerimientos no funcionales y supuestos (fuente de verdad) |
| [Casos de uso](Docs/casos-de-uso.md) | Actores, diagrama y especificación de los 12 casos de uso |
| [Manual de usuario](Docs/manual-usuario.md) | Uso de cada pantalla por el operador |
| [Arquitectura](Docs/arquitectura.md) | Capas, componentes, secuencia de la venta, modelo de clases y decisiones |
| [Plan de pruebas](Docs/plan-de-pruebas.md) | Estrategia, casos de prueba y resultados |
| [Prototipos](Docs/prototipos/index.html) | Pantallas navegables en HTML |

## Convenciones

- Código, comentarios, mensajes y documentación en español.
- Commits en inglés con *Conventional Commits* (`feat:`, `fix:`, `chore:`, `docs:`).

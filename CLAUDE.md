# CLAUDE.md

Este archivo proporciona orientación a Claude Code (claude.ai/code) para trabajar con el código de este repositorio.

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

# Migraciones de EF Core (el DbContext vive en Infrastructure, el proyecto de inicio es Api)
dotnet ef migrations add <Nombre> --project src/Ventas.Infrastructure --startup-project src/Ventas.Api
dotnet ef database update --project src/Ventas.Infrastructure --startup-project src/Ventas.Api
```

## Arquitectura

Clean Architecture con cuatro capas en `src/`. Dirección de dependencias (impuesta por las referencias entre proyectos):

```
Api ──> Application ──> Domain
 └────> Infrastructure ──> Application
```

- **Domain**: entidades y reglas de negocio; sin dependencias.
- **Application**: casos de uso, interfaces (p. ej. repositorios) y validadores de FluentValidation. Define las abstracciones que implementa Infrastructure.
- **Infrastructure**: EF Core (`Microsoft.EntityFrameworkCore.SqlServer`), DbContext e implementaciones de repositorios.
- **Api**: controladores y raíz de composición de DI (`Program.cs`). Contiene `Microsoft.EntityFrameworkCore.Design` para las herramientas `dotnet ef`.

Los tests unitarios solo referencian Domain + Application (Infrastructure se simula con Moq). Los tests de integración referencian Api y levantan el host completo.

El proyecto sigue siendo la plantilla inicial (marcadores `Class1.cs`, `WeatherForecast`, `UnitTest1.cs`); ninguna capa tiene código real todavía, y `Program.cs` aún no registra los servicios de Application/Infrastructure, el DbContext ni `app.MapScalarApiReference()`.

## Esquema de base de datos

`Docs/database/BDVentas.sql` es el esquema de referencia de la base de datos existente (32 tablas, solo estructura, sin datos). Las tablas usan el prefijo `SGP_` y nombres de columna en notación húngara (`vch` varchar, `chr` char, `dtm` datetime, `int`/`dec`...), con columnas de auditoría `vchCreador`, `dtmFechaCreacion`, `vchModificador`, `dtmFechaModificacion` y un indicador de estado `chrSituacionRegistro`. Las entidades de EF Core deben mapearse a estos nombres existentes.

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

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Stack

ASP.NET Core Web API on .NET 10 (`net10.0`), EF Core 10 with SQL Server, FluentValidation, OpenAPI + Scalar. Tests use xUnit, Moq and FluentAssertions; integration tests use `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`).

## Commands

Run from the repo root (`Ventas.slnx` includes all six projects; register new ones with `dotnet sln Ventas.slnx add <path>.csproj`).

```bash
dotnet build
dotnet test                                         # all test projects
dotnet run --project src/Ventas.Api                 # http://localhost:5070 / https://localhost:7205
dotnet test tests/Ventas.UnitTests
dotnet test tests/Ventas.IntegrationTests
dotnet test tests/Ventas.UnitTests --filter "FullyQualifiedName~ClassName.MethodName"   # single test

# EF Core migrations (DbContext lives in Infrastructure, startup project is Api)
dotnet ef migrations add <Name> --project src/Ventas.Infrastructure --startup-project src/Ventas.Api
dotnet ef database update --project src/Ventas.Infrastructure --startup-project src/Ventas.Api
```

## Architecture

Clean Architecture with four layers under `src/`. Dependency direction (enforced by project references):

```
Api ──> Application ──> Domain
 └────> Infrastructure ──> Application
```

- **Domain**: entities and business rules; no dependencies.
- **Application**: use cases, interfaces (e.g. repositories) and FluentValidation validators. Defines abstractions that Infrastructure implements.
- **Infrastructure**: EF Core (`Microsoft.EntityFrameworkCore.SqlServer`), DbContext and repository implementations.
- **Api**: controllers, DI composition root (`Program.cs`). Holds `Microsoft.EntityFrameworkCore.Design` for `dotnet ef` tooling.

Unit tests reference only Domain + Application (mock Infrastructure with Moq). Integration tests reference Api and spin up the full host.

The project is still a template scaffold (`Class1.cs`, `WeatherForecast`, `UnitTest1.cs` placeholders); none of the layers have real code yet, and `Program.cs` does not yet register Application/Infrastructure services, DbContext or `app.MapScalarApiReference()`.

## Database schema

`Docs/database/BDVentas.sql` is the reference schema of the existing database (32 tables, schema only, no data). Tables use the `SGP_` prefix and Hungarian-style column names (`vch` varchar, `chr` char, `dtm` datetime, `int`/`dec`...), with audit columns `vchCreador`, `dtmFechaCreacion`, `vchModificador`, `dtmFechaModificacion` and a status flag `chrSituacionRegistro`. EF Core entities must map to these existing names.

## Configuration

The connection string `ConnectionStrings:DefaultConnection` (SQL Server LocalDB, database `BDVenta`) is stored in **User Secrets** for `Ventas.Api`, not in `appsettings.json` (which keeps an empty placeholder). Set it on a new machine with:

```bash
cd src/Ventas.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection string>"
```

## Conventions

- Commit messages in English using Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`).
- Remote: https://github.com/JuanElio/GalaxyVentas.git, branch `main`.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ventas.Domain.Seguridad;
using Ventas.Infrastructure.Persistence;

namespace Ventas.IntegrationTests.Persistence;

/// <summary>
/// Verifica que el modelo de EF apunte a las tablas y columnas reales (no requiere conexión).
/// </summary>
public class MapeoTests(AplicacionPrueba app) : IClassFixture<AplicacionPrueba>
{
    private string ScriptDelModelo()
    {
        using var scope = app.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<VentasDbContext>().Database.GenerateCreateScript();
    }

    [Fact]
    public void Usuario_SeMapeaASgpUsuarioConSusColumnasReales()
    {
        using var scope = app.Services.CreateScope();
        var tipo = scope.ServiceProvider.GetRequiredService<VentasDbContext>().Model.FindEntityType(typeof(Usuario))!;

        Assert.Equal("SGP_USUARIO", tipo.GetTableName());
        var columnas = tipo.GetProperties().Select(p => p.GetColumnName()).ToHashSet();
        Assert.Superset(new HashSet<string>
        {
            "numCodigoUsuario", "vchLogin", "vchPassword", "vchNombre", "vchCodPuntoVenta", "chrSituacionRegistro",
            "dtmFechaInicioAct", "dtmFechaFinAct", "dtmFechaExpiraClave",
            "vchCreador", "dtmFechaCreacion", "vchModificador", "dtmFechaModificacion",
        }, columnas);
    }

    [Fact]
    public void Modelo_SeConstruyeSinErrores() =>
        Assert.Contains("CREATE TABLE [SGP_USUARIO]", ScriptDelModelo());
}

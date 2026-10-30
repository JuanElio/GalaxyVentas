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

    [Theory]
    [InlineData("SGP_USUARIO")]
    [InlineData("SGP_CLIENTE")]
    [InlineData("SGP_ARTICULO")]
    [InlineData("SGP_TARIFA")]
    [InlineData("SGP_TARIFA_CANAL")]
    [InlineData("SGP_PUNTO_VENTA")]
    [InlineData("SGP_BANCO")]
    [InlineData("SGP_TABLA")]
    [InlineData("SGP_DETALLE_TABLA")]
    [InlineData("PGA_TRANSACCION_KARDEX")]
    public void Modelo_MapeaLasTablasReales(string tabla) =>
        Assert.Contains($"CREATE TABLE [{tabla}]", ScriptDelModelo());

    [Fact]
    public void Kardex_UsaLasColumnasSinNotacionHungara()
    {
        var script = ScriptDelModelo();
        var kardex = script[script.IndexOf("CREATE TABLE [PGA_TRANSACCION_KARDEX]", StringComparison.Ordinal)..];
        kardex = kardex[..kardex.IndexOf(");", StringComparison.Ordinal)];

        foreach (var columna in new[] { "[CodArticulo]", "[CodTarifaCanal]", "[CodPuntoVenta]", "[Cantidad]",
                     "[SituacionRegistro]", "[Creador]", "[FechaCreacion]" })
            Assert.Contains(columna, kardex);
    }
}

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;
using Ventas.Infrastructure.Persistence;

namespace Ventas.IntegrationTests.Persistence;

/// <summary>
/// Consultas de solo lectura contra la BD real (BDVenta) para verificar que EF traduce bien
/// los mapeos y las comparaciones sin espacios. Si no hay cadena de conexión configurada en
/// User Secrets, las pruebas terminan sin hacer nada.
/// </summary>
[Trait("Categoria", "BaseDatos")]
public class ConsultasBaseDatosTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private async Task EnBaseDatosAsync(Func<IServiceProvider, Task> prueba)
    {
        var cadena = factory.Services.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(cadena))
            return;

        using var scope = factory.Services.CreateScope();
        await prueba(scope.ServiceProvider);
    }

    [Fact]
    public Task Articulos_DelCanal01ConTarifasVigentesYCodigosSinEspacios() => EnBaseDatosAsync(async servicios =>
    {
        var repositorio = servicios.GetRequiredService<IArticuloRepository>();

        var articulos = await repositorio.ListarConTarifasVigentesAsync("01", null, DateTime.Now, 100, CancellationToken.None);

        Assert.NotEmpty(articulos);
        Assert.All(articulos, a =>
        {
            Assert.Equal(a.Articulo.Codigo.Trim(), a.Articulo.Codigo);
            Assert.NotEmpty(a.Tarifas);
        });
        Assert.True(await repositorio.ExisteCanalActivoAsync("01", CancellationToken.None));
    });

    [Fact]
    public Task Articulos_FiltraPorTexto() => EnBaseDatosAsync(async servicios =>
    {
        var repositorio = servicios.GetRequiredService<IArticuloRepository>();
        var todos = await repositorio.ListarConTarifasVigentesAsync("01", null, DateTime.Now, 100, CancellationToken.None);
        var palabra = todos.First(a => !string.IsNullOrWhiteSpace(a.Articulo.Descripcion)).Articulo.Descripcion!.Trim().Split(' ')[0];

        var filtrados = await repositorio.ListarConTarifasVigentesAsync("01", palabra, DateTime.Now, 100, CancellationToken.None);

        Assert.NotEmpty(filtrados);
        Assert.All(filtrados, a => Assert.True(
            a.Articulo.Codigo.Contains(palabra, StringComparison.OrdinalIgnoreCase)
            || a.Articulo.Descripcion!.Contains(palabra, StringComparison.OrdinalIgnoreCase)));
    });

    [Fact]
    public Task Stock_SeCalculaSinErrores() => EnBaseDatosAsync(async servicios =>
    {
        var stock = await servicios.GetRequiredService<IStockService>()
            .ObtenerStockAsync("001", ["2010110001", "7043210060"], CancellationToken.None);

        Assert.NotNull(stock);
    });

    [Fact]
    public Task PuntoVenta_SeEncuentraConElCodigoSinEspacios() => EnBaseDatosAsync(async servicios =>
    {
        var puntoVenta = await servicios.GetRequiredService<IPuntoVentaRepository>().ObtenerAsync("001", CancellationToken.None);

        Assert.Equal(" 001", puntoVenta?.Codigo);
    });

    [Fact]
    public Task Catalogos_MediosDePagoYTiposDeComprobanteActivos() => EnBaseDatosAsync(async servicios =>
    {
        var repositorio = servicios.GetRequiredService<ICatalogoRepository>();

        var medios = await repositorio.ListarItemsActivosAsync(Catalogos.MedioPago, CancellationToken.None);
        var tipos = await repositorio.ListarItemsActivosAsync(Catalogos.TipoComprobante, CancellationToken.None);

        Assert.Superset(new HashSet<string> { "1", "2", "3", "4", "5" }, medios.Select(m => m.Valor!.Trim()).ToHashSet());
        Assert.Equal(["F", "B"], tipos.Select(t => t.Valor!.Trim()));
        Assert.NotEmpty(await repositorio.ListarBancosActivosAsync(CancellationToken.None));
    });

    [Fact]
    public Task Clientes_BuscaPorDocumento() => EnBaseDatosAsync(async servicios =>
    {
        var documento = await servicios.GetRequiredService<VentasDbContext>().Set<Cliente>()
            .Where(c => EF.Property<bool?>(c, "_activo") == true && c.NumeroDocumento != null && c.NumeroDocumento != "")
            .Select(c => c.NumeroDocumento)
            .FirstAsync();

        var encontrados = await servicios.GetRequiredService<IClienteRepository>()
            .BuscarActivosPorDocumentoAsync(documento, CancellationToken.None);

        Assert.NotEmpty(encontrados);
        Assert.All(encontrados, c => Assert.True(c.Activo));
    });
}

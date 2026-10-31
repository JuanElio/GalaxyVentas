using FluentAssertions;
using Moq;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Domain.Maestros;

namespace Ventas.UnitTests.Application;

public class ArticuloServiceTests
{
    private static readonly DateTime Hoy = new(2026, 10, 29, 10, 0, 0);

    private readonly Mock<IArticuloRepository> _articulos = new();
    private readonly Mock<IPuntoVentaRepository> _puntosVenta = new();
    private readonly Mock<IStockService> _stock = new();
    private readonly Mock<IUsuarioActual> _usuario = new();
    private readonly ArticuloService _servicio;

    public ArticuloServiceTests()
    {
        _usuario.SetupGet(u => u.CodigoPuntoVenta).Returns("001");
        _puntosVenta.Setup(p => p.ObtenerAsync("001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PuntoVenta(" 001", "SEDE CENTRAL", "Z01", activo: true));
        _articulos.Setup(a => a.ObtenerCanalAsync("30", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TarifaCanal(" 30", "Hemeroteca", activo: true));
        _stock.Setup(s => s.ObtenerStockAsync(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["ART001"] = 12 });

        _servicio = new ArticuloService(_articulos.Object, _puntosVenta.Object, _stock.Object, _usuario.Object, new RelojFijo(Hoy));
    }

    private static readonly CategoriaVenta Bienes = new(1, "Bienes", activo: true);
    private static readonly CategoriaVenta Libros = new(3, " Libros ", activo: true);

    private static Tarifa TarifaDe(string articulo, decimal tope, decimal precio) =>
        new(articulo, " 30", tope, precio, new DateTime(2020, 1, 1), new DateTime(2050, 1, 1), activo: true);

    private void DadoArticulos(params ArticuloConTarifas[] articulos) =>
        _articulos.Setup(a => a.ListarConTarifasVigentesAsync("V41", "30", It.IsAny<int?>(), It.IsAny<string?>(), Hoy,
            ArticuloService.MaximoResultados, It.IsAny<CancellationToken>())).ReturnsAsync(articulos);

    [Fact]
    public async Task ListarDisponibles_CombinaPrecioStockCategoriaEIgvOrdenadosPorCategoria()
    {
        DadoArticulos(
            new ArticuloConTarifas(new Articulo("LIB001", "ATLAS", "UND", true), [TarifaDe("LIB001", 999999, 50.00m)], Libros, 0m),
            new ArticuloConTarifas(new Articulo("ART001", "CUADERNO A4 ", "UND", true),
                [TarifaDe("ART001", 15, 102.60m), TarifaDe("ART001", 6, 83.00m)], Bienes, 0.18m),
            new ArticuloConTarifas(new Articulo("ART002", "AGENDA", "UND", true), [TarifaDe("ART002", 999999, 36.60m)], Bienes, 0.18m));

        var disponibles = await _servicio.ListarDisponiblesAsync(null, null, CancellationToken.None);

        disponibles.Should().Equal(
            new ArticuloDisponibleDto("ART002", "AGENDA", "UND", 36.60m, 0, 1, "Bienes", 0.18m),
            new ArticuloDisponibleDto("ART001", "CUADERNO A4", "UND", 83.00m, 12, 1, "Bienes", 0.18m),
            new ArticuloDisponibleDto("LIB001", "ATLAS", "UND", 50.00m, 0, 3, "Libros", 0m));
        _stock.Verify(s => s.ObtenerStockAsync("001", It.Is<IReadOnlyCollection<string>>(c => c.Count == 3), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ListarDisponibles_PasaElTextoDeBusquedaSinEspacios()
    {
        DadoArticulos();

        await _servicio.ListarDisponiblesAsync("  cuaderno ", 3, CancellationToken.None);

        _articulos.Verify(a => a.ListarConTarifasVigentesAsync("V41", "30", 3, "cuaderno", Hoy, It.IsAny<int>(), It.IsAny<CancellationToken>()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task ListarDisponibles_SinCategoria_NoFiltra(int? categoria)
    {
        DadoArticulos();

        await _servicio.ListarDisponiblesAsync(null, categoria, CancellationToken.None);

        _articulos.Verify(a => a.ListarConTarifasVigentesAsync("V41", "30", null, null, Hoy, It.IsAny<int>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ListarCategorias_DelTipoDeOrdenYCanalDeLaVenta()
    {
        _articulos.Setup(a => a.ListarCategoriasAsync("V41", "30", It.IsAny<CancellationToken>()))
            .ReturnsAsync([Libros, Bienes, new CategoriaVenta(5, null, activo: true)]);

        var categorias = await _servicio.ListarCategoriasAsync(CancellationToken.None);

        categorias.Should().Equal(
            new CategoriaVentaDto(1, "Bienes"), new CategoriaVentaDto(3, "Libros"), new CategoriaVentaDto(5, "Categoría 5"));
    }

    [Fact]
    public async Task ListarDisponibles_PuntoDeVentaInactivo_NoEncontrado()
    {
        _puntosVenta.Setup(p => p.ObtenerAsync("001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PuntoVenta(" 001", "SEDE", null, activo: false));

        var accion = () => _servicio.ListarDisponiblesAsync(null, null, CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("*punto de venta*");
    }

    [Fact]
    public async Task ListarDisponibles_CanalInactivo_NoEncontrado()
    {
        _articulos.Setup(a => a.ObtenerCanalAsync("30", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TarifaCanal(" 30", "Hemeroteca", activo: false));

        var accion = () => _servicio.ListarDisponiblesAsync(null, null, CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("*canal*");
    }
}

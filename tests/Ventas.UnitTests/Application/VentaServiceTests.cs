using FluentAssertions;
using FluentValidation;
using Moq;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Application.Ventas;
using Ventas.Domain.Comun;
using Ventas.Domain.Maestros;

namespace Ventas.UnitTests.Application;

public class VentaServiceTests
{
    private static readonly DateTime Hoy = new(2026, 10, 29, 10, 0, 0);

    private readonly Mock<IArticuloRepository> _articulos = new();
    private readonly Mock<IPuntoVentaRepository> _puntosVenta = new();
    private readonly Mock<IStockService> _stock = new();
    private readonly Mock<ICorrelativoService> _correlativos = new();
    private readonly Mock<IUsuarioActual> _usuario = new();
    private readonly VentaService _servicio;

    public VentaServiceTests()
    {
        _usuario.SetupGet(u => u.CodigoPuntoVenta).Returns("001");
        _usuario.SetupGet(u => u.Login).Returns("jperez");
        _usuario.SetupGet(u => u.Nombre).Returns("Juan Pérez");

        // Escalas reales del artículo 7041110101; ART002 con escala única
        _articulos.Setup(a => a.ObtenerConTarifasVigentesAsync("V41", "30", It.IsAny<IReadOnlyCollection<string>>(), Hoy,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ArticuloConTarifas(new Articulo("7041110101", "AVISO CLASIFICADO", "CM", true),
                    [Tarifa("7041110101", 6, 83.0000m), Tarifa("7041110101", 15, 102.6000m)], Bienes, 0.18m),
                new ArticuloConTarifas(new Articulo("ART002", "ANEXO", "PAG", true), [Tarifa("ART002", 999999, 36.6000m)], Bienes, 0.18m),
                new ArticuloConTarifas(new Articulo("LIB001", "LIBRO EXONERADO", "UND", true), [Tarifa("LIB001", 999999, 50.0000m)], Libros, 0m),
            ]);
        _stock.Setup(s => s.ObtenerStockAsync("001", It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["7041110101"] = 100, ["ART002"] = 3, ["LIB001"] = 10 });

        _servicio = new VentaService(_articulos.Object, _puntosVenta.Object, _stock.Object, _correlativos.Object,
            _usuario.Object, new CotizarVentaValidator(), new RelojFijo(Hoy));
    }

    private static readonly CategoriaVenta Bienes = new(1, "Bienes", activo: true);
    private static readonly CategoriaVenta Libros = new(3, "Libros", activo: true);

    private static Tarifa Tarifa(string articulo, decimal tope, decimal precio) =>
        new(articulo, " 30", tope, precio, new DateTime(2021, 12, 15), new DateTime(2050, 1, 1), activo: true);

    private Task<CotizacionDto> Cotizar(params LineaVentaDto[] lineas) =>
        _servicio.CotizarAsync(new CotizarVentaDto(lineas), CancellationToken.None);

    [Fact]
    public async Task Cotizar_PrecioSegunLaEscalaDeLaCantidadYTotalesDelDominio()
    {
        var cotizacion = await Cotizar(new LineaVentaDto("7041110101", 10), new LineaVentaDto("ART002", 1));

        var aviso = cotizacion.Lineas[0];
        aviso.Tarifa.Should().Be(102.6000m); // 10 unidades caen en la escala de 15
        aviso.SubTotal.Should().Be(1026.0000m);
        aviso.Should().Match<LineaCotizadaDto>(l => l.ValorVenta + l.Igv == l.SubTotal);
        cotizacion.Lineas[1].Tarifa.Should().Be(36.6000m);

        var esperados = Importes.Totalizar(cotizacion.Lineas.Select(l => (l.SubTotal, l.Igv)));
        cotizacion.MontoTotal.Should().Be(esperados.MontoTotal).And.Be(1062.6000m);
        cotizacion.SubTotal.Should().Be(esperados.SubTotal);
        cotizacion.Valida.Should().BeTrue();
    }

    [Fact]
    public async Task Cotizar_ArticuloExonerado_NoDesglosaIgv()
    {
        var cotizacion = await Cotizar(new LineaVentaDto("LIB001", 2));

        cotizacion.Lineas.Single().Should().Match<LineaCotizadaDto>(l => l.SubTotal == 100.0000m && l.ValorVenta == 100.0000m && l.Igv == 0);
        cotizacion.MontoIgv.Should().Be(0);
        cotizacion.MontoTotal.Should().Be(100.0000m);
    }

    [Fact]
    public async Task Cotizar_StockInsuficiente_ObservaLaLineaPeroCalculaElImporte()
    {
        var cotizacion = await Cotizar(new LineaVentaDto("ART002", 5));

        var linea = cotizacion.Lineas.Single();
        linea.Observacion.Should().Be("Stock insuficiente: disponible 3, solicitado 5.");
        linea.SubTotal.Should().Be(183.0000m);
        cotizacion.Valida.Should().BeFalse();
    }

    [Fact]
    public async Task Cotizar_CantidadMayorQueLaEscalaMasAlta_SinPrecio()
    {
        var cotizacion = await Cotizar(new LineaVentaDto("7041110101", 20));

        cotizacion.Lineas.Single().Should().Match<LineaCotizadaDto>(l => l.Tarifa == null && l.SubTotal == 0
            && l.Observacion!.Contains("No hay tarifa vigente"));
        cotizacion.MontoTotal.Should().Be(0);
        cotizacion.Valida.Should().BeFalse();
    }

    [Fact]
    public async Task Cotizar_ArticuloSinTarifa_ObservadoYFueraDelTotal()
    {
        var cotizacion = await Cotizar(new LineaVentaDto("ART002", 1), new LineaVentaDto("NOEXISTE", 1));

        cotizacion.Lineas[1].Observacion.Should().Contain("NOEXISTE");
        cotizacion.MontoTotal.Should().Be(36.6000m);
        cotizacion.Valida.Should().BeFalse();
    }

    [Fact]
    public async Task Cotizar_SinLineas_TotalesEnCeroYNoValida()
    {
        var cotizacion = await Cotizar();

        cotizacion.Lineas.Should().BeEmpty();
        cotizacion.MontoTotal.Should().Be(0);
        cotizacion.Valida.Should().BeFalse();
    }

    [Fact]
    public async Task Cotizar_CantidadDecimal_ErrorDeValidacion()
    {
        var accion = () => Cotizar(new LineaVentaDto("ART002", 1.5m));

        await accion.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ObtenerContexto_DatosDelPuntoDeVentaCanalYAlmacen()
    {
        _puntosVenta.Setup(p => p.ObtenerAsync("001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PuntoVenta(" 001", "SEDE CENTRAL ", "Z01", activo: true));
        _puntosVenta.Setup(p => p.ObtenerAlmacenPrincipalAsync("001", It.IsAny<CancellationToken>())).ReturnsAsync("ALM01 ");
        _articulos.Setup(a => a.ObtenerCanalAsync("30", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TarifaCanal(" 30", "Hemeroteca   ", activo: true));

        var contexto = await _servicio.ObtenerContextoAsync(CancellationToken.None);

        contexto.Should().Be(new ContextoVentaDto("001", "SEDE CENTRAL", "30 · Hemeroteca", "Soles (SOL)", "V41",
            "ALM01", "Juan Pérez (jperez)"));
    }

    [Fact]
    public async Task ObtenerContexto_PuntoDeVentaInexistente_NoEncontrado()
    {
        var accion = () => _servicio.ObtenerContextoAsync(CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Fact]
    public async Task ConsultarNumeracion_SerieDelPuntoDeVentaConSieteDigitos()
    {
        _correlativos.Setup(c => c.ConsultarSiguienteAsync("B001", It.IsAny<CancellationToken>())).ReturnsAsync(243455);

        var numeracion = await _servicio.ConsultarNumeracionAsync("B", CancellationToken.None);

        numeracion.Should().Be(new NumeracionDto("B001", "0243455"));
    }

    [Fact]
    public async Task ConsultarNumeracion_SinImpresora_NumeroNulo() =>
        (await _servicio.ConsultarNumeracionAsync("F", CancellationToken.None)).Should().Be(new NumeracionDto("F001", null));

    [Fact]
    public async Task ConsultarNumeracion_TipoInvalido_ExcepcionDeDominio()
    {
        var accion = () => _servicio.ConsultarNumeracionAsync("X", CancellationToken.None);

        await accion.Should().ThrowAsync<DominioException>();
    }
}

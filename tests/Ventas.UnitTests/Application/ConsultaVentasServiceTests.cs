using FluentAssertions;
using Moq;
using Ventas.Application.Comprobantes;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Maestros;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;
using Ventas.UnitTests.Domain;

namespace Ventas.UnitTests.Application;

public class ConsultaVentasServiceTests
{
    private readonly Mock<IConsultaVentasRepository> _consultas = new();
    private readonly Mock<IPuntoVentaRepository> _puntosVenta = new();
    private readonly Mock<ICatalogoRepository> _catalogos = new();
    private readonly Mock<IUsuarioActual> _usuario = new();
    private readonly ConsultaVentasService _servicio;

    public ConsultaVentasServiceTests()
    {
        _usuario.SetupGet(u => u.CodigoPuntoVenta).Returns("001");
        _puntosVenta.Setup(p => p.ObtenerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PuntoVenta(" 001", "HEMEROTECA", "L10", activo: true));
        _catalogos.Setup(c => c.ListarItemsActivosAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int codigo, CancellationToken _) => codigo switch
            {
                Catalogos.TipoComprobante => [new ItemCatalogo(24, 2, "B", "BOLETA", true)],
                Catalogos.MedioPago => [new ItemCatalogo(14, 7, "4", "TARJETA CREDITO", true)],
                Catalogos.FormaPago => [new ItemCatalogo(10, 9, "C00", "CONTADO", true)],
                Catalogos.TipoDocumento => [new ItemCatalogo(2, 24, "2", "DNI", true)],
                _ => [],
            });
        _catalogos.Setup(c => c.ListarBancosActivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Banco("002", "BCP", true)]);

        _servicio = new ConsultaVentasService(_consultas.Object, _puntosVenta.Object, _catalogos.Object, _usuario.Object);
    }

    /// <summary>Venta 4521 con su comprobante 77 (B001-0000845) pagado con tarjeta, en el punto de venta indicado.</summary>
    private (Venta Venta, Comprobante Comprobante) VentaEmitida(string puntoVenta = "001")
    {
        var venta = VentaPrueba.Crear("B", puntoVenta).ConId(4521);
        venta.AgregarLinea("ART001", "UND", 2, 59.0000m);
        var comprobante = Comprobante.Emitir(venta,
            new ClienteComprobante(VentaPrueba.CodigoCliente, "QUISPE ROSA", "AV. LIMA 1", "2", "41234567", false),
            "L10-SEDE CENTRAL", 845, new DateTime(2026, 10, 29, 10, 31, 0),
            new Dictionary<string, string> { ["ART001"] = "CUADERNO A4" }).ConId(77);
        comprobante.AsignarPago(MedioPago.Registrar(venta, TipoMedioPago.TarjetaCredito, "002", "OP-1"));
        venta.MarcarEmitida(77);

        _consultas.Setup(c => c.ObtenerComprobanteAsync(77, It.IsAny<CancellationToken>())).ReturnsAsync(comprobante);
        _consultas.Setup(c => c.ObtenerVentaAsync(4521, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VentaRegistrada(venta, "jperez"));
        return (venta, comprobante);
    }

    [Fact]
    public async Task ObtenerComprobante_DevuelveCabeceraDetallePagoYTrazabilidadConDescripciones()
    {
        VentaEmitida();

        var dto = await _servicio.ObtenerComprobanteAsync(77, CancellationToken.None);

        dto.NumeroCompleto.Should().Be("B001-0000845");
        dto.TipoComprobante.Should().Be("BOLETA");
        dto.Estado.Should().Be("Cancelado");
        dto.FormaPago.Should().Be("CONTADO");
        dto.Documento.Should().Be("DNI 41234567");
        dto.PuntoVenta.Should().Be("HEMEROTECA");
        dto.CodigoTransaccion.Should().Be(4521);
        dto.Usuario.Should().Be("jperez");
        dto.MontoTotal.Should().Be(118.0000m);
        dto.Detalle.Should().ContainSingle().Which.Should().Be(
            new DetalleComprobanteDto(1, "ART001", "CUADERNO A4", "UND", 2, 59.0000m, 100.0000m, 18.0000m, 118.0000m));
        dto.Pagos.Should().ContainSingle().Which.Should().Be(new PagoComprobanteDto("TARJETA CREDITO", "BCP", "OP-1", 118.0000m));
        dto.Enlaces.Should().Be("ítem 1 ↔ línea 1");
    }

    [Fact]
    public async Task ObtenerComprobante_DeOtroPuntoDeVenta_AccesoDenegado()
    {
        VentaEmitida(puntoVenta: "002");

        var accion = () => _servicio.ObtenerComprobanteAsync(77, CancellationToken.None);

        await accion.Should().ThrowAsync<AccesoDenegadoException>().WithMessage("*B002-0000845*otro punto de venta*");
    }

    [Fact]
    public async Task ObtenerComprobante_Inexistente_NoEncontrado()
    {
        var accion = () => _servicio.ObtenerComprobanteAsync(999, CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Theory]
    [InlineData("845")]
    [InlineData("0000845")]
    [InlineData(" 845 ")]
    public async Task BuscarComprobante_FormaLaSerieDelPuntoDeVentaYCompletaElNumero(string numero)
    {
        _consultas.Setup(c => c.BuscarCodigoComprobanteAsync("B001", "0000845", It.IsAny<CancellationToken>())).ReturnsAsync(77);

        (await _servicio.BuscarComprobanteAsync("B", numero, CancellationToken.None)).Should().Be(77);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("0")]
    public async Task BuscarComprobante_NumeroInvalido_NoEncontrado(string? numero)
    {
        var accion = () => _servicio.BuscarComprobanteAsync("B", numero, CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Fact]
    public async Task BuscarComprobanteDeVenta_DevuelveElComprobanteEnlazado()
    {
        VentaEmitida();

        (await _servicio.BuscarComprobanteDeVentaAsync(4521, CancellationToken.None)).Should().Be(77);
    }

    [Fact]
    public async Task BuscarComprobanteDeVenta_DeOtroPuntoDeVenta_AccesoDenegado()
    {
        VentaEmitida(puntoVenta: "002");

        var accion = () => _servicio.BuscarComprobanteDeVentaAsync(4521, CancellationToken.None);

        await accion.Should().ThrowAsync<AccesoDenegadoException>();
    }

    [Fact]
    public async Task BuscarComprobanteDeVenta_SinEmitir_NoEncontrado()
    {
        var venta = VentaPrueba.Crear().ConId(10);
        _consultas.Setup(c => c.ObtenerVentaAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new VentaRegistrada(venta, "x"));

        var accion = () => _servicio.BuscarComprobanteDeVentaAsync(10, CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Fact]
    public async Task ListarRecientes_SoloDelPuntoDeVentaDeLaSesionYConMaximo()
    {
        _consultas.Setup(c => c.ListarAsync(It.IsAny<FiltroVentasRegistradas>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await _servicio.ListarRecientesAsync(CancellationToken.None);

        _consultas.Verify(c => c.ListarAsync(
            It.Is<FiltroVentasRegistradas>(f => f.CodigoPuntoVenta == "001" && f.Maximo == ConsultaVentasService.MaximoRecientes),
            It.IsAny<CancellationToken>()));
    }
}

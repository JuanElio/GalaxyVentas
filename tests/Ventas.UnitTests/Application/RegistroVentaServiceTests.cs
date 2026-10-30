using FluentAssertions;
using FluentValidation;
using Moq;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Application.Ventas;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Comun;
using Ventas.Domain.Inventario;
using Ventas.Domain.Maestros;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;

namespace Ventas.UnitTests.Application;

public class RegistroVentaServiceTests
{
    private static readonly DateTime Hoy = new(2026, 10, 29, 10, 0, 0);

    private readonly Mock<IClienteRepository> _clientes = new();
    private readonly Mock<IArticuloRepository> _articulos = new();
    private readonly Mock<IPuntoVentaRepository> _puntosVenta = new();
    private readonly Mock<IStockService> _stock = new();
    private readonly Mock<ICorrelativoService> _correlativos = new();
    private readonly Mock<IVentaRepository> _ventas = new();
    private readonly Mock<IComprobanteRepository> _comprobantes = new();
    private readonly Mock<IKardexRepository> _kardex = new();
    private readonly Mock<IUnitOfWork> _unidadTrabajo = new();
    private readonly Mock<IUsuarioActual> _usuario = new();
    private readonly RegistroVentaService _servicio;

    private readonly Impresora _impresora = new(1023, "B001", 243455, activo: true);
    private Venta? _ventaGrabada;
    private Comprobante? _comprobanteGrabado;
    private List<MovimientoKardex> _kardexGrabado = [];

    private static readonly Cliente ClienteDni =
        Cliente.Registrar("2", "41234567", "Rosa", "Quispe Díaz", null, "AV. LIMA 123 ", null, null).ConId(125);

    public RegistroVentaServiceTests()
    {
        _usuario.SetupGet(u => u.CodigoPuntoVenta).Returns("001");
        _puntosVenta.Setup(p => p.ObtenerAsync("001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PuntoVenta(" 001", "HEMEROTECA", "L10", activo: true));
        _puntosVenta.Setup(p => p.ObtenerAlmacenPrincipalAsync("001", It.IsAny<CancellationToken>())).ReturnsAsync("110");
        _puntosVenta.Setup(p => p.ObtenerZonaVentaAsync("L10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ZonaVenta("L10", "SEDE CENTRAL                  "));
        _puntosVenta.Setup(p => p.ObtenerTipoOrdenAsync("V01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TipoOrden("V01", "VENTA", 1, activo: true));
        _clientes.Setup(c => c.ObtenerActivoAsync(125, It.IsAny<CancellationToken>())).ReturnsAsync(ClienteDni);
        _articulos.Setup(a => a.ObtenerConTarifasVigentesAsync("01", It.IsAny<IReadOnlyCollection<string>>(), Hoy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ArticuloConTarifas(new Articulo("ART001", "CUADERNO A4", "UND", true),
                    [new Tarifa("ART001", " 01", 999999, 59.0000m, new DateTime(2020, 1, 1), new DateTime(2050, 1, 1), true)]),
            ]);
        _stock.Setup(s => s.ObtenerStockAsync("001", It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["ART001"] = 5 });
        _correlativos.Setup(c => c.ObtenerParaEmitirAsync("B001", It.IsAny<CancellationToken>())).ReturnsAsync(_impresora);

        _unidadTrabajo.Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<Task<VentaRegistradaDto>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task<VentaRegistradaDto>> operacion, CancellationToken _) => operacion());
        _ventas.Setup(v => v.AgregarAsync(It.IsAny<Venta>(), It.IsAny<CancellationToken>()))
            .Callback((Venta v, CancellationToken _) => _ventaGrabada = v);
        _comprobantes.Setup(c => c.AgregarAsync(It.IsAny<Comprobante>(), It.IsAny<CancellationToken>()))
            .Callback((Comprobante c, CancellationToken _) => _comprobanteGrabado = c);
        _kardex.Setup(k => k.AgregarAsync(It.IsAny<IEnumerable<MovimientoKardex>>(), It.IsAny<CancellationToken>()))
            .Callback((IEnumerable<MovimientoKardex> m, CancellationToken _) => _kardexGrabado = m.ToList());

        _servicio = new RegistroVentaService(_clientes.Object, _articulos.Object, _puntosVenta.Object, _stock.Object,
            _correlativos.Object, _ventas.Object, _comprobantes.Object, _kardex.Object, _unidadTrabajo.Object,
            _usuario.Object, new RegistrarVentaValidator(), new RelojFijo(Hoy));
    }

    private static RegistrarVentaDto Datos(string tipo = "B", decimal cantidad = 2, PagoVentaDto? pago = null) =>
        new(125, tipo, [new LineaVentaDto("ART001", cantidad)], pago ?? new PagoVentaDto(1, null, null));

    [Fact]
    public async Task Registrar_ArmaVentaComprobanteKardexYPagoConLosDatosDeLaSesion()
    {
        var resultado = await _servicio.RegistrarAsync(Datos(), CancellationToken.None);

        resultado.Serie.Should().Be("B001");
        resultado.Numero.Should().Be("0243455");
        resultado.MontoTotal.Should().Be(118.0000m);
        _impresora.Correlativo.Should().Be(243456);

        _ventaGrabada!.Should().BeEquivalentTo(new
        {
            CodigoPuntoVenta = "001",
            CodigoCliente = 125L,
            TipoOrden = "V01",
            TipoLiquidacion = "1",
            CodigoTarifaCanal = "01",
            CodigoAlmacen = "110",
            CodigoMoneda = "SOL",
            FormaPago = "C00",
            TipoComprobante = "B",
            Situacion = SituacionVenta.Emitida,
        }, o => o.ExcludingMissingMembers());

        _comprobanteGrabado!.NombreCliente.Should().Be("Quispe Díaz Rosa"); // apellidos y nombres, como el procedimiento
        _comprobanteGrabado.DireccionCliente.Should().Be("AV. LIMA 123");
        _comprobanteGrabado.ZonaVenta.Should().Be("L10-SEDE CENTRAL");
        _comprobanteGrabado.Estado.Should().Be(EstadoComprobante.Cancelado);
        _comprobanteGrabado.Pagos.Single().MedioPago.Should().Match<MedioPago>(m => m.Tipo == TipoMedioPago.Efectivo && m.Monto == 118.0000m);

        _kardexGrabado.Should().ContainSingle().Which.Should().Match<MovimientoKardex>(m =>
            m.Cantidad == -2 && m.CodigoPuntoVenta == "001" && m.Descripcion == "Salida por venta B001-0243455");

        // Reserva el stock antes de leerlo y graba en tres pasos dentro de la transacción
        _stock.Verify(s => s.BloquearAsync("001", It.Is<IReadOnlyCollection<string>>(c => c.Single() == "ART001"), It.IsAny<CancellationToken>()));
        _unidadTrabajo.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Registrar_StockInsuficiente_RechazaSinGrabarNiTomarCorrelativo()
    {
        var accion = () => _servicio.RegistrarAsync(Datos(cantidad: 6), CancellationToken.None);

        await accion.Should().ThrowAsync<DominioException>().WithMessage("Stock insuficiente para ART001: disponible 5, solicitado 6.");
        _ventas.VerifyNoOtherCalls();
        _impresora.Correlativo.Should().Be(243455);
    }

    [Fact]
    public async Task Registrar_SinImpresoraParaLaSerie_Rechaza()
    {
        var accion = () => _servicio.RegistrarAsync(Datos(tipo: "F"), CancellationToken.None);

        await accion.Should().ThrowAsync<DominioException>().WithMessage("*impresora activa*F001*");
    }

    [Fact]
    public async Task Registrar_PuntoDeVentaSinAlmacen_Rechaza()
    {
        _puntosVenta.Setup(p => p.ObtenerAlmacenPrincipalAsync("001", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        var accion = () => _servicio.RegistrarAsync(Datos(), CancellationToken.None);

        await accion.Should().ThrowAsync<DominioException>().WithMessage("*almacén*");
        _unidadTrabajo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Registrar_ClienteInexistente_NoEncontrado()
    {
        var accion = () => _servicio.RegistrarAsync(Datos() with { CodigoCliente = 999 }, CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("*999*");
    }

    [Fact]
    public async Task Registrar_ArticuloSinTarifa_Rechaza()
    {
        var accion = () => _servicio.RegistrarAsync(
            new RegistrarVentaDto(125, "B", [new LineaVentaDto("NOEXISTE", 1)], new PagoVentaDto(1, null, null)), CancellationToken.None);

        await accion.Should().ThrowAsync<DominioException>().WithMessage("*NOEXISTE*");
    }

    [Fact]
    public async Task Registrar_TarjetaSinBanco_ErrorDeValidacionSinAbrirTransaccion()
    {
        var accion = () => _servicio.RegistrarAsync(Datos(pago: new PagoVentaDto(4, null, null)), CancellationToken.None);

        await accion.Should().ThrowAsync<ValidationException>();
        _unidadTrabajo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Registrar_ConTarjeta_GrabaBancoYNumeroDeOperacion()
    {
        await _servicio.RegistrarAsync(Datos(pago: new PagoVentaDto(4, "002", "OP-7781")), CancellationToken.None);

        _comprobanteGrabado!.Pagos.Single().MedioPago.Should().Match<MedioPago>(m =>
            m.Tipo == TipoMedioPago.TarjetaCredito && m.CodigoBanco == "002" && m.NumeroOperacion == "OP-7781");
    }
}

using FluentAssertions;
using FluentValidation;
using Moq;
using Ventas.Application.Comprobantes;
using Ventas.Application.Maestros;
using Ventas.Application.Reportes;
using Ventas.Application.Seguridad;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Maestros;
using Ventas.Domain.Pagos;

namespace Ventas.UnitTests.Application;

public class ReporteVentasServiceTests
{
    private static readonly DateTime Hoy = new(2026, 10, 29);

    private readonly Mock<IConsultaVentasRepository> _consultas = new();
    private readonly Mock<ICatalogoRepository> _catalogos = new();
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IUsuarioActual> _usuario = new();
    private readonly ReporteVentasService _servicio;

    public ReporteVentasServiceTests()
    {
        _usuario.SetupGet(u => u.CodigoPuntoVenta).Returns("001");
        _catalogos.Setup(c => c.ListarItemsActivosAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int codigo, CancellationToken _) => codigo switch
            {
                Catalogos.TipoComprobante => [new ItemCatalogo(24, 1, "F", "FACTURA", true), new ItemCatalogo(24, 2, "B", "BOLETA", true)],
                Catalogos.MedioPago => [new ItemCatalogo(14, 4, "1", "EFECTIVO", true), new ItemCatalogo(14, 7, "4", "TARJETA CREDITO", true)],
                _ => [],
            });
        _servicio = new ReporteVentasService(_consultas.Object, _catalogos.Object, _usuarios.Object, _usuario.Object,
            new FiltroReporteVentasValidator());
    }

    private static FilaVentaRegistrada Fila(long codigo, string tipo, decimal total, TipoMedioPago medio) =>
        new(codigo, codigo + 1000, Hoy.AddHours(10), tipo, $"{tipo}001", codigo.ToString("D7"), "2", "41234567", "CLIENTE",
            total - Math.Round(total / 1.18m * 0.18m, 4), Math.Round(total / 1.18m * 0.18m, 4), total, medio,
            EstadoComprobante.Cancelado, "jperez");

    [Fact]
    public async Task Generar_TotalesSonLaSumaDeLasFilasYAgrupaPorMedioDePago()
    {
        _consultas.Setup(c => c.ListarAsync(It.IsAny<FiltroVentasRegistradas>(), It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            Fila(1, "B", 118.0000m, TipoMedioPago.Efectivo),
            Fila(2, "F", 59.0050m, TipoMedioPago.TarjetaCredito),
            Fila(3, "B", 10.0000m, TipoMedioPago.Efectivo),
        ]);

        var reporte = await _servicio.GenerarAsync(new FiltroReporteVentasDto(Hoy, Hoy), CancellationToken.None);

        reporte.CantidadComprobantes.Should().Be(3);
        reporte.MontoTotal.Should().Be(187.0050m);
        reporte.SubTotal.Should().Be(reporte.Filas.Sum(f => f.SubTotal));
        reporte.MontoIgv.Should().Be(reporte.Filas.Sum(f => f.MontoIgv));
        reporte.Filas[1].Should().Match<FilaReporteVentasDto>(f =>
            f.TipoComprobante == "FACTURA" && f.Comprobante == "F001-0000002" && f.MedioPago == "TARJETA CREDITO" && f.Estado == "Cancelado");
        reporte.PorMedioPago.Should().Equal(
            new TotalMedioPagoDto("EFECTIVO", 2, 128.0000m),
            new TotalMedioPagoDto("TARJETA CREDITO", 1, 59.0050m));
    }

    [Fact]
    public async Task Generar_SinVentas_ReporteVacioConTotalesEnCero()
    {
        _consultas.Setup(c => c.ListarAsync(It.IsAny<FiltroVentasRegistradas>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var reporte = await _servicio.GenerarAsync(new FiltroReporteVentasDto(Hoy, Hoy), CancellationToken.None);

        reporte.CantidadComprobantes.Should().Be(0);
        reporte.MontoTotal.Should().Be(0);
        reporte.PorMedioPago.Should().BeEmpty();
    }

    [Fact]
    public async Task Generar_PasaLosFiltrosConElPuntoDeVentaDeLaSesion()
    {
        _consultas.Setup(c => c.ListarAsync(It.IsAny<FiltroVentasRegistradas>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await _servicio.GenerarAsync(new FiltroReporteVentasDto(Hoy.AddDays(-5).AddHours(15), Hoy, "F", 4, " jperez "), CancellationToken.None);

        _consultas.Verify(c => c.ListarAsync(It.Is<FiltroVentasRegistradas>(f =>
            f.CodigoPuntoVenta == "001" && f.Desde == Hoy.AddDays(-5) && f.Hasta == Hoy && f.TipoComprobante == "F"
            && f.MedioPago == TipoMedioPago.TarjetaCredito && f.Usuario == "jperez" && f.Maximo == null), It.IsAny<CancellationToken>()));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(30, true)]   // 31 días con ambos extremos
    [InlineData(31, false)]  // 32 días
    public async Task Generar_RangoMaximoDe31Dias(int diasAtras, bool valido)
    {
        _consultas.Setup(c => c.ListarAsync(It.IsAny<FiltroVentasRegistradas>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var accion = () => _servicio.GenerarAsync(new FiltroReporteVentasDto(Hoy.AddDays(-diasAtras), Hoy), CancellationToken.None);

        if (valido)
            await accion.Should().NotThrowAsync();
        else
            await accion.Should().ThrowAsync<ValidationException>().WithMessage("*31 días*");
    }

    [Fact]
    public async Task Generar_FechaInicialMayorQueLaFinal_ErrorDeValidacion()
    {
        var accion = () => _servicio.GenerarAsync(new FiltroReporteVentasDto(Hoy, Hoy.AddDays(-1)), CancellationToken.None);

        await accion.Should().ThrowAsync<ValidationException>().WithMessage("*inicial no puede ser mayor*");
        _consultas.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Generar_SinFechas_ErrorDeValidacion()
    {
        var accion = () => _servicio.GenerarAsync(new FiltroReporteVentasDto(null, null), CancellationToken.None);

        (await accion.Should().ThrowAsync<ValidationException>()).Which.Errors.Should().HaveCount(2);
    }
}

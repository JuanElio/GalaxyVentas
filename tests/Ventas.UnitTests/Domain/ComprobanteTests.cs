using FluentAssertions;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Comun;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;

namespace Ventas.UnitTests.Domain;

public class ComprobanteTests
{
    private static readonly DateTime FechaEmision = new(2026, 10, 29, 10, 31, 0);

    private static readonly ClienteComprobante ClienteDni =
        new(VentaPrueba.CodigoCliente, "JUAN PÉREZ", "AV. LIMA 123", "01", "12345678", TieneRuc: false);

    private static readonly ClienteComprobante ClienteRuc =
        new(VentaPrueba.CodigoCliente, "EMPRESA SAC", "AV. ARENALES 456", "06", "20123456789", TieneRuc: true);

    private static readonly Dictionary<string, string> Nombres = new()
    {
        ["ART001"] = "CUADERNO A4",
        ["ART002"] = "LAPICERO AZUL",
    };

    private static Venta VentaConLineas(string tipoComprobante = "B")
    {
        var venta = VentaPrueba.Crear(tipoComprobante);
        venta.AgregarLinea("ART001", "UND", 2, 59.0000m);
        venta.AgregarLinea("ART002", "UND", 3, 1.5000m);
        return venta;
    }

    [Fact]
    public void Emitir_CopiaClienteSerieNumeroYTotalesDeLaVenta()
    {
        var venta = VentaConLineas();

        var comprobante = Comprobante.Emitir(venta, ClienteDni, "Z01", 1521, FechaEmision, Nombres);

        comprobante.Serie.Should().Be("B001");
        comprobante.Numero.Should().Be("0001521");
        comprobante.FechaEmision.Should().Be(FechaEmision);
        comprobante.NombreCliente.Should().Be("JUAN PÉREZ");
        comprobante.NumeroDocumento.Should().Be("12345678");
        comprobante.ZonaVenta.Should().Be("Z01");
        comprobante.Estado.Should().Be(EstadoComprobante.Emitido);
        comprobante.Facturado.Should().BeTrue();
        comprobante.TieneMedioPago.Should().BeFalse();
        comprobante.TipoCambio.Should().Be(1m);

        // RN-12: el total del comprobante es igual al de la venta
        comprobante.MontoBruto.Should().Be(venta.MontoBruto);
        comprobante.SubTotal.Should().Be(venta.SubTotal);
        comprobante.MontoIgv.Should().Be(venta.MontoIgv);
        comprobante.MontoTotal.Should().Be(venta.MontoTotal).And.Be(122.5000m);
    }

    [Fact]
    public void Emitir_UnItemYUnMovimientoPorCadaLineaDeLaVenta()
    {
        var comprobante = Comprobante.Emitir(VentaConLineas(), ClienteDni, null, 1, FechaEmision, Nombres);

        comprobante.Detalles.Should().HaveCount(2);
        var item = comprobante.Detalles[0];
        item.Item.Should().Be(1);
        item.CodigoArticulo.Should().Be("ART001");
        item.NombreArticulo.Should().Be("CUADERNO A4");
        item.Cantidad.Should().Be(2);
        item.PrecioUnitario.Should().Be(59.0000m);
        item.SubTotal.Should().Be(118.0000m);
        item.Igv.Should().Be(18.0000m);
        item.MontoBruto.Should().Be(item.SubTotal + item.MontoRecargo - item.MontoDescuento);

        comprobante.Movimientos.Select(m => (m.Item, m.LineaTransaccion)).Should().Equal((1, 1), (2, 2));
    }

    [Fact]
    public void Emitir_FacturaConClienteSinRuc_LanzaExcepcionDeDominio()
    {
        var accion = () => Comprobante.Emitir(VentaConLineas("F"), ClienteDni, null, 1, FechaEmision, Nombres);

        accion.Should().Throw<DominioException>().WithMessage("*factura*RUC*");
    }

    [Fact]
    public void Emitir_FacturaConClienteConRuc_UsaSerieF()
    {
        var comprobante = Comprobante.Emitir(VentaConLineas("F"), ClienteRuc, null, 7, FechaEmision, Nombres);

        comprobante.Serie.Should().Be("F001");
        comprobante.Numero.Should().Be("0000007");
    }

    [Fact]
    public void Emitir_ClienteDistintoAlDeLaVenta_LanzaExcepcionDeDominio()
    {
        var otroCliente = ClienteDni with { Codigo = 999 };

        var accion = () => Comprobante.Emitir(VentaConLineas(), otroCliente, null, 1, FechaEmision, Nombres);

        accion.Should().Throw<DominioException>();
    }

    [Fact]
    public void Emitir_VentaSinLineas_LanzaExcepcionDeDominio()
    {
        var accion = () => Comprobante.Emitir(VentaPrueba.Crear(), ClienteDni, null, 1, FechaEmision, Nombres);

        accion.Should().Throw<DominioException>().WithMessage("*al menos una línea*");
    }

    [Fact]
    public void Emitir_FaltaNombreDeArticulo_LanzaExcepcionDeDominio()
    {
        var nombres = new Dictionary<string, string> { ["ART001"] = "CUADERNO A4" };

        var accion = () => Comprobante.Emitir(VentaConLineas(), ClienteDni, null, 1, FechaEmision, nombres);

        accion.Should().Throw<DominioException>().WithMessage("*ART002*");
    }

    [Fact]
    public void AsignarPago_PorElTotal_DejaElComprobanteCancelado()
    {
        var venta = VentaConLineas();
        var comprobante = Comprobante.Emitir(venta, ClienteDni, null, 1, FechaEmision, Nombres);
        var pago = MedioPago.Registrar(venta, TipoMedioPago.Efectivo, null, null);

        comprobante.AsignarPago(pago);

        comprobante.Estado.Should().Be(EstadoComprobante.Cancelado);
        comprobante.TieneMedioPago.Should().BeTrue();
        comprobante.MontoPagado.Should().Be(comprobante.MontoTotal);
        comprobante.Pagos.Should().ContainSingle()
            .Which.Should().Match<PagoComprobante>(p => p.Monto == comprobante.MontoTotal && p.MedioPago == pago);
    }

    [Fact]
    public void AsignarPago_SegundoPago_LanzaExcepcionDeDominio()
    {
        var venta = VentaConLineas();
        var comprobante = Comprobante.Emitir(venta, ClienteDni, null, 1, FechaEmision, Nombres);
        comprobante.AsignarPago(MedioPago.Registrar(venta, TipoMedioPago.Efectivo, null, null));

        var accion = () => comprobante.AsignarPago(MedioPago.Registrar(venta, TipoMedioPago.Efectivo, null, null));

        accion.Should().Throw<DominioException>().WithMessage("*ya tiene un medio de pago*");
    }

    [Fact]
    public void AsignarPago_MontoDistintoAlTotal_LanzaExcepcionDeDominio()
    {
        var comprobante = Comprobante.Emitir(VentaConLineas(), ClienteDni, null, 1, FechaEmision, Nombres);
        var otraVenta = VentaPrueba.Crear();
        otraVenta.AgregarLinea("ART001", "UND", 1, 1m);

        var accion = () => comprobante.AsignarPago(MedioPago.Registrar(otraVenta, TipoMedioPago.Efectivo, null, null));

        accion.Should().Throw<DominioException>().WithMessage("*igual al total*");
    }
}

using FluentAssertions;
using Ventas.Domain.Comun;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;

namespace Ventas.UnitTests.Domain;

public class MedioPagoTests
{
    private static Venta VentaDeCien()
    {
        var venta = VentaPrueba.Crear();
        venta.AgregarLinea("ART001", "UND", 1, 100m);
        return venta;
    }

    [Fact]
    public void Registrar_Efectivo_PorElTotalSinBancoNiOperacion()
    {
        var pago = MedioPago.Registrar(VentaDeCien(), TipoMedioPago.Efectivo, "BCP", "123");

        pago.Tipo.Should().Be(TipoMedioPago.Efectivo);
        pago.Monto.Should().Be(100m);
        pago.MontoOrigen.Should().Be(100m);
        pago.CodigoMoneda.Should().Be("PEN");
        pago.TipoCambio.Should().Be(1m);
        pago.CodigoBanco.Should().BeNull();
        pago.NumeroOperacion.Should().BeNull();
    }

    [Theory]
    [InlineData(TipoMedioPago.Cheque)]
    [InlineData(TipoMedioPago.TarjetaDebito)]
    [InlineData(TipoMedioPago.TarjetaCredito)]
    [InlineData(TipoMedioPago.BoletaDeposito)]
    public void Registrar_MedioConBanco_GuardaBancoYOperacion(TipoMedioPago tipo)
    {
        var pago = MedioPago.Registrar(VentaDeCien(), tipo, " 002 ", " OP-9981 ");

        pago.Tipo.Should().Be(tipo);
        pago.CodigoBanco.Should().Be("002");
        pago.NumeroOperacion.Should().Be("OP-9981");
    }

    [Theory]
    [InlineData(TipoMedioPago.TarjetaCredito, null, "OP-1", "*banco*")]
    [InlineData(TipoMedioPago.TarjetaDebito, " ", "OP-1", "*banco*")]
    [InlineData(TipoMedioPago.Cheque, "002", null, "*número de operación*")]
    [InlineData(TipoMedioPago.BoletaDeposito, "002", "", "*número de operación*")]
    public void Registrar_FaltaBancoUOperacion_LanzaExcepcionDeDominio(
        TipoMedioPago tipo, string? banco, string? operacion, string mensaje)
    {
        var accion = () => MedioPago.Registrar(VentaDeCien(), tipo, banco, operacion);

        accion.Should().Throw<DominioException>().WithMessage(mensaje);
    }

    [Fact]
    public void Registrar_MedioDePagoInexistente_LanzaExcepcionDeDominio()
    {
        var accion = () => MedioPago.Registrar(VentaDeCien(), (TipoMedioPago)9, null, null);

        accion.Should().Throw<DominioException>();
    }

    [Fact]
    public void Registrar_VentaSinImporte_LanzaExcepcionDeDominio()
    {
        var accion = () => MedioPago.Registrar(VentaPrueba.Crear(), TipoMedioPago.Efectivo, null, null);

        accion.Should().Throw<DominioException>().WithMessage("*mayor que 0*");
    }
}

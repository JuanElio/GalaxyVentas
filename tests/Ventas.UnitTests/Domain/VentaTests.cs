using FluentAssertions;
using Ventas.Domain.Comun;
using Ventas.Domain.Ventas;

namespace Ventas.UnitTests.Domain;

public class VentaTests
{
    [Fact]
    public void AgregarLinea_TarifaConIgv_DesglosaElIgvDeLaLinea()
    {
        var venta = VentaPrueba.Crear();

        var linea = venta.AgregarLinea("ART001", "UND", cantidad: 2, tarifa: 59.0000m);

        linea.NumeroLinea.Should().Be(1);
        linea.MontoBruto.Should().Be(118.0000m);
        linea.SubTotal.Should().Be(118.0000m);
        linea.Igv.Should().Be(18.0000m);
        linea.ValorVenta.Should().Be(100.0000m);
    }

    [Fact]
    public void AgregarLinea_CalculaLaCabeceraConLaConvencionDeReferencia()
    {
        var venta = VentaPrueba.Crear();

        venta.AgregarLinea("ART001", "UND", 2, 59.0000m);

        venta.MontoBruto.Should().Be(118.0000m);
        venta.SubTotal.Should().Be(100.0000m);
        venta.MontoIgv.Should().Be(18.0000m);
        venta.MontoTotal.Should().Be(118.0000m);
        venta.MontoDescuento.Should().Be(0);
        venta.MontoRecargo.Should().Be(0);
    }

    [Fact]
    public void AgregarLinea_VariasLineas_TotalesSonLaSumaExactaDeLasLineas()
    {
        var venta = VentaPrueba.Crear();

        venta.AgregarLinea("ART001", "UND", 3, 10.0000m);
        venta.AgregarLinea("ART002", "UND", 1, 7.3300m);
        venta.AgregarLinea("ART003", "KG", 1.5m, 4.9900m);

        venta.Detalles.Select(d => d.NumeroLinea).Should().Equal(1, 2, 3);
        venta.MontoBruto.Should().Be(venta.Detalles.Sum(d => d.SubTotal));
        venta.SubTotal.Should().Be(venta.Detalles.Sum(d => d.SubTotal - d.Igv));
        venta.MontoIgv.Should().Be(venta.Detalles.Sum(d => d.Igv));
        venta.MontoTotal.Should().Be(venta.SubTotal + venta.MontoIgv);
        // El cliente paga exactamente Σ (cantidad × tarifa)
        venta.MontoTotal.Should().Be(30.0000m + 7.3300m + 7.4850m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AgregarLinea_CantidadNoPositiva_LanzaExcepcionDeDominio(decimal cantidad)
    {
        var venta = VentaPrueba.Crear();

        var accion = () => venta.AgregarLinea("ART001", "UND", cantidad, 10m);

        accion.Should().Throw<DominioException>().WithMessage("*mayor que 0*");
        venta.Detalles.Should().BeEmpty();
    }

    [Fact]
    public void AgregarLinea_TarifaNegativa_LanzaExcepcionDeDominio()
    {
        var accion = () => VentaPrueba.Crear().AgregarLinea("ART001", "UND", 1, -0.01m);

        accion.Should().Throw<DominioException>().WithMessage("*negativa*");
    }

    [Fact]
    public void AgregarLinea_ArticuloRepetido_LanzaExcepcionDeDominio()
    {
        var venta = VentaPrueba.Crear();
        venta.AgregarLinea("ART001", "UND", 1, 10m);

        var accion = () => venta.AgregarLinea(" art001 ", "UND", 2, 10m);

        accion.Should().Throw<DominioException>().WithMessage("*ya está en otra línea*");
    }

    [Fact]
    public void Crear_VentaNueva_QuedaRegistradaEnSolesConTipoDeCambioUno()
    {
        var venta = VentaPrueba.Crear();

        venta.Situacion.Should().Be(SituacionVenta.Registrada);
        venta.TipoCambio.Should().Be(1m);
        venta.CodigoComprobante.Should().BeNull();
    }

    [Fact]
    public void Crear_SinCliente_LanzaExcepcionDeDominio()
    {
        var accion = () => new Venta("001", 0, "V01", "01", "01", "ALM01", "PEN", "C00", "B", DateTime.Now);

        accion.Should().Throw<DominioException>().WithMessage("El cliente es obligatorio.");
    }

    [Fact]
    public void MarcarEmitida_EnlazaElComprobanteYCambiaLaSituacion()
    {
        var venta = VentaPrueba.Crear();
        venta.AgregarLinea("ART001", "UND", 1, 10m);

        venta.MarcarEmitida(4521);

        venta.Situacion.Should().Be(SituacionVenta.Emitida);
        venta.CodigoComprobante.Should().Be(4521);
    }

    [Fact]
    public void MarcarEmitida_SinLineas_LanzaExcepcionDeDominio()
    {
        var accion = () => VentaPrueba.Crear().MarcarEmitida(1);

        accion.Should().Throw<DominioException>().WithMessage("*al menos una línea*");
    }

    [Fact]
    public void MarcarEmitida_DosVeces_LanzaExcepcionDeDominio()
    {
        var venta = VentaPrueba.Crear();
        venta.AgregarLinea("ART001", "UND", 1, 10m);
        venta.MarcarEmitida(1);

        var accion = () => venta.MarcarEmitida(2);

        accion.Should().Throw<DominioException>().WithMessage("*ya tiene un comprobante*");
    }

    [Fact]
    public void AgregarLinea_VentaEmitida_LanzaExcepcionDeDominio()
    {
        var venta = VentaPrueba.Crear();
        venta.AgregarLinea("ART001", "UND", 1, 10m);
        venta.MarcarEmitida(1);

        var accion = () => venta.AgregarLinea("ART002", "UND", 1, 10m);

        accion.Should().Throw<DominioException>();
    }
}

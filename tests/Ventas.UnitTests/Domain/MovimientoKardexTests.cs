using FluentAssertions;
using Ventas.Domain.Comun;
using Ventas.Domain.Inventario;

namespace Ventas.UnitTests.Domain;

public class MovimientoKardexTests
{
    [Fact]
    public void SalidaPorVenta_GrabaLosValoresDelProcedimientoDeReferencia()
    {
        var movimiento = MovimientoKardex.SalidaPorVenta(
            " ART001", "01", " 001", new DateTime(2026, 10, 29, 10, 30, 15), 3, 4521, "B001", "0001521");

        movimiento.CodigoArticulo.Should().Be("ART001");
        movimiento.CodigoCanal.Should().Be("01");
        movimiento.CodigoPuntoVenta.Should().Be("001");
        movimiento.TipoTransaccion.Should().Be("VENTA");
        movimiento.FechaTransaccion.Should().Be(new DateTime(2026, 10, 29));
        movimiento.Cantidad.Should().Be(-3);
        movimiento.CodigoTransaccion.Should().Be(4521);
        movimiento.Descripcion.Should().Be("Salida por venta B001-0001521");
        movimiento.Estado.Should().BeEmpty();
        movimiento.Proceso.Should().BeEmpty();
        Guid.TryParse(movimiento.GuidRegistro, out _).Should().BeTrue();
        movimiento.Activo.Should().BeTrue();
    }

    [Fact]
    public void SalidaPorVenta_CantidadNoPositiva_LanzaExcepcionDeDominio()
    {
        var accion = () => MovimientoKardex.SalidaPorVenta("ART001", "01", "001", DateTime.Today, 0, 1, "B001", "0000001");

        accion.Should().Throw<DominioException>();
    }
}

using FluentAssertions;
using Ventas.Domain.Comun;

namespace Ventas.UnitTests.Domain;

public class ImportesTests
{
    [Theory]
    [InlineData(118.0000, 100.0000, 18.0000)]
    [InlineData(10.0000, 8.4746, 1.5254)]
    [InlineData(0.0100, 0.0085, 0.0015)]
    [InlineData(0, 0, 0)]
    public void DesglosarIgv_SeparaBaseEIgvSinPerderCentimos(decimal importe, decimal valorEsperado, decimal igvEsperado)
    {
        var (valorVenta, igv) = Importes.DesglosarIgv(importe);

        valorVenta.Should().Be(valorEsperado);
        igv.Should().Be(igvEsperado);
        (valorVenta + igv).Should().Be(importe);
    }

    [Fact]
    public void CalcularLinea_SubtotalEsCantidadPorTarifaYElIgvSeDesglosa()
    {
        var linea = Importes.CalcularLinea(2, 59.0000m);

        linea.Should().Be(new ImportesLinea(MontoBruto: 118.0000m, SubTotal: 118.0000m, ValorVenta: 100.0000m, Igv: 18.0000m));
    }

    [Fact]
    public void CalcularLinea_ArticuloExonerado_SinIgv()
    {
        var linea = Importes.CalcularLinea(2, 59.0000m, tasaIgv: 0m);

        linea.Should().Be(new ImportesLinea(MontoBruto: 118.0000m, SubTotal: 118.0000m, ValorVenta: 118.0000m, Igv: 0m));
    }

    [Fact]
    public void DesglosarIgv_TasaNegativa_LanzaExcepcionDeDominio()
    {
        var accion = () => Importes.DesglosarIgv(10m, -0.18m);

        accion.Should().Throw<DominioException>();
    }

    [Fact]
    public void Totalizar_SumaConLaConvencionDeReferencia()
    {
        var totales = Importes.Totalizar([(118.0000m, 18.0000m), (10.0000m, 1.5254m)]);

        totales.Should().Be(new TotalesVenta(MontoBruto: 128.0000m, SubTotal: 108.4746m, MontoIgv: 19.5254m, MontoTotal: 128.0000m));
    }

    [Fact]
    public void Totalizar_SinLineas_TodoEnCero() =>
        Importes.Totalizar([]).Should().Be(new TotalesVenta(0, 0, 0, 0));

    [Theory]
    [InlineData(1.00005, 1.0001)]
    [InlineData(1.00004, 1.0000)]
    [InlineData(2.12345678, 2.1235)]
    public void Redondear_UsaRedondeoComercialACuatroDecimales(decimal valor, decimal esperado) =>
        Importes.Redondear(valor).Should().Be(esperado);
}

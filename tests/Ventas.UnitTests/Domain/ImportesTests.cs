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

    [Theory]
    [InlineData(1.00005, 1.0001)]
    [InlineData(1.00004, 1.0000)]
    [InlineData(2.12345678, 2.1235)]
    public void Redondear_UsaRedondeoComercialACuatroDecimales(decimal valor, decimal esperado) =>
        Importes.Redondear(valor).Should().Be(esperado);
}

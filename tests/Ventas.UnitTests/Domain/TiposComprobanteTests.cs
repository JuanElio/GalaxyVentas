using FluentAssertions;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Comun;

namespace Ventas.UnitTests.Domain;

public class TiposComprobanteTests
{
    [Theory]
    [InlineData("F", "001", "F001")]
    [InlineData("B", "001", "B001")]
    [InlineData("b", " 002 ", "B002")]
    [InlineData("FA", "003", "F003")]
    public void FormarSerie_LetraDelTipoMasPuntoDeVenta(string tipo, string puntoVenta, string esperada) =>
        TiposComprobante.FormarSerie(tipo, puntoVenta).Should().Be(esperada);

    [Fact]
    public void FormarSerie_SuperaCuatroCaracteres_LanzaExcepcionDeDominio()
    {
        var accion = () => TiposComprobante.FormarSerie("B", "0001");

        accion.Should().Throw<DominioException>().WithMessage("*B0001*4 caracteres*");
    }

    [Theory]
    [InlineData("N")]
    [InlineData("")]
    [InlineData(" ")]
    public void Letra_TipoQueNoEsFacturaNiBoleta_LanzaExcepcionDeDominio(string tipo)
    {
        var accion = () => TiposComprobante.Letra(tipo);

        accion.Should().Throw<DominioException>();
    }

    [Theory]
    [InlineData(1, "0000001")]
    [InlineData(1521, "0001521")]
    [InlineData(9999999, "9999999")]
    public void FormatearNumero_SieteDigitosConCeros(int correlativo, string esperado) =>
        TiposComprobante.FormatearNumero(correlativo).Should().Be(esperado);

    [Fact]
    public void FormatearNumero_CorrelativoCero_LanzaExcepcionDeDominio()
    {
        var accion = () => TiposComprobante.FormatearNumero(0);

        accion.Should().Throw<DominioException>();
    }
}

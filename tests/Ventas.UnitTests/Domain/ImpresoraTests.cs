using FluentAssertions;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Comun;

namespace Ventas.UnitTests.Domain;

public class ImpresoraTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData(0, 1)]
    [InlineData(243455, 243455)]
    public void SiguienteNumero_EsElCorrelativoGuardadoO1(int? correlativo, int esperado) =>
        new Impresora(1023, "B001", correlativo, activo: true).SiguienteNumero.Should().Be(esperado);

    [Fact]
    public void TomarNumero_DevuelveElGuardadoYDejaElSiguiente()
    {
        var impresora = new Impresora(1023, "B001", 243455, activo: true);

        impresora.TomarNumero().Should().Be(243455);
        impresora.Correlativo.Should().Be(243456);
        impresora.TomarNumero().Should().Be(243456);
    }

    [Fact]
    public void TomarNumero_ImpresoraInactiva_LanzaExcepcionDeDominio()
    {
        var accion = () => new Impresora(1, "B001", 5, activo: false).TomarNumero();

        accion.Should().Throw<DominioException>().WithMessage("*B001*inactiva*");
    }
}

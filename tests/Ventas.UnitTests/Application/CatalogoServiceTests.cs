using FluentAssertions;
using Moq;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;

namespace Ventas.UnitTests.Application;

public class CatalogoServiceTests
{
    private readonly Mock<ICatalogoRepository> _catalogos = new();
    private readonly CatalogoService _servicio;

    public CatalogoServiceTests()
    {
        _servicio = new CatalogoService(_catalogos.Object);
    }

    [Fact]
    public async Task ListarMediosPago_SoloLosQueAdmiteElSistema()
    {
        _catalogos.Setup(c => c.ListarItemsActivosAsync(Catalogos.MedioPago, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new ItemCatalogo(14, 4, "1", "EFECTIVO", true),
            new ItemCatalogo(14, 6, "3", "TARJETA DEBITO", true),
            new ItemCatalogo(14, 7, "4", "TARJETA CREDITO", true),
            new ItemCatalogo(14, 9, "6", "ANTICIPO", true),
            new ItemCatalogo(14, 10, "7", "QR", true),
        ]);

        var medios = await _servicio.ListarMediosPagoAsync(CancellationToken.None);

        medios.Select(m => m.Valor).Should().Equal("1", "3", "4");
    }

    [Fact]
    public async Task Listar_RecortaEspaciosYOmiteItemsSinValor()
    {
        _catalogos.Setup(c => c.ListarItemsActivosAsync(Catalogos.TipoComprobante, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new ItemCatalogo(24, 1, "F ", " FACTURA", true),
            new ItemCatalogo(24, 2, null, "SIN VALOR", true),
            new ItemCatalogo(24, 3, "B", null, true),
        ]);

        var tipos = await _servicio.ListarAsync(Catalogos.TipoComprobante, CancellationToken.None);

        tipos.Should().Equal(new ItemCatalogoDto("F", "FACTURA"), new ItemCatalogoDto("B", "B"));
    }

    [Fact]
    public async Task ListarBancos_OrdenadosPorDescripcion()
    {
        _catalogos.Setup(c => c.ListarBancosActivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new Banco("009", "SCOTIABANK", true),
            new Banco("002", "BCP", true),
        ]);

        var bancos = await _servicio.ListarBancosAsync(CancellationToken.None);

        bancos.Select(b => b.Descripcion).Should().Equal("BCP", "SCOTIABANK");
    }
}

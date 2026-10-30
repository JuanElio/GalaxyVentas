using FluentAssertions;
using Ventas.Application.Ventas;

namespace Ventas.UnitTests.Application;

public class VentaValidadoresTests
{
    private readonly RegistrarVentaValidator _registrar = new();
    private readonly CotizarVentaValidator _cotizar = new();

    private static RegistrarVentaDto Venta(PagoVentaDto? pago = null, params LineaVentaDto[] lineas) =>
        new(125, "B", lineas.Length > 0 ? lineas : [new LineaVentaDto("ART001", 2)], pago ?? new PagoVentaDto(1, null, null));

    [Fact]
    public void VentaEnEfectivo_EsValida() =>
        _registrar.Validate(Venta()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void MedioConBanco_ExigeBancoYOperacion(int medio)
    {
        var errores = _registrar.Validate(Venta(new PagoVentaDto(medio, null, " "))).Errors;

        errores.Select(e => e.PropertyName).Should().Contain(["Pago.CodigoBanco", "Pago.NumeroOperacion"]);
        _registrar.Validate(Venta(new PagoVentaDto(medio, "002", "OP-1"))).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(7)]
    public void MedioNoAdmitido_EsInvalido(int medio) =>
        _registrar.Validate(Venta(new PagoVentaDto(medio, null, null))).Errors
            .Should().Contain(e => e.PropertyName == "Pago.MedioPago");

    [Fact]
    public void SinClienteTipoNiLineas_EsInvalida()
    {
        var errores = _registrar.Validate(new RegistrarVentaDto(0, "X", [], new PagoVentaDto(1, null, null))).Errors;

        errores.Select(e => e.PropertyName).Should().Contain(["CodigoCliente", "TipoComprobante", "Lineas"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.5)]
    [InlineData(1_000_000)]
    public void CantidadNoEnteraNoPositivaOExcesiva_EsInvalida(decimal cantidad) =>
        _cotizar.Validate(new CotizarVentaDto([new LineaVentaDto("ART001", cantidad)])).IsValid.Should().BeFalse();

    [Fact]
    public void ArticuloRepetido_EsInvalido()
    {
        var dto = new CotizarVentaDto([new LineaVentaDto("ART001", 1), new LineaVentaDto(" art001", 2)]);

        _cotizar.Validate(dto).Errors.Should().Contain(e => e.ErrorMessage.Contains("repetirse"));
        _registrar.Validate(Venta(null, dto.Lineas.ToArray())).Errors.Should().Contain(e => e.ErrorMessage.Contains("repetirse"));
    }

    [Fact]
    public void CotizacionSinLineas_EsValida() =>
        _cotizar.Validate(new CotizarVentaDto([])).IsValid.Should().BeTrue();
}

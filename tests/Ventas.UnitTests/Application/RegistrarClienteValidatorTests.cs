using FluentAssertions;
using Ventas.Application.Maestros;

namespace Ventas.UnitTests.Application;

public class RegistrarClienteValidatorTests
{
    private readonly RegistrarClienteValidator _validador = new();

    private static RegistrarClienteDto Dni(string numero = "12345678") =>
        new("2", numero, "Juan", "Pérez", null, null, null, null);

    private static RegistrarClienteDto Ruc(string numero = "20123456789") =>
        new("1", numero, null, null, "EMPRESA SAC", "AV. LIMA 123", null, null);

    [Fact]
    public void DniYRucCompletos_SonValidos()
    {
        _validador.Validate(Dni()).IsValid.Should().BeTrue();
        _validador.Validate(Ruc()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("1234567A")]
    public void Dni_SinOchoDigitos_EsInvalido(string numero) =>
        _validador.Validate(Dni(numero)).Errors.Should().Contain(e => e.PropertyName == "NumeroDocumento");

    [Theory]
    [InlineData("2012345678")]
    [InlineData("201234567890")]
    public void Ruc_SinOnceDigitos_EsInvalido(string numero) =>
        _validador.Validate(Ruc(numero)).Errors.Should().Contain(e => e.PropertyName == "NumeroDocumento");

    [Fact]
    public void TipoDeDocumentoNoAdmitido_EsInvalido() =>
        _validador.Validate(Dni() with { TipoDocumento = "4" }).Errors.Should().Contain(e => e.PropertyName == "TipoDocumento");

    [Fact]
    public void PersonaNaturalSinNombresNiApellidos_EsInvalida()
    {
        var errores = _validador.Validate(Dni() with { Nombres = " ", Apellidos = null }).Errors;

        errores.Select(e => e.PropertyName).Should().Contain(["Nombres", "Apellidos"]);
    }

    [Fact]
    public void Ruc20SinRazonSocialNiDireccion_EsInvalido()
    {
        var errores = _validador.Validate(Ruc() with { RazonSocial = null, DireccionFacturacion = "" }).Errors;

        errores.Select(e => e.PropertyName).Should().Contain(["RazonSocial", "DireccionFacturacion"]);
    }

    [Fact]
    public void Ruc10_PideNombresYApellidosEnLugarDeRazonSocial()
    {
        var datos = new RegistrarClienteDto("1", "10456789012", "Ana", "Torres", null, "JR. CUSCO 45", null, null);

        _validador.Validate(datos).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CorreoConFormatoInvalido_EsInvalido() =>
        _validador.Validate(Dni() with { Email = "no-es-correo" }).Errors.Should().Contain(e => e.PropertyName == "Email");
}

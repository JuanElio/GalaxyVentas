using FluentAssertions;
using Ventas.Application.Seguridad;

namespace Ventas.UnitTests.Application;

public class CambiarClaveValidatorTests
{
    private readonly CambiarClaveValidator _validador = new();

    private static CambiarClaveDto Datos(string nueva, string? confirmacion = null, string actual = "Ventas2026") =>
        new(actual, nueva, confirmacion ?? nueva);

    [Theory]
    [InlineData("Nueva1")]
    [InlineData("Nueva2027")]
    [InlineData("A1234567890bcdef")]
    public void ClaveValida_SinErrores(string nueva) =>
        _validador.Validate(Datos(nueva)).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("", "Ingrese la nueva contraseña.")]
    [InlineData("Ab123", "La nueva contraseña debe tener entre 6 y 16 caracteres.")]
    [InlineData("A1234567890bcdefg", "La nueva contraseña debe tener entre 6 y 16 caracteres.")]
    [InlineData("Nueva 2027", "La nueva contraseña no puede tener espacios.")]
    [InlineData("SoloLetras", "La nueva contraseña debe tener al menos una letra y un número.")]
    [InlineData("12345678", "La nueva contraseña debe tener al menos una letra y un número.")]
    [InlineData("Ventas2026", "La nueva contraseña debe ser distinta de la actual.")]
    public void ClaveNuevaInvalida_UnSoloMensajeEnElCampo(string nueva, string mensaje) =>
        _validador.Validate(Datos(nueva)).Errors.Where(e => e.PropertyName == nameof(CambiarClaveDto.ClaveNueva))
            .Should().ContainSingle().Which.ErrorMessage.Should().Be(mensaje);

    [Fact]
    public void ConfirmacionDistinta_EsInvalida() =>
        _validador.Validate(Datos("Nueva2027", "Nueva2028")).Errors
            .Should().ContainSingle(e => e.PropertyName == nameof(CambiarClaveDto.ConfirmacionClave));

    [Fact]
    public void SinClaveActual_EsInvalida() =>
        _validador.Validate(Datos("Nueva2027", actual: "")).Errors
            .Should().ContainSingle(e => e.PropertyName == nameof(CambiarClaveDto.ClaveActual));
}

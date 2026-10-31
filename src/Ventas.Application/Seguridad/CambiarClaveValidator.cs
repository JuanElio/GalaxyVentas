using FluentValidation;

namespace Ventas.Application.Seguridad;

public sealed class CambiarClaveValidator : AbstractValidator<CambiarClaveDto>
{
    // SGP_USUARIO.vchPassword es varchar(16)
    public const int LongitudMinima = 6;
    public const int LongitudMaxima = 16;

    public CambiarClaveValidator()
    {
        RuleFor(d => d.ClaveActual)
            .NotEmpty().WithMessage("Ingrese su contraseña actual.");

        // Se detiene en el primer error para mostrar un solo mensaje por campo
        RuleFor(d => d.ClaveNueva)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ingrese la nueva contraseña.")
            .Length(LongitudMinima, LongitudMaxima)
                .WithMessage($"La nueva contraseña debe tener entre {LongitudMinima} y {LongitudMaxima} caracteres.")
            .Must(c => !c.Any(char.IsWhiteSpace)).WithMessage("La nueva contraseña no puede tener espacios.")
            .Must(c => c.Any(char.IsLetter) && c.Any(char.IsDigit))
                .WithMessage("La nueva contraseña debe tener al menos una letra y un número.")
            .NotEqual(d => d.ClaveActual).WithMessage("La nueva contraseña debe ser distinta de la actual.");

        RuleFor(d => d.ConfirmacionClave)
            .Equal(d => d.ClaveNueva).WithMessage("La confirmación no coincide con la nueva contraseña.");
    }
}

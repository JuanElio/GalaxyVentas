using FluentValidation;

namespace Ventas.Application.Seguridad;

public sealed class IniciarSesionValidator : AbstractValidator<IniciarSesionDto>
{
    // Longitud de SGP_USUARIO.vchLogin y vchPassword
    private const int LongitudMaxima = 16;

    public IniciarSesionValidator()
    {
        RuleFor(d => d.Login)
            .NotEmpty().WithMessage("Ingrese el usuario.")
            .MaximumLength(LongitudMaxima).WithMessage($"El usuario tiene como máximo {LongitudMaxima} caracteres.");

        RuleFor(d => d.Clave)
            .NotEmpty().WithMessage("Ingrese la contraseña.")
            .MaximumLength(LongitudMaxima).WithMessage($"La contraseña tiene como máximo {LongitudMaxima} caracteres.");
    }
}

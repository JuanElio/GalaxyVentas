using FluentValidation;
using Ventas.Domain.Maestros;

namespace Ventas.Application.Maestros;

public sealed class RegistrarClienteValidator : AbstractValidator<RegistrarClienteDto>
{
    public RegistrarClienteValidator()
    {
        RuleFor(d => d.TipoDocumento)
            .Must(t => t is TiposDocumento.Dni or TiposDocumento.Ruc)
            .WithMessage("Seleccione DNI o RUC como tipo de documento.");

        RuleFor(d => d.NumeroDocumento)
            .NotEmpty().WithMessage("Ingrese el número de documento.")
            .Must(n => n.Trim().All(char.IsAsciiDigit)).WithMessage("El número de documento solo admite dígitos.");

        RuleFor(d => d.NumeroDocumento)
            .Must(n => n.Trim().Length == TiposDocumento.LongitudDni)
            .When(d => d.TipoDocumento == TiposDocumento.Dni && !string.IsNullOrWhiteSpace(d.NumeroDocumento))
            .WithMessage($"El DNI tiene {TiposDocumento.LongitudDni} dígitos.");

        RuleFor(d => d.NumeroDocumento)
            .Must(n => n.Trim().Length == TiposDocumento.LongitudRuc)
            .When(d => d.TipoDocumento == TiposDocumento.Ruc && !string.IsNullOrWhiteSpace(d.NumeroDocumento))
            .WithMessage($"El RUC tiene {TiposDocumento.LongitudRuc} dígitos.");

        When(EsPersonaJuridica, () =>
            RuleFor(d => d.RazonSocial).NotEmpty().WithMessage("Ingrese la razón social."));

        When(d => !EsPersonaJuridica(d), () =>
        {
            RuleFor(d => d.Nombres).NotEmpty().WithMessage("Ingrese los nombres.");
            RuleFor(d => d.Apellidos).NotEmpty().WithMessage("Ingrese los apellidos.");
        });

        RuleFor(d => d.DireccionFacturacion)
            .NotEmpty().When(d => d.TipoDocumento == TiposDocumento.Ruc)
            .WithMessage("Ingrese la dirección de facturación del cliente con RUC.");

        // Longitudes de las columnas de SGP_CLIENTE
        RuleFor(d => d.Nombres).MaximumLength(128).WithMessage("Los nombres tienen como máximo 128 caracteres.");
        RuleFor(d => d.Apellidos).MaximumLength(128).WithMessage("Los apellidos tienen como máximo 128 caracteres.");
        RuleFor(d => d.RazonSocial).MaximumLength(128).WithMessage("La razón social tiene como máximo 128 caracteres.");
        RuleFor(d => d.DireccionFacturacion).MaximumLength(128).WithMessage("La dirección tiene como máximo 128 caracteres.");
        RuleFor(d => d.Telefono).MaximumLength(15).WithMessage("El teléfono tiene como máximo 15 caracteres.");
        RuleFor(d => d.Email)
            .MaximumLength(128).WithMessage("El correo tiene como máximo 128 caracteres.")
            .EmailAddress().When(d => !string.IsNullOrWhiteSpace(d.Email)).WithMessage("El correo no tiene un formato válido.");
    }

    private static bool EsPersonaJuridica(RegistrarClienteDto d) =>
        !string.IsNullOrWhiteSpace(d.NumeroDocumento)
        && TiposDocumento.DeducirTipoPersona(d.TipoDocumento, d.NumeroDocumento.Trim()) == TiposDocumento.PersonaJuridica;
}

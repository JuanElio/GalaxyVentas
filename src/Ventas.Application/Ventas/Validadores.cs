using FluentValidation;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Pagos;

namespace Ventas.Application.Ventas;

public sealed class LineaVentaValidator : AbstractValidator<LineaVentaDto>
{
    public const decimal CantidadMaxima = 999_999;

    public LineaVentaValidator()
    {
        RuleFor(l => l.CodigoArticulo)
            .NotEmpty().WithMessage("Indique el artículo de la línea.")
            .MaximumLength(16).WithMessage("El código de artículo tiene como máximo 16 caracteres.");

        // El procedimiento de referencia trata la cantidad como entero (RN-08)
        RuleFor(l => l.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor que 0.")
            .Must(c => c == decimal.Truncate(c)).WithMessage("La cantidad debe ser un número entero.")
            .LessThanOrEqualTo(CantidadMaxima).WithMessage($"La cantidad no puede superar {CantidadMaxima:N0}.");
    }
}

public sealed class CotizarVentaValidator : AbstractValidator<CotizarVentaDto>
{
    public CotizarVentaValidator()
    {
        RuleFor(d => d.Lineas).NotNull().WithMessage("Envíe las líneas de la venta.");
        RuleForEach(d => d.Lineas).SetValidator(new LineaVentaValidator());
        RuleFor(d => d.Lineas).Must(SinArticulosRepetidos).When(d => d.Lineas is not null)
            .WithMessage("Un artículo no puede repetirse en dos líneas de la venta.");
    }

    internal static bool SinArticulosRepetidos(IReadOnlyList<LineaVentaDto> lineas) =>
        lineas
            .Where(l => !string.IsNullOrWhiteSpace(l.CodigoArticulo))
            .GroupBy(l => l.CodigoArticulo.Trim(), StringComparer.OrdinalIgnoreCase)
            .All(g => g.Count() == 1);
}

public sealed class RegistrarVentaValidator : AbstractValidator<RegistrarVentaDto>
{
    public RegistrarVentaValidator()
    {
        RuleFor(d => d.CodigoCliente).GreaterThan(0).WithMessage("Seleccione el cliente.");

        RuleFor(d => d.TipoComprobante)
            .Must(TiposComprobanteValidos.EsValido).WithMessage("Seleccione factura o boleta.");

        RuleFor(d => d.Lineas)
            .NotEmpty().WithMessage("Agregue al menos un artículo.")
            .Must(CotizarVentaValidator.SinArticulosRepetidos).When(d => d.Lineas is not null)
            .WithMessage("Un artículo no puede repetirse en dos líneas de la venta.");
        RuleForEach(d => d.Lineas).SetValidator(new LineaVentaValidator());

        RuleFor(d => d.Pago).NotNull().WithMessage("Indique el medio de pago.");
        When(d => d.Pago is not null, () =>
        {
            RuleFor(d => d.Pago.MedioPago)
                .Must(m => Enum.IsDefined((TipoMedioPago)m)).WithMessage("Seleccione un medio de pago válido.");

            When(d => Enum.IsDefined((TipoMedioPago)d.Pago.MedioPago) && ((TipoMedioPago)d.Pago.MedioPago).RequiereBancoYOperacion(), () =>
            {
                RuleFor(d => d.Pago.CodigoBanco).NotEmpty().WithMessage("Seleccione el banco.");
                RuleFor(d => d.Pago.NumeroOperacion).NotEmpty().WithMessage("Ingrese el número de operación.");
            });

            // Longitudes de SGP_MEDIO_PAGO_BS
            RuleFor(d => d.Pago.CodigoBanco).MaximumLength(12).WithMessage("El código de banco tiene como máximo 12 caracteres.");
            RuleFor(d => d.Pago.NumeroOperacion).MaximumLength(40).WithMessage("El número de operación tiene como máximo 40 caracteres.");
        });
    }
}

internal static class TiposComprobanteValidos
{
    public static bool EsValido(string? tipo) =>
        tipo is not null && (tipo == TiposComprobante.LetraFactura.ToString() || tipo == TiposComprobante.LetraBoleta.ToString());
}

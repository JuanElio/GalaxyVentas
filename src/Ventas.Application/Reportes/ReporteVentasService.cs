using FluentValidation;
using Ventas.Application.Comprobantes;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Domain.Pagos;

namespace Ventas.Application.Reportes;

public sealed record FiltroReporteVentasDto(
    DateTime? Desde,
    DateTime? Hasta,
    string? TipoComprobante = null,
    int? MedioPago = null,
    string? Usuario = null);

public sealed record FilaReporteVentasDto(
    long CodigoComprobante,
    long? CodigoTransaccion,
    DateTime FechaEmision,
    string TipoComprobante,
    string Comprobante,
    string Documento,
    string Cliente,
    decimal SubTotal,
    decimal MontoIgv,
    decimal MontoTotal,
    string MedioPago,
    string Estado,
    string Usuario);

public sealed record TotalMedioPagoDto(string MedioPago, int Pagos, decimal Monto);

public sealed record ReporteVentasDto(
    DateTime Desde,
    DateTime Hasta,
    IReadOnlyList<FilaReporteVentasDto> Filas,
    decimal SubTotal,
    decimal MontoIgv,
    decimal MontoTotal,
    IReadOnlyList<TotalMedioPagoDto> PorMedioPago)
{
    public int CantidadComprobantes => Filas.Count;
}

public sealed class FiltroReporteVentasValidator : AbstractValidator<FiltroReporteVentasDto>
{
    /// <summary>Rango máximo del reporte en días, ambos extremos incluidos (P-19).</summary>
    public const int RangoMaximoDias = 31;

    public FiltroReporteVentasValidator()
    {
        RuleFor(f => f.Desde).NotNull().WithMessage("Indique la fecha inicial.");
        RuleFor(f => f.Hasta).NotNull().WithMessage("Indique la fecha final.");

        When(f => f.Desde is not null && f.Hasta is not null, () =>
        {
            RuleFor(f => f.Desde).LessThanOrEqualTo(f => f.Hasta)
                .WithMessage("La fecha inicial no puede ser mayor que la final.");
            RuleFor(f => f.Hasta)
                .Must((f, hasta) => (hasta!.Value.Date - f.Desde!.Value.Date).Days + 1 <= RangoMaximoDias)
                .When(f => f.Desde <= f.Hasta)
                .WithMessage($"El rango de fechas no puede superar {RangoMaximoDias} días.");
        });

        RuleFor(f => f.TipoComprobante)
            .Must(t => t is "F" or "B").When(f => !string.IsNullOrWhiteSpace(f.TipoComprobante))
            .WithMessage("Seleccione factura o boleta.");
        RuleFor(f => f.MedioPago)
            .Must(m => Enum.IsDefined((TipoMedioPago)m!.Value)).When(f => f.MedioPago is not null)
            .WithMessage("Seleccione un medio de pago válido.");
    }
}

/// <summary>
/// Reporte de las ventas emitidas en el punto de venta de la sesión (RF-22, RN-19). Los totales son
/// la suma exacta de las filas (4 decimales); la vista los muestra con 2 (P-11).
/// </summary>
public sealed class ReporteVentasService(
    IConsultaVentasRepository consultas,
    ICatalogoRepository catalogos,
    IUsuarioRepository usuarios,
    IUsuarioActual usuarioActual,
    IValidator<FiltroReporteVentasDto> validador)
{
    /// <summary>Usuarios activos del punto de venta de la sesión, para el filtro del reporte.</summary>
    public async Task<IReadOnlyList<ItemCatalogoDto>> ListarUsuariosAsync(CancellationToken ct)
    {
        var lista = await usuarios.ListarActivosPorPuntoVentaAsync(usuarioActual.CodigoPuntoVenta, ct);
        return lista
            .Select(u => new ItemCatalogoDto(u.Login.Trim(), $"{u.Nombre?.Trim() ?? u.Login.Trim()} ({u.Login.Trim()})"))
            .ToList();
    }

    public async Task<ReporteVentasDto> GenerarAsync(FiltroReporteVentasDto filtro, CancellationToken ct)
    {
        await validador.ValidateAndThrowAsync(filtro, ct);

        var desde = filtro.Desde!.Value.Date;
        var hasta = filtro.Hasta!.Value.Date;
        var filas = await consultas.ListarAsync(new FiltroVentasRegistradas(
            usuarioActual.CodigoPuntoVenta,
            desde,
            hasta,
            string.IsNullOrWhiteSpace(filtro.TipoComprobante) ? null : filtro.TipoComprobante.Trim(),
            filtro.MedioPago is { } medio ? (TipoMedioPago)medio : null,
            string.IsNullOrWhiteSpace(filtro.Usuario) ? null : filtro.Usuario.Trim()), ct);

        var descripciones = await Descripciones.CargarAsync(catalogos, ct);
        var lineas = filas
            .Select(f => new FilaReporteVentasDto(
                f.CodigoComprobante,
                f.CodigoTransaccion,
                f.FechaEmision,
                descripciones.TipoComprobante(f.TipoComprobante),
                $"{f.Serie}-{f.Numero}",
                $"{descripciones.TipoDocumento(f.TipoDocumento)} {f.NumeroDocumento}".Trim(),
                f.NombreCliente,
                f.SubTotal,
                f.MontoIgv,
                f.MontoTotal,
                descripciones.MedioPago(f.MedioPago),
                Descripciones.Estado(f.Estado),
                f.Usuario))
            .ToList();

        var porMedio = lineas
            .GroupBy(l => l.MedioPago)
            .Select(g => new TotalMedioPagoDto(g.Key, g.Count(), g.Sum(l => l.MontoTotal)))
            .OrderByDescending(t => t.Monto)
            .ToList();

        return new ReporteVentasDto(desde, hasta, lineas,
            lineas.Sum(l => l.SubTotal), lineas.Sum(l => l.MontoIgv), lineas.Sum(l => l.MontoTotal), porMedio);
    }
}

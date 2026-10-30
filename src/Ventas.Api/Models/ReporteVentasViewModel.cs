using Ventas.Application.Maestros;
using Ventas.Application.Reportes;

namespace Ventas.Api.Models;

public sealed class ReporteVentasViewModel
{
    public required FiltroReporteVentasDto Filtro { get; init; }

    public ReporteVentasDto? Reporte { get; init; }

    public IReadOnlyList<string> Errores { get; init; } = [];

    public IReadOnlyList<ItemCatalogoDto> TiposComprobante { get; init; } = [];

    public IReadOnlyList<ItemCatalogoDto> MediosPago { get; init; } = [];

    public IReadOnlyList<ItemCatalogoDto> Usuarios { get; init; } = [];

    public required string PuntoVenta { get; init; }
}

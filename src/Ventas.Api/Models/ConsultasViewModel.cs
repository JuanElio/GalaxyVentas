using Ventas.Application.Comprobantes;

namespace Ventas.Api.Models;

public sealed class ConsultasViewModel
{
    public long? Transaccion { get; init; }

    public string? TipoComprobante { get; init; }

    public string? Numero { get; init; }

    /// <summary>Pestaña activa: "venta" o "comprobante".</summary>
    public string Pestana { get; init; } = "venta";

    public string? Error { get; init; }

    public IReadOnlyList<VentaResumenDto> Recientes { get; init; } = [];
}

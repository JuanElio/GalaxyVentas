using Ventas.Domain.Comprobantes;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;

namespace Ventas.Application.Comprobantes;

/// <summary>Venta leída de la BD junto con el usuario que la registró (vchCreador).</summary>
public sealed record VentaRegistrada(Venta Venta, string Usuario);

/// <summary>Fila plana de una venta emitida, para listados y el reporte.</summary>
public sealed record FilaVentaRegistrada(
    long CodigoComprobante,
    long? CodigoTransaccion,
    DateTime FechaEmision,
    string TipoComprobante,
    string Serie,
    string Numero,
    string? TipoDocumento,
    string? NumeroDocumento,
    string NombreCliente,
    decimal SubTotal,
    decimal MontoIgv,
    decimal MontoTotal,
    TipoMedioPago? MedioPago,
    EstadoComprobante Estado,
    string Usuario);

/// <summary>Filtros de las ventas emitidas de un punto de venta; las fechas son de emisión (días completos).</summary>
public sealed record FiltroVentasRegistradas(
    string CodigoPuntoVenta,
    DateTime? Desde = null,
    DateTime? Hasta = null,
    string? TipoComprobante = null,
    TipoMedioPago? MedioPago = null,
    string? Usuario = null,
    int? Maximo = null);

/// <summary>Lecturas de ventas y comprobantes ya registrados (RF-16, RF-17, RF-22).</summary>
public interface IConsultaVentasRepository
{
    /// <summary>Comprobante activo con su detalle, sus enlaces a la venta y sus pagos.</summary>
    Task<Comprobante?> ObtenerComprobanteAsync(long codigo, CancellationToken ct);

    Task<long?> BuscarCodigoComprobanteAsync(string serie, string numero, CancellationToken ct);

    Task<VentaRegistrada?> ObtenerVentaAsync(long codigoTransaccion, CancellationToken ct);

    /// <summary>Ventas emitidas que cumplen el filtro, de la más antigua a la más reciente (o las últimas si hay máximo).</summary>
    Task<IReadOnlyList<FilaVentaRegistrada>> ListarAsync(FiltroVentasRegistradas filtro, CancellationToken ct);
}

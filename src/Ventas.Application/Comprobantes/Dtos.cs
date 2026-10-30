namespace Ventas.Application.Comprobantes;

/// <summary>Comprobante con su venta, para la vista del comprobante (RF-16, RF-17).</summary>
public sealed record ComprobanteDetalleDto(
    long CodigoComprobante,
    long CodigoTransaccion,
    string TipoComprobante,
    string Serie,
    string Numero,
    DateTime FechaEmision,
    string Estado,
    string FormaPago,
    string Moneda,
    string CodigoPuntoVenta,
    string PuntoVenta,
    string? ZonaVenta,
    string NombreCliente,
    string Documento,
    string? DireccionCliente,
    decimal SubTotal,
    decimal MontoDescuento,
    decimal MontoIgv,
    decimal MontoTotal,
    decimal MontoPagado,
    IReadOnlyList<DetalleComprobanteDto> Detalle,
    IReadOnlyList<PagoComprobanteDto> Pagos,
    string Enlaces,
    string Canal,
    string Almacen,
    string TipoOrden,
    string Usuario,
    DateTime FechaVenta)
{
    public string NumeroCompleto => $"{Serie}-{Numero}";
}

public sealed record DetalleComprobanteDto(
    int Item,
    string CodigoArticulo,
    string Descripcion,
    string? UnidadMedida,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal ValorVenta,
    decimal Igv,
    decimal SubTotal);

public sealed record PagoComprobanteDto(string MedioPago, string? Banco, string? NumeroOperacion, decimal Monto);

/// <summary>Venta reciente del punto de venta, para el listado de consultas.</summary>
public sealed record VentaResumenDto(
    long CodigoComprobante,
    long? CodigoTransaccion,
    DateTime FechaEmision,
    string Comprobante,
    string Cliente,
    decimal MontoTotal);

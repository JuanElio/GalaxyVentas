namespace Ventas.Application.Ventas;

public sealed record LineaVentaDto(string CodigoArticulo, decimal Cantidad);

/// <summary>Un solo medio de pago por el total: el monto no se envía (RN-16).</summary>
public sealed record PagoVentaDto(int MedioPago, string? CodigoBanco, string? NumeroOperacion);

/// <summary>
/// Datos que envía la pantalla para registrar la venta. Usuario, punto de venta, canal, moneda y
/// tipo de orden no se envían: se toman de la sesión y de <see cref="ParametrosVenta"/> (RN-19).
/// </summary>
public sealed record RegistrarVentaDto(
    long CodigoCliente,
    string TipoComprobante,
    IReadOnlyList<LineaVentaDto> Lineas,
    PagoVentaDto Pago);

public sealed record CotizarVentaDto(IReadOnlyList<LineaVentaDto> Lineas);

/// <summary>Importes de una línea calculados por el dominio; <see cref="Observacion"/> explica por qué no se puede vender.</summary>
public sealed record LineaCotizadaDto(
    int Linea,
    string CodigoArticulo,
    string Descripcion,
    string? UnidadMedida,
    decimal Cantidad,
    decimal? Tarifa,
    decimal ValorVenta,
    decimal Igv,
    decimal SubTotal,
    decimal Stock,
    string? Observacion);

public sealed record CotizacionDto(
    IReadOnlyList<LineaCotizadaDto> Lineas,
    decimal MontoBruto,
    decimal SubTotal,
    decimal MontoIgv,
    decimal MontoTotal,
    bool Valida);

/// <summary>Datos fijos de la venta que la pantalla muestra al operador.</summary>
public sealed record ContextoVentaDto(
    string CodigoPuntoVenta,
    string PuntoVenta,
    string Canal,
    string Moneda,
    string TipoOrden,
    string? Almacen,
    string Usuario);

/// <summary>Serie y próximo número del comprobante; <see cref="Numero"/> es null si la serie no tiene impresora.</summary>
public sealed record NumeracionDto(string Serie, string? Numero);

/// <summary>Resultado de registrar la venta: transacción, comprobante emitido y total cobrado.</summary>
public sealed record VentaRegistradaDto(
    long CodigoTransaccion,
    long CodigoComprobante,
    string Serie,
    string Numero,
    DateTime FechaEmision,
    decimal MontoTotal);

using Ventas.Domain.Comun;

namespace Ventas.Domain.Inventario;

/// <summary>
/// Movimiento de stock (PGA_TRANSACCION_KARDEX). El stock de un artículo en un punto de venta
/// es la suma de las cantidades de sus movimientos (RN-09).
/// </summary>
public class MovimientoKardex
{
    public const string TipoVenta = "VENTA";

    private bool? _activo;

    private MovimientoKardex()
    {
    }

    public int Id { get; private set; }

    public string? CodigoArticulo { get; private set; }

    public string? CodigoCanal { get; private set; }

    public string? CodigoPuntoVenta { get; private set; }

    public string? TipoTransaccion { get; private set; }

    public DateTime? FechaTransaccion { get; private set; }

    /// <summary>Positiva para un ingreso, negativa para una salida.</summary>
    public decimal? Cantidad { get; private set; }

    public long? CodigoTransaccion { get; private set; }

    public string? Descripcion { get; private set; }

    public string? Estado { get; private set; }

    public string? Proceso { get; private set; }

    public string? GuidRegistro { get; private set; }

    public bool Activo => _activo == true;

    /// <summary>Salida por la venta de una línea, con los valores del procedimiento de referencia (RF-09).</summary>
    public static MovimientoKardex SalidaPorVenta(string codigoArticulo, string codigoCanal, string codigoPuntoVenta,
        DateTime fechaVenta, decimal cantidad, long codigoTransaccion, string serie, string numero)
    {
        if (cantidad <= 0)
            throw new DominioException($"La cantidad vendida del artículo {codigoArticulo} debe ser mayor que 0.");

        return new MovimientoKardex
        {
            CodigoArticulo = codigoArticulo.Trim(),
            CodigoCanal = codigoCanal.Trim(),
            CodigoPuntoVenta = codigoPuntoVenta.Trim(),
            TipoTransaccion = TipoVenta,
            FechaTransaccion = fechaVenta.Date,
            Cantidad = -cantidad,
            CodigoTransaccion = codigoTransaccion,
            Descripcion = $"Salida por venta {serie}-{numero}",
            Estado = string.Empty,
            Proceso = string.Empty,
            GuidRegistro = Guid.NewGuid().ToString().ToUpperInvariant(),
            _activo = true,
        };
    }
}

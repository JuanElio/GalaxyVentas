namespace Ventas.Domain.Comprobantes;

/// <summary>
/// Enlace entre un ítem del comprobante y la línea de la venta que lo origina.
/// </summary>
public class MovimientoComprobante
{
    private MovimientoComprobante()
    {
    }

    internal MovimientoComprobante(int item, long codigoTransaccion, int lineaTransaccion)
    {
        Item = item;
        CodigoTransaccion = codigoTransaccion;
        LineaTransaccion = lineaTransaccion;
    }

    public long CodigoComprobante { get; private set; }

    public int Item { get; private set; }

    public long CodigoTransaccion { get; private set; }

    public int LineaTransaccion { get; private set; }
}

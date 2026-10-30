using Ventas.Domain.Ventas;

namespace Ventas.Domain.Comprobantes;

/// <summary>
/// Ítem del comprobante. Se crea a partir de una línea de la venta al emitir el comprobante.
/// </summary>
public class DetalleComprobante
{
    private DetalleComprobante()
    {
        CodigoArticulo = NombreArticulo = null!;
    }

    internal DetalleComprobante(DetalleVenta linea, string nombreArticulo)
    {
        Item = linea.NumeroLinea;
        CodigoArticulo = linea.CodigoArticulo;
        NombreArticulo = nombreArticulo;
        UnidadMedida = linea.UnidadMedida;
        Cantidad = linea.Cantidad;
        PrecioUnitario = linea.Tarifa;
        SubTotal = linea.SubTotal;
        Igv = linea.Igv;
        MontoDescuento = 0;
        MontoRecargo = 0;
        MontoBruto = SubTotal + MontoRecargo - MontoDescuento;
    }

    public long CodigoComprobante { get; private set; }

    public int Item { get; private set; }

    public string CodigoArticulo { get; private set; }

    public string NombreArticulo { get; private set; }

    public string? UnidadMedida { get; private set; }

    public decimal Cantidad { get; private set; }

    public decimal PrecioUnitario { get; private set; }

    public decimal MontoBruto { get; private set; }

    public decimal MontoDescuento { get; private set; }

    public decimal MontoRecargo { get; private set; }

    public decimal SubTotal { get; private set; }

    public decimal Igv { get; private set; }
}

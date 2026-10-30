using Ventas.Domain.Comun;

namespace Ventas.Domain.Ventas;

/// <summary>
/// Línea de una venta. Solo se crea a través de <see cref="Venta.AgregarLinea"/>.
/// </summary>
public class DetalleVenta
{
    private DetalleVenta()
    {
        CodigoArticulo = null!;
    }

    internal DetalleVenta(int numeroLinea, string codigoArticulo, string? unidadMedida, decimal cantidad, decimal tarifa)
    {
        if (cantidad <= 0)
            throw new DominioException($"La cantidad del artículo {codigoArticulo} debe ser mayor que 0.");
        if (tarifa < 0)
            throw new DominioException($"La tarifa del artículo {codigoArticulo} no puede ser negativa.");

        NumeroLinea = numeroLinea;
        CodigoArticulo = codigoArticulo;
        UnidadMedida = unidadMedida;
        Cantidad = cantidad;
        Tarifa = tarifa;

        // La tarifa incluye el IGV (P-15): el subtotal de la línea es el importe con IGV (RN-04)
        var importes = Importes.CalcularLinea(cantidad, tarifa);
        MontoBruto = importes.MontoBruto;
        SubTotal = importes.SubTotal;
        Igv = importes.Igv;
    }

    public long CodigoTransaccion { get; private set; }

    public int NumeroLinea { get; private set; }

    public string CodigoArticulo { get; private set; }

    public string? UnidadMedida { get; private set; }

    public decimal Cantidad { get; private set; }

    /// <summary>Precio unitario con IGV de la tarifa vigente.</summary>
    public decimal Tarifa { get; private set; }

    public decimal MontoBruto { get; private set; }

    /// <summary>Importe de la línea con IGV.</summary>
    public decimal SubTotal { get; private set; }

    public decimal Igv { get; private set; }

    /// <summary>Base imponible de la línea (subtotal sin IGV).</summary>
    public decimal ValorVenta => SubTotal - Igv;
}

namespace Ventas.Domain.Comun;

/// <summary>
/// Reglas comunes de importes: desglose del IGV y redondeo a 4 decimales (RN-03, RN-05).
/// </summary>
public static class Importes
{
    /// <summary>Tasa general de IGV; cada artículo usa la de su código de IVA (SGP_DETALLE_IVA).</summary>
    public const decimal TasaIgv = 0.18m;

    public const int Decimales = 4;

    /// <summary>Redondeo comercial (0.00005 sube) a 4 decimales.</summary>
    public static decimal Redondear(decimal valor) =>
        Math.Round(valor, Decimales, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Desglosa el IGV de un importe que ya lo incluye (P-15): devuelve el valor de venta
    /// (base imponible) y el IGV, cuya suma es exactamente el importe recibido.
    /// </summary>
    public static (decimal ValorVenta, decimal Igv) DesglosarIgv(decimal importeConIgv, decimal tasaIgv = TasaIgv)
    {
        if (tasaIgv < 0)
            throw new DominioException("La tasa de IGV no puede ser negativa.");

        var valorVenta = Redondear(importeConIgv / (1 + tasaIgv));
        return (valorVenta, importeConIgv - valorVenta);
    }

    /// <summary>
    /// Importes de una línea con tarifa que incluye IGV (RF-06): bruto y subtotal = cantidad × tarifa;
    /// el IGV se desglosa del subtotal con la tasa del artículo (0 si está exonerado).
    /// </summary>
    public static ImportesLinea CalcularLinea(decimal cantidad, decimal tarifa, decimal tasaIgv = TasaIgv)
    {
        var subTotal = Redondear(cantidad * tarifa);
        var (valorVenta, igv) = DesglosarIgv(subTotal, tasaIgv);
        return new ImportesLinea(subTotal, subTotal, valorVenta, igv);
    }

    /// <summary>
    /// Totales de cabecera con la convención del procedimiento de referencia (RF-06): bruto = Σ subtotal;
    /// subtotal = Σ (subtotal − IGV); IGV = Σ IGV; total = subtotal + IGV. Sin descuento ni recargo (P-03).
    /// </summary>
    public static TotalesVenta Totalizar(IEnumerable<(decimal SubTotal, decimal Igv)> lineas)
    {
        var lista = lineas.ToList();
        var subTotal = lista.Sum(l => l.SubTotal - l.Igv);
        var igv = lista.Sum(l => l.Igv);
        return new TotalesVenta(lista.Sum(l => l.SubTotal), subTotal, igv, subTotal + igv);
    }
}

public readonly record struct ImportesLinea(decimal MontoBruto, decimal SubTotal, decimal ValorVenta, decimal Igv);

public readonly record struct TotalesVenta(decimal MontoBruto, decimal SubTotal, decimal MontoIgv, decimal MontoTotal);

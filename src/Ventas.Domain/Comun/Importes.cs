namespace Ventas.Domain.Comun;

/// <summary>
/// Reglas comunes de importes: tasa de IGV y redondeo a 4 decimales (RN-03, RN-05).
/// </summary>
public static class Importes
{
    public const decimal TasaIgv = 0.18m;

    public const int Decimales = 4;

    /// <summary>Redondeo comercial (0.00005 sube) a 4 decimales.</summary>
    public static decimal Redondear(decimal valor) =>
        Math.Round(valor, Decimales, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Desglosa el IGV de un importe que ya lo incluye (P-15): devuelve el valor de venta
    /// (base imponible) y el IGV, cuya suma es exactamente el importe recibido.
    /// </summary>
    public static (decimal ValorVenta, decimal Igv) DesglosarIgv(decimal importeConIgv)
    {
        var valorVenta = Redondear(importeConIgv / (1 + TasaIgv));
        return (valorVenta, importeConIgv - valorVenta);
    }
}

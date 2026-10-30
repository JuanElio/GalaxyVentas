using Ventas.Domain.Comun;

namespace Ventas.Domain.Comprobantes;

/// <summary>
/// Reglas que dependen del tipo de comprobante: la letra de la serie es el primer carácter
/// del código del tipo, <c>F</c> para factura y <c>B</c> para boleta (P-14).
/// </summary>
public static class TiposComprobante
{
    public const char LetraFactura = 'F';

    public const char LetraBoleta = 'B';

    /// <summary>Longitud máxima de la serie (SGP_IMPRESORA.vchNumSerie).</summary>
    public const int LongitudMaximaSerie = 4;

    public static char Letra(string codigoTipoComprobante)
    {
        var letra = string.IsNullOrWhiteSpace(codigoTipoComprobante)
            ? '\0'
            : char.ToUpperInvariant(codigoTipoComprobante.Trim()[0]);

        return letra is LetraFactura or LetraBoleta
            ? letra
            : throw new DominioException($"El tipo de comprobante '{codigoTipoComprobante}' no es factura ni boleta.");
    }

    public static bool EsFactura(string codigoTipoComprobante) => Letra(codigoTipoComprobante) == LetraFactura;

    /// <summary>Serie = letra del tipo de comprobante + código del punto de venta (RN-13).</summary>
    public static string FormarSerie(string codigoTipoComprobante, string codigoPuntoVenta)
    {
        if (string.IsNullOrWhiteSpace(codigoPuntoVenta))
            throw new DominioException("El punto de venta es obligatorio para formar la serie.");

        var serie = Letra(codigoTipoComprobante) + codigoPuntoVenta.Trim();
        return serie.Length <= LongitudMaximaSerie
            ? serie
            : throw new DominioException($"La serie {serie} supera los {LongitudMaximaSerie} caracteres permitidos.");
    }

    /// <summary>Número del comprobante con 7 dígitos y ceros a la izquierda (RF-11).</summary>
    public static string FormatearNumero(int correlativo) =>
        correlativo > 0
            ? correlativo.ToString("D7")
            : throw new DominioException("El correlativo del comprobante debe ser mayor que 0.");
}

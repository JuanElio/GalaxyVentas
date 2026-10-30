namespace Ventas.Application.Ventas;

/// <summary>Valores fijos de la venta que el operador no ingresa.</summary>
public static class ParametrosVenta
{
    /// <summary>Canal (tarifa canal) fijo de todas las ventas.</summary>
    public const string Canal = "01";

    /// <summary>Tipo de orden por defecto del procedimiento de referencia.</summary>
    public const string TipoOrden = "V01";

    /// <summary>Soles en el catálogo 16 (tipo de moneda) (RN-21).</summary>
    public const string Moneda = "SOL";

    /// <summary>Contado en el catálogo 10 (forma de pago) (P-05).</summary>
    public const string FormaPagoContado = "C00";
}

namespace Ventas.Application.Ventas;

/// <summary>Valores fijos de la venta que el operador no ingresa.</summary>
public static class ParametrosVenta
{
    /// <summary>
    /// Canal (tarifa canal) fijo de todas las ventas: el de las categorías de artículo de
    /// SGP_CATEGORIA_ARTICULO para <see cref="TipoOrden"/>.
    /// </summary>
    public const string Canal = "30";

    /// <summary>Tipo de orden de todas las ventas: V41 (Tickets de Hemeroteca).</summary>
    public const string TipoOrden = "V41";

    /// <summary>Soles en el catálogo 16 (tipo de moneda) (RN-21).</summary>
    public const string Moneda = "SOL";

    /// <summary>Contado en el catálogo 10 (forma de pago) (P-05).</summary>
    public const string FormaPagoContado = "C00";

    /// <summary>
    /// Mayor código de comprobante que cabe en SGP_CABECERA_TRANSACCION_BS.numCodComprobante,
    /// que es numeric(9,4) (P-12).
    /// </summary>
    public const long CodigoComprobanteMaximoEnCabecera = 99_999;
}

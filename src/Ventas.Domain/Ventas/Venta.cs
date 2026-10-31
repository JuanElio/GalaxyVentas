using Ventas.Domain.Comun;

namespace Ventas.Domain.Ventas;

/// <summary>
/// Raíz del agregado de la venta: sus líneas solo se crean a través de ella y los
/// importes de la cabecera son siempre la suma de sus líneas (RN-05).
/// </summary>
public class Venta
{
    private readonly List<DetalleVenta> _detalles = [];

    private Venta()
    {
        CodigoPuntoVenta = TipoOrden = TipoLiquidacion = CodigoTarifaCanal = null!;
        CodigoAlmacen = CodigoMoneda = FormaPago = TipoComprobante = null!;
    }

    public Venta(
        string codigoPuntoVenta,
        long codigoCliente,
        string tipoOrden,
        string tipoLiquidacion,
        string codigoTarifaCanal,
        string codigoAlmacen,
        string codigoMoneda,
        string formaPago,
        string tipoComprobante,
        DateTime fechaVenta)
    {
        CodigoPuntoVenta = Requerido(codigoPuntoVenta, "El punto de venta");
        CodigoCliente = codigoCliente > 0 ? codigoCliente : throw new DominioException("El cliente es obligatorio.");
        TipoOrden = Requerido(tipoOrden, "El tipo de orden");
        TipoLiquidacion = Requerido(tipoLiquidacion, "El tipo de liquidación");
        CodigoTarifaCanal = Requerido(codigoTarifaCanal, "El canal");
        CodigoAlmacen = Requerido(codigoAlmacen, "El almacén");
        CodigoMoneda = Requerido(codigoMoneda, "La moneda");
        FormaPago = Requerido(formaPago, "La forma de pago");
        TipoComprobante = Requerido(tipoComprobante, "El tipo de comprobante");
        FechaVenta = fechaVenta;
        Situacion = SituacionVenta.Registrada;
    }

    public long Id { get; private set; }

    public string CodigoPuntoVenta { get; private set; }

    public long CodigoCliente { get; private set; }

    public string TipoOrden { get; private set; }

    public string TipoLiquidacion { get; private set; }

    public string CodigoTarifaCanal { get; private set; }

    public string CodigoAlmacen { get; private set; }

    public string CodigoMoneda { get; private set; }

    /// <summary>Siempre 1: todas las ventas son en soles (RN-21).</summary>
    public decimal TipoCambio { get; private set; } = 1m;

    public string FormaPago { get; private set; }

    public string TipoComprobante { get; private set; }

    public DateTime FechaVenta { get; private set; }

    public SituacionVenta Situacion { get; private set; }

    public long? CodigoComprobante { get; private set; }

    public decimal MontoBruto { get; private set; }

    public decimal MontoDescuento { get; private set; }

    public decimal MontoRecargo { get; private set; }

    /// <summary>Base imponible: Σ (subtotal − IGV) de las líneas.</summary>
    public decimal SubTotal { get; private set; }

    public decimal MontoIgv { get; private set; }

    public decimal MontoTotal { get; private set; }

    public IReadOnlyList<DetalleVenta> Detalles => _detalles;

    public DetalleVenta AgregarLinea(string codigoArticulo, string? unidadMedida, decimal cantidad, decimal tarifa,
        decimal tasaIgv = Importes.TasaIgv)
    {
        if (Situacion != SituacionVenta.Registrada)
            throw new DominioException("No se pueden agregar líneas a una venta con comprobante emitido.");

        var codigo = Requerido(codigoArticulo, "El artículo");
        if (_detalles.Any(d => string.Equals(d.CodigoArticulo, codigo, StringComparison.OrdinalIgnoreCase)))
            throw new DominioException($"El artículo {codigo} ya está en otra línea de la venta.");

        var detalle = new DetalleVenta(_detalles.Count + 1, codigo, unidadMedida, cantidad, tarifa, tasaIgv);
        _detalles.Add(detalle);
        RecalcularTotales();
        return detalle;
    }

    /// <summary>Enlaza el comprobante emitido y pasa la venta a emitida (RF-10).</summary>
    public void MarcarEmitida(long codigoComprobante)
    {
        if (Situacion != SituacionVenta.Registrada)
            throw new DominioException("La venta ya tiene un comprobante emitido.");
        if (_detalles.Count == 0)
            throw new DominioException("La venta debe tener al menos una línea.");

        CodigoComprobante = codigoComprobante;
        Situacion = SituacionVenta.Emitida;
    }

    private void RecalcularTotales()
    {
        var totales = Importes.Totalizar(_detalles.Select(d => (d.SubTotal, d.Igv)));
        MontoBruto = totales.MontoBruto;
        SubTotal = totales.SubTotal;
        MontoIgv = totales.MontoIgv;
        MontoTotal = totales.MontoTotal;
        // Descuento y recargo fuera de alcance (P-03)
        MontoDescuento = 0;
        MontoRecargo = 0;
    }

    private static string Requerido(string? valor, string campo) =>
        string.IsNullOrWhiteSpace(valor) ? throw new DominioException($"{campo} es obligatorio.") : valor.Trim();
}

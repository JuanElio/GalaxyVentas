using Ventas.Domain.Comun;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;

namespace Ventas.Domain.Comprobantes;

/// <summary>
/// Raíz del agregado del comprobante (factura o boleta) que se emite por una venta.
/// </summary>
public class Comprobante
{
    private readonly List<DetalleComprobante> _detalles = [];
    private readonly List<MovimientoComprobante> _movimientos = [];
    private readonly List<PagoComprobante> _pagos = [];

    private Comprobante()
    {
        CodigoPuntoVenta = NombreCliente = TipoDocumento = NumeroDocumento = null!;
        FormaPago = TipoComprobante = Serie = Numero = CodigoMoneda = null!;
    }

    public long Id { get; private set; }

    public long CodigoCliente { get; private set; }

    public string CodigoPuntoVenta { get; private set; }

    public string? ZonaVenta { get; private set; }

    public string NombreCliente { get; private set; }

    public string? DireccionCliente { get; private set; }

    public string TipoDocumento { get; private set; }

    public string NumeroDocumento { get; private set; }

    public string FormaPago { get; private set; }

    public string TipoComprobante { get; private set; }

    public string Serie { get; private set; }

    public string Numero { get; private set; }

    public DateTime FechaEmision { get; private set; }

    public string CodigoMoneda { get; private set; }

    /// <summary>Siempre 1: todos los comprobantes son en soles (RN-21).</summary>
    public decimal TipoCambio { get; private set; } = 1m;

    public EstadoComprobante Estado { get; private set; }

    public bool Facturado { get; private set; }

    public bool TieneMedioPago { get; private set; }

    public decimal MontoBruto { get; private set; }

    public decimal MontoDescuento { get; private set; }

    public decimal MontoRecargo { get; private set; }

    public decimal SubTotal { get; private set; }

    public decimal MontoIgv { get; private set; }

    public decimal MontoTotal { get; private set; }

    public decimal MontoPagado { get; private set; }

    public IReadOnlyList<DetalleComprobante> Detalles => _detalles;

    public IReadOnlyList<MovimientoComprobante> Movimientos => _movimientos;

    public IReadOnlyList<PagoComprobante> Pagos => _pagos;

    /// <summary>
    /// Emite el comprobante de la venta (RF-10 a RF-12). La serie y el correlativo los obtiene
    /// el llamador de la impresora del punto de venta; los nombres de los artículos se copian
    /// al detalle, igual que los datos del cliente.
    /// </summary>
    public static Comprobante Emitir(
        Venta venta,
        ClienteComprobante cliente,
        string? zonaVenta,
        int correlativo,
        DateTime fechaEmision,
        IReadOnlyDictionary<string, string> nombresArticulos)
    {
        if (venta.Situacion != SituacionVenta.Registrada)
            throw new DominioException("La venta ya tiene un comprobante emitido.");
        if (venta.Detalles.Count == 0)
            throw new DominioException("La venta debe tener al menos una línea.");
        if (cliente.Codigo != venta.CodigoCliente)
            throw new DominioException("El cliente del comprobante no es el de la venta.");
        if (TiposComprobante.EsFactura(venta.TipoComprobante) && !cliente.TieneRuc)
            throw new DominioException("Para emitir una factura el cliente debe tener RUC.");

        var comprobante = new Comprobante
        {
            CodigoCliente = cliente.Codigo,
            CodigoPuntoVenta = venta.CodigoPuntoVenta,
            ZonaVenta = zonaVenta,
            NombreCliente = cliente.Nombre,
            DireccionCliente = cliente.Direccion,
            TipoDocumento = cliente.TipoDocumento,
            NumeroDocumento = cliente.NumeroDocumento,
            FormaPago = venta.FormaPago,
            TipoComprobante = venta.TipoComprobante,
            Serie = TiposComprobante.FormarSerie(venta.TipoComprobante, venta.CodigoPuntoVenta),
            Numero = TiposComprobante.FormatearNumero(correlativo),
            FechaEmision = fechaEmision,
            CodigoMoneda = venta.CodigoMoneda,
            Estado = EstadoComprobante.Emitido,
            Facturado = true,
        };

        foreach (var linea in venta.Detalles)
        {
            if (!nombresArticulos.TryGetValue(linea.CodigoArticulo, out var nombre))
                throw new DominioException($"Falta la descripción del artículo {linea.CodigoArticulo}.");

            comprobante._detalles.Add(new DetalleComprobante(linea, nombre));
            comprobante._movimientos.Add(new MovimientoComprobante(linea.NumeroLinea, venta.Id, linea.NumeroLinea));
        }

        comprobante.RecalcularTotales();
        return comprobante;
    }

    /// <summary>
    /// Asigna el pago por el total y deja el comprobante cancelado (RF-14, RF-15).
    /// </summary>
    public void AsignarPago(MedioPago medioPago)
    {
        if (Estado == EstadoComprobante.Cancelado || _pagos.Count > 0)
            throw new DominioException("El comprobante ya tiene un medio de pago asignado.");
        if (medioPago.Monto != MontoTotal)
            throw new DominioException("El monto del pago debe ser igual al total del comprobante.");

        _pagos.Add(new PagoComprobante(medioPago, medioPago.Monto));
        MontoPagado = medioPago.Monto;
        TieneMedioPago = true;
        Estado = EstadoComprobante.Cancelado;
    }

    private void RecalcularTotales()
    {
        // Misma convención que la cabecera de la venta (RF-06): ambos totales coinciden (RN-12)
        var totales = Importes.Totalizar(_detalles.Select(d => (d.SubTotal, d.Igv)));
        MontoBruto = totales.MontoBruto;
        SubTotal = totales.SubTotal;
        MontoIgv = totales.MontoIgv;
        MontoTotal = totales.MontoTotal;
        MontoDescuento = 0;
        MontoRecargo = 0;
    }
}

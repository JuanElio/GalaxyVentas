namespace Ventas.Domain.Pagos;

/// <summary>
/// Asignación de un medio de pago al comprobante (RF-14).
/// </summary>
public class PagoComprobante
{
    private PagoComprobante()
    {
        MedioPago = null!;
    }

    internal PagoComprobante(MedioPago medioPago, decimal monto)
    {
        MedioPago = medioPago;
        Monto = monto;
    }

    public long CodigoComprobante { get; private set; }

    public long CodigoMedioPago { get; private set; }

    public MedioPago MedioPago { get; private set; }

    public decimal Monto { get; private set; }
}

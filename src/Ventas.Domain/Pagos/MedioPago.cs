using Ventas.Domain.Comun;
using Ventas.Domain.Ventas;

namespace Ventas.Domain.Pagos;

/// <summary>
/// Pago de una venta. Es al contado y por el total: el operador no ingresa el monto (RN-16).
/// </summary>
public class MedioPago
{
    private MedioPago()
    {
        CodigoMoneda = null!;
    }

    private MedioPago(long codigoTransaccion, TipoMedioPago tipo, decimal monto, string codigoMoneda,
        string? codigoBanco, string? numeroOperacion)
    {
        CodigoTransaccion = codigoTransaccion;
        Tipo = tipo;
        Monto = monto;
        MontoOrigen = monto;
        CodigoMoneda = codigoMoneda;
        CodigoBanco = codigoBanco;
        NumeroOperacion = numeroOperacion;
    }

    public long Id { get; private set; }

    public long CodigoTransaccion { get; private set; }

    public TipoMedioPago Tipo { get; private set; }

    public decimal Monto { get; private set; }

    public decimal MontoOrigen { get; private set; }

    public string CodigoMoneda { get; private set; }

    /// <summary>Siempre 1: todos los pagos son en soles (RN-21).</summary>
    public decimal TipoCambio { get; private set; } = 1m;

    public string? CodigoBanco { get; private set; }

    public string? NumeroOperacion { get; private set; }

    /// <summary>Registra el pago de la venta por su total (RF-13, RN-16).</summary>
    public static MedioPago Registrar(Venta venta, TipoMedioPago tipo, string? codigoBanco, string? numeroOperacion)
    {
        if (!Enum.IsDefined(tipo))
            throw new DominioException($"El medio de pago '{(int)tipo}' no es válido.");
        if (venta.MontoTotal <= 0)
            throw new DominioException("El total de la venta debe ser mayor que 0 para registrar el pago.");

        if (!tipo.RequiereBancoYOperacion())
            return new MedioPago(venta.Id, tipo, venta.MontoTotal, venta.CodigoMoneda, null, null);

        if (string.IsNullOrWhiteSpace(codigoBanco))
            throw new DominioException("El banco es obligatorio para el medio de pago seleccionado.");
        if (string.IsNullOrWhiteSpace(numeroOperacion))
            throw new DominioException("El número de operación es obligatorio para el medio de pago seleccionado.");

        return new MedioPago(venta.Id, tipo, venta.MontoTotal, venta.CodigoMoneda,
            codigoBanco.Trim(), numeroOperacion.Trim());
    }
}

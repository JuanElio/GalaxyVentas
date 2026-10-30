namespace Ventas.Domain.Pagos;

/// <summary>
/// Medios de pago; el valor es el código de SGP_MEDIO_PAGO_BS.vchMedioPago (P-09).
/// </summary>
public enum TipoMedioPago
{
    Efectivo = 1,
    Cheque = 2,
    TarjetaDebito = 3,
    TarjetaCredito = 4,
    BoletaDeposito = 5,
}

public static class TipoMedioPagoExtensions
{
    /// <summary>Cheque, tarjeta y depósito exigen banco y número de operación (RF-13).</summary>
    public static bool RequiereBancoYOperacion(this TipoMedioPago tipo) => tipo != TipoMedioPago.Efectivo;
}

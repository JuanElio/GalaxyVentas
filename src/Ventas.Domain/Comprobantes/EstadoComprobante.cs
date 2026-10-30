namespace Ventas.Domain.Comprobantes;

public enum EstadoComprobante
{
    /// <summary>Emitido, todavía sin medio de pago asignado.</summary>
    Emitido,

    /// <summary>Pagado por el total (en BD, vchEstado '2').</summary>
    Cancelado,
}

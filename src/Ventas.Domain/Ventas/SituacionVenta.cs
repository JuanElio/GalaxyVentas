namespace Ventas.Domain.Ventas;

public enum SituacionVenta
{
    /// <summary>Cabecera grabada sin comprobante (en BD, chrSituacionRegistro 'I').</summary>
    Registrada,

    /// <summary>Comprobante emitido (en BD, chrSituacionRegistro 'A').</summary>
    Emitida,
}

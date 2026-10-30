namespace Ventas.Domain.Comprobantes;

/// <summary>
/// Datos del cliente tal como están al emitir el comprobante (RF-12).
/// </summary>
public sealed record ClienteComprobante(
    long Codigo,
    string Nombre,
    string? Direccion,
    string TipoDocumento,
    string NumeroDocumento,
    bool TieneRuc);

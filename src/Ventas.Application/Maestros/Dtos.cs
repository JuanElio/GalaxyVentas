using Ventas.Domain.Maestros;

namespace Ventas.Application.Maestros;

public sealed record ClienteDto(
    long Codigo,
    string TipoDocumento,
    string NumeroDocumento,
    string Nombre,
    string? DireccionFacturacion,
    bool TieneRuc)
{
    public static ClienteDto Desde(Cliente cliente) => new(
        cliente.Id,
        cliente.TipoDocumento.Trim(),
        cliente.NumeroDocumento.Trim(),
        cliente.NombreCompleto,
        cliente.DireccionFacturacion?.Trim(),
        cliente.TieneRuc);
}

public sealed record RegistrarClienteDto(
    string TipoDocumento,
    string NumeroDocumento,
    string? Nombres,
    string? Apellidos,
    string? RazonSocial,
    string? DireccionFacturacion,
    string? Telefono,
    string? Email);

public sealed record ArticuloDisponibleDto(
    string Codigo,
    string Descripcion,
    string? UnidadMedida,
    decimal PrecioUnitario,
    decimal Stock);

public sealed record ItemCatalogoDto(string Valor, string Descripcion);

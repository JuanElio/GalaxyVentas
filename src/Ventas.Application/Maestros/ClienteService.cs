using FluentValidation;
using FluentValidation.Results;
using Ventas.Application.Comun;
using Ventas.Domain.Maestros;

namespace Ventas.Application.Maestros;

/// <summary>Búsqueda (RF-02) y registro (RF-03) de clientes.</summary>
public sealed class ClienteService(
    IClienteRepository clientes,
    IUnitOfWork unidadTrabajo,
    IValidator<RegistrarClienteDto> validador)
{
    public async Task<IReadOnlyList<ClienteDto>> BuscarPorDocumentoAsync(string? numeroDocumento, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento))
            throw new ValidationException([new ValidationFailure("NumeroDocumento", "Ingrese el número de documento.")]);

        var numero = numeroDocumento.Trim();
        var encontrados = await clientes.BuscarActivosPorDocumentoAsync(numero, ct);

        return encontrados.Count > 0
            ? encontrados.Select(ClienteDto.Desde).ToList()
            : throw new NoEncontradoException($"No existe un cliente activo con el documento {numero}.");
    }

    public async Task<ClienteDto> RegistrarAsync(RegistrarClienteDto datos, CancellationToken ct)
    {
        await validador.ValidateAndThrowAsync(datos, ct);

        var numero = datos.NumeroDocumento.Trim();
        if (await clientes.ExisteActivoAsync(datos.TipoDocumento, numero, ct))
            throw new ConflictoException($"Ya existe un cliente activo con el documento {numero}.");

        var cliente = Cliente.Registrar(datos.TipoDocumento, numero, datos.Nombres, datos.Apellidos,
            datos.RazonSocial, datos.DireccionFacturacion, datos.Telefono, datos.Email);

        await clientes.AgregarAsync(cliente, ct);
        await unidadTrabajo.GuardarCambiosAsync(ct);

        return ClienteDto.Desde(cliente);
    }
}

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
    // SGP_CLIENTE no tiene índices por documento ni por nombre: se exige un texto mínimo y se limitan las palabras
    public const int LongitudMinimaNombre = 3;
    public const int MaximoPalabras = 5;

    /// <summary>
    /// Busca clientes activos (RF-02): si el texto son solo dígitos, por número de documento; si no,
    /// por nombre o razón social, exigiendo que cada palabra aparezca en nombres, apellidos o razón social.
    /// </summary>
    public async Task<IReadOnlyList<ClienteDto>> BuscarAsync(string? texto, CancellationToken ct)
    {
        var buscado = texto?.Trim() ?? string.Empty;
        if (buscado.Length == 0)
            throw ErrorDeBusqueda("Ingrese el número de documento o el nombre del cliente.");

        return buscado.All(char.IsAsciiDigit)
            ? await BuscarPorDocumentoAsync(buscado, ct)
            : await BuscarPorNombreAsync(buscado, ct);
    }

    private async Task<IReadOnlyList<ClienteDto>> BuscarPorDocumentoAsync(string numero, CancellationToken ct)
    {
        var encontrados = await clientes.BuscarActivosPorDocumentoAsync(numero, ct);

        return encontrados.Count > 0
            ? encontrados.Select(ClienteDto.Desde).ToList()
            : throw new NoEncontradoException($"No existe un cliente activo con el documento {numero}.");
    }

    private async Task<IReadOnlyList<ClienteDto>> BuscarPorNombreAsync(string texto, CancellationToken ct)
    {
        if (texto.Length < LongitudMinimaNombre)
            throw ErrorDeBusqueda($"Ingrese al menos {LongitudMinimaNombre} caracteres del nombre o la razón social.");

        var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaximoPalabras)
            .ToList();
        var encontrados = await clientes.BuscarActivosPorNombreAsync(palabras, ct);

        return encontrados.Count > 0
            ? encontrados.Select(ClienteDto.Desde).ToList()
            : throw new NoEncontradoException($"No existe un cliente activo cuyo nombre o razón social contenga «{texto}».");
    }

    private static ValidationException ErrorDeBusqueda(string mensaje) =>
        new([new ValidationFailure("Texto", mensaje)]);

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

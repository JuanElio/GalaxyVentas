using FluentValidation;
using FluentValidation.Results;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;

namespace Ventas.Application.Seguridad;

/// <summary>Datos del usuario de la sesión y cambio de su contraseña (página Mis datos).</summary>
public sealed class MisDatosService(
    IUsuarioRepository usuarios,
    IPuntoVentaRepository puntosVenta,
    IVerificadorClave verificadorClave,
    ICambioClave cambioClave,
    IUnitOfWork unidadTrabajo,
    IUsuarioActual usuarioActual,
    IValidator<CambiarClaveDto> validador)
{
    public const string MensajeClaveActualIncorrecta = "La contraseña actual no es correcta.";

    public async Task<MisDatosDto> ObtenerAsync(CancellationToken ct)
    {
        var login = usuarioActual.Login;
        var usuario = await usuarios.ObtenerPorLoginAsync(login, ct)
            ?? throw new NoEncontradoException($"No existe el usuario {login}.");

        // El punto de venta es siempre el de la sesión (RN-19)
        var codigoPuntoVenta = usuarioActual.CodigoPuntoVenta;
        var puntoVenta = await puntosVenta.ObtenerAsync(codigoPuntoVenta, ct);

        return new MisDatosDto(
            usuario.Login.Trim(),
            string.IsNullOrWhiteSpace(usuario.Nombre) ? usuario.Login.Trim() : usuario.Nombre.Trim(),
            Limpiar(usuario.NumeroDocumento),
            Limpiar(usuario.Email),
            Limpiar(usuario.Telefono),
            codigoPuntoVenta,
            Limpiar(puntoVenta?.Descripcion),
            usuario.InicioVigencia,
            usuario.FinVigencia,
            usuario.FechaExpiraClave);
    }

    /// <summary>
    /// Cambia la contraseña del usuario de la sesión si la actual es correcta. El login se toma
    /// siempre de la sesión, nunca de la petición (RN-19).
    /// </summary>
    public async Task CambiarClaveAsync(CambiarClaveDto datos, CancellationToken ct)
    {
        await validador.ValidateAndThrowAsync(datos, ct);

        var login = usuarioActual.Login;
        if (!await verificadorClave.VerificarAsync(login, datos.ClaveActual, ct))
            throw new ValidationException([new ValidationFailure(nameof(CambiarClaveDto.ClaveActual), MensajeClaveActualIncorrecta)]);

        await cambioClave.CambiarAsync(login, datos.ClaveNueva, ct);
        await unidadTrabajo.GuardarCambiosAsync(ct);
    }

    private static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}

using FluentValidation;
using Ventas.Application.Comun;

namespace Ventas.Application.Seguridad;

/// <summary>
/// Inicio de sesión (RF-21) y revalidación de la sesión en cada petición (RF-19).
/// </summary>
public sealed class AutenticacionService(
    IUsuarioRepository usuarios,
    IVerificadorClave verificadorClave,
    IValidator<IniciarSesionDto> validador,
    TimeProvider reloj)
{
    // Mismo mensaje para usuario inexistente, inactivo, no vigente o contraseña incorrecta
    public const string MensajeCredencialesInvalidas = "Usuario o contraseña incorrectos.";

    public async Task<SesionDto> IniciarSesionAsync(IniciarSesionDto datos, CancellationToken ct)
    {
        await validador.ValidateAndThrowAsync(datos, ct);

        var login = datos.Login.Trim();
        var claveCorrecta = await verificadorClave.VerificarAsync(login, datos.Clave, ct);
        var usuario = claveCorrecta ? await usuarios.ObtenerPorLoginAsync(login, ct) : null;
        var hoy = Hoy();

        if (usuario is null || !usuario.EstaVigente(hoy))
            throw new NoAutenticadoException(MensajeCredencialesInvalidas);
        if (usuario.ClaveExpirada(hoy))
            throw new NoAutenticadoException("Su contraseña expiró. Solicite al administrador que la renueve.");
        if (!usuario.TienePuntoVenta)
            throw new AccesoDenegadoException("El usuario no tiene un punto de venta asignado.");

        return new SesionDto(
            usuario.Login.Trim(),
            string.IsNullOrWhiteSpace(usuario.Nombre) ? usuario.Login.Trim() : usuario.Nombre.Trim(),
            usuario.CodigoPuntoVenta!.Trim());
    }

    /// <summary>
    /// La sesión sigue siendo válida si el usuario existe, está activo y vigente, y conserva
    /// el punto de venta con el que inició sesión.
    /// </summary>
    public async Task<bool> ValidarSesionAsync(string login, string codigoPuntoVenta, CancellationToken ct)
    {
        var usuario = await usuarios.ObtenerPorLoginAsync(login, ct);

        return usuario is not null
            && usuario.EstaVigente(Hoy())
            && usuario.PerteneceAPuntoVenta(codigoPuntoVenta);
    }

    private DateTime Hoy() => reloj.GetLocalNow().DateTime;
}

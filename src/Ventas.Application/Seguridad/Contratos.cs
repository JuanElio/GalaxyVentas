using Ventas.Domain.Seguridad;

namespace Ventas.Application.Seguridad;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorLoginAsync(string login, CancellationToken ct);

    /// <summary>Usuarios activos asignados al punto de venta, ordenados por nombre.</summary>
    Task<IReadOnlyList<Usuario>> ListarActivosPorPuntoVentaAsync(string codigoPuntoVenta, CancellationToken ct);
}

/// <summary>Compara la contraseña ingresada con la guardada en SGP_USUARIO (P-02).</summary>
public interface IVerificadorClave
{
    /// <summary>Devuelve false si el usuario no existe o la contraseña no coincide.</summary>
    Task<bool> VerificarAsync(string login, string clave, CancellationToken ct);
}

/// <summary>
/// Cambia la contraseña de SGP_USUARIO.vchPassword. Se guarda en texto plano, como la espera el
/// sistema actual (P-02); el cambio se graba con <see cref="Comun.IUnitOfWork.GuardarCambiosAsync"/>.
/// </summary>
public interface ICambioClave
{
    Task CambiarAsync(string login, string claveNueva, CancellationToken ct);
}

/// <summary>Usuario de la sesión, leído de los claims de la cookie (RN-19).</summary>
public interface IUsuarioActual
{
    bool EstaAutenticado { get; }

    string Login { get; }

    string Nombre { get; }

    string CodigoPuntoVenta { get; }
}

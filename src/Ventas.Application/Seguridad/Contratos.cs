using Ventas.Domain.Seguridad;

namespace Ventas.Application.Seguridad;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorLoginAsync(string login, CancellationToken ct);
}

/// <summary>Compara la contraseña ingresada con la guardada en SGP_USUARIO (P-02).</summary>
public interface IVerificadorClave
{
    /// <summary>Devuelve false si el usuario no existe o la contraseña no coincide.</summary>
    Task<bool> VerificarAsync(string login, string clave, CancellationToken ct);
}

/// <summary>Usuario de la sesión, leído de los claims de la cookie (RN-19).</summary>
public interface IUsuarioActual
{
    bool EstaAutenticado { get; }

    string Login { get; }

    string Nombre { get; }

    string CodigoPuntoVenta { get; }
}

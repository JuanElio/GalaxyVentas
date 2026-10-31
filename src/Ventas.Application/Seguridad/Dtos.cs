namespace Ventas.Application.Seguridad;

public sealed record IniciarSesionDto(string Login, string Clave);

/// <summary>Datos que se guardan en la cookie de sesión; nunca incluye la contraseña.</summary>
public sealed record SesionDto(string Login, string Nombre, string CodigoPuntoVenta);

/// <summary>Datos del usuario de la sesión para la página Mis datos; nunca incluye la contraseña.</summary>
public sealed record MisDatosDto(
    string Login,
    string Nombre,
    string? NumeroDocumento,
    string? Email,
    string? Telefono,
    string CodigoPuntoVenta,
    string? PuntoVenta,
    DateTime InicioVigencia,
    DateTime FinVigencia,
    DateTime? FechaExpiraClave);

public sealed record CambiarClaveDto(string ClaveActual, string ClaveNueva, string ConfirmacionClave);

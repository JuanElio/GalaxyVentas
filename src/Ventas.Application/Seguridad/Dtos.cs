namespace Ventas.Application.Seguridad;

public sealed record IniciarSesionDto(string Login, string Clave);

/// <summary>Datos que se guardan en la cookie de sesión; nunca incluye la contraseña.</summary>
public sealed record SesionDto(string Login, string Nombre, string CodigoPuntoVenta);

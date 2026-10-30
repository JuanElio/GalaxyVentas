namespace Ventas.Application.Comun;

/// <summary>Credenciales incorrectas, usuario no vigente o contraseña expirada (401).</summary>
public class NoAutenticadoException(string message) : Exception(message);

/// <summary>El recurso solicitado no existe o no está activo (404).</summary>
public class NoEncontradoException(string message) : Exception(message);

/// <summary>El recurso pertenece a otro punto de venta o el usuario no tiene acceso (403).</summary>
public class AccesoDenegadoException(string message) : Exception(message);

/// <summary>La operación choca con el estado actual de los datos, p. ej. un duplicado (409).</summary>
public class ConflictoException(string message) : Exception(message);

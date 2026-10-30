namespace Ventas.Domain.Comun;

/// <summary>
/// Se lanza cuando una operación viola una regla de negocio o un invariante del dominio.
/// </summary>
public class DominioException(string message) : Exception(message);

namespace Ventas.Application.Comun;

public interface IUnitOfWork
{
    Task GuardarCambiosAsync(CancellationToken ct);

    /// <summary>
    /// Ejecuta la operación en una única transacción de BD (RN-15): confirma si termina bien y
    /// deshace todo si lanza una excepción. Si ya hay una transacción abierta, se une a ella.
    /// </summary>
    Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken ct);
}

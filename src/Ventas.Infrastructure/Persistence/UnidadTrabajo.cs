using Ventas.Application.Comun;

namespace Ventas.Infrastructure.Persistence;

public sealed class UnidadTrabajo(VentasDbContext db) : IUnitOfWork
{
    public async Task GuardarCambiosAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);

    public async Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken ct)
    {
        // Una transacción ya abierta (p. ej. la de una prueba) decide el commit o el rollback
        if (db.Database.CurrentTransaction is not null)
            return await operacion();

        // Si la operación lanza una excepción, la transacción se descarta sin commit: rollback completo
        await using var transaccion = await db.Database.BeginTransactionAsync(ct);
        var resultado = await operacion();
        await transaccion.CommitAsync(ct);
        return resultado;
    }
}

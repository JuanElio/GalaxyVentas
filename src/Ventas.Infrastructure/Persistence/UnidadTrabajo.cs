using Ventas.Application.Comun;

namespace Ventas.Infrastructure.Persistence;

public sealed class UnidadTrabajo(VentasDbContext db) : IUnitOfWork
{
    public async Task GuardarCambiosAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}

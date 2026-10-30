using Microsoft.EntityFrameworkCore;
using Ventas.Application.Seguridad;
using Ventas.Domain.Seguridad;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Seguridad;

public sealed class UsuarioRepository(VentasDbContext db) : IUsuarioRepository
{
    public async Task<Usuario?> ObtenerPorLoginAsync(string login, CancellationToken ct) =>
        await db.Set<Usuario>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Login == login, ct);
}

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

    // SGP_USUARIO guarda el punto de venta con espacio inicial (' 001')
    public async Task<IReadOnlyList<Usuario>> ListarActivosPorPuntoVentaAsync(string codigoPuntoVenta, CancellationToken ct)
    {
        var puntoVenta = codigoPuntoVenta.Trim();
        return await db.Set<Usuario>()
            .AsNoTracking()
            .Where(u => u.CodigoPuntoVenta != null && u.CodigoPuntoVenta.Trim() == puntoVenta
                && EF.Property<bool?>(u, Conversiones.CampoActivo) == true)
            .OrderBy(u => u.Nombre)
            .ToListAsync(ct);
    }
}

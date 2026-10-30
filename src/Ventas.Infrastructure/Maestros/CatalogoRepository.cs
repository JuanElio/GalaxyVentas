using Microsoft.EntityFrameworkCore;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Maestros;

public sealed class CatalogoRepository(VentasDbContext db) : ICatalogoRepository
{
    public async Task<IReadOnlyList<ItemCatalogo>> ListarItemsActivosAsync(int codigoCatalogo, CancellationToken ct)
    {
        var catalogoActivo = db.Set<Catalogo>()
            .Any(c => c.Codigo == codigoCatalogo && EF.Property<bool?>(c, Conversiones.CampoActivo) == true);

        return await db.Set<ItemCatalogo>().AsNoTracking()
            .Where(i => i.CodigoCatalogo == codigoCatalogo
                && EF.Property<bool?>(i, Conversiones.CampoActivo) == true
                && catalogoActivo)
            .OrderBy(i => i.Item)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Banco>> ListarBancosActivosAsync(CancellationToken ct) =>
        await db.Set<Banco>().AsNoTracking()
            .Where(b => EF.Property<bool?>(b, Conversiones.CampoActivo) == true)
            .ToListAsync(ct);
}

using Microsoft.EntityFrameworkCore;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Maestros;

public sealed class PuntoVentaRepository(VentasDbContext db) : IPuntoVentaRepository
{
    // Los códigos se guardan con un espacio inicial (' 001'); la sesión los tiene sin espacios
    public async Task<PuntoVenta?> ObtenerAsync(string codigo, CancellationToken ct)
    {
        var buscado = codigo.Trim();
        return await db.Set<PuntoVenta>().AsNoTracking().FirstOrDefaultAsync(p => p.Codigo.Trim() == buscado, ct);
    }

    public async Task<string?> ObtenerAlmacenPrincipalAsync(string codigoPuntoVenta, CancellationToken ct)
    {
        var buscado = codigoPuntoVenta.Trim();
        return await db.Set<PuntoVentaAlmacen>().AsNoTracking()
            .Where(a => a.CodigoPuntoVenta.Trim() == buscado && EF.Property<bool?>(a, Conversiones.CampoActivo) == true)
            .OrderBy(a => a.CodigoAlmacen)
            .Select(a => a.CodigoAlmacen)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ZonaVenta?> ObtenerZonaVentaAsync(string codigoZona, CancellationToken ct) =>
        await db.Set<ZonaVenta>().AsNoTracking().FirstOrDefaultAsync(z => z.Codigo == codigoZona, ct);

    public async Task<TipoOrden?> ObtenerTipoOrdenAsync(string codigo, CancellationToken ct)
    {
        var buscado = codigo.Trim();
        return await db.Set<TipoOrden>().AsNoTracking().FirstOrDefaultAsync(t => t.Codigo.Trim() == buscado, ct);
    }
}

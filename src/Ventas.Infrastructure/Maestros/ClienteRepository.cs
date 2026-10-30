using Microsoft.EntityFrameworkCore;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Maestros;

public sealed class ClienteRepository(VentasDbContext db) : IClienteRepository
{
    // SGP_CLIENTE no tiene índice por documento (unos 200 ms por búsqueda); se limita el resultado
    private const int MaximoCoincidencias = 20;

    private IQueryable<Cliente> Activos =>
        db.Set<Cliente>().Where(c => EF.Property<bool?>(c, Conversiones.CampoActivo) == true);

    public async Task<IReadOnlyList<Cliente>> BuscarActivosPorDocumentoAsync(string numeroDocumento, CancellationToken ct) =>
        await Activos.AsNoTracking()
            .Where(c => c.NumeroDocumento == numeroDocumento)
            .OrderByDescending(c => c.Id)
            .Take(MaximoCoincidencias)
            .ToListAsync(ct);

    public async Task<bool> ExisteActivoAsync(string tipoDocumento, string numeroDocumento, CancellationToken ct) =>
        await Activos.AnyAsync(c => c.TipoDocumento == tipoDocumento && c.NumeroDocumento == numeroDocumento, ct);

    public async Task<Cliente?> ObtenerActivoAsync(long codigo, CancellationToken ct) =>
        await Activos.FirstOrDefaultAsync(c => c.Id == codigo, ct);

    public async Task AgregarAsync(Cliente cliente, CancellationToken ct) =>
        await db.Set<Cliente>().AddAsync(cliente, ct);
}

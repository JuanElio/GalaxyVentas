using Ventas.Application.Ventas;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Inventario;
using Ventas.Domain.Ventas;
using Ventas.Infrastructure.Persistence;
using Ventas.Infrastructure.Persistence.Configurations;

namespace Ventas.Infrastructure.Ventas;

public sealed class VentaRepository(VentasDbContext db) : IVentaRepository
{
    public async Task AgregarAsync(Venta venta, CancellationToken ct)
    {
        await db.Set<Venta>().AddAsync(venta, ct);
        // Identificador del registro, como el que recibe el procedimiento de referencia
        db.Entry(venta).Property(VentaConfiguration.GuidRegistro).CurrentValue = Guid.NewGuid().ToString().ToUpperInvariant();
    }
}

public sealed class ComprobanteRepository(VentasDbContext db) : IComprobanteRepository
{
    public async Task AgregarAsync(Comprobante comprobante, CancellationToken ct)
    {
        await db.Set<Comprobante>().AddAsync(comprobante, ct);
        // vchCodTransaccion guarda el número de transacción como texto (pga_hemeroteca_emitir_comprobante)
        db.Entry(comprobante).Property(ComprobanteConfiguration.CodigoTransaccion).CurrentValue =
            comprobante.Movimientos.Select(m => m.CodigoTransaccion).Distinct().Single().ToString();
    }
}

public sealed class KardexRepository(VentasDbContext db) : IKardexRepository
{
    public async Task AgregarAsync(IEnumerable<MovimientoKardex> movimientos, CancellationToken ct) =>
        await db.Set<MovimientoKardex>().AddRangeAsync(movimientos, ct);
}

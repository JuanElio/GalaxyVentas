using Microsoft.EntityFrameworkCore;
using Ventas.Application.Maestros;
using Ventas.Domain.Inventario;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Inventario;

/// <summary>
/// Stock = Σ Cantidad de PGA_TRANSACCION_KARDEX por artículo y punto de venta (RN-09). No se
/// filtra por canal: el procedimiento de referencia graba CodTarifaCanal en NULL.
/// </summary>
public sealed class StockService(VentasDbContext db) : IStockService
{
    public async Task<IReadOnlyDictionary<string, decimal>> ObtenerStockAsync(
        string codigoPuntoVenta, IReadOnlyCollection<string> codigosArticulo, CancellationToken ct)
    {
        var codigos = codigosArticulo.Select(c => c.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (codigos.Count == 0)
            return new Dictionary<string, decimal>();

        var puntoVenta = codigoPuntoVenta.Trim();
        var saldos = await db.Set<MovimientoKardex>().AsNoTracking()
            .Where(m => m.CodigoPuntoVenta!.Trim() == puntoVenta
                && codigos.Contains(m.CodigoArticulo!.Trim())
                && EF.Property<bool?>(m, Conversiones.CampoActivo) == true)
            .GroupBy(m => m.CodigoArticulo!.Trim())
            .Select(g => new { Codigo = g.Key, Stock = g.Sum(m => m.Cantidad ?? 0) })
            .ToListAsync(ct);

        return saldos.ToDictionary(s => s.Codigo, s => s.Stock, StringComparer.OrdinalIgnoreCase);
    }
}

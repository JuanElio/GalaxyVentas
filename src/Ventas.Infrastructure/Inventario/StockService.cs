using Microsoft.EntityFrameworkCore;
using Ventas.Application.Comun;
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
    // Tiempo máximo de espera por el stock que está usando otra venta
    private const int EsperaBloqueoMilisegundos = 10_000;

    public async Task<IReadOnlyDictionary<string, decimal>> ObtenerStockAsync(
        string codigoPuntoVenta, IReadOnlyCollection<string> codigosArticulo, CancellationToken ct)
    {
        var codigos = Normalizar(codigosArticulo);
        if (codigos.Count == 0)
            return new Dictionary<string, decimal>();

        // El conversor del punto de venta compara con el formato de la BD (' 001')
        var puntoVenta = codigoPuntoVenta.Trim();
        var saldos = await db.Set<MovimientoKardex>().AsNoTracking()
            .Where(m => m.CodigoPuntoVenta == puntoVenta
                && codigos.Contains(m.CodigoArticulo!.Trim())
                && EF.Property<bool?>(m, Conversiones.CampoActivo) == true)
            .GroupBy(m => m.CodigoArticulo!.Trim())
            .Select(g => new { Codigo = g.Key, Stock = g.Sum(m => m.Cantidad ?? 0) })
            .ToListAsync(ct);

        return saldos.ToDictionary(s => s.Codigo, s => s.Stock, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Un bloqueo de aplicación (sp_getapplock) por artículo y punto de venta, liberado al terminar
    /// la transacción. El kardex no tiene índices, así que bloquear filas no evitaría que dos
    /// ventas leyeran el mismo saldo. Se toman en orden para no provocar interbloqueos.
    /// </summary>
    public async Task BloquearAsync(string codigoPuntoVenta, IReadOnlyCollection<string> codigosArticulo, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("El bloqueo del stock requiere una transacción abierta.");

        var puntoVenta = codigoPuntoVenta.Trim();
        foreach (var codigo in Normalizar(codigosArticulo).Order(StringComparer.OrdinalIgnoreCase))
        {
            var recurso = $"GalaxyVentas:stock:{puntoVenta}:{codigo.ToUpperInvariant()}";
            var resultado = await db.Database
                .SqlQuery<int>($"""
                    DECLARE @resultado int;
                    EXEC @resultado = sp_getapplock @Resource = {recurso}, @LockMode = 'Exclusive',
                        @LockOwner = 'Transaction', @LockTimeout = {EsperaBloqueoMilisegundos};
                    SELECT @resultado AS Value;
                    """)
                .ToListAsync(ct);

            // 0 o 1 = concedido; negativo = tiempo agotado, cancelado o interbloqueo
            if (resultado.Single() < 0)
                throw new ConflictoException(
                    $"El stock del artículo {codigo} está siendo usado por otra venta. Intente nuevamente.");
        }
    }

    private static List<string> Normalizar(IReadOnlyCollection<string> codigos) =>
        codigos.Select(c => c.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
}

using Microsoft.EntityFrameworkCore;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Maestros;

/// <summary>
/// SGP_TARIFA guarda los códigos de artículo y canal con espacios a la izquierda y SGP_ARTICULO
/// casi siempre sin ellos: se comparan sin espacios, como el procedimiento de referencia.
/// </summary>
public sealed class ArticuloRepository(VentasDbContext db) : IArticuloRepository
{
    public async Task<IReadOnlyList<ArticuloConTarifas>> ListarConTarifasVigentesAsync(
        string codigoCanal, string? texto, DateTime fecha, int maximo, CancellationToken ct)
    {
        var tarifasPorArticulo = await TarifasVigentesAsync(codigoCanal, null, fecha, ct);
        if (tarifasPorArticulo.Count == 0)
            return [];

        var codigos = tarifasPorArticulo.Keys.ToList();
        var consulta = ArticulosActivos(codigos);
        if (texto is not null)
            consulta = consulta.Where(a => a.Codigo.Contains(texto) || (a.Descripcion != null && a.Descripcion.Contains(texto)));

        var articulos = await consulta.OrderBy(a => a.Descripcion).Take(maximo).ToListAsync(ct);
        return Combinar(articulos, tarifasPorArticulo);
    }

    public async Task<IReadOnlyList<ArticuloConTarifas>> ObtenerConTarifasVigentesAsync(
        string codigoCanal, IReadOnlyCollection<string> codigosArticulo, DateTime fecha, CancellationToken ct)
    {
        var pedidos = codigosArticulo.Select(c => c.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (pedidos.Count == 0)
            return [];

        var tarifasPorArticulo = await TarifasVigentesAsync(codigoCanal, pedidos, fecha, ct);
        if (tarifasPorArticulo.Count == 0)
            return [];

        var articulos = await ArticulosActivos(tarifasPorArticulo.Keys.ToList()).ToListAsync(ct);
        return Combinar(articulos, tarifasPorArticulo);
    }

    public async Task<TarifaCanal?> ObtenerCanalAsync(string codigoCanal, CancellationToken ct)
    {
        var canal = codigoCanal.Trim();
        return await db.Set<TarifaCanal>().AsNoTracking().FirstOrDefaultAsync(c => c.Codigo.Trim() == canal, ct);
    }

    private async Task<Dictionary<string, IReadOnlyList<Tarifa>>> TarifasVigentesAsync(
        string codigoCanal, IReadOnlyCollection<string>? codigosArticulo, DateTime fecha, CancellationToken ct)
    {
        var canal = codigoCanal.Trim();
        var dia = fecha.Date;

        var consulta = db.Set<Tarifa>().AsNoTracking()
            .Where(t => t.CodigoCanal.Trim() == canal
                && EF.Property<bool?>(t, Conversiones.CampoActivo) == true
                && t.Precio != null
                && t.InicioVigencia <= fecha
                && t.FinVigencia >= dia);
        if (codigosArticulo is not null)
            consulta = consulta.Where(t => codigosArticulo.Contains(t.CodigoArticulo.Trim()));

        var tarifas = await consulta.ToListAsync(ct);
        return tarifas
            .GroupBy(t => t.CodigoArticulo.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Tarifa>)g.ToList(), StringComparer.OrdinalIgnoreCase);
    }

    private IQueryable<Articulo> ArticulosActivos(List<string> codigos) =>
        db.Set<Articulo>().AsNoTracking()
            .Where(a => EF.Property<bool?>(a, Conversiones.CampoActivo) == true && codigos.Contains(a.Codigo.Trim()));

    private static List<ArticuloConTarifas> Combinar(
        IEnumerable<Articulo> articulos, Dictionary<string, IReadOnlyList<Tarifa>> tarifasPorArticulo) =>
        articulos
            .Select(a => new ArticuloConTarifas(
                new Articulo(a.Codigo.Trim(), a.Descripcion, a.UnidadMedida, a.Activo),
                tarifasPorArticulo[a.Codigo.Trim()]))
            .ToList();
}

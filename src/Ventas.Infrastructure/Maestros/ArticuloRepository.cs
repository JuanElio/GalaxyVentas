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
        var canal = codigoCanal.Trim();
        var dia = fecha.Date;

        var tarifas = await db.Set<Tarifa>().AsNoTracking()
            .Where(t => t.CodigoCanal.Trim() == canal
                && EF.Property<bool?>(t, Conversiones.CampoActivo) == true
                && t.Precio != null
                && t.InicioVigencia <= fecha
                && t.FinVigencia >= dia)
            .ToListAsync(ct);

        var tarifasPorArticulo = tarifas
            .GroupBy(t => t.CodigoArticulo.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Tarifa>)g.ToList(), StringComparer.OrdinalIgnoreCase);
        if (tarifasPorArticulo.Count == 0)
            return [];

        var codigos = tarifasPorArticulo.Keys.ToList();
        var consulta = db.Set<Articulo>().AsNoTracking()
            .Where(a => EF.Property<bool?>(a, Conversiones.CampoActivo) == true && codigos.Contains(a.Codigo.Trim()));
        if (texto is not null)
            consulta = consulta.Where(a => a.Codigo.Contains(texto) || (a.Descripcion != null && a.Descripcion.Contains(texto)));

        var articulos = await consulta
            .OrderBy(a => a.Descripcion)
            .Take(maximo)
            .ToListAsync(ct);

        return articulos
            .Select(a => new ArticuloConTarifas(
                new Articulo(a.Codigo.Trim(), a.Descripcion, a.UnidadMedida, a.Activo),
                tarifasPorArticulo[a.Codigo.Trim()]))
            .ToList();
    }

    public async Task<bool> ExisteCanalActivoAsync(string codigoCanal, CancellationToken ct)
    {
        var canal = codigoCanal.Trim();
        return await db.Set<TarifaCanal>()
            .AnyAsync(c => c.Codigo.Trim() == canal && EF.Property<bool?>(c, Conversiones.CampoActivo) == true, ct);
    }
}

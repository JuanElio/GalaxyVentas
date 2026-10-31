using Microsoft.EntityFrameworkCore;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Maestros;

/// <summary>
/// Reproduce la consulta de artículos por categoría del sistema actual (SGP_ARTICULO +
/// SGP_CATEGORIA_ARTICULO + SGP_TARIFA + SGP_DETALLE_IVA + SGP_CATEGORIA_VENTA). Las tablas guardan
/// los códigos con y sin espacios a la izquierda, así que se comparan sin espacios.
/// </summary>
public sealed class ArticuloRepository(VentasDbContext db) : IArticuloRepository
{
    public async Task<IReadOnlyList<ArticuloConTarifas>> ListarConTarifasVigentesAsync(
        string tipoOrden, string codigoCanal, int? codigoCategoria, string? texto, DateTime fecha, int maximo, CancellationToken ct)
    {
        var vendibles = await VendiblesAsync(tipoOrden, codigoCanal, codigoCategoria, null, texto, fecha, ct);
        return vendibles
            .OrderBy(a => a.Categoria.Codigo)
            .ThenBy(a => a.Articulo.Descripcion, StringComparer.CurrentCultureIgnoreCase)
            .Take(maximo)
            .ToList();
    }

    public async Task<IReadOnlyList<ArticuloConTarifas>> ObtenerConTarifasVigentesAsync(
        string tipoOrden, string codigoCanal, IReadOnlyCollection<string> codigosArticulo, DateTime fecha, CancellationToken ct)
    {
        var pedidos = codigosArticulo.Select(c => c.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (pedidos.Count == 0)
            return [];

        return await VendiblesAsync(tipoOrden, codigoCanal, null, pedidos, null, fecha, ct);
    }

    public async Task<IReadOnlyList<CategoriaVenta>> ListarCategoriasAsync(string tipoOrden, string codigoCanal, CancellationToken ct)
    {
        var codigos = (await CategoriasPorArticuloAsync(tipoOrden, codigoCanal, null, null, ct)).Values
            .Select(c => c.Codigo).Distinct().ToList();

        return await db.Set<CategoriaVenta>().AsNoTracking()
            .Where(c => codigos.Contains(c.Codigo))
            .OrderBy(c => c.Codigo)
            .ToListAsync(ct);
    }

    public async Task<TarifaCanal?> ObtenerCanalAsync(string codigoCanal, CancellationToken ct)
    {
        var canal = codigoCanal.Trim();
        return await db.Set<TarifaCanal>().AsNoTracking().FirstOrDefaultAsync(c => c.Codigo.Trim() == canal, ct);
    }

    private async Task<List<ArticuloConTarifas>> VendiblesAsync(string tipoOrden, string codigoCanal, int? codigoCategoria,
        IReadOnlyCollection<string>? codigosArticulo, string? texto, DateTime fecha, CancellationToken ct)
    {
        var categorias = await CategoriasPorArticuloAsync(tipoOrden, codigoCanal, codigoCategoria, codigosArticulo, ct);
        if (categorias.Count == 0)
            return [];

        var tarifas = await TarifasVigentesAsync(codigoCanal, categorias.Keys.ToList(), fecha, ct);
        if (tarifas.Count == 0)
            return [];

        var codigos = tarifas.Keys.ToList();
        var consulta = db.Set<Articulo>().AsNoTracking()
            .Where(a => EF.Property<bool?>(a, Conversiones.CampoActivo) == true && codigos.Contains(a.Codigo.Trim()));
        if (texto is not null)
            consulta = consulta.Where(a => a.Codigo.Contains(texto) || (a.Descripcion != null && a.Descripcion.Contains(texto)));
        var articulos = await consulta.ToListAsync(ct);

        var tasas = await TasasIgvAsync(articulos.Select(a => a.CodigoIva), fecha, ct);

        // Sin tasa de IVA vigente el artículo no se puede vender (la consulta de referencia lo excluye)
        return articulos
            .Select(a => (Articulo: a, Codigo: a.Codigo.Trim(), CodigoIva: a.CodigoIva?.Trim() ?? string.Empty))
            .Where(a => tasas.ContainsKey(a.CodigoIva))
            .Select(a => new ArticuloConTarifas(
                new Articulo(a.Codigo, a.Articulo.Descripcion, a.Articulo.UnidadMedida, a.Articulo.Activo, a.CodigoIva),
                tarifas[a.Codigo], categorias[a.Codigo], tasas[a.CodigoIva]))
            .ToList();
    }

    /// <summary>
    /// Categoría activa de cada artículo para el tipo de orden y canal. Si un artículo tiene más de
    /// una, se usa la de menor código (la consulta de referencia ordena por categoría).
    /// </summary>
    private async Task<Dictionary<string, CategoriaVenta>> CategoriasPorArticuloAsync(string tipoOrden, string codigoCanal,
        int? codigoCategoria, IReadOnlyCollection<string>? codigosArticulo, CancellationToken ct)
    {
        var orden = tipoOrden.Trim();
        var canal = codigoCanal.Trim();

        var consulta =
            from asignacion in db.Set<CategoriaArticulo>().AsNoTracking()
            join categoria in db.Set<CategoriaVenta>().AsNoTracking() on asignacion.CodigoCategoria equals categoria.Codigo
            where EF.Property<bool?>(asignacion, Conversiones.CampoActivo) == true
                && EF.Property<bool?>(categoria, Conversiones.CampoActivo) == true
                && asignacion.TipoOrden!.Trim() == orden
                && asignacion.CodigoCanal.Trim() == canal
            select new { Articulo = asignacion.CodigoArticulo.Trim(), Categoria = categoria };
        if (codigoCategoria is not null)
            consulta = consulta.Where(c => c.Categoria.Codigo == codigoCategoria);
        if (codigosArticulo is not null)
            consulta = consulta.Where(c => codigosArticulo.Contains(c.Articulo));

        var filas = await consulta.ToListAsync(ct);
        return filas
            .GroupBy(f => f.Articulo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.MinBy(f => f.Categoria.Codigo)!.Categoria, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, IReadOnlyList<Tarifa>>> TarifasVigentesAsync(
        string codigoCanal, IReadOnlyCollection<string> codigosArticulo, DateTime fecha, CancellationToken ct)
    {
        var canal = codigoCanal.Trim();
        var dia = fecha.Date;

        var tarifas = await db.Set<Tarifa>().AsNoTracking()
            .Where(t => t.CodigoCanal.Trim() == canal
                && EF.Property<bool?>(t, Conversiones.CampoActivo) == true
                && t.Precio != null
                && t.InicioVigencia <= fecha
                && t.FinVigencia >= dia
                && codigosArticulo.Contains(t.CodigoArticulo.Trim()))
            .ToListAsync(ct);

        return tarifas
            .GroupBy(t => t.CodigoArticulo.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Tarifa>)g.ToList(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Tasa vigente de cada código de IVA (sin espacios) que tiene una.</summary>
    private async Task<Dictionary<string, decimal>> TasasIgvAsync(IEnumerable<string?> codigosIva, DateTime fecha, CancellationToken ct)
    {
        var codigos = codigosIva.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (codigos.Count == 0)
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var detalles = await db.Set<DetalleIva>().AsNoTracking()
            .Where(d => codigos.Contains(d.CodigoIva.Trim()))
            .ToListAsync(ct);

        return detalles
            .GroupBy(d => d.CodigoIva.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => (Codigo: g.Key, Tasa: DetalleIva.TasaVigente(g, fecha)))
            .Where(t => t.Tasa is not null)
            .ToDictionary(t => t.Codigo, t => t.Tasa!.Value, StringComparer.OrdinalIgnoreCase);
    }
}

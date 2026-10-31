using Ventas.Application.Comun;
using Ventas.Application.Seguridad;
using Ventas.Application.Ventas;
using Ventas.Domain.Maestros;

namespace Ventas.Application.Maestros;

/// <summary>
/// Artículos disponibles para la venta en el punto de venta de la sesión, agrupados por categoría
/// de venta (RF-01).
/// </summary>
public sealed class ArticuloService(
    IArticuloRepository articulos,
    IPuntoVentaRepository puntosVenta,
    IStockService stock,
    IUsuarioActual usuarioActual,
    TimeProvider reloj)
{
    public const int MaximoResultados = 100;

    public async Task<IReadOnlyList<ArticuloDisponibleDto>> ListarDisponiblesAsync(string? texto, int? codigoCategoria,
        CancellationToken ct)
    {
        var codigoPuntoVenta = usuarioActual.CodigoPuntoVenta;
        var puntoVenta = await puntosVenta.ObtenerAsync(codigoPuntoVenta, ct);
        if (puntoVenta is not { Activo: true })
            throw new NoEncontradoException($"El punto de venta {codigoPuntoVenta} no existe o está inactivo.");
        if (await articulos.ObtenerCanalAsync(ParametrosVenta.Canal, ct) is not { Activo: true })
            throw new NoEncontradoException($"El canal {ParametrosVenta.Canal} no existe o está inactivo.");

        var fecha = reloj.GetLocalNow().DateTime;
        var candidatos = await articulos.ListarConTarifasVigentesAsync(ParametrosVenta.TipoOrden, ParametrosVenta.Canal,
            codigoCategoria is > 0 ? codigoCategoria : null, string.IsNullOrWhiteSpace(texto) ? null : texto.Trim(),
            fecha, MaximoResultados, ct);

        var stockPorArticulo = await stock.ObtenerStockAsync(
            codigoPuntoVenta, candidatos.Select(c => c.Articulo.Codigo).ToList(), ct);

        // El precio mostrado es el de una unidad; al vender se recalcula según la cantidad (RN-07)
        return candidatos
            .Select(c => (Candidato: c, Tarifa: Tarifa.Elegir(c.Tarifas, 1, fecha)))
            .Where(c => c.Tarifa is not null)
            .Select(c => new ArticuloDisponibleDto(
                c.Candidato.Articulo.Codigo,
                c.Candidato.Articulo.Descripcion?.Trim() ?? c.Candidato.Articulo.Codigo,
                c.Candidato.Articulo.UnidadMedida?.Trim(),
                c.Tarifa!.Precio!.Value,
                stockPorArticulo.GetValueOrDefault(c.Candidato.Articulo.Codigo),
                c.Candidato.Categoria.Codigo,
                DescribirCategoria(c.Candidato.Categoria),
                c.Candidato.TasaIgv))
            .OrderBy(a => a.CodigoCategoria)
            .ThenBy(a => a.Descripcion, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>Categorías de venta que tienen artículos para el tipo de orden y canal de la venta.</summary>
    public async Task<IReadOnlyList<CategoriaVentaDto>> ListarCategoriasAsync(CancellationToken ct)
    {
        var categorias = await articulos.ListarCategoriasAsync(ParametrosVenta.TipoOrden, ParametrosVenta.Canal, ct);
        return categorias
            .OrderBy(c => c.Codigo)
            .Select(c => new CategoriaVentaDto(c.Codigo, DescribirCategoria(c)))
            .ToList();
    }

    private static string DescribirCategoria(CategoriaVenta categoria) =>
        string.IsNullOrWhiteSpace(categoria.Descripcion) ? $"Categoría {categoria.Codigo}" : categoria.Descripcion.Trim();
}

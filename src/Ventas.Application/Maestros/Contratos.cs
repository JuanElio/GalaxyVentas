using Ventas.Domain.Maestros;

namespace Ventas.Application.Maestros;

public interface IClienteRepository
{
    /// <summary>Clientes activos con ese número de documento (puede haber más de uno en datos heredados).</summary>
    Task<IReadOnlyList<Cliente>> BuscarActivosPorDocumentoAsync(string numeroDocumento, CancellationToken ct);

    /// <summary>Clientes activos en los que cada palabra aparece en nombres, apellidos o razón social.</summary>
    Task<IReadOnlyList<Cliente>> BuscarActivosPorNombreAsync(IReadOnlyList<string> palabras, CancellationToken ct);

    Task<bool> ExisteActivoAsync(string tipoDocumento, string numeroDocumento, CancellationToken ct);

    Task<Cliente?> ObtenerActivoAsync(long codigo, CancellationToken ct);

    Task AgregarAsync(Cliente cliente, CancellationToken ct);
}

/// <summary>
/// Artículo activo con su categoría de venta, la tasa de IGV vigente de su código de IVA
/// (fracción: 0.18, o 0 si está exonerado) y sus tarifas vigentes en el canal.
/// </summary>
public sealed record ArticuloConTarifas(Articulo Articulo, IReadOnlyList<Tarifa> Tarifas, CategoriaVenta Categoria, decimal TasaIgv);

/// <summary>
/// Artículos vendibles: activos, asignados a una categoría activa para el tipo de orden y canal
/// (SGP_CATEGORIA_ARTICULO), con tarifa vigente en ese canal y con tasa de IVA vigente.
/// </summary>
public interface IArticuloRepository
{
    /// <summary>
    /// Artículos vendibles a la fecha, filtrados opcionalmente por categoría y por código o
    /// descripción, ordenados por categoría y descripción. Los códigos se devuelven sin espacios.
    /// </summary>
    Task<IReadOnlyList<ArticuloConTarifas>> ListarConTarifasVigentesAsync(
        string tipoOrden, string codigoCanal, int? codigoCategoria, string? texto, DateTime fecha, int maximo, CancellationToken ct);

    /// <summary>Los artículos pedidos que son vendibles a la fecha.</summary>
    Task<IReadOnlyList<ArticuloConTarifas>> ObtenerConTarifasVigentesAsync(
        string tipoOrden, string codigoCanal, IReadOnlyCollection<string> codigosArticulo, DateTime fecha, CancellationToken ct);

    /// <summary>Categorías activas que tienen algún artículo activo asignado para el tipo de orden y canal.</summary>
    Task<IReadOnlyList<CategoriaVenta>> ListarCategoriasAsync(string tipoOrden, string codigoCanal, CancellationToken ct);

    Task<TarifaCanal?> ObtenerCanalAsync(string codigoCanal, CancellationToken ct);
}

public interface IPuntoVentaRepository
{
    Task<PuntoVenta?> ObtenerAsync(string codigo, CancellationToken ct);

    /// <summary>Primer almacén activo (menor código) del punto de venta, o null si no tiene (P-13).</summary>
    Task<string?> ObtenerAlmacenPrincipalAsync(string codigoPuntoVenta, CancellationToken ct);

    Task<ZonaVenta?> ObtenerZonaVentaAsync(string codigoZona, CancellationToken ct);

    Task<TipoOrden?> ObtenerTipoOrdenAsync(string codigo, CancellationToken ct);
}

public interface ICatalogoRepository
{
    /// <summary>Ítems activos de un catálogo activo; vacío si el catálogo no existe o está inactivo.</summary>
    Task<IReadOnlyList<ItemCatalogo>> ListarItemsActivosAsync(int codigoCatalogo, CancellationToken ct);

    Task<IReadOnlyList<Banco>> ListarBancosActivosAsync(CancellationToken ct);
}

/// <summary>Stock calculado sobre PGA_TRANSACCION_KARDEX por artículo y punto de venta (RN-09).</summary>
public interface IStockService
{
    /// <summary>Stock de cada artículo pedido; los que no tienen movimientos no aparecen (stock 0).</summary>
    Task<IReadOnlyDictionary<string, decimal>> ObtenerStockAsync(
        string codigoPuntoVenta, IReadOnlyCollection<string> codigosArticulo, CancellationToken ct);

    /// <summary>
    /// Reserva el stock de esos artículos hasta el fin de la transacción, para que dos ventas
    /// simultáneas no vendan el mismo saldo (RF-08). Requiere una transacción abierta.
    /// </summary>
    Task BloquearAsync(string codigoPuntoVenta, IReadOnlyCollection<string> codigosArticulo, CancellationToken ct);
}

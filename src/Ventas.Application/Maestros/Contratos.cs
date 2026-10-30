using Ventas.Domain.Maestros;

namespace Ventas.Application.Maestros;

public interface IClienteRepository
{
    /// <summary>Clientes activos con ese número de documento (puede haber más de uno en datos heredados).</summary>
    Task<IReadOnlyList<Cliente>> BuscarActivosPorDocumentoAsync(string numeroDocumento, CancellationToken ct);

    Task<bool> ExisteActivoAsync(string tipoDocumento, string numeroDocumento, CancellationToken ct);

    Task<Cliente?> ObtenerActivoAsync(long codigo, CancellationToken ct);

    Task AgregarAsync(Cliente cliente, CancellationToken ct);
}

/// <summary>Artículo activo con sus tarifas vigentes en un canal.</summary>
public sealed record ArticuloConTarifas(Articulo Articulo, IReadOnlyList<Tarifa> Tarifas);

public interface IArticuloRepository
{
    /// <summary>
    /// Artículos activos que tienen al menos una tarifa vigente en el canal a la fecha, filtrados
    /// opcionalmente por código o descripción. Los códigos se devuelven sin espacios.
    /// </summary>
    Task<IReadOnlyList<ArticuloConTarifas>> ListarConTarifasVigentesAsync(
        string codigoCanal, string? texto, DateTime fecha, int maximo, CancellationToken ct);

    /// <summary>Los artículos pedidos que están activos y tienen tarifa vigente en el canal.</summary>
    Task<IReadOnlyList<ArticuloConTarifas>> ObtenerConTarifasVigentesAsync(
        string codigoCanal, IReadOnlyCollection<string> codigosArticulo, DateTime fecha, CancellationToken ct);

    Task<TarifaCanal?> ObtenerCanalAsync(string codigoCanal, CancellationToken ct);
}

public interface IPuntoVentaRepository
{
    Task<PuntoVenta?> ObtenerAsync(string codigo, CancellationToken ct);

    /// <summary>Primer almacén activo (menor código) del punto de venta, o null si no tiene (P-13).</summary>
    Task<string?> ObtenerAlmacenPrincipalAsync(string codigoPuntoVenta, CancellationToken ct);
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
}

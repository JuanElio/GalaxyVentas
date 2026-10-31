namespace Ventas.Domain.Maestros;

/// <summary>Asigna un artículo a una categoría de venta para un tipo de orden y canal.</summary>
public class CategoriaArticulo
{
    private bool? _activo;

    private CategoriaArticulo()
    {
        CodigoArticulo = CodigoCanal = null!;
    }

    public CategoriaArticulo(int codigoCategoria, string codigoArticulo, string? tipoOrden, string codigoCanal, bool activo)
    {
        CodigoCategoria = codigoCategoria;
        CodigoArticulo = codigoArticulo;
        TipoOrden = tipoOrden;
        CodigoCanal = codigoCanal;
        _activo = activo;
    }

    public int CodigoCategoria { get; private set; }

    public string CodigoArticulo { get; private set; }

    public string? TipoOrden { get; private set; }

    public string CodigoCanal { get; private set; }

    public bool Activo => _activo == true;
}

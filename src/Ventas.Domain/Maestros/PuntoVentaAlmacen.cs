namespace Ventas.Domain.Maestros;

/// <summary>Almacén asociado a un punto de venta (SGP_PUNTO_VENTA_ALMACEN).</summary>
public class PuntoVentaAlmacen
{
    private bool? _activo;

    private PuntoVentaAlmacen()
    {
        CodigoAlmacen = CodigoPuntoVenta = null!;
    }

    public PuntoVentaAlmacen(string codigoAlmacen, string codigoPuntoVenta, bool activo)
    {
        CodigoAlmacen = codigoAlmacen;
        CodigoPuntoVenta = codigoPuntoVenta;
        _activo = activo;
    }

    public string CodigoAlmacen { get; private set; }

    public string CodigoPuntoVenta { get; private set; }

    public bool Activo => _activo == true;
}

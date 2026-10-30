namespace Ventas.Domain.Maestros;

/// <summary>Zona de venta (SGP_ZONA_VENTA) a la que pertenece un punto de venta.</summary>
public class ZonaVenta
{
    private ZonaVenta()
    {
        Codigo = null!;
    }

    public ZonaVenta(string codigo, string? descripcion)
    {
        Codigo = codigo;
        Descripcion = descripcion;
    }

    public string Codigo { get; private set; }

    public string? Descripcion { get; private set; }
}

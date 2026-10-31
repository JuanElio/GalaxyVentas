namespace Ventas.Domain.Maestros;

/// <summary>Categoría con la que se agrupan los artículos en la venta (Bienes, Libros, Periódicos...).</summary>
public class CategoriaVenta
{
    private bool? _activo;

    private CategoriaVenta()
    {
    }

    public CategoriaVenta(int codigo, string? descripcion, bool activo)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        _activo = activo;
    }

    public int Codigo { get; private set; }

    public string? Descripcion { get; private set; }

    public bool Activo => _activo == true;
}

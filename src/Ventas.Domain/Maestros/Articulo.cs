namespace Ventas.Domain.Maestros;

public class Articulo
{
    private bool? _activo;

    private Articulo()
    {
        Codigo = null!;
    }

    public Articulo(string codigo, string? descripcion, string? unidadMedida, bool activo)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        UnidadMedida = unidadMedida;
        _activo = activo;
    }

    public string Codigo { get; private set; }

    public string? Descripcion { get; private set; }

    public string? UnidadMedida { get; private set; }

    public bool Activo => _activo == true;
}

namespace Ventas.Domain.Maestros;

public class Banco
{
    private bool? _activo;

    private Banco()
    {
        Codigo = null!;
    }

    public Banco(string codigo, string? descripcion, bool activo)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        _activo = activo;
    }

    public string Codigo { get; private set; }

    public string? Descripcion { get; private set; }

    public bool Activo => _activo == true;
}

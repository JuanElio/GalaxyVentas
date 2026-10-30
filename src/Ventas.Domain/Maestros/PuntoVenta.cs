namespace Ventas.Domain.Maestros;

public class PuntoVenta
{
    private bool? _activo;

    private PuntoVenta()
    {
        Codigo = null!;
    }

    public PuntoVenta(string codigo, string? descripcion, string? zonaVenta, bool activo)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        ZonaVenta = zonaVenta;
        _activo = activo;
    }

    /// <summary>Código tal como está en la BD (con espacio inicial, p. ej. ' 001').</summary>
    public string Codigo { get; private set; }

    public string? Descripcion { get; private set; }

    public string? ZonaVenta { get; private set; }

    public bool Activo => _activo == true;
}

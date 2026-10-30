namespace Ventas.Domain.Maestros;

/// <summary>Canal (lista de precios) de SGP_TARIFA_CANAL.</summary>
public class TarifaCanal
{
    private bool? _activo;

    private TarifaCanal()
    {
        Codigo = null!;
    }

    public TarifaCanal(string codigo, string? descripcion, bool activo)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        _activo = activo;
    }

    /// <summary>Código tal como está en la BD (con espacio inicial, p. ej. ' 01').</summary>
    public string Codigo { get; private set; }

    public string? Descripcion { get; private set; }

    public bool Activo => _activo == true;
}

namespace Ventas.Domain.Maestros;

public class Articulo
{
    private bool? _activo;

    private Articulo()
    {
        Codigo = null!;
    }

    public Articulo(string codigo, string? descripcion, string? unidadMedida, bool activo, string? codigoIva = null)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        UnidadMedida = unidadMedida;
        CodigoIva = codigoIva;
        _activo = activo;
    }

    public string Codigo { get; private set; }

    public string? Descripcion { get; private set; }

    public string? UnidadMedida { get; private set; }

    /// <summary>Código de IVA (SGP_IVA); su tasa vigente está en <see cref="DetalleIva"/>.</summary>
    public string? CodigoIva { get; private set; }

    public bool Activo => _activo == true;
}

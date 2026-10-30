namespace Ventas.Domain.Maestros;

/// <summary>
/// Precio de un artículo en un canal para un tramo de cantidad: <see cref="CantidadMaxima"/>
/// es el tope del tramo (numCantidad).
/// </summary>
public class Tarifa
{
    private bool? _activo;

    private Tarifa()
    {
        CodigoArticulo = CodigoCanal = null!;
    }

    public Tarifa(string codigoArticulo, string codigoCanal, decimal cantidadMaxima, decimal? precio,
        DateTime inicioVigencia, DateTime finVigencia, bool activo)
    {
        CodigoArticulo = codigoArticulo;
        CodigoCanal = codigoCanal;
        CantidadMaxima = cantidadMaxima;
        Precio = precio;
        InicioVigencia = inicioVigencia;
        FinVigencia = finVigencia;
        _activo = activo;
    }

    public string CodigoArticulo { get; private set; }

    public string CodigoCanal { get; private set; }

    public decimal CantidadMaxima { get; private set; }

    /// <summary>Precio unitario con IGV (P-15).</summary>
    public decimal? Precio { get; private set; }

    public DateTime InicioVigencia { get; private set; }

    public DateTime FinVigencia { get; private set; }

    public bool Activo => _activo == true;

    /// <summary>
    /// Vigente si inicio ≤ fecha y la fecha no pasa del día de caducidad (incluido, como en
    /// el procedimiento de referencia) (RN-06).
    /// </summary>
    public bool EstaVigente(DateTime fecha) =>
        Activo && Precio is not null && InicioVigencia <= fecha && fecha < FinVigencia.Date.AddDays(1);

    /// <summary>
    /// Elige la tarifa de una cantidad (RN-07, P-06): entre las vigentes, la de menor tope que
    /// cubra la cantidad; si hay varias para el mismo tope, la de vigencia más reciente.
    /// Devuelve null si ninguna cubre la cantidad.
    /// </summary>
    public static Tarifa? Elegir(IEnumerable<Tarifa> tarifas, decimal cantidad, DateTime fecha) =>
        tarifas
            .Where(t => t.EstaVigente(fecha) && t.CantidadMaxima >= cantidad)
            .OrderBy(t => t.CantidadMaxima)
            .ThenByDescending(t => t.InicioVigencia)
            .FirstOrDefault();
}

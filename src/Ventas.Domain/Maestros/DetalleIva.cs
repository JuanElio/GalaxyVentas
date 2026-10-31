namespace Ventas.Domain.Maestros;

/// <summary>
/// Porcentaje de IVA de un código a partir de una fecha de vigencia. <see cref="Porcentaje"/> se
/// guarda como porcentaje (18.0000 = 18 %).
/// </summary>
public class DetalleIva
{
    private bool? _activo;

    private DetalleIva()
    {
        CodigoIva = null!;
    }

    public DetalleIva(string codigoIva, DateTime fechaVigencia, decimal? porcentaje, bool activo)
    {
        CodigoIva = codigoIva;
        FechaVigencia = fechaVigencia;
        Porcentaje = porcentaje;
        _activo = activo;
    }

    public string CodigoIva { get; private set; }

    public DateTime FechaVigencia { get; private set; }

    public decimal? Porcentaje { get; private set; }

    public bool Activo => _activo == true;

    /// <summary>Tasa como fracción (18 % → 0.18).</summary>
    public decimal? Tasa => Porcentaje / 100m;

    /// <summary>
    /// Tasa vigente a la fecha: la del registro activo con la fecha de vigencia más reciente que no
    /// la supere (como pga_obtener_impuesto_iva). Null si el código no tiene ninguna.
    /// </summary>
    public static decimal? TasaVigente(IEnumerable<DetalleIva> detalles, DateTime fecha) =>
        detalles
            .Where(d => d.Activo && d.Porcentaje is not null && d.FechaVigencia <= fecha)
            .OrderByDescending(d => d.FechaVigencia)
            .FirstOrDefault()?.Tasa;
}

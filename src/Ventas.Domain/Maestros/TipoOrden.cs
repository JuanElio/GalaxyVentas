namespace Ventas.Domain.Maestros;

/// <summary>Tipo de orden (SGP_TIPO_ORDEN); define el tipo de liquidación de la venta.</summary>
public class TipoOrden
{
    private bool? _activo;

    private TipoOrden()
    {
        Codigo = null!;
    }

    public TipoOrden(string codigo, string? descripcion, int? codigoTipoLiquidacion, bool activo)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        CodigoTipoLiquidacion = codigoTipoLiquidacion;
        _activo = activo;
    }

    public string Codigo { get; private set; }

    public string? Descripcion { get; private set; }

    public int? CodigoTipoLiquidacion { get; private set; }

    public bool Activo => _activo == true;

    /// <summary>Tipo de liquidación como lo graba el procedimiento de referencia (texto vacío si no tiene).</summary>
    public string TipoLiquidacion => CodigoTipoLiquidacion?.ToString() ?? string.Empty;
}

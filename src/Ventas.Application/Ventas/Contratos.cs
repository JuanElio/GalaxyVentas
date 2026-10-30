namespace Ventas.Application.Ventas;

/// <summary>Correlativos de los comprobantes guardados en SGP_IMPRESORA (RN-13).</summary>
public interface ICorrelativoService
{
    /// <summary>
    /// Próximo número de la serie sin reservarlo (solo para mostrarlo); null si no hay una
    /// impresora activa con esa serie.
    /// </summary>
    Task<int?> ConsultarSiguienteAsync(string serie, CancellationToken ct);
}

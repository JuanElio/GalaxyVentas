using Ventas.Domain.Comprobantes;
using Ventas.Domain.Inventario;
using Ventas.Domain.Ventas;

namespace Ventas.Application.Ventas;

/// <summary>Correlativos de los comprobantes guardados en SGP_IMPRESORA (RN-13).</summary>
public interface ICorrelativoService
{
    /// <summary>
    /// Próximo número de la serie sin reservarlo (solo para mostrarlo); null si no hay una
    /// impresora activa con esa serie.
    /// </summary>
    Task<int?> ConsultarSiguienteAsync(string serie, CancellationToken ct);

    /// <summary>
    /// Impresora activa de la serie, bloqueada hasta el fin de la transacción para que dos ventas
    /// no tomen el mismo número (RF-11). Null si no existe. Requiere una transacción abierta.
    /// </summary>
    Task<Impresora?> ObtenerParaEmitirAsync(string serie, CancellationToken ct);
}

public interface IVentaRepository
{
    Task AgregarAsync(Venta venta, CancellationToken ct);
}

public interface IComprobanteRepository
{
    /// <summary>Agrega el comprobante con su detalle, sus enlaces a la venta y sus pagos.</summary>
    Task AgregarAsync(Comprobante comprobante, CancellationToken ct);
}

public interface IKardexRepository
{
    Task AgregarAsync(IEnumerable<MovimientoKardex> movimientos, CancellationToken ct);
}

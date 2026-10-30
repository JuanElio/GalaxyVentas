using Microsoft.EntityFrameworkCore;
using Ventas.Application.Ventas;
using Ventas.Domain.Comprobantes;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Ventas;

/// <summary>
/// La impresora se busca solo por vchNumSerie: su vchTipComprobante no es fiable (RF-11).
/// </summary>
public sealed class CorrelativoService(VentasDbContext db) : ICorrelativoService
{
    public async Task<int?> ConsultarSiguienteAsync(string serie, CancellationToken ct)
    {
        var impresora = await db.Set<Impresora>().AsNoTracking()
            .Where(i => i.Serie == serie && EF.Property<bool?>(i, Conversiones.CampoActivo) == true)
            .OrderBy(i => i.Id)
            .FirstOrDefaultAsync(ct);

        return impresora?.SiguienteNumero;
    }

    /// <summary>
    /// Lee la impresora con UPDLOCK: otra venta de la misma serie espera hasta el commit, así que
    /// nunca dos comprobantes toman el mismo número y, si la venta falla, el correlativo no avanza.
    /// </summary>
    public async Task<Impresora?> ObtenerParaEmitirAsync(string serie, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Tomar un correlativo requiere una transacción abierta.");

        return await db.Set<Impresora>()
            .FromSql($"SELECT * FROM SGP_IMPRESORA WITH (UPDLOCK, ROWLOCK) WHERE vchNumSerie = {serie}")
            .Where(i => EF.Property<bool?>(i, Conversiones.CampoActivo) == true)
            .OrderBy(i => i.Id)
            .FirstOrDefaultAsync(ct);
    }
}

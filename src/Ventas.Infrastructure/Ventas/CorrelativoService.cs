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
}

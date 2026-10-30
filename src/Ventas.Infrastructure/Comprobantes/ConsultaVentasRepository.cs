using Microsoft.EntityFrameworkCore;
using Ventas.Application.Comprobantes;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Infrastructure.Comprobantes;

/// <summary>Lecturas de solo consulta (sin seguimiento de cambios) de ventas y comprobantes.</summary>
public sealed class ConsultaVentasRepository(VentasDbContext db) : IConsultaVentasRepository
{
    private IQueryable<Comprobante> ComprobantesActivos =>
        db.Set<Comprobante>().AsNoTracking()
            .Where(c => EF.Property<string>(c, Conversiones.PropiedadSituacion) == "A");

    public async Task<Comprobante?> ObtenerComprobanteAsync(long codigo, CancellationToken ct) =>
        await ComprobantesActivos
            .Include(c => c.Detalles)
            .Include(c => c.Movimientos)
            .Include(c => c.Pagos).ThenInclude(p => p.MedioPago)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == codigo, ct);

    public async Task<long?> BuscarCodigoComprobanteAsync(string serie, string numero, CancellationToken ct) =>
        await ComprobantesActivos
            .Where(c => c.Serie == serie && c.Numero == numero)
            .OrderByDescending(c => c.Id)
            .Select(c => (long?)c.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<VentaRegistrada?> ObtenerVentaAsync(long codigoTransaccion, CancellationToken ct)
    {
        var registro = await db.Set<Venta>().AsNoTracking()
            .Where(v => v.Id == codigoTransaccion)
            .Select(v => new { Venta = v, Usuario = EF.Property<string>(v, Auditoria.Creador) })
            .FirstOrDefaultAsync(ct);

        return registro is null ? null : new VentaRegistrada(registro.Venta, registro.Usuario);
    }

    public async Task<IReadOnlyList<FilaVentaRegistrada>> ListarAsync(FiltroVentasRegistradas filtro, CancellationToken ct)
    {
        // El conversor del punto de venta compara con el formato de la BD (' 001')
        var puntoVenta = filtro.CodigoPuntoVenta.Trim();
        var consulta = ComprobantesActivos.Where(c => c.CodigoPuntoVenta == puntoVenta);

        if (filtro.Desde is { } desde)
            consulta = consulta.Where(c => c.FechaEmision >= desde.Date);
        if (filtro.Hasta is { } hasta)
            consulta = consulta.Where(c => c.FechaEmision < hasta.Date.AddDays(1));
        if (filtro.TipoComprobante is { } tipo)
            consulta = consulta.Where(c => c.TipoComprobante == tipo);
        if (filtro.MedioPago is { } medio)
            consulta = consulta.Where(c => c.Pagos.Any(p => p.MedioPago.Tipo == medio));
        if (filtro.Usuario is { } usuario)
            consulta = consulta.Where(c => EF.Property<string>(c, Auditoria.Creador) == usuario);

        consulta = filtro.Maximo is { } maximo
            ? consulta.OrderByDescending(c => c.FechaEmision).ThenByDescending(c => c.Id).Take(maximo)
            : consulta.OrderBy(c => c.FechaEmision).ThenBy(c => c.Id);

        return await consulta
            .Select(c => new FilaVentaRegistrada(
                c.Id,
                c.Movimientos.Select(m => (long?)m.CodigoTransaccion).FirstOrDefault(),
                c.FechaEmision,
                c.TipoComprobante,
                c.Serie,
                c.Numero,
                c.TipoDocumento,
                c.NumeroDocumento,
                c.NombreCliente,
                c.SubTotal,
                c.MontoIgv,
                c.MontoTotal,
                c.Pagos.Select(p => (TipoMedioPago?)p.MedioPago.Tipo).FirstOrDefault(),
                c.Estado,
                EF.Property<string>(c, Auditoria.Creador)))
            .ToListAsync(ct);
    }
}

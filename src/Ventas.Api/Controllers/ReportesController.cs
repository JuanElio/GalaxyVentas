using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Api.Models;
using Ventas.Application.Maestros;
using Ventas.Application.Reportes;
using Ventas.Application.Seguridad;
using Ventas.Domain.Maestros;

namespace Ventas.Api.Controllers;

/// <summary>Reporte de ventas registradas del punto de venta de la sesión (RF-22).</summary>
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class ReportesController(
    ReporteVentasService reporte,
    CatalogoService catalogos,
    IUsuarioActual usuarioActual,
    TimeProvider reloj) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Ventas(DateTime? desde, DateTime? hasta, string? tipoComprobante, int? medioPago,
        string? usuario, CancellationToken ct)
    {
        // Sin filtros (primera visita): el reporte del día
        var primeraVisita = Request.Query.Count == 0;
        var hoy = reloj.GetLocalNow().Date;
        var filtro = primeraVisita
            ? new FiltroReporteVentasDto(hoy, hoy)
            : new FiltroReporteVentasDto(desde, hasta, tipoComprobante, medioPago, usuario);

        ReporteVentasDto? resultado = null;
        IReadOnlyList<string> errores = [];
        try
        {
            resultado = await reporte.GenerarAsync(filtro, ct);
        }
        catch (ValidationException ex)
        {
            errores = ex.Errors.Select(e => e.ErrorMessage).ToList();
        }

        return View(new ReporteVentasViewModel
        {
            Filtro = filtro,
            Reporte = resultado,
            Errores = errores,
            TiposComprobante = await catalogos.ListarAsync(Catalogos.TipoComprobante, ct),
            MediosPago = await catalogos.ListarMediosPagoAsync(ct),
            Usuarios = await reporte.ListarUsuariosAsync(ct),
            PuntoVenta = usuarioActual.CodigoPuntoVenta,
        });
    }
}

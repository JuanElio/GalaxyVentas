using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Api.Models;
using Ventas.Application.Comprobantes;
using Ventas.Application.Comun;
using Ventas.Domain.Comun;

namespace Ventas.Api.Controllers;

/// <summary>Consulta de una venta por transacción (RF-16) o de un comprobante por tipo y número (RF-17).</summary>
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class ConsultasController(ConsultaVentasService consultas) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(long? transaccion, string? tipoComprobante, string? numero, CancellationToken ct)
    {
        var porComprobante = !string.IsNullOrWhiteSpace(numero);
        string? error = null;

        if (transaccion is not null || porComprobante)
        {
            try
            {
                var codigo = porComprobante
                    ? await consultas.BuscarComprobanteAsync(tipoComprobante, numero, ct)
                    : await consultas.BuscarComprobanteDeVentaAsync(transaccion!.Value, ct);
                return RedirectToAction(nameof(ComprobantesController.Ver), "Comprobantes", new { id = codigo });
            }
            catch (Exception ex) when (ex is NoEncontradoException or AccesoDenegadoException or DominioException)
            {
                // En la búsqueda el error se muestra en el mismo formulario
                error = ex.Message;
            }
        }

        return View(new ConsultasViewModel
        {
            Transaccion = transaccion,
            TipoComprobante = tipoComprobante,
            Numero = numero,
            Pestana = porComprobante ? "comprobante" : "venta",
            Error = error,
            Recientes = await consultas.ListarRecientesAsync(ct),
        });
    }
}

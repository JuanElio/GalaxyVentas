using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Comprobantes;

namespace Ventas.Api.Controllers;

/// <summary>Vista del comprobante emitido (RF-17).</summary>
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class ComprobantesController(ConsultaVentasService consultas) : Controller
{
    [HttpGet("/Comprobantes/Ver/{id:long}")]
    public async Task<IActionResult> Ver(long id, bool nuevo, CancellationToken ct)
    {
        ViewData["Nuevo"] = nuevo;
        return View(await consultas.ObtenerComprobanteAsync(id, ct));
    }
}

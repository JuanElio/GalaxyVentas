using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Maestros;

namespace Ventas.Api.Controllers;

/// <summary>Acciones JSON de artículos que invoca jQuery (RF-01).</summary>
[Authorize]
[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class ArticulosController(ArticuloService articulos) : Controller
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArticuloDisponibleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disponibles(string? texto, CancellationToken ct) =>
        Json(await articulos.ListarDisponiblesAsync(texto, ct));
}

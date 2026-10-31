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
    public async Task<IActionResult> Disponibles(string? texto, int? categoria, CancellationToken ct) =>
        Json(await articulos.ListarDisponiblesAsync(texto, categoria, ct));

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoriaVentaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Categorias(CancellationToken ct) =>
        Json(await articulos.ListarCategoriasAsync(ct));
}

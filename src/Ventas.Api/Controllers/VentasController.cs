using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Ventas;

namespace Ventas.Api.Controllers;

/// <summary>Pantallas de ventas. Las acciones JSON que invoca jQuery están en <see cref="VentasApiController"/>.</summary>
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class VentasController(VentaService ventas) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Registrar(CancellationToken ct) =>
        View(await ventas.ObtenerContextoAsync(ct));
}

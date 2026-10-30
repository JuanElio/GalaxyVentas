using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;

namespace Ventas.Api.Controllers;

/// <summary>Catálogos activos para los combos de la pantalla de venta (RF-04).</summary>
[Authorize]
[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
[ProducesResponseType<IReadOnlyList<ItemCatalogoDto>>(StatusCodes.Status200OK)]
public class CatalogosController(CatalogoService catalogos) : Controller
{
    [HttpGet]
    public async Task<IActionResult> TiposDocumento(CancellationToken ct) =>
        Json(await catalogos.ListarAsync(Catalogos.TipoDocumento, ct));

    [HttpGet]
    public async Task<IActionResult> TiposComprobante(CancellationToken ct) =>
        Json(await catalogos.ListarAsync(Catalogos.TipoComprobante, ct));

    [HttpGet]
    public async Task<IActionResult> MediosPago(CancellationToken ct) =>
        Json(await catalogos.ListarMediosPagoAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Bancos(CancellationToken ct) =>
        Json(await catalogos.ListarBancosAsync(ct));
}

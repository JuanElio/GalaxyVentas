using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Ventas;

namespace Ventas.Api.Controllers;

/// <summary>Acciones JSON de la pantalla de venta que invoca jQuery (comparten la ruta /Ventas).</summary>
[Authorize]
[ApiController]
[Route("Ventas")]
[Produces("application/json")]
public class VentasApiController(VentaService ventas) : Controller
{
    /// <summary>Importes calculados por el dominio para las líneas de la pantalla (RF-06, RF-07, RF-08).</summary>
    [HttpPost("Cotizar")]
    [ProducesResponseType<CotizacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cotizar(CotizarVentaDto datos, CancellationToken ct) =>
        Ok(await ventas.CotizarAsync(datos, ct));

    /// <summary>Serie y próximo número del comprobante, solo para mostrarlo (RN-13).</summary>
    [HttpGet("SiguienteNumero")]
    [ProducesResponseType<NumeracionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SiguienteNumero(string? tipoComprobante, CancellationToken ct) =>
        Ok(await ventas.ConsultarNumeracionAsync(tipoComprobante, ct));
}

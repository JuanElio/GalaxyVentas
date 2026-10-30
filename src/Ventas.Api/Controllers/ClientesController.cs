using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Maestros;

namespace Ventas.Api.Controllers;

/// <summary>Acciones JSON de clientes que invoca jQuery (RF-02, RF-03).</summary>
[Authorize]
[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class ClientesController(ClienteService clientes) : Controller
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClienteDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar(string? numeroDocumento, CancellationToken ct) =>
        Json(await clientes.BuscarPorDocumentoAsync(numeroDocumento, ct));

    [HttpPost]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarClienteDto datos, CancellationToken ct)
    {
        var cliente = await clientes.RegistrarAsync(datos, ct);
        return StatusCode(StatusCodes.Status201Created, cliente);
    }
}

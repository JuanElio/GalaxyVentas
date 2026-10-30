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
    /// <summary>Por número de documento si el texto son solo dígitos; si no, por nombre o razón social.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClienteDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar(string? texto, CancellationToken ct) =>
        Json(await clientes.BuscarAsync(texto, ct));

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

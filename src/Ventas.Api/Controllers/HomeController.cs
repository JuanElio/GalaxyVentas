using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ventas.Api.Comun;
using Ventas.Api.Models;

namespace Ventas.Api.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    public IActionResult Index() => View();

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var idSolicitud = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        if (HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error is not { } excepcion)
            return View(new ErrorViewModel(idSolicitud));

        // Errores esperados (no encontrado, acceso denegado...): se muestra su mensaje; un error inesperado, no
        var problema = ManejadorExcepciones.CrearProblema(excepcion);
        var status = problema.Status ?? StatusCodes.Status500InternalServerError;
        return status == StatusCodes.Status500InternalServerError
            ? View(new ErrorViewModel(idSolicitud))
            : View(new ErrorViewModel(idSolicitud, status, problema.Title, problema.Detail));
    }
}

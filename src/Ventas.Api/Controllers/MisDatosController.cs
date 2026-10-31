using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Api.Models;
using Ventas.Application.Seguridad;

namespace Ventas.Api.Controllers;

/// <summary>Datos del usuario de la sesión y cambio de su contraseña.</summary>
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class MisDatosController(MisDatosService misDatos) : Controller
{
    public const string MensajeClaveCambiada = "Su contraseña se cambió correctamente.";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new MisDatosViewModel { Datos = await misDatos.ObtenerAsync(ct) });

    [HttpPost]
    public async Task<IActionResult> CambiarClave(MisDatosViewModel modelo, CancellationToken ct)
    {
        try
        {
            await misDatos.CambiarClaveAsync(
                new CambiarClaveDto(modelo.ClaveActual ?? string.Empty, modelo.ClaveNueva ?? string.Empty,
                    modelo.ConfirmacionClave ?? string.Empty), ct);
        }
        catch (ValidationException ex)
        {
            // En el formulario el error se muestra en la misma vista, junto a cada campo
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return View(nameof(Index), new MisDatosViewModel { Datos = await misDatos.ObtenerAsync(ct) });
        }

        // Post/Redirect/Get: recargar la página no vuelve a enviar las contraseñas
        TempData["Mensaje"] = MensajeClaveCambiada;
        return RedirectToAction(nameof(Index));
    }
}

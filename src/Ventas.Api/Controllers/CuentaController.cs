using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Api.Models;
using Ventas.Api.Seguridad;
using Ventas.Application.Comun;
using Ventas.Application.Seguridad;

namespace Ventas.Api.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class CuentaController(AutenticacionService autenticacion) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult IniciarSesion(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirigirAlInicio(returnUrl);

        return View(new IniciarSesionViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> IniciarSesion(IniciarSesionViewModel modelo, CancellationToken ct)
    {
        SesionDto sesion;
        try
        {
            sesion = await autenticacion.IniciarSesionAsync(new IniciarSesionDto(modelo.Login, modelo.Clave), ct);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return VistaConError(modelo);
        }
        catch (Exception ex) when (ex is NoAutenticadoException or AccesoDenegadoException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return VistaConError(modelo);
        }

        var esquema = CookieAuthenticationDefaults.AuthenticationScheme;
        await HttpContext.SignInAsync(esquema, ClaimsSesion.CrearPrincipal(sesion, esquema));

        return RedirigirAlInicio(modelo.ReturnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> CerrarSesion()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(IniciarSesion));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private ViewResult VistaConError(IniciarSesionViewModel modelo)
    {
        // La contraseña nunca se devuelve a la vista
        modelo.Clave = string.Empty;
        return View(modelo);
    }

    // Tras iniciar sesión se va a la pantalla de registrar venta (RF-21)
    private IActionResult RedirigirAlInicio(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(VentasController.Registrar), "Ventas");
}

using System.Security.Claims;
using Ventas.Application.Seguridad;

namespace Ventas.Api.Seguridad;

/// <summary>Claims que guarda la cookie de sesión: login, nombre y punto de venta (RF-21).</summary>
public static class ClaimsSesion
{
    public const string Nombre = "nombre";

    public const string PuntoVenta = "punto_venta";

    public static ClaimsPrincipal CrearPrincipal(SesionDto sesion, string esquema) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, sesion.Login),
            new Claim(Nombre, sesion.Nombre),
            new Claim(PuntoVenta, sesion.CodigoPuntoVenta),
        ], esquema));
}

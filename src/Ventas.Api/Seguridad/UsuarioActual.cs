using System.Security.Claims;
using Ventas.Application.Seguridad;

namespace Ventas.Api.Seguridad;

/// <summary>
/// Usuario de la sesión leído de los claims de la cookie; nunca de los datos de la petición (RN-19).
/// </summary>
public sealed class UsuarioActual(IHttpContextAccessor httpContextAccessor) : IUsuarioActual
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool EstaAutenticado => Principal?.Identity?.IsAuthenticated == true;

    public string Login => Leer(ClaimTypes.Name);

    public string Nombre => Leer(ClaimsSesion.Nombre);

    public string CodigoPuntoVenta => Leer(ClaimsSesion.PuntoVenta);

    private string Leer(string tipo) =>
        Principal?.FindFirstValue(tipo)
        ?? throw new InvalidOperationException($"La sesión no tiene el dato '{tipo}'.");
}

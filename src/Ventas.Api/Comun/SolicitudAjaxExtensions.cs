using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ventas.Api.Comun;

public static class SolicitudAjaxExtensions
{
    /// <summary>
    /// Indica si la petición la hace jQuery (cabecera X-Requested-With) o espera JSON,
    /// para responder con ProblemDetails en lugar de una página o una redirección.
    /// </summary>
    public static bool EsAjax(this HttpRequest request) =>
        request.Headers.XRequestedWith == "XMLHttpRequest"
        || request.Headers.Accept.Any(valor => valor?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);

    /// <summary>
    /// Escribe un ProblemDetails (RFC 7807) como application/problem+json. Se escribe directamente
    /// porque IProblemDetailsService deja vacía la respuesta de un challenge 401/403 en .NET 10.
    /// </summary>
    public static async Task EscribirProblemaAsync(this HttpContext httpContext, ProblemDetails problema)
    {
        problema.Status ??= StatusCodes.Status500InternalServerError;
        problema.Instance ??= httpContext.Request.Path;
        problema.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problema.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problema, problema.GetType(), options: null,
            contentType: "application/problem+json", httpContext.RequestAborted);
    }
}

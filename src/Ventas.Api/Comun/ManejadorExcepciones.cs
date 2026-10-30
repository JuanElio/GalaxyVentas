using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Comun;
using Ventas.Domain.Comun;

namespace Ventas.Api.Comun;

/// <summary>
/// Traduce las excepciones de las llamadas AJAX a ProblemDetails (RFC 7807). Las peticiones
/// de página no se tratan aquí: siguen a la página de error de UseExceptionHandler.
/// </summary>
public sealed class ManejadorExcepciones(ILogger<ManejadorExcepciones> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problema = CrearProblema(exception);

        if (problema.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado en {Metodo} {Ruta}", httpContext.Request.Method, httpContext.Request.Path);

        if (!httpContext.Request.EsAjax())
            return false;

        await httpContext.EscribirProblemaAsync(problema);
        return true;
    }

    public static ProblemDetails CrearProblema(Exception exception) => exception switch
    {
        ValidationException validacion => new ValidationProblemDetails(
            validacion.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Los datos enviados no son válidos.",
        },
        DominioException => Problema(StatusCodes.Status422UnprocessableEntity, "No se puede completar la operación.", exception.Message),
        ConflictoException => Problema(StatusCodes.Status409Conflict, "La operación entra en conflicto con los datos actuales.", exception.Message),
        NoAutenticadoException => Problema(StatusCodes.Status401Unauthorized, "No autenticado.", exception.Message),
        NoEncontradoException => Problema(StatusCodes.Status404NotFound, "No se encontró el recurso solicitado.", exception.Message),
        AccesoDenegadoException => Problema(StatusCodes.Status403Forbidden, "No tiene acceso a este recurso.", exception.Message),
        // El detalle de un error inesperado no se expone al cliente
        _ => Problema(StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado.", "Intente nuevamente o contacte al administrador."),
    };

    private static ProblemDetails Problema(int status, string titulo, string detalle) =>
        new() { Status = status, Title = titulo, Detail = detalle };
}

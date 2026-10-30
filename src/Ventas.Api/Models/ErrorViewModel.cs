namespace Ventas.Api.Models;

/// <summary>Datos de la página de error; el detalle solo se muestra para errores esperados (no 500).</summary>
public sealed record ErrorViewModel(string IdSolicitud, int Status = 500, string? Titulo = null, string? Detalle = null);

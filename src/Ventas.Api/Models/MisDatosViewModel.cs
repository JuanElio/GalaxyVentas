using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ventas.Application.Seguridad;

namespace Ventas.Api.Models;

public sealed class MisDatosViewModel
{
    [BindNever]
    public MisDatosDto? Datos { get; set; }

    public string ClaveActual { get; set; } = string.Empty;

    public string ClaveNueva { get; set; } = string.Empty;

    public string ConfirmacionClave { get; set; } = string.Empty;
}

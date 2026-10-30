namespace Ventas.Api.Models;

public sealed class IniciarSesionViewModel
{
    public string Login { get; set; } = string.Empty;

    public string Clave { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

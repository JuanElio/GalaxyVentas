using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Ventas.Application.Seguridad;
using Ventas.Domain.Seguridad;

namespace Ventas.IntegrationTests;

/// <summary>
/// Levanta la aplicación con usuarios en memoria en lugar de SGP_USUARIO.
/// </summary>
public sealed partial class AplicacionPrueba : WebApplicationFactory<Program>
{
    public const string Clave = "Ventas2026";

    public UsuariosEnMemoria Usuarios { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IUsuarioRepository>(Usuarios);
            services.AddSingleton<IVerificadorClave>(Usuarios);
        });

    public HttpClient CrearCliente() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost"),
    });

    /// <summary>Extrae el token antifalsificación del formulario de una página.</summary>
    public static async Task<string> ObtenerTokenAsync(HttpClient cliente, string ruta)
    {
        var html = await cliente.GetStringAsync(ruta);
        return TokenRegex().Match(html) is { Success: true } coincidencia
            ? coincidencia.Groups[1].Value
            : throw new InvalidOperationException($"La página {ruta} no tiene token antifalsificación.");
    }

    /// <summary>Inicia sesión con el formulario y devuelve la respuesta del POST.</summary>
    public static async Task<HttpResponseMessage> IniciarSesionAsync(HttpClient cliente, string login, string clave = Clave)
    {
        var token = await ObtenerTokenAsync(cliente, "/Cuenta/IniciarSesion");
        return await cliente.PostAsync("/Cuenta/IniciarSesion", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Login"] = login,
            ["Clave"] = clave,
            ["__RequestVerificationToken"] = token,
        }));
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}

public sealed class UsuariosEnMemoria : IUsuarioRepository, IVerificadorClave
{
    private readonly ConcurrentDictionary<string, Usuario> _usuarios = new(StringComparer.OrdinalIgnoreCase);

    public Usuario Agregar(string login, string? puntoVenta = "001", bool activo = true, DateTime? expiraClave = null)
    {
        var usuario = new Usuario(login, $"Operador {login}", puntoVenta, activo,
            DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1), expiraClave);
        _usuarios[login] = usuario;
        return usuario;
    }

    public Task<Usuario?> ObtenerPorLoginAsync(string login, CancellationToken ct) =>
        Task.FromResult(_usuarios.GetValueOrDefault(login));

    public Task<bool> VerificarAsync(string login, string clave, CancellationToken ct) =>
        Task.FromResult(_usuarios.ContainsKey(login) && clave == AplicacionPrueba.Clave);
}

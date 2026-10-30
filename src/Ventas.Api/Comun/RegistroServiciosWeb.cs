using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventas.Api.Seguridad;
using Ventas.Application.Seguridad;

namespace Ventas.Api.Comun;

public static class RegistroServiciosWeb
{
    /// <summary>Cabecera con la que jQuery envía el token antifalsificación.</summary>
    public const string CabeceraAntifalsificacion = "RequestVerificationToken";

    public const string RutaInicioSesion = "/Cuenta/IniciarSesion";

    public static IServiceCollection AddWeb(this IServiceCollection services)
    {
        services
            .AddControllersWithViews(opciones =>
                // Valida el token en todo POST, PUT y DELETE
                opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
            .ConfigureApiBehaviorOptions(opciones =>
                // Cuerpo JSON ilegible en una acción [ApiController]: mismo formato que los errores de validación
                opciones.InvalidModelStateResponseFactory = contexto =>
                    new BadRequestObjectResult(new ValidationProblemDetails(contexto.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Los datos enviados no son válidos.",
                    })
                    {
                        ContentTypes = { "application/problem+json" },
                    });

        // Las vistas no codifican tildes ni eñes como entidades HTML
        services.Configure<WebEncoderOptions>(opciones =>
            opciones.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

        services.AddAntiforgery(opciones =>
        {
            opciones.HeaderName = CabeceraAntifalsificacion;
            opciones.Cookie.Name = "GalaxyVentas.Antifalsificacion";
            opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            opciones.Cookie.SameSite = SameSiteMode.Strict;
        });

        // Sesión por cookie válida 8 horas, sin renovación automática (RNF-06, P-18)
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opciones =>
            {
                opciones.Cookie.Name = "GalaxyVentas.Sesion";
                opciones.Cookie.HttpOnly = true;
                opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                opciones.Cookie.SameSite = SameSiteMode.Strict;
                opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
                opciones.SlidingExpiration = false;
                opciones.LoginPath = RutaInicioSesion;
                opciones.LogoutPath = "/Cuenta/CerrarSesion";
                opciones.AccessDeniedPath = "/Cuenta/AccesoDenegado";
                opciones.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = ValidarSesionAsync,
                    // Sin sesión, una llamada AJAX recibe 401 y una página se redirige al login (RF-19)
                    OnRedirectToLogin = contexto => ResponderAsync(contexto, StatusCodes.Status401Unauthorized,
                        "La sesión no está iniciada o expiró."),
                    OnRedirectToAccessDenied = contexto => ResponderAsync(contexto, StatusCodes.Status403Forbidden,
                        "No tiene acceso a este recurso."),
                };
            });

        // Todo endpoint exige sesión salvo los marcados con [AllowAnonymous]
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddProblemDetails();
        services.AddExceptionHandler<ManejadorExcepciones>();

        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioActual, UsuarioActual>();

        return services;
    }

    /// <summary>
    /// En cada petición, la sesión deja de ser válida si el usuario ya no está activo o vigente,
    /// o si cambió su punto de venta en SGP_USUARIO (RF-19).
    /// </summary>
    private static async Task ValidarSesionAsync(CookieValidatePrincipalContext contexto)
    {
        var login = contexto.Principal?.FindFirstValue(ClaimTypes.Name);
        var puntoVenta = contexto.Principal?.FindFirstValue(ClaimsSesion.PuntoVenta);
        var autenticacion = contexto.HttpContext.RequestServices.GetRequiredService<AutenticacionService>();

        var valida = login is not null && puntoVenta is not null
            && await autenticacion.ValidarSesionAsync(login, puntoVenta, contexto.HttpContext.RequestAborted);

        if (!valida)
        {
            contexto.RejectPrincipal();
            await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    private static async Task ResponderAsync(RedirectContext<CookieAuthenticationOptions> contexto, int status, string titulo)
    {
        if (!contexto.Request.EsAjax())
        {
            contexto.Response.Redirect(contexto.RedirectUri);
            return;
        }

        await contexto.HttpContext.EscribirProblemaAsync(new ProblemDetails { Status = status, Title = titulo });
    }
}

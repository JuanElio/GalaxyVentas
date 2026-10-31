using System.Net;
using Ventas.Api.Controllers;
using Ventas.Application.Seguridad;

namespace Ventas.IntegrationTests.Web;

public class MisDatosTests(AplicacionPrueba app) : IClassFixture<AplicacionPrueba>
{
    private async Task<HttpClient> ConSesionAsync(string login)
    {
        app.Usuarios.Agregar(login);
        var cliente = app.CrearCliente();
        await AplicacionPrueba.IniciarSesionAsync(cliente, login);
        return cliente;
    }

    private static async Task<HttpResponseMessage> CambiarClaveAsync(HttpClient cliente, string actual, string nueva, string confirmacion)
    {
        var token = await AplicacionPrueba.ObtenerTokenAsync(cliente, "/MisDatos");
        return await cliente.PostAsync("/MisDatos/CambiarClave", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ClaveActual"] = actual,
            ["ClaveNueva"] = nueva,
            ["ConfirmacionClave"] = confirmacion,
            ["__RequestVerificationToken"] = token,
        }));
    }

    [Fact]
    public async Task Index_ConSesion_MuestraLosDatosDelUsuarioYLaOpcionDelMenu()
    {
        var cliente = await ConSesionAsync("datos1");

        var html = await cliente.GetStringAsync("/MisDatos");

        Assert.Contains("Operador datos1", html);
        Assert.Contains("datos1@dop.pe", html);
        Assert.Contains("12345678", html);
        // La opción del menú aparece resaltada (el orden de los atributos lo decide el tag helper)
        Assert.Matches("<a (?=[^>]*href=\"/MisDatos\")(?=[^>]*aria-current=\"page\")[^>]*>", html);
        Assert.DoesNotContain(AplicacionPrueba.Clave, html);
    }

    [Fact]
    public async Task Index_SinSesion_RedirigeAlInicioDeSesion()
    {
        var respuesta = await app.CrearCliente().GetAsync("/MisDatos");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith("/Cuenta/IniciarSesion", respuesta.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task CambiarClave_Correcta_RedirigeConMensajeYLaNuevaClaveSirveParaIngresar()
    {
        var cliente = await ConSesionAsync("datos2");

        var respuesta = await CambiarClaveAsync(cliente, AplicacionPrueba.Clave, "Nueva2027", "Nueva2027");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/MisDatos", respuesta.Headers.Location?.OriginalString);
        Assert.Contains(MisDatosController.MensajeClaveCambiada, await cliente.GetStringAsync("/MisDatos"));

        var conClaveAnterior = await AplicacionPrueba.IniciarSesionAsync(app.CrearCliente(), "datos2");
        Assert.Equal(HttpStatusCode.OK, conClaveAnterior.StatusCode);
        var conClaveNueva = await AplicacionPrueba.IniciarSesionAsync(app.CrearCliente(), "datos2", "Nueva2027");
        Assert.Equal(HttpStatusCode.Redirect, conClaveNueva.StatusCode);
    }

    [Fact]
    public async Task CambiarClave_ClaveActualIncorrecta_MuestraElErrorSinDevolverLasClaves()
    {
        var cliente = await ConSesionAsync("datos3");

        var respuesta = await CambiarClaveAsync(cliente, "Incorrecta1", "Nueva2027", "Nueva2027");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var html = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains(MisDatosService.MensajeClaveActualIncorrecta, html);
        Assert.DoesNotContain("Incorrecta1", html);
        Assert.DoesNotContain("Nueva2027", html);
        Assert.Equal(HttpStatusCode.Redirect, (await AplicacionPrueba.IniciarSesionAsync(app.CrearCliente(), "datos3")).StatusCode);
    }

    [Fact]
    public async Task CambiarClave_SinTokenAntifalsificacion_Rechaza()
    {
        var cliente = await ConSesionAsync("datos4");

        var respuesta = await cliente.PostAsync("/MisDatos/CambiarClave", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ClaveActual"] = AplicacionPrueba.Clave,
            ["ClaveNueva"] = "Nueva2027",
            ["ConfirmacionClave"] = "Nueva2027",
        }));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}

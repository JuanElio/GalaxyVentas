using System.Net;

namespace Ventas.IntegrationTests.Web;

public class CuentaTests(AplicacionPrueba app) : IClassFixture<AplicacionPrueba>
{
    [Fact]
    public async Task IniciarSesion_CredencialesCorrectas_CreaLaCookieSeguraYMuestraElUsuario()
    {
        app.Usuarios.Agregar("jperez");
        var cliente = app.CrearCliente();

        var respuesta = await AplicacionPrueba.IniciarSesionAsync(cliente, "jperez");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Ventas/Registrar", respuesta.Headers.Location?.OriginalString);
        var cookie = Assert.Single(respuesta.Headers.GetValues("Set-Cookie"), c => c.StartsWith("GalaxyVentas.Sesion="));
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AplicacionPrueba.Clave, cookie);

        var inicio = await cliente.GetStringAsync("/");
        Assert.Contains("Operador jperez", inicio);
        Assert.Contains("Punto de venta 001", inicio);
    }

    [Fact]
    public async Task IniciarSesion_ConReturnUrlLocal_VuelveALaPaginaPedida()
    {
        app.Usuarios.Agregar("retorno");
        var cliente = app.CrearCliente();
        var token = await AplicacionPrueba.ObtenerTokenAsync(cliente, "/Cuenta/IniciarSesion");

        var respuesta = await cliente.PostAsync("/Cuenta/IniciarSesion", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Login"] = "retorno",
            ["Clave"] = AplicacionPrueba.Clave,
            ["ReturnUrl"] = "https://sitio-externo.com/robar",
            ["__RequestVerificationToken"] = token,
        }));

        // Una URL externa se ignora para evitar redirecciones abiertas
        Assert.Equal("/Ventas/Registrar", respuesta.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("noexiste", AplicacionPrueba.Clave)]
    [InlineData("mlopez", "otra-clave")]
    public async Task IniciarSesion_CredencialesIncorrectas_MensajeGenericoSinSesion(string login, string clave)
    {
        app.Usuarios.Agregar("mlopez");
        var cliente = app.CrearCliente();

        var respuesta = await AplicacionPrueba.IniciarSesionAsync(cliente, login, clave);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Usuario o contraseña incorrectos.", await respuesta.Content.ReadAsStringAsync());
        Assert.False(respuesta.Headers.TryGetValues("Set-Cookie", out var cookies)
            && cookies.Any(c => c.StartsWith("GalaxyVentas.Sesion=")));
    }

    [Fact]
    public async Task IniciarSesion_SinPuntoDeVenta_MuestraElMotivo()
    {
        app.Usuarios.Agregar("pvargas", puntoVenta: null);

        var respuesta = await AplicacionPrueba.IniciarSesionAsync(app.CrearCliente(), "pvargas");

        Assert.Contains("no tiene un punto de venta asignado", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task IniciarSesion_SinTokenAntifalsificacion_Responde400()
    {
        app.Usuarios.Agregar("sintoken");

        var respuesta = await app.CrearCliente().PostAsync("/Cuenta/IniciarSesion", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["Login"] = "sintoken", ["Clave"] = AplicacionPrueba.Clave }));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Sesion_CambiaElPuntoDeVentaDelUsuario_DejaDeSerValida()
    {
        app.Usuarios.Agregar("cambiapv", puntoVenta: "001");
        var cliente = app.CrearCliente();
        await AplicacionPrueba.IniciarSesionAsync(cliente, "cambiapv");

        app.Usuarios.Agregar("cambiapv", puntoVenta: "002");
        var respuesta = await cliente.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Contains("/Cuenta/IniciarSesion", respuesta.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Sesion_UsuarioDesactivado_LlamadaAjaxRecibe401()
    {
        app.Usuarios.Agregar("desactivado");
        var cliente = app.CrearCliente();
        await AplicacionPrueba.IniciarSesionAsync(cliente, "desactivado");

        app.Usuarios.Agregar("desactivado", activo: false);
        cliente.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        var respuesta = await cliente.GetAsync("/");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task CerrarSesion_EliminaLaSesion()
    {
        app.Usuarios.Agregar("salir");
        var cliente = app.CrearCliente();
        await AplicacionPrueba.IniciarSesionAsync(cliente, "salir");
        var token = await AplicacionPrueba.ObtenerTokenAsync(cliente, "/");

        var salida = await cliente.PostAsync("/Cuenta/CerrarSesion", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));

        Assert.Equal(HttpStatusCode.Redirect, salida.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await cliente.GetAsync("/")).StatusCode);
    }
}

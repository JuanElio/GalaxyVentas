using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ventas.IntegrationTests.Web;

public class SeguridadTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private HttpClient CrearCliente() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost"),
    });

    [Fact]
    public async Task Pagina_SinSesion_RedirigeAlInicioDeSesion()
    {
        var respuesta = await CrearCliente().GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith("https://localhost/Cuenta/IniciarSesion", respuesta.Headers.Location?.ToString());
    }

    [Fact]
    public async Task LlamadaAjax_SinSesion_Responde401ConProblemDetails()
    {
        var cliente = CrearCliente();
        cliente.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

        var respuesta = await cliente.GetAsync("/Home/Index");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(401, problema?.Status);
    }

    [Fact]
    public async Task ArchivosEstaticos_SinSesion_SeSirven()
    {
        var respuesta = await CrearCliente().GetAsync("/js/site.js");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task PaginaDeError_SinSesion_SeMuestra()
    {
        var respuesta = await CrearCliente().GetAsync("/Home/Error");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }
}

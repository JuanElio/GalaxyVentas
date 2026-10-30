using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;

namespace Ventas.IntegrationTests.Web;

public class ClientesTests(AplicacionPrueba app) : IClassFixture<AplicacionPrueba>
{
    [Fact]
    public async Task Buscar_ClienteExistente_DevuelveJson()
    {
        app.Clientes.Agregar(Cliente.Registrar("2", "41234567", "Rosa", "Quispe", null, null, null, null));
        var cliente = await app.CrearClienteAjaxConSesionAsync("buscador");

        var encontrados = await cliente.GetFromJsonAsync<List<ClienteDto>>("/Clientes/Buscar?numeroDocumento=41234567");

        Assert.Equal("Rosa Quispe", Assert.Single(encontrados!).Nombre);
    }

    [Fact]
    public async Task Buscar_ClienteInexistente_Responde404ConProblemDetails()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("buscador404");

        var respuesta = await cliente.GetAsync("/Clientes/Buscar?numeroDocumento=00000000");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Contains("00000000", problema?.Detail);
    }

    [Fact]
    public async Task Registrar_ClienteNuevo_Responde201()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("registrador");

        var respuesta = await cliente.PostAsJsonAsync("/Clientes/Registrar",
            new RegistrarClienteDto("2", "47654321", "Luis", "Ramos", null, null, null, null));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.Equal("Luis Ramos", (await respuesta.Content.ReadFromJsonAsync<ClienteDto>())?.Nombre);
    }

    [Fact]
    public async Task Registrar_Duplicado_Responde409()
    {
        app.Clientes.Agregar(Cliente.Registrar("2", "45556667", "Ana", "Paz", null, null, null, null));
        var cliente = await app.CrearClienteAjaxConSesionAsync("duplicador");

        var respuesta = await cliente.PostAsJsonAsync("/Clientes/Registrar",
            new RegistrarClienteDto("2", "45556667", "Ana", "Paz", null, null, null, null));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Registrar_DatosInvalidos_Responde400ConErroresPorCampo()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("validador");

        var respuesta = await cliente.PostAsJsonAsync("/Clientes/Registrar",
            new RegistrarClienteDto("2", "123", null, null, null, null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("NumeroDocumento", problema!.Errors.Keys);
        Assert.Contains("Nombres", problema.Errors.Keys);
    }

    [Fact]
    public async Task Registrar_SinTokenAntifalsificacion_Responde400SinGrabar()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("sintokenajax");
        cliente.DefaultRequestHeaders.Remove("RequestVerificationToken");

        var respuesta = await cliente.PostAsJsonAsync("/Clientes/Registrar",
            new RegistrarClienteDto("2", "48889990", "Eva", "Luna", null, null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.False(await app.Clientes.ExisteActivoAsync("2", "48889990", CancellationToken.None));
    }

    [Fact]
    public async Task Buscar_SinSesion_Responde401()
    {
        var cliente = app.CrearCliente();
        cliente.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

        var respuesta = await cliente.GetAsync("/Clientes/Buscar?numeroDocumento=41234567");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }
}

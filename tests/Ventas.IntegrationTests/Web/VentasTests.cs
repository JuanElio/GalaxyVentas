using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.Ventas;

namespace Ventas.IntegrationTests.Web;

public class VentasTests(AplicacionPrueba app) : IClassFixture<AplicacionPrueba>
{
    [Fact]
    public async Task Registrar_ConSesion_MuestraElContextoDeLaVenta()
    {
        app.Usuarios.Agregar("vendedor");
        var cliente = app.CrearCliente();
        await AplicacionPrueba.IniciarSesionAsync(cliente, "vendedor");

        var html = await cliente.GetStringAsync("/Ventas/Registrar");

        Assert.Contains("001 · SEDE CENTRAL", html);
        Assert.Contains("30 · Precio Especial", html);
        Assert.Contains("Soles (SOL)", html);
        Assert.Contains("ALM01", html);
        Assert.Contains("Operador vendedor (vendedor)", html);
        Assert.Contains("registrar-venta.js", html);
    }

    [Fact]
    public async Task Registrar_SinSesion_RedirigeAlLogin()
    {
        var respuesta = await app.CrearCliente().GetAsync("/Ventas/Registrar");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Contains("/Cuenta/IniciarSesion", respuesta.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Cotizar_DevuelveLosImportesCalculadosPorElDominio()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("cotizador");

        var respuesta = await cliente.PostAsJsonAsync("/Ventas/Cotizar", new CotizarVentaDto([new LineaVentaDto("ART001", 3)]));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cotizacion = (await respuesta.Content.ReadFromJsonAsync<CotizacionDto>())!;
        var linea = Assert.Single(cotizacion.Lineas);
        Assert.Equal(20.0000m, linea.Tarifa);
        Assert.Equal(60.0000m, linea.SubTotal);
        Assert.Equal(9.1525m, linea.Igv);
        Assert.Equal(60.0000m, cotizacion.MontoTotal);
        Assert.True(cotizacion.Valida);
    }

    [Fact]
    public async Task Cotizar_CantidadInvalida_Responde400ConProblemDetails()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("cotizador400");

        var respuesta = await cliente.PostAsJsonAsync("/Ventas/Cotizar", new CotizarVentaDto([new LineaVentaDto("ART001", 0)]));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("Lineas[0].Cantidad", problema!.Errors.Keys);
    }

    [Fact]
    public async Task Cotizar_SinTokenAntifalsificacion_Responde400()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("cotizasintoken");
        cliente.DefaultRequestHeaders.Remove("RequestVerificationToken");

        var respuesta = await cliente.PostAsJsonAsync("/Ventas/Cotizar", new CotizarVentaDto([]));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task RegistrarVenta_DatosInvalidos_Responde400ConErroresPorCampo()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("registra400");

        var respuesta = await cliente.PostAsJsonAsync("/Ventas/Registrar",
            new RegistrarVentaDto(0, "X", [], new PagoVentaDto(4, null, null)));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Superset(new HashSet<string> { "CodigoCliente", "TipoComprobante", "Lineas", "Pago.CodigoBanco" },
            problema!.Errors.Keys.ToHashSet());
    }

    [Fact]
    public async Task RegistrarVenta_SinTokenAntifalsificacion_Responde400()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("registrasintok");
        cliente.DefaultRequestHeaders.Remove("RequestVerificationToken");

        var respuesta = await cliente.PostAsJsonAsync("/Ventas/Registrar",
            new RegistrarVentaDto(125, "B", [new LineaVentaDto("ART001", 1)], new PagoVentaDto(1, null, null)));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task SiguienteNumero_SerieDelPuntoDeVentaDeLaSesion()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("numerador");

        var numeracion = await cliente.GetFromJsonAsync<NumeracionDto>("/Ventas/SiguienteNumero?tipoComprobante=B");

        Assert.Equal(new NumeracionDto("B001", "0000007"), numeracion);
    }

    [Fact]
    public async Task SiguienteNumero_TipoInvalido_Responde422()
    {
        var cliente = await app.CrearClienteAjaxConSesionAsync("numerador422");

        var respuesta = await cliente.GetAsync("/Ventas/SiguienteNumero?tipoComprobante=X");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }
}

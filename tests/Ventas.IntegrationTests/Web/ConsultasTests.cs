using System.Net;
using Moq;
using Ventas.Application.Comprobantes;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;

namespace Ventas.IntegrationTests.Web;

public class ConsultasTests(AplicacionPrueba app) : IClassFixture<AplicacionPrueba>
{
    private static T ConId<T>(T entidad, long id) where T : class
    {
        typeof(T).GetProperty("Id")!.SetValue(entidad, id);
        return entidad;
    }

    /// <summary>Simula la venta <paramref name="transaccion"/> con su comprobante <paramref name="codigo"/> en el punto de venta indicado.</summary>
    private void DadoComprobante(long codigo, long transaccion, string puntoVenta)
    {
        var venta = ConId(new Venta(puntoVenta, 125, "V01", "1", "01", "110", "SOL", "C00", "B", new DateTime(2026, 10, 29, 10, 30, 0)), transaccion);
        venta.AgregarLinea("ART001", "UND", 2, 20.0000m);
        var comprobante = ConId(Comprobante.Emitir(venta,
            new ClienteComprobante(125, "QUISPE YANQUI ROSA", "AV. LIMA 1", "2", "41234567", false),
            "L10-SEDE CENTRAL", 845, new DateTime(2026, 10, 29, 10, 31, 0),
            new Dictionary<string, string> { ["ART001"] = "CUADERNO A4" }), codigo);
        comprobante.AsignarPago(MedioPago.Registrar(venta, TipoMedioPago.Efectivo, null, null));
        venta.MarcarEmitida(codigo);

        app.Consultas.Setup(c => c.ObtenerComprobanteAsync(codigo, It.IsAny<CancellationToken>())).ReturnsAsync(comprobante);
        app.Consultas.Setup(c => c.ObtenerVentaAsync(transaccion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VentaRegistrada(venta, "vendedor"));
    }

    private async Task<HttpClient> ClienteConSesionAsync(string login)
    {
        app.Usuarios.Agregar(login);
        var cliente = app.CrearCliente();
        await AplicacionPrueba.IniciarSesionAsync(cliente, login);
        return cliente;
    }

    [Fact]
    public async Task VerComprobante_MuestraClienteDetalleTotalesYPago()
    {
        DadoComprobante(77, 4521, "001");
        var cliente = await ClienteConSesionAsync("consultor");

        var html = await cliente.GetStringAsync("/Comprobantes/Ver/77?nuevo=true");

        Assert.Contains("B001-0000845", html);
        Assert.Contains("QUISPE YANQUI ROSA", html);
        Assert.Contains("CUADERNO A4", html);
        Assert.Contains("S/ 40.00", html);
        Assert.Contains("Venta registrada.", html);
        Assert.Contains("ítem 1 ↔ línea 1", html);
    }

    [Fact]
    public async Task VerComprobante_DeOtroPuntoDeVenta_Responde403ConElMotivo()
    {
        DadoComprobante(78, 4522, "002");
        var cliente = await ClienteConSesionAsync("consultor403");

        var respuesta = await cliente.GetAsync("/Comprobantes/Ver/78");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Contains("pertenece a otro punto de venta", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task VerComprobante_Inexistente_Responde404()
    {
        var cliente = await ClienteConSesionAsync("consultor404");

        var respuesta = await cliente.GetAsync("/Comprobantes/Ver/999999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Contains("No existe el comprobante 999999.", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Consultas_PorTransaccion_RedirigeAlComprobante()
    {
        DadoComprobante(79, 4523, "001");
        var cliente = await ClienteConSesionAsync("consultatrans");

        var respuesta = await cliente.GetAsync("/Consultas?transaccion=4523");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Comprobantes/Ver/79", respuesta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Consultas_ComprobanteInexistente_MuestraElMensajeEnLaPagina()
    {
        var cliente = await ClienteConSesionAsync("consultanum");

        var respuesta = await cliente.GetAsync("/Consultas?tipoComprobante=B&numero=12");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("No existe el comprobante B001-0000012.", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Reporte_PrimeraVisita_MuestraElReporteDelDia()
    {
        var cliente = await ClienteConSesionAsync("reportero");

        var html = await cliente.GetStringAsync("/Reportes/Ventas");

        Assert.Contains("Resultado", html);
        Assert.Contains("No hay ventas en el rango", html);
    }

    [Fact]
    public async Task Reporte_RangoMayorA31Dias_MuestraElError()
    {
        var cliente = await ClienteConSesionAsync("reportero31");

        var html = await cliente.GetStringAsync("/Reportes/Ventas?desde=2026-09-01&hasta=2026-10-15");

        Assert.Contains("no puede superar 31 días", html);
        Assert.DoesNotContain("<h2 id=\"h-res\">", html);
    }
}

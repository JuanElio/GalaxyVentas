using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ventas.Application.Comprobantes;
using Ventas.Application.Maestros;
using Ventas.Application.Reportes;
using Ventas.Application.Seguridad;
using Ventas.Application.Ventas;
using Ventas.Domain.Comun;
using Ventas.Domain.Maestros;
using Ventas.Infrastructure.Persistence;

namespace Ventas.IntegrationTests.Persistence;

/// <summary>
/// Registro completo de una venta contra la BD real (BDVenta). La prueba del camino feliz abre una
/// transacción y la deshace al final, así que no deja datos; la de error deja que el servicio abra
/// su propia transacción para comprobar el rollback. Si no hay cadena de conexión, no hacen nada.
/// </summary>
[Trait("Categoria", "BaseDatos")]
public class RegistroVentaBaseDatosTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private const string PuntoVenta = "001";

    private sealed class UsuarioDePrueba : IUsuarioActual
    {
        public bool EstaAutenticado => true;
        public string Login => "pruebas";
        public string Nombre => "Pruebas de integración";
        public string CodigoPuntoVenta => PuntoVenta;
    }

    private WebApplicationFactory<Program> App => factory.WithWebHostBuilder(b =>
        b.ConfigureTestServices(s => s.AddScoped<IUsuarioActual, UsuarioDePrueba>()));

    private bool HayBaseDatos() =>
        !string.IsNullOrWhiteSpace(factory.Services.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection"));

    private static async Task<T> EscalarAsync<T>(VentasDbContext db, FormattableString sql) =>
        (await db.Database.SqlQuery<T>(sql).ToListAsync()).Single();

    /// <summary>Un artículo con precio para 2 unidades y stock en el punto de venta, y un cliente activo con DNI.</summary>
    private static async Task<(string Articulo, long Cliente)> DatosDeVentaAsync(IServiceProvider servicios)
    {
        var articulos = await servicios.GetRequiredService<IArticuloRepository>()
            .ListarConTarifasVigentesAsync("01", null, DateTime.Now, 100, CancellationToken.None);
        var stock = await servicios.GetRequiredService<IStockService>()
            .ObtenerStockAsync(PuntoVenta, articulos.Select(a => a.Articulo.Codigo).ToList(), CancellationToken.None);
        var articulo = articulos.First(a => Tarifa.Elegir(a.Tarifas, 2, DateTime.Now) is not null
            && stock.GetValueOrDefault(a.Articulo.Codigo) >= 2).Articulo.Codigo;

        var db = servicios.GetRequiredService<VentasDbContext>();
        var cliente = await db.Set<Cliente>()
            .Where(c => EF.Property<bool?>(c, "_activo") == true && c.TipoDocumento == "2" && c.TipoPersona == "1")
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .FirstAsync();

        return (articulo, cliente);
    }

    [Fact]
    public async Task Registrar_GrabaVentaComprobanteKardexYPagoConLosValoresDeReferencia()
    {
        if (!HayBaseDatos())
            return;

        using var scope = App.Services.CreateScope();
        var servicios = scope.ServiceProvider;
        var db = servicios.GetRequiredService<VentasDbContext>();
        var (articulo, cliente) = await DatosDeVentaAsync(servicios);

        await using var transaccion = await db.Database.BeginTransactionAsync();
        var correlativoAntes = await EscalarAsync<int>(db,
            $"SELECT TOP 1 ISNULL(vchCorrelativo, 1) AS Value FROM SGP_IMPRESORA WHERE vchNumSerie = 'B001' AND chrSituacionRegistro = 'A' ORDER BY intCodImpresora");
        var stockAntes = (await servicios.GetRequiredService<IStockService>()
            .ObtenerStockAsync(PuntoVenta, [articulo], CancellationToken.None))[articulo];

        var venta = await servicios.GetRequiredService<RegistroVentaService>().RegistrarAsync(
            new RegistrarVentaDto(cliente, "B", [new LineaVentaDto(articulo, 2)], new PagoVentaDto(1, null, null)),
            CancellationToken.None);

        // Comprobante: serie, correlativo y estado cancelado
        Assert.Equal("B001", venta.Serie);
        Assert.Equal(correlativoAntes.ToString("D7"), venta.Numero);
        Assert.Equal(correlativoAntes + 1, await EscalarAsync<int>(db,
            $"SELECT TOP 1 vchCorrelativo AS Value FROM SGP_IMPRESORA WHERE vchNumSerie = 'B001' AND chrSituacionRegistro = 'A' ORDER BY intCodImpresora"));

        var t = venta.CodigoTransaccion;
        var c = venta.CodigoComprobante;

        // Cabecera: formato heredado de punto de venta y canal, estado 'A' y enlace al comprobante
        Assert.Equal(" 001|A| 01|1|V01|1|0|SOL|C00|B|pruebas", await EscalarAsync<string>(db, $"""
            SELECT vchCodPuntoVenta + '|' + chrSituacionRegistro + '|' + vchCodTarifaCanal + '|' + CONVERT(varchar, intEstado)
                + '|' + vchTipoOrden + '|' + RTRIM(vchTipLiquidacion) + '|' + chrFlagClienteAgencia + '|' + vchCodMoneda
                + '|' + vchFormaPago + '|' + vchTipComprobante + '|' + vchCreador AS Value
            FROM SGP_CABECERA_TRANSACCION_BS WHERE numCodTransaccion = {t}
            """));
        Assert.Equal((decimal)c, await EscalarAsync<decimal>(db,
            $"SELECT numCodComprobante AS Value FROM SGP_CABECERA_TRANSACCION_BS WHERE numCodTransaccion = {t}"));

        // Totales: cabecera = comprobante = Σ líneas = pago (RN-12)
        Assert.Equal(venta.MontoTotal, await EscalarAsync<decimal>(db,
            $"SELECT SUM(numSubTotal) AS Value FROM SGP_DETALLE_TRANSACCION_BS WHERE numCodTransaccion = {t}"));
        Assert.Equal(1, await EscalarAsync<int>(db, $"""
            SELECT COUNT(*) AS Value FROM SGP_CABECERA_TRANSACCION_BS ct
            JOIN SGP_COMPROBANTE_BS cb ON cb.numCodComprobante = {c}
            WHERE ct.numCodTransaccion = {t} AND ct.numMontoTotal = cb.numMontoTotal AND ct.numMontoBruto = cb.numMontoBruto
              AND ct.numSubTotal = cb.numSubTotal AND ct.numMontoIGV = cb.numMontoIGV AND cb.numMontoPagado = cb.numMontoTotal
            """));

        // Comprobante: valores fijos del procedimiento de emisión
        Assert.Equal(" 001|2|S|S|N|0|0|1.0000|B001", await EscalarAsync<string>(db, $"""
            SELECT vchCodPuntoVenta + '|' + vchEstado + '|' + chrFlagFacturado + '|' + vchFlagMedioPago + '|' + chrFlagMigrado
                + '|' + vchOrdenVenta + '|' + chrFlagClienteAgencia + '|' + CONVERT(varchar, numTipoCambio) + '|' + vchNumSerie AS Value
            FROM SGP_COMPROBANTE_BS WHERE numCodComprobante = {c}
            """));
        Assert.Equal(t.ToString(), await EscalarAsync<string>(db,
            $"SELECT vchCodTransaccion AS Value FROM SGP_COMPROBANTE_BS WHERE numCodComprobante = {c}"));
        Assert.Equal(1, await EscalarAsync<int>(db,
            $"SELECT COUNT(*) AS Value FROM SGP_DETALLE_COMPROBANTE_BS WHERE numCodComprobante = {c} AND numVeces = 0 AND numPrecioUnitario > 0"));
        Assert.Equal(1, await EscalarAsync<int>(db,
            $"SELECT COUNT(*) AS Value FROM SGP_MOVIMIENTO_TRANSACCION_BS WHERE numCodComprobante = {c} AND numCodTransaccion = {t} AND numItem = 1 AND numLinTransaccion = 1"));

        // Kardex: salida por la cantidad vendida y el stock baja exactamente en esa cantidad (RF-09)
        Assert.Equal($"VENTA|-2.0000| 001|01|Salida por venta B001-{venta.Numero}", await EscalarAsync<string>(db, $"""
            SELECT TipoTransaccion + '|' + CONVERT(varchar, Cantidad) + '|' + CodPuntoVenta + '|' + CodTarifaCanal + '|' + Descripcion AS Value
            FROM PGA_TRANSACCION_KARDEX WHERE CodTransaccion = {t}
            """));
        Assert.Equal(stockAntes - 2, (await servicios.GetRequiredService<IStockService>()
            .ObtenerStockAsync(PuntoVenta, [articulo], CancellationToken.None))[articulo]);

        // Pago: un solo medio por el total, asignado al comprobante (RF-13, RF-14)
        Assert.Equal("1|2|0|N|PGA|SOL|1.0000", await EscalarAsync<string>(db, $"""
            SELECT vchMedioPago + '|' + vchEstado + '|' + CONVERT(varchar, numLinea) + '|' + chrFlagPendienteAjuste + '|' + vchOrigen
                + '|' + vchCodMoneda + '|' + CONVERT(varchar, numTipoCambio) AS Value
            FROM SGP_MEDIO_PAGO_BS WHERE numCodTransaccion = {t}
            """));
        Assert.Equal(venta.MontoTotal, await EscalarAsync<decimal>(db, $"""
            SELECT mpc.numMontoPago AS Value FROM SGP_MEDIO_PAGO_COMPROBANTE_BS mpc
            JOIN SGP_MEDIO_PAGO_BS mp ON mp.numCodMediPago = mpc.numCodMediPago
            WHERE mpc.numCodComprobante = {c} AND mp.numCodTransaccion = {t}
            """));

        // Consultas y reporte sobre la venta recién registrada (RF-16, RF-17, RF-22)
        var consultas = servicios.GetRequiredService<ConsultaVentasService>();
        Assert.Equal(c, await consultas.BuscarComprobanteDeVentaAsync(t, CancellationToken.None));
        Assert.Equal(c, await consultas.BuscarComprobanteAsync("B", venta.Numero, CancellationToken.None));
        var detalle = await consultas.ObtenerComprobanteAsync(c, CancellationToken.None);
        Assert.Equal("BOLETA", detalle.TipoComprobante);
        Assert.Equal("Cancelado", detalle.Estado);
        Assert.Equal("pruebas", detalle.Usuario);
        Assert.Equal(articulo, Assert.Single(detalle.Detalle).CodigoArticulo);
        Assert.Equal(venta.MontoTotal, Assert.Single(detalle.Pagos).Monto);

        var hoy = DateTime.Today;
        var reporte = await servicios.GetRequiredService<ReporteVentasService>().GenerarAsync(
            new FiltroReporteVentasDto(hoy, hoy, "B", 1, "pruebas"), CancellationToken.None);
        var fila = Assert.Single(reporte.Filas, f => f.CodigoComprobante == c);
        Assert.Equal($"B001-{venta.Numero}", fila.Comprobante);
        Assert.Equal("EFECTIVO", fila.MedioPago);
        Assert.Equal(venta.MontoTotal, fila.MontoTotal);
        Assert.Contains(reporte.PorMedioPago, m => m.MedioPago == "EFECTIVO" && m.Monto >= venta.MontoTotal);

        // Al salir del using la transacción se deshace: la BD queda como estaba
    }

    [Fact]
    public async Task Registrar_FallaDespuesDeGrabarLaCabecera_DeshaceTodo()
    {
        if (!HayBaseDatos())
            return;

        using var scope = App.Services.CreateScope();
        var servicios = scope.ServiceProvider;
        var db = servicios.GetRequiredService<VentasDbContext>();
        var (articulo, cliente) = await DatosDeVentaAsync(servicios);

        var cabecerasAntes = await EscalarAsync<int>(db, $"SELECT COUNT(*) AS Value FROM SGP_CABECERA_TRANSACCION_BS");
        var correlativoAntes = await EscalarAsync<int>(db,
            $"SELECT TOP 1 ISNULL(vchCorrelativo, 1) AS Value FROM SGP_IMPRESORA WHERE vchNumSerie = 'F001' AND chrSituacionRegistro = 'A' ORDER BY intCodImpresora");
        var kardexAntes = await EscalarAsync<int>(db, $"SELECT COUNT(*) AS Value FROM PGA_TRANSACCION_KARDEX");

        // Factura para un cliente con DNI: el dominio la rechaza al emitir, después de grabar la cabecera
        var accion = () => servicios.GetRequiredService<RegistroVentaService>().RegistrarAsync(
            new RegistrarVentaDto(cliente, "F", [new LineaVentaDto(articulo, 1)], new PagoVentaDto(1, null, null)),
            CancellationToken.None);

        var error = await Assert.ThrowsAsync<DominioException>(accion);
        Assert.Contains("RUC", error.Message);
        Assert.Equal(cabecerasAntes, await EscalarAsync<int>(db, $"SELECT COUNT(*) AS Value FROM SGP_CABECERA_TRANSACCION_BS"));
        Assert.Equal(kardexAntes, await EscalarAsync<int>(db, $"SELECT COUNT(*) AS Value FROM PGA_TRANSACCION_KARDEX"));
        Assert.Equal(correlativoAntes, await EscalarAsync<int>(db,
            $"SELECT TOP 1 ISNULL(vchCorrelativo, 1) AS Value FROM SGP_IMPRESORA WHERE vchNumSerie = 'F001' AND chrSituacionRegistro = 'A' ORDER BY intCodImpresora"));
    }

    [Fact]
    public async Task Bloqueos_UnaSegundaVentaEsperaAQueTermineLaPrimera()
    {
        if (!HayBaseDatos())
            return;

        using var scopeA = App.Services.CreateScope();
        using var scopeB = App.Services.CreateScope();
        var dbA = scopeA.ServiceProvider.GetRequiredService<VentasDbContext>();
        var dbB = scopeB.ServiceProvider.GetRequiredService<VentasDbContext>();
        var (articulo, _) = await DatosDeVentaAsync(scopeA.ServiceProvider);

        // La venta A toma el stock del artículo y la impresora B001
        var transaccionA = await dbA.Database.BeginTransactionAsync();
        await scopeA.ServiceProvider.GetRequiredService<IStockService>().BloquearAsync(PuntoVenta, [articulo], CancellationToken.None);
        Assert.NotNull(await scopeA.ServiceProvider.GetRequiredService<ICorrelativoService>().ObtenerParaEmitirAsync("B001", CancellationToken.None));

        // La venta B intenta lo mismo y queda esperando
        await using var transaccionB = await dbB.Database.BeginTransactionAsync();
        var stockB = scopeB.ServiceProvider.GetRequiredService<IStockService>().BloquearAsync(PuntoVenta, [articulo], CancellationToken.None);
        await Task.Delay(1000);
        Assert.False(stockB.IsCompleted, "La segunda venta no debió obtener el stock mientras la primera lo tiene.");

        // Al terminar A, B continúa
        await transaccionA.RollbackAsync();
        await transaccionA.DisposeAsync();
        await stockB.WaitAsync(TimeSpan.FromSeconds(5));
        var impresoraB = scopeB.ServiceProvider.GetRequiredService<ICorrelativoService>().ObtenerParaEmitirAsync("B001", CancellationToken.None);
        Assert.NotNull(await impresoraB.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task Registrar_StockInsuficiente_ExcepcionDeDominioSinGrabar()
    {
        if (!HayBaseDatos())
            return;

        using var scope = App.Services.CreateScope();
        var servicios = scope.ServiceProvider;
        var db = servicios.GetRequiredService<VentasDbContext>();
        var (articulo, cliente) = await DatosDeVentaAsync(servicios);
        var cabecerasAntes = await EscalarAsync<int>(db, $"SELECT COUNT(*) AS Value FROM SGP_CABECERA_TRANSACCION_BS");

        var accion = () => servicios.GetRequiredService<RegistroVentaService>().RegistrarAsync(
            new RegistrarVentaDto(cliente, "B", [new LineaVentaDto(articulo, 999_999)], new PagoVentaDto(1, null, null)),
            CancellationToken.None);

        var error = await Assert.ThrowsAsync<DominioException>(accion);
        Assert.Contains("Stock insuficiente", error.Message);
        Assert.Equal(cabecerasAntes, await EscalarAsync<int>(db, $"SELECT COUNT(*) AS Value FROM SGP_CABECERA_TRANSACCION_BS"));
    }
}

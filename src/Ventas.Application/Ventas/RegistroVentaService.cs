using FluentValidation;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Comun;
using Ventas.Domain.Inventario;
using Ventas.Domain.Maestros;
using Ventas.Domain.Pagos;
using Ventas.Domain.Ventas;

namespace Ventas.Application.Ventas;

/// <summary>
/// Registra la venta, emite su comprobante, descuenta el stock y registra el pago en una única
/// transacción (RF-05 a RF-15, RN-15). Reproduce los valores de los procedimientos de referencia
/// pga_cabecera_transaccion_hemeroteca_agregar, pga_hemeroteca_emitir_comprobante y
/// pga_hemeroteca_comprobante_cancelar.
/// </summary>
public sealed class RegistroVentaService(
    IClienteRepository clientes,
    IArticuloRepository articulos,
    IPuntoVentaRepository puntosVenta,
    IStockService stock,
    ICorrelativoService correlativos,
    IVentaRepository ventas,
    IComprobanteRepository comprobantes,
    IKardexRepository kardex,
    IUnitOfWork unidadTrabajo,
    IUsuarioActual usuarioActual,
    IValidator<RegistrarVentaDto> validador,
    TimeProvider reloj)
{
    // SGP_COMPROBANTE_BS.vchZonaVenta es varchar(50); el procedimiento recorta con SUBSTRING(x, 0, 50)
    private const int LongitudMaximaZona = 50;

    public async Task<VentaRegistradaDto> RegistrarAsync(RegistrarVentaDto datos, CancellationToken ct)
    {
        await validador.ValidateAndThrowAsync(datos, ct);

        // El punto de venta es siempre el de la sesión, nunca el de la petición (RN-19)
        var codigoPuntoVenta = usuarioActual.CodigoPuntoVenta;
        var puntoVenta = await puntosVenta.ObtenerAsync(codigoPuntoVenta, ct);
        if (puntoVenta is not { Activo: true })
            throw new NoEncontradoException($"El punto de venta {codigoPuntoVenta} no existe o está inactivo.");

        var cliente = await clientes.ObtenerActivoAsync(datos.CodigoCliente, ct)
            ?? throw new NoEncontradoException($"No existe un cliente activo con código {datos.CodigoCliente}.");

        var tipoOrden = await puntosVenta.ObtenerTipoOrdenAsync(ParametrosVenta.TipoOrden, ct);
        if (tipoOrden is not { Activo: true })
            throw new NoEncontradoException($"El tipo de orden {ParametrosVenta.TipoOrden} no existe o está inactivo.");

        var almacen = await puntosVenta.ObtenerAlmacenPrincipalAsync(codigoPuntoVenta, ct)
            ?? throw new DominioException($"El punto de venta {codigoPuntoVenta} no tiene un almacén activo asignado.");

        var zonaVenta = await DescribirZonaAsync(puntoVenta, ct);
        var serie = TiposComprobante.FormarSerie(datos.TipoComprobante, codigoPuntoVenta);
        var codigos = datos.Lineas.Select(l => l.CodigoArticulo.Trim()).ToList();

        return await unidadTrabajo.EjecutarEnTransaccionAsync(async () =>
        {
            // Reservar el stock antes de leerlo: otra venta del mismo artículo espera hasta el commit (RF-08)
            await stock.BloquearAsync(codigoPuntoVenta, codigos, ct);

            var fecha = reloj.GetLocalNow().DateTime;
            var encontrados = (await articulos.ObtenerConTarifasVigentesAsync(ParametrosVenta.TipoOrden, ParametrosVenta.Canal, codigos, fecha, ct))
                .ToDictionary(a => a.Articulo.Codigo, StringComparer.OrdinalIgnoreCase);
            var stockActual = await stock.ObtenerStockAsync(codigoPuntoVenta, codigos, ct);

            var impresora = await correlativos.ObtenerParaEmitirAsync(serie, ct)
                ?? throw new DominioException($"No existe una impresora activa para la serie {serie}.");

            // 1. Cabecera y detalle de la venta (situación 'I')
            var venta = new Venta(codigoPuntoVenta, cliente.Id, tipoOrden.Codigo.Trim(), tipoOrden.TipoLiquidacion,
                ParametrosVenta.Canal, almacen.Trim(), ParametrosVenta.Moneda, ParametrosVenta.FormaPagoContado,
                datos.TipoComprobante, fecha);
            foreach (var linea in datos.Lineas)
                AgregarLinea(venta, linea, encontrados, stockActual, fecha);

            await ventas.AgregarAsync(venta, ct);
            await unidadTrabajo.GuardarCambiosAsync(ct);

            // 2. Comprobante con el correlativo de la impresora, pago por el total y salida del kardex
            var comprobante = Comprobante.Emitir(venta, DatosCliente(cliente), zonaVenta, impresora.TomarNumero(), fecha,
                encontrados.ToDictionary(e => e.Key, e => e.Value.Articulo.Descripcion?.Trim() ?? e.Key, StringComparer.OrdinalIgnoreCase));

            var pago = MedioPago.Registrar(venta, (TipoMedioPago)datos.Pago.MedioPago, datos.Pago.CodigoBanco, datos.Pago.NumeroOperacion);
            comprobante.AsignarPago(pago);

            await comprobantes.AgregarAsync(comprobante, ct);
            await kardex.AgregarAsync(venta.Detalles.Select(d => MovimientoKardex.SalidaPorVenta(
                d.CodigoArticulo, ParametrosVenta.Canal, codigoPuntoVenta, fecha, d.Cantidad, venta.Id,
                comprobante.Serie, comprobante.Numero)), ct);
            await unidadTrabajo.GuardarCambiosAsync(ct);

            // 3. La cabecera apunta al comprobante y pasa a 'A'
            if (comprobante.Id > ParametrosVenta.CodigoComprobanteMaximoEnCabecera)
                throw new InvalidOperationException(
                    $"El código de comprobante {comprobante.Id} no cabe en SGP_CABECERA_TRANSACCION_BS.numCodComprobante (P-12).");
            venta.MarcarEmitida(comprobante.Id);
            await unidadTrabajo.GuardarCambiosAsync(ct);

            return new VentaRegistradaDto(venta.Id, comprobante.Id, comprobante.Serie, comprobante.Numero,
                comprobante.FechaEmision, comprobante.MontoTotal);
        }, ct);
    }

    private static void AgregarLinea(Venta venta, LineaVentaDto linea, IReadOnlyDictionary<string, ArticuloConTarifas> encontrados,
        IReadOnlyDictionary<string, decimal> stockActual, DateTime fecha)
    {
        var codigo = linea.CodigoArticulo.Trim();
        if (!encontrados.TryGetValue(codigo, out var articulo))
            throw new DominioException($"El artículo {codigo} no existe, está inactivo o no tiene categoría, tarifa o IGV vigente.");

        var tarifa = Tarifa.Elegir(articulo.Tarifas, linea.Cantidad, fecha)
            ?? throw new DominioException($"No hay tarifa vigente para vender {linea.Cantidad:0} unidades de {codigo}.");

        var disponible = stockActual.GetValueOrDefault(codigo);
        if (linea.Cantidad > disponible)
            throw new DominioException(
                $"Stock insuficiente para {codigo}: disponible {disponible:0.####}, solicitado {linea.Cantidad:0}.");

        venta.AgregarLinea(codigo, articulo.Articulo.UnidadMedida?.Trim(), linea.Cantidad, tarifa.Precio!.Value, articulo.TasaIgv);
    }

    private static ClienteComprobante DatosCliente(Cliente cliente) => new(
        cliente.Id,
        cliente.NombreComprobante,
        cliente.DireccionFacturacion?.TrimEnd(),
        cliente.TipoDocumento.Trim(),
        cliente.NumeroDocumento.Trim(),
        cliente.TieneRuc);

    /// <summary>Zona como la arma el procedimiento: código + '-' + descripción, recortada a 49 caracteres si pasa de 50.</summary>
    private async Task<string?> DescribirZonaAsync(PuntoVenta puntoVenta, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(puntoVenta.ZonaVenta))
            return null;

        var zona = await puntosVenta.ObtenerZonaVentaAsync(puntoVenta.ZonaVenta, ct);
        if (zona is null)
            return null;

        var texto = $"{zona.Codigo.Trim()}-{zona.Descripcion?.Trim()}";
        return texto.Length > LongitudMaximaZona ? texto[..(LongitudMaximaZona - 1)] : texto;
    }
}

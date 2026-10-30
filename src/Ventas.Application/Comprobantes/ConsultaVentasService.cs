using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Domain.Ventas;
using Ventas.Domain.Comprobantes;

namespace Ventas.Application.Comprobantes;

/// <summary>
/// Consulta de una venta (RF-16) y de un comprobante (RF-17). El operador solo ve los del punto de
/// venta de su sesión (RN-19): los de otro punto de venta responden acceso denegado.
/// </summary>
public sealed class ConsultaVentasService(
    IConsultaVentasRepository consultas,
    IPuntoVentaRepository puntosVenta,
    ICatalogoRepository catalogos,
    IUsuarioActual usuarioActual)
{
    public const int MaximoRecientes = 20;

    public async Task<ComprobanteDetalleDto> ObtenerComprobanteAsync(long codigoComprobante, CancellationToken ct)
    {
        var comprobante = await consultas.ObtenerComprobanteAsync(codigoComprobante, ct)
            ?? throw new NoEncontradoException($"No existe el comprobante {codigoComprobante}.");
        VerificarPuntoVenta(comprobante.CodigoPuntoVenta, $"El comprobante {comprobante.Serie}-{comprobante.Numero}");

        var codigoTransaccion = comprobante.Movimientos.Select(m => m.CodigoTransaccion).FirstOrDefault();
        var registro = await consultas.ObtenerVentaAsync(codigoTransaccion, ct)
            ?? throw new NoEncontradoException($"No existe la venta del comprobante {comprobante.Serie}-{comprobante.Numero}.");

        return await MapearAsync(comprobante, registro, ct);
    }

    /// <summary>
    /// Busca por tipo y número en el punto de venta de la sesión: la serie es la del tipo en ese punto
    /// de venta y el número se completa con ceros a 7 dígitos (RF-11, RN-19).
    /// </summary>
    public async Task<long> BuscarComprobanteAsync(string? tipoComprobante, string? numero, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(numero) || !int.TryParse(numero.Trim(), out var correlativo) || correlativo <= 0)
            throw new NoEncontradoException("Indique un número de comprobante válido.");

        var serie = TiposComprobante.FormarSerie(tipoComprobante ?? string.Empty, usuarioActual.CodigoPuntoVenta);
        var numeroCompleto = TiposComprobante.FormatearNumero(correlativo);

        return await consultas.BuscarCodigoComprobanteAsync(serie, numeroCompleto, ct)
            ?? throw new NoEncontradoException($"No existe el comprobante {serie}-{numeroCompleto}.");
    }

    /// <summary>Código del comprobante de una venta, para mostrarla (RF-16).</summary>
    public async Task<long> BuscarComprobanteDeVentaAsync(long codigoTransaccion, CancellationToken ct)
    {
        var registro = await consultas.ObtenerVentaAsync(codigoTransaccion, ct);
        if (registro is null || registro.Venta.Situacion != SituacionVenta.Emitida)
            throw new NoEncontradoException($"No existe una venta activa con el número de transacción {codigoTransaccion}.");
        VerificarPuntoVenta(registro.Venta.CodigoPuntoVenta, $"La venta {codigoTransaccion}");

        return registro.Venta.CodigoComprobante
            ?? throw new NoEncontradoException($"La venta {codigoTransaccion} no tiene comprobante emitido.");
    }

    public async Task<IReadOnlyList<VentaResumenDto>> ListarRecientesAsync(CancellationToken ct)
    {
        var filas = await consultas.ListarAsync(
            new FiltroVentasRegistradas(usuarioActual.CodigoPuntoVenta, Maximo: MaximoRecientes), ct);

        return filas
            .Select(f => new VentaResumenDto(f.CodigoComprobante, f.CodigoTransaccion, f.FechaEmision,
                $"{f.Serie}-{f.Numero}", f.NombreCliente, f.MontoTotal))
            .ToList();
    }

    private void VerificarPuntoVenta(string codigoPuntoVenta, string recurso)
    {
        if (!string.Equals(codigoPuntoVenta.Trim(), usuarioActual.CodigoPuntoVenta.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new AccesoDenegadoException($"{recurso} pertenece a otro punto de venta.");
    }

    private async Task<ComprobanteDetalleDto> MapearAsync(Comprobante c, VentaRegistrada registro, CancellationToken ct)
    {
        var descripciones = await Descripciones.CargarAsync(catalogos, ct, conBancos: true);
        var puntoVenta = await puntosVenta.ObtenerAsync(c.CodigoPuntoVenta, ct);
        var venta = registro.Venta;

        return new ComprobanteDetalleDto(
            c.Id,
            venta.Id,
            descripciones.TipoComprobante(c.TipoComprobante),
            c.Serie,
            c.Numero,
            c.FechaEmision,
            Descripciones.Estado(c.Estado),
            descripciones.FormaPago(c.FormaPago),
            $"Soles ({venta.CodigoMoneda})",
            c.CodigoPuntoVenta,
            puntoVenta?.Descripcion?.Trim() ?? c.CodigoPuntoVenta,
            c.ZonaVenta,
            c.NombreCliente,
            $"{descripciones.TipoDocumento(c.TipoDocumento)} {c.NumeroDocumento}".Trim(),
            c.DireccionCliente,
            c.SubTotal,
            c.MontoDescuento,
            c.MontoIgv,
            c.MontoTotal,
            c.MontoPagado,
            c.Detalles.OrderBy(d => d.Item)
                .Select(d => new DetalleComprobanteDto(d.Item, d.CodigoArticulo.Trim(), d.NombreArticulo, d.UnidadMedida,
                    d.Cantidad, d.PrecioUnitario, d.SubTotal - d.Igv, d.Igv, d.SubTotal))
                .ToList(),
            c.Pagos
                .Select(p => new PagoComprobanteDto(descripciones.MedioPago(p.MedioPago.Tipo),
                    p.MedioPago.CodigoBanco is null ? null : descripciones.Banco(p.MedioPago.CodigoBanco),
                    p.MedioPago.NumeroOperacion, p.Monto))
                .ToList(),
            string.Join(" · ", c.Movimientos.OrderBy(m => m.Item).Select(m => $"ítem {m.Item} ↔ línea {m.LineaTransaccion}")),
            venta.CodigoTarifaCanal,
            venta.CodigoAlmacen,
            venta.TipoOrden,
            registro.Usuario,
            venta.FechaVenta);
    }
}

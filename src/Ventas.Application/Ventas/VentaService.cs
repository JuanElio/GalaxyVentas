using FluentValidation;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Comun;
using Ventas.Domain.Maestros;

namespace Ventas.Application.Ventas;

/// <summary>
/// Casos de uso de la pantalla de venta. La cotización calcula los importes con las mismas reglas
/// de Domain que se aplicarán al registrar, para que la pantalla no repita el cálculo.
/// </summary>
public sealed class VentaService(
    IArticuloRepository articulos,
    IPuntoVentaRepository puntosVenta,
    IStockService stock,
    ICorrelativoService correlativos,
    IUsuarioActual usuarioActual,
    IValidator<CotizarVentaDto> validadorCotizacion,
    TimeProvider reloj)
{
    public async Task<ContextoVentaDto> ObtenerContextoAsync(CancellationToken ct)
    {
        var codigoPuntoVenta = usuarioActual.CodigoPuntoVenta;
        var puntoVenta = await puntosVenta.ObtenerAsync(codigoPuntoVenta, ct);
        if (puntoVenta is not { Activo: true })
            throw new NoEncontradoException($"El punto de venta {codigoPuntoVenta} no existe o está inactivo.");

        var canal = await articulos.ObtenerCanalAsync(ParametrosVenta.Canal, ct);
        var almacen = await puntosVenta.ObtenerAlmacenPrincipalAsync(codigoPuntoVenta, ct);

        return new ContextoVentaDto(
            codigoPuntoVenta,
            puntoVenta.Descripcion?.Trim() ?? codigoPuntoVenta,
            canal?.Descripcion is { } descripcion ? $"{ParametrosVenta.Canal} · {descripcion.Trim()}" : ParametrosVenta.Canal,
            $"Soles ({ParametrosVenta.Moneda})",
            ParametrosVenta.TipoOrden,
            almacen?.Trim(),
            $"{usuarioActual.Nombre} ({usuarioActual.Login})");
    }

    /// <summary>
    /// Calcula precio (según la escala de cantidad), IGV, subtotal y stock de cada línea, y los
    /// totales de la venta. Las líneas que no se pueden vender llevan una observación.
    /// </summary>
    public async Task<CotizacionDto> CotizarAsync(CotizarVentaDto datos, CancellationToken ct)
    {
        await validadorCotizacion.ValidateAndThrowAsync(datos, ct);

        var fecha = reloj.GetLocalNow().DateTime;
        var codigos = datos.Lineas.Select(l => l.CodigoArticulo.Trim()).ToList();
        var encontrados = (await articulos.ObtenerConTarifasVigentesAsync(ParametrosVenta.TipoOrden, ParametrosVenta.Canal, codigos, fecha, ct))
            .ToDictionary(a => a.Articulo.Codigo, StringComparer.OrdinalIgnoreCase);
        var stockPorArticulo = await stock.ObtenerStockAsync(usuarioActual.CodigoPuntoVenta, codigos, ct);

        var lineas = datos.Lineas
            .Select((linea, indice) => Cotizar(indice + 1, linea, encontrados, stockPorArticulo, fecha))
            .ToList();
        var totales = Importes.Totalizar(lineas.Where(l => l.Tarifa is not null).Select(l => (l.SubTotal, l.Igv)));

        return new CotizacionDto(lineas, totales.MontoBruto, totales.SubTotal, totales.MontoIgv, totales.MontoTotal,
            Valida: lineas.Count > 0 && lineas.All(l => l.Observacion is null));
    }

    /// <summary>Serie del tipo de comprobante en el punto de venta de la sesión y su próximo número (RN-13).</summary>
    public async Task<NumeracionDto> ConsultarNumeracionAsync(string? tipoComprobante, CancellationToken ct)
    {
        var serie = TiposComprobante.FormarSerie(tipoComprobante ?? string.Empty, usuarioActual.CodigoPuntoVenta);
        var siguiente = await correlativos.ConsultarSiguienteAsync(serie, ct);

        return new NumeracionDto(serie, siguiente is { } numero ? TiposComprobante.FormatearNumero(numero) : null);
    }

    private static LineaCotizadaDto Cotizar(int numeroLinea, LineaVentaDto linea,
        IReadOnlyDictionary<string, ArticuloConTarifas> encontrados, IReadOnlyDictionary<string, decimal> stockPorArticulo,
        DateTime fecha)
    {
        var codigo = linea.CodigoArticulo.Trim();
        var disponible = stockPorArticulo.GetValueOrDefault(codigo);

        if (!encontrados.TryGetValue(codigo, out var articulo))
            return SinPrecio(numeroLinea, codigo, codigo, null, linea.Cantidad, disponible,
                $"El artículo {codigo} no existe, está inactivo o no tiene categoría, tarifa o IGV vigente.");

        var descripcion = articulo.Articulo.Descripcion?.Trim() ?? codigo;
        var unidad = articulo.Articulo.UnidadMedida?.Trim();
        var tarifa = Tarifa.Elegir(articulo.Tarifas, linea.Cantidad, fecha);
        if (tarifa is null)
            return SinPrecio(numeroLinea, codigo, descripcion, unidad, linea.Cantidad, disponible,
                $"No hay tarifa vigente para vender {linea.Cantidad:0} unidades de {codigo}.");

        var importes = Importes.CalcularLinea(linea.Cantidad, tarifa.Precio!.Value, articulo.TasaIgv);
        var observacion = linea.Cantidad > disponible
            ? $"Stock insuficiente: disponible {disponible:0.####}, solicitado {linea.Cantidad:0}."
            : null;

        return new LineaCotizadaDto(numeroLinea, codigo, descripcion, unidad, linea.Cantidad, tarifa.Precio,
            importes.ValorVenta, importes.Igv, importes.SubTotal, disponible, observacion);
    }

    private static LineaCotizadaDto SinPrecio(int numeroLinea, string codigo, string descripcion, string? unidad,
        decimal cantidad, decimal disponible, string observacion) =>
        new(numeroLinea, codigo, descripcion, unidad, cantidad, null, 0, 0, 0, disponible, observacion);
}

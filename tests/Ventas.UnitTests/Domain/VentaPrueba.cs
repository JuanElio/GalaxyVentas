using Ventas.Domain.Ventas;

namespace Ventas.UnitTests.Domain;

/// <summary>Construye ventas válidas para las pruebas.</summary>
internal static class VentaPrueba
{
    public const long CodigoCliente = 125;

    public static Venta Crear(string tipoComprobante = "B", string codigoPuntoVenta = "001") =>
        new(
            codigoPuntoVenta: codigoPuntoVenta,
            codigoCliente: CodigoCliente,
            tipoOrden: "V01",
            tipoLiquidacion: "01",
            codigoTarifaCanal: "01",
            codigoAlmacen: "ALM01",
            codigoMoneda: "PEN",
            formaPago: "C00",
            tipoComprobante: tipoComprobante,
            fechaVenta: new DateTime(2026, 10, 29, 10, 30, 0));
}

using Ventas.Application.Ventas;

namespace Ventas.Api.Models;

/// <summary>Respuesta de POST /Ventas/Registrar: la venta registrada y la página de su comprobante.</summary>
public sealed record VentaRegistradaRespuesta(VentaRegistradaDto Venta, string UrlComprobante);

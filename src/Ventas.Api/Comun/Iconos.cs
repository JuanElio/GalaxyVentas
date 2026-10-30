using Microsoft.AspNetCore.Html;

namespace Ventas.Api.Comun;

/// <summary>
/// Íconos SVG en línea (trazo de 24×24, color heredado con currentColor) para el menú y el inicio
/// de sesión, sin fuentes ni librerías externas.
/// </summary>
public static class Iconos
{
    private static readonly Dictionary<string, string> Trazos = new()
    {
        ["logo"] = """<path d="M3 3h2l2.4 11.2a2 2 0 0 0 2 1.6h7.7a2 2 0 0 0 1.9-1.4L21 8H6"/><circle cx="10" cy="20" r="1.4"/><circle cx="17" cy="20" r="1.4"/>""",
        ["inicio"] = """<path d="M3 11.5 12 4l9 7.5"/><path d="M5 10v10h5v-6h4v6h5V10"/>""",
        ["venta"] = """<rect x="3" y="4" width="18" height="16" rx="2"/><path d="M3 9h18"/><path d="M8 14h3"/><path d="M15 13v4"/><path d="M13 15h4"/>""",
        ["consultas"] = """<circle cx="11" cy="11" r="6.5"/><path d="m20 20-4.2-4.2"/>""",
        ["reporte"] = """<path d="M4 20V4"/><path d="M4 20h16"/><rect x="7" y="12" width="3" height="5"/><rect x="12" y="8" width="3" height="9"/><rect x="17" y="10" width="3" height="7"/>""",
        ["salir"] = """<path d="M15 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3"/><path d="M10 16l-4-4 4-4"/><path d="M6 12h10"/>""",
        ["menu"] = """<path d="M4 6h16"/><path d="M4 12h16"/><path d="M4 18h16"/>""",
        ["usuario"] = """<circle cx="12" cy="8" r="4"/><path d="M4 21c1.5-4 4.5-6 8-6s6.5 2 8 6"/>""",
        ["candado"] = """<rect x="4" y="10" width="16" height="11" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/>""",
        ["ojo"] = """<path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/>""",
        ["comprobante"] = """<path d="M6 3h12v18l-3-2-3 2-3-2-3 2z"/><path d="M9 8h6"/><path d="M9 12h6"/>""",
        ["stock"] = """<path d="M3 7l9-4 9 4-9 4z"/><path d="M3 7v10l9 4 9-4V7"/><path d="M12 11v10"/>""",
        ["alerta"] = """<circle cx="12" cy="12" r="9"/><path d="M12 8v5"/><path d="M12 16.5v.5"/>""",
    };

    public static IHtmlContent Svg(string nombre) => new HtmlString(
        $"""<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false">{Trazos[nombre]}</svg>""");
}

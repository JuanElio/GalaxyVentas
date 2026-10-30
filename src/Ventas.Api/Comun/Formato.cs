using System.Globalization;

namespace Ventas.Api.Comun;

/// <summary>Formato de presentación en las vistas: importes con 2 decimales (P-11) y fechas de Perú.</summary>
public static class Formato
{
    private static readonly CultureInfo Peru = CultureInfo.GetCultureInfo("es-PE");

    /// <summary>Redondeo comercial a 2 decimales solo para mostrar; el valor grabado conserva 4.</summary>
    public static string Importe(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero).ToString("N2", Peru);

    public static string Soles(decimal valor) => $"S/ {Importe(valor)}";

    public static string Cantidad(decimal valor) => valor.ToString("0.####", Peru);

    public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", Peru);

    public static string FechaHora(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", Peru);
}

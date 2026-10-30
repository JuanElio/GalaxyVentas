namespace Ventas.Domain.Maestros;

/// <summary>Códigos del catálogo 2 (tipo de documento) y 5 (tipo de persona) de SGP_DETALLE_TABLA.</summary>
public static class TiposDocumento
{
    public const string Ruc = "1";
    public const string Dni = "2";
    public const string Otros = "3";
    public const string CarnetExtranjeria = "4";

    public const int LongitudDni = 8;
    public const int LongitudRuc = 11;

    public const string PersonaNatural = "1";
    public const string PersonaJuridica = "2";

    /// <summary>
    /// DNI → persona natural; RUC que empieza con 20 → persona jurídica; otro RUC (10, 15, 17)
    /// → persona natural con negocio (P-20).
    /// </summary>
    public static string DeducirTipoPersona(string tipoDocumento, string numeroDocumento) =>
        tipoDocumento == Ruc && numeroDocumento.StartsWith("20", StringComparison.Ordinal)
            ? PersonaJuridica
            : PersonaNatural;
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ventas.Infrastructure.Persistence;

/// <summary>Conversores de los flags y códigos heredados del esquema.</summary>
internal static class Conversiones
{
    /// <summary>Campo privado <c>bool? _activo</c> que las entidades exponen como <c>Activo</c>.</summary>
    public const string CampoActivo = "_activo";

    /// <summary>
    /// chrSituacionRegistro: 'A' = activo; otro valor = inactivo. La propiedad debe ser bool?
    /// para que un NULL de la columna llegue como null (EF no pasa los NULL al conversor).
    /// </summary>
    public static readonly ValueConverter<bool?, string?> SituacionRegistro = new(
        activo => activo == true ? "A" : "I",
        valor => valor == "A");

    /// <summary>Flags 'S'/'N' (chrFlagFacturado, vchFlagMedioPago, chrFlagPendienteAjuste...).</summary>
    public static readonly ValueConverter<bool, string> FlagSN = new(
        valor => valor ? "S" : "N",
        texto => texto == "S");

    /// <summary>
    /// Código de punto de venta: el dominio usa '001' y la BD lo guarda con espacio inicial (' 001'),
    /// igual que SGP_PUNTO_VENTA y SGP_USUARIO, porque otros procesos lo comparan por igualdad.
    /// </summary>
    public static readonly ValueConverter<string, string> CodigoPuntoVenta = new(
        codigo => codigo.Trim().PadLeft(4),
        texto => texto.Trim());

    /// <summary>
    /// Canal en la cabecera de la venta, normalizado como el procedimiento de referencia: sin
    /// espacios y, si tiene 2 caracteres, con un espacio delante ('01' → ' 01').
    /// </summary>
    public static readonly ValueConverter<string, string> CanalCabecera = new(
        codigo => codigo.Trim().Length == 2 ? " " + codigo.Trim() : codigo.Trim(),
        texto => texto.Trim());

    /// <summary>Recorta el texto a la longitud de la columna al grabar.</summary>
    public static ValueConverter<string, string> Recortar(int longitud) => new(
        texto => texto.Length > longitud ? texto.Substring(0, longitud) : texto,
        texto => texto);

    /// <summary>Mapea el campo <c>_activo</c> a la columna de situación del registro (borrado lógico).</summary>
    public static void MapearSituacionRegistro<T>(this EntityTypeBuilder<T> builder,
        string tipoColumna = "char(1)", string columna = "chrSituacionRegistro") where T : class
    {
        builder.Property<bool?>(CampoActivo).HasColumnName(columna).HasColumnType(tipoColumna)
            .HasConversion(SituacionRegistro);
        builder.Ignore("Activo");
    }

    /// <summary>
    /// Columna de situación de las tablas que el dominio no da de baja (detalles, movimientos):
    /// se mapea como propiedad de sombra y se graba 'A' (registro activo).
    /// </summary>
    public const string PropiedadSituacion = "SituacionRegistro";

    public static void MapearSituacionActiva<T>(this EntityTypeBuilder<T> builder) where T : class =>
        builder.Property<string>(PropiedadSituacion).HasColumnName("chrSituacionRegistro").HasColumnType("char(1)")
            .ValorAlInsertar("A");
}

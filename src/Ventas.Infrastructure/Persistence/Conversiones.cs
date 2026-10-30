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

    /// <summary>Mapea el campo <c>_activo</c> a la columna de situación del registro (borrado lógico).</summary>
    public static void MapearSituacionRegistro<T>(this EntityTypeBuilder<T> builder,
        string tipoColumna = "char(1)", string columna = "chrSituacionRegistro") where T : class
    {
        builder.Property<bool?>(CampoActivo).HasColumnName(columna).HasColumnType(tipoColumna)
            .HasConversion(SituacionRegistro);
        builder.Ignore("Activo");
    }
}

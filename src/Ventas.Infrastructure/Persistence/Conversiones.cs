using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ventas.Infrastructure.Persistence;

/// <summary>Conversores de los flags y códigos heredados del esquema.</summary>
internal static class Conversiones
{
    /// <summary>
    /// chrSituacionRegistro: 'A' = activo; otro valor = inactivo. La propiedad debe ser bool?
    /// para que un NULL de la columna llegue como null (EF no pasa los NULL al conversor).
    /// </summary>
    public static readonly ValueConverter<bool?, string?> SituacionRegistro = new(
        activo => activo == true ? "A" : "I",
        valor => valor == "A");
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ventas.Infrastructure.Persistence;

/// <summary>
/// Columnas de auditoría como propiedades de sombra (RF-18): el dominio no las conoce y las
/// rellena <see cref="AuditoriaInterceptor"/>.
/// </summary>
internal static class Auditoria
{
    public const string Creador = nameof(Creador);
    public const string FechaCreacion = nameof(FechaCreacion);
    public const string Modificador = nameof(Modificador);
    public const string FechaModificacion = nameof(FechaModificacion);

    /// <summary>Tablas SGP_: vchCreador, dtmFechaCreacion, vchModificador, dtmFechaModificacion.</summary>
    public static void MapearAuditoriaSgp<T>(this EntityTypeBuilder<T> builder) where T : class
    {
        builder.Property<string>(Creador).HasColumnName("vchCreador").HasMaxLength(16).IsUnicode(false);
        builder.Property<DateTime>(FechaCreacion).HasColumnName("dtmFechaCreacion").HasColumnType("datetime");
        builder.Property<string?>(Modificador).HasColumnName("vchModificador").HasMaxLength(16).IsUnicode(false);
        builder.Property<DateTime?>(FechaModificacion).HasColumnName("dtmFechaModificacion").HasColumnType("datetime");
    }

    /// <summary>Tablas PGA_: Creador, FechaCreacion, Modificador, FechaModificacion.</summary>
    public static void MapearAuditoriaPga<T>(this EntityTypeBuilder<T> builder) where T : class
    {
        builder.Property<string?>(Creador).HasColumnName("Creador").HasMaxLength(50).IsUnicode(false);
        builder.Property<DateTime?>(FechaCreacion).HasColumnName("FechaCreacion").HasColumnType("datetime");
        builder.Property<string?>(Modificador).HasColumnName("Modificador").HasMaxLength(50).IsUnicode(false);
        builder.Property<DateTime?>(FechaModificacion).HasColumnName("FechaModificacion").HasColumnType("datetime");
    }
}

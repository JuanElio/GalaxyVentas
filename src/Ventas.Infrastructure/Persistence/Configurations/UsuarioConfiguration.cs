using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ventas.Domain.Seguridad;

namespace Ventas.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    /// <summary>Propiedad de sombra con la contraseña; solo la lee <see cref="Seguridad.VerificadorClave"/>.</summary>
    public const string Clave = nameof(Clave);

    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("SGP_USUARIO");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("numCodigoUsuario").ValueGeneratedOnAdd();
        builder.Property(u => u.Login).HasColumnName("vchLogin").HasMaxLength(16).IsUnicode(false);
        builder.Property(u => u.Nombre).HasColumnName("vchNombre").HasMaxLength(64).IsUnicode(false);
        builder.Property(u => u.CodigoPuntoVenta).HasColumnName("vchCodPuntoVenta").HasMaxLength(4).IsUnicode(false);
        builder.Property<bool?>("_activo").HasColumnName("chrSituacionRegistro").HasColumnType("char(1)")
            .HasConversion(Conversiones.SituacionRegistro);
        builder.Ignore(u => u.Activo);
        builder.Property(u => u.InicioVigencia).HasColumnName("dtmFechaInicioAct").HasColumnType("datetime");
        builder.Property(u => u.FinVigencia).HasColumnName("dtmFechaFinAct").HasColumnType("datetime");
        builder.Property(u => u.FechaExpiraClave).HasColumnName("dtmFechaExpiraClave").HasColumnType("datetime");
        builder.Ignore(u => u.TienePuntoVenta);

        builder.Property<string>(Clave).HasColumnName("vchPassword").HasMaxLength(16).IsUnicode(false);
        builder.MapearAuditoriaSgp();
    }
}

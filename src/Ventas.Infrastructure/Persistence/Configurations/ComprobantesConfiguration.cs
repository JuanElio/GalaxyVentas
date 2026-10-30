using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Maestros;

namespace Ventas.Infrastructure.Persistence.Configurations;

internal sealed class ImpresoraConfiguration : IEntityTypeConfiguration<Impresora>
{
    public void Configure(EntityTypeBuilder<Impresora> builder)
    {
        builder.ToTable("SGP_IMPRESORA");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id).HasColumnName("intCodImpresora").ValueGeneratedNever();
        builder.Property(i => i.Serie).HasColumnName("vchNumSerie").HasMaxLength(4).IsUnicode(false);
        // Pese al prefijo vch, la columna es int
        builder.Property(i => i.Correlativo).HasColumnName("vchCorrelativo");
        builder.Ignore(i => i.SiguienteNumero);
        builder.MapearSituacionRegistro();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class PuntoVentaAlmacenConfiguration : IEntityTypeConfiguration<PuntoVentaAlmacen>
{
    public void Configure(EntityTypeBuilder<PuntoVentaAlmacen> builder)
    {
        builder.ToTable("SGP_PUNTO_VENTA_ALMACEN");
        builder.HasKey(a => new { a.CodigoAlmacen, a.CodigoPuntoVenta });

        builder.Property(a => a.CodigoAlmacen).HasColumnName("vchCodAlmacen").HasMaxLength(16).IsUnicode(false);
        builder.Property(a => a.CodigoPuntoVenta).HasColumnName("vchCodPuntoVenta").HasMaxLength(4).IsUnicode(false);
        builder.MapearSituacionRegistro();
        builder.MapearAuditoriaSgp();
    }
}

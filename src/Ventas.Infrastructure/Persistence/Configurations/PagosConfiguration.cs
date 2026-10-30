using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ventas.Domain.Pagos;

namespace Ventas.Infrastructure.Persistence.Configurations;

/// <summary>Medio de pago; valores fijos de pga_hemeroteca_comprobante_cancelar.</summary>
internal sealed class MedioPagoConfiguration : IEntityTypeConfiguration<MedioPago>
{
    public void Configure(EntityTypeBuilder<MedioPago> builder)
    {
        builder.ToTable("SGP_MEDIO_PAGO_BS");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("numCodMediPago").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>().ValueGeneratedOnAdd();
        builder.Property(m => m.CodigoTransaccion).HasColumnName("numCodTransaccion").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>();
        // vchMedioPago: '1' efectivo ... '5' boleta de depósito (P-09)
        builder.Property(m => m.Tipo).HasColumnName("vchMedioPago").HasMaxLength(2).IsUnicode(false)
            .HasConversion(t => ((int)t).ToString(), texto => (TipoMedioPago)int.Parse(texto));
        builder.Property(m => m.Monto).HasColumnName("numMonto").HasColumnType("numeric(15,4)");
        builder.Property(m => m.MontoOrigen).HasColumnName("numMontoOrigen").HasColumnType("numeric(15,4)");
        builder.Property(m => m.CodigoMoneda).HasColumnName("vchCodMoneda").HasMaxLength(3).IsUnicode(false);
        builder.Property(m => m.TipoCambio).HasColumnName("numTipoCambio").HasColumnType("numeric(9,4)");
        builder.Property(m => m.CodigoBanco).HasColumnName("vchCodBanco").HasMaxLength(12).IsUnicode(false);
        builder.Property(m => m.NumeroOperacion).HasColumnName("vchNumDocumento").HasMaxLength(40).IsUnicode(false);

        builder.Property<string>("Estado").HasColumnName("vchEstado").HasMaxLength(2).IsUnicode(false).ValorAlInsertar("2"); // cancelado
        builder.Property<int?>("Linea").HasColumnName("numLinea").ValorAlInsertar(0);
        builder.Property<string>("FlagPendienteAjuste").HasColumnName("chrFlagPendienteAjuste").HasMaxLength(1).IsUnicode(false).ValorAlInsertar("N");
        builder.Property<string>("TipoCodigo").HasColumnName("vchTipoCodigo").HasMaxLength(3).IsUnicode(false).ValorAlInsertar(string.Empty);
        builder.Property<string>("NumdocCodigo").HasColumnName("vchNumdocCodigo").HasMaxLength(30).IsUnicode(false).ValorAlInsertar(string.Empty);
        builder.Property<string>("Origen").HasColumnName("vchOrigen").HasMaxLength(20).IsUnicode(false).ValorAlInsertar("PGA");
        builder.MapearSituacionActiva();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class PagoComprobanteConfiguration : IEntityTypeConfiguration<PagoComprobante>
{
    public void Configure(EntityTypeBuilder<PagoComprobante> builder)
    {
        builder.ToTable("SGP_MEDIO_PAGO_COMPROBANTE_BS");
        builder.HasKey(p => new { p.CodigoComprobante, p.CodigoMedioPago });

        builder.Property(p => p.CodigoComprobante).HasColumnName("numCodComprobante").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>();
        builder.Property(p => p.CodigoMedioPago).HasColumnName("numCodMediPago").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>();
        builder.Property(p => p.Monto).HasColumnName("numMontoPago").HasColumnType("numeric(15,4)");
        builder.HasOne(p => p.MedioPago).WithMany().HasForeignKey(p => p.CodigoMedioPago);
        builder.MapearSituacionActiva();
        builder.MapearAuditoriaSgp();
    }
}

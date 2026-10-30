using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ventas.Domain.Ventas;

namespace Ventas.Infrastructure.Persistence.Configurations;

/// <summary>Cabecera de la venta; valores fijos de pga_cabecera_transaccion_hemeroteca_agregar.</summary>
internal sealed class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public const string GuidRegistro = nameof(GuidRegistro);

    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("SGP_CABECERA_TRANSACCION_BS");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasColumnName("numCodTransaccion").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>().ValueGeneratedOnAdd();
        builder.Property(v => v.TipoOrden).HasColumnName("vchTipoOrden").HasMaxLength(3).IsUnicode(false);
        builder.Property(v => v.CodigoPuntoVenta).HasColumnName("vchCodPuntoVenta").HasMaxLength(4).IsUnicode(false)
            .HasConversion(Conversiones.CodigoPuntoVenta);
        // numCodCliente es int aquí y numeric(18,0) en SGP_CLIENTE
        builder.Property(v => v.CodigoCliente).HasColumnName("numCodCliente").HasConversion<int>();
        builder.Property(v => v.FormaPago).HasColumnName("vchFormaPago").HasMaxLength(3).IsUnicode(false);
        builder.Property(v => v.TipoComprobante).HasColumnName("vchTipComprobante").HasMaxLength(2).IsUnicode(false);
        builder.Property(v => v.MontoBruto).HasColumnName("numMontoBruto").HasColumnType("numeric(15,4)");
        builder.Property(v => v.MontoDescuento).HasColumnName("numMontoDescuento").HasColumnType("numeric(15,4)");
        builder.Property(v => v.MontoRecargo).HasColumnName("numMontoRecargo").HasColumnType("numeric(15,4)");
        builder.Property(v => v.SubTotal).HasColumnName("numSubTotal").HasColumnType("numeric(15,4)");
        builder.Property(v => v.MontoIgv).HasColumnName("numMontoIGV").HasColumnType("numeric(15,4)");
        builder.Property(v => v.MontoTotal).HasColumnName("numMontoTotal").HasColumnType("numeric(15,4)");
        builder.Property(v => v.TipoCambio).HasColumnName("numTipoCambio").HasColumnType("numeric(9,4)");
        // numeric(9,4) frente a numeric(18,0) en SGP_COMPROBANTE_BS (P-12)
        builder.Property(v => v.CodigoComprobante).HasColumnName("numCodComprobante").HasColumnType("numeric(9,4)")
            .HasConversion<decimal?>();
        builder.Property(v => v.CodigoAlmacen).HasColumnName("vchCodAlmacen").HasMaxLength(8).IsUnicode(false);
        builder.Property(v => v.CodigoTarifaCanal).HasColumnName("vchCodTarifaCanal").HasMaxLength(20).IsUnicode(false)
            .HasConversion(Conversiones.CanalCabecera);
        builder.Property(v => v.CodigoMoneda).HasColumnName("vchCodMoneda").HasMaxLength(6).IsUnicode(false);
        builder.Property(v => v.FechaVenta).HasColumnName("dtmFechaVenta").HasColumnType("datetime");
        builder.Property(v => v.Situacion).HasColumnName("chrSituacionRegistro").HasColumnType("char(1)")
            .HasConversion(s => s == SituacionVenta.Emitida ? "A" : "I", texto => texto == "A" ? SituacionVenta.Emitida : SituacionVenta.Registrada);
        builder.Property(v => v.TipoLiquidacion).HasColumnName("vchTipLiquidacion").HasColumnType("char(2)");

        builder.Property<int?>("Estado").HasColumnName("intEstado").ValorAlInsertar(1); // P-10
        builder.Property<string>("TipoBase").HasColumnName("vchTipBase").HasMaxLength(24).IsUnicode(false).ValorAlInsertar(string.Empty);
        builder.Property<string>("FlagClienteAgencia").HasColumnName("chrFlagClienteAgencia").HasColumnType("char(1)").ValorAlInsertar("0");
        builder.Property<string>(GuidRegistro).HasColumnName("vchGuidRegistro").HasMaxLength(100).IsUnicode(false);
        builder.MapearAuditoriaSgp();

        builder.HasMany(v => v.Detalles).WithOne().HasForeignKey(d => d.CodigoTransaccion);
        builder.Navigation(v => v.Detalles).HasField("_detalles").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.ToTable("SGP_DETALLE_TRANSACCION_BS");
        builder.HasKey(d => new { d.CodigoTransaccion, d.NumeroLinea });

        builder.Property(d => d.CodigoTransaccion).HasColumnName("numCodTransaccion").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>();
        builder.Property(d => d.NumeroLinea).HasColumnName("numLinTransaccion").ValueGeneratedNever();
        builder.Property(d => d.CodigoArticulo).HasColumnName("vchCodArticulo").HasMaxLength(16).IsUnicode(false);
        builder.Property(d => d.UnidadMedida).HasColumnName("vchUnidadMedida").HasMaxLength(3).IsUnicode(false);
        builder.Property(d => d.Cantidad).HasColumnName("numCantidad").HasColumnType("numeric(15,4)");
        builder.Property(d => d.Tarifa).HasColumnName("numTarifa").HasColumnType("numeric(15,4)");
        builder.Property(d => d.MontoBruto).HasColumnName("numMontoBruto").HasColumnType("numeric(15,4)");
        builder.Property(d => d.SubTotal).HasColumnName("numSubTotal").HasColumnType("numeric(15,4)");
        builder.Property(d => d.Igv).HasColumnName("numIGV").HasColumnType("numeric(15,4)");
        builder.Ignore(d => d.ValorVenta);
        builder.MapearSituacionActiva();
        builder.MapearAuditoriaSgp();
    }
}

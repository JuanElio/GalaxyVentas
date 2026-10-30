using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Maestros;

namespace Ventas.Infrastructure.Persistence.Configurations;

/// <summary>Comprobante; valores fijos de pga_hemeroteca_emitir_comprobante.</summary>
internal sealed class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
{
    public const string CodigoTransaccion = nameof(CodigoTransaccion);

    public void Configure(EntityTypeBuilder<Comprobante> builder)
    {
        builder.ToTable("SGP_COMPROBANTE_BS");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("numCodComprobante").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>().ValueGeneratedOnAdd();
        builder.Property(c => c.CodigoCliente).HasColumnName("numCodCliente").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>();
        builder.Property(c => c.CodigoPuntoVenta).HasColumnName("vchCodPuntoVenta").HasMaxLength(4).IsUnicode(false)
            .HasConversion(Conversiones.CodigoPuntoVenta);
        builder.Property(c => c.ZonaVenta).HasColumnName("vchZonaVenta").HasMaxLength(50).IsUnicode(false);
        builder.Property(c => c.NombreCliente).HasColumnName("vchNomComprobante").HasMaxLength(64).IsUnicode(false)
            .HasConversion(Conversiones.Recortar(64));
        builder.Property(c => c.DireccionCliente).HasColumnName("vchDirComprobante").HasMaxLength(128).IsUnicode(false);
        builder.Property(c => c.TipoDocumento).HasColumnName("vchTipoDocumento").HasMaxLength(2).IsUnicode(false);
        builder.Property(c => c.NumeroDocumento).HasColumnName("vchNumDocumento").HasMaxLength(16).IsUnicode(false);
        builder.Property(c => c.FormaPago).HasColumnName("vchFormaPago").HasMaxLength(3).IsUnicode(false);
        builder.Property(c => c.TipoComprobante).HasColumnName("vchTipoComprobante").HasMaxLength(2).IsUnicode(false);
        builder.Property(c => c.Serie).HasColumnName("vchNumSerie").HasMaxLength(4).IsUnicode(false);
        builder.Property(c => c.Numero).HasColumnName("vchNumComprobante").HasMaxLength(10).IsUnicode(false);
        builder.Property(c => c.FechaEmision).HasColumnName("dtmFecEmision").HasColumnType("datetime");
        builder.Property(c => c.TipoCambio).HasColumnName("numTipoCambio").HasColumnType("numeric(9,4)");
        // Catálogo 25 (estado del comprobante): '1' emitido, '2' cancelado
        builder.Property(c => c.Estado).HasColumnName("vchEstado").HasMaxLength(2).IsUnicode(false)
            .HasConversion(e => e == EstadoComprobante.Cancelado ? "2" : "1",
                texto => texto == "2" ? EstadoComprobante.Cancelado : EstadoComprobante.Emitido);
        builder.Property(c => c.Facturado).HasColumnName("chrFlagFacturado").HasColumnType("char(1)")
            .HasConversion(Conversiones.FlagSN);
        builder.Property(c => c.TieneMedioPago).HasColumnName("vchFlagMedioPago").HasMaxLength(1).IsUnicode(false)
            .HasConversion(Conversiones.FlagSN);
        builder.Property(c => c.MontoBruto).HasColumnName("numMontoBruto").HasColumnType("numeric(15,4)");
        builder.Property(c => c.MontoDescuento).HasColumnName("numMontoDescuento").HasColumnType("numeric(15,4)");
        builder.Property(c => c.MontoRecargo).HasColumnName("numMontoRecargo").HasColumnType("numeric(15,4)");
        builder.Property(c => c.SubTotal).HasColumnName("numSubTotal").HasColumnType("numeric(15,4)");
        builder.Property(c => c.MontoIgv).HasColumnName("numMontoIGV").HasColumnType("numeric(15,4)");
        builder.Property(c => c.MontoTotal).HasColumnName("numMontoTotal").HasColumnType("numeric(15,4)");
        builder.Property(c => c.MontoPagado).HasColumnName("numMontoPagado").HasColumnType("numeric(15,4)");
        // SGP_COMPROBANTE_BS no tiene columna de moneda
        builder.Ignore(c => c.CodigoMoneda);

        builder.Property<string>("FlagMigrado").HasColumnName("chrFlagMigrado").HasColumnType("char(1)").ValorAlInsertar("N");
        builder.Property<string>("Observaciones").HasColumnName("vchObservaciones").HasMaxLength(128).IsUnicode(false).ValorAlInsertar(string.Empty);
        builder.Property<string>("OrdenVenta").HasColumnName("vchOrdenVenta").HasMaxLength(32).IsUnicode(false).ValorAlInsertar("0");
        builder.Property<string>("FlagClienteAgencia").HasColumnName("chrFlagClienteAgencia").HasColumnType("char(1)").ValorAlInsertar("0");
        builder.Property<string>(CodigoTransaccion).HasColumnName("vchCodTransaccion").HasMaxLength(128).IsUnicode(false);
        builder.MapearSituacionActiva();
        builder.MapearAuditoriaSgp();

        builder.HasMany(c => c.Detalles).WithOne().HasForeignKey(d => d.CodigoComprobante);
        builder.Navigation(c => c.Detalles).HasField("_detalles").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(c => c.Movimientos).WithOne().HasForeignKey(m => m.CodigoComprobante);
        builder.Navigation(c => c.Movimientos).HasField("_movimientos").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(c => c.Pagos).WithOne().HasForeignKey(p => p.CodigoComprobante);
        builder.Navigation(c => c.Pagos).HasField("_pagos").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class DetalleComprobanteConfiguration : IEntityTypeConfiguration<DetalleComprobante>
{
    public void Configure(EntityTypeBuilder<DetalleComprobante> builder)
    {
        builder.ToTable("SGP_DETALLE_COMPROBANTE_BS");
        builder.HasKey(d => new { d.CodigoComprobante, d.Item });

        builder.Property(d => d.CodigoComprobante).HasColumnName("numCodComprobante").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>();
        builder.Property(d => d.Item).HasColumnName("numItem").ValueGeneratedNever();
        builder.Property(d => d.CodigoArticulo).HasColumnName("vchCodArticulo").HasMaxLength(16).IsUnicode(false);
        builder.Property(d => d.NombreArticulo).HasColumnName("vchNomArticulo").HasMaxLength(128).IsUnicode(false);
        builder.Property(d => d.UnidadMedida).HasColumnName("vchUnidadMedida").HasMaxLength(3).IsUnicode(false);
        builder.Property(d => d.Cantidad).HasColumnName("numCantidad").HasColumnType("numeric(15,4)");
        builder.Property(d => d.PrecioUnitario).HasColumnName("numPrecioUnitario").HasColumnType("numeric(15,4)");
        builder.Property(d => d.MontoBruto).HasColumnName("numMontoBruto").HasColumnType("numeric(15,4)");
        builder.Property(d => d.MontoDescuento).HasColumnName("numMontoDescuento").HasColumnType("numeric(15,4)");
        builder.Property(d => d.MontoRecargo).HasColumnName("numMontoRecargo").HasColumnType("numeric(15,4)");
        builder.Property(d => d.SubTotal).HasColumnName("numSubTotal").HasColumnType("numeric(15,4)");
        builder.Property(d => d.Igv).HasColumnName("numIGV").HasColumnType("numeric(15,4)");

        // Columnas de avisos y publicaciones, sin uso en la venta de artículos: se graban en 0
        builder.Property<int?>("Veces").HasColumnName("numVeces").ValorAlInsertar(0);
        builder.Property<decimal?>("Pagina").HasColumnName("numPagina").HasColumnType("numeric(15,4)").ValorAlInsertar(0m);
        builder.Property<int?>("Palabra").HasColumnName("numPalabra").ValorAlInsertar(0);
        builder.Property<int?>("Ancho").HasColumnName("numAncho").ValorAlInsertar(0);
        builder.Property<int?>("Alto").HasColumnName("numAlto").ValorAlInsertar(0);
        builder.Property<int?>("Ejemplar").HasColumnName("numEjemplar").ValorAlInsertar(0);
        builder.Property<int?>("Dia").HasColumnName("numDia").ValorAlInsertar(0);
        builder.MapearSituacionActiva();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class MovimientoComprobanteConfiguration : IEntityTypeConfiguration<MovimientoComprobante>
{
    public void Configure(EntityTypeBuilder<MovimientoComprobante> builder)
    {
        builder.ToTable("SGP_MOVIMIENTO_TRANSACCION_BS");
        builder.HasKey(m => new { m.CodigoComprobante, m.Item, m.CodigoTransaccion, m.LineaTransaccion });

        builder.Property(m => m.CodigoComprobante).HasColumnName("numCodComprobante").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>();
        builder.Property(m => m.Item).HasColumnName("numItem").ValueGeneratedNever();
        builder.Property(m => m.CodigoTransaccion).HasColumnName("numCodTransaccion").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>();
        builder.Property(m => m.LineaTransaccion).HasColumnName("numLinTransaccion").ValueGeneratedNever();
        builder.MapearSituacionActiva();
        builder.MapearAuditoriaSgp();
    }
}

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

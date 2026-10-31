using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ventas.Domain.Inventario;
using Ventas.Domain.Maestros;

namespace Ventas.Infrastructure.Persistence.Configurations;

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("SGP_CLIENTE");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("numCodCliente").HasColumnType("numeric(18,0)")
            .HasConversion<decimal>().ValueGeneratedOnAdd();
        builder.Property(c => c.TipoDocumento).HasColumnName("vchTipDocumento").HasMaxLength(2).IsUnicode(false);
        builder.Property(c => c.NumeroDocumento).HasColumnName("vchNumDocumento").HasMaxLength(16).IsUnicode(false);
        builder.Property(c => c.TipoPersona).HasColumnName("vchTipPersona").HasMaxLength(2).IsUnicode(false);
        builder.Property(c => c.Nombres).HasColumnName("vchNombres").HasMaxLength(128).IsUnicode(false);
        builder.Property(c => c.Apellidos).HasColumnName("vchApellidos").HasMaxLength(128).IsUnicode(false);
        builder.Property(c => c.RazonSocial).HasColumnName("vchRazonSocial").HasMaxLength(128).IsUnicode(false);
        builder.Property(c => c.DireccionFacturacion).HasColumnName("vchDirecFacturacion").HasMaxLength(128).IsUnicode(false);
        builder.Property(c => c.Telefono).HasColumnName("vchTelefono").HasMaxLength(15).IsUnicode(false);
        builder.Property(c => c.Email).HasColumnName("vchEmail").HasMaxLength(128).IsUnicode(false);
        builder.MapearSituacionRegistro();
        builder.Ignore(c => c.TieneRuc);
        builder.Ignore(c => c.EsPersonaJuridica);
        builder.Ignore(c => c.NombreCompleto);
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class ArticuloConfiguration : IEntityTypeConfiguration<Articulo>
{
    public void Configure(EntityTypeBuilder<Articulo> builder)
    {
        builder.ToTable("SGP_ARTICULO");
        builder.HasKey(a => a.Codigo);

        builder.Property(a => a.Codigo).HasColumnName("vchCodArticulo").HasMaxLength(16).IsUnicode(false);
        builder.Property(a => a.Descripcion).HasColumnName("vchDescripcion").HasMaxLength(64).IsUnicode(false);
        builder.Property(a => a.UnidadMedida).HasColumnName("vchUnidadMedida").HasMaxLength(3).IsUnicode(false);
        builder.Property(a => a.CodigoIva).HasColumnName("vchCodIva").HasMaxLength(9).IsUnicode(false);
        builder.MapearSituacionRegistro();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class CategoriaVentaConfiguration : IEntityTypeConfiguration<CategoriaVenta>
{
    public void Configure(EntityTypeBuilder<CategoriaVenta> builder)
    {
        // Sin columnas de auditoría; el estado está en vchestado ('A' = activo)
        builder.ToTable("SGP_CATEGORIA_VENTA");
        builder.HasKey(c => c.Codigo);

        builder.Property(c => c.Codigo).HasColumnName("intCodCategoria").ValueGeneratedNever();
        builder.Property(c => c.Descripcion).HasColumnName("vchDescripcion").HasMaxLength(50).IsUnicode(false);
        builder.MapearSituacionRegistro("varchar(1)", "vchestado");
    }
}

internal sealed class CategoriaArticuloConfiguration : IEntityTypeConfiguration<CategoriaArticulo>
{
    public void Configure(EntityTypeBuilder<CategoriaArticulo> builder)
    {
        // El canal se guarda con o sin espacio inicial (' 30' y '30'): se compara sin espacios
        builder.ToTable("SGP_CATEGORIA_ARTICULO");
        builder.HasKey(c => new { c.CodigoCategoria, c.CodigoArticulo, c.CodigoCanal });

        builder.Property(c => c.CodigoCategoria).HasColumnName("intCodCategoria").ValueGeneratedNever();
        builder.Property(c => c.CodigoArticulo).HasColumnName("vchCodArticulo").HasMaxLength(16).IsUnicode(false);
        builder.Property(c => c.TipoOrden).HasColumnName("vchCodTipoOrden").HasMaxLength(3).IsUnicode(false);
        builder.Property(c => c.CodigoCanal).HasColumnName("vchCodTarifaCanal").HasMaxLength(3).IsUnicode(false);
        builder.MapearSituacionRegistro("varchar(1)", "vchestado");
    }
}

internal sealed class DetalleIvaConfiguration : IEntityTypeConfiguration<DetalleIva>
{
    public void Configure(EntityTypeBuilder<DetalleIva> builder)
    {
        // vchCodIva se guarda con espacios a la izquierda ('      G01'): se compara sin espacios
        builder.ToTable("SGP_DETALLE_IVA");
        builder.HasKey(d => new { d.CodigoIva, d.FechaVigencia });

        builder.Property(d => d.CodigoIva).HasColumnName("vchCodIva").HasMaxLength(9).IsUnicode(false);
        builder.Property(d => d.FechaVigencia).HasColumnName("dtmFechaVigencia").HasColumnType("datetime");
        builder.Property(d => d.Porcentaje).HasColumnName("numValorIGV").HasColumnType("numeric(15,4)");
        builder.Ignore(d => d.Tasa);
        builder.MapearSituacionRegistro();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class TarifaConfiguration : IEntityTypeConfiguration<Tarifa>
{
    public void Configure(EntityTypeBuilder<Tarifa> builder)
    {
        // SGP_TARIFA usa nvarchar y guarda los códigos con espacios a la izquierda
        builder.ToTable("SGP_TARIFA");
        builder.HasKey(t => new { t.CodigoArticulo, t.CodigoCanal, t.CantidadMaxima, t.InicioVigencia });

        builder.Property(t => t.CodigoArticulo).HasColumnName("vchCodArticulo").HasMaxLength(16).IsUnicode();
        builder.Property(t => t.CodigoCanal).HasColumnName("vchCodTarifaCanal").HasMaxLength(3).IsUnicode();
        builder.Property(t => t.CantidadMaxima).HasColumnName("numCantidad").HasColumnType("numeric(15,4)");
        builder.Property(t => t.Precio).HasColumnName("numPrecioUnidad").HasColumnType("numeric(15,4)");
        builder.Property(t => t.InicioVigencia).HasColumnName("dtmFecInicio").HasColumnType("datetime");
        builder.Property(t => t.FinVigencia).HasColumnName("dtmFecCaducidad").HasColumnType("datetime");
        builder.MapearSituacionRegistro("varchar(1)");
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class TarifaCanalConfiguration : IEntityTypeConfiguration<TarifaCanal>
{
    public void Configure(EntityTypeBuilder<TarifaCanal> builder)
    {
        builder.ToTable("SGP_TARIFA_CANAL");
        builder.HasKey(c => c.Codigo);

        builder.Property(c => c.Codigo).HasColumnName("vchCodTarifaCanal").HasMaxLength(3).IsUnicode(false);
        builder.Property(c => c.Descripcion).HasColumnName("vchDescripcion").HasMaxLength(30).IsUnicode(false);
        builder.MapearSituacionRegistro();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class PuntoVentaConfiguration : IEntityTypeConfiguration<PuntoVenta>
{
    public void Configure(EntityTypeBuilder<PuntoVenta> builder)
    {
        builder.ToTable("SGP_PUNTO_VENTA");
        builder.HasKey(p => p.Codigo);

        builder.Property(p => p.Codigo).HasColumnName("vchCodPuntoVenta").HasMaxLength(4).IsUnicode(false);
        builder.Property(p => p.Descripcion).HasColumnName("vchDescripcion").HasMaxLength(45).IsUnicode(false);
        builder.Property(p => p.ZonaVenta).HasColumnName("vchZonaVenta").HasMaxLength(6).IsUnicode(false);
        builder.MapearSituacionRegistro();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class TipoOrdenConfiguration : IEntityTypeConfiguration<TipoOrden>
{
    public void Configure(EntityTypeBuilder<TipoOrden> builder)
    {
        builder.ToTable("SGP_TIPO_ORDEN");
        builder.HasKey(t => t.Codigo);

        builder.Property(t => t.Codigo).HasColumnName("vchCodTipoOrden").HasMaxLength(6).IsUnicode(false);
        builder.Property(t => t.Descripcion).HasColumnName("vchDescripcion").HasMaxLength(64).IsUnicode(false);
        builder.Property(t => t.CodigoTipoLiquidacion).HasColumnName("intCodTipoLiquidacion");
        builder.Ignore(t => t.TipoLiquidacion);
        builder.MapearSituacionRegistro();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class ZonaVentaConfiguration : IEntityTypeConfiguration<ZonaVenta>
{
    public void Configure(EntityTypeBuilder<ZonaVenta> builder)
    {
        builder.ToTable("SGP_ZONA_VENTA");
        builder.HasKey(z => z.Codigo);

        builder.Property(z => z.Codigo).HasColumnName("vchZonaVenta").HasMaxLength(6).IsUnicode(false);
        builder.Property(z => z.Descripcion).HasColumnName("vchDescripcion").HasMaxLength(30).IsUnicode(false);
    }
}

internal sealed class BancoConfiguration : IEntityTypeConfiguration<Banco>
{
    public void Configure(EntityTypeBuilder<Banco> builder)
    {
        builder.ToTable("SGP_BANCO");
        builder.HasKey(b => b.Codigo);

        builder.Property(b => b.Codigo).HasColumnName("vchCodBanco").HasMaxLength(12).IsUnicode(false);
        builder.Property(b => b.Descripcion).HasColumnName("vchDescripcion").HasMaxLength(64).IsUnicode(false);
        builder.MapearSituacionRegistro();
        builder.MapearAuditoriaSgp();
    }
}

internal sealed class CatalogoConfiguration : IEntityTypeConfiguration<Catalogo>
{
    public void Configure(EntityTypeBuilder<Catalogo> builder)
    {
        builder.ToTable("SGP_TABLA");
        builder.HasKey(c => c.Codigo);

        builder.Property(c => c.Codigo).HasColumnName("numCodigo").ValueGeneratedNever();
        builder.Property(c => c.Nombre).HasColumnName("vchNombre").HasMaxLength(64).IsUnicode(false);
        builder.MapearSituacionRegistro();
    }
}

internal sealed class ItemCatalogoConfiguration : IEntityTypeConfiguration<ItemCatalogo>
{
    public void Configure(EntityTypeBuilder<ItemCatalogo> builder)
    {
        builder.ToTable("SGP_DETALLE_TABLA");
        builder.HasKey(i => new { i.CodigoCatalogo, i.Item });

        builder.Property(i => i.CodigoCatalogo).HasColumnName("numCodigo");
        builder.Property(i => i.Item).HasColumnName("numItem");
        builder.Property(i => i.Valor).HasColumnName("vchValor").HasMaxLength(64).IsUnicode(false);
        builder.Property(i => i.Descripcion).HasColumnName("vchDescripcion").HasMaxLength(128).IsUnicode(false);
        builder.MapearSituacionRegistro();
    }
}

internal sealed class MovimientoKardexConfiguration : IEntityTypeConfiguration<MovimientoKardex>
{
    public void Configure(EntityTypeBuilder<MovimientoKardex> builder)
    {
        // Tabla PGA_: columnas sin notación húngara
        builder.ToTable("PGA_TRANSACCION_KARDEX");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("CodTransaccionKardex").ValueGeneratedOnAdd();
        builder.Property(m => m.CodigoArticulo).HasColumnName("CodArticulo").HasMaxLength(20).IsUnicode(false);
        builder.Property(m => m.CodigoCanal).HasColumnName("CodTarifaCanal").HasMaxLength(3).IsUnicode(false);
        builder.Property(m => m.CodigoPuntoVenta).HasColumnName("CodPuntoVenta").HasMaxLength(4).IsUnicode(false)
            .HasConversion((ValueConverter)Conversiones.CodigoPuntoVenta);
        builder.Property(m => m.TipoTransaccion).HasColumnName("TipoTransaccion").HasMaxLength(50).IsUnicode(false);
        builder.Property(m => m.FechaTransaccion).HasColumnName("FechaTransaccion").HasColumnType("datetime");
        builder.Property(m => m.Cantidad).HasColumnName("Cantidad").HasColumnType("numeric(18,4)");
        builder.Property(m => m.CodigoTransaccion).HasColumnName("CodTransaccion").HasColumnType("numeric(18,0)")
            .HasConversion<decimal?>();
        builder.Property(m => m.Descripcion).HasColumnName("Descripcion").HasMaxLength(100).IsUnicode(false);
        builder.Property(m => m.Estado).HasColumnName("Estado").HasMaxLength(10).IsUnicode(false);
        builder.Property(m => m.Proceso).HasColumnName("Proceso").HasMaxLength(50).IsUnicode(false);
        builder.Property(m => m.GuidRegistro).HasColumnName("GuidRegistro").HasMaxLength(100).IsUnicode(false);
        builder.MapearSituacionRegistro("varchar(1)", "SituacionRegistro");
        builder.MapearAuditoriaPga();
    }
}

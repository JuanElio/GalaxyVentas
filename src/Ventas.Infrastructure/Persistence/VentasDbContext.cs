using Microsoft.EntityFrameworkCore;

namespace Ventas.Infrastructure.Persistence;

/// <summary>
/// Contexto sobre la base de datos existente BDVenta. El esquema no se modifica (RNF-01):
/// cada entidad se mapea a su tabla real en Persistence/Configurations.
/// </summary>
public class VentasDbContext(DbContextOptions<VentasDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VentasDbContext).Assembly);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Application.Ventas;
using Ventas.Infrastructure.Ventas;
using Ventas.Infrastructure.Inventario;
using Ventas.Infrastructure.Maestros;
using Ventas.Infrastructure.Persistence;
using Ventas.Infrastructure.Seguridad;

namespace Ventas.Infrastructure;

public static class RegistroServicios
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // La cadena de conexión está en User Secrets, no en appsettings.json (RNF-04)
        var cadenaConexion = configuration.GetConnectionString("DefaultConnection");

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditoriaInterceptor>();
        services.AddSingleton<ValoresHeredadosInterceptor>();
        services.AddDbContext<VentasDbContext>((proveedor, opciones) => opciones
            .UseSqlServer(cadenaConexion)
            .AddInterceptors(
                proveedor.GetRequiredService<ValoresHeredadosInterceptor>(),
                proveedor.GetRequiredService<AuditoriaInterceptor>()));

        services.AddScoped<IUnitOfWork, UnidadTrabajo>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IVerificadorClave, VerificadorClave>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IArticuloRepository, ArticuloRepository>();
        services.AddScoped<IPuntoVentaRepository, PuntoVentaRepository>();
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ICorrelativoService, CorrelativoService>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
        services.AddScoped<IKardexRepository, KardexRepository>();

        return services;
    }
}

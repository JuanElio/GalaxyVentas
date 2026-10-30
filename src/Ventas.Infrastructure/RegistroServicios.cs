using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ventas.Application.Seguridad;
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
        services.AddDbContext<VentasDbContext>((proveedor, opciones) => opciones
            .UseSqlServer(cadenaConexion)
            .AddInterceptors(proveedor.GetRequiredService<AuditoriaInterceptor>()));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IVerificadorClave, VerificadorClave>();

        return services;
    }
}

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Application.Ventas;

namespace Ventas.Application;

public static class RegistroServicios
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(RegistroServicios).Assembly, includeInternalTypes: true);
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<AutenticacionService>();
        services.AddScoped<ClienteService>();
        services.AddScoped<ArticuloService>();
        services.AddScoped<CatalogoService>();
        services.AddScoped<VentaService>();

        return services;
    }
}

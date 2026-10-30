using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ventas.Infrastructure.Persistence;

/// <summary>
/// Columnas heredadas con un valor fijo al insertar (p. ej. chrFlagMigrado = 'N', vchOrigen = 'PGA')
/// que el dominio no conoce. Se declaran en la configuración de cada tabla con
/// <see cref="ValorAlInsertar{T}"/> y las rellena <see cref="ValoresHeredadosInterceptor"/>.
/// </summary>
internal static class ValoresHeredados
{
    public const string Anotacion = "Ventas:ValorAlInsertar";

    public static PropertyBuilder<T> ValorAlInsertar<T>(this PropertyBuilder<T> propiedad, T valor) =>
        propiedad.HasAnnotation(Anotacion, valor);
}

/// <summary>
/// Al insertar, asigna el valor declarado con <see cref="ValoresHeredados.ValorAlInsertar{T}"/> a las
/// propiedades que no tienen valor. La BD no tiene valores por defecto para estas columnas.
/// </summary>
public sealed class ValoresHeredadosInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            Asignar(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            Asignar(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private static void Asignar(DbContext contexto)
    {
        foreach (var entrada in contexto.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            foreach (var propiedad in entrada.Metadata.GetProperties())
            {
                if (propiedad.FindAnnotation(ValoresHeredados.Anotacion) is not { } anotacion)
                    continue;

                var valorActual = entrada.Property(propiedad.Name);
                if (valorActual.CurrentValue is null || Equals(valorActual.CurrentValue, propiedad.Sentinel))
                    valorActual.CurrentValue = anotacion.Value;
            }
        }
    }
}

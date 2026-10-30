using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ventas.Application.Seguridad;

namespace Ventas.Infrastructure.Persistence;

/// <summary>
/// Rellena las columnas de auditoría con el usuario de la sesión y la fecha del servidor (RF-18).
/// </summary>
public sealed class AuditoriaInterceptor(IUsuarioActual usuarioActual, TimeProvider reloj) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            Auditar(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            Auditar(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private void Auditar(DbContext contexto)
    {
        var entradas = contexto.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();
        if (entradas.Count == 0)
            return;

        if (!usuarioActual.EstaAutenticado)
            throw new InvalidOperationException("No hay un usuario en la sesión para registrar la auditoría.");

        var login = usuarioActual.Login;
        var ahora = reloj.GetLocalNow().DateTime;

        foreach (var entrada in entradas)
        {
            var (usuario, fecha) = entrada.State == EntityState.Added
                ? (Auditoria.Creador, Auditoria.FechaCreacion)
                : (Auditoria.Modificador, Auditoria.FechaModificacion);

            if (entrada.Metadata.FindProperty(usuario) is not null)
                entrada.Property(usuario).CurrentValue = login;
            if (entrada.Metadata.FindProperty(fecha) is not null)
                entrada.Property(fecha).CurrentValue = ahora;
        }
    }
}

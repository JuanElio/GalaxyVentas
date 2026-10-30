using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ventas.Application.Seguridad;
using Ventas.Domain.Seguridad;
using Ventas.Infrastructure.Persistence;
using Ventas.Infrastructure.Persistence.Configurations;

namespace Ventas.Infrastructure.Seguridad;

/// <summary>
/// SGP_USUARIO.vchPassword está en texto plano (P-02). La comparación se hace en memoria para
/// que distinga mayúsculas (la intercalación de SQL Server no lo hace) y en tiempo constante.
/// </summary>
public sealed class VerificadorClave(VentasDbContext db) : IVerificadorClave
{
    public async Task<bool> VerificarAsync(string login, string clave, CancellationToken ct)
    {
        var guardada = await db.Set<Usuario>()
            .Where(u => u.Login == login)
            .Select(u => EF.Property<string>(u, UsuarioConfiguration.Clave))
            .FirstOrDefaultAsync(ct);

        return guardada is not null
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(guardada), Encoding.UTF8.GetBytes(clave));
    }
}

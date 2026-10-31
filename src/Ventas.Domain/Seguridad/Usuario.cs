namespace Ventas.Domain.Seguridad;

/// <summary>
/// Operador del sistema. La contraseña no forma parte de la entidad: solo la lee el
/// verificador de credenciales de Infrastructure.
/// </summary>
public class Usuario
{
    // Nullable porque la columna de situación admite NULL, que se interpreta como inactivo
    private bool? _activo;

    private Usuario()
    {
        Login = null!;
    }

    public Usuario(string login, string? nombre, string? codigoPuntoVenta, bool activo,
        DateTime inicioVigencia, DateTime finVigencia, DateTime? fechaExpiraClave,
        string? numeroDocumento = null, string? email = null, string? telefono = null)
    {
        Login = login;
        Nombre = nombre;
        CodigoPuntoVenta = codigoPuntoVenta;
        _activo = activo;
        InicioVigencia = inicioVigencia;
        FinVigencia = finVigencia;
        FechaExpiraClave = fechaExpiraClave;
        NumeroDocumento = numeroDocumento;
        Email = email;
        Telefono = telefono;
    }

    public int Id { get; private set; }

    public string Login { get; private set; }

    public string? Nombre { get; private set; }

    public string? CodigoPuntoVenta { get; private set; }

    public bool Activo => _activo == true;

    public DateTime InicioVigencia { get; private set; }

    public DateTime FinVigencia { get; private set; }

    public DateTime? FechaExpiraClave { get; private set; }

    public string? NumeroDocumento { get; private set; }

    public string? Email { get; private set; }

    public string? Telefono { get; private set; }

    public bool TienePuntoVenta => !string.IsNullOrWhiteSpace(CodigoPuntoVenta);

    /// <summary>Activo y dentro de su periodo de vigencia, comparando solo fechas (RN-18).</summary>
    public bool EstaVigente(DateTime hoy) =>
        Activo && InicioVigencia.Date <= hoy.Date && hoy.Date <= FinVigencia.Date;

    /// <summary>La contraseña expiró si su fecha es anterior a hoy; sin fecha no expira (P-17).</summary>
    public bool ClaveExpirada(DateTime hoy) => FechaExpiraClave is { } expira && expira.Date < hoy.Date;

    public bool PerteneceAPuntoVenta(string codigoPuntoVenta) =>
        TienePuntoVenta
        && string.Equals(CodigoPuntoVenta!.Trim(), codigoPuntoVenta.Trim(), StringComparison.OrdinalIgnoreCase);
}

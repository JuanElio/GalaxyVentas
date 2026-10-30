using Ventas.Domain.Comun;

namespace Ventas.Domain.Maestros;

public class Cliente
{
    private bool? _activo;

    private Cliente()
    {
        TipoDocumento = NumeroDocumento = TipoPersona = null!;
    }

    public long Id { get; private set; }

    public string TipoDocumento { get; private set; }

    public string NumeroDocumento { get; private set; }

    public string TipoPersona { get; private set; }

    public string? Nombres { get; private set; }

    public string? Apellidos { get; private set; }

    public string? RazonSocial { get; private set; }

    public string? DireccionFacturacion { get; private set; }

    public string? Telefono { get; private set; }

    public string? Email { get; private set; }

    public bool Activo => _activo == true;

    public bool TieneRuc => TipoDocumento.Trim() == TiposDocumento.Ruc;

    public bool EsPersonaJuridica => TipoPersona.Trim() == TiposDocumento.PersonaJuridica;

    /// <summary>Razón social o, si no tiene, nombres y apellidos.</summary>
    public string NombreCompleto =>
        !string.IsNullOrWhiteSpace(RazonSocial)
            ? RazonSocial.Trim()
            : string.Join(' ', new[] { Nombres, Apellidos }.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()));

    /// <summary>
    /// Registra un cliente nuevo (RF-03). El formato del documento lo valida Application;
    /// aquí se aseguran los datos obligatorios según el tipo de persona.
    /// </summary>
    public static Cliente Registrar(string tipoDocumento, string numeroDocumento, string? nombres, string? apellidos,
        string? razonSocial, string? direccionFacturacion, string? telefono, string? email)
    {
        if (tipoDocumento is not (TiposDocumento.Dni or TiposDocumento.Ruc))
            throw new DominioException("Solo se pueden registrar clientes con DNI o RUC.");
        if (string.IsNullOrWhiteSpace(numeroDocumento))
            throw new DominioException("El número de documento es obligatorio.");

        var numero = numeroDocumento.Trim();
        var cliente = new Cliente
        {
            TipoDocumento = tipoDocumento,
            NumeroDocumento = numero,
            TipoPersona = TiposDocumento.DeducirTipoPersona(tipoDocumento, numero),
            Nombres = Limpiar(nombres),
            Apellidos = Limpiar(apellidos),
            RazonSocial = Limpiar(razonSocial),
            DireccionFacturacion = Limpiar(direccionFacturacion),
            Telefono = Limpiar(telefono),
            Email = Limpiar(email),
            _activo = true,
        };

        if (cliente.EsPersonaJuridica && cliente.RazonSocial is null)
            throw new DominioException("La razón social es obligatoria para una persona jurídica.");
        if (!cliente.EsPersonaJuridica && (cliente.Nombres is null || cliente.Apellidos is null))
            throw new DominioException("Los nombres y apellidos son obligatorios para una persona natural.");
        if (cliente.TieneRuc && cliente.DireccionFacturacion is null)
            throw new DominioException("La dirección de facturación es obligatoria para un cliente con RUC.");

        return cliente;
    }

    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}

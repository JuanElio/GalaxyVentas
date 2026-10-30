using Ventas.Domain.Comun;

namespace Ventas.Domain.Comprobantes;

/// <summary>
/// Impresora fiscal de una serie (SGP_IMPRESORA). <see cref="Correlativo"/> guarda el
/// <b>próximo</b> número a emitir (RN-13).
/// </summary>
public class Impresora
{
    private bool? _activo;

    private Impresora()
    {
    }

    public Impresora(int id, string? serie, int? correlativo, bool activo)
    {
        Id = id;
        Serie = serie;
        Correlativo = correlativo;
        _activo = activo;
    }

    public int Id { get; private set; }

    public string? Serie { get; private set; }

    public int? Correlativo { get; private set; }

    public bool Activo => _activo == true;

    /// <summary>Número que tomará el próximo comprobante: el correlativo guardado o 1 si está vacío.</summary>
    public int SiguienteNumero => Correlativo is > 0 ? Correlativo.Value : 1;

    /// <summary>Toma el próximo número para un comprobante y deja la impresora con el siguiente (RF-11).</summary>
    public int TomarNumero()
    {
        if (!Activo)
            throw new DominioException($"La impresora de la serie {Serie} está inactiva.");

        var numero = SiguienteNumero;
        Correlativo = numero + 1;
        return numero;
    }
}

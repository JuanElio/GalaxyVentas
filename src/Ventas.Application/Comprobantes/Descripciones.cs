using Ventas.Application.Maestros;
using Ventas.Domain.Comprobantes;
using Ventas.Domain.Maestros;
using Ventas.Domain.Pagos;

namespace Ventas.Application.Comprobantes;

/// <summary>Descripciones de los códigos de catálogo y de los bancos para mostrar en pantalla.</summary>
public sealed class Descripciones
{
    private readonly Dictionary<int, Dictionary<string, string>> _catalogos;
    private readonly Dictionary<string, string> _bancos;

    private Descripciones(Dictionary<int, Dictionary<string, string>> catalogos, Dictionary<string, string> bancos)
    {
        _catalogos = catalogos;
        _bancos = bancos;
    }

    public static async Task<Descripciones> CargarAsync(ICatalogoRepository repositorio, CancellationToken ct, bool conBancos = false)
    {
        var catalogos = new Dictionary<int, Dictionary<string, string>>();
        foreach (var codigo in new[] { Catalogos.TipoDocumento, Catalogos.FormaPago, Catalogos.MedioPago, Catalogos.TipoComprobante })
        {
            var items = await repositorio.ListarItemsActivosAsync(codigo, ct);
            catalogos[codigo] = items
                .Where(i => !string.IsNullOrWhiteSpace(i.Valor))
                .GroupBy(i => i.Valor!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Descripcion?.Trim() ?? g.Key, StringComparer.OrdinalIgnoreCase);
        }

        var bancos = conBancos
            ? (await repositorio.ListarBancosActivosAsync(ct))
                .ToDictionary(b => b.Codigo.Trim(), b => b.Descripcion?.Trim() ?? b.Codigo.Trim(), StringComparer.OrdinalIgnoreCase)
            : [];

        return new Descripciones(catalogos, bancos);
    }

    public string TipoComprobante(string codigo) => Buscar(Catalogos.TipoComprobante, codigo);

    public string FormaPago(string codigo) => Buscar(Catalogos.FormaPago, codigo);

    public string TipoDocumento(string? codigo) => codigo is null ? string.Empty : Buscar(Catalogos.TipoDocumento, codigo);

    public string MedioPago(TipoMedioPago? tipo) => tipo is { } t ? Buscar(Catalogos.MedioPago, ((int)t).ToString()) : "Sin pago";

    public string Banco(string? codigo) => codigo is null ? string.Empty : _bancos.GetValueOrDefault(codigo.Trim(), codigo.Trim());

    public static string Estado(EstadoComprobante estado) => estado == EstadoComprobante.Cancelado ? "Cancelado" : "Emitido";

    private string Buscar(int catalogo, string codigo) =>
        _catalogos.GetValueOrDefault(catalogo)?.GetValueOrDefault(codigo.Trim()) ?? codigo.Trim();
}

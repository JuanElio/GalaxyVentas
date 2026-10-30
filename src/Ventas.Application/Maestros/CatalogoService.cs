using Ventas.Domain.Maestros;
using Ventas.Domain.Pagos;

namespace Ventas.Application.Maestros;

/// <summary>Valores activos de los catálogos y de los bancos (RF-04, RN-02).</summary>
public sealed class CatalogoService(ICatalogoRepository catalogos)
{
    public async Task<IReadOnlyList<ItemCatalogoDto>> ListarAsync(int codigoCatalogo, CancellationToken ct)
    {
        var items = await catalogos.ListarItemsActivosAsync(codigoCatalogo, ct);
        return items
            .Where(i => !string.IsNullOrWhiteSpace(i.Valor))
            .Select(i => new ItemCatalogoDto(i.Valor!.Trim(), i.Descripcion?.Trim() ?? i.Valor.Trim()))
            .ToList();
    }

    /// <summary>Solo los medios de pago que admite el sistema (1 a 5, P-09).</summary>
    public async Task<IReadOnlyList<ItemCatalogoDto>> ListarMediosPagoAsync(CancellationToken ct)
    {
        var items = await ListarAsync(Catalogos.MedioPago, ct);
        return items
            .Where(i => int.TryParse(i.Valor, out var codigo) && Enum.IsDefined((TipoMedioPago)codigo))
            .ToList();
    }

    public async Task<IReadOnlyList<ItemCatalogoDto>> ListarBancosAsync(CancellationToken ct)
    {
        var bancos = await catalogos.ListarBancosActivosAsync(ct);
        return bancos
            .Select(b => new ItemCatalogoDto(b.Codigo.Trim(), b.Descripcion?.Trim() ?? b.Codigo.Trim()))
            .OrderBy(b => b.Descripcion, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}

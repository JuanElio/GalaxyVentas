namespace Ventas.Domain.Maestros;

/// <summary>Ítem de un catálogo genérico (SGP_DETALLE_TABLA).</summary>
public class ItemCatalogo
{
    private bool? _activo;

    private ItemCatalogo()
    {
    }

    public ItemCatalogo(int codigoCatalogo, int item, string? valor, string? descripcion, bool activo)
    {
        CodigoCatalogo = codigoCatalogo;
        Item = item;
        Valor = valor;
        Descripcion = descripcion;
        _activo = activo;
    }

    public int CodigoCatalogo { get; private set; }

    public int Item { get; private set; }

    public string? Valor { get; private set; }

    public string? Descripcion { get; private set; }

    public bool Activo => _activo == true;
}

/// <summary>Cabecera de un catálogo (SGP_TABLA).</summary>
public class Catalogo
{
    private bool? _activo;

    private Catalogo()
    {
        Nombre = null!;
    }

    public Catalogo(int codigo, string nombre, bool activo)
    {
        Codigo = codigo;
        Nombre = nombre;
        _activo = activo;
    }

    public int Codigo { get; private set; }

    public string Nombre { get; private set; }

    public bool Activo => _activo == true;
}

/// <summary>Códigos de SGP_TABLA que usa el sistema.</summary>
public static class Catalogos
{
    public const int TipoDocumento = 2;
    public const int TipoPersona = 5;
    public const int FormaPago = 10;
    public const int MedioPago = 14;
    public const int Moneda = 16;
    public const int TipoComprobante = 24;
}

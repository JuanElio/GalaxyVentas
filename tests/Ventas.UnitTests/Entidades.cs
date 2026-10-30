namespace Ventas.UnitTests;

/// <summary>Ayudas para simular entidades ya grabadas (el Id lo genera la BD y tiene setter privado).</summary>
internal static class Entidades
{
    public static T ConId<T>(this T entidad, long id) where T : class
    {
        typeof(T).GetProperty("Id")!.SetValue(entidad, id);
        return entidad;
    }
}

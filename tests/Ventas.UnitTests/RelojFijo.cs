namespace Ventas.UnitTests;

/// <summary>Reloj con una fecha fija para las pruebas (hora local = UTC).</summary>
internal sealed class RelojFijo(DateTime ahora) : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(ahora, DateTimeKind.Utc));
}

using FluentAssertions;
using Ventas.Domain.Seguridad;

namespace Ventas.UnitTests.Domain;

public class UsuarioTests
{
    private static readonly DateTime Hoy = new(2026, 10, 29, 15, 0, 0);

    private static Usuario Crear(bool activo = true, DateTime? inicio = null, DateTime? fin = null,
        DateTime? expiraClave = null, string? puntoVenta = "001") =>
        new("jperez", "Juan Pérez", puntoVenta, activo,
            inicio ?? new DateTime(2026, 1, 1), fin ?? new DateTime(2026, 12, 31), expiraClave);

    [Fact]
    public void EstaVigente_ActivoDentroDelPeriodo_EsVerdadero() =>
        Crear().EstaVigente(Hoy).Should().BeTrue();

    [Fact]
    public void EstaVigente_Inactivo_EsFalso() =>
        Crear(activo: false).EstaVigente(Hoy).Should().BeFalse();

    [Theory]
    [InlineData("2026-10-30", "2026-12-31", false)] // aún no empieza
    [InlineData("2026-01-01", "2026-10-28", false)] // ya terminó
    [InlineData("2026-10-29", "2026-10-29", true)]  // los extremos cuentan, sin importar la hora
    public void EstaVigente_ComparaSoloFechas(string inicio, string fin, bool esperado) =>
        Crear(inicio: DateTime.Parse(inicio), fin: DateTime.Parse(fin)).EstaVigente(Hoy).Should().Be(esperado);

    [Theory]
    [InlineData(null, false)]
    [InlineData("2026-10-29", false)]
    [InlineData("2026-10-28", true)]
    public void ClaveExpirada_SoloSiLaFechaEsAnteriorAHoy(string? expira, bool esperado) =>
        Crear(expiraClave: expira is null ? null : DateTime.Parse(expira)).ClaveExpirada(Hoy).Should().Be(esperado);

    [Theory]
    [InlineData("001", "001", true)]
    [InlineData("001 ", " 001", true)]
    [InlineData("001", "002", false)]
    [InlineData(null, "001", false)]
    public void PerteneceAPuntoVenta_ComparaSinEspacios(string? delUsuario, string consultado, bool esperado) =>
        Crear(puntoVenta: delUsuario).PerteneceAPuntoVenta(consultado).Should().Be(esperado);
}

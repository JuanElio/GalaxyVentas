using FluentAssertions;
using Ventas.Domain.Maestros;

namespace Ventas.UnitTests.Domain;

public class TarifaTests
{
    private static readonly DateTime Hoy = new(2026, 10, 29, 10, 0, 0);

    private static Tarifa Crear(decimal tope, decimal? precio, DateTime? inicio = null, DateTime? fin = null, bool activo = true) =>
        new("7041110101", " 01", tope, precio, inicio ?? new DateTime(2021, 12, 15), fin ?? new DateTime(2050, 1, 1), activo);

    // Escalas reales del artículo 7041110101 en el canal 01
    private static readonly Tarifa[] Escalas =
    [
        Crear(6, 83.0000m),
        Crear(15, 102.6000m),
        Crear(30, 138.7000m),
        Crear(60, 203.4830m),
    ];

    [Theory]
    [InlineData(1, 83.0000)]
    [InlineData(6, 83.0000)]
    [InlineData(7, 102.6000)]
    [InlineData(30, 138.7000)]
    [InlineData(31, 203.4830)]
    [InlineData(60, 203.4830)]
    public void Elegir_UsaLaMenorEscalaQueCubreLaCantidad(decimal cantidad, decimal precioEsperado) =>
        Tarifa.Elegir(Escalas, cantidad, Hoy)!.Precio.Should().Be(precioEsperado);

    [Fact]
    public void Elegir_CantidadMayorQueLaEscalaMasAlta_NoHayTarifa() =>
        Tarifa.Elegir(Escalas, 61, Hoy).Should().BeNull();

    [Fact]
    public void Elegir_EscalaUnica999999_CubreCualquierCantidadNormal() =>
        Tarifa.Elegir([Crear(999999, 36.6000m)], 1, Hoy)!.Precio.Should().Be(36.6000m);

    [Fact]
    public void Elegir_MismaEscalaConVariasVigencias_UsaLaMasReciente()
    {
        Tarifa[] tarifas =
        [
            Crear(6, 84.0000m, inicio: new DateTime(2018, 8, 8)),
            Crear(6, 84.0200m, inicio: new DateTime(2018, 8, 14)),
        ];

        Tarifa.Elegir(tarifas, 1, Hoy)!.Precio.Should().Be(84.0200m);
    }

    [Fact]
    public void Elegir_IgnoraTarifasInactivasSinPrecioONoVigentes()
    {
        Tarifa[] tarifas =
        [
            Crear(6, 10m, activo: false),
            Crear(6, null),
            Crear(6, 11m, inicio: Hoy.AddDays(1)),
            Crear(6, 12m, fin: Hoy.AddDays(-1)),
            Crear(15, 50m),
        ];

        Tarifa.Elegir(tarifas, 1, Hoy)!.Precio.Should().Be(50m);
    }

    [Fact]
    public void EstaVigente_ElDiaDeCaducidadCuentaCompleto()
    {
        var tarifa = Crear(6, 10m, fin: new DateTime(2026, 10, 29));

        tarifa.EstaVigente(new DateTime(2026, 10, 29, 23, 59, 0)).Should().BeTrue();
        tarifa.EstaVigente(new DateTime(2026, 10, 30)).Should().BeFalse();
    }
}

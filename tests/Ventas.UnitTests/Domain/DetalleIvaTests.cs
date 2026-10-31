using FluentAssertions;
using Ventas.Domain.Maestros;

namespace Ventas.UnitTests.Domain;

public class DetalleIvaTests
{
    private static readonly DateTime Hoy = new(2026, 10, 30);

    [Fact]
    public void TasaVigente_UsaLaVigenciaMasRecienteQueNoPaseDeLaFechaYLaConvierteEnFraccion()
    {
        DetalleIva[] detalles =
        [
            new("IGV1", new DateTime(1999, 1, 1), 18.0000m, activo: true),
            new("IGV1", new DateTime(2005, 2, 1), 19.0000m, activo: true),
            new("IGV1", new DateTime(2027, 1, 1), 20.0000m, activo: true),
        ];

        DetalleIva.TasaVigente(detalles, Hoy).Should().Be(0.19m);
    }

    [Fact]
    public void TasaVigente_Exonerado_Cero() =>
        DetalleIva.TasaVigente([new DetalleIva("E20", new DateTime(2015, 8, 15), 0m, activo: true)], Hoy).Should().Be(0m);

    [Fact]
    public void TasaVigente_IgnoraInactivosYSinPorcentaje()
    {
        DetalleIva[] detalles =
        [
            new("G01", new DateTime(2015, 1, 1), 18.0000m, activo: true),
            new("G01", new DateTime(2016, 1, 1), 10.0000m, activo: false),
            new("G01", new DateTime(2017, 1, 1), null, activo: true),
        ];

        DetalleIva.TasaVigente(detalles, Hoy).Should().Be(0.18m);
    }

    [Fact]
    public void TasaVigente_SinRegistros_Null() =>
        DetalleIva.TasaVigente([], Hoy).Should().BeNull();
}

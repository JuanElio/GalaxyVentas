using FluentAssertions;
using FluentValidation;
using Moq;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Domain.Maestros;

namespace Ventas.UnitTests.Application;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _clientes = new();
    private readonly Mock<IUnitOfWork> _unidadTrabajo = new();
    private readonly ClienteService _servicio;

    private static readonly RegistrarClienteDto NuevoDni = new("2", "12345678", "Juan", "Pérez", null, "AV. LIMA 1", null, null);

    public ClienteServiceTests()
    {
        _servicio = new ClienteService(_clientes.Object, _unidadTrabajo.Object, new RegistrarClienteValidator());
    }

    [Fact]
    public async Task Buscar_ClienteExistente_DevuelveSusDatos()
    {
        var cliente = Cliente.Registrar("1", "20123456789", null, null, "EMPRESA SAC ", " AV. LIMA 123", null, null);
        _clientes.Setup(c => c.BuscarActivosPorDocumentoAsync("20123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync([cliente]);

        var encontrados = await _servicio.BuscarAsync(" 20123456789 ", CancellationToken.None);

        encontrados.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            TipoDocumento = "1",
            NumeroDocumento = "20123456789",
            Nombre = "EMPRESA SAC",
            DireccionFacturacion = "AV. LIMA 123",
            TieneRuc = true,
        });
    }

    [Fact]
    public async Task Buscar_SinCoincidencias_NoEncontrado()
    {
        _clientes.Setup(c => c.BuscarActivosPorDocumentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var accion = () => _servicio.BuscarAsync("99999999", CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("*99999999*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task Buscar_SinDocumento_ErrorDeValidacion(string? documento)
    {
        var accion = () => _servicio.BuscarAsync(documento, CancellationToken.None);

        await accion.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Buscar_TextoConLetras_BuscaPorNombreCadaPalabraUnaVez()
    {
        var cliente = Cliente.Registrar("2", "41234567", "ROSA", "QUISPE YANQUI", null, null, null, null);
        _clientes.Setup(c => c.BuscarActivosPorNombreAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([cliente]);

        var encontrados = await _servicio.BuscarAsync("  quispe   rosa QUISPE ", CancellationToken.None);

        encontrados.Should().ContainSingle().Which.Nombre.Should().Be("ROSA QUISPE YANQUI");
        _clientes.Verify(c => c.BuscarActivosPorNombreAsync(
            It.Is<IReadOnlyList<string>>(p => p.SequenceEqual(new[] { "quispe", "rosa" })), It.IsAny<CancellationToken>()));
        _clientes.Verify(c => c.BuscarActivosPorDocumentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Buscar_MasDeCincoPalabras_UsaSoloLasPrimerasCinco()
    {
        _clientes.Setup(c => c.BuscarActivosPorNombreAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Cliente.Registrar("2", "41234567", "A", "B", null, null, null, null)]);

        await _servicio.BuscarAsync("uno dos tres cuatro cinco seis", CancellationToken.None);

        _clientes.Verify(c => c.BuscarActivosPorNombreAsync(
            It.Is<IReadOnlyList<string>>(p => p.Count == ClienteService.MaximoPalabras), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Buscar_NombreSinCoincidencias_NoEncontrado()
    {
        _clientes.Setup(c => c.BuscarActivosPorNombreAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var accion = () => _servicio.BuscarAsync("zzzqqq", CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("*nombre o razón social*zzzqqq*");
    }

    [Fact]
    public async Task Buscar_NombreDeMenosDeTresCaracteres_ErrorDeValidacionSinConsultar()
    {
        var accion = () => _servicio.BuscarAsync("ab", CancellationToken.None);

        await accion.Should().ThrowAsync<ValidationException>().WithMessage("*al menos 3 caracteres*");
        _clientes.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Registrar_ClienteNuevo_LoGuardaYDevuelveSusDatos()
    {
        var resultado = await _servicio.RegistrarAsync(NuevoDni, CancellationToken.None);

        resultado.Nombre.Should().Be("Juan Pérez");
        resultado.TieneRuc.Should().BeFalse();
        _clientes.Verify(c => c.AgregarAsync(It.Is<Cliente>(n => n.NumeroDocumento == "12345678"), It.IsAny<CancellationToken>()));
        _unidadTrabajo.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Registrar_DocumentoYaRegistrado_ConflictoSinGrabar()
    {
        _clientes.Setup(c => c.ExisteActivoAsync("2", "12345678", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var accion = () => _servicio.RegistrarAsync(NuevoDni, CancellationToken.None);

        await accion.Should().ThrowAsync<ConflictoException>().WithMessage("*ya existe*12345678*");
        _unidadTrabajo.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Registrar_DatosInvalidos_ErrorDeValidacionSinConsultarLaBase()
    {
        var accion = () => _servicio.RegistrarAsync(NuevoDni with { NumeroDocumento = "123" }, CancellationToken.None);

        await accion.Should().ThrowAsync<ValidationException>();
        _clientes.VerifyNoOtherCalls();
    }
}

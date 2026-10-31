using FluentAssertions;
using FluentValidation;
using Moq;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Domain.Maestros;
using Ventas.Domain.Seguridad;

namespace Ventas.UnitTests.Application;

public class MisDatosServiceTests
{
    private const string ClaveActual = "Ventas2026";
    private const string ClaveNueva = "Nueva2027";

    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IPuntoVentaRepository> _puntosVenta = new();
    private readonly Mock<IVerificadorClave> _verificador = new();
    private readonly Mock<ICambioClave> _cambioClave = new();
    private readonly Mock<IUnitOfWork> _unidadTrabajo = new();
    private readonly Mock<IUsuarioActual> _usuarioActual = new();
    private readonly MisDatosService _servicio;

    public MisDatosServiceTests()
    {
        _usuarioActual.SetupGet(u => u.Login).Returns("jperez");
        _usuarioActual.SetupGet(u => u.CodigoPuntoVenta).Returns("001");
        _usuarios.Setup(u => u.ObtenerPorLoginAsync("jperez", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Usuario("jperez ", " Juan Pérez ", " 001", true, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
                new DateTime(2027, 3, 31), numeroDocumento: "12345678", email: " jperez@dop.pe ", telefono: "  "));
        _puntosVenta.Setup(p => p.ObtenerAsync("001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PuntoVenta(" 001", "HEMEROTECA ", "L10", activo: true));
        _verificador.Setup(v => v.VerificarAsync("jperez", ClaveActual, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _servicio = new MisDatosService(_usuarios.Object, _puntosVenta.Object, _verificador.Object, _cambioClave.Object,
            _unidadTrabajo.Object, _usuarioActual.Object, new CambiarClaveValidator());
    }

    [Fact]
    public async Task Obtener_DatosDelUsuarioDeLaSesionSinEspacios()
    {
        var datos = await _servicio.ObtenerAsync(CancellationToken.None);

        datos.Should().Be(new MisDatosDto("jperez", "Juan Pérez", "12345678", "jperez@dop.pe", null, "001", "HEMEROTECA",
            new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), new DateTime(2027, 3, 31)));
    }

    [Fact]
    public async Task Obtener_UsuarioInexistente_NoEncontrado()
    {
        _usuarios.Setup(u => u.ObtenerPorLoginAsync("jperez", It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);

        var accion = () => _servicio.ObtenerAsync(CancellationToken.None);

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Fact]
    public async Task CambiarClave_ClaveActualCorrecta_CambiaLaDelUsuarioDeLaSesionYGraba()
    {
        await _servicio.CambiarClaveAsync(new CambiarClaveDto(ClaveActual, ClaveNueva, ClaveNueva), CancellationToken.None);

        _cambioClave.Verify(c => c.CambiarAsync("jperez", ClaveNueva, It.IsAny<CancellationToken>()));
        _unidadTrabajo.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task CambiarClave_ClaveActualIncorrecta_ErrorEnElCampoSinCambiar()
    {
        var accion = () => _servicio.CambiarClaveAsync(new CambiarClaveDto("otra-clave1", ClaveNueva, ClaveNueva), CancellationToken.None);

        (await accion.Should().ThrowAsync<ValidationException>()).Which.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CambiarClaveDto.ClaveActual) && e.ErrorMessage == MisDatosService.MensajeClaveActualIncorrecta);
        _cambioClave.VerifyNoOtherCalls();
        _unidadTrabajo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CambiarClave_DatosInvalidos_NoVerificaNiCambia()
    {
        var accion = () => _servicio.CambiarClaveAsync(new CambiarClaveDto(ClaveActual, "corta", "corta"), CancellationToken.None);

        await accion.Should().ThrowAsync<ValidationException>();
        _verificador.VerifyNoOtherCalls();
        _cambioClave.VerifyNoOtherCalls();
    }
}

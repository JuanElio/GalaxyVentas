using FluentAssertions;
using FluentValidation;
using Moq;
using Ventas.Application.Comun;
using Ventas.Application.Seguridad;
using Ventas.Domain.Seguridad;

namespace Ventas.UnitTests.Application;

public class AutenticacionServiceTests
{
    private const string Clave = "Ventas2026";
    private static readonly DateTime Hoy = new(2026, 10, 29, 9, 0, 0);

    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IVerificadorClave> _verificador = new();
    private readonly AutenticacionService _servicio;

    public AutenticacionServiceTests()
    {
        _servicio = new AutenticacionService(_usuarios.Object, _verificador.Object,
            new IniciarSesionValidator(), new RelojFijo(Hoy));
    }

    private void DadoUsuario(Usuario? usuario, bool claveCorrecta = true)
    {
        _verificador.Setup(v => v.VerificarAsync("jperez", Clave, It.IsAny<CancellationToken>())).ReturnsAsync(claveCorrecta);
        _usuarios.Setup(u => u.ObtenerPorLoginAsync("jperez", It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
    }

    private static Usuario CrearUsuario(bool activo = true, DateTime? fin = null, DateTime? expiraClave = null,
        string? puntoVenta = "001", string? nombre = "Juan Pérez") =>
        new("jperez", nombre, puntoVenta, activo, new DateTime(2026, 1, 1), fin ?? new DateTime(2026, 12, 31), expiraClave);

    [Fact]
    public async Task IniciarSesion_CredencialesCorrectas_DevuelveLaSesionSinContrasena()
    {
        DadoUsuario(CrearUsuario(puntoVenta: "001 "));

        var sesion = await _servicio.IniciarSesionAsync(new IniciarSesionDto(" jperez ", Clave), CancellationToken.None);

        sesion.Should().Be(new SesionDto("jperez", "Juan Pérez", "001"));
    }

    [Fact]
    public async Task IniciarSesion_UsuarioSinNombre_UsaElLogin()
    {
        DadoUsuario(CrearUsuario(nombre: null));

        var sesion = await _servicio.IniciarSesionAsync(new IniciarSesionDto("jperez", Clave), CancellationToken.None);

        sesion.Nombre.Should().Be("jperez");
    }

    [Fact]
    public async Task IniciarSesion_ContrasenaIncorrecta_MensajeGenericoSinConsultarElUsuario()
    {
        DadoUsuario(CrearUsuario(), claveCorrecta: false);

        var accion = () => _servicio.IniciarSesionAsync(new IniciarSesionDto("jperez", Clave), CancellationToken.None);

        await accion.Should().ThrowAsync<NoAutenticadoException>()
            .WithMessage(AutenticacionService.MensajeCredencialesInvalidas);
        _usuarios.Verify(u => u.ObtenerPorLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static TheoryData<Usuario?> UsuariosNoValidos => new()
    {
        null,
        CrearUsuario(activo: false),
        CrearUsuario(fin: new DateTime(2026, 6, 30)),
    };

    [Theory]
    [MemberData(nameof(UsuariosNoValidos))]
    public async Task IniciarSesion_UsuarioInexistenteInactivoONoVigente_MismoMensajeGenerico(Usuario? usuario)
    {
        DadoUsuario(usuario);

        var accion = () => _servicio.IniciarSesionAsync(new IniciarSesionDto("jperez", Clave), CancellationToken.None);

        await accion.Should().ThrowAsync<NoAutenticadoException>()
            .WithMessage(AutenticacionService.MensajeCredencialesInvalidas);
    }

    [Fact]
    public async Task IniciarSesion_ContrasenaExpirada_IndicaQueExpiro()
    {
        DadoUsuario(CrearUsuario(expiraClave: new DateTime(2026, 10, 28)));

        var accion = () => _servicio.IniciarSesionAsync(new IniciarSesionDto("jperez", Clave), CancellationToken.None);

        await accion.Should().ThrowAsync<NoAutenticadoException>().WithMessage("*contraseña expiró*");
    }

    [Fact]
    public async Task IniciarSesion_SinPuntoDeVenta_AccesoDenegado()
    {
        DadoUsuario(CrearUsuario(puntoVenta: " "));

        var accion = () => _servicio.IniciarSesionAsync(new IniciarSesionDto("jperez", Clave), CancellationToken.None);

        await accion.Should().ThrowAsync<AccesoDenegadoException>().WithMessage("*punto de venta*");
    }

    [Theory]
    [InlineData("", Clave, "Login")]
    [InlineData("jperez", "", "Clave")]
    [InlineData("usuario_demasiado_largo", Clave, "Login")]
    public async Task IniciarSesion_DatosInvalidos_ErrorDeValidacionSinConsultarLaBase(string login, string clave, string campo)
    {
        var accion = () => _servicio.IniciarSesionAsync(new IniciarSesionDto(login, clave), CancellationToken.None);

        (await accion.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(e => e.PropertyName == campo);
        _verificador.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidarSesion_UsuarioVigenteConElMismoPuntoDeVenta_EsValida()
    {
        DadoUsuario(CrearUsuario());

        (await _servicio.ValidarSesionAsync("jperez", "001", CancellationToken.None)).Should().BeTrue();
    }

    public static TheoryData<Usuario?, string> SesionesNoValidas => new()
    {
        { null, "001" },
        { CrearUsuario(activo: false), "001" },
        { CrearUsuario(fin: new DateTime(2026, 10, 28)), "001" },
        { CrearUsuario(), "002" }, // cambió el punto de venta asignado
    };

    [Theory]
    [MemberData(nameof(SesionesNoValidas))]
    public async Task ValidarSesion_UsuarioNoVigenteOConOtroPuntoDeVenta_NoEsValida(Usuario? usuario, string puntoVenta)
    {
        DadoUsuario(usuario);

        (await _servicio.ValidarSesionAsync("jperez", puntoVenta, CancellationToken.None)).Should().BeFalse();
    }
}

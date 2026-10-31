using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ventas.Application.Comprobantes;
using Ventas.Application.Comun;
using Ventas.Application.Maestros;
using Ventas.Application.Seguridad;
using Ventas.Application.Ventas;
using Ventas.Domain.Maestros;
using Ventas.Domain.Seguridad;

namespace Ventas.IntegrationTests;

/// <summary>
/// Levanta la aplicación sin base de datos: usuarios y clientes en memoria, y maestros simulados con Moq.
/// </summary>
public sealed partial class AplicacionPrueba : WebApplicationFactory<Program>
{
    public const string Clave = "Ventas2026";

    public UsuariosEnMemoria Usuarios { get; } = new();

    public ClientesEnMemoria Clientes { get; } = new();

    public Mock<IPuntoVentaRepository> PuntosVenta { get; } = new();

    public Mock<IArticuloRepository> Articulos { get; } = new();

    public Mock<IStockService> Stock { get; } = new();

    public Mock<ICorrelativoService> Correlativos { get; } = new();

    public Mock<IConsultaVentasRepository> Consultas { get; } = new();

    public Mock<ICatalogoRepository> Catalogos { get; } = new();

    public AplicacionPrueba()
    {
        // Punto de venta 001 con un artículo (ART001, 20.00 con IGV, 50 en stock) y la serie B001
        PuntosVenta.Setup(p => p.ObtenerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PuntoVenta(" 001", "SEDE CENTRAL", "Z01", activo: true));
        PuntosVenta.Setup(p => p.ObtenerAlmacenPrincipalAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("ALM01");
        Articulos.Setup(a => a.ObtenerCanalAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TarifaCanal(" 30", "Precio Especial", activo: true));
        Articulos.Setup(a => a.ObtenerConTarifasVigentesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([ArticuloDePrueba]);
        Articulos.Setup(a => a.ListarConTarifasVigentesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([ArticuloDePrueba]);
        Articulos.Setup(a => a.ListarCategoriasAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([ArticuloDePrueba.Categoria]);
        Stock.Setup(s => s.ObtenerStockAsync(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["ART001"] = 50 });
        Correlativos.Setup(c => c.ConsultarSiguienteAsync("B001", It.IsAny<CancellationToken>())).ReturnsAsync(7);
        Consultas.Setup(c => c.ListarAsync(It.IsAny<FiltroVentasRegistradas>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Catalogos.Setup(c => c.ListarItemsActivosAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Catalogos.Setup(c => c.ListarBancosActivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }

    private static readonly ArticuloConTarifas ArticuloDePrueba = new(
        new Articulo("ART001", "CUADERNO A4", "UND", activo: true),
        [new Tarifa("ART001", " 30", 999999, 20.0000m, new DateTime(2020, 1, 1), new DateTime(2050, 1, 1), activo: true)],
        new CategoriaVenta(1, "Bienes", activo: true), 0.18m);

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IUsuarioRepository>(Usuarios);
            services.AddSingleton<IVerificadorClave>(Usuarios);
            services.AddSingleton<ICambioClave>(Usuarios);
            services.AddSingleton<IClienteRepository>(Clientes);
            services.AddSingleton<IUnitOfWork>(Clientes);
            services.AddSingleton(PuntosVenta.Object);
            services.AddSingleton(Articulos.Object);
            services.AddSingleton(Stock.Object);
            services.AddSingleton(Correlativos.Object);
            services.AddSingleton(Consultas.Object);
            services.AddSingleton(Catalogos.Object);
        });

    /// <summary>Cliente HTTP con sesión iniciada, cabecera AJAX y token antifalsificación.</summary>
    public async Task<HttpClient> CrearClienteAjaxConSesionAsync(string login)
    {
        Usuarios.Agregar(login);
        var cliente = CrearCliente();
        await IniciarSesionAsync(cliente, login);
        var token = await ObtenerTokenAsync(cliente, "/");
        cliente.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        cliente.DefaultRequestHeaders.Add("RequestVerificationToken", token);
        return cliente;
    }

    public HttpClient CrearCliente() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost"),
    });

    /// <summary>Extrae el token antifalsificación del formulario de una página.</summary>
    public static async Task<string> ObtenerTokenAsync(HttpClient cliente, string ruta)
    {
        var html = await cliente.GetStringAsync(ruta);
        return TokenRegex().Match(html) is { Success: true } coincidencia
            ? coincidencia.Groups[1].Value
            : throw new InvalidOperationException($"La página {ruta} no tiene token antifalsificación.");
    }

    /// <summary>Inicia sesión con el formulario y devuelve la respuesta del POST.</summary>
    public static async Task<HttpResponseMessage> IniciarSesionAsync(HttpClient cliente, string login, string clave = Clave)
    {
        var token = await ObtenerTokenAsync(cliente, "/Cuenta/IniciarSesion");
        return await cliente.PostAsync("/Cuenta/IniciarSesion", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Login"] = login,
            ["Clave"] = clave,
            ["__RequestVerificationToken"] = token,
        }));
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}

public sealed class ClientesEnMemoria : IClienteRepository, IUnitOfWork
{
    private readonly ConcurrentBag<Cliente> _clientes = [];

    public void Agregar(Cliente cliente) => _clientes.Add(cliente);

    public Task<IReadOnlyList<Cliente>> BuscarActivosPorDocumentoAsync(string numeroDocumento, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Cliente>>(_clientes.Where(c => c.NumeroDocumento == numeroDocumento).ToList());

    public Task<IReadOnlyList<Cliente>> BuscarActivosPorNombreAsync(IReadOnlyList<string> palabras, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Cliente>>(_clientes
            .Where(c => palabras.All(p => $"{c.Nombres} {c.Apellidos} {c.RazonSocial}".Contains(p, StringComparison.OrdinalIgnoreCase)))
            .ToList());

    public Task<bool> ExisteActivoAsync(string tipoDocumento, string numeroDocumento, CancellationToken ct) =>
        Task.FromResult(_clientes.Any(c => c.TipoDocumento == tipoDocumento && c.NumeroDocumento == numeroDocumento));

    public Task<Cliente?> ObtenerActivoAsync(long codigo, CancellationToken ct) =>
        Task.FromResult(_clientes.FirstOrDefault(c => c.Id == codigo));

    public Task AgregarAsync(Cliente cliente, CancellationToken ct)
    {
        _clientes.Add(cliente);
        return Task.CompletedTask;
    }

    public Task GuardarCambiosAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken ct) => operacion();
}

public sealed class UsuariosEnMemoria : IUsuarioRepository, IVerificadorClave, ICambioClave
{
    private readonly ConcurrentDictionary<string, Usuario> _usuarios = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _claves = new(StringComparer.OrdinalIgnoreCase);

    public Usuario Agregar(string login, string? puntoVenta = "001", bool activo = true, DateTime? expiraClave = null)
    {
        var usuario = new Usuario(login, $"Operador {login}", puntoVenta, activo,
            DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1), expiraClave,
            numeroDocumento: "12345678", email: $"{login}@dop.pe");
        _usuarios[login] = usuario;
        _claves.TryAdd(login, AplicacionPrueba.Clave);
        return usuario;
    }

    public Task<Usuario?> ObtenerPorLoginAsync(string login, CancellationToken ct) =>
        Task.FromResult(_usuarios.GetValueOrDefault(login));

    public Task<IReadOnlyList<Usuario>> ListarActivosPorPuntoVentaAsync(string codigoPuntoVenta, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Usuario>>(_usuarios.Values
            .Where(u => u.Activo && u.PerteneceAPuntoVenta(codigoPuntoVenta)).ToList());

    public Task<bool> VerificarAsync(string login, string clave, CancellationToken ct) =>
        Task.FromResult(_usuarios.ContainsKey(login) && _claves.GetValueOrDefault(login) == clave);

    public Task CambiarAsync(string login, string claveNueva, CancellationToken ct)
    {
        _claves[login] = claveNueva;
        return Task.CompletedTask;
    }
}

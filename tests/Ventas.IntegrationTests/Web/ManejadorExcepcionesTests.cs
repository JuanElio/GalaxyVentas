using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Ventas.Api.Comun;
using Ventas.Application.Comun;
using Ventas.Domain.Comun;

namespace Ventas.IntegrationTests.Web;

public class ManejadorExcepcionesTests
{
    public static TheoryData<Exception, int> Casos => new()
    {
        { new DominioException("Stock insuficiente."), 422 },
        { new ConflictoException("Cliente duplicado."), 409 },
        { new NoAutenticadoException("Usuario o contraseña incorrectos."), 401 },
        { new NoEncontradoException("No existe."), 404 },
        { new AccesoDenegadoException("Otro punto de venta."), 403 },
        { new InvalidOperationException("Detalle interno"), 500 },
    };

    [Theory]
    [MemberData(nameof(Casos))]
    public void CrearProblema_AsignaElCodigoHttpSegunLaExcepcion(Exception excepcion, int status)
    {
        var problema = ManejadorExcepciones.CrearProblema(excepcion);

        Assert.Equal(status, problema.Status);
    }

    [Fact]
    public void CrearProblema_ErrorInesperado_NoExponeElDetalleInterno()
    {
        var problema = ManejadorExcepciones.CrearProblema(new InvalidOperationException("cadena de conexión secreta"));

        Assert.DoesNotContain("secreta", problema.Detail);
    }

    [Fact]
    public void CrearProblema_ErrorDeValidacion_AgrupaLosMensajesPorCampo()
    {
        var excepcion = new ValidationException(
        [
            new ValidationFailure("Cantidad", "La cantidad debe ser mayor que 0."),
            new ValidationFailure("Cantidad", "La cantidad debe ser entera."),
            new ValidationFailure("CodigoCliente", "El cliente es obligatorio."),
        ]);

        var problema = Assert.IsType<ValidationProblemDetails>(ManejadorExcepciones.CrearProblema(excepcion));

        Assert.Equal(400, problema.Status);
        Assert.Equal(2, problema.Errors["Cantidad"].Length);
        Assert.Single(problema.Errors["CodigoCliente"]);
    }
}

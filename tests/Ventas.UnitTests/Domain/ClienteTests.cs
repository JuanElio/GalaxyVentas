using FluentAssertions;
using Ventas.Domain.Comun;
using Ventas.Domain.Maestros;

namespace Ventas.UnitTests.Domain;

public class ClienteTests
{
    [Fact]
    public void Registrar_Dni_PersonaNaturalActivaConNombreCompleto()
    {
        var cliente = Cliente.Registrar(TiposDocumento.Dni, " 12345678 ", " Juan ", "Pérez Díaz", null, null, " ", null);

        cliente.NumeroDocumento.Should().Be("12345678");
        cliente.TipoPersona.Should().Be(TiposDocumento.PersonaNatural);
        cliente.NombreCompleto.Should().Be("Juan Pérez Díaz");
        cliente.TieneRuc.Should().BeFalse();
        cliente.Activo.Should().BeTrue();
        cliente.Telefono.Should().BeNull();
    }

    [Fact]
    public void Registrar_Ruc20_PersonaJuridicaConRazonSocial()
    {
        var cliente = Cliente.Registrar(TiposDocumento.Ruc, "20123456789", null, null, "EMPRESA SAC", "AV. LIMA 123", null, null);

        cliente.TipoPersona.Should().Be(TiposDocumento.PersonaJuridica);
        cliente.NombreCompleto.Should().Be("EMPRESA SAC");
        cliente.TieneRuc.Should().BeTrue();
    }

    [Fact]
    public void Registrar_Ruc10_PersonaNaturalConNegocio()
    {
        var cliente = Cliente.Registrar(TiposDocumento.Ruc, "10456789012", "Ana", "Torres", null, "JR. CUSCO 45", null, null);

        cliente.TipoPersona.Should().Be(TiposDocumento.PersonaNatural);
        cliente.TieneRuc.Should().BeTrue();
    }

    [Fact]
    public void NombreComprobante_NaturalApellidosYNombres_JuridicaRazonSocial()
    {
        Cliente.Registrar(TiposDocumento.Dni, "12345678", "Juan", "Pérez Díaz", null, null, null, null)
            .NombreComprobante.Should().Be("Pérez Díaz Juan");
        Cliente.Registrar(TiposDocumento.Ruc, "20123456789", null, null, " EMPRESA SAC ", "AV. LIMA", null, null)
            .NombreComprobante.Should().Be("EMPRESA SAC");
    }

    [Fact]
    public void Registrar_PersonaJuridicaSinRazonSocial_LanzaExcepcionDeDominio()
    {
        var accion = () => Cliente.Registrar(TiposDocumento.Ruc, "20123456789", "Ana", "Torres", null, "AV. LIMA", null, null);

        accion.Should().Throw<DominioException>().WithMessage("*razón social*");
    }

    [Fact]
    public void Registrar_RucSinDireccion_LanzaExcepcionDeDominio()
    {
        var accion = () => Cliente.Registrar(TiposDocumento.Ruc, "20123456789", null, null, "EMPRESA SAC", null, null, null);

        accion.Should().Throw<DominioException>().WithMessage("*dirección*");
    }

    [Fact]
    public void Registrar_OtroTipoDeDocumento_LanzaExcepcionDeDominio()
    {
        var accion = () => Cliente.Registrar(TiposDocumento.CarnetExtranjeria, "001234567", "Ana", "Torres", null, null, null, null);

        accion.Should().Throw<DominioException>().WithMessage("*DNI o RUC*");
    }
}

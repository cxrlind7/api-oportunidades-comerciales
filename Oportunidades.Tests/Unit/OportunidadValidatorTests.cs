using System.Globalization;
using FluentAssertions;
using Oportunidades.Application.DTOs;
using Oportunidades.Application.Validation;

namespace Oportunidades.Tests.Unit;

public class OportunidadValidatorTests
{
    [Fact]
    public void Crear_QuitaEspaciosDeNombreYCliente()
    {
        var resultado = OportunidadValidator.ValidarCreacion(new CrearOportunidadRequest("  ERP  ", " ACME ", 1500.50m));

        resultado.Should().Be(("ERP", "ACME", 1500.50m));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("10.123")]
    public void Crear_MontoInvalido_Falla(string monto)
    {
        var request = new CrearOportunidadRequest("ERP", "ACME", decimal.Parse(monto, CultureInfo.InvariantCulture));

        var accion = () => OportunidadValidator.ValidarCreacion(request);

        accion.Should().Throw<RequestValidationException>()
            .Which.Errors.Should().ContainKey("montoEstimado");
    }
}

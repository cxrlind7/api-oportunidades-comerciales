using FluentAssertions;
using Oportunidades.Domain.Entities;
using Oportunidades.Domain.Enums;
using Oportunidades.Domain.Exceptions;

namespace Oportunidades.Tests.Unit;

public class OportunidadTransicionesTests
{
    private static Oportunidad CrearEn(EstadoOportunidad estado) =>
        new(1, "Proyecto", "Cliente", 100m, estado, DateTime.UtcNow);

    [Theory]
    [InlineData(EstadoOportunidad.Nueva, EstadoOportunidad.EnSeguimiento)]
    [InlineData(EstadoOportunidad.EnSeguimiento, EstadoOportunidad.Ganada)]
    [InlineData(EstadoOportunidad.EnSeguimiento, EstadoOportunidad.Perdida)]
    public void TransicionPermitida_CambiaElEstado(EstadoOportunidad actual, EstadoOportunidad nuevo)
    {
        var oportunidad = CrearEn(actual);

        var cambio = oportunidad.CambiarEstado(nuevo);

        cambio.Should().BeTrue();
        oportunidad.Estado.Should().Be(nuevo);
    }

    [Theory]
    [InlineData(EstadoOportunidad.Nueva, EstadoOportunidad.Ganada)]
    [InlineData(EstadoOportunidad.Nueva, EstadoOportunidad.Perdida)]
    [InlineData(EstadoOportunidad.EnSeguimiento, EstadoOportunidad.Nueva)]
    [InlineData(EstadoOportunidad.Ganada, EstadoOportunidad.EnSeguimiento)]
    [InlineData(EstadoOportunidad.Ganada, EstadoOportunidad.Perdida)]
    [InlineData(EstadoOportunidad.Perdida, EstadoOportunidad.Nueva)]
    public void TransicionNoPermitida_LanzaExcepcion(EstadoOportunidad actual, EstadoOportunidad nuevo)
    {
        var oportunidad = CrearEn(actual);

        var accion = () => oportunidad.CambiarEstado(nuevo);

        accion.Should().Throw<TransicionEstadoInvalidaException>();
        oportunidad.Estado.Should().Be(actual);
    }

    [Fact]
    public void MismoEstado_NoCambiaNada()
    {
        var oportunidad = CrearEn(EstadoOportunidad.Ganada);

        var cambio = oportunidad.CambiarEstado(EstadoOportunidad.Ganada);

        cambio.Should().BeFalse();
        oportunidad.Estado.Should().Be(EstadoOportunidad.Ganada);
    }
}

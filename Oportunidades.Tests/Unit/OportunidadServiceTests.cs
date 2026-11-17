using FluentAssertions;
using Moq;
using Oportunidades.Application.DTOs;
using Oportunidades.Application.Interfaces;
using Oportunidades.Application.Services;
using Oportunidades.Domain.Entities;
using Oportunidades.Domain.Enums;
using Oportunidades.Domain.Exceptions;

namespace Oportunidades.Tests.Unit;

public class OportunidadServiceTests
{
    [Fact]
    public async Task CambiarEstado_SiOtraPeticionLoCambioAntes_LanzaConflicto()
    {
        var repository = new Mock<IOportunidadRepository>();
        repository.Setup(r => r.ObtenerPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Oportunidad(1, "ERP", "ACME", 100m, EstadoOportunidad.EnSeguimiento, DateTime.UtcNow));

        // el SP no actualiza nada porque el estado ya cambió en la BD
        repository.Setup(r => r.ActualizarEstadoAsync(1, EstadoOportunidad.EnSeguimiento, EstadoOportunidad.Ganada, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new OportunidadService(repository.Object);

        var accion = () => service.ActualizarEstadoAsync(1, new ActualizarEstadoRequest("Ganada"), default);

        await accion.Should().ThrowAsync<ConcurrencyConflictException>();
    }
}

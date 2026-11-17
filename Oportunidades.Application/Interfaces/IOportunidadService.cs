using Oportunidades.Application.DTOs;

namespace Oportunidades.Application.Interfaces;

public interface IOportunidadService
{
    Task<OportunidadResponse> CrearAsync(CrearOportunidadRequest request, CancellationToken cancellationToken);

    Task<OportunidadResponse> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<OportunidadResponse>> ListarAsync(string? estado, CancellationToken cancellationToken);

    Task<OportunidadResponse> ActualizarEstadoAsync(int id, ActualizarEstadoRequest request, CancellationToken cancellationToken);
}

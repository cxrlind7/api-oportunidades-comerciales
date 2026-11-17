using Oportunidades.Domain.Entities;
using Oportunidades.Domain.Enums;

namespace Oportunidades.Application.Interfaces;

public interface IOportunidadRepository
{
    Task<Oportunidad> CrearAsync(string nombre, string cliente, decimal montoEstimado, CancellationToken cancellationToken);

    Task<Oportunidad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Oportunidad>> ListarAsync(EstadoOportunidad? estado, CancellationToken cancellationToken);

    // Regresa false si no se actualizó nada (el estado en la BD ya no era estadoEsperado)
    Task<bool> ActualizarEstadoAsync(int id, EstadoOportunidad estadoEsperado, EstadoOportunidad nuevoEstado, CancellationToken cancellationToken);
}

using Oportunidades.Domain.Entities;
using Oportunidades.Domain.Enums;

namespace Oportunidades.Application.DTOs;

public sealed record OportunidadResponse(
    int Id,
    string Nombre,
    string Cliente,
    decimal MontoEstimado,
    EstadoOportunidad Estado,
    DateTime FechaCreacionUtc)
{
    public static OportunidadResponse FromEntity(Oportunidad oportunidad) => new(
        oportunidad.Id,
        oportunidad.Nombre,
        oportunidad.Cliente,
        oportunidad.MontoEstimado,
        oportunidad.Estado,
        oportunidad.FechaCreacionUtc);
}

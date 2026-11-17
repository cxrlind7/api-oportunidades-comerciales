using Oportunidades.Domain.Enums;

namespace Oportunidades.Domain.Exceptions;

public class TransicionEstadoInvalidaException : ConflictException
{
    public EstadoOportunidad EstadoActual { get; }
    public EstadoOportunidad EstadoSolicitado { get; }

    public TransicionEstadoInvalidaException(EstadoOportunidad estadoActual, EstadoOportunidad estadoSolicitado)
        : base($"No se puede cambiar de {estadoActual} a {estadoSolicitado}.")
    {
        EstadoActual = estadoActual;
        EstadoSolicitado = estadoSolicitado;
    }
}

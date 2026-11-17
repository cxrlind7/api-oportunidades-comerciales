using Oportunidades.Domain.Enums;
using Oportunidades.Domain.Exceptions;

namespace Oportunidades.Domain.Entities;

public class Oportunidad
{
    public int Id { get; }
    public string Nombre { get; }
    public string Cliente { get; }
    public decimal MontoEstimado { get; }
    public EstadoOportunidad Estado { get; private set; }
    public DateTime FechaCreacionUtc { get; }

    public Oportunidad(
        int id,
        string nombre,
        string cliente,
        decimal montoEstimado,
        EstadoOportunidad estado,
        DateTime fechaCreacionUtc)
    {
        Id = id;
        Nombre = nombre;
        Cliente = cliente;
        MontoEstimado = montoEstimado;
        Estado = estado;
        FechaCreacionUtc = fechaCreacionUtc;
    }

    // Regresa false si es el mismo estado (no cambia nada) y true si sí cambió.
    // Si la transición no se permite lanza TransicionEstadoInvalidaException.
    public bool CambiarEstado(EstadoOportunidad nuevoEstado)
    {
        if (nuevoEstado == Estado)
            return false;

        if (!EsTransicionPermitida(Estado, nuevoEstado))
            throw new TransicionEstadoInvalidaException(Estado, nuevoEstado);

        Estado = nuevoEstado;
        return true;
    }

    // Nueva -> EnSeguimiento -> Ganada o Perdida
    public static bool EsTransicionPermitida(EstadoOportunidad actual, EstadoOportunidad nuevo) =>
        (actual, nuevo) switch
        {
            (EstadoOportunidad.Nueva, EstadoOportunidad.EnSeguimiento) => true,
            (EstadoOportunidad.EnSeguimiento, EstadoOportunidad.Ganada) => true,
            (EstadoOportunidad.EnSeguimiento, EstadoOportunidad.Perdida) => true,
            _ => false
        };
}

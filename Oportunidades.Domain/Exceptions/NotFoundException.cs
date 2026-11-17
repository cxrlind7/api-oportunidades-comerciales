namespace Oportunidades.Domain.Exceptions;

// 404
public class NotFoundException : Exception
{
    public NotFoundException(string recurso, object id)
        : base($"No se encontró {recurso} con id {id}.")
    {
    }
}

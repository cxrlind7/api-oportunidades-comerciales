namespace Oportunidades.Domain.Exceptions;

// Cuando otra petición cambió el registro antes que nosotros
public class ConcurrencyConflictException : ConflictException
{
    public ConcurrencyConflictException(string recurso, object id)
        : base($"{recurso} {id} fue modificada por otra petición, vuelve a intentarlo.")
    {
    }
}

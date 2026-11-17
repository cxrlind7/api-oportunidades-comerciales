namespace Oportunidades.Domain.Exceptions;

// Base para los errores que se regresan como 409
public abstract class ConflictException : Exception
{
    protected ConflictException(string message) : base(message)
    {
    }
}

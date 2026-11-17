namespace Oportunidades.Application.Validation;

// Se regresa como 400. Errors tiene los mensajes por campo.
public class RequestValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Hay datos inválidos en la petición.")
    {
        Errors = errors;
    }
}

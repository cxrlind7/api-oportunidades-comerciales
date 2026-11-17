namespace Oportunidades.Application.Validation;

// Junta todos los errores para regresarlos juntos y no uno por uno
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            messages = new List<string>();
            _errors[field] = messages;
        }

        messages.Add(message);
    }

    public void ThrowIfAny()
    {
        if (HasErrors)
            throw new RequestValidationException(_errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));
    }
}

using Oportunidades.Application.DTOs;
using Oportunidades.Domain.Enums;

namespace Oportunidades.Application.Validation;

// Validaciones de lo que llega a la API. Los nombres de campo van igual que en el JSON.
public static class OportunidadValidator
{
    public const int NombreMaxLength = 100;
    public const int ClienteMaxLength = 120;

    // DECIMAL(18,2) = 16 enteros y 2 decimales
    public const decimal MontoMaximo = 9_999_999_999_999_999.99m;

    private static readonly string EstadosValidos = string.Join(", ", Enum.GetNames<EstadoOportunidad>());

    // Valida y regresa los textos ya con Trim
    public static (string Nombre, string Cliente, decimal MontoEstimado) ValidarCreacion(CrearOportunidadRequest request)
    {
        var errors = new ValidationErrors();

        var nombre = ValidarTexto(request.Nombre, "nombre", NombreMaxLength, errors);
        var cliente = ValidarTexto(request.Cliente, "cliente", ClienteMaxLength, errors);
        ValidarMonto(request.MontoEstimado, errors);

        errors.ThrowIfAny();
        return (nombre!, cliente!, request.MontoEstimado!.Value);
    }

    public static void ValidarId(int id)
    {
        var errors = new ValidationErrors();
        ValidarId(id, errors);
        errors.ThrowIfAny();
    }

    // Valida id y estado juntos para regresar los dos errores si vienen mal
    public static EstadoOportunidad ValidarCambioEstado(int id, ActualizarEstadoRequest request)
    {
        var errors = new ValidationErrors();
        ValidarId(id, errors);

        EstadoOportunidad estado = default;
        if (string.IsNullOrWhiteSpace(request.Estado))
            errors.Add("estado", "El estado es obligatorio.");
        else if (!TryParseEstado(request.Estado, out estado))
            errors.Add("estado", MensajeEstadoInvalido(request.Estado));

        errors.ThrowIfAny();
        return estado;
    }

    // Si no mandan filtro regresa null (todas)
    public static EstadoOportunidad? ValidarFiltroEstado(string? estado)
    {
        if (string.IsNullOrEmpty(estado))
            return null;

        if (TryParseEstado(estado, out var resultado))
            return resultado;

        var errors = new ValidationErrors();
        errors.Add("estado", MensajeEstadoInvalido(estado));
        errors.ThrowIfAny();
        return null;
    }

    // No uso Enum.TryParse porque acepta números ("1") y eso no debería ser válido
    public static bool TryParseEstado(string? valor, out EstadoOportunidad estado)
    {
        estado = default;
        var nombre = valor?.Trim();
        if (string.IsNullOrEmpty(nombre))
            return false;

        foreach (var candidato in Enum.GetValues<EstadoOportunidad>())
        {
            if (string.Equals(candidato.ToString(), nombre, StringComparison.OrdinalIgnoreCase))
            {
                estado = candidato;
                return true;
            }
        }

        return false;
    }

    private static void ValidarId(int id, ValidationErrors errors)
    {
        if (id <= 0)
            errors.Add("id", "El id debe ser mayor a 0.");
    }

    private static string? ValidarTexto(string? valor, string campo, int maxLength, ValidationErrors errors)
    {
        var texto = valor?.Trim();

        if (string.IsNullOrEmpty(texto))
            errors.Add(campo, $"El {campo} es obligatorio.");
        else if (texto.Length > maxLength)
            errors.Add(campo, $"El {campo} no puede tener más de {maxLength} caracteres.");

        return texto;
    }

    private static void ValidarMonto(decimal? monto, ValidationErrors errors)
    {
        if (monto is null)
        {
            errors.Add("montoEstimado", "El monto es obligatorio.");
            return;
        }

        if (monto <= 0)
            errors.Add("montoEstimado", "El monto debe ser mayor a 0.");

        if (monto > MontoMaximo)
            errors.Add("montoEstimado", "El monto no puede tener más de 16 dígitos enteros.");

        if (decimal.Round(monto.Value, 2) != monto.Value)
            errors.Add("montoEstimado", "El monto solo puede tener 2 decimales.");
    }

    private static string MensajeEstadoInvalido(string valor) =>
        $"'{valor}' no es un estado válido. Usa: {EstadosValidos}.";
}

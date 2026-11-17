namespace Oportunidades.Application.DTOs;

// El estado llega como string para poder validar y regresar 400 si no existe
public sealed record ActualizarEstadoRequest(string? Estado);

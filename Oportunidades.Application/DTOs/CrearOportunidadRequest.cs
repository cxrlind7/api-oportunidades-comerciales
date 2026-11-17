namespace Oportunidades.Application.DTOs;

// Todo es nullable para poder decir qué campo falta en vez de recibir valores por defecto
public sealed record CrearOportunidadRequest(string? Nombre, string? Cliente, decimal? MontoEstimado);

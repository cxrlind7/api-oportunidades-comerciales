namespace Oportunidades.Api.Auth;

public sealed record LoginRequest(string? Usuario, string? Password);

// ExpiresIn va en segundos
public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    DateTime ExpiresAtUtc,
    string Usuario,
    string Rol);

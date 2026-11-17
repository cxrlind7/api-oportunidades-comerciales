namespace Oportunidades.Api.Auth;

// Sección "Jwt" del appsettings. La Key va en appsettings.Development.json (no se sube a git)
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public const int MinKeyBytes = 32; // HS256 pide mínimo 32 bytes

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

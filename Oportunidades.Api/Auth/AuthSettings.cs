namespace Oportunidades.Api.Auth;

// Usuarios para el login. El rol está en appsettings.json y el password en appsettings.Development.json
public sealed class AuthSettings
{
    public const string SectionName = "Auth";

    public Dictionary<string, UsuarioSettings> Usuarios { get; set; } = new();
}

public sealed class UsuarioSettings
{
    public string Rol { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

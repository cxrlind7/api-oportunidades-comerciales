using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Oportunidades.Api.Auth;

// Genera los tokens con la misma key, issuer y audience que valida la API
public sealed class JwtTokenFactory
{
    private readonly JwtSettings _jwt;

    public JwtTokenFactory(IOptions<JwtSettings> options)
    {
        _jwt = options.Value;
    }

    public LoginResponse Crear(string usuario, string rol)
    {
        var ahora = DateTime.UtcNow;
        var expira = ahora.AddMinutes(_jwt.ExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = expira,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = usuario,
                ["role"] = rol,
                ["jti"] = Guid.NewGuid().ToString()
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new LoginResponse(token, "Bearer", _jwt.ExpirationMinutes * 60, expira, usuario, rol);
    }
}

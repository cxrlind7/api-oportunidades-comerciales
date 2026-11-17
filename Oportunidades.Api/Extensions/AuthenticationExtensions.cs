using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Oportunidades.Api.Auth;

namespace Oportunidades.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        // Si falta algo de la config la API no arranca y dice qué falta
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer), "Falta Jwt:Issuer en appsettings.json")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Audience), "Falta Jwt:Audience en appsettings.json")
            .Validate(s => Encoding.UTF8.GetByteCount(s.Key) >= JwtSettings.MinKeyBytes,
                "Falta Jwt:Key o es muy corta (mínimo 32 caracteres). Revisa appsettings.Development.json")
            .Validate(s => s.ExpirationMinutes is > 0 and <= 1440,
                "Jwt:ExpirationMinutes debe estar entre 1 y 1440")
            .ValidateOnStart();

        services.AddOptions<AuthSettings>()
            .BindConfiguration(AuthSettings.SectionName)
            .Validate(s => s.Usuarios.Count > 0, "No hay usuarios en Auth:Usuarios")
            .Validate(s => s.Usuarios.Values.All(u => Roles.Todos.Contains(u.Rol)),
                "Cada usuario debe tener rol Consulta o Editor")
            .Validate(s => s.Usuarios.Values.All(u => !string.IsNullOrEmpty(u.Password)),
                "Falta el password de algún usuario. Revisa appsettings.Development.json")
            .ValidateOnStart();

        services.AddSingleton<JwtTokenFactory>();
        services.AddSingleton<CredencialesService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Lo configuro con IOptions para que las pruebas puedan cambiar la config
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                options.MapInboundClaims = false; // para que "role" y "sub" lleguen con ese nombre
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,

                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Politicas.Lectura, policy => policy.RequireRole(Roles.Consulta, Roles.Editor))
            .AddPolicy(Politicas.Escritura, policy => policy.RequireRole(Roles.Editor));

        return services;
    }
}

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Oportunidades.Api.Auth;

public sealed class CredencialesService
{
    private readonly IOptions<AuthSettings> _settings;

    public CredencialesService(IOptions<AuthSettings> settings)
    {
        _settings = settings;
    }

    // Regresa el usuario y su rol, o null si el usuario o el password están mal
    public (string Usuario, string Rol)? Validar(string usuario, string password)
    {
        var encontrado = _settings.Value.Usuarios
            .FirstOrDefault(u => string.Equals(u.Key, usuario, StringComparison.OrdinalIgnoreCase));

        // comparo siempre aunque no exista el usuario, así no se puede saber cuáles existen por el tiempo de respuesta
        var esperado = encontrado.Value?.Password ?? string.Empty;
        var coincide = CryptographicOperations.FixedTimeEquals(Hash(password), Hash(esperado));

        return encontrado.Value is not null && coincide
            ? (encontrado.Key, encontrado.Value.Rol)
            : null;
    }

    private static byte[] Hash(string valor) => SHA256.HashData(Encoding.UTF8.GetBytes(valor));
}

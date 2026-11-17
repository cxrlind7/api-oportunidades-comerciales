using Microsoft.Data.SqlClient;

namespace Oportunidades.Infrastructure.Persistence;

public sealed class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Falta la cadena de conexión.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public SqlConnection Create() => new(_connectionString);
}

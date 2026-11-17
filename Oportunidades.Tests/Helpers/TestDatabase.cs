using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace Oportunidades.Tests.Helpers;

// Crea una base solo para las pruebas usando los scripts de database/ y la borra al final
public sealed class TestDatabase
{
    private const string Nombre = "OportunidadesComerciales_Tests";
    private const string Servidor = @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";

    public string ConnectionString => $"{Servidor};Database={Nombre}";

    private string ConexionMaster => $"{Servidor};Database=master";

    public async Task CrearAsync()
    {
        await EliminarAsync();

        // uso los mismos scripts pero cambiando el nombre de la base
        var carpeta = Path.Combine(AppContext.BaseDirectory, "database");
        foreach (var script in Directory.GetFiles(carpeta, "*.sql").Order())
        {
            var sql = (await File.ReadAllTextAsync(script)).Replace("OportunidadesComerciales", Nombre);
            await EjecutarAsync(sql);
        }
    }

    public async Task EliminarAsync()
    {
        SqlConnection.ClearAllPools();
        await EjecutarAsync($"""
            IF DB_ID('{Nombre}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{Nombre}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{Nombre}];
            END
            """);
    }

    // ADO.NET no entiende GO, entonces separo el script y ejecuto cada parte
    private async Task EjecutarAsync(string sql)
    {
        await using var connection = new SqlConnection(ConexionMaster);
        await connection.OpenAsync();

        var partes = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        foreach (var parte in partes.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            await using var command = new SqlCommand(parte, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}

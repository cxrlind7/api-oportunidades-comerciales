using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oportunidades.Api.Auth;

namespace Oportunidades.Tests.Helpers;

// Levanta la API en memoria con su propia config y una base de pruebas en SQL Server.
// Uso el ambiente "Testing" para no depender de appsettings.Development.json
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string PasswordConsulta = "consulta-pruebas";
    public const string PasswordEditor = "editor-pruebas";

    private readonly TestDatabase _database = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _database.ConnectionString,
                ["Jwt:Key"] = "clave-exclusiva-de-pruebas-de-integracion-0123456789",
                ["Auth:Usuarios:consulta:Password"] = PasswordConsulta,
                ["Auth:Usuarios:editor:Password"] = PasswordEditor
            }));
    }

    // Cliente con token del rol que le pase. Si rol es null va sin token
    public HttpClient CrearCliente(string? rol)
    {
        var client = CreateClient();
        if (rol is not null)
        {
            var token = Services.GetRequiredService<JwtTokenFactory>().Crear($"pruebas-{rol.ToLowerInvariant()}", rol);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        return client;
    }

    public Task InitializeAsync() => _database.CrearAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.EliminarAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "API contra SQL Server";
}

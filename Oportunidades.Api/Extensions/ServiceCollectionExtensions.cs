using Microsoft.Extensions.Options;
using Oportunidades.Application.Interfaces;
using Oportunidades.Application.Services;
using Oportunidades.Infrastructure.Persistence;
using Oportunidades.Infrastructure.Repositories;

namespace Oportunidades.Api.Extensions;

public sealed class SqlServerSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOportunidades(this IServiceCollection services)
    {
        services.AddOptions<SqlServerSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
                settings.ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
            .Validate(s => !string.IsNullOrWhiteSpace(s.ConnectionString),
                "Falta ConnectionStrings:DefaultConnection. Revisa appsettings.Development.json")
            .ValidateOnStart();

        services.AddSingleton(sp =>
            new SqlConnectionFactory(sp.GetRequiredService<IOptions<SqlServerSettings>>().Value.ConnectionString));

        services.AddScoped<IOportunidadRepository, OportunidadRepository>();
        services.AddScoped<IOportunidadService, OportunidadService>();

        return services;
    }
}

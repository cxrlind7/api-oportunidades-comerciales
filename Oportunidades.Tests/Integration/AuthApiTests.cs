using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Oportunidades.Api.Auth;
using Oportunidades.Tests.Helpers;

namespace Oportunidades.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthApiTests
{
    private readonly ApiFactory _factory;

    public AuthApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_RegresaUnTokenQueSirve()
    {
        var client = _factory.CrearCliente(rol: null);

        var respuesta = await client.PostAsJsonAsync("/api/auth/login",
            new { usuario = "editor", password = ApiFactory.PasswordEditor });

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();

        // uso el token para llamar a otro endpoint
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        (await client.GetAsync("/api/oportunidades")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Oportunidades.Api.Auth;
using Oportunidades.Application.DTOs;
using Oportunidades.Domain.Enums;
using Oportunidades.Tests.Helpers;

namespace Oportunidades.Tests.Integration;

[Collection(ApiCollection.Name)]
public class OportunidadesApiTests
{
    private const string Ruta = "/api/oportunidades";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ApiFactory _factory;

    public OportunidadesApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Crear_YLuegoConsultar()
    {
        var editor = _factory.CrearCliente(Roles.Editor);

        var respuesta = await editor.PostAsJsonAsync(Ruta, new { nombre = " ERP ", cliente = "ACME", montoEstimado = 2500.50m });

        respuesta.StatusCode.Should().Be(HttpStatusCode.Created);
        respuesta.Headers.Location.Should().NotBeNull();

        var creada = await respuesta.Content.ReadFromJsonAsync<OportunidadResponse>(Json);
        creada!.Nombre.Should().Be("ERP");
        creada.Estado.Should().Be(EstadoOportunidad.Nueva);

        // la consulto con el rol Consulta usando la URL del Location
        var consulta = _factory.CrearCliente(Roles.Consulta);
        var obtenida = await consulta.GetFromJsonAsync<OportunidadResponse>(respuesta.Headers.Location, Json);

        obtenida.Should().BeEquivalentTo(creada);
    }

    public static TheoryData<object, string> DatosInvalidos => new()
    {
        { new { nombre = "   ", cliente = "ACME", montoEstimado = 10m }, "nombre" },
        { new { nombre = "ERP", cliente = new string('c', 121), montoEstimado = 10m }, "cliente" },
        { new { nombre = "ERP", cliente = "ACME", montoEstimado = -5m }, "montoEstimado" }
    };

    [Theory]
    [MemberData(nameof(DatosInvalidos))]
    public async Task Crear_ConDatosInvalidos_Regresa400(object body, string campo)
    {
        var respuesta = await _factory.CrearCliente(Roles.Editor).PostAsJsonAsync(Ruta, body);

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
        problema!.Errors.Should().ContainKey(campo);
    }

    [Fact]
    public async Task Listar_ConEstadoQueNoExiste_Regresa400()
    {
        var respuesta = await _factory.CrearCliente(Roles.Consulta).GetAsync($"{Ruta}?estado=Cancelada");

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Consultar_IdQueNoExiste_Regresa404()
    {
        var respuesta = await _factory.CrearCliente(Roles.Consulta).GetAsync($"{Ruta}/999999");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SinToken_Regresa401()
    {
        var respuesta = await _factory.CrearCliente(rol: null).GetAsync(Ruta);

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RolConsulta_NoPuedeCrear_Regresa403()
    {
        var respuesta = await _factory.CrearCliente(Roles.Consulta)
            .PostAsJsonAsync(Ruta, new { nombre = "ERP", cliente = "ACME", montoEstimado = 10m });

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RolConsulta_NoPuedeCambiarEstado_Regresa403()
    {
        var respuesta = await _factory.CrearCliente(Roles.Consulta)
            .PatchAsJsonAsync($"{Ruta}/1/estado", new { estado = "EnSeguimiento" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

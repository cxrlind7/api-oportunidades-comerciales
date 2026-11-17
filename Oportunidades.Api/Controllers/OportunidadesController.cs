using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportunidades.Api.Auth;
using Oportunidades.Application.DTOs;
using Oportunidades.Application.Interfaces;

namespace Oportunidades.Api.Controllers;

// El controller solo recibe y responde. Las validaciones están en el servicio
// y los errores los convierte GlobalExceptionHandler.
[ApiController]
[Route("api/oportunidades")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class OportunidadesController : ControllerBase
{
    private readonly IOportunidadService _service;

    public OportunidadesController(IOportunidadService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = Politicas.Escritura)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OportunidadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OportunidadResponse>> Crear([FromBody] CrearOportunidadRequest request, CancellationToken cancellationToken)
    {
        var creada = await _service.CrearAsync(request, cancellationToken);

        // CreatedAtAction pone el 201 y el header Location
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Politicas.Lectura)]
    [ProducesResponseType(typeof(OportunidadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OportunidadResponse>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        return Ok(await _service.ObtenerPorIdAsync(id, cancellationToken));
    }

    // estado es opcional, si no viene regresa todas
    [HttpGet]
    [Authorize(Policy = Politicas.Lectura)]
    [ProducesResponseType(typeof(IReadOnlyList<OportunidadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<OportunidadResponse>>> Listar([FromQuery] string? estado, CancellationToken cancellationToken)
    {
        return Ok(await _service.ListarAsync(estado, cancellationToken));
    }

    [HttpPatch("{id}/estado")]
    [Authorize(Policy = Politicas.Escritura)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OportunidadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OportunidadResponse>> ActualizarEstado(int id, [FromBody] ActualizarEstadoRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.ActualizarEstadoAsync(id, request, cancellationToken));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportunidades.Api.Auth;
using Oportunidades.Application.Validation;

namespace Oportunidades.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly CredencialesService _credenciales;
    private readonly JwtTokenFactory _tokenFactory;

    public AuthController(CredencialesService credenciales, JwtTokenFactory tokenFactory)
    {
        _credenciales = credenciales;
        _tokenFactory = tokenFactory;
    }

    // Regresa el token. Se copia el accessToken y se pega en Authorize de Swagger
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var errors = new ValidationErrors();
        if (string.IsNullOrWhiteSpace(request.Usuario))
            errors.Add("usuario", "El usuario es obligatorio.");
        if (string.IsNullOrEmpty(request.Password))
            errors.Add("password", "La contraseña es obligatoria.");
        errors.ThrowIfAny();

        var autenticado = _credenciales.Validar(request.Usuario!.Trim(), request.Password!);
        if (autenticado is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "No autorizado",
                detail: "Usuario o contraseña incorrectos.");
        }

        var (usuario, rol) = autenticado.Value;
        return Ok(_tokenFactory.Crear(usuario, rol));
    }
}

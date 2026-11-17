using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Oportunidades.Application.Validation;
using Oportunidades.Domain.Exceptions;

namespace Oportunidades.Api.Middleware;

// Aquí caen todas las excepciones y se convierten a ProblemDetails:
// validación = 400, no existe = 404, conflicto = 409, cualquier otra = 500
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ProblemDetailsFactory problemDetailsFactory,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _problemDetailsFactory = problemDetailsFactory;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            RequestValidationException e => CrearProblemaValidacion(httpContext, e),

            NotFoundException e => _problemDetailsFactory.CreateProblemDetails(
                httpContext, StatusCodes.Status404NotFound, "No encontrado", detail: e.Message),

            ConflictException e => _problemDetailsFactory.CreateProblemDetails(
                httpContext, StatusCodes.Status409Conflict, "Conflicto", detail: e.Message),

            _ => null
        };

        if (problem is null)
        {
            // el error completo solo va al log, al cliente le mando un mensaje general
            _logger.LogError(exception, "Error no controlado en {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            problem = _problemDetailsFactory.CreateProblemDetails(
                httpContext,
                StatusCodes.Status500InternalServerError,
                "Error interno",
                detail: "Ocurrió un error, intenta más tarde.");
        }

        problem.Instance = httpContext.Request.Path;
        httpContext.Response.StatusCode = problem.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private ValidationProblemDetails CrearProblemaValidacion(HttpContext httpContext, RequestValidationException exception)
    {
        var modelState = new ModelStateDictionary();
        foreach (var (campo, mensajes) in exception.Errors)
        foreach (var mensaje in mensajes)
            modelState.AddModelError(campo, mensaje);

        return _problemDetailsFactory.CreateValidationProblemDetails(
            httpContext,
            modelState,
            StatusCodes.Status400BadRequest,
            title: exception.Message);
    }
}

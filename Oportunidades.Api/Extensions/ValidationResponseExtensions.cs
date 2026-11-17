using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Oportunidades.Api.Extensions;

public static class ValidationResponseExtensions
{
    private const string CampoCuerpo = "body";

    // Cambia el 400 automático de [ApiController] (JSON mal escrito, tipo incorrecto, id que no es número)
    // para que use los mismos nombres de campo que mis validaciones
    public static IServiceCollection ConfigureValidationResponses(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = CrearRespuesta);

        return services;
    }

    private static IActionResult CrearRespuesta(ActionContext context)
    {
        var parametrosCuerpo = context.ActionDescriptor.Parameters
            .Where(p => p.BindingInfo?.BindingSource == BindingSource.Body)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var conError = context.ModelState
            .Where(e => e.Value is { Errors.Count: > 0 })
            .Select(e => e.Key)
            .ToList();

        // cuando un campo trae un tipo incorrecto también sale "request is required", ese lo quito
        var hayErroresEspecificos = conError.Any(k => !EsErrorDeCuerpo(k, parametrosCuerpo));

        var errores = new ModelStateDictionary();
        foreach (var clave in conError)
        {
            if (EsErrorDeCuerpo(clave, parametrosCuerpo))
            {
                if (!hayErroresEspecificos && !errores.ContainsKey(CampoCuerpo))
                    errores.AddModelError(CampoCuerpo, "El body es obligatorio y debe ser un JSON válido.");
                continue;
            }

            var campo = NormalizarCampo(clave);
            errores.AddModelError(campo, $"El valor de '{campo}' no es válido.");
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateValidationProblemDetails(
            context.HttpContext,
            errores,
            StatusCodes.Status400BadRequest,
            title: "Hay datos inválidos en la petición.");
        problem.Instance = context.HttpContext.Request.Path;

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    }

    private static bool EsErrorDeCuerpo(string clave, HashSet<string> parametrosCuerpo) =>
        clave is "" or "$" || parametrosCuerpo.Contains(clave);

    // "$.montoEstimado" → "montoEstimado"; "Id" → "id"
    private static string NormalizarCampo(string clave)
    {
        var campo = clave.StartsWith("$.", StringComparison.Ordinal) ? clave[2..] : clave;
        return campo.Length > 0 ? char.ToLowerInvariant(campo[0]) + campo[1..] : campo;
    }
}

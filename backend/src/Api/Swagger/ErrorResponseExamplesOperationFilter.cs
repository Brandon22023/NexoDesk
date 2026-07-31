using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Swagger;

/// <summary>Agrega ejemplos contractuales a las respuestas ProblemDetails.</summary>
public sealed class ErrorResponseExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = "/" + context.ApiDescription.RelativePath?.TrimStart('/');
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();

        foreach (var response in operation.Responses)
        {
            if (response.Value.Content.TryGetValue("application/problem+json", out var mediaType))
            {
                var examples = GetExamples(path, method, response.Key);
                if (examples.Count > 0) mediaType.Examples = examples;
            }
        }
    }

    private static Dictionary<string, OpenApiExample> GetExamples(
        string path, string? method, string statusCode)
    {
        if (path == "/api/v1/solicitudes" && method == "GET" && statusCode == "400")
            return One("parametroInvalido", Problem(400, "PARAMETRO_INVALIDO", "Parámetro inválido",
                "page debe ser mayor o igual a 1 y pageSize debe estar entre 1 y 100."));

        if (path == "/api/v1/solicitudes/{id}/transiciones" && method == "POST")
        {
            if (statusCode == "409") return One("transicionInvalida", Problem(409,
                "TRANSICION_INVALIDA", "Transición inválida",
                "No se puede aplicar 'resolver' sobre una solicitud en estado 'Nueva'."));
            if (statusCode == "422") return Transition422Examples();
        }

        if (path == "/api/v1/auth/login" && method == "POST" && statusCode == "401")
            return One("credencialesInvalidas", Problem(401, "NO_AUTENTICADO", "No autenticado",
                "El correo o la contraseña no son válidos."));

        return statusCode switch
        {
            "401" => One("noAutenticado", Problem(401, "NO_AUTENTICADO", "No autenticado",
                "El token es ausente, inválido o expiró.")),
            "403" => One("operacionNoPermitida", Problem(403, "OPERACION_NO_PERMITIDA",
                "Operación no permitida", "El rol del usuario no permite esta operación.")),
            "404" => One("recursoNoEncontrado", Problem(404, "RECURSO_NO_ENCONTRADO",
                "Recurso no encontrado", "La solicitud no existe o no pertenece a la organización actual.")),
            "422" => One("validacion", Problem(422, "VALIDACION", "Error de validación",
                "Uno o más campos no son válidos.", "titulo", "El título debe tener al menos 5 caracteres.")),
            "500" => One("errorInterno", Problem(500, "ERROR_INTERNO", "Error interno",
                "Ocurrió un error inesperado.")),
            _ => []
        };
    }

    private static Dictionary<string, OpenApiExample> Transition422Examples() => new()
    {
        ["agenteInvalido"] = Example(Problem(422, "AGENTE_INVALIDO", "Agente inválido",
            "Debe indicar un agente válido.")),
        ["motivoRequerido"] = Example(Problem(422, "MOTIVO_REQUERIDO", "Motivo requerido",
            "El motivo para resolver debe tener al menos 20 caracteres.")),
        ["validacion"] = Example(Problem(422, "VALIDACION", "Error de validación",
            "Uno o más campos no son válidos.", "accion", "El campo Accion es obligatorio."))
    };

    private static Dictionary<string, OpenApiExample> One(string name, OpenApiObject value) =>
        new() { [name] = Example(value) };

    private static OpenApiExample Example(OpenApiObject value) => new() { Value = value };

    private static OpenApiObject Problem(
        int status,
        string codigo,
        string title,
        string detail,
        string? errorField = null,
        string? errorMessage = null)
    {
        var value = new OpenApiObject
        {
            ["type"] = new OpenApiString(
                $"https://mesasitec.local/errores/{codigo.ToLowerInvariant().Replace('_', '-')}"),
            ["title"] = new OpenApiString(title),
            ["status"] = new OpenApiInteger(status),
            ["detail"] = new OpenApiString(detail),
            ["codigo"] = new OpenApiString(codigo)
        };

        if (errorField is not null && errorMessage is not null)
        {
            value["errores"] = new OpenApiObject
            {
                [errorField] = new OpenApiArray { new OpenApiString(errorMessage) }
            };
        }

        return value;
    }
}

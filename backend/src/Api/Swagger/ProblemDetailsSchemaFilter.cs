using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Swagger;

/// <summary>Documenta las extensiones obligatorias del formato de error de NexoDesk.</summary>
public sealed class ProblemDetailsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(ProblemDetails)) return;

        schema.Properties["codigo"] = new OpenApiSchema
        {
            Type = "string",
            Description = "Código de error obligatorio definido por el contrato."
        };
        schema.Properties["errores"] = new OpenApiSchema
        {
            Type = "object",
            Description = "Errores por campo; solo aparece en validaciones 400 y 422.",
            AdditionalProperties = new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema { Type = "string" }
            }
        };
        schema.Required.Add("codigo");
    }
}

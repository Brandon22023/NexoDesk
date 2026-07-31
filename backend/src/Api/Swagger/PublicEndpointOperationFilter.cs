using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Swagger;

/// <summary>Elimina el requisito Bearer global de las operaciones anónimas.</summary>
public sealed class PublicEndpointOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var allowsAnonymous = context.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute), true)
            || context.MethodInfo.DeclaringType?.IsDefined(typeof(AllowAnonymousAttribute), true) == true;

        if (allowsAnonymous)
        {
            operation.Security?.Clear();
        }
    }
}

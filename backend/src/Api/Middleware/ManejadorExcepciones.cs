using Aplicacion.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

// Manejador global de excepciones.
// Convierte las excepciones de la aplicación en respuestas HTTP
public sealed class ManejadorExcepciones(
    ILogger<ManejadorExcepciones> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception, 
        CancellationToken cancellationToken)
    {   
        // Relaciona cada excepción de negocio con su código HTTP
        // y el código de error esperado por la API.
        var (status, title, codigo) = exception switch
        {
            NoAutenticadoException => (401, "No autenticado", "NO_AUTENTICADO"),
            RecursoNoEncontradoException => (404, "Recurso no encontrado", "RECURSO_NO_ENCONTRADO"),
            // RN-03: los permisos insuficientes siempre usan 403 y este código.
            OperacionNoPermitidaException => (403, "Operación no permitida", "OPERACION_NO_PERMITIDA"),
            TransicionInvalidaException => (409, "Transición inválida", "TRANSICION_INVALIDA"),
            // RN-05: un agente inválido responde 422 con este código.
            AgenteInvalidoException => (422, "Agente inválido", "AGENTE_INVALIDO"),
            // RN-06: motivo ausente o corto responde 422 con este código.
            MotivoRequeridoException => (422, "Motivo requerido", "MOTIVO_REQUERIDO"),
            ValidacionException => (422, "Error de validación", "VALIDACION"),
            _ => (500, "Error interno", "ERROR_INTERNO")
        };
        // Solo los errores inesperados se registran como error interno.
        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado durante la petición.");
        }
        
        // Construye la respuesta siguiendo el formato application/problem+json.
        var problem = new ProblemDetails
        {
            Type = $"https://mesasitec.local/errores/{codigo.ToLowerInvariant().Replace('_', '-')}",
            Title = title,
            Status = status,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado."
                : exception.Message
        };

        // Campo obligatorio que utilizan las pruebas automáticas.
        problem.Extensions["codigo"] = codigo;

         // Los errores de validación incluyen el detalle por campo.
        if (exception is ValidacionException validacion)
        {
            problem.Extensions["errores"] = validacion.Errores;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}

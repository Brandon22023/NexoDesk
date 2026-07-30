using Aplicacion.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

public sealed class ManejadorExcepciones(
    ILogger<ManejadorExcepciones> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, codigo) = exception switch
        {
            NoAutenticadoException => (401, "No autenticado", "NO_AUTENTICADO"),
            RecursoNoEncontradoException => (404, "Recurso no encontrado", "RECURSO_NO_ENCONTRADO"),
            OperacionNoPermitidaException => (403, "Operación no permitida", "OPERACION_NO_PERMITIDA"),
            TransicionInvalidaException => (409, "Transición inválida", "TRANSICION_INVALIDA"),
            AgenteInvalidoException => (422, "Agente inválido", "AGENTE_INVALIDO"),
            MotivoRequeridoException => (422, "Motivo requerido", "MOTIVO_REQUERIDO"),
            ValidacionException => (422, "Error de validación", "VALIDACION"),
            _ => (500, "Error interno", "ERROR_INTERNO")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado durante la petición.");
        }

        var problem = new ProblemDetails
        {
            Type = $"https://mesasitec.local/errores/{codigo.ToLowerInvariant().Replace('_', '-')}",
            Title = title,
            Status = status,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado."
                : exception.Message
        };
        problem.Extensions["codigo"] = codigo;

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

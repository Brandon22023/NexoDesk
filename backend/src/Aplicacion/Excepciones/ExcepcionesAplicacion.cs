namespace Aplicacion.Excepciones;

public abstract class ExcepcionAplicacion(
    string codigo,
    string message) : Exception(message)
{
    public string Codigo { get; } = codigo;
}

public sealed class RecursoNoEncontradoException(string message)
    : ExcepcionAplicacion("RECURSO_NO_ENCONTRADO", message);

public sealed class NoAutenticadoException(string message)
    : ExcepcionAplicacion("NO_AUTENTICADO", message);

public sealed class OperacionNoPermitidaException(string message)
    : ExcepcionAplicacion("OPERACION_NO_PERMITIDA", message);

public sealed class TransicionInvalidaException(string message)
    : ExcepcionAplicacion("TRANSICION_INVALIDA", message);

public sealed class AgenteInvalidoException(string message)
    : ExcepcionAplicacion("AGENTE_INVALIDO", message);

public sealed class MotivoRequeridoException(string message)
    : ExcepcionAplicacion("MOTIVO_REQUERIDO", message);

public sealed class ValidacionException(
    string message,
    IReadOnlyDictionary<string, string[]> errores)
    : ExcepcionAplicacion("VALIDACION", message)
{
    public IReadOnlyDictionary<string, string[]> Errores { get; } = errores;
}

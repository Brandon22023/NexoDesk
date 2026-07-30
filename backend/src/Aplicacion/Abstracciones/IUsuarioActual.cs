using Aplicacion.DTOs.Autenticacion;

namespace Aplicacion.Abstracciones;

public interface IUsuarioActual
{
    ContextoUsuarioActual? Obtener();
}

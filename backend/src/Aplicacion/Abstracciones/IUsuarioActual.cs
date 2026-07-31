using Aplicacion.DTOs.Autenticacion;

namespace Aplicacion.Abstracciones;


// Proporciona información del usuario autenticado actualmente.
public interface IUsuarioActual
{
    // Obtiene el contexto del usuario desde la sesión activa.
    ContextoUsuarioActual? Obtener();
}

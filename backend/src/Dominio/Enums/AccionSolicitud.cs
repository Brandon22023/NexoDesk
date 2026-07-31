namespace Dominio.Enums;

/// Enumeración que define las acciones o transiciones que pueden
/// realizarse sobre una solicitud durante su ciclo de vida.
public enum AccionSolicitud
{
    Asignar,
    Iniciar,
    Resolver,
    Cerrar,
    Reabrir,
    Cancelar
}

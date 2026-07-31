namespace Dominio.Enums;

/// Enumeración que define los posibles estados por los que puede
/// pasar una solicitud durante su ciclo de vida.
public enum EstadoSolicitud
{
    Nueva,
    Asignada,
    EnProceso,
    Resuelta,
    Cerrada,
    Cancelada
}

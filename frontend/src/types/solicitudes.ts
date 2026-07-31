// Estos valores reflejan el contrato del backend y mantienen alineados filtros, formularios y acciones visibles.
export const ESTADOS_SOLICITUD = [
  'Nueva',
  'Asignada',
  'EnProceso',
  'Resuelta',
  'Cerrada',
  'Cancelada',
] as const

export const PRIORIDADES_SOLICITUD = [
  'Baja',
  'Media',
  'Alta',
  'Critica',
] as const

export const ACCIONES_SOLICITUD = [
  'asignar',
  'iniciar',
  'resolver',
  'cerrar',
  'reabrir',
  'cancelar',
] as const

export type EstadoSolicitud = typeof ESTADOS_SOLICITUD[number]
export type PrioridadSolicitud = typeof PRIORIDADES_SOLICITUD[number]
export type AccionSolicitud = typeof ACCIONES_SOLICITUD[number]

export interface ReferenciaSolicitudDto {
  id: string
  nombre: string
}

export interface SolicitudListaDto {
  id: string
  codigo: string
  titulo: string
  estado: EstadoSolicitud
  prioridad: PrioridadSolicitud
  categoria: ReferenciaSolicitudDto
  agente: ReferenciaSolicitudDto | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface PaginaSolicitudesDto {
  items: SolicitudListaDto[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface SolicitudDetalleDto {
  id: string
  codigo: string
  titulo: string
  descripcion: string
  estado: EstadoSolicitud
  prioridad: PrioridadSolicitud
  categoria: ReferenciaSolicitudDto
  solicitante: ReferenciaSolicitudDto
  agente: ReferenciaSolicitudDto | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
}

export interface SolicitudFormData {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: PrioridadSolicitud
}

export type CrearSolicitudRequest = SolicitudFormData
export type EditarSolicitudRequest = SolicitudFormData

export interface TransicionSolicitudRequest {
  accion: AccionSolicitud
  agenteId?: string
  motivo?: string
}

export type OrdenSolicitudes =
  | 'fechaCreacion'
  | '-fechaCreacion'
  | 'prioridad'
  | '-prioridad'
  | 'codigo'

export interface FiltrosSolicitudes {
  estado: EstadoSolicitud | ''
  prioridad: PrioridadSolicitud | ''
  categoriaId: string
  agenteId: string
  q: string
  vencidas: boolean | null
  page: number
  pageSize: number
  sort: OrdenSolicitudes
}

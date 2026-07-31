import type {
  CrearSolicitudRequest,
  EditarSolicitudRequest,
  FiltrosSolicitudes,
  PaginaSolicitudesDto,
  SolicitudDetalleDto,
  TransicionSolicitudRequest,
} from '../types/solicitudes'
import { httpRequest } from './http'

const BASE_PATH = '/api/v1/solicitudes'

export function listarSolicitudes(
  filtros: FiltrosSolicitudes,
  signal?: AbortSignal,
): Promise<PaginaSolicitudesDto> {
  // Los filtros se envían al servidor para mantener resultados y paginación coherentes para toda la organización.
  const query = new URLSearchParams({
    page: String(filtros.page),
    pageSize: String(filtros.pageSize),
    sort: filtros.sort,
  })

  appendIfValue(query, 'estado', filtros.estado)
  appendIfValue(query, 'prioridad', filtros.prioridad)
  appendIfValue(query, 'categoriaId', filtros.categoriaId)
  appendIfValue(query, 'agenteId', filtros.agenteId)
  appendIfValue(query, 'q', filtros.q.trim())

  if (filtros.vencidas !== null) {
    query.set('vencidas', String(filtros.vencidas))
  }

  return httpRequest<PaginaSolicitudesDto>(
    `${BASE_PATH}?${query.toString()}`,
    { signal },
  )
}

export function crearSolicitud(
  request: CrearSolicitudRequest,
): Promise<SolicitudDetalleDto> {
  // La creación devuelve el detalle para llevar a la persona directamente al seguimiento de su solicitud.
  return httpRequest<SolicitudDetalleDto>(BASE_PATH, {
    method: 'POST',
    body: request,
  })
}

export function obtenerSolicitud(
  id: string,
  signal?: AbortSignal,
): Promise<SolicitudDetalleDto> {
  // El detalle se consulta por separado para mostrar siempre la información y las acciones vigentes.
  return httpRequest<SolicitudDetalleDto>(`${BASE_PATH}/${id}`, { signal })
}

export function editarSolicitud(
  id: string,
  request: EditarSolicitudRequest,
): Promise<SolicitudDetalleDto> {
  // El servidor vuelve a validar permisos y recalcula el SLA si cambia categoría o prioridad.
  return httpRequest<SolicitudDetalleDto>(`${BASE_PATH}/${id}`, {
    method: 'PUT',
    body: request,
  })
}

export function transicionarSolicitud(
  id: string,
  request: TransicionSolicitudRequest,
): Promise<SolicitudDetalleDto> {
  // Cada cambio de estado se valida en el servidor según el rol, el estado actual y sus datos requeridos.
  return httpRequest<SolicitudDetalleDto>(
    `${BASE_PATH}/${id}/transiciones`,
    {
      method: 'POST',
      body: request,
    },
  )
}

function appendIfValue(
  query: URLSearchParams,
  key: string,
  value: string,
): void {
  // Los filtros vacíos se omiten para que el servidor aplique el listado general sin criterios innecesarios.
  if (value) {
    query.set(key, value)
  }
}

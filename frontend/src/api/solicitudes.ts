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
  return httpRequest<SolicitudDetalleDto>(BASE_PATH, {
    method: 'POST',
    body: request,
  })
}

export function obtenerSolicitud(
  id: string,
  signal?: AbortSignal,
): Promise<SolicitudDetalleDto> {
  return httpRequest<SolicitudDetalleDto>(`${BASE_PATH}/${id}`, { signal })
}

export function editarSolicitud(
  id: string,
  request: EditarSolicitudRequest,
): Promise<SolicitudDetalleDto> {
  return httpRequest<SolicitudDetalleDto>(`${BASE_PATH}/${id}`, {
    method: 'PUT',
    body: request,
  })
}

export function transicionarSolicitud(
  id: string,
  request: TransicionSolicitudRequest,
): Promise<SolicitudDetalleDto> {
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
  if (value) {
    query.set(key, value)
  }
}

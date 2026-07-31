import type { ApiProblem } from '../types/auth'

interface HttpClientConfiguration {
  getAccessToken: () => string | null
  onUnauthorized: () => void
}

interface HttpRequestOptions extends Omit<RequestInit, 'body'> {
  body?: unknown
  authenticated?: boolean
  redirectOnUnauthorized?: boolean
}

let configuration: HttpClientConfiguration = {
  getAccessToken: () => null,
  onUnauthorized: () => undefined,
}

export class HttpError extends Error {
  readonly status: number
  readonly codigo?: string
  readonly errores?: Record<string, string[]>

  constructor(message: string, status: number, problem?: ApiProblem) {
    super(message)
    this.name = 'HttpError'
    this.status = status
    this.codigo = problem?.codigo
    this.errores = problem?.errores
  }
}

export function configureHttpClient(
  nextConfiguration: HttpClientConfiguration,
): void {
  // La aplicación centraliza aquí cómo obtiene la sesión y cómo responde cuando el acceso deja de ser válido.
  configuration = nextConfiguration
}

export async function httpRequest<T>(
  path: string,
  options: HttpRequestOptions = {},
): Promise<T> {
  // Las solicitudes protegidas incluyen el token actual para que la API aplique permisos y aislamiento por organización.
  const {
    body,
    authenticated = true,
    redirectOnUnauthorized = true,
    ...requestInit
  } = options
  const headers = new Headers(requestInit.headers)

  headers.set('Accept', 'application/json')

  if (body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }

  if (authenticated) {
    const token = configuration.getAccessToken()
    if (token) {
      headers.set('Authorization', `Bearer ${token}`)
    }
  }

  const response = await fetch(path, {
    ...requestInit,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  const payload = await readResponseBody(response)

  if (!response.ok) {
    // Los errores del contrato se conservan para mostrar mensajes comprensibles en cada pantalla.
    const problem = isApiProblem(payload) ? payload : undefined

    if (response.status === 401 && redirectOnUnauthorized) {
      // Si la sesión expiró, se redirige al login para evitar acciones con un token inválido.
      configuration.onUnauthorized()
    }

    throw new HttpError(
      problem?.detail ?? 'No fue posible completar la solicitud.',
      response.status,
      problem,
    )
  }

  return payload as T
}

async function readResponseBody(response: Response): Promise<unknown> {
  // La respuesta se adapta a su contenido para que cada pantalla pueda mostrar datos o mensajes de error del servidor.
  if (response.status === 204) {
    return undefined
  }

  const contentType = response.headers.get('content-type') ?? ''
  return contentType.includes('json')
    ? response.json().catch(() => null)
    : response.text()
}

function isApiProblem(value: unknown): value is ApiProblem {
  // Solo se tratan como errores de negocio las respuestas que respetan el formato esperado por la interfaz.
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as Partial<ApiProblem>
  return (candidate.detail === undefined || typeof candidate.detail === 'string')
    && (candidate.codigo === undefined || typeof candidate.codigo === 'string')
}

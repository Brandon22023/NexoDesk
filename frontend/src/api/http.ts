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
  configuration = nextConfiguration
}

export async function httpRequest<T>(
  path: string,
  options: HttpRequestOptions = {},
): Promise<T> {
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
    const problem = isApiProblem(payload) ? payload : undefined

    if (response.status === 401 && redirectOnUnauthorized) {
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
  if (response.status === 204) {
    return undefined
  }

  const contentType = response.headers.get('content-type') ?? ''
  return contentType.includes('json')
    ? response.json().catch(() => null)
    : response.text()
}

function isApiProblem(value: unknown): value is ApiProblem {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as Partial<ApiProblem>
  return (candidate.detail === undefined || typeof candidate.detail === 'string')
    && (candidate.codigo === undefined || typeof candidate.codigo === 'string')
}

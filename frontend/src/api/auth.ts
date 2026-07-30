import type {
  ApiProblem,
  LoginCredentials,
  LoginResponse,
  UsuarioSesion,
} from '../types/auth'

const LOGIN_ENDPOINT = '/api/v1/auth/login'
const ROLES: ReadonlySet<string> = new Set(['Admin', 'Agente', 'Solicitante'])

export class AuthApiError extends Error {
  readonly status: number
  readonly codigo?: string
  readonly errores?: Record<string, string[]>

  constructor(message: string, status: number, problem?: ApiProblem) {
    super(message)
    this.name = 'AuthApiError'
    this.status = status
    this.codigo = problem?.codigo
    this.errores = problem?.errores
  }
}

export async function login(
  credentials: LoginCredentials,
  signal?: AbortSignal,
): Promise<LoginResponse> {
  const response = await fetch(LOGIN_ENDPOINT, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(credentials),
    signal,
  })

  const payload: unknown = await response.json().catch(() => null)

  if (!response.ok) {
    const problem = isApiProblem(payload) ? payload : undefined
    throw new AuthApiError(
      problem?.detail ?? 'No pudimos verificar tus credenciales.',
      response.status,
      problem,
    )
  }

  if (!isLoginResponse(payload)) {
    throw new AuthApiError(
      'El servidor devolvió una respuesta de autenticación inválida.',
      response.status,
    )
  }

  return payload
}

function isApiProblem(value: unknown): value is ApiProblem {
  return isRecord(value)
    && (value.detail === undefined || typeof value.detail === 'string')
    && (value.codigo === undefined || typeof value.codigo === 'string')
}

function isLoginResponse(value: unknown): value is LoginResponse {
  if (!isRecord(value) || !isUsuarioSesion(value.usuario)) {
    return false
  }

  return typeof value.accessToken === 'string'
    && value.accessToken.length > 0
    && typeof value.expiraEn === 'number'
    && Number.isFinite(value.expiraEn)
    && value.expiraEn > 0
}

function isUsuarioSesion(value: unknown): value is UsuarioSesion {
  if (!isRecord(value)) {
    return false
  }

  return typeof value.id === 'string'
    && typeof value.nombre === 'string'
    && typeof value.email === 'string'
    && typeof value.rol === 'string'
    && ROLES.has(value.rol)
    && typeof value.tenantId === 'string'
    && typeof value.tenantNombre === 'string'
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

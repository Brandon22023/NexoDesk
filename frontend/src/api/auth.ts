import type {
  LoginCredentials,
  LoginResponse,
  UsuarioSesion,
} from '../types/auth'
import { HttpError, httpRequest } from './http'

const LOGIN_ENDPOINT = '/api/v1/auth/login'
const ROLES: ReadonlySet<string> = new Set(['Admin', 'Agente', 'Solicitante'])

export { HttpError as AuthApiError }

export async function login(
  credentials: LoginCredentials,
  signal?: AbortSignal,
): Promise<LoginResponse> {
  const payload = await httpRequest<LoginResponse>(LOGIN_ENDPOINT, {
    method: 'POST',
    body: credentials,
    signal,
    authenticated: false,
    redirectOnUnauthorized: false,
  })

  if (!isLoginResponse(payload)) {
    throw new HttpError(
      'El servidor devolvió una respuesta de autenticación inválida.',
      502,
    )
  }

  return payload
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

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
  
  // El inicio de sesión no usa un token previo y no debe redirigir si las credenciales son rechazadas.
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
  // Se valida la respuesta antes de guardar la sesión para no dejar a la persona con acceso incompleto.
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
  // La identidad debe incluir rol y organización para que la interfaz pueda aplicar sus reglas de acceso.
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
  // Esta comprobación evita interpretar respuestas incompletas como datos válidos de la aplicación.
  return typeof value === 'object' && value !== null
}

export type RolUsuario = 'Admin' | 'Agente' | 'Solicitante'

export interface LoginCredentials {
  email: string
  password: string
}

export interface UsuarioSesion {
  id: string
  nombre: string
  email: string
  rol: RolUsuario
  tenantId: string
  tenantNombre: string
}

export interface LoginResponse {
  accessToken: string
  expiraEn: number
  usuario: UsuarioSesion
}

export interface AuthSession {
  accessToken: string
  expiresAt: number
  usuario: UsuarioSesion
}

export interface ApiProblem {
  title?: string
  detail?: string
  status?: number
  codigo?: string
  errores?: Record<string, string[]>
}

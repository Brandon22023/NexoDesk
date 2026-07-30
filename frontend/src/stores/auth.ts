import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { login } from '../api/auth'
import type {
  AuthSession,
  LoginCredentials,
  LoginResponse,
  UsuarioSesion,
} from '../types/auth'

const SESSION_KEY = 'mesasitec.auth.session'

export const useAuthStore = defineStore('auth', () => {
  const session = ref<AuthSession | null>(readSession())
  let expirationTimer: number | null = null

  const accessToken = computed(() => session.value?.accessToken ?? null)
  const usuario = computed<UsuarioSesion | null>(
    () => session.value?.usuario ?? null,
  )
  const isAuthenticated = computed(
    () => session.value !== null && session.value.expiresAt > Date.now(),
  )

  async function signIn(
    credentials: LoginCredentials,
    signal?: AbortSignal,
  ): Promise<LoginResponse> {
    const response = await login(credentials, signal)
    const nextSession: AuthSession = {
      accessToken: response.accessToken,
      expiresAt: Date.now() + response.expiraEn * 1_000,
      usuario: response.usuario,
    }

    session.value = nextSession
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(nextSession))
    scheduleExpiration(nextSession.expiresAt)
    return response
  }

  function signOut(): void {
    session.value = null
    sessionStorage.removeItem(SESSION_KEY)
    if (expirationTimer !== null) {
      window.clearTimeout(expirationTimer)
      expirationTimer = null
    }
  }

  function scheduleExpiration(expiresAt: number): void {
    if (expirationTimer !== null) {
      window.clearTimeout(expirationTimer)
    }

    expirationTimer = window.setTimeout(
      signOut,
      Math.max(expiresAt - Date.now(), 0),
    )
  }

  if (session.value) {
    scheduleExpiration(session.value.expiresAt)
  }

  return {
    accessToken,
    usuario,
    isAuthenticated,
    signIn,
    signOut,
  }
})

function readSession(): AuthSession | null {
  const rawSession = sessionStorage.getItem(SESSION_KEY)

  if (!rawSession) {
    return null
  }

  try {
    const parsed: unknown = JSON.parse(rawSession)

    if (!isAuthSession(parsed) || parsed.expiresAt <= Date.now()) {
      sessionStorage.removeItem(SESSION_KEY)
      return null
    }

    return parsed
  } catch {
    sessionStorage.removeItem(SESSION_KEY)
    return null
  }
}

function isAuthSession(value: unknown): value is AuthSession {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as Partial<AuthSession>
  const user = candidate.usuario

  return typeof candidate.accessToken === 'string'
    && typeof candidate.expiresAt === 'number'
    && typeof user === 'object'
    && user !== null
    && typeof user.id === 'string'
    && typeof user.nombre === 'string'
    && typeof user.email === 'string'
    && typeof user.rol === 'string'
    && typeof user.tenantId === 'string'
    && typeof user.tenantNombre === 'string'
}

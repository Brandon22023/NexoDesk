import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { login } from '../api/auth'
import type {
  AuthSession,
  LoginCredentials,
  LoginResponse,
  UsuarioSesion,
} from '../types/auth'

const SESSION_KEY = 'nexodesk.auth.session'

export const useAuthStore = defineStore('auth', () => {
  // La sesión vive solo durante la pestaña actual para no conservar el acceso en equipos compartidos.
  const session = ref<AuthSession | null>(readSession())
  let expirationTimer: number | null = null

  // El token se expone de forma controlada para autorizar las llamadas protegidas de la aplicación.
  const accessToken = computed(() => session.value?.accessToken ?? null)
  // La identidad actual permite adaptar la interfaz al rol y a la organización de la persona.
  const usuario = computed<UsuarioSesion | null>(
    () => session.value?.usuario ?? null,
  )
  // El acceso solo es válido mientras la sesión exista y no haya vencido.
  const isAuthenticated = computed(
    () => session.value !== null && session.value.expiresAt > Date.now(),
  )

  // Al ingresar se conserva la identidad y el vencimiento para aplicar permisos sin volver a autenticar en cada pantalla.
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
    // Cerrar sesión elimina los datos locales para que nadie reutilice el acceso anterior.
    session.value = null
    sessionStorage.removeItem(SESSION_KEY)
    if (expirationTimer !== null) {
      window.clearTimeout(expirationTimer)
      expirationTimer = null
    }
  }

  function scheduleExpiration(expiresAt: number): void {
    // La sesión se cierra al vencer aunque la persona permanezca en la misma pantalla.
    if (expirationTimer !== null) {
      window.clearTimeout(expirationTimer)
    }

    expirationTimer = window.setTimeout(
      signOut,
      Math.max(expiresAt - Date.now(), 0),
    )
  }

  if (session.value) {
    // Una sesión recuperada también respeta su hora de vencimiento original.
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
  // Solo se recuperan sesiones completas y vigentes para evitar mostrar datos de una identidad inválida.
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
  // La sesión guardada debe tener toda la identidad necesaria antes de restaurar el acceso de la persona.
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

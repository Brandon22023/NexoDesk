<script setup lang="ts">
import { onBeforeUnmount, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import LoginBackground from '../components/auth/LoginBackground.vue'
import LoginCard from '../components/auth/LoginCard.vue'
import { AuthApiError } from '../api/auth'
import { useAuthStore } from '../stores/auth'
import type { LoginCredentials } from '../types/auth'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()

const isSubmitting = ref(false)
const serverError = ref('')
const successMessage = ref('')
let activeRequest: AbortController | null = null

async function handleLogin(credentials: LoginCredentials): Promise<void> {
  activeRequest?.abort()
  activeRequest = new AbortController()
  isSubmitting.value = true
  serverError.value = ''
  successMessage.value = ''

  try {
    await authStore.signIn(credentials, activeRequest.signal)
    successMessage.value = 'Sesión iniciada correctamente.'

    const redirect = getSafeRedirect(route.query.redirect)
    await router.replace(redirect ?? { name: 'solicitudes' })
  } catch (error: unknown) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      return
    }

    serverError.value = error instanceof AuthApiError
      ? error.message
      : 'No fue posible conectar con el servicio. Inténtalo nuevamente.'
  } finally {
    isSubmitting.value = false
  }
}

function clearFeedback(): void {
  serverError.value = ''
  successMessage.value = ''
}

function getSafeRedirect(value: unknown): string | null {
  return typeof value === 'string'
    && value.startsWith('/')
    && !value.startsWith('//')
    && value !== '/login'
    ? value
    : null
}

onBeforeUnmount(() => {
  activeRequest?.abort()
})
</script>

<template>
  <main class="login-page">
    <LoginBackground />

    <section class="login-form-panel" aria-label="Inicio de sesión">
      <LoginCard
        :is-submitting="isSubmitting"
        :server-error="serverError"
        :success-message="successMessage"
        @submit="handleLogin"
        @clear-feedback="clearFeedback"
      />
    </section>
  </main>
</template>

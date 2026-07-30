<script setup lang="ts">
import { ShieldCheck } from '@lucide/vue'
import LoginForm from './LoginForm.vue'
import type { LoginCredentials } from '../../types/auth'

const props = withDefaults(defineProps<{
  isSubmitting?: boolean
  serverError?: string
  successMessage?: string
}>(), {
  isSubmitting: false,
  serverError: '',
  successMessage: '',
})

const emit = defineEmits<{
  submit: [credentials: LoginCredentials]
  clearFeedback: []
}>()
</script>

<template>
  <article class="login-card" aria-labelledby="login-title">
    <header class="login-card__heading">
      <p class="login-eyebrow">ACCESO AL SERVICE DESK</p>
      <h2 id="login-title">Ingresa a tu espacio</h2>
      <p>Gestiona solicitudes y da seguimiento a tu operación.</p>
    </header>

    <LoginForm
      :is-submitting="props.isSubmitting"
      :server-error="props.serverError"
      :success-message="props.successMessage"
      @submit="emit('submit', $event)"
      @clear-feedback="emit('clearFeedback')"
    />

    <p class="login-card__security">
      <ShieldCheck :size="15" :stroke-width="1.8" aria-hidden="true" />
      Sesión protegida mediante autenticación segura.
    </p>
  </article>
</template>

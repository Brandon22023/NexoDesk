<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import {
  ArrowRight,
  CircleAlert,
  CircleCheck,
  Eye,
  EyeOff,
  Info,
  LoaderCircle,
  LockKeyhole,
  Mail,
} from '@lucide/vue'
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

const email = ref('')
const password = ref('')
const showPassword = ref(false)
const hasSubmitted = ref(false)
const recoveryHint = ref('')

const emailIsValid = computed(() =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value),
)

const emailError = computed(() =>
  hasSubmitted.value && !emailIsValid.value
    ? 'Ingresa un correo electrónico válido.'
    : '',
)

const passwordError = computed(() =>
  hasSubmitted.value && password.value.length === 0
    ? 'Ingresa tu contraseña para continuar.'
    : '',
)

watch([email, password], () => {
  recoveryHint.value = ''
  emit('clearFeedback')
})

function handleSubmit(): void {
  hasSubmitted.value = true
  recoveryHint.value = ''

  if (!emailIsValid.value || password.value.length === 0) {
    return
  }

  emit('submit', {
    email: email.value.trim(),
    password: password.value,
  })
}

function showRecoveryHint(): void {
  recoveryHint.value =
    'Solicita el restablecimiento de acceso al administrador de tu organización.'
}
</script>

<template>
  <form
    class="login-form"
    novalidate
    :aria-busy="props.isSubmitting"
    @submit.prevent="handleSubmit"
  >
    <div class="login-form__field login-form__field--email">
      <label for="login-email">Correo electrónico</label>
      <div class="login-input" :class="{ 'login-input--error': emailError }">
        <span class="login-input__icon" aria-hidden="true">
          <Mail :size="18" :stroke-width="1.8" />
        </span>
        <input
          id="login-email"
          v-model.trim="email"
          data-testid="login-email"
          type="email"
          inputmode="email"
          autocomplete="email"
          spellcheck="false"
          placeholder="nombre@empresa.com"
          :disabled="props.isSubmitting"
          :aria-invalid="Boolean(emailError)"
          :aria-describedby="emailError ? 'login-email-error' : undefined"
        />
      </div>
      <p
        v-if="emailError"
        id="login-email-error"
        class="login-form__field-error"
      >
        {{ emailError }}
      </p>
    </div>

    <div class="login-form__field login-form__field--password">
      <div class="login-form__label-row">
        <label for="login-password">Contraseña</label>
        <button type="button" @click="showRecoveryHint">
          ¿Olvidaste tu contraseña?
        </button>
      </div>
      <div class="login-input" :class="{ 'login-input--error': passwordError }">
        <span class="login-input__icon" aria-hidden="true">
          <LockKeyhole :size="18" :stroke-width="1.8" />
        </span>
        <input
          id="login-password"
          v-model="password"
          data-testid="login-password"
          :type="showPassword ? 'text' : 'password'"
          autocomplete="current-password"
          placeholder="Ingresa tu contraseña"
          :disabled="props.isSubmitting"
          :aria-invalid="Boolean(passwordError)"
          :aria-describedby="passwordError ? 'login-password-error' : undefined"
        />
        <button
          class="login-input__visibility"
          type="button"
          :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
          :disabled="props.isSubmitting"
          @click="showPassword = !showPassword"
        >
          <EyeOff
            v-if="showPassword"
            :size="18"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <Eye
            v-else
            :size="18"
            :stroke-width="1.8"
            aria-hidden="true"
          />
        </button>
      </div>
      <p
        v-if="passwordError"
        id="login-password-error"
        class="login-form__field-error"
      >
        {{ passwordError }}
      </p>
    </div>

    <p
      v-if="props.serverError"
      data-testid="login-error"
      class="login-form__message login-form__message--error"
      role="alert"
    >
      <CircleAlert :size="17" :stroke-width="1.8" aria-hidden="true" />
      <span>{{ props.serverError }}</span>
    </p>
    <p
      v-else-if="props.successMessage"
      class="login-form__message login-form__message--success"
      role="status"
    >
      <CircleCheck :size="17" :stroke-width="1.8" aria-hidden="true" />
      <span>{{ props.successMessage }}</span>
    </p>
    <p
      v-else-if="recoveryHint"
      class="login-form__message login-form__message--info"
      role="status"
    >
      <Info :size="17" :stroke-width="1.8" aria-hidden="true" />
      <span>{{ recoveryHint }}</span>
    </p>

    <button
      data-testid="login-submit"
      class="login-form__submit"
      type="submit"
      :disabled="props.isSubmitting"
    >
      <span>
        {{ props.isSubmitting ? 'Validando acceso…' : 'Ingresar a MesaSitec' }}
      </span>
      <LoaderCircle
        v-if="props.isSubmitting"
        class="login-form__spinner"
        :size="20"
        :stroke-width="1.8"
        aria-hidden="true"
      />
      <ArrowRight
        v-else
        class="login-form__submit-icon"
        :size="20"
        :stroke-width="1.8"
        aria-hidden="true"
      />
    </button>
  </form>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { Workflow, X } from '@lucide/vue'
import type { UsuarioSesion } from '../../types/auth'
import type {
  AccionSolicitud,
  TransicionSolicitudRequest,
} from '../../types/solicitudes'

const props = defineProps<{
  accion: AccionSolicitud
  usuario: UsuarioSesion
  isSubmitting: boolean
  serverError: string
}>()

const emit = defineEmits<{
  confirm: [request: TransicionSolicitudRequest]
  close: []
}>()

const agenteId = ref(props.usuario.id)
const motivo = ref('')
const attempted = ref(false)
const requiresAgent = computed(() => props.accion === 'asignar')
const requiresReason = computed(
  () => props.accion === 'resolver' || props.accion === 'cancelar',
)
const minimumReasonLength = computed(() => props.accion === 'resolver' ? 20 : 10)
const title = computed(() => {
  const labels: Record<AccionSolicitud, string> = {
    asignar: 'Asignar solicitud',
    iniciar: 'Iniciar atención',
    resolver: 'Resolver solicitud',
    cerrar: 'Cerrar solicitud',
    reabrir: 'Reabrir solicitud',
    cancelar: 'Cancelar solicitud',
  }
  return labels[props.accion]
})
const validationError = computed(() => {
  if (!attempted.value) return ''
  if (requiresAgent.value && !agenteId.value) return 'Selecciona un agente.'
  if (
    requiresReason.value
    && motivo.value.trim().length < minimumReasonLength.value
  ) {
    return `El motivo debe tener al menos ${minimumReasonLength.value} caracteres.`
  }
  return ''
})

function confirm(): void {
  attempted.value = true
  if (validationError.value) return

  emit('confirm', {
    accion: props.accion,
    ...(requiresAgent.value ? { agenteId: agenteId.value } : {}),
    ...(requiresReason.value ? { motivo: motivo.value.trim() } : {}),
  })
}

function handleKeydown(event: KeyboardEvent): void {
  if (event.key === 'Escape' && !props.isSubmitting) emit('close')
}

onMounted(() => document.addEventListener('keydown', handleKeydown))
onBeforeUnmount(() => document.removeEventListener('keydown', handleKeydown))
</script>

<template>
  <div
    data-testid="modal-accion"
    class="modal-backdrop"
    role="presentation"
    @click.self="!props.isSubmitting && emit('close')"
  >
    <section
      class="action-modal surface"
      role="dialog"
      aria-modal="true"
      aria-labelledby="action-modal-title"
    >
      <header>
        <span aria-hidden="true">
          <Workflow :size="20" :stroke-width="1.8" />
        </span>
        <div>
          <p class="page-eyebrow">CAMBIO DE ESTADO</p>
          <h2 id="action-modal-title">{{ title }}</h2>
        </div>
        <button
          class="icon-button"
          type="button"
          aria-label="Cerrar"
          :disabled="props.isSubmitting"
          @click="emit('close')"
        >
          <X :size="18" :stroke-width="1.8" aria-hidden="true" />
        </button>
      </header>

      <label v-if="requiresAgent" class="request-form__field">
        <span>Agente responsable</span>
        <select
          v-model="agenteId"
          data-testid="modal-select-agente"
          class="form-control"
        >
          <option :value="props.usuario.id">
            {{ props.usuario.nombre }} · {{ props.usuario.rol }}
          </option>
        </select>
        <small>
          El contrato actual permite asignarte la solicitud. El directorio de
          agentes se agregará cuando exista su endpoint.
        </small>
      </label>

      <label v-if="requiresReason" class="request-form__field">
        <span>Motivo</span>
        <textarea
          v-model="motivo"
          data-testid="modal-motivo"
          class="form-control"
          rows="5"
          :placeholder="`Explica el motivo en al menos ${minimumReasonLength} caracteres`"
        ></textarea>
      </label>

      <p
        v-if="validationError || props.serverError"
        data-testid="modal-error"
        class="request-form__server-error"
        role="alert"
      >
        {{ validationError || props.serverError }}
      </p>

      <footer>
        <button
          data-testid="modal-cancelar"
          class="button button--secondary"
          type="button"
          :disabled="props.isSubmitting"
          @click="emit('close')"
        >
          Volver
        </button>
        <button
          data-testid="modal-confirmar"
          class="button button--primary"
          type="button"
          :disabled="props.isSubmitting"
          @click="confirm"
        >
          {{ props.isSubmitting ? 'Procesando…' : 'Confirmar acción' }}
        </button>
      </footer>
    </section>
  </div>
</template>

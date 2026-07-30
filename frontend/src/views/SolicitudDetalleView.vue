<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ArrowLeft, Clock3, Pencil, UserRound } from '@lucide/vue'
import { useRoute } from 'vue-router'
import AppState from '../components/ui/AppState.vue'
import SolicitudActionModal from '../components/solicitudes/SolicitudActionModal.vue'
import { HttpError } from '../api/http'
import { obtenerSolicitud, transicionarSolicitud } from '../api/solicitudes'
import { useAuthStore } from '../stores/auth'
import { useUiStore } from '../stores/ui'
import type {
  AccionSolicitud,
  EstadoSolicitud,
  SolicitudDetalleDto,
  TransicionSolicitudRequest,
} from '../types/solicitudes'

const STATE_ACTIONS: Record<EstadoSolicitud, AccionSolicitud[]> = {
  Nueva: ['asignar', 'cancelar'],
  Asignada: ['iniciar', 'asignar', 'cancelar'],
  EnProceso: ['resolver', 'asignar', 'cancelar'],
  Resuelta: ['cerrar', 'reabrir'],
  Cerrada: [],
  Cancelada: [],
}

const route = useRoute()
const authStore = useAuthStore()
const uiStore = useUiStore()
const solicitud = ref<SolicitudDetalleDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')
const selectedAction = ref<AccionSolicitud | null>(null)
const actionSubmitting = ref(false)
const actionError = ref('')
const solicitudId = computed(() => String(route.params.id))
const usuario = computed(() => authStore.usuario)

const canEdit = computed(() => {
  if (!solicitud.value || !usuario.value) return false
  if (usuario.value.rol === 'Admin' || usuario.value.rol === 'Agente') return true
  return solicitud.value.solicitante.id === usuario.value.id
    && solicitud.value.estado === 'Nueva'
})

const availableActions = computed(() => {
  if (!solicitud.value || !usuario.value) return []
  return STATE_ACTIONS[solicitud.value.estado].filter((action) => {
    if (usuario.value?.rol === 'Admin') return true
    if (usuario.value?.rol === 'Agente') return action !== 'cancelar'
    return action === 'cerrar'
      && solicitud.value?.solicitante.id === usuario.value?.id
  })
})

const dateFormatter = new Intl.DateTimeFormat('es-GT', {
  dateStyle: 'long',
  timeStyle: 'short',
})

function formatDate(value: string | null): string {
  return value ? dateFormatter.format(new Date(value)) : 'No registrada'
}

function actionLabel(action: AccionSolicitud): string {
  const labels: Record<AccionSolicitud, string> = {
    asignar: 'Asignar',
    iniciar: 'Iniciar',
    resolver: 'Resolver',
    cerrar: 'Cerrar',
    reabrir: 'Reabrir',
    cancelar: 'Cancelar',
  }
  return labels[action]
}

async function load(): Promise<void> {
  loading.value = true
  errorMessage.value = ''
  try {
    solicitud.value = await obtenerSolicitud(solicitudId.value)
  } catch (error: unknown) {
    errorMessage.value = error instanceof HttpError
      ? error.message
      : 'No fue posible cargar la solicitud.'
  } finally {
    loading.value = false
  }
}

function openAction(action: AccionSolicitud): void {
  selectedAction.value = action
  actionError.value = ''
}

async function confirmAction(request: TransicionSolicitudRequest): Promise<void> {
  actionSubmitting.value = true
  actionError.value = ''
  try {
    solicitud.value = await transicionarSolicitud(solicitudId.value, request)
    selectedAction.value = null
    uiStore.showToast(`Acción "${actionLabel(request.accion)}" aplicada correctamente.`)
  } catch (error: unknown) {
    actionError.value = error instanceof HttpError
      ? error.message
      : 'No fue posible aplicar la acción.'
  } finally {
    actionSubmitting.value = false
  }
}

onMounted(() => void load())
</script>

<template>
  <section class="page detail-page">
    <RouterLink class="back-link" to="/solicitudes">
      <ArrowLeft :size="16" :stroke-width="1.8" aria-hidden="true" />
      Volver al listado
    </RouterLink>

    <AppState
      v-if="loading"
      state="loading"
      title="Cargando solicitud"
      message="Estamos consultando el detalle y su estado actual."
    />
    <AppState
      v-else-if="errorMessage"
      state="error"
      title="No pudimos cargar la solicitud"
      :message="errorMessage"
      @retry="load"
    />
    <AppState
      v-else-if="!solicitud"
      state="empty"
      title="Solicitud no disponible"
    />

    <template v-else>
      <header class="detail-header surface">
        <div>
          <p data-testid="detalle-codigo" class="detail-code">
            {{ solicitud.codigo }}
          </p>
          <h1 data-testid="detalle-titulo">{{ solicitud.titulo }}</h1>
          <div class="detail-header__badges">
            <span data-testid="detalle-estado" class="status-badge">
              {{ solicitud.estado === 'EnProceso' ? 'En proceso' : solicitud.estado }}
            </span>
            <span data-testid="detalle-prioridad" class="priority-badge">
              {{ solicitud.prioridad === 'Critica' ? 'Crítica' : solicitud.prioridad }}
            </span>
            <span
              v-if="solicitud.vencida"
              data-testid="detalle-vencida"
              class="overdue-badge"
            >
              SLA vencido
            </span>
          </div>
        </div>

        <div class="detail-header__actions">
          <RouterLink
            v-if="canEdit"
            data-testid="btn-editar"
            class="button button--secondary"
            :to="{ name: 'solicitud-editar', params: { id: solicitud.id } }"
          >
            <Pencil :size="16" :stroke-width="1.8" aria-hidden="true" />
            Editar
          </RouterLink>
          <button
            v-for="action in availableActions"
            :key="action"
            :data-testid="`btn-accion-${action}`"
            class="button"
            :class="action === 'cancelar' ? 'button--danger' : 'button--primary'"
            type="button"
            @click="openAction(action)"
          >
            {{ actionLabel(action) }}
          </button>
        </div>
      </header>

      <div class="detail-grid">
        <article class="detail-card surface detail-card--description">
          <p class="page-eyebrow">DESCRIPCIÓN</p>
          <p data-testid="detalle-descripcion">{{ solicitud.descripcion }}</p>
        </article>

        <article class="detail-card surface">
          <p class="page-eyebrow">ORGANIZACIÓN</p>
          <dl>
            <div>
              <dt>Categoría</dt>
              <dd data-testid="detalle-categoria">{{ solicitud.categoria.nombre }}</dd>
            </div>
            <div>
              <dt>Solicitante</dt>
              <dd>{{ solicitud.solicitante.nombre }}</dd>
            </div>
            <div>
              <dt>Agente</dt>
              <dd data-testid="detalle-agente">
                <UserRound :size="15" :stroke-width="1.8" aria-hidden="true" />
                {{ solicitud.agente?.nombre ?? 'Sin asignar' }}
              </dd>
            </div>
          </dl>
        </article>

        <article class="detail-card surface">
          <p class="page-eyebrow">SEGUIMIENTO</p>
          <dl>
            <div>
              <dt>Creada</dt>
              <dd data-testid="detalle-fecha-creacion">
                {{ formatDate(solicitud.fechaCreacion) }}
              </dd>
            </div>
            <div>
              <dt>Límite SLA</dt>
              <dd data-testid="detalle-fecha-limite">
                <Clock3 :size="15" :stroke-width="1.8" aria-hidden="true" />
                {{ formatDate(solicitud.fechaLimiteSla) }}
              </dd>
            </div>
            <div v-if="solicitud.fechaResolucion">
              <dt>Resolución</dt>
              <dd>{{ formatDate(solicitud.fechaResolucion) }}</dd>
            </div>
          </dl>
        </article>

        <article
          v-if="solicitud.motivoResolucion || solicitud.motivoCancelacion"
          class="detail-card surface detail-card--wide"
        >
          <p class="page-eyebrow">MOTIVO REGISTRADO</p>
          <p data-testid="detalle-motivo">
            {{ solicitud.motivoResolucion ?? solicitud.motivoCancelacion }}
          </p>
        </article>
      </div>

      <SolicitudActionModal
        v-if="selectedAction && usuario"
        :accion="selectedAction"
        :usuario="usuario"
        :is-submitting="actionSubmitting"
        :server-error="actionError"
        @confirm="confirmAction"
        @close="selectedAction = null"
      />
    </template>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import SolicitudForm from '../components/solicitudes/SolicitudForm.vue'
import AppState from '../components/ui/AppState.vue'
import { listarCategorias } from '../api/categorias'
import { editarSolicitud, obtenerSolicitud } from '../api/solicitudes'
import { HttpError } from '../api/http'
import { useUiStore } from '../stores/ui'
import type { CategoriaDto } from '../types/categorias'
import type { SolicitudDetalleDto, SolicitudFormData } from '../types/solicitudes'

const route = useRoute()
const router = useRouter()
const uiStore = useUiStore()
const categorias = ref<CategoriaDto[]>([])
const solicitud = ref<SolicitudDetalleDto | null>(null)
const loading = ref(true)
const submitting = ref(false)
const errorMessage = ref('')
const serverError = ref('')
const solicitudId = computed(() => String(route.params.id))
const initialValue = computed<SolicitudFormData | null>(() => solicitud.value
  ? {
      titulo: solicitud.value.titulo,
      descripcion: solicitud.value.descripcion,
      categoriaId: solicitud.value.categoria.id,
      prioridad: solicitud.value.prioridad,
    }
  : null)

async function load(): Promise<void> {
  loading.value = true
  errorMessage.value = ''
  try {
    const [detail, categories] = await Promise.all([
      obtenerSolicitud(solicitudId.value),
      listarCategorias(),
    ])
    solicitud.value = detail
    categorias.value = categories
  } catch (error: unknown) {
    errorMessage.value = error instanceof HttpError
      ? error.message
      : 'No fue posible cargar la solicitud.'
  } finally {
    loading.value = false
  }
}

async function update(data: SolicitudFormData): Promise<void> {
  submitting.value = true
  serverError.value = ''
  try {
    const updated = await editarSolicitud(solicitudId.value, data)
    uiStore.showToast(`Solicitud ${updated.codigo} actualizada correctamente.`)
    await router.push({ name: 'solicitud-detalle', params: { id: updated.id } })
  } catch (error: unknown) {
    serverError.value = error instanceof HttpError
      ? error.message
      : 'No fue posible actualizar la solicitud.'
  } finally {
    submitting.value = false
  }
}

onMounted(() => void load())
</script>

<template>
  <section class="page">
    <header class="page-header">
      <div class="page-header__copy">
        <p class="page-eyebrow">EDICIÓN</p>
        <h1 class="page-title">Editar solicitud</h1>
        <p class="page-description">
          Los cambios de prioridad o categoría recalculan automáticamente el SLA.
        </p>
      </div>
    </header>

    <AppState
      v-if="loading"
      state="loading"
      title="Cargando solicitud"
      message="Estamos preparando la información para editar."
    />
    <AppState
      v-else-if="errorMessage"
      state="error"
      title="No pudimos cargar la solicitud"
      :message="errorMessage"
      @retry="load"
    />
    <AppState
      v-else-if="categorias.length === 0"
      state="empty"
      title="No hay categorías disponibles"
      message="No es posible editar hasta que exista una categoría activa."
    />
    <SolicitudForm
      v-else-if="initialValue"
      :key="solicitudId"
      :categorias="categorias"
      :initial-value="initialValue"
      :is-submitting="submitting"
      :server-error="serverError"
      submit-label="Guardar cambios"
      @submit="update"
      @cancel="router.push({ name: 'solicitud-detalle', params: { id: solicitudId } })"
    />
  </section>
</template>

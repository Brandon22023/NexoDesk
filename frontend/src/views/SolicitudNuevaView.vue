<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import SolicitudForm from '../components/solicitudes/SolicitudForm.vue'
import AppState from '../components/ui/AppState.vue'
import { listarCategorias } from '../api/categorias'
import { crearSolicitud } from '../api/solicitudes'
import { HttpError } from '../api/http'
import { useUiStore } from '../stores/ui'
import type { CategoriaDto } from '../types/categorias'
import type { SolicitudFormData } from '../types/solicitudes'

const router = useRouter()
const uiStore = useUiStore()
const categorias = ref<CategoriaDto[]>([])
const loading = ref(true)
const submitting = ref(false)
const errorMessage = ref('')
const serverError = ref('')

async function loadCategorias(): Promise<void> {
  // El formulario espera las categorías activas para que la nueva solicitud tenga un SLA válido desde el inicio.
  loading.value = true
  errorMessage.value = ''

  try {
    categorias.value = await listarCategorias()
  } catch (error: unknown) {
    errorMessage.value = error instanceof HttpError
      ? error.message
      : 'No fue posible cargar las categorías.'
  } finally {
    loading.value = false
  }
}

async function create(data: SolicitudFormData): Promise<void> {
  // Tras crear la solicitud se abre su detalle para que la persona pueda seguir su código y estado.
  submitting.value = true
  serverError.value = ''

  try {
    const solicitud = await crearSolicitud(data)
    uiStore.showToast(`Solicitud ${solicitud.codigo} creada correctamente.`)
    await router.push({
      name: 'solicitud-detalle',
      params: { id: solicitud.id },
    })
  } catch (error: unknown) {
    serverError.value = error instanceof HttpError
      ? error.message
      : 'No fue posible crear la solicitud.'
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  // Las categorías se consultan al abrir la pantalla para respetar las opciones activas de la organización.
  void loadCategorias()
})
</script>

<template>
  <section class="page">
    <header class="page-header">
      <div class="page-header__copy">
        <p class="page-eyebrow">NUEVO REGISTRO</p>
        <h1 class="page-title">Nueva solicitud</h1>
        <p class="page-description">
          Describe la necesidad y el sistema calculará automáticamente el SLA.
        </p>
      </div>
    </header>

    <AppState
      v-if="loading"
      state="loading"
      title="Cargando formulario"
      message="Estamos preparando las categorías disponibles."
    />
    <AppState
      v-else-if="errorMessage"
      state="error"
      title="No pudimos preparar el formulario"
      :message="errorMessage"
      @retry="loadCategorias"
    />
    <AppState
      v-else-if="categorias.length === 0"
      state="empty"
      title="No hay categorías disponibles"
      message="Solicita a un administrador que habilite al menos una categoría."
    />
    <SolicitudForm
      v-else
      :categorias="categorias"
      :is-submitting="submitting"
      :server-error="serverError"
      submit-label="Crear solicitud"
      @submit="create"
      @cancel="router.push('/solicitudes')"
    />
  </section>
</template>

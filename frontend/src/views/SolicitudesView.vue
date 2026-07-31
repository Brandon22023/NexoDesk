<script setup lang="ts">
import { onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { Plus } from '@lucide/vue'
import AppState from '../components/ui/AppState.vue'
import SolicitudFilters from '../components/solicitudes/SolicitudFilters.vue'
import SolicitudesTable from '../components/solicitudes/SolicitudesTable.vue'
import { listarCategorias } from '../api/categorias'
import { HttpError } from '../api/http'
import { listarSolicitudes } from '../api/solicitudes'
import type { CategoriaDto } from '../types/categorias'
import type {
  FiltrosSolicitudes,
  PaginaSolicitudesDto,
} from '../types/solicitudes'

const DEFAULT_FILTERS: FiltrosSolicitudes = {
  estado: '',
  prioridad: '',
  categoriaId: '',
  agenteId: '',
  q: '',
  vencidas: null,
  page: 1,
  pageSize: 20,
  sort: '-fechaCreacion',
}

const filters = reactive<FiltrosSolicitudes>({ ...DEFAULT_FILTERS })
const page = ref<PaginaSolicitudesDto | null>(null)
const categorias = ref<CategoriaDto[]>([])
const loading = ref(true)
const errorMessage = ref('')
const categoryWarning = ref('')
let listController: AbortController | null = null
let searchTimer: number | null = null

async function loadSolicitudes(): Promise<void> {
  // Se descarta la consulta anterior para que una búsqueda reciente no muestre resultados desactualizados.
  listController?.abort()
  listController = new AbortController()
  loading.value = true
  errorMessage.value = ''

  try {
    page.value = await listarSolicitudes(filters, listController.signal)
  } catch (error: unknown) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      return
    }

    errorMessage.value = error instanceof HttpError
      ? error.message
      : 'No fue posible cargar las solicitudes.'
  } finally {
    loading.value = false
  }
}

async function loadCategorias(): Promise<void> {
  // Si las categorías no cargan, el listado sigue disponible para no impedir el seguimiento de solicitudes.
  categoryWarning.value = ''

  try {
    categorias.value = await listarCategorias()
  } catch {
    categoryWarning.value =
      'Las categorías no están disponibles temporalmente.'
  }
}

function applyFilters(nextFilters: FiltrosSolicitudes): void {
  // Todo cambio de criterio vuelve a la primera página para no mostrar una página que ya no existe en el resultado filtrado.
  const searchChanged = nextFilters.q !== filters.q
  Object.assign(filters, nextFilters, { page: 1 })

  if (searchChanged) {
    if (searchTimer !== null) {
      window.clearTimeout(searchTimer)
    }

    // La búsqueda espera una pausa breve para no consultar por cada letra escrita.
    searchTimer = window.setTimeout(() => {
      void loadSolicitudes()
      searchTimer = null
    }, 350)
    return
  }

  void loadSolicitudes()
}

function clearFilters(): void {
  // Limpiar restaura la vista inicial del listado, incluida la primera página.
  Object.assign(filters, DEFAULT_FILTERS)
  void loadSolicitudes()
}

function changePage(nextPage: number): void {
  // Los filtros se conservan al cambiar de página para no perder el contexto de búsqueda.
  filters.page = nextPage
  void loadSolicitudes()
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => {
  // El listado y sus opciones de filtro se preparan al entrar para mostrar la información actual de la organización.
  void Promise.all([loadSolicitudes(), loadCategorias()])
})

onBeforeUnmount(() => {
  // Al abandonar el listado se cancelan tareas pendientes para no actualizar una pantalla que ya no está activa.
  listController?.abort()
  if (searchTimer !== null) {
    window.clearTimeout(searchTimer)
  }
})
</script>

<template>
  <section class="page requests-page">
    <header class="page-header">
      <div class="page-header__copy">
        <p class="page-eyebrow">GESTIÓN DE SERVICIOS</p>
        <h1 class="page-title">Solicitudes</h1>
        <p class="page-description">
          Consulta, filtra y da seguimiento al trabajo de tu organización.
        </p>
      </div>
      <RouterLink
        data-testid="btn-nueva-solicitud"
        class="button button--primary"
        to="/solicitudes/nueva"
      >
        <Plus :size="18" :stroke-width="1.8" aria-hidden="true" />
        Nueva solicitud
      </RouterLink>
    </header>

    <SolicitudFilters
      :model-value="filters"
      :categorias="categorias"
      @update:model-value="applyFilters"
      @clear="clearFilters"
    />

    <p v-if="categoryWarning" class="inline-warning">{{ categoryWarning }}</p>

    <AppState
      v-if="loading"
      state="loading"
      data-testid="listado-cargando"
      title="Cargando solicitudes"
      message="Estamos consultando la información más reciente."
    />
    <AppState
      v-else-if="errorMessage"
      state="error"
      title="No pudimos cargar el listado"
      :message="errorMessage"
      @retry="loadSolicitudes"
    />
    <AppState
      v-else-if="!page || page.items.length === 0"
      state="empty"
      data-testid="listado-vacio"
      title="No hay solicitudes para mostrar"
      message="Ajusta los filtros o registra una nueva solicitud."
    />
    <template v-else>
      <SolicitudesTable :solicitudes="page.items" />

      <nav class="pagination" aria-label="Paginación de solicitudes">
        <button
          data-testid="paginacion-anterior"
          class="button button--secondary"
          type="button"
          :disabled="page.page <= 1"
          @click="changePage(page.page - 1)"
        >
          Anterior
        </button>
        <p data-testid="paginacion-info">
          Página {{ page.page }} de {{ Math.max(page.totalPaginas, 1) }} — {{ page.total }} resultados
        </p>
        <button
          data-testid="paginacion-siguiente"
          class="button button--secondary"
          type="button"
          :disabled="page.page >= page.totalPaginas"
          @click="changePage(page.page + 1)"
        >
          Siguiente
        </button>
      </nav>
    </template>
  </section>
</template>

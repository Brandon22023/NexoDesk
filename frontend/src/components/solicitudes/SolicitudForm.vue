<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ArrowLeft, Save } from '@lucide/vue'
import type { CategoriaDto } from '../../types/categorias'
import type {
  PrioridadSolicitud,
  SolicitudFormData,
} from '../../types/solicitudes'

const props = withDefaults(defineProps<{
  categorias: CategoriaDto[]
  initialValue?: SolicitudFormData
  isSubmitting?: boolean
  serverError?: string
  submitLabel?: string
}>(), {
  initialValue: () => ({
    titulo: '',
    descripcion: '',
    categoriaId: '',
    prioridad: 'Media',
  }),
  isSubmitting: false,
  serverError: '',
  submitLabel: 'Guardar solicitud',
})

const emit = defineEmits<{
  submit: [value: SolicitudFormData]
  cancel: []
}>()

const form = reactive<SolicitudFormData>({ ...props.initialValue })
const attempted = ref(false)

const tituloError = computed(() => {
  if (!attempted.value) return ''
  const length = form.titulo.trim().length
  if (length < 5) return 'El título debe tener al menos 5 caracteres.'
  if (length > 120) return 'El título no puede superar 120 caracteres.'
  return ''
})

const descripcionError = computed(() => {
  if (!attempted.value) return ''
  const length = form.descripcion.trim().length
  if (length < 10) return 'La descripción debe tener al menos 10 caracteres.'
  if (length > 4_000) return 'La descripción no puede superar 4000 caracteres.'
  return ''
})

const categoriaError = computed(() =>
  attempted.value && !form.categoriaId
    ? 'Selecciona una categoría.'
    : '',
)

function submit(): void {
  attempted.value = true

  if (tituloError.value || descripcionError.value || categoriaError.value) {
    return
  }

  emit('submit', {
    titulo: form.titulo.trim(),
    descripcion: form.descripcion.trim(),
    categoriaId: form.categoriaId,
    prioridad: form.prioridad,
  })
}

function setPrioridad(event: Event): void {
  form.prioridad = (event.target as HTMLSelectElement)
    .value as PrioridadSolicitud
}
</script>

<template>
  <form class="request-form surface" novalidate @submit.prevent="submit">
    <fieldset :disabled="props.isSubmitting">
      <label class="request-form__field">
        <span>Título</span>
        <input
          v-model="form.titulo"
          data-testid="form-titulo"
          class="form-control"
          type="text"
          maxlength="120"
          autocomplete="off"
          placeholder="Resume brevemente la solicitud"
          :aria-invalid="Boolean(tituloError)"
        />
        <small>{{ form.titulo.length }}/120</small>
        <em v-if="tituloError" data-testid="error-titulo">
          {{ tituloError }}
        </em>
      </label>

      <label class="request-form__field request-form__field--wide">
        <span>Descripción</span>
        <textarea
          v-model="form.descripcion"
          data-testid="form-descripcion"
          class="form-control"
          rows="7"
          maxlength="4000"
          placeholder="Describe el problema, su impacto y cualquier detalle útil"
          :aria-invalid="Boolean(descripcionError)"
        ></textarea>
        <small>{{ form.descripcion.length }}/4000</small>
        <em v-if="descripcionError" data-testid="error-descripcion">
          {{ descripcionError }}
        </em>
      </label>

      <label class="request-form__field">
        <span>Categoría</span>
        <select
          v-model="form.categoriaId"
          data-testid="form-categoria"
          class="form-control"
          :aria-invalid="Boolean(categoriaError)"
        >
          <option value="" disabled>Selecciona una categoría</option>
          <option
            v-for="categoria in props.categorias"
            :key="categoria.id"
            :value="categoria.id"
          >
            {{ categoria.nombre }} · {{ categoria.slaHoras }} h SLA
          </option>
        </select>
        <em v-if="categoriaError" data-testid="error-categoria">
          {{ categoriaError }}
        </em>
      </label>

      <label class="request-form__field">
        <span>Prioridad</span>
        <select
          data-testid="form-prioridad"
          class="form-control"
          :value="form.prioridad"
          @change="setPrioridad"
        >
          <option value="Baja">Baja</option>
          <option value="Media">Media</option>
          <option value="Alta">Alta</option>
          <option value="Critica">Crítica</option>
        </select>
      </label>
    </fieldset>

    <p v-if="props.serverError" class="request-form__server-error" role="alert">
      {{ props.serverError }}
    </p>

    <footer class="request-form__actions">
      <button
        data-testid="form-cancelar"
        class="button button--secondary"
        type="button"
        :disabled="props.isSubmitting"
        @click="emit('cancel')"
      >
        <ArrowLeft :size="17" :stroke-width="1.8" aria-hidden="true" />
        Cancelar
      </button>
      <button
        data-testid="form-submit"
        class="button button--primary"
        type="submit"
        :disabled="props.isSubmitting"
      >
        <Save :size="17" :stroke-width="1.8" aria-hidden="true" />
        {{ props.isSubmitting ? 'Guardando…' : props.submitLabel }}
      </button>
    </footer>
  </form>
</template>

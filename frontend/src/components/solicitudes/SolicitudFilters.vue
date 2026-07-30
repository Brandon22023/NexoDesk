<script setup lang="ts">
import { RotateCcw, Search } from '@lucide/vue'
import type { CategoriaDto } from '../../types/categorias'
import type { FiltrosSolicitudes } from '../../types/solicitudes'

const props = defineProps<{
  modelValue: FiltrosSolicitudes
  categorias: CategoriaDto[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: FiltrosSolicitudes]
  clear: []
}>()

function update(patch: Partial<FiltrosSolicitudes>): void {
  emit('update:modelValue', { ...props.modelValue, ...patch })
}

function getValue(event: Event): string {
  return (event.target as HTMLInputElement).value
}

function updateVencidas(event: Event): void {
  const value = getValue(event)
  update({ vencidas: value === '' ? null : value === 'true' })
}
</script>

<template>
  <section class="filters surface" aria-label="Filtros de solicitudes">
    <label class="filters__search">
      <span>Buscar</span>
      <div>
        <Search :size="17" :stroke-width="1.8" aria-hidden="true" />
        <input
          data-testid="filtro-busqueda"
          type="search"
          :value="props.modelValue.q"
          placeholder="Código, título o descripción"
          @input="update({ q: getValue($event) })"
        />
      </div>
    </label>

    <label>
      <span>Estado</span>
      <select
        data-testid="filtro-estado"
        :value="props.modelValue.estado"
        @change="update({ estado: getValue($event) as FiltrosSolicitudes['estado'] })"
      >
        <option value="">Todos</option>
        <option value="Nueva">Nueva</option>
        <option value="Asignada">Asignada</option>
        <option value="EnProceso">En proceso</option>
        <option value="Resuelta">Resuelta</option>
        <option value="Cerrada">Cerrada</option>
        <option value="Cancelada">Cancelada</option>
      </select>
    </label>

    <label>
      <span>Prioridad</span>
      <select
        data-testid="filtro-prioridad"
        :value="props.modelValue.prioridad"
        @change="update({ prioridad: getValue($event) as FiltrosSolicitudes['prioridad'] })"
      >
        <option value="">Todas</option>
        <option value="Critica">Crítica</option>
        <option value="Alta">Alta</option>
        <option value="Media">Media</option>
        <option value="Baja">Baja</option>
      </select>
    </label>

    <label>
      <span>Categoría</span>
      <select
        data-testid="filtro-categoria"
        :value="props.modelValue.categoriaId"
        @change="update({ categoriaId: getValue($event) })"
      >
        <option value="">Todas</option>
        <option
          v-for="categoria in props.categorias"
          :key="categoria.id"
          :value="categoria.id"
        >
          {{ categoria.nombre }}
        </option>
      </select>
    </label>

    <label>
      <span>SLA</span>
      <select
        data-testid="filtro-vencidas"
        :value="props.modelValue.vencidas === null ? '' : String(props.modelValue.vencidas)"
        @change="updateVencidas"
      >
        <option value="">Todas</option>
        <option value="true">Vencidas</option>
        <option value="false">En plazo</option>
      </select>
    </label>

    <button
      data-testid="btn-limpiar-filtros"
      class="button button--secondary filters__clear"
      type="button"
      @click="emit('clear')"
    >
      <RotateCcw :size="16" :stroke-width="1.8" aria-hidden="true" />
      Limpiar
    </button>
  </section>
</template>

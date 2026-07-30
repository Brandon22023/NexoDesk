<script setup lang="ts">
import { ArrowUpRight, Clock3 } from '@lucide/vue'
import type { SolicitudListaDto } from '../../types/solicitudes'

const props = defineProps<{
  solicitudes: SolicitudListaDto[]
}>()

const dateFormatter = new Intl.DateTimeFormat('es-GT', {
  dateStyle: 'medium',
  timeStyle: 'short',
})

function formatDate(value: string): string {
  return dateFormatter.format(new Date(value))
}

function estadoLabel(value: SolicitudListaDto['estado']): string {
  return value === 'EnProceso' ? 'En proceso' : value
}

function prioridadLabel(value: SolicitudListaDto['prioridad']): string {
  return value === 'Critica' ? 'Crítica' : value
}
</script>

<template>
  <div class="table-shell surface">
    <table data-testid="tabla-solicitudes" class="requests-table">
      <thead>
        <tr>
          <th>Código</th>
          <th>Solicitud</th>
          <th>Estado</th>
          <th>Prioridad</th>
          <th>Agente</th>
          <th>SLA</th>
          <th aria-label="Abrir"></th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="solicitud in props.solicitudes"
          :key="solicitud.id"
          data-testid="fila-solicitud"
          :data-codigo="solicitud.codigo"
        >
          <td data-testid="celda-codigo">
            <RouterLink
              class="requests-table__code"
              :to="{ name: 'solicitud-detalle', params: { id: solicitud.id } }"
            >
              {{ solicitud.codigo }}
            </RouterLink>
          </td>
          <td>
            <strong>{{ solicitud.titulo }}</strong>
            <small>{{ solicitud.categoria.nombre }}</small>
          </td>
          <td data-testid="celda-estado">
            <span
              class="status-badge"
              :class="`status-badge--${solicitud.estado.toLowerCase()}`"
            >
              {{ estadoLabel(solicitud.estado) }}
            </span>
          </td>
          <td data-testid="celda-prioridad">
            <span
              class="priority-badge"
              :class="`priority-badge--${solicitud.prioridad.toLowerCase()}`"
            >
              {{ prioridadLabel(solicitud.prioridad) }}
            </span>
          </td>
          <td>{{ solicitud.agente?.nombre ?? 'Sin asignar' }}</td>
          <td data-testid="celda-sla">
            <div
              class="requests-table__sla"
              :class="{ 'is-overdue': solicitud.vencida }"
            >
              <Clock3 :size="15" :stroke-width="1.8" aria-hidden="true" />
              <span>{{ formatDate(solicitud.fechaLimiteSla) }}</span>
              <em v-if="solicitud.vencida" data-testid="badge-vencida">
                Vencida
              </em>
            </div>
          </td>
          <td>
            <RouterLink
              class="icon-button"
              :to="{ name: 'solicitud-detalle', params: { id: solicitud.id } }"
              :aria-label="`Abrir ${solicitud.codigo}`"
            >
              <ArrowUpRight :size="17" :stroke-width="1.8" aria-hidden="true" />
            </RouterLink>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

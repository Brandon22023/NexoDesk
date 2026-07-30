<script setup lang="ts">
import { CircleAlert, Inbox, LoaderCircle } from '@lucide/vue'

const props = withDefaults(defineProps<{
  state: 'loading' | 'empty' | 'error'
  title?: string
  message?: string
  dataTestid?: string
  compact?: boolean
}>(), {
  title: '',
  message: '',
  dataTestid: undefined,
  compact: false,
})

const emit = defineEmits<{
  retry: []
}>()
</script>

<template>
  <div
    class="app-state"
    :class="{ 'app-state--compact': props.compact }"
    :data-testid="props.dataTestid"
    :aria-live="props.state === 'loading' ? 'polite' : undefined"
  >
    <LoaderCircle
      v-if="props.state === 'loading'"
      class="app-state__loader"
      :size="28"
      :stroke-width="1.8"
      aria-hidden="true"
    />
    <Inbox
      v-else-if="props.state === 'empty'"
      :size="31"
      :stroke-width="1.7"
      aria-hidden="true"
    />
    <CircleAlert
      v-else
      :size="31"
      :stroke-width="1.7"
      aria-hidden="true"
    />

    <strong>
      {{ props.title || (props.state === 'loading' ? 'Cargando…' : '') }}
    </strong>
    <p v-if="props.message">{{ props.message }}</p>
    <button
      v-if="props.state === 'error'"
      class="button button--secondary button--small"
      type="button"
      @click="emit('retry')"
    >
      Reintentar
    </button>
  </div>
</template>

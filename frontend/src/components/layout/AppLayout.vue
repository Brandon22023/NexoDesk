<script setup lang="ts">
import { CircleAlert, CircleCheck, Info, X } from '@lucide/vue'
import AppNavigation from './AppNavigation.vue'
import { useUiStore } from '../../stores/ui'

const uiStore = useUiStore()
</script>

<template>
  <div class="app-shell">
    <AppNavigation />

    <main class="app-content">
      <RouterView />
    </main>

    <Transition name="toast">
      <div
        v-if="uiStore.toast"
        data-testid="toast-mensaje"
        class="app-toast"
        :class="`app-toast--${uiStore.toast.tone}`"
        role="status"
      >
        <CircleCheck
          v-if="uiStore.toast.tone === 'success'"
          :size="19"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <CircleAlert
          v-else-if="uiStore.toast.tone === 'error'"
          :size="19"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <Info
          v-else
          :size="19"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <span>{{ uiStore.toast.text }}</span>
        <button
          type="button"
          aria-label="Cerrar notificación"
          @click="uiStore.dismissToast"
        >
          <X :size="16" :stroke-width="1.8" aria-hidden="true" />
        </button>
      </div>
    </Transition>
  </div>
</template>

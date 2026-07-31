import { ref } from 'vue'
import { defineStore } from 'pinia'

type ToastTone = 'success' | 'error' | 'info'

interface ToastMessage {
  text: string
  tone: ToastTone
}

export const useUiStore = defineStore('ui', () => {
  // Los avisos se comparten entre pantallas para confirmar acciones sin interrumpir el trabajo.
  const toast = ref<ToastMessage | null>(null)
  let dismissTimer: number | null = null

  function showToast(
    text: string,
    tone: ToastTone = 'success',
    duration = 4_000,
  ): void {
    // Un nuevo aviso reemplaza al anterior para que la persona vea siempre el resultado más reciente.
    toast.value = { text, tone }

    if (dismissTimer !== null) {
      window.clearTimeout(dismissTimer)
    }

    dismissTimer = window.setTimeout(() => {
      toast.value = null
      dismissTimer = null
    }, duration)
  }

  function dismissToast(): void {
    // El aviso puede cerrarse manualmente antes de que termine su tiempo de lectura.
    toast.value = null
    if (dismissTimer !== null) {
      window.clearTimeout(dismissTimer)
      dismissTimer = null
    }
  }

  return {
    toast,
    showToast,
    dismissToast,
  }
})

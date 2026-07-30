import { ref } from 'vue'
import { defineStore } from 'pinia'

type ToastTone = 'success' | 'error' | 'info'

interface ToastMessage {
  text: string
  tone: ToastTone
}

export const useUiStore = defineStore('ui', () => {
  const toast = ref<ToastMessage | null>(null)
  let dismissTimer: number | null = null

  function showToast(
    text: string,
    tone: ToastTone = 'success',
    duration = 4_000,
  ): void {
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

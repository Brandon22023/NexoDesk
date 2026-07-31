import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './router'
import { configureHttpClient } from './api/http'
import { useAuthStore } from './stores/auth'
import { pinia } from './stores/pinia'

const authStore = useAuthStore(pinia)

// Todas las llamadas protegidas comparten la sesión actual y devuelven al login cuando ya no es válida.
configureHttpClient({
  getAccessToken: () => authStore.accessToken,
  onUnauthorized: () => {
    authStore.signOut()

    if (router.currentRoute.value.name !== 'login') {
      void router.replace({
        name: 'login',
        query: { redirect: router.currentRoute.value.fullPath },
      })
    }
  },
})

createApp(App)
  .use(pinia)
  .use(router)
  .mount('#app')

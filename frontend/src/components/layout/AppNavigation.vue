<script setup lang="ts">
import { Headset, LogOut, Plus, Tickets } from '@lucide/vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const authStore = useAuthStore()
const router = useRouter()

async function logout(): Promise<void> {
  authStore.signOut()
  await router.replace({ name: 'login' })
}
</script>

<template>
  <header data-testid="app-nav" class="app-nav">
    <RouterLink class="app-nav__brand" to="/solicitudes">
      <span class="app-nav__brand-mark" aria-hidden="true">
        <Headset :size="19" :stroke-width="1.8" />
      </span>
      <span>Mesa<span>Sitec</span></span>
    </RouterLink>

    <nav class="app-nav__links" aria-label="Navegación principal">
      <RouterLink to="/solicitudes">
        <Tickets :size="17" :stroke-width="1.8" aria-hidden="true" />
        Solicitudes
      </RouterLink>
      <RouterLink to="/solicitudes/nueva">
        <Plus :size="17" :stroke-width="1.8" aria-hidden="true" />
        Nueva
      </RouterLink>
    </nav>

    <div class="app-nav__account">
      <div class="app-nav__identity">
        <span data-testid="nav-usuario-nombre">
          {{ authStore.usuario?.nombre }}
        </span>
        <small data-testid="nav-usuario-rol">
          {{ authStore.usuario?.rol }} · {{ authStore.usuario?.tenantNombre }}
        </small>
      </div>
      <button
        data-testid="btn-logout"
        class="icon-button"
        type="button"
        aria-label="Cerrar sesión"
        title="Cerrar sesión"
        @click="logout"
      >
        <LogOut :size="18" :stroke-width="1.8" aria-hidden="true" />
      </button>
    </div>
  </header>
</template>

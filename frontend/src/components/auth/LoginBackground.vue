<script setup lang="ts">
import { Headset, Workflow } from '@lucide/vue'
</script>

<template>
  <div class="login-ambient login-ambient--one" aria-hidden="true"></div>
  <div class="login-ambient login-ambient--two" aria-hidden="true"></div>
  <div class="login-ambient login-ambient--three" aria-hidden="true"></div>

<section class="login-brand-panel" aria-label="NexoDesk">
<RouterLink class="login-brand" to="/login" aria-label="NexoDesk, inicio">
      <span class="login-brand__mark" aria-hidden="true">
        <Headset :size="18" :stroke-width="1.8" />
      </span>
<span>Nexo<span>Desk</span></span>
    </RouterLink>

    <div class="login-brand__copy">
      <p class="login-eyebrow">SERVICE DESK MULTITENANT</p>
      <h1>Cada solicitud visible. Cada equipo coordinado.</h1>
      <p>
        Centraliza incidencias, categorías y acuerdos de servicio con
        trazabilidad de principio a fin.
      </p>
    </div>

    <div class="login-brand__note">
      <span aria-hidden="true">
        <Workflow :size="17" :stroke-width="1.8" />
      </span>
      <p>Flujos claros para que cada solicitud avance hasta su resolución.</p>
    </div>
  </section>
</template>

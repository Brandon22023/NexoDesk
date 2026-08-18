import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { pinia } from '../stores/pinia'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: () => import('../components/layout/AppLayout.vue'),
      meta: {
        requiresAuth: true,
      },
      children: [
        {
          path: '',
          redirect: { name: 'solicitudes' },
        },
        {
          path: 'solicitudes',
          name: 'solicitudes',
          component: () => import('../views/SolicitudesView.vue'),
          meta: {
title: 'Solicitudes | NexoDesk',
          },
        },
        {
          path: 'solicitudes/nueva',
          name: 'solicitud-nueva',
          component: () => import('../views/SolicitudNuevaView.vue'),
          meta: {
title: 'Nueva solicitud | NexoDesk',
          },
        },
        {
          path: 'solicitudes/:id',
          name: 'solicitud-detalle',
          component: () => import('../views/SolicitudDetalleView.vue'),
          meta: {
title: 'Detalle de solicitud | NexoDesk',
          },
        },
        {
          path: 'solicitudes/:id/editar',
          name: 'solicitud-editar',
          component: () => import('../views/SolicitudEditarView.vue'),
          meta: {
title: 'Editar solicitud | NexoDesk',
          },
        },
      ],
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: {
title: 'Iniciar sesión | NexoDesk',
      },
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: { name: 'login' },
    },
  ],
  scrollBehavior: () => ({ top: 0 }),
})

router.beforeEach((to) => {
  // El guard protege las pantallas internas y evita que una sesión vencida acceda a información de la organización.
  const authStore = useAuthStore(pinia)

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    // Se guarda el destino para que la persona retome la pantalla solicitada después de iniciar sesión.
    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }

  if (to.name === 'login' && authStore.isAuthenticated) {
    return { name: 'solicitudes' }
  }

  return true
})

router.afterEach((to) => {
  // El título cambia con la pantalla para que la navegación sea clara entre varias pestañas.
  document.title = typeof to.meta.title === 'string'
    ? to.meta.title
: 'NexoDesk'
})

export default router

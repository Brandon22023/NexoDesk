import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
  const env = {
    ...loadEnv(mode, '..', ''),
    ...process.env,
  }

  const host = env.VITE_HOST
  const port = Number(env.FRONTEND_CONTAINER_PORT)
  const apiTarget = env.API_PROXY_TARGET

  if (!host) {
    throw new Error('VITE_HOST is required')
  }

  if (!Number.isInteger(port) || port <= 0) {
    throw new Error('FRONTEND_CONTAINER_PORT must be a valid port')
  }

  if (!apiTarget) {
    throw new Error('API_PROXY_TARGET is required')
  }

  return {
    plugins: [vue()],
    envDir: '..',
    server: {
      host,
      port,
      strictPort: true,
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
        },
      },
    },
  }
})

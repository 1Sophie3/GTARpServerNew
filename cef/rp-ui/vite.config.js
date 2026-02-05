import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  base: './',
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    rollupOptions: {
      input: {
        login: 'login.html',
        hud: 'hud.html',
        inventory: 'inventory.html'
      }
    }
  },
  server: {
    port: 3000
  }
})

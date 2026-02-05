import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig({
  plugins: [vue()],
  base: './',
  root: './',
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    rollupOptions: {
      input: {
        login: path.resolve(__dirname, 'login.html'),
        hud: path.resolve(__dirname, 'hud.html')
      }
    }
  },
  server: {
    port: 3000,
    open: '/login.html'
  }
})
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:44322', // Match your .NET API port from launchSettings.json
        secure: false,
        changeOrigin: true,
      },
    },
  },
});
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true
  },
  server: {
    port: 5176,
    proxy: {
      '/ws/market-prices': {
        target: 'ws://localhost:8086',
        ws: true
      }
    }
  }
});

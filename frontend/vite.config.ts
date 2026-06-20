import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Dev server proxies /api to the .NET API host so the SPA and API share an origin in development.
export default defineConfig({
  plugins: [react()],
  build: {
    // The antd vendor chunk is ~1.3 MB raw (~0.4 MB gzip) and is a single long-cacheable file,
    // loaded once and shared across all routes — accepted, so lift the advisory warning above it.
    chunkSizeWarningLimit: 1500,
    rollupOptions: {
      output: {
        // Split large shared vendors into long-cacheable chunks separate from app code.
        manualChunks: {
          react: ['react', 'react-dom', 'react-router-dom'],
          antd: ['antd', '@ant-design/icons'],
          charts: ['recharts'],
          map: ['leaflet', 'react-leaflet'],
        },
      },
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL ?? 'http://localhost:8080',
        changeOrigin: true,
      },
    },
  },
});

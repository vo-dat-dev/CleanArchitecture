import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const target =
  process.env['services__webapi__https__0'] ||
  process.env['services__webapi__http__0'];

const customerServiceTarget =
  process.env['services__customer-service-api__https__0'] ||
  process.env['services__customer-service-api__http__0'] ||
  process.env['services__customer_service_api__https__0'] ||
  process.env['services__customer_service_api__http__0'];

console.log('[vite] webapi target:', target);
console.log('[vite] customerService target:', customerServiceTarget);
console.log('[vite] all services env vars:', Object.keys(process.env).filter(k => k.startsWith('services__')));

const proxyOptions = target
  ? { target, secure: false, changeOrigin: true }
  : undefined;

const customerServiceProxyOptions = customerServiceTarget
  ? { target: customerServiceTarget, secure: false, changeOrigin: true }
  : undefined;

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT!),
    proxy: {
      ...(customerServiceProxyOptions ? {
        '/api/Customers': customerServiceProxyOptions,
      } : {}),
      ...(proxyOptions ? {
        '/api': proxyOptions,
        '/openapi': proxyOptions,
        '/scalar': proxyOptions,
        '/weatherforecast': proxyOptions,
        '/WeatherForecast': proxyOptions,
      } : {}),
    },
  },
  build: {
    outDir: 'build',
  },
});

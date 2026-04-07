import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import mkcert from 'vite-plugin-mkcert';
import { fileURLToPath, URL } from 'node:url';
import Components from 'unplugin-vue-components/vite';
import { PrimeVueResolver } from 'unplugin-vue-components/resolvers';

export default defineConfig(() => {
    const useHttps = process.env.VITE_DEV_HTTPS === 'true';
    const apiBaseUrl = process.env.VITE_APP_API_BASE_URL || 'http://localhost:5221';

    return {
        build: { sourcemap: process.env.VITE_ENABLE_SOURCEMAP === 'true' },
        server: {
            port: 7220,
            https: useHttps,
            proxy: {
                '/v1': {
                    target: apiBaseUrl,
                    changeOrigin: true,
                    secure: false,
                },
            },
        },
        plugins: [vue(), mkcert(), Components({ resolvers: [PrimeVueResolver()] })],
        resolve: { alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) } },
    };
});

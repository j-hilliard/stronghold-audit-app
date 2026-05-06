<template>
    <Teleport to="body">
        <div class="vp-pill" @click.stop="cycle" :title="`Viewport: ${current.label} — click to cycle`">
            <span class="vp-pill__icon">{{ current.icon }}</span>
            <span>{{ current.label }}</span>
        </div>
    </Teleport>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';

const STORAGE_KEY = 'stronghold-dev-viewport';

const PRESETS = [
    { key: 'desktop', label: 'Desktop', icon: '🖥', meta: 'width=device-width, initial-scale=1' },
    { key: 'tablet',  label: 'Tablet',  icon: '📱', meta: 'width=768, initial-scale=1' },
    { key: 'phone',   label: 'Phone',   icon: '📲', meta: 'width=390, initial-scale=1' },
];

const currentIndex = ref(0);
const current = ref(PRESETS[0]);

function applyViewport(index: number) {
    const preset = PRESETS[index];
    const tag = document.querySelector('meta[name="viewport"]');
    if (tag) tag.setAttribute('content', preset.meta);
    currentIndex.value = index;
    current.value = preset;
    localStorage.setItem(STORAGE_KEY, preset.key);
}

function cycle() {
    applyViewport((currentIndex.value + 1) % PRESETS.length);
}

onMounted(() => {
    const saved = localStorage.getItem(STORAGE_KEY);
    const idx = PRESETS.findIndex(p => p.key === saved);
    applyViewport(idx >= 0 ? idx : 0);
});
</script>

<style scoped>
.vp-pill {
    position: fixed;
    bottom: 10px;
    left: 12px;
    z-index: 9999;
    display: flex;
    align-items: center;
    gap: 5px;
    padding: 3px 9px;
    background: rgba(15, 23, 42, 0.9);
    border: 1px solid #1e40af;
    border-radius: 999px;
    cursor: pointer;
    font-size: 10px;
    font-weight: 600;
    color: #60a5fa;
    white-space: nowrap;
    user-select: none;
    opacity: 0.55;
    transition: opacity 0.15s, border-color 0.15s;
}
.vp-pill:hover { opacity: 1; border-color: #3b82f6; }
.vp-pill__icon { font-size: 11px; line-height: 1; }
</style>

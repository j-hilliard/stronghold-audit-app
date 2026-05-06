<template>
    <Teleport to="body">
        <!-- Pill -->
        <div class="vp-pill" @click.stop="openPanel = !openPanel" :title="`Dev viewport: ${current.label}`">
            <span class="vp-dot" :style="{ background: current.color }" />
            <span>{{ current.label }}</span>
            <span v-if="current.width" class="vp-dim">· {{ current.width }}px</span>
        </div>

        <!-- Preset panel (opens above the pill) -->
        <Transition name="vp-slide">
            <div v-if="openPanel" class="vp-panel" @click.stop>
                <p class="vp-panel__title">Viewport Preview</p>
                <ul class="vp-panel__list">
                    <li v-for="preset in PRESETS" :key="preset.key">
                        <button
                            type="button"
                            class="vp-panel__item"
                            :class="{ active: current.key === preset.key }"
                            @click="selectPreset(preset)"
                        >
                            <span class="vp-dot" :style="{ background: preset.color }" />
                            <span class="vp-panel__name">{{ preset.label }}</span>
                            <span class="vp-panel__dim">{{ preset.width ? preset.width + 'px' : 'full width' }}</span>
                        </button>
                    </li>
                </ul>
                <p class="vp-panel__hint">
                    Constrains <code>#app</code> and collapses<br>
                    the sidebar to simulate narrow layouts.<br>
                    <em>Tailwind breakpoints still reflect the<br>real browser window width.</em>
                </p>
            </div>
        </Transition>
    </Teleport>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue';

const STORAGE_KEY = 'stronghold-dev-viewport';
const STYLE_ID    = 'dev-vp-injected';

interface Preset {
    key: string;
    label: string;
    color: string;
    width: number | null; // null = full/desktop
}

const PRESETS: Preset[] = [
    { key: 'desktop', label: 'Desktop', color: '#22d3ee', width: null },
    { key: 'tablet',  label: 'Tablet',  color: '#a78bfa', width: 768  },
    { key: 'phone',   label: 'Phone',   color: '#fb923c', width: 390  },
];

const openPanel = ref(false);
const current   = ref<Preset>(PRESETS[0]);

// ── CSS injection ──────────────────────────────────────────────────────────────
// Constrains #app (the Vue mount point) to target width and centers it against a
// dark "outside viewport" background. Fixed/teleported elements (dialogs, toasts)
// render on <body> directly and are unaffected.

function buildCSS(preset: Preset): string {
    if (!preset.width) return '';
    return `
html {
    background: #03070f !important;
    background-image:
        repeating-linear-gradient(
            45deg,
            transparent, transparent 5px,
            rgba(255,255,255,0.018) 5px, rgba(255,255,255,0.018) 6px
        ) !important;
}
#app {
    max-width: ${preset.width}px !important;
    margin: 0 auto !important;
    min-height: 100vh;
    outline: 1px solid rgba(255,255,255,0.07);
    box-shadow: 0 0 80px rgba(0,0,0,0.95);
    overflow-x: hidden;
}
`.trim();
}

function inject(preset: Preset) {
    document.getElementById(STYLE_ID)?.remove();
    const css = buildCSS(preset);
    if (!css) return;
    const el = document.createElement('style');
    el.id = STYLE_ID;
    el.textContent = css;
    document.head.appendChild(el);
}

// ── Layout signal ──────────────────────────────────────────────────────────────
// AppLayout listens for this event and collapses or restores the sidebar so that
// the narrow constrained frame is usable (not half-eaten by the side nav).

function signalLayout(preset: Preset) {
    window.dispatchEvent(
        new CustomEvent<{ preset: string }>('dev-viewport-change', {
            detail: { preset: preset.key },
        }),
    );
}

// ── Apply ──────────────────────────────────────────────────────────────────────

function apply(preset: Preset) {
    inject(preset);
    signalLayout(preset);
    current.value = preset;
    localStorage.setItem(STORAGE_KEY, preset.key);
}

function selectPreset(preset: Preset) {
    apply(preset);
    openPanel.value = false;
}

// Close panel on outside click
function onDocClick() {
    openPanel.value = false;
}

onMounted(() => {
    document.addEventListener('click', onDocClick);
    const savedKey = localStorage.getItem(STORAGE_KEY);
    const preset   = PRESETS.find(p => p.key === savedKey) ?? PRESETS[0];
    apply(preset);
});

onBeforeUnmount(() => {
    document.removeEventListener('click', onDocClick);
    document.getElementById(STYLE_ID)?.remove();
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
    background: rgba(15, 23, 42, 0.92);
    border: 1px solid #1e3a5f;
    border-radius: 999px;
    cursor: pointer;
    font-size: 10px;
    font-weight: 600;
    color: #7dd3fc;
    white-space: nowrap;
    user-select: none;
    opacity: 0.55;
    transition: opacity 0.15s, border-color 0.15s;
}
.vp-pill:hover { opacity: 1; border-color: #3b82f6; }

.vp-dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    flex-shrink: 0;
    display: inline-block;
}

.vp-dim { color: #475569; font-weight: 400; }

/* Panel */
.vp-panel {
    position: fixed;
    bottom: 36px;
    left: 12px;
    z-index: 9999;
    background: #0f172a;
    border: 1px solid #1e3a5f;
    border-radius: 8px;
    width: 210px;
    padding: 10px 0 8px;
    box-shadow: 0 4px 24px rgba(0,0,0,0.7);
}
.vp-panel__title {
    font-size: 10px;
    font-weight: 700;
    letter-spacing: 0.08em;
    color: #7dd3fc;
    text-transform: uppercase;
    padding: 0 12px 6px;
    border-bottom: 1px solid #1e293b;
    margin: 0 0 4px;
}
.vp-panel__list {
    list-style: none;
    margin: 0;
    padding: 0;
}
.vp-panel__item {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 12px;
    background: none;
    border: none;
    cursor: pointer;
    transition: background 0.1s;
    text-align: left;
}
.vp-panel__item:hover { background: #1e293b; }
.vp-panel__item.active { background: #172033; }
.vp-panel__name {
    font-size: 12px;
    font-weight: 600;
    color: #cbd5e1;
    flex: 1;
}
.vp-panel__item.active .vp-panel__name { color: #7dd3fc; }
.vp-panel__dim {
    font-size: 10px;
    color: #475569;
}
.vp-panel__hint {
    font-size: 10px;
    color: #334155;
    line-height: 1.5;
    padding: 8px 12px 0;
    margin: 6px 0 0;
    border-top: 1px solid #1e293b;
}
.vp-panel__hint em { font-style: italic; color: #2d3f54; }
.vp-panel__hint code { font-family: monospace; color: #3d5166; }

/* Transition */
.vp-slide-enter-active, .vp-slide-leave-active { transition: opacity 0.12s, transform 0.12s; }
.vp-slide-enter-from, .vp-slide-leave-to { opacity: 0; transform: translateY(4px); }
</style>

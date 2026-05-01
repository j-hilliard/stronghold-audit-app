<template>
    <div class="relative" ref="containerRef">
        <!-- Trigger button -->
        <button
            type="button"
            class="add-block-trigger"
            @click="open = !open"
            title="Add block"
        >
            <i class="pi pi-plus text-xs" />
            Add Block
        </button>

        <!-- Dropdown panel -->
        <Transition name="abm-fade">
            <div v-if="open" class="abm-panel" @keydown.esc="open = false">
                <div
                    v-for="group in GROUPS"
                    :key="group"
                    class="abm-group"
                >
                    <div class="abm-group-label">{{ group }}</div>
                    <div class="abm-grid">
                        <button
                            v-for="meta in byGroup(group)"
                            :key="meta.type"
                            type="button"
                            class="abm-item"
                            @click="pick(meta.type)"
                        >
                            <i :class="[meta.icon, 'text-sm text-slate-400']" />
                            <span>{{ meta.label }}</span>
                        </button>
                    </div>
                </div>
            </div>
        </Transition>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import type { BlockType } from '../types/report-block';
import { ADDABLE_BLOCKS } from '../composables/useBlockComposer';

const emit = defineEmits<{ add: [type: BlockType] }>();

const open        = ref(false);
const containerRef = ref<HTMLElement | null>(null);

const GROUPS = ['Text', 'Data', 'Media', 'Layout'] as const;

function byGroup(group: string) {
    return ADDABLE_BLOCKS.filter(b => b.group === group);
}

function pick(type: BlockType) {
    open.value = false;
    emit('add', type);
}

function onDocClick(e: MouseEvent) {
    if (!containerRef.value?.contains(e.target as Node)) open.value = false;
}

onMounted(()  => document.addEventListener('click', onDocClick, true));
onUnmounted(() => document.removeEventListener('click', onDocClick, true));
</script>

<style scoped>
.add-block-trigger {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 6px 14px;
    font-size: 0.75rem;
    font-weight: 500;
    color: #94a3b8;
    background: #1e293b;
    border: 1px dashed #475569;
    border-radius: 8px;
    cursor: pointer;
    transition: color 0.15s, border-color 0.15s, background 0.15s;
}
.add-block-trigger:hover {
    color: #e2e8f0;
    border-color: #64748b;
    background: #273548;
}

.abm-panel {
    position: absolute;
    bottom: calc(100% + 6px);
    left: 50%;
    transform: translateX(-50%);
    width: 320px;
    background: #1e293b;
    border: 1px solid #334155;
    border-radius: 10px;
    padding: 12px;
    z-index: 100;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.5);
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.abm-group-label {
    font-size: 0.65rem;
    font-weight: 700;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: #475569;
    margin-bottom: 4px;
}

.abm-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 4px;
}

.abm-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    padding: 8px 6px;
    font-size: 0.7rem;
    color: #94a3b8;
    background: transparent;
    border: 1px solid transparent;
    border-radius: 6px;
    cursor: pointer;
    transition: background 0.12s, color 0.12s, border-color 0.12s;
    line-height: 1.2;
    text-align: center;
}
.abm-item:hover {
    background: #273548;
    border-color: #3b4f6b;
    color: #e2e8f0;
}

/* Transition */
.abm-fade-enter-active, .abm-fade-leave-active { transition: opacity 0.12s, transform 0.12s; }
.abm-fade-enter-from, .abm-fade-leave-to { opacity: 0; transform: translateX(-50%) translateY(4px); }
</style>

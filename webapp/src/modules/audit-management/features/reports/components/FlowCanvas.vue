<template>
    <div class="flow-canvas flex flex-col min-h-0">

        <!-- ── Mode bar ─────────────────────────────────────────────────────── -->
        <div class="fc-modebar">
            <span class="text-xs text-slate-500">{{ blocks.length }} block{{ blocks.length !== 1 ? 's' : '' }}</span>
            <div class="flex items-center gap-1.5 ml-auto">
                <button
                    class="fc-mode-btn"
                    :class="editMode ? 'fc-mode-btn--active' : ''"
                    @click="$emit('toggle-edit-mode')"
                    :title="editMode ? 'Switch to preview' : 'Switch to edit'"
                >
                    <i :class="editMode ? 'pi pi-eye' : 'pi pi-pencil'" class="text-[10px]" />
                    {{ editMode ? 'Preview' : 'Edit' }}
                </button>
            </div>
        </div>

        <!-- ── Block list ────────────────────────────────────────────────────── -->
        <div class="flex-1 overflow-y-auto px-4 py-3 fc-scroll" ref="scrollEl">

            <!-- Empty state -->
            <div v-if="!localBlocks.length" class="fc-empty">
                <i class="pi pi-file-edit text-4xl text-slate-600" />
                <p class="text-sm text-slate-400 font-medium mt-3">Canvas is empty</p>
                <p class="text-xs text-slate-500 mt-1">Generate a report or add blocks below to get started.</p>
            </div>

            <draggable
                v-model="localBlocks"
                item-key="id"
                handle=".fc-drag-handle"
                :animation="150"
                ghost-class="fc-ghost"
                drag-class="fc-dragging"
                @end="onDragEnd"
            >
                <template #item="{ element: block }">
                    <div
                        class="fc-block group"
                        :data-block-id="block.id"
                    >
                        <!-- Left drag handle (edit mode only) -->
                        <div
                            v-if="editMode"
                            class="fc-drag-handle"
                            title="Drag to reorder"
                        >
                            <i class="pi pi-grip-lines text-slate-500 text-xs" />
                        </div>

                        <!-- Block content -->
                        <div class="fc-block-inner flex-1 min-w-0">
                            <!-- Type label badge (edit mode) -->
                            <div v-if="editMode" class="fc-type-badge">
                                {{ TYPE_LABELS[block.type] ?? block.type }}
                            </div>

                            <BlockRenderer
                                :block="block"
                                :edit-mode="editMode"
                                @update:content="(c) => $emit('update-content', block.id, c)"
                                @update:isEdited="(v) => $emit('update-is-edited', block.id, v)"
                            />
                        </div>

                        <!-- Right action buttons (edit mode only) -->
                        <div
                            v-if="editMode"
                            class="fc-actions opacity-0 group-hover:opacity-100"
                        >
                            <button class="fc-action-btn" title="Duplicate" @click="$emit('duplicate', block.id)">
                                <i class="pi pi-copy text-xs" />
                            </button>
                            <button class="fc-action-btn fc-action-btn--danger" title="Remove" @click="$emit('remove', block.id)">
                                <i class="pi pi-trash text-xs" />
                            </button>
                        </div>
                    </div>
                </template>
            </draggable>

            <!-- Add block button at bottom -->
            <div v-if="editMode" class="flex justify-center py-4">
                <AddBlockMenu @add="(type) => $emit('add', type)" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import draggable from 'vuedraggable';
import type { ReportBlock, BlockType } from '../types/report-block';
import BlockRenderer from './BlockRenderer.vue';
import AddBlockMenu from './AddBlockMenu.vue';

const props = defineProps<{
    blocks: ReportBlock[];
    editMode: boolean;
}>();

const emit = defineEmits<{
    (e: 'reorder',          blocks: ReportBlock[]): void;
    (e: 'add',              type: BlockType): void;
    (e: 'remove',           id: string): void;
    (e: 'duplicate',        id: string): void;
    (e: 'update-content',   id: string, content: unknown): void;
    (e: 'update-is-edited', id: string, value: boolean): void;
    (e: 'toggle-edit-mode'): void;
}>();

// ── Local copy for vuedraggable ───────────────────────────────────────────────
// vuedraggable v-model mutates the array in-place on drag.
// We maintain a local shallow copy and emit reorder when drag ends.

const localBlocks = ref<ReportBlock[]>([...props.blocks]);

watch(
    () => props.blocks,
    (val) => { localBlocks.value = [...val]; },
    { deep: false },
);

function onDragEnd() {
    emit('reorder', [...localBlocks.value]);
}

// ── Type badge labels ─────────────────────────────────────────────────────────

const TYPE_LABELS: Partial<Record<ReportBlock['type'], string>> = {
    'cover':            'Cover',
    'cover-page':       'Cover Page',
    'heading':          'Heading',
    'kpi-grid':         'KPI Grid',
    'chart-bar':        'Bar Chart',
    'chart-line':       'Line Chart',
    'narrative':        'Rich Text',
    'callout':          'Callout',
    'ca-table':         'CA Table',
    'image':            'Image',
    'column-row':       'Two Columns',
    'divider':          'Divider',
    'spacer':           'Spacer',
    'toc-sidebar':      'TOC Sidebar',
    'oval-callout':     'Oval Callout',
    'findings-category':'Findings',
};

const scrollEl = ref<HTMLElement | null>(null);
</script>

<style scoped>
.flow-canvas {
    height: 100%;
    background: var(--surface-1, #0f172a);
}

/* ── Mode bar ────────────────────────────────────────────────────────── */
.fc-modebar {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 16px;
    background: var(--surface-2, #1e293b);
    border-bottom: 1px solid rgba(71, 85, 105, 0.4);
    flex-shrink: 0;
}

.fc-mode-btn {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 3px 10px;
    font-size: 0.7rem;
    font-weight: 500;
    border-radius: 4px;
    border: 1px solid #334155;
    background: transparent;
    color: #64748b;
    cursor: pointer;
    transition: all 0.12s;
}
.fc-mode-btn:hover          { background: #273548; color: #94a3b8; }
.fc-mode-btn--active        { background: #1e3a5f; border-color: #2563eb; color: #93c5fd; }

/* ── Scroll area ─────────────────────────────────────────────────────── */
.fc-scroll { scroll-behavior: smooth; }

/* ── Empty state ─────────────────────────────────────────────────────── */
.fc-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 240px;
    text-align: center;
}

/* ── Block wrapper ───────────────────────────────────────────────────── */
.fc-block {
    display: flex;
    align-items: flex-start;
    gap: 0;
    margin-bottom: 6px;
    border-radius: 8px;
    border: 1px solid transparent;
    transition: border-color 0.12s;
    position: relative;
}
.fc-block:hover {
    border-color: rgba(71, 85, 105, 0.5);
}

/* ── Drag handle ─────────────────────────────────────────────────────── */
.fc-drag-handle {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 20px;
    flex-shrink: 0;
    align-self: stretch;
    cursor: grab;
    opacity: 0;
    transition: opacity 0.12s;
    border-radius: 8px 0 0 8px;
    background: transparent;
}
.fc-drag-handle:active { cursor: grabbing; }
.fc-block:hover .fc-drag-handle { opacity: 1; }

/* ── Block inner ─────────────────────────────────────────────────────── */
.fc-block-inner {
    background: var(--surface-2, #1e293b);
    border-radius: 6px;
    overflow: hidden;
    border: 1px solid rgba(51, 65, 85, 0.5);
}

/* ── Type badge ──────────────────────────────────────────────────────── */
.fc-type-badge {
    display: inline-block;
    padding: 2px 8px;
    font-size: 0.6rem;
    font-weight: 600;
    letter-spacing: 0.07em;
    text-transform: uppercase;
    color: #475569;
    background: rgba(30, 41, 59, 0.9);
    border-bottom: 1px solid rgba(51, 65, 85, 0.5);
    width: 100%;
    box-sizing: border-box;
}

/* ── Action buttons ──────────────────────────────────────────────────── */
.fc-actions {
    display: flex;
    flex-direction: column;
    gap: 3px;
    padding: 2px 0 2px 4px;
    align-self: flex-start;
    transition: opacity 0.12s;
}

.fc-action-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    border-radius: 4px;
    border: 1px solid #334155;
    background: #1e293b;
    color: #64748b;
    cursor: pointer;
    transition: background 0.1s, color 0.1s, border-color 0.1s;
}
.fc-action-btn:hover { background: #273548; color: #94a3b8; border-color: #475569; }
.fc-action-btn--danger:hover { background: #3f1010; color: #f87171; border-color: #7f1d1d; }

/* ── Drag ghost / active ─────────────────────────────────────────────── */
.fc-ghost {
    opacity: 0.25;
    background: rgba(59, 130, 246, 0.08);
    border-radius: 8px;
}
.fc-dragging {
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.5);
    border-color: rgba(59, 130, 246, 0.4) !important;
}
</style>

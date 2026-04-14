<template>
    <!-- Dark canvas area — document page floats inside -->
    <div ref="scrollEl" class="flex-1 overflow-auto composer-canvas py-8 relative" style="background: #2a3142;">

        <!-- A4 document page (794 px wide = 210 mm @ 96 dpi) -->
        <div
            ref="pageEl"
            class="document-page mx-auto shadow-2xl print-page relative"
            style="width: 794px; background: #0f172a; border-radius: 4px; padding: 0;"
            :style="[pageZoomStyle, pageHeightStyle]"
            @click.self="$emit('select', null)"
        >
            <!-- Page guide lines (every 1123px after the first page) -->
            <div
                v-for="n in extraPageGuides"
                :key="n"
                class="absolute left-0 right-0 border-t border-dashed border-slate-600/40 pointer-events-none z-50"
                :style="{ top: `${n * 1123}px` }"
            />

            <!-- Blocks -->
            <div
                v-for="block in sortedBlocks"
                :key="block.id"
                class="block-wrapper absolute group"
                :class="[
                    selectedId === block.id
                        ? 'ring-1 ring-blue-400/70'
                        : 'hover:ring-1 hover:ring-slate-500/40',
                ]"
                :style="wrapperStyle(block)"
                :data-testid="`composer-block-${block.id}`"
                @click.stop="$emit('select', block)"
            >
                <!-- 8 resize handles — always in DOM, v-show to avoid focus-stealing DOM inserts -->
                <template v-if="!block.layout?.locked">
                    <div v-show="selectedId === block.id" class="rh rh-nw" @mousedown.stop.prevent="startResize($event, block, 'nw')" />
                    <div v-show="selectedId === block.id" class="rh rh-n"  @mousedown.stop.prevent="startResize($event, block, 'n')" />
                    <div v-show="selectedId === block.id" class="rh rh-ne" @mousedown.stop.prevent="startResize($event, block, 'ne')" />
                    <div v-show="selectedId === block.id" class="rh rh-e"  @mousedown.stop.prevent="startResize($event, block, 'e')" />
                    <div v-show="selectedId === block.id" class="rh rh-se" @mousedown.stop.prevent="startResize($event, block, 'se')" />
                    <div v-show="selectedId === block.id" class="rh rh-s"  @mousedown.stop.prevent="startResize($event, block, 's')" />
                    <div v-show="selectedId === block.id" class="rh rh-sw" @mousedown.stop.prevent="startResize($event, block, 'sw')" />
                    <div v-show="selectedId === block.id" class="rh rh-w"  @mousedown.stop.prevent="startResize($event, block, 'w')" />
                </template>

                <!-- Block action buttons — v-show keeps DOM stable so focus isn't disrupted -->
                <div
                    v-if="!block.isNewsletterBlock"
                    v-show="selectedId === block.id"
                    class="absolute -top-7 right-0 flex items-center gap-1 z-30 print:hidden"
                    @mousedown.stop
                >
                    <button @click.stop="$emit('move-up', block.id)"    class="block-btn" title="Move up"><i class="pi pi-chevron-up" /></button>
                    <button @click.stop="$emit('move-down', block.id)"  class="block-btn" title="Move down"><i class="pi pi-chevron-down" /></button>
                    <button @click.stop="$emit('duplicate', block.id)"  class="block-btn" title="Duplicate"><i class="pi pi-copy" /></button>
                    <button @click.stop="$emit('remove', block.id)"     class="block-btn block-btn--danger" title="Remove"><i class="pi pi-times" /></button>
                </div>

                <!-- Newsletter lock badge -->
                <div v-if="block.isNewsletterBlock" class="absolute -top-6 right-0 hidden group-hover:flex z-30 print:hidden" @mousedown.stop>
                    <span class="px-1.5 py-0.5 bg-slate-700 rounded text-slate-300 text-xs flex items-center gap-1">
                        <i class="pi pi-lock text-xs" /> Newsletter
                    </span>
                </div>

                <!-- Drag handle bar — only visible on hover, only for non-locked blocks -->
                <div
                    v-if="!block.isNewsletterBlock && !block.layout?.locked"
                    class="drag-handle-bar print:hidden"
                    :class="selectedId === block.id ? 'drag-handle-bar--active' : ''"
                    @mousedown.stop.prevent="startDrag($event, block)"
                    title="Drag to move"
                >
                    <i class="pi pi-grip-lines text-slate-400 text-xs" />
                </div>

                <!-- Block content — full user interaction, not draggable -->
                <div class="block-inner" :class="paddingClass(block)">
                    <component
                        :is="componentFor(block.type)"
                        v-bind="blockProps(block)"
                        :selected-inner-block-id="selectedId"
                        @update:content="(c: unknown) => $emit('update-content', block.id, c)"
                        @update:isEdited="(v: boolean) => $emit('update-is-edited', block.id, v)"
                        @select-inner="(b: ReportBlock) => $emit('select', b)"
                    />
                </div>
            </div>

            <!-- Empty state -->
            <div
                v-if="!blocks.length"
                class="absolute inset-0 flex flex-col items-center justify-center text-slate-500 text-sm space-y-2 pointer-events-none"
            >
                <i class="pi pi-file-edit text-3xl" />
                <div>Add blocks from the toolbar, or click Generate to build the report.</div>
            </div>
        </div>

        <!-- Zoom controls -->
        <div class="zoom-controls print:hidden">
            <button @click="zoomOut" :disabled="zoom <= 50" class="zoom-btn" title="Zoom out"><i class="pi pi-minus text-xs" /></button>
            <span class="zoom-pct">{{ zoom }}%</span>
            <button @click="zoomIn"  :disabled="zoom >= 150" class="zoom-btn" title="Zoom in"><i class="pi pi-plus text-xs" /></button>
            <button @click="zoom = 100" class="zoom-btn zoom-reset" title="Reset zoom"><i class="pi pi-refresh text-xs" /></button>
        </div>
    </div>
</template>

<script setup lang="ts">
import { defineAsyncComponent, ref, computed } from 'vue';
import type { ReportBlock, BlockType, BlockLayout } from '../types/report-block';

const props = defineProps<{
    blocks: ReportBlock[];
    selectedId: string | null;
}>();

const emit = defineEmits<{
    (e: 'select', block: ReportBlock | null): void;
    (e: 'remove', id: string): void;
    (e: 'move-up', id: string): void;
    (e: 'move-down', id: string): void;
    (e: 'duplicate', id: string): void;
    (e: 'update-content', id: string, content: unknown): void;
    (e: 'update-is-edited', id: string, value: boolean): void;
    (e: 'update-layout', id: string, layout: Partial<BlockLayout>): void;
}>();

const scrollEl = ref<HTMLElement | null>(null);
const pageEl   = ref<HTMLElement | null>(null);

// ── Zoom ──────────────────────────────────────────────────────────────────────
const ZOOM_STEPS = [50, 75, 100, 125, 150];
const zoom = ref(100);
const pageZoomStyle = computed(() => {
    if (zoom.value === 100) return {};
    const scale = zoom.value / 100;
    return { transform: `scale(${scale})`, transformOrigin: 'top center', marginBottom: `${(scale - 1) * 1123}px` };
});
function zoomIn()  { const n = ZOOM_STEPS.find(s => s > zoom.value); if (n) zoom.value = n; }
function zoomOut() { const p = [...ZOOM_STEPS].reverse().find(s => s < zoom.value); if (p) zoom.value = p; }

// ── Page height — grows to contain all blocks ─────────────────────────────────
const MIN_PAGE_H = 1123;
const pageContentH = computed(() => {
    if (!props.blocks.length) return MIN_PAGE_H;
    const maxBottom = Math.max(...props.blocks.map(b => (b.layout?.y ?? 0) + (b.layout?.height || 160)));
    return Math.max(MIN_PAGE_H, maxBottom + 80);
});
const pageHeightStyle = computed(() => ({ height: `${pageContentH.value}px` }));
const extraPageGuides = computed(() => {
    const count = Math.floor(pageContentH.value / MIN_PAGE_H);
    return Array.from({ length: count }, (_, i) => i + 1);
});

// Blocks sorted by zIndex so higher z renders on top
const sortedBlocks = computed(() =>
    [...props.blocks].sort((a, b) => (a.layout?.zIndex ?? 1) - (b.layout?.zIndex ?? 1)),
);

// ── Block wrapper style ───────────────────────────────────────────────────────
function wrapperStyle(block: ReportBlock): Record<string, string> {
    const l = block.layout ?? { x: 40, y: 0, width: 714, height: 0, zIndex: 1 };
    const s = block.style;
    const style: Record<string, string> = {
        left:   `${l.x}px`,
        top:    `${l.y}px`,
        width:  `${l.width}px`,
        height: l.height > 0 ? `${l.height}px` : 'auto',
        zIndex: String(l.zIndex ?? 1),
    };
    if (s?.backgroundColor && block.type !== 'cover') style.backgroundColor = s.backgroundColor;
    if (s?.textColor)   style.color       = s.textColor;
    if (s?.borderColor) style.borderColor = s.borderColor;
    return style;
}

// ── Drag to move — initiated only from the drag handle bar ───────────────────
const SNAP = 8; // px grid snap
function snap(v: number) { return Math.round(v / SNAP) * SNAP; }

type DragState = { blockId: string; startX: number; startY: number; origX: number; origY: number };
let dragState: DragState | null = null;

function startDrag(e: MouseEvent, block: ReportBlock) {
    if (block.layout?.locked) return;
    dragState = {
        blockId: block.id,
        startX: e.clientX,
        startY: e.clientY,
        origX: block.layout?.x ?? 40,
        origY: block.layout?.y ?? 0,
    };
    window.addEventListener('mousemove', onDragMove);
    window.addEventListener('mouseup', onDragEnd);
}

function onDragMove(e: MouseEvent) {
    if (!dragState) return;
    const scale = zoom.value / 100;
    const dx = (e.clientX - dragState.startX) / scale;
    const dy = (e.clientY - dragState.startY) / scale;
    if (Math.abs(dx) < 2 && Math.abs(dy) < 2) return; // dead zone
    emit('update-layout', dragState.blockId, {
        x: Math.max(0, snap(dragState.origX + dx)),
        y: Math.max(0, snap(dragState.origY + dy)),
    });
}

function onDragEnd() {
    dragState = null;
    window.removeEventListener('mousemove', onDragMove);
    window.removeEventListener('mouseup', onDragEnd);
}

// ── Resize ────────────────────────────────────────────────────────────────────
type ResizeHandle = 'n' | 'ne' | 'e' | 'se' | 's' | 'sw' | 'w' | 'nw';
type ResizeState = { blockId: string; handle: ResizeHandle; startX: number; startY: number; orig: BlockLayout };
let resizeState: ResizeState | null = null;

const MIN_W = 80;
const MIN_H = 40;

function startResize(e: MouseEvent, block: ReportBlock, handle: ResizeHandle) {
    // Measure the actual rendered dimensions (offsetWidth/Height ignore CSS transforms).
    // This is critical for blocks with height:0 (auto) so resize math starts from reality.
    const el = pageEl.value?.querySelector<HTMLElement>(`[data-testid="composer-block-${block.id}"]`);
    const renderedW = el?.offsetWidth  ?? block.layout?.width  ?? 714;
    const renderedH = el?.offsetHeight ?? block.layout?.height ?? 160;

    const layout = block.layout ?? { x: 40, y: 0, width: 714, height: 0, zIndex: 1 };
    resizeState = {
        blockId: block.id,
        handle,
        startX: e.clientX,
        startY: e.clientY,
        orig: { ...layout, width: renderedW, height: renderedH },
    };
    window.addEventListener('mousemove', onResizeMove);
    window.addEventListener('mouseup', onResizeEnd);
}

function onResizeMove(e: MouseEvent) {
    if (!resizeState) return;
    const scale = zoom.value / 100;
    const dx = (e.clientX - resizeState.startX) / scale;
    const dy = (e.clientY - resizeState.startY) / scale;
    const { orig, handle } = resizeState;
    let { x, y, width, height } = orig;

    if (handle.includes('e')) width  = Math.max(MIN_W, snap(orig.width + dx));
    if (handle.includes('s')) height = Math.max(MIN_H, snap(height + dy));
    if (handle.includes('w')) {
        const newW = Math.max(MIN_W, snap(orig.width - dx));
        x = orig.x + orig.width - newW;
        width = newW;
    }
    if (handle.includes('n')) {
        const newH = Math.max(MIN_H, snap(height - dy));
        y = orig.y + height - newH;
        height = newH;
    }

    emit('update-layout', resizeState.blockId, { x, y, width, height });
}

function onResizeEnd() {
    resizeState = null;
    window.removeEventListener('mousemove', onResizeMove);
    window.removeEventListener('mouseup', onResizeEnd);
}

// ── Block components ──────────────────────────────────────────────────────────
const blockComponents: Record<BlockType, ReturnType<typeof defineAsyncComponent>> = {
    'cover':               defineAsyncComponent(() => import('./blocks/CoverBlock.vue')),
    'heading':             defineAsyncComponent(() => import('./blocks/HeadingBlock.vue')),
    'kpi-grid':            defineAsyncComponent(() => import('./blocks/KpiGridBlock.vue')),
    'chart-bar':           defineAsyncComponent(() => import('./blocks/BarChartBlock.vue')),
    'chart-line':          defineAsyncComponent(() => import('./blocks/LineChartBlock.vue')),
    'narrative':           defineAsyncComponent(() => import('./blocks/NarrativeBlock.vue')),
    'callout':             defineAsyncComponent(() => import('./blocks/CalloutBlock.vue')),
    'ca-table':            defineAsyncComponent(() => import('./blocks/CaTableBlock.vue')),
    'image':               defineAsyncComponent(() => import('./blocks/ImageBlock.vue')),
    'column-row':          defineAsyncComponent(() => import('./blocks/ColumnRowBlock.vue')),
    'divider':             defineAsyncComponent(() => import('./blocks/DividerBlock.vue')),
    'spacer':              defineAsyncComponent(() => import('./blocks/SpacerBlock.vue')),
    'toc-sidebar':         defineAsyncComponent(() => import('./blocks/TocSidebarBlock.vue')),
    'oval-callout':        defineAsyncComponent(() => import('./blocks/OvalCalloutBlock.vue')),
    'findings-category':   defineAsyncComponent(() => import('./blocks/FindingsCategoryBlock.vue')),
};

function componentFor(type: BlockType) { return blockComponents[type]; }

function blockProps(block: ReportBlock) {
    return { content: block.content, style: block.style, isEdited: block.isEdited };
}

function paddingClass(block: ReportBlock) {
    // Cover blocks are always full-bleed — no gap between selection ring and content
    if (block.type === 'cover') return 'p-0';
    switch (block.style?.padding) {
        case 'none': return 'p-0';
        case 'sm':   return 'p-2';
        case 'lg':   return 'p-8';
        default:     return 'p-4';
    }
}
</script>

<style scoped>
/* ── Block wrapper ── */
.block-wrapper {
    box-sizing: border-box;
    border: 1px solid transparent;
    border-radius: 6px;
    transition: border-color 0.1s, box-shadow 0.1s;
    /* Flex column so block-inner fills the wrapper height when height is fixed */
    display: flex;
    flex-direction: column;
}

/* ── Drag handle bar — thin strip at top, only visible on hover/selection ── */
.drag-handle-bar {
    position: absolute;
    top: -14px;
    left: 50%;
    transform: translateX(-50%);
    display: none;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 14px;
    background: #334155;
    border-radius: 4px 4px 0 0;
    cursor: grab;
    z-index: 35;
}
.drag-handle-bar:active { cursor: grabbing; }
.drag-handle-bar--active { display: flex; }
.block-wrapper:hover .drag-handle-bar { display: flex; }

/* ── Block inner — fills wrapper height so content scales with resize ── */
.block-inner {
    user-select: text;
    cursor: default;
    flex: 1 1 auto;   /* fill wrapper height when wrapper has fixed height */
    min-height: 0;
    box-sizing: border-box;
    /* NO overflow: hidden — content should fill, not be clipped */
}

/* ── Contenteditable edit cues ── */
/* Must be :deep because the contenteditable lives inside async block components */
.block-inner :deep([contenteditable="true"]) {
    cursor: text;
    user-select: text;
    -webkit-user-select: text;
    border-radius: 3px;
    transition: outline 0.1s;
}
.block-inner :deep([contenteditable="true"]:hover) {
    outline: 1px dashed rgba(96, 165, 250, 0.35);
}
.block-inner :deep([contenteditable="true"]:focus) {
    outline: 2px solid rgba(96, 165, 250, 0.55);
    background: rgba(96, 165, 250, 0.04);
}

@media print {
    .block-inner :deep([contenteditable="true"]:focus),
    .block-inner :deep([contenteditable="true"]:hover) {
        outline: none;
        background: none;
    }
}

/* ── Block action buttons ── */
.block-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 22px;
    height: 22px;
    border-radius: 4px;
    background: #334155;
    color: #94a3b8;
    border: none;
    cursor: pointer;
    font-size: 10px;
    transition: background 0.1s, color 0.1s;
}
.block-btn:hover { background: #475569; color: #e2e8f0; }
.block-btn--danger:hover { background: #7f1d1d; color: #fca5a5; }

/* ── Resize handles ── */
.rh {
    position: absolute;
    width: 10px;
    height: 10px;
    background: #3b82f6;
    border: 2px solid #1e293b;
    border-radius: 2px;
    z-index: 40;
}
.rh-nw { top: -5px;  left: -5px;   cursor: nw-resize; }
.rh-n  { top: -5px;  left: calc(50% - 5px); cursor: n-resize; }
.rh-ne { top: -5px;  right: -5px;  cursor: ne-resize; }
.rh-e  { top: calc(50% - 5px); right: -5px; cursor: e-resize; }
.rh-se { bottom: -5px; right: -5px; cursor: se-resize; }
.rh-s  { bottom: -5px; left: calc(50% - 5px); cursor: s-resize; }
.rh-sw { bottom: -5px; left: -5px;  cursor: sw-resize; }
.rh-w  { top: calc(50% - 5px); left: -5px; cursor: w-resize; }

/* ── Zoom controls ── */
.zoom-controls {
    position: sticky;
    bottom: 12px;
    float: right;
    margin-right: 12px;
    display: flex;
    align-items: center;
    gap: 2px;
    background: #1e293b;
    border: 1px solid #334155;
    border-radius: 6px;
    padding: 3px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.4);
    z-index: 20;
}
.zoom-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    border-radius: 4px;
    background: transparent;
    color: #94a3b8;
    border: none;
    cursor: pointer;
    transition: background 0.1s, color 0.1s;
}
.zoom-btn:hover:not(:disabled) { background: #334155; color: #e2e8f0; }
.zoom-btn:disabled { opacity: 0.3; cursor: default; }
.zoom-pct {
    font-size: 11px;
    color: #94a3b8;
    padding: 0 4px;
    min-width: 36px;
    text-align: center;
    font-variant-numeric: tabular-nums;
}
.zoom-reset { border-left: 1px solid #334155; border-radius: 0 4px 4px 0; }

@media print {
    .zoom-controls { display: none; }
    .rh { display: none; }
    .block-btn { display: none; }
}
</style>

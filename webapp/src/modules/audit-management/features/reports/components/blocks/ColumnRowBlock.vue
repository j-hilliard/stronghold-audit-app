<template>
    <div class="column-row-block" :style="gridStyle">
        <!-- Left column -->
        <div class="column-slot" :class="{ 'column-slot--empty': !leftBlocks.length }">
            <draggable
                v-model="leftBlocks"
                handle=".col-drag-handle"
                item-key="id"
                animation="150"
                ghost-class="opacity-40"
                group="column-left"
            >
                <template #item="{ element: innerBlock }">
                    <div
                        class="relative group/inner rounded border transition-colors mb-2 cursor-pointer"
                        :class="selectedInnerBlockId === innerBlock.id
                            ? 'border-blue-400/70 ring-1 ring-blue-400/30'
                            : 'border-transparent hover:border-slate-600/60'"
                        :style="innerBlockStyle(innerBlock)"
                        :data-testid="`col-block-${innerBlock.id}`"
                        @click.stop="$emit('select-inner', innerBlock)"
                    >
                        <!-- Inner block controls -->
                        <div class="absolute top-1 right-1 hidden group-hover/inner:flex items-center gap-0.5 z-10">
                            <span class="col-drag-handle p-0.5 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs cursor-grab active:cursor-grabbing" title="Drag"><i class="pi pi-bars" /></span>
                            <button @click.stop="moveInnerBlock('left', innerBlock.id, -1)" class="p-0.5 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs" title="Up"><i class="pi pi-chevron-up" /></button>
                            <button @click.stop="moveInnerBlock('left', innerBlock.id, 1)" class="p-0.5 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs" title="Down"><i class="pi pi-chevron-down" /></button>
                            <button @click.stop="duplicateInnerBlock('left', innerBlock.id)" class="p-0.5 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs" title="Duplicate"><i class="pi pi-copy" /></button>
                            <button @click.stop="removeInnerBlock('left', innerBlock.id)" class="p-0.5 bg-slate-700 hover:bg-red-900 rounded text-slate-300 hover:text-red-300 text-xs" title="Remove"><i class="pi pi-times" /></button>
                        </div>
                        <component
                            :is="blockComponents[innerBlock.type]"
                            v-bind="innerBlockProps(innerBlock)"
                            @update:content="(c: unknown) => updateInnerContent('left', innerBlock.id, c)"
                        />
                    </div>
                </template>
            </draggable>

            <!-- Add block button -->
            <div class="col-add-btn-wrap" @click.stop>
                <button v-if="!showPickerLeft" @click.stop="showPickerLeft = true" class="col-add-btn">
                    <i class="pi pi-plus text-xs" /> Add Block
                </button>
                <div v-else class="col-mini-palette" @click.stop>
                    <button
                        v-for="item in miniPalette"
                        :key="item.type"
                        @click.stop="addBlock('left', item.type); showPickerLeft = false"
                        class="col-palette-item"
                    >
                        <i :class="item.icon" class="text-xs text-slate-500 w-3" />
                        {{ item.label }}
                    </button>
                    <button @click.stop="showPickerLeft = false" class="col-palette-close">Cancel</button>
                </div>
            </div>
        </div>

        <!-- Right column -->
        <div class="column-slot" :class="{ 'column-slot--empty': !rightBlocks.length }">
            <draggable
                v-model="rightBlocks"
                handle=".col-drag-handle"
                item-key="id"
                animation="150"
                ghost-class="opacity-40"
                group="column-right"
            >
                <template #item="{ element: innerBlock }">
                    <div
                        class="relative group/inner rounded border transition-colors mb-2 cursor-pointer"
                        :class="selectedInnerBlockId === innerBlock.id
                            ? 'border-blue-400/70 ring-1 ring-blue-400/30'
                            : 'border-transparent hover:border-slate-600/60'"
                        :style="innerBlockStyle(innerBlock)"
                        :data-testid="`col-block-${innerBlock.id}`"
                        @click.stop="$emit('select-inner', innerBlock)"
                    >
                        <!-- Inner block controls -->
                        <div class="absolute top-1 right-1 hidden group-hover/inner:flex items-center gap-0.5 z-10">
                            <span class="col-drag-handle p-0.5 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs cursor-grab active:cursor-grabbing" title="Drag"><i class="pi pi-bars" /></span>
                            <button @click.stop="moveInnerBlock('right', innerBlock.id, -1)" class="p-0.5 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs" title="Up"><i class="pi pi-chevron-up" /></button>
                            <button @click.stop="moveInnerBlock('right', innerBlock.id, 1)" class="p-0.5 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs" title="Down"><i class="pi pi-chevron-down" /></button>
                            <button @click.stop="duplicateInnerBlock('right', innerBlock.id)" class="p-0.5 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs" title="Duplicate"><i class="pi pi-copy" /></button>
                            <button @click.stop="removeInnerBlock('right', innerBlock.id)" class="p-0.5 bg-slate-700 hover:bg-red-900 rounded text-slate-300 hover:text-red-300 text-xs" title="Remove"><i class="pi pi-times" /></button>
                        </div>
                        <component
                            :is="blockComponents[innerBlock.type]"
                            v-bind="innerBlockProps(innerBlock)"
                            @update:content="(c: unknown) => updateInnerContent('right', innerBlock.id, c)"
                        />
                    </div>
                </template>
            </draggable>

            <!-- Add block button -->
            <div class="col-add-btn-wrap" @click.stop>
                <button v-if="!showPickerRight" @click.stop="showPickerRight = true" class="col-add-btn">
                    <i class="pi pi-plus text-xs" /> Add Block
                </button>
                <div v-else class="col-mini-palette" @click.stop>
                    <button
                        v-for="item in miniPalette"
                        :key="item.type"
                        @click.stop="addBlock('right', item.type); showPickerRight = false"
                        class="col-palette-item"
                    >
                        <i :class="item.icon" class="text-xs text-slate-500 w-3" />
                        {{ item.label }}
                    </button>
                    <button @click.stop="showPickerRight = false" class="col-palette-close">Cancel</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, defineAsyncComponent } from 'vue';
import draggable from 'vuedraggable';
import type { ReportBlock, ColumnRowBlock, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: ColumnRowBlock['content'];
    style: BlockStyle;
    isEdited: boolean;
    selectedInnerBlockId?: string | null;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: ColumnRowBlock['content']): void;
    (e: 'select-inner', block: ReportBlock): void;
}>();

// ── Column blocks as writable computed (sync with content) ────────────────────

const leftBlocks = computed({
    get: () => props.content.leftBlocks,
    set: (val) => emit('update:content', { ...props.content, leftBlocks: val }),
});

const rightBlocks = computed({
    get: () => props.content.rightBlocks,
    set: (val) => emit('update:content', { ...props.content, rightBlocks: val }),
});

// ── Grid layout ───────────────────────────────────────────────────────────────

const gridStyle = computed(() => {
    const ratioMap: Record<string, string> = {
        '30/70': '3fr 7fr',
        '50/50': '1fr 1fr',
        '60/40': '3fr 2fr',
        '40/60': '2fr 3fr',
        '70/30': '7fr 3fr',
    };
    const gapMap: Record<string, string> = { none: '0', sm: '8px', md: '16px', lg: '24px' };
    return {
        gridTemplateColumns: ratioMap[props.content.ratio] ?? '1fr 1fr',
        gap: gapMap[props.content.gap] ?? '16px',
    };
});

// ── Block component registry (same as canvas, no column-row to prevent nesting) ──

const blockComponents: Record<string, ReturnType<typeof defineAsyncComponent>> = {
    'cover':        defineAsyncComponent(() => import('./CoverBlock.vue')),
    'heading':      defineAsyncComponent(() => import('./HeadingBlock.vue')),
    'kpi-grid':     defineAsyncComponent(() => import('./KpiGridBlock.vue')),
    'chart-bar':    defineAsyncComponent(() => import('./BarChartBlock.vue')),
    'chart-line':   defineAsyncComponent(() => import('./LineChartBlock.vue')),
    'narrative':    defineAsyncComponent(() => import('./NarrativeBlock.vue')),
    'callout':      defineAsyncComponent(() => import('./CalloutBlock.vue')),
    'ca-table':     defineAsyncComponent(() => import('./CaTableBlock.vue')),
    'image':        defineAsyncComponent(() => import('./ImageBlock.vue')),
    'divider':      defineAsyncComponent(() => import('./DividerBlock.vue')),
    'spacer':       defineAsyncComponent(() => import('./SpacerBlock.vue')),
    'toc-sidebar':  defineAsyncComponent(() => import('./TocSidebarBlock.vue')),
    'oval-callout': defineAsyncComponent(() => import('./OvalCalloutBlock.vue')),
};

// ── Mini palette for the + Add Block picker ───────────────────────────────────

const miniPalette = [
    { type: 'heading',      label: 'Heading',      icon: 'pi pi-bars' },
    { type: 'narrative',    label: 'Narrative',     icon: 'pi pi-align-left' },
    { type: 'callout',      label: 'Callout',       icon: 'pi pi-info-circle' },
    { type: 'image',        label: 'Image',         icon: 'pi pi-image' },
    { type: 'chart-bar',    label: 'Bar Chart',     icon: 'pi pi-chart-bar' },
    { type: 'chart-line',   label: 'Trend Chart',   icon: 'pi pi-chart-line' },
    { type: 'kpi-grid',     label: 'KPI Cards',     icon: 'pi pi-th-large' },
    { type: 'toc-sidebar',  label: 'TOC / Inside',  icon: 'pi pi-bookmark' },
    { type: 'oval-callout', label: 'Oval Callout',  icon: 'pi pi-circle' },
    { type: 'divider',      label: 'Divider',       icon: 'pi pi-minus' },
    { type: 'spacer',       label: 'Spacer',        icon: 'pi pi-arrows-v' },
];

const showPickerLeft = ref(false);
const showPickerRight = ref(false);

// ── Helpers ───────────────────────────────────────────────────────────────────

function innerBlockProps(block: ReportBlock) {
    return { content: block.content, style: block.style, isEdited: block.isEdited };
}

function innerBlockStyle(block: ReportBlock) {
    const s = block.style;
    const parts: string[] = [];
    if (s?.backgroundColor && block.type !== 'cover') parts.push(`background-color: ${s.backgroundColor}`);
    if (s?.textColor) parts.push(`color: ${s.textColor}`);
    return parts.join('; ');
}

function updateInnerContent(side: 'left' | 'right', id: string, content: unknown) {
    const key = side === 'left' ? 'leftBlocks' : 'rightBlocks';
    const arr = props.content[key].map(b => b.id === id ? { ...b, content } as ReportBlock : b);
    emit('update:content', { ...props.content, [key]: arr });
}

function removeInnerBlock(side: 'left' | 'right', id: string) {
    const key = side === 'left' ? 'leftBlocks' : 'rightBlocks';
    emit('update:content', { ...props.content, [key]: props.content[key].filter(b => b.id !== id) });
}

function moveInnerBlock(side: 'left' | 'right', id: string, delta: -1 | 1) {
    const key = side === 'left' ? 'leftBlocks' : 'rightBlocks';
    const arr = [...props.content[key]];
    const idx = arr.findIndex(b => b.id === id);
    const newIdx = idx + delta;
    if (newIdx < 0 || newIdx >= arr.length) return;
    [arr[idx], arr[newIdx]] = [arr[newIdx], arr[idx]];
    emit('update:content', { ...props.content, [key]: arr });
}

function duplicateInnerBlock(side: 'left' | 'right', id: string) {
    const key = side === 'left' ? 'leftBlocks' : 'rightBlocks';
    const arr = [...props.content[key]];
    const idx = arr.findIndex(b => b.id === id);
    if (idx === -1) return;
    const clone = JSON.parse(JSON.stringify(arr[idx])) as ReportBlock;
    clone.id = crypto.randomUUID();
    arr.splice(idx + 1, 0, clone);
    emit('update:content', { ...props.content, [key]: arr });
}

function addBlock(side: 'left' | 'right', type: string) {
    const key = side === 'left' ? 'leftBlocks' : 'rightBlocks';
    const newBlock: ReportBlock = {
        id: crypto.randomUUID(),
        type: type as ReportBlock['type'],
        isEdited: false,
        style: {},
        content: getDefaultContent(type),
    } as ReportBlock;
    emit('update:content', { ...props.content, [key]: [...props.content[key], newBlock] });
}

function getDefaultContent(type: string): unknown {
    switch (type) {
        case 'heading':      return { text: 'New Heading', level: 2 };
        case 'narrative':    return { text: '' };
        case 'callout':      return { title: 'Note', body: '', variant: 'info' };
        case 'image':        return { url: '', alt: '', caption: '', width: 'full' };
        case 'divider':      return { thickness: 1, variant: 'solid', color: '#475569', marginY: 'md' };
        case 'spacer':       return { height: 'md' };
        case 'toc-sidebar':  return { title: 'INSIDE', items: [], darkBackground: true };
        case 'oval-callout': return { title: 'strong-hold', phonetic: "/'strôNG.hōld/ noun.", body: 'A place where a particular cause or belief is strongly defended or upheld.', backgroundColor: '#1e3a5f', textColor: '#ffffff' };
        case 'chart-bar':    return { title: 'Bar Chart', labels: [], datasets: [], caption: '' };
        case 'chart-line':   return { sectionName: '', title: 'Trend Chart', labels: [], datasets: [], caption: '', examples: '' };
        case 'kpi-grid':     return { cards: [], showComparison: false };
        default:             return {};
    }
}
</script>

<style scoped>
.column-row-block {
    display: grid;
    width: 100%;
}

.column-slot {
    min-height: 80px;
    position: relative;
}

.column-slot--empty {
    border: 1px dashed rgba(100, 116, 139, 0.3);
    border-radius: 6px;
    padding: 8px;
}

.col-add-btn-wrap {
    margin-top: 4px;
}

.col-add-btn {
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 4px;
    padding: 5px 8px;
    border: 1px dashed rgba(100, 116, 139, 0.4);
    border-radius: 4px;
    font-size: 0.7rem;
    color: rgba(148, 163, 184, 0.6);
    background: transparent;
    cursor: pointer;
    transition: all 0.15s ease;
}

.col-add-btn:hover {
    border-color: rgba(99, 179, 237, 0.5);
    color: rgba(99, 179, 237, 0.8);
    background: rgba(99, 179, 237, 0.04);
}

.col-mini-palette {
    background: #1e293b;
    border: 1px solid #334155;
    border-radius: 6px;
    padding: 4px;
    display: flex;
    flex-direction: column;
    gap: 1px;
    max-height: 240px;
    overflow-y: auto;
}

.col-palette-item {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.72rem;
    color: #cbd5e1;
    text-align: left;
    background: transparent;
    cursor: pointer;
    transition: background 0.1s;
}

.col-palette-item:hover {
    background: #334155;
}

.col-palette-close {
    margin-top: 4px;
    padding: 3px 8px;
    border-radius: 4px;
    font-size: 0.7rem;
    color: #64748b;
    background: transparent;
    cursor: pointer;
    text-align: center;
    border: 1px solid #334155;
}

.col-palette-close:hover {
    color: #94a3b8;
}

@media print {
    .col-add-btn-wrap { display: none; }
    .column-slot--empty { border: none; }
}
</style>

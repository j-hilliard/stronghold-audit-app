<template>
    <!-- Dark canvas area — document page floats inside it -->
    <div class="flex-1 overflow-y-auto composer-canvas py-8" style="background: #2a3142;">
        <!-- A4 document page (794 px wide = 210 mm @ 96 dpi) -->
        <div
            class="document-page mx-auto shadow-2xl print-page"
            style="width: 794px; min-height: 1123px; border-radius: 4px; padding: 40px 48px; background: #0f172a;"
        >
            <draggable
                v-model="localBlocks"
                handle=".drag-handle"
                item-key="id"
                animation="150"
                ghost-class="opacity-40"
                @end="onDragEnd"
            >
                <template #item="{ element: block }">
                    <div
                        class="relative group rounded-lg border transition-colors cursor-pointer print-block mb-3"
                        :class="[
                            selectedId === block.id
                                ? 'border-blue-400 ring-1 ring-blue-400/40'
                                : 'border-transparent hover:border-slate-600',
                            paddingClass(block.style?.padding)
                        ]"
                        :style="blockStyle(block)"
                        :data-testid="`composer-block-${block.id}`"
                        @click="$emit('select', block)"
                    >
                        <!-- Block controls (hover) — hidden for locked newsletter blocks -->
                        <div
                            v-if="!block.isNewsletterBlock"
                            class="absolute -top-3 right-2 hidden group-hover:flex items-center gap-1 z-10"
                        >
                            <!-- Drag handle -->
                            <span
                                class="drag-handle p-1 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs cursor-grab active:cursor-grabbing"
                                title="Drag to reorder"
                            ><i class="pi pi-bars" /></span>
                            <button
                                @click.stop="$emit('move-up', block.id)"
                                class="p-1 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs"
                                title="Move up"
                            ><i class="pi pi-chevron-up" /></button>
                            <button
                                @click.stop="$emit('move-down', block.id)"
                                class="p-1 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 text-xs"
                                title="Move down"
                            ><i class="pi pi-chevron-down" /></button>
                            <button
                                @click.stop="$emit('remove', block.id)"
                                class="p-1 bg-slate-700 hover:bg-red-900 rounded text-slate-300 hover:text-red-300 text-xs"
                                title="Remove block"
                            ><i class="pi pi-times" /></button>
                        </div>
                        <!-- Newsletter lock indicator -->
                        <div v-else class="absolute -top-3 right-2 hidden group-hover:flex items-center z-10">
                            <span class="px-1.5 py-0.5 bg-slate-700 rounded text-slate-300 text-xs flex items-center gap-1">
                                <i class="pi pi-lock text-xs" /> Newsletter block
                            </span>
                        </div>

                        <!-- Block renderer -->
                        <component
                            :is="componentFor(block.type)"
                            v-bind="blockProps(block)"
                            @update:content="(c: unknown) => $emit('update-content', block.id, c)"
                            @update:isEdited="(v: boolean) => $emit('update-is-edited', block.id, v)"
                        />
                    </div>
                </template>
            </draggable>

            <!-- Empty state -->
            <div
                v-if="!localBlocks.length"
                class="flex flex-col items-center justify-center py-24 text-slate-500 text-sm space-y-2"
            >
                <i class="pi pi-file-edit text-3xl" />
                <div>Add blocks from the toolbar on the left, or click Generate to build the report.</div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { defineAsyncComponent, computed, watch } from 'vue';
import draggable from 'vuedraggable';
import type { ReportBlock, BlockType } from '../types/report-block';

const props = defineProps<{
    blocks: ReportBlock[];
    selectedId: string | null;
}>();

const emit = defineEmits<{
    (e: 'select', block: ReportBlock): void;
    (e: 'remove', id: string): void;
    (e: 'move-up', id: string): void;
    (e: 'move-down', id: string): void;
    (e: 'update-content', id: string, content: unknown): void;
    (e: 'update-is-edited', id: string, value: boolean): void;
    (e: 'reorder', ids: string[]): void;
}>();

// Local copy for vuedraggable v-model — synced from props
const localBlocks = computed({
    get: () => props.blocks,
    set: (val) => emit('reorder', val.map(b => b.id)),
});

function onDragEnd() {
    // computed setter fires on v-model change; this is a no-op safety hook
}

// Keep localBlocks in sync when parent updates (e.g. after generate)
watch(() => props.blocks, () => {
    // computed getter always reflects props.blocks — no manual sync needed
});

const blockComponents: Record<BlockType, ReturnType<typeof defineAsyncComponent>> = {
    'cover':      defineAsyncComponent(() => import('./blocks/CoverBlock.vue')),
    'heading':    defineAsyncComponent(() => import('./blocks/HeadingBlock.vue')),
    'kpi-grid':   defineAsyncComponent(() => import('./blocks/KpiGridBlock.vue')),
    'chart-bar':  defineAsyncComponent(() => import('./blocks/BarChartBlock.vue')),
    'chart-line': defineAsyncComponent(() => import('./blocks/LineChartBlock.vue')),
    'narrative':  defineAsyncComponent(() => import('./blocks/NarrativeBlock.vue')),
    'callout':    defineAsyncComponent(() => import('./blocks/CalloutBlock.vue')),
    'ca-table':   defineAsyncComponent(() => import('./blocks/CaTableBlock.vue')),
    'image':      defineAsyncComponent(() => import('./blocks/ImageBlock.vue')),
};

function componentFor(type: BlockType) {
    return blockComponents[type];
}

function blockProps(block: ReportBlock) {
    return {
        content: block.content,
        style: block.style,
        isEdited: block.isEdited,
    };
}

function paddingClass(padding?: string) {
    switch (padding) {
        case 'none': return 'p-0';
        case 'sm':   return 'p-2';
        case 'lg':   return 'p-8';
        default:     return 'p-4';
    }
}

function blockStyle(block: ReportBlock) {
    const s = block.style;
    const parts: string[] = [];
    if (s?.backgroundColor && block.type !== 'cover') parts.push(`background-color: ${s.backgroundColor}`);
    if (s?.textColor) parts.push(`color: ${s.textColor}`);
    if (s?.borderColor) parts.push(`border-color: ${s.borderColor}`);
    return parts.join('; ');
}
</script>

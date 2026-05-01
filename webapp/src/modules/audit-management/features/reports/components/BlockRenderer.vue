<template>
    <div
        class="block-renderer"
        :class="paddingClass"
        :style="containerStyle"
    >
        <Suspense>
            <component
                :is="blockComponent"
                v-bind="blockProps"
                @update:content="(c: unknown) => $emit('update:content', c)"
                @update:isEdited="(v: boolean) => $emit('update:isEdited', v)"
            />
            <template #fallback>
                <div class="flex items-center justify-center h-12 text-slate-500 text-xs gap-2">
                    <i class="pi pi-spin pi-spinner text-xs" /> Loading…
                </div>
            </template>
        </Suspense>
    </div>
</template>

<script setup lang="ts">
import { computed, defineAsyncComponent } from 'vue';
import type { ReportBlock, BlockType } from '../types/report-block';

const props = defineProps<{
    block: ReportBlock;
    editMode?: boolean;
}>();

defineEmits<{
    (e: 'update:content', content: unknown): void;
    (e: 'update:isEdited', value: boolean): void;
}>();

// ── Async component registry — same as ComposerCanvas ────────────────────────

const REGISTRY: Record<BlockType, ReturnType<typeof defineAsyncComponent>> = {
    'cover':             defineAsyncComponent(() => import('./blocks/CoverBlock.vue')),
    'cover-page':        defineAsyncComponent(() => import('./blocks/CoverPageBlock.vue')),
    'heading':           defineAsyncComponent(() => import('./blocks/HeadingBlock.vue')),
    'kpi-grid':          defineAsyncComponent(() => import('./blocks/KpiGridBlock.vue')),
    'chart-bar':         defineAsyncComponent(() => import('./blocks/BarChartBlock.vue')),
    'chart-line':        defineAsyncComponent(() => import('./blocks/LineChartBlock.vue')),
    'narrative':         defineAsyncComponent(() => import('./blocks/NarrativeBlock.vue')),
    'callout':           defineAsyncComponent(() => import('./blocks/CalloutBlock.vue')),
    'ca-table':          defineAsyncComponent(() => import('./blocks/CaTableBlock.vue')),
    'image':             defineAsyncComponent(() => import('./blocks/ImageBlock.vue')),
    'column-row':        defineAsyncComponent(() => import('./blocks/ColumnRowBlock.vue')),
    'divider':           defineAsyncComponent(() => import('./blocks/DividerBlock.vue')),
    'spacer':            defineAsyncComponent(() => import('./blocks/SpacerBlock.vue')),
    'toc-sidebar':       defineAsyncComponent(() => import('./blocks/TocSidebarBlock.vue')),
    'oval-callout':      defineAsyncComponent(() => import('./blocks/OvalCalloutBlock.vue')),
    'findings-category': defineAsyncComponent(() => import('./blocks/FindingsCategoryBlock.vue')),
};

const blockComponent = computed(() => REGISTRY[props.block.type]);

// Props forwarded to each block component — identical to ComposerCanvas.blockProps()
const blockProps = computed(() => ({
    content:  props.block.content,
    style:    props.block.style,
    isEdited: props.block.isEdited,
}));

// ── Visual container — applies BlockStyle to the wrapper div ─────────────────
// (keeps block components ignorant of their outer container color)

const containerStyle = computed(() => {
    const s = props.block.style;
    const style: Record<string, string> = {};
    if (s?.backgroundColor && props.block.type !== 'cover' && props.block.type !== 'cover-page') {
        style.backgroundColor = s.backgroundColor;
    }
    if (s?.textColor)   style.color       = s.textColor;
    if (s?.borderColor) style.borderColor = s.borderColor;
    return style;
});

const paddingClass = computed(() => {
    if (props.block.type === 'cover' || props.block.type === 'cover-page') return 'p-0';
    switch (props.block.style?.padding) {
        case 'none': return 'p-0';
        case 'sm':   return 'p-2';
        case 'lg':   return 'p-8';
        default:     return 'p-4';
    }
});
</script>

<style scoped>
.block-renderer {
    width: 100%;
    box-sizing: border-box;
}
</style>

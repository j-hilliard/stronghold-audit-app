<template>
    <div :class="marginClass">
        <hr :class="hrClass" :style="hrStyle" />
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { DividerContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: DividerContent;
    style: BlockStyle;
    isEdited: boolean;
}>();

const marginClass = computed(() => {
    const map: Record<string, string> = { sm: 'my-2', md: 'my-4', lg: 'my-8' };
    return map[props.content.marginY] ?? 'my-4';
});

const hrClass = computed(() => {
    const thicknessMap: Record<number, string> = { 1: 'border-t', 2: 'border-t-2', 4: 'border-t-4' };
    const variantMap: Record<string, string> = { solid: 'border-solid', dashed: 'border-dashed', dotted: 'border-dotted' };
    return [
        thicknessMap[props.content.thickness] ?? 'border-t',
        variantMap[props.content.variant] ?? 'border-solid',
        'w-full',
    ];
});

const hrStyle = computed(() => ({
    borderColor: props.content.color || '#475569',
}));
</script>

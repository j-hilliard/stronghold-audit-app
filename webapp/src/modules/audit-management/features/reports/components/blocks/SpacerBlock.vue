<template>
    <div :class="heightClass" class="spacer-block w-full" />
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { SpacerContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: SpacerContent;
    style: BlockStyle;
    isEdited: boolean;
}>();

const heightClass = computed(() => {
    const map: Record<string, string> = { xs: 'h-2', sm: 'h-4', md: 'h-8', lg: 'h-12', xl: 'h-16' };
    return map[props.content.height] ?? 'h-8';
});
</script>

<style scoped>
/* Dotted outline visible in editor, hidden in print */
.spacer-block {
    border: 1px dashed rgba(100, 116, 139, 0.35);
    border-radius: 2px;
    position: relative;
}
.spacer-block::after {
    content: 'Spacer';
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    font-size: 10px;
    color: rgba(100, 116, 139, 0.5);
    letter-spacing: 0.08em;
    text-transform: uppercase;
    pointer-events: none;
}
@media print {
    .spacer-block { border: none; }
    .spacer-block::after { display: none; }
}
</style>

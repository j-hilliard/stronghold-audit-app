<template>
    <!-- BlockStyle.backgroundColor is intentionally unused here.
         Visual identity comes from content.primaryColor and content.backgroundImageUrl. -->
    <div
        class="relative rounded-xl overflow-hidden min-h-48 flex flex-col justify-end p-8 print:rounded-none print:min-h-64"
        :style="bgStyle"
    >
        <!-- overlay -->
        <div class="absolute inset-0 bg-black/30" />
        <div class="relative z-10 space-y-1">
            <div class="text-xs font-semibold uppercase tracking-widest" :style="accentStyle">
                {{ content.divisionCode }} Compliance Report
            </div>
            <h1 class="text-3xl font-bold text-white">{{ content.divisionName }}</h1>
            <div class="text-lg font-medium text-white/80">{{ content.period }}</div>
            <div class="text-xs text-white/50 pt-2">Prepared by {{ content.preparedBy }}</div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { CoverContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: CoverContent;
    style: BlockStyle;
}>();

const bgStyle = computed(() => {
    const image = props.content.backgroundImageUrl
        ? `url('${props.content.backgroundImageUrl}') center/cover no-repeat, `
        : '';
    return `background: ${image}${props.content.primaryColor};`;
});

// accentColor from BlockStyle if provided, otherwise derive from primaryColor
const accentStyle = computed(() => {
    const color = props.style.accentColor ?? '#f59e0b';
    return `color: ${color};`;
});
</script>

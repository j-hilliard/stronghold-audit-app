<template>
    <div class="rounded-lg border-l-4 p-4" :class="containerClass">
        <div class="flex items-start gap-2">
            <i :class="iconClass" class="mt-0.5 shrink-0" />
            <div class="flex-1 space-y-1">
                <div class="text-sm font-semibold" :class="titleClass">{{ content.title }}</div>
                <div class="text-sm" :class="bodyClass">{{ content.body }}</div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { CalloutContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: CalloutContent;
    style: BlockStyle;
}>();

const containerClass = computed(() => ({
    'bg-blue-900/20 border-blue-500': props.content.variant === 'info',
    'bg-amber-900/20 border-amber-500': props.content.variant === 'warning',
    'bg-emerald-900/20 border-emerald-500': props.content.variant === 'success',
    'bg-red-900/20 border-red-500': props.content.variant === 'danger',
}));

const iconClass = computed(() => ({
    'pi pi-info-circle text-blue-400': props.content.variant === 'info',
    'pi pi-exclamation-triangle text-amber-400': props.content.variant === 'warning',
    'pi pi-check-circle text-emerald-400': props.content.variant === 'success',
    'pi pi-times-circle text-red-400': props.content.variant === 'danger',
}));

const titleClass = computed(() => ({
    'text-blue-300': props.content.variant === 'info',
    'text-amber-300': props.content.variant === 'warning',
    'text-emerald-300': props.content.variant === 'success',
    'text-red-300': props.content.variant === 'danger',
}));

const bodyClass = computed(() => ({
    'text-blue-200/80': props.content.variant === 'info',
    'text-amber-200/80': props.content.variant === 'warning',
    'text-emerald-200/80': props.content.variant === 'success',
    'text-red-200/80': props.content.variant === 'danger',
}));
</script>

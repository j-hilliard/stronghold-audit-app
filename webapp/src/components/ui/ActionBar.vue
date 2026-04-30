<template>
    <div :class="[
        'flex flex-wrap items-center gap-2',
        sticky
            ? 'sticky top-0 z-20 px-4 py-2.5 bg-surface-2/95 backdrop-blur-sm border-b border-slate-700/60 shadow-elevation-1'
            : 'px-4 py-2',
    ]">
        <div v-if="title" class="flex items-center gap-2 flex-1 min-w-0">
            <span class="text-sm font-semibold text-slate-200 truncate">{{ title }}</span>
        </div>
        <div :class="['flex flex-wrap items-center gap-2', title ? '' : 'w-full']">
            <slot />
        </div>
    </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
    title?: string;
    sticky?: boolean;
}>(), {
    sticky: false,
});
</script>

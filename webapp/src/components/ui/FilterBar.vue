<template>
    <div class="flex flex-wrap items-end gap-3 px-4 py-3 border-b border-slate-700/40 bg-surface-1/50">
        <slot />
        <div v-if="$slots.actions" class="flex items-center gap-2 ml-auto">
            <slot name="actions" />
        </div>
    </div>
</template>

<script setup lang="ts">
defineProps<{ label?: string }>();
</script>

<template>
    <div class="flex flex-col md:flex-row md:items-center md:justify-between px-4 pt-4 pb-3 mb-2 gap-3 border-b border-slate-700/50">
        <div class="flex items-center gap-3">
            <div v-if="icon" class="flex-shrink-0 w-10 h-10 rounded-lg bg-blue-500/10 border border-blue-500/20 flex items-center justify-center">
                <i :class="[icon, 'text-blue-400 text-lg']" />
            </div>
            <div>
                <h1 class="text-xl font-bold text-white leading-tight">{{ title }}</h1>
                <p v-if="subtitle" class="text-sm text-slate-400 mt-0.5">{{ subtitle }}</p>
            </div>
        </div>
        <div class="flex flex-wrap items-center gap-2 md:mt-0 mt-1 min-w-0">
            <slot />
        </div>
    </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
    icon?: string;
    title?: string;
    subtitle?: string;
}>(), {
    icon: '',
    title: '',
    subtitle: '',
});
</script>

<template>
    <div :class="['rounded-card border border-slate-700/60 bg-surface-2 overflow-hidden', elevated ? 'shadow-elevation-2' : 'shadow-elevation-1', $attrs.class]">
        <!-- Header -->
        <div v-if="title || $slots.header" class="flex items-center justify-between px-4 py-3 border-b border-slate-700/50">
            <div class="flex items-center gap-2">
                <i v-if="icon" :class="[icon, 'text-slate-400 text-sm']" />
                <span class="text-sm font-semibold text-slate-200">{{ title }}</span>
            </div>
            <slot name="header" />
        </div>
        <!-- Content -->
        <div :class="padded ? 'p-4' : ''">
            <slot />
        </div>
        <!-- Footer -->
        <div v-if="$slots.footer" class="px-4 py-3 border-t border-slate-700/40 bg-slate-900/30">
            <slot name="footer" />
        </div>
    </div>
</template>

<script setup lang="ts">
defineOptions({ inheritAttrs: false });

withDefaults(defineProps<{
    title?: string;
    icon?: string;
    elevated?: boolean;
    padded?: boolean;
}>(), {
    elevated: false,
    padded: true,
});
</script>

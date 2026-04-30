<template>
    <div :class="['flex flex-col items-center justify-center text-center py-16 px-6', $attrs.class]">
        <!-- Icon ring -->
        <div :class="['w-16 h-16 rounded-full flex items-center justify-center mb-4 border', iconBg, iconBorder]">
            <i :class="[resolvedIcon, 'text-2xl', iconColor]" />
        </div>

        <!-- Text -->
        <h3 class="text-base font-semibold text-slate-200 mb-1">{{ title }}</h3>
        <p class="text-sm text-slate-400 max-w-xs leading-relaxed">{{ message }}</p>

        <!-- Action slot -->
        <div v-if="$slots.action" class="mt-5">
            <slot name="action" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{
    icon?: string;
    title?: string;
    message?: string;
    variant?: 'default' | 'search' | 'error';
}>(), {
    variant: 'default',
    title: 'Nothing here yet',
    message: 'No data to display.',
});

defineOptions({ inheritAttrs: false });

const resolvedIcon = computed(() => {
    if (props.icon) return props.icon;
    if (props.variant === 'search') return 'pi pi-search';
    if (props.variant === 'error') return 'pi pi-exclamation-circle';
    return 'pi pi-inbox';
});

const iconBg     = computed(() => props.variant === 'error' ? 'bg-red-950/50' : 'bg-slate-800');
const iconBorder = computed(() => props.variant === 'error' ? 'border-red-700/50' : 'border-slate-700/50');
const iconColor  = computed(() => props.variant === 'error' ? 'text-red-400' : 'text-slate-500');
</script>

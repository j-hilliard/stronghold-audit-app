<template>
    <div :class="['flex items-start gap-3 rounded-lg border-l-4 px-4 py-3.5', variantClasses.bg, variantClasses.border, variantClasses.leftBorder]">
        <i :class="[resolvedIcon, 'text-lg mt-0.5 flex-shrink-0', variantClasses.icon]" />
        <div class="flex-1 min-w-0">
            <p v-if="title" :class="['font-semibold text-sm mb-0.5', variantClasses.title]">{{ title }}</p>
            <p :class="['text-sm leading-relaxed', variantClasses.message]">
                <slot>{{ message }}</slot>
            </p>
        </div>
        <button v-if="dismissible" @click="$emit('dismiss')"
            class="flex-shrink-0 text-slate-500 hover:text-slate-300 transition-colors ml-1 -mr-1">
            <i class="pi pi-times text-xs" />
        </button>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{
    variant?: 'info' | 'success' | 'warning' | 'danger';
    title?: string;
    message?: string;
    icon?: string;
    dismissible?: boolean;
}>(), {
    variant: 'info',
    dismissible: false,
});

defineEmits<{ dismiss: [] }>();

const ICON_MAP = { info: 'pi pi-info-circle', success: 'pi pi-check-circle', warning: 'pi pi-exclamation-triangle', danger: 'pi pi-times-circle' };
const resolvedIcon = computed(() => props.icon ?? ICON_MAP[props.variant]);

const variantClasses = computed(() => {
    const map = {
        info: {
            bg: 'bg-blue-950/50',
            border: 'border-blue-700/50',
            leftBorder: 'border-l-blue-500',
            icon: 'text-blue-400',
            title: 'text-blue-200',
            message: 'text-blue-300/90',
        },
        success: {
            bg: 'bg-emerald-950/50',
            border: 'border-emerald-700/50',
            leftBorder: 'border-l-emerald-500',
            icon: 'text-emerald-400',
            title: 'text-emerald-200',
            message: 'text-emerald-300/90',
        },
        warning: {
            bg: 'bg-amber-950/50',
            border: 'border-amber-700/50',
            leftBorder: 'border-l-amber-500',
            icon: 'text-amber-400',
            title: 'text-amber-200',
            message: 'text-amber-300/90',
        },
        danger: {
            bg: 'bg-red-950/60',
            border: 'border-red-700/50',
            leftBorder: 'border-l-red-500',
            icon: 'text-red-400',
            title: 'text-red-200',
            message: 'text-red-300/90',
        },
    };
    return map[props.variant];
});
</script>

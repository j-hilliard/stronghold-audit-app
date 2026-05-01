<template>
    <div
        :class="[
            'relative rounded-card border p-4 transition-all duration-200',
            interactive ? 'cursor-pointer' : '',
            interactive && !active ? 'hover:-translate-y-1' : '',
            variantClasses.bg,
            active ? variantClasses.activeBorder : variantClasses.border,
            interactive && !active ? variantClasses.hover : '',
            active ? 'ring-1 ' + variantClasses.activeRing : '',
        ]"
        v-bind="interactive ? { role: 'button', tabindex: '0' } : {}"
        @click="interactive && $emit('click', $event)"
        @keydown.enter="interactive && $emit('click', $event)"
    >
        <slot name="top-right" />

        <!-- Icon -->
        <div v-if="icon" :class="['w-8 h-8 rounded-lg flex items-center justify-center mb-3', variantClasses.iconBg]">
            <i :class="[icon, 'text-sm', variantClasses.iconColor]" />
        </div>

        <!-- Value -->
        <div :class="['text-3xl font-bold leading-none', variantClasses.value]">
            {{ value ?? '—' }}
        </div>

        <!-- Label -->
        <div class="text-xs mt-1.5 font-medium" style="color: var(--text-secondary)">{{ label }}</div>

        <!-- Trend -->
        <div v-if="trend !== undefined && trend !== null" class="text-xs mt-1.5 font-medium"
            :class="trendPositive ? 'text-[color:var(--color-success)]' : 'text-[color:var(--color-danger)]'">
            <template v-if="trendLabel">{{ trendLabel }}</template>
            <template v-else>{{ trendPositive ? '↑' : '↓' }} {{ Math.abs(trend) }}</template>
        </div>

        <!-- Sublabel -->
        <div v-if="sublabel" class="text-xs mt-1" style="color: var(--text-muted)">{{ sublabel }}</div>

        <!-- Drill hint / active indicator -->
        <div v-if="interactive" class="absolute bottom-2 right-2.5 text-[10px] pointer-events-none transition-colors"
            :class="active ? variantClasses.iconColor : 'text-slate-600'">
            <i :class="active ? 'pi pi-check-circle' : 'pi pi-arrow-up-right'" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{
    label: string;
    value?: string | number | null;
    icon?: string;
    variant?: 'default' | 'success' | 'warning' | 'danger' | 'info';
    trend?: number | null;
    trendLabel?: string;
    trendPositiveDirection?: 'up' | 'down';
    sublabel?: string;
    interactive?: boolean;
    active?: boolean;
}>(), {
    variant: 'default',
    interactive: false,
    active: false,
    trendPositiveDirection: 'up',
});

defineEmits<{ click: [e: Event] }>();

const trendPositive = computed(() => {
    if (props.trend === undefined || props.trend === null) return null;
    return props.trendPositiveDirection === 'up'
        ? props.trend >= 0
        : props.trend <= 0;
});

const variantClasses = computed(() => {
    const map = {
        default: {
            bg: 'bg-surface-2',
            border: 'border-slate-700/60',
            activeBorder: 'border-blue-500/60',
            activeRing: 'ring-blue-500/30',
            hover: 'hover:shadow-card-hover hover:border-blue-500/40',
            value: 'text-white',
            iconBg: 'bg-slate-700/60',
            iconColor: 'text-slate-300',
        },
        success: {
            bg: 'bg-emerald-950/50',
            border: 'border-emerald-800/50',
            activeBorder: 'border-emerald-500/70',
            activeRing: 'ring-emerald-500/30',
            hover: 'hover:shadow-glow-success hover:border-emerald-500/50',
            value: 'text-emerald-400',
            iconBg: 'bg-emerald-900/60',
            iconColor: 'text-emerald-400',
        },
        warning: {
            bg: 'bg-amber-950/40',
            border: 'border-amber-800/50',
            activeBorder: 'border-amber-500/70',
            activeRing: 'ring-amber-500/30',
            hover: 'hover:shadow-card-hover-warn hover:border-amber-500/50',
            value: 'text-amber-400',
            iconBg: 'bg-amber-900/60',
            iconColor: 'text-amber-400',
        },
        danger: {
            bg: 'bg-red-950/40',
            border: 'border-red-800/50',
            activeBorder: 'border-red-500/70',
            activeRing: 'ring-red-500/30',
            hover: 'hover:shadow-card-hover-danger hover:border-red-500/50',
            value: 'text-red-400',
            iconBg: 'bg-red-900/60',
            iconColor: 'text-red-400',
        },
        info: {
            bg: 'bg-blue-950/40',
            border: 'border-blue-800/50',
            activeBorder: 'border-blue-500/70',
            activeRing: 'ring-blue-500/30',
            hover: 'hover:shadow-glow-focus hover:border-blue-500/50',
            value: 'text-blue-400',
            iconBg: 'bg-blue-900/60',
            iconColor: 'text-blue-400',
        },
    };
    return map[props.variant];
});
</script>

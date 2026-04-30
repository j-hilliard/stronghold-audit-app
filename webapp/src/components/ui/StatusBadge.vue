<template>
    <span :class="['inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold border', classes]">
        <span v-if="dot" :class="['w-1.5 h-1.5 rounded-full', dotColor]" />
        {{ label ?? value }}
    </span>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{
    value: string;
    label?: string;
    dot?: boolean;
    map?: Record<string, { classes: string; dot?: string }>;
}>(), {
    dot: false,
});

const DEFAULT_MAP: Record<string, { classes: string; dot: string }> = {
    // Audit statuses
    Draft:      { classes: 'bg-amber-950/60 border-amber-700/60 text-amber-300',   dot: 'bg-amber-400' },
    Submitted:  { classes: 'bg-blue-950/60 border-blue-700/60 text-blue-300',      dot: 'bg-blue-400' },
    Reopened:   { classes: 'bg-orange-950/60 border-orange-700/60 text-orange-300', dot: 'bg-orange-400' },
    Closed:     { classes: 'bg-emerald-950/60 border-emerald-700/60 text-emerald-300', dot: 'bg-emerald-400' },
    // CA statuses
    Open:       { classes: 'bg-red-950/60 border-red-700/60 text-red-300',         dot: 'bg-red-400' },
    InProgress: { classes: 'bg-amber-950/60 border-amber-700/60 text-amber-300',   dot: 'bg-amber-400' },
    Voided:     { classes: 'bg-slate-800 border-slate-700 text-slate-400',         dot: 'bg-slate-500' },
    // Question statuses
    Conforming:    { classes: 'bg-emerald-950/60 border-emerald-700/60 text-emerald-300', dot: 'bg-emerald-400' },
    NonConforming: { classes: 'bg-red-950/60 border-red-700/60 text-red-300',            dot: 'bg-red-400' },
    Warning:       { classes: 'bg-amber-950/60 border-amber-700/60 text-amber-300',      dot: 'bg-amber-400' },
    NA:            { classes: 'bg-slate-800 border-slate-700 text-slate-400',            dot: 'bg-slate-500' },
};

const resolved = computed(() => {
    const lookup = props.map ?? DEFAULT_MAP;
    return lookup[props.value] ?? { classes: 'bg-slate-800 border-slate-700 text-slate-300', dot: 'bg-slate-400' };
});

const classes = computed(() => resolved.value.classes);
const dotColor = computed(() => (resolved.value as any).dot ?? 'bg-slate-400');
</script>

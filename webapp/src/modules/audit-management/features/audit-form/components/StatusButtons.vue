<template>
    <div class="flex gap-2 flex-wrap">
        <button
            v-for="btn in visibleButtons"
            :key="btn.value"
            type="button"
            :class="[
                'flex items-center justify-center gap-1.5 px-3 py-1.5 rounded text-sm font-semibold border-2 transition-all duration-150 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-offset-slate-900',
                modelValue === btn.value ? btn.activeClass : btn.inactiveClass,
            ]"
            :disabled="disabled"
            :aria-pressed="modelValue === btn.value"
            :aria-label="btn.label"
            @click="toggle(btn.value)"
        >
            <span class="leading-none">{{ btn.icon }}</span>
            <span>{{ btn.label }}</span>
        </button>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Button {
    value: string;
    label: string;
    icon: string;
    activeClass: string;
    inactiveClass: string;
}

const props = withDefaults(defineProps<{
    modelValue: string | null;
    allowNA?: boolean;
    disabled?: boolean;
}>(), {
    allowNA: true,
    disabled: false,
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: string | null): void;
}>();

const ALL_BUTTONS: Button[] = [
    {
        value: 'Conforming',
        label: 'Conforming',
        icon: '✓',
        activeClass:   'bg-emerald-600 border-emerald-400 text-white shadow-emerald-900/40 shadow-md',
        inactiveClass: 'bg-emerald-900/20 border-emerald-600 text-emerald-400 hover:bg-emerald-600/30 hover:border-emerald-400 hover:text-emerald-200',
    },
    {
        value: 'NonConforming',
        label: 'Non-Conforming',
        icon: '✗',
        activeClass:   'bg-red-600 border-red-400 text-white shadow-red-900/40 shadow-md',
        inactiveClass: 'bg-red-900/20 border-red-600 text-red-400 hover:bg-red-600/30 hover:border-red-400 hover:text-red-200',
    },
    {
        value: 'Warning',
        label: 'Warning',
        icon: '⚠',
        activeClass:   'bg-amber-500 border-amber-400 text-white shadow-amber-900/40 shadow-md',
        inactiveClass: 'bg-amber-900/20 border-amber-600 text-amber-400 hover:bg-amber-500/30 hover:border-amber-400 hover:text-amber-200',
    },
    {
        value: 'NA',
        label: 'N/A',
        icon: '—',
        activeClass:   'bg-slate-500 border-slate-300 text-white',
        inactiveClass: 'bg-slate-800/40 border-slate-500 text-slate-400 hover:bg-slate-500/30 hover:border-slate-300 hover:text-slate-200',
    },
];

const visibleButtons = computed(() =>
    props.allowNA ? ALL_BUTTONS : ALL_BUTTONS.filter(b => b.value !== 'NA'),
);

function toggle(value: string) {
    emit('update:modelValue', props.modelValue === value ? null : value);
}
</script>

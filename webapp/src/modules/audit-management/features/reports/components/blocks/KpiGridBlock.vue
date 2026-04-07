<template>
    <div class="space-y-3">
        <!-- Comparison header labels -->
        <div v-if="showComparison && enabledCards.length" class="flex justify-end gap-4 px-1">
            <span class="text-xs text-blue-400 font-medium uppercase tracking-wide">Division</span>
            <span class="text-xs text-slate-400 font-medium uppercase tracking-wide">Company</span>
        </div>

        <div class="grid grid-cols-2 gap-3 sm:grid-cols-3">
            <div
                v-for="card in enabledCards"
                :key="card.label"
                class="rounded-lg p-3 border flex flex-col gap-1"
                :class="cardClass(card.variant)"
            >
                <div class="text-xs text-slate-400 uppercase tracking-wide leading-tight">{{ card.label }}</div>

                <div v-if="content.showComparison && card.companyValue !== undefined" class="flex items-end gap-2">
                    <div class="flex-1">
                        <span class="text-xl font-bold" :class="valueClass(card.variant)">{{ card.value }}</span>
                        <span v-if="card.unit" class="text-xs font-normal ml-0.5 text-slate-400">{{ card.unit }}</span>
                    </div>
                    <div class="text-right">
                        <span class="text-sm font-medium text-slate-400">{{ card.companyValue }}</span>
                        <span v-if="card.unit" class="text-xs font-normal ml-0.5 text-slate-500">{{ card.unit }}</span>
                    </div>
                </div>
                <div v-else>
                    <span class="text-2xl font-bold" :class="valueClass(card.variant)">{{ card.value }}</span>
                    <span v-if="card.unit" class="text-sm font-normal ml-1">{{ card.unit }}</span>
                </div>

                <div v-if="card.delta" class="text-xs text-slate-500">{{ card.delta }}</div>
            </div>
        </div>

        <div v-if="!enabledCards.length" class="text-center text-slate-500 text-sm py-6">
            No cards enabled — select cards in the property panel.
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { KpiGridContent, BlockStyle, KpiCard } from '../../types/report-block';

const props = defineProps<{
    content: KpiGridContent;
    style: BlockStyle;
}>();

const enabledCards = computed(() => props.content.cards.filter(c => c.enabled));
const showComparison = computed(() => props.content.showComparison);

function cardClass(variant: KpiCard['variant']) {
    return {
        'bg-slate-800 border-slate-700': variant === 'neutral',
        'bg-emerald-900/30 border-emerald-700/50': variant === 'good',
        'bg-amber-900/30 border-amber-700/50': variant === 'warn',
        'bg-red-900/30 border-red-700/50': variant === 'bad',
    };
}

function valueClass(variant: KpiCard['variant']) {
    return {
        'text-slate-200': variant === 'neutral',
        'text-emerald-400': variant === 'good',
        'text-amber-400': variant === 'warn',
        'text-red-400': variant === 'bad',
    };
}
</script>

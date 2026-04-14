<template>
    <div class="flex flex-col gap-2" style="height: 100%; min-height: 320px;">
        <div class="text-sm font-semibold text-slate-200 flex-shrink-0">{{ content.title }}</div>
        <div v-if="content.labels.length" class="flex-1 min-h-0" style="min-height: 160px;">
            <Chart type="line" :data="chartData" :options="chartOptions" style="height: 100%; width: 100%;" />
        </div>
        <div v-else class="flex-1 flex items-center justify-center text-slate-500 text-sm">
            No trend data available
        </div>
        <p v-if="content.caption" class="text-xs text-slate-400 italic">{{ content.caption }}</p>

        <!-- Examples / Findings box — matches the legacy newsletter "Examples:" sections -->
        <div class="flex-shrink-0 mt-3 border-t border-slate-700/60 pt-3">
            <div class="flex items-center gap-2 mb-1">
                <span class="text-xs font-semibold text-slate-400 uppercase tracking-wide">Examples</span>
                <button
                    v-if="!editing"
                    @click.stop="editing = true"
                    class="text-xs text-blue-400 hover:text-blue-300"
                >Edit</button>
                <button
                    v-else
                    @click.stop="editing = false"
                    class="text-xs text-blue-400 hover:text-blue-300"
                >Done</button>
            </div>
            <textarea
                v-if="editing"
                v-model="localExamples"
                rows="4"
                class="w-full bg-slate-700 border border-slate-600 rounded px-3 py-2 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500 resize-y"
                placeholder="• Finding example 1&#10;• Finding example 2&#10;• Finding example 3"
                @input="onInput"
            />
            <div
                v-else
                class="text-sm text-slate-300 leading-relaxed whitespace-pre-wrap cursor-text rounded p-2 hover:bg-slate-700/50 transition-colors min-h-8"
                :class="{ 'text-slate-500 italic': !content.examples }"
                @click.stop="editing = true"
            >
                {{ content.examples || 'Click to add finding examples for this section…' }}
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import Chart from 'primevue/chart';
import type { LineChartContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: LineChartContent;
    style: BlockStyle;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: LineChartContent): void;
}>();

const editing = ref(false);
const localExamples = ref(props.content.examples ?? '');

watch(() => props.content.examples, (v) => { localExamples.value = v ?? ''; });

function onInput() {
    emit('update:content', { ...props.content, examples: localExamples.value });
}

const chartData = computed(() => ({
    labels: props.content.labels,
    datasets: props.content.datasets.map(ds => ({
        label: ds.label,
        data: ds.data,
        borderColor: ds.borderColor,
        borderDash: ds.borderDash,
        backgroundColor: 'transparent',
        pointRadius: 3,
        tension: 0.3,
    })),
}));

const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: {
            display: true,
            labels: { color: '#94a3b8', font: { size: 10 } },
        },
    },
    scales: {
        x: { ticks: { color: '#94a3b8', font: { size: 10 } }, grid: { color: 'rgba(148,163,184,0.1)' } },
        y: { ticks: { color: '#94a3b8', font: { size: 10 } }, grid: { color: 'rgba(148,163,184,0.1)' }, beginAtZero: true },
    },
};
</script>

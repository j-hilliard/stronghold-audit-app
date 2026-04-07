<template>
    <div class="space-y-2">
        <div class="text-sm font-semibold text-slate-200">{{ content.title }}</div>
        <div v-if="content.labels.length">
            <Chart type="bar" :data="chartData" :options="chartOptions" style="height: 220px;" />
        </div>
        <div v-else class="flex items-center justify-center h-32 text-slate-500 text-sm">
            No section data available
        </div>
        <p v-if="content.caption" class="text-xs text-slate-400 italic">{{ content.caption }}</p>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import Chart from 'primevue/chart';
import type { BarChartContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: BarChartContent;
    style: BlockStyle;
}>();

const chartData = computed(() => ({
    labels: props.content.labels,
    datasets: props.content.datasets.map(ds => ({
        label: ds.label,
        data: ds.data,
        backgroundColor: ds.backgroundColor ?? 'rgba(59,130,246,0.75)',
        borderRadius: 4,
    })),
}));

const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: { display: false },
    },
    scales: {
        x: { ticks: { color: '#94a3b8', font: { size: 10 } }, grid: { color: 'rgba(148,163,184,0.1)' } },
        y: { ticks: { color: '#94a3b8', font: { size: 10 } }, grid: { color: 'rgba(148,163,184,0.1)' }, beginAtZero: true },
    },
};
</script>

<template>
    <div class="flex flex-col gap-2" style="height: 100%; min-height: 260px;">
        <div class="text-sm font-semibold text-slate-200 flex-shrink-0">{{ content.title }}</div>
        <div v-if="content.labels.length" class="flex-1 min-h-0" style="min-height: 180px;">
            <Chart type="bar" :data="chartData" :options="chartOptions" style="height: 100%; width: 100%;" />
        </div>
        <div v-else class="flex-1 flex items-center justify-center text-slate-500 text-sm">
            No section data available
        </div>
        <p v-if="content.caption" class="text-xs text-slate-400 italic flex-shrink-0">{{ content.caption }}</p>
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

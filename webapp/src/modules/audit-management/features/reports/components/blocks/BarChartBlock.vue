<template>
    <div class="flex flex-col gap-2" style="height: 100%; min-height: 260px;">
        <div class="text-sm font-semibold text-slate-200 flex-shrink-0">{{ content.title }}</div>
        <div v-if="content.labels.length" class="flex-1 min-h-0" style="min-height: 180px;">
            <canvas ref="canvasEl" class="h-full w-full block" />
        </div>
        <div v-else class="flex-1 flex items-center justify-center text-slate-500 text-sm">
            No section data available
        </div>
        <p v-if="content.caption" class="text-xs text-slate-400 italic flex-shrink-0">{{ content.caption }}</p>
    </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import {
    Chart as ChartJS,
    BarController,
    BarElement,
    CategoryScale,
    LinearScale,
    Legend,
    Tooltip,
    type ChartData,
    type ChartOptions,
} from 'chart.js';
import type { BarChartContent, BlockStyle } from '../../types/report-block';

ChartJS.register(BarController, BarElement, CategoryScale, LinearScale, Legend, Tooltip);

const props = defineProps<{
    content: BarChartContent;
    style: BlockStyle;
}>();

const canvasEl = ref<HTMLCanvasElement | null>(null);
let chart: ChartJS<'bar'> | null = null;

const chartData = computed<ChartData<'bar'>>(() => ({
    labels: props.content.labels,
    datasets: props.content.datasets.map(ds => ({
        label: ds.label,
        data: ds.data,
        backgroundColor: ds.backgroundColor ?? 'rgba(59,130,246,0.75)',
        borderRadius: 4,
    })),
}));

const chartOptions: ChartOptions<'bar'> = {
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

function destroyChart() {
    if (chart) {
        chart.destroy();
        chart = null;
    }
}

function renderChart() {
    if (!canvasEl.value || props.content.labels.length === 0) return;
    destroyChart();
    chart = new ChartJS(canvasEl.value, {
        type: 'bar',
        data: chartData.value,
        options: chartOptions,
    });
}

watch(
    () => props.content,
    async () => {
        await nextTick();
        renderChart();
    },
    { deep: true },
);

onMounted(async () => {
    await nextTick();
    renderChart();
});

onBeforeUnmount(() => {
    destroyChart();
});
</script>

<template>
    <div class="report-section-trend">
        <div class="section-heading">
            <i class="pi pi-chart-line text-blue-400" />
            <span>Conformance Trend</span>
        </div>

        <div v-if="trends.length === 0" class="py-6 text-center text-slate-500 text-sm">
            No trend data — click Generate to load.
        </div>

        <div v-else>
            <Chart type="line" :data="chartData" :options="chartOptions" class="w-full" style="height:220px" />
            <div class="flex items-center gap-4 mt-3 justify-center">
                <div class="flex items-center gap-1.5">
                    <div class="w-3 h-0.5 rounded" :style="{ background: chart.division }" />
                    <span class="text-xs text-slate-400">Division</span>
                </div>
                <div class="flex items-center gap-1.5 opacity-60">
                    <div class="w-3 h-0.5 rounded" :style="{ background: chart.company }" />
                    <span class="text-xs text-slate-400">Company avg</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import Chart from 'primevue/chart';
import { chart, makeChartDefaults } from '@/design-system';
import type { ReportTrendPoint } from '../../composables/useReportBuilder';

const props = defineProps<{ trends: ReportTrendPoint[] }>();

const chartData = computed(() => ({
    labels: props.trends.map(t => t.quarter),
    datasets: [
        {
            label: 'Division NC/Audit',
            data: props.trends.map(t => +t.divisionNcRate.toFixed(2)),
            borderColor: chart.division,
            backgroundColor: chart.divisionFill,
            tension: 0.3,
            fill: true,
            pointRadius: 4,
        },
        {
            label: 'Company NC/Audit',
            data: props.trends.map(t => t.companyNcRate != null ? +t.companyNcRate.toFixed(2) : null),
            borderColor: chart.company,
            borderDash: chart.companyDash,
            tension: 0.3,
            fill: false,
            pointRadius: 3,
        },
    ],
}));

const chartOptions = {
    ...makeChartDefaults(),
    scales: {
        ...makeChartDefaults().scales,
        y: {
            ...makeChartDefaults().scales.y,
            title: {
                display: true,
                text: 'NC / Audit',
                color: chart.axisTitle,
                font: { size: 11 },
            },
        },
    },
};
</script>

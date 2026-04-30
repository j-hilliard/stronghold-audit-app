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
                    <div class="w-3 h-0.5 bg-blue-400 rounded" />
                    <span class="text-xs text-slate-400">Division</span>
                </div>
                <div class="flex items-center gap-1.5">
                    <div class="w-3 h-0.5 bg-slate-500 rounded" style="border-top: 2px dashed #64748b" />
                    <span class="text-xs text-slate-400">Company</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import Chart from 'primevue/chart';
import type { ReportTrendPoint } from '../../composables/useReportBuilder';

const props = defineProps<{ trends: ReportTrendPoint[] }>();

const chartData = computed(() => ({
    labels: props.trends.map(t => t.quarter),
    datasets: [
        {
            label: 'Division NC/Audit',
            data: props.trends.map(t => +t.divisionNcRate.toFixed(2)),
            borderColor: '#3b82f6',
            backgroundColor: 'rgba(59,130,246,0.1)',
            tension: 0.3,
            fill: true,
            pointRadius: 4,
        },
        {
            label: 'Company NC/Audit',
            data: props.trends.map(t => t.companyNcRate != null ? +t.companyNcRate.toFixed(2) : null),
            borderColor: '#64748b',
            borderDash: [5, 5],
            tension: 0.3,
            fill: false,
            pointRadius: 3,
        },
    ],
}));

const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: { display: false },
        tooltip: { mode: 'index', intersect: false },
    },
    scales: {
        x: {
            grid: { color: 'rgba(148,163,184,0.1)' },
            ticks: { color: '#94a3b8', font: { size: 11 } },
        },
        y: {
            grid: { color: 'rgba(148,163,184,0.1)' },
            ticks: { color: '#94a3b8', font: { size: 11 } },
            title: { display: true, text: 'NC / Audit', color: '#64748b', font: { size: 11 } },
        },
    },
};
</script>

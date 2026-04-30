<template>
    <div class="report-section-kpis">
        <div class="section-heading">
            <i class="pi pi-chart-bar text-blue-400" />
            <span>Key Performance Indicators</span>
        </div>

        <div v-if="kpis.length === 0" class="py-6 text-center text-slate-500 text-sm">
            No data — click Generate to load KPIs.
        </div>

        <div v-else class="grid grid-cols-3 gap-3">
            <div
                v-for="kpi in kpis"
                :key="kpi.label"
                class="bg-slate-800 border rounded-xl p-4"
                :class="variantBorder(kpi.variant)"
            >
                <div class="text-xs text-slate-400 mb-1.5 font-medium uppercase tracking-wide">{{ kpi.label }}</div>
                <div class="text-2xl font-bold" :class="variantText(kpi.variant)">
                    {{ kpi.value }}
                    <span v-if="kpi.unit" class="text-xs font-normal text-slate-500 ml-1">{{ kpi.unit }}</span>
                </div>
                <div v-if="kpi.companyValue" class="text-xs text-slate-500 mt-1">
                    Company: {{ kpi.companyValue }}
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import type { ReportKpi } from '../../composables/useReportBuilder';

defineProps<{ kpis: ReportKpi[] }>();

function variantText(v: ReportKpi['variant']): string {
    if (v === 'good') return 'text-emerald-400';
    if (v === 'warn') return 'text-amber-400';
    if (v === 'bad')  return 'text-red-400';
    return 'text-white';
}

function variantBorder(v: ReportKpi['variant']): string {
    if (v === 'good') return 'border-emerald-700/50';
    if (v === 'warn') return 'border-amber-700/50';
    if (v === 'bad')  return 'border-red-700/50';
    return 'border-slate-700';
}
</script>

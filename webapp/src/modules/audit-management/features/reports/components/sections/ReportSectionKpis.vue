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
            <MetricCard
                v-for="kpi in kpis"
                :key="kpi.label"
                :label="kpi.label"
                :value="kpi.unit ? `${kpi.value}${kpi.unit}` : kpi.value"
                :variant="kpiVariantToSemantic(kpi.variant)"
                :sublabel="kpi.companyValue ? `Company: ${kpi.companyValue}` : undefined"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { MetricCard, kpiVariantToSemantic } from '@/design-system';
import type { ReportKpi } from '../../composables/useReportBuilder';

defineProps<{ kpis: ReportKpi[] }>();
</script>

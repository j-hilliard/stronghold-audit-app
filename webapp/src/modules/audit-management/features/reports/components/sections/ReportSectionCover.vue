<template>
    <div class="report-section-cover bg-slate-900 border border-slate-700 rounded-xl overflow-hidden">
        <div class="h-2 bg-gradient-to-r from-blue-600 to-blue-400" />
        <div class="p-8">
            <div class="flex items-start justify-between">
                <div>
                    <div class="text-xs font-semibold tracking-widest text-blue-400 uppercase mb-2">
                        {{ templateLabel }}
                    </div>
                    <h1 class="text-3xl font-bold text-white leading-tight">
                        {{ divisionCode }} — {{ divisionName }}
                    </h1>
                    <div class="text-lg text-slate-300 mt-1">{{ period || 'All Time' }}</div>
                    <div v-if="dateFrom || dateTo" class="text-sm text-slate-500 mt-1">
                        {{ dateFrom }} → {{ dateTo }}
                    </div>
                </div>
                <div class="text-right shrink-0 ml-6">
                    <div class="text-xs text-slate-500 uppercase tracking-wide">Prepared</div>
                    <div class="text-sm text-slate-300 mt-0.5">{{ today }}</div>
                </div>
            </div>
            <div class="mt-6 pt-5 border-t border-slate-700 flex items-center gap-2">
                <i class="pi pi-shield text-blue-400 text-sm" />
                <span class="text-xs text-slate-500 font-medium">Stronghold Compliance Audit System</span>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { ReportType } from '../../types/report-template';
import { REPORT_TEMPLATES } from '../../types/report-template';

const props = defineProps<{
    templateType: ReportType;
    divisionCode: string;
    divisionName: string;
    period: string;
    dateFrom: string | null;
    dateTo:   string | null;
}>();

const templateLabel = computed(() =>
    REPORT_TEMPLATES.find(t => t.type === props.templateType)?.label ?? 'Report',
);

const today = new Date().toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
</script>

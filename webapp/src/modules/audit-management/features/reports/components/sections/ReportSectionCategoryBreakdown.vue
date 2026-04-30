<template>
    <div class="report-section-category-breakdown">
        <div class="section-heading">
            <i class="pi pi-sitemap text-blue-400" />
            <span>Category Breakdown</span>
        </div>

        <div v-if="categories.length === 0" class="py-6 text-center text-slate-500 text-sm">
            No non-conformances found in this period.
        </div>

        <div v-else class="space-y-2">
            <div
                v-for="row in categories"
                :key="row.sectionName"
                class="flex items-center gap-3"
            >
                <div class="w-44 shrink-0 text-xs text-slate-300 truncate" :title="row.sectionName">
                    {{ row.sectionName }}
                </div>
                <div class="flex-1 h-5 bg-slate-800 rounded overflow-hidden">
                    <div
                        class="h-full rounded transition-all duration-500"
                        :class="barColor(row.variant)"
                        :style="{ width: `${Math.min(100, row.rate * 100)}%` }"
                    />
                </div>
                <div class="w-20 text-right shrink-0">
                    <span class="text-xs font-semibold" :class="textColor(row.variant)">
                        {{ row.ncCount }} NC
                    </span>
                    <span class="text-xs text-slate-500 ml-1">({{ (row.rate * 100).toFixed(0) }}%)</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import type { ReportCategoryRow } from '../../composables/useReportBuilder';

defineProps<{ categories: ReportCategoryRow[] }>();

function barColor(v: ReportCategoryRow['variant']): string {
    if (v === 'good') return 'bg-emerald-500/70';
    if (v === 'warn') return 'bg-amber-500/70';
    return 'bg-red-500/70';
}

function textColor(v: ReportCategoryRow['variant']): string {
    if (v === 'good') return 'text-emerald-400';
    if (v === 'warn') return 'text-amber-400';
    return 'text-red-400';
}
</script>

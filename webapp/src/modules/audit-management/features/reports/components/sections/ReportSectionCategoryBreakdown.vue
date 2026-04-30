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
                <div class="flex-1 h-4 rounded overflow-hidden" style="background: var(--surface-2);">
                    <div
                        class="h-full rounded transition-all duration-500"
                        :style="{ width: `${Math.min(100, row.rate * 100)}%`, background: barColor(row.variant) }"
                    />
                </div>
                <div class="w-20 text-right shrink-0">
                    <span class="text-xs font-semibold" :style="{ color: textColor(row.variant) }">
                        {{ row.ncCount }} NC
                    </span>
                    <span class="text-xs text-slate-500 ml-1">({{ (row.rate * 100).toFixed(0) }}%)</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { status } from '@/design-system';
import type { ReportCategoryRow } from '../../composables/useReportBuilder';

defineProps<{ categories: ReportCategoryRow[] }>();

function barColor(v: ReportCategoryRow['variant']): string {
    if (v === 'good') return `${status.success.DEFAULT}b3`;   // 70% opacity
    if (v === 'warn') return `${status.warning.DEFAULT}b3`;
    return `${status.danger.DEFAULT}b3`;
}

function textColor(v: ReportCategoryRow['variant']): string {
    if (v === 'good') return status.success.DEFAULT;
    if (v === 'warn') return status.warning.DEFAULT;
    return status.danger.DEFAULT;
}
</script>

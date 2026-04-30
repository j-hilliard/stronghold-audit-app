<template>
    <div class="report-section-ca-table">
        <div class="section-heading">
            <i class="pi pi-exclamation-triangle text-amber-400" />
            <span>Open Corrective Actions</span>
            <span v-if="caRows.length" class="ml-auto text-xs text-slate-400">
                {{ caRows.length }} open
                <span v-if="overdueCount > 0" class="text-red-400 ml-1">({{ overdueCount }} overdue)</span>
            </span>
        </div>

        <div v-if="caRows.length === 0" class="py-6 text-center text-emerald-400 text-sm">
            <i class="pi pi-check-circle mr-2" />No open corrective actions.
        </div>

        <div v-else class="overflow-x-auto">
            <table class="w-full text-sm border-collapse">
                <thead>
                    <tr class="text-left text-xs text-slate-400 uppercase tracking-wide border-b border-slate-700">
                        <th class="pb-2 pr-4">Description</th>
                        <th class="pb-2 pr-4">Assigned To</th>
                        <th class="pb-2 pr-4">Due Date</th>
                        <th class="pb-2 pr-4">Status</th>
                        <th class="pb-2 text-right">Days Open</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="row in caRows"
                        :key="row.id"
                        class="border-b border-slate-800 transition-colors"
                        :class="row.isOverdue ? 'ca-overdue-row' : ''"
                    >
                        <td class="py-2.5 pr-4 text-slate-200 max-w-xs">
                            <span class="line-clamp-2">{{ row.description }}</span>
                        </td>
                        <td class="py-2.5 pr-4 text-slate-400 text-xs whitespace-nowrap">{{ row.assignedTo }}</td>
                        <td class="py-2.5 pr-4 text-xs whitespace-nowrap"
                            :class="row.isOverdue ? 'text-red-400 font-semibold' : 'text-slate-400'">
                            {{ row.dueDate }}
                        </td>
                        <td class="py-2.5 pr-4">
                            <StatusBadge
                                :value="row.isOverdue ? 'Overdue' : row.status"
                                :dot="true"
                                :map="STATUS_MAP"
                            />
                        </td>
                        <td class="py-2.5 text-right text-xs"
                            :class="row.isOverdue ? 'text-red-400 font-bold' : 'text-slate-400'">
                            {{ row.daysOpen }}d
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { StatusBadge } from '@/design-system';
import type { ReportCaRow } from '../../composables/useReportBuilder';

const props = defineProps<{ caRows: ReportCaRow[] }>();

const overdueCount = computed(() => props.caRows.filter(r => r.isOverdue).length);

const STATUS_MAP: Record<string, { classes: string; dot: string }> = {
    Overdue:    { classes: 'bg-red-950/60 border-red-700/60 text-red-300',         dot: 'bg-red-400' },
    Open:       { classes: 'bg-red-950/60 border-red-700/60 text-red-300',         dot: 'bg-red-400' },
    InProgress: { classes: 'bg-amber-950/60 border-amber-700/60 text-amber-300',   dot: 'bg-amber-400' },
    Closed:     { classes: 'bg-emerald-950/60 border-emerald-700/60 text-emerald-300', dot: 'bg-emerald-400' },
    Voided:     { classes: 'bg-slate-800 border-slate-700 text-slate-400',         dot: 'bg-slate-500' },
};
</script>

<style scoped>
.ca-overdue-row td {
    background-color: rgba(239, 68, 68, 0.035);
}
.ca-overdue-row td:first-child {
    border-left: 3px solid #ef4444;
}
</style>

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
                        class="border-b border-slate-800"
                        :class="row.isOverdue ? 'bg-red-950/20' : ''"
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
                            <span class="text-xs px-2 py-0.5 rounded-full font-medium"
                                :class="statusClass(row.status, row.isOverdue)">
                                {{ row.isOverdue ? 'Overdue' : row.status }}
                            </span>
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
import type { ReportCaRow } from '../../composables/useReportBuilder';

const props = defineProps<{ caRows: ReportCaRow[] }>();

const overdueCount = computed(() => props.caRows.filter(r => r.isOverdue).length);

function statusClass(status: string, isOverdue: boolean): string {
    if (isOverdue) return 'bg-red-900/50 text-red-300';
    if (status === 'Closed') return 'bg-emerald-900/50 text-emerald-300';
    if (status === 'InProgress') return 'bg-blue-900/50 text-blue-300';
    return 'bg-slate-700 text-slate-300';
}
</script>

<template>
    <div>
        <div v-if="!content.rows.length" class="text-sm text-slate-500 italic py-4 text-center">
            No open corrective actions for the selected period.
        </div>
        <table v-else class="w-full text-xs text-left border-collapse">
            <thead>
                <tr class="border-b border-slate-700 text-slate-400 uppercase tracking-wide">
                    <th class="py-2 pr-3">Description</th>
                    <th class="py-2 pr-3">Assigned To</th>
                    <th class="py-2 pr-3">Due Date</th>
                    <th class="py-2 pr-3">Status</th>
                    <th class="py-2 text-right">Days Open</th>
                </tr>
            </thead>
            <tbody>
                <tr
                    v-for="row in content.rows"
                    :key="row.id"
                    class="border-b border-slate-800"
                    :class="{ 'bg-red-900/10': row.isOverdue }"
                >
                    <td class="py-2 pr-3 text-slate-300 max-w-xs">{{ row.description }}</td>
                    <td class="py-2 pr-3 text-slate-400">{{ row.assignedTo || '—' }}</td>
                    <td class="py-2 pr-3" :class="row.isOverdue ? 'text-red-400 font-medium' : 'text-slate-400'">
                        {{ row.dueDate || '—' }}
                        <span v-if="row.isOverdue" class="ml-1 text-red-400">⚠</span>
                    </td>
                    <td class="py-2 pr-3">
                        <span
                            class="px-1.5 py-0.5 rounded text-xs font-medium"
                            :class="statusClass(row.status)"
                        >{{ row.status }}</span>
                    </td>
                    <td class="py-2 text-right text-slate-400">{{ row.daysOpen }}d</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script setup lang="ts">
import type { CaTableContent, BlockStyle } from '../../types/report-block';

defineProps<{
    content: CaTableContent;
    style: BlockStyle;
}>();

function statusClass(status: string) {
    switch (status) {
        case 'Open': return 'bg-red-900/40 text-red-300';
        case 'InProgress': return 'bg-amber-900/40 text-amber-300';
        case 'Closed': return 'bg-emerald-900/40 text-emerald-300';
        default: return 'bg-slate-700 text-slate-300';
    }
}
</script>

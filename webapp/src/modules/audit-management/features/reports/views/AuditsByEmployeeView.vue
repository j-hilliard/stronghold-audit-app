<template>
    <div>
        <BasePageHeader
            title="Audits by Employee"
            subtitle="Performance summary grouped by auditor"
            icon="pi pi-users"
        >
            <Button
                label="Back to Dashboard"
                icon="pi pi-arrow-left"
                severity="secondary"
                outlined
                size="small"
                @click="router.push('/audit-management/reports')"
            />
        </BasePageHeader>

        <!-- Filters -->
        <div class="px-4 pt-3 pb-0 flex flex-wrap gap-3 items-end">
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">Division</label>
                <Dropdown
                    v-model="filterDivisionId"
                    :options="[{ id: null, code: 'All Divisions' }, ...store.divisions]"
                    option-label="code"
                    option-value="id"
                    placeholder="All Divisions"
                    class="w-44"
                    @change="load"
                />
            </div>
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">From</label>
                <Calendar
                    v-model="filterDateFrom"
                    placeholder="From date"
                    dateFormat="yy-mm-dd"
                    class="w-36"
                    :show-clear="!!filterDateFrom"
                    @date-select="load"
                    @clear-click="load"
                />
            </div>
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">To</label>
                <Calendar
                    v-model="filterDateTo"
                    placeholder="To date"
                    dateFormat="yy-mm-dd"
                    class="w-36"
                    :show-clear="!!filterDateTo"
                    @date-select="load"
                    @clear-click="load"
                />
            </div>
            <Button icon="pi pi-refresh" severity="secondary" size="small" outlined :loading="loading" @click="load" />
        </div>

        <div class="p-4">
            <div v-if="loading" class="flex justify-center py-12">
                <ProgressSpinner />
            </div>

            <div v-else-if="rows.length === 0" class="text-center py-12 text-slate-400">
                <i class="pi pi-users text-4xl mb-3 block opacity-30" />
                <p class="text-sm">No audit data found for the selected filters.</p>
            </div>

            <div v-else>
                <!-- Summary KPIs -->
                <div class="grid grid-cols-2 md:grid-cols-4 gap-3 mb-5">
                    <div class="bg-slate-800 border border-slate-700 rounded-lg p-4">
                        <div class="text-2xl font-bold text-white">{{ rows.length }}</div>
                        <div class="text-xs text-slate-400 mt-1">Auditors</div>
                    </div>
                    <div class="bg-slate-800 border border-slate-700 rounded-lg p-4">
                        <div class="text-2xl font-bold text-sky-400">{{ totalAudits }}</div>
                        <div class="text-xs text-slate-400 mt-1">Total Audits</div>
                    </div>
                    <div class="bg-slate-800 border border-slate-700 rounded-lg p-4">
                        <div :class="['text-2xl font-bold', avgScoreColor]">{{ avgScoreDisplay }}</div>
                        <div class="text-xs text-slate-400 mt-1">Avg Score</div>
                    </div>
                    <div class="bg-slate-800 border border-slate-700 rounded-lg p-4">
                        <div class="text-2xl font-bold text-red-400">{{ totalNcs }}</div>
                        <div class="text-xs text-slate-400 mt-1">Total NCs</div>
                    </div>
                </div>

                <!-- Employee table -->
                <DataTable
                    :value="rows"
                    :sort-field="sortField"
                    :sort-order="sortOrder"
                    removable-sort
                    class="employee-table"
                    :pt="{
                        table: { class: 'w-full text-sm' },
                        thead: { class: 'sticky top-0 z-10' },
                    }"
                    @sort="(e: any) => onSort(e)"
                >
                    <Column field="auditor" header="Auditor" sortable>
                        <template #body="{ data }">
                            <div class="font-medium text-white">{{ data.auditor }}</div>
                            <div v-if="data.lastDivisionCode" class="text-xs text-slate-500 mt-0.5">
                                Last: {{ data.lastDivisionCode }}
                            </div>
                        </template>
                    </Column>
                    <Column field="auditCount" header="Audits" sortable class="text-center" />
                    <Column field="avgScorePercent" header="Avg Score" sortable>
                        <template #body="{ data }">
                            <span v-if="data.avgScorePercent != null" :class="['font-semibold', scoreColor(data.avgScorePercent)]">
                                {{ data.avgScorePercent }}%
                            </span>
                            <span v-else class="text-slate-500">—</span>
                        </template>
                    </Column>
                    <Column field="totalNonConforming" header="NCs" sortable>
                        <template #body="{ data }">
                            <span :class="data.totalNonConforming > 0 ? 'text-red-400 font-semibold' : 'text-slate-400'">
                                {{ data.totalNonConforming }}
                            </span>
                        </template>
                    </Column>
                    <Column field="totalWarnings" header="Warnings" sortable>
                        <template #body="{ data }">
                            <span :class="data.totalWarnings > 0 ? 'text-amber-400' : 'text-slate-400'">
                                {{ data.totalWarnings }}
                            </span>
                        </template>
                    </Column>
                    <Column field="lastAuditDate" header="Last Audit" sortable>
                        <template #body="{ data }">
                            <span class="text-slate-300">{{ data.lastAuditDate ?? '—' }}</span>
                        </template>
                    </Column>
                </DataTable>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import Button from 'primevue/button';
import Calendar from 'primevue/calendar';
import Dropdown from 'primevue/dropdown';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import ProgressSpinner from 'primevue/progressspinner';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';

const router  = useRouter();
const store   = useAuditStore();
const service = useAuditService();

interface EmployeeRow {
    auditor: string;
    auditCount: number;
    avgScorePercent: number | null;
    totalNonConforming: number;
    totalWarnings: number;
    lastAuditDate: string | null;
    lastDivisionCode: string | null;
}

const loading          = ref(false);
const rows             = ref<EmployeeRow[]>([]);
const filterDivisionId = ref<number | null>(null);
const filterDateFrom   = ref<Date | null>(null);
const filterDateTo     = ref<Date | null>(null);
const sortField        = ref('auditCount');
const sortOrder        = ref<1 | -1>(-1);

async function load() {
    loading.value = true;
    try {
        const params: Record<string, string> = {};
        if (filterDivisionId.value) params.divisionId = String(filterDivisionId.value);
        if (filterDateFrom.value) params.dateFrom = filterDateFrom.value.toISOString();
        if (filterDateTo.value) params.dateTo = filterDateTo.value.toISOString();
        rows.value = await service.getAuditsByEmployee(params);
    } finally {
        loading.value = false;
    }
}

function onSort(e: { sortField: string; sortOrder: number }) {
    sortField.value = e.sortField;
    sortOrder.value = (e.sortOrder as 1 | -1);
}

const totalAudits    = computed(() => rows.value.reduce((s, r) => s + r.auditCount, 0));
const totalNcs       = computed(() => rows.value.reduce((s, r) => s + r.totalNonConforming, 0));
const overallAvg     = computed(() => {
    const scored = rows.value.filter(r => r.avgScorePercent != null);
    return scored.length ? Math.round(scored.reduce((s, r) => s + r.avgScorePercent!, 0) / scored.length * 10) / 10 : null;
});
const avgScoreDisplay = computed(() => overallAvg.value != null ? `${overallAvg.value}%` : '—');
const avgScoreColor   = computed(() => {
    const v = overallAvg.value;
    if (v == null) return 'text-slate-400';
    if (v >= 90) return 'text-emerald-400';
    if (v >= 75) return 'text-amber-400';
    return 'text-red-400';
});

function scoreColor(pct: number) {
    if (pct >= 90) return 'text-emerald-400';
    if (pct >= 75) return 'text-amber-400';
    return 'text-red-400';
}

onMounted(async () => {
    await Promise.all([store.loadDivisions(), load()]);
});
</script>

<style scoped>
:deep(.employee-table .p-datatable-thead th) {
    background: rgb(30, 41, 59);
    color: #94a3b8;
    font-size: 11px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    border-bottom: 1px solid rgb(51, 65, 85);
}
:deep(.employee-table .p-datatable-tbody td) {
    border-bottom: 1px solid rgb(51, 65, 85);
    padding: 10px 12px;
}
:deep(.employee-table .p-datatable-tbody tr:hover td) {
    background: rgba(99, 179, 237, 0.04);
}
</style>

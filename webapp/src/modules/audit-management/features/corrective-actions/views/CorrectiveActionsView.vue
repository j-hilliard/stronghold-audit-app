<template>
    <div class="flex flex-col gap-4 p-4">
        <!-- Filters -->
        <div class="flex flex-wrap gap-3 items-end">
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">Division</label>
                <Dropdown
                    v-model="filterDivisionId"
                    :options="divisionOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="All Divisions"
                    show-clear
                    class="w-52"
                    @change="load"
                />
            </div>
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">Status</label>
                <Dropdown
                    v-model="filterStatus"
                    :options="statusOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="All Statuses"
                    show-clear
                    class="w-44"
                    @change="load"
                />
            </div>
            <Button
                icon="pi pi-file-excel"
                label="Export Excel"
                outlined
                size="small"
                :loading="exportingXlsx"
                @click="exportExcel"
            />
            <Button
                icon="pi pi-refresh"
                label="Refresh"
                outlined
                size="small"
                :loading="loading"
                @click="load"
            />
        </div>

        <!-- Summary KPI Cards -->
        <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4">
                <div class="text-2xl font-bold text-white">{{ displayTotal }}</div>
                <div class="text-xs text-slate-400 mt-1">Total Actions</div>
            </div>
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4">
                <div class="text-2xl font-bold text-blue-400">{{ displayOpen }}</div>
                <div class="text-xs text-slate-400 mt-1">Open</div>
            </div>
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4 kpi-card--danger">
                <div class="text-2xl font-bold text-red-400">{{ displayOverdue }}</div>
                <div class="text-xs text-slate-400 mt-1">Overdue</div>
            </div>
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4">
                <div class="text-2xl font-bold text-green-400">{{ displayClosed }}</div>
                <div class="text-xs text-slate-400 mt-1">Closed</div>
            </div>
        </div>

        <!-- Data Table -->
        <DataTable
            :value="items"
            :loading="loading"
            sort-field="createdAt"
            :sort-order="-1"
            :row-class="rowClass"
            striped-rows
            scrollable
            scroll-height="flex"
            class="text-sm"
            data-key="id"
            @row-dblclick="(e) => goToAudit(e.data.auditId)"
        >
            <template #empty>
                <div class="text-center text-slate-400 py-8">No corrective actions found.</div>
            </template>

            <Column field="divisionCode" header="Division" sortable style="min-width:110px">
                <template #body="{ data }">
                    <span class="font-medium">{{ data.divisionCode }}</span>
                </template>
            </Column>

            <Column field="jobNumber" header="Job #" sortable style="min-width:90px">
                <template #body="{ data }">
                    <span class="text-slate-300">{{ data.jobNumber || '—' }}</span>
                </template>
            </Column>

            <Column field="auditDate" header="Audit Date" sortable style="min-width:110px">
                <template #body="{ data }">
                    <span class="text-slate-300">{{ data.auditDate || '—' }}</span>
                </template>
            </Column>

            <Column field="description" header="Description" style="min-width:220px">
                <template #body="{ data }">
                    <div class="ca-line-clamp-2 text-slate-200">{{ data.description }}</div>
                    <div class="text-xs text-slate-500 mt-0.5 ca-line-clamp-1">{{ data.questionText }}</div>
                </template>
            </Column>

            <Column field="assignedTo" header="Assigned To" sortable style="min-width:140px">
                <template #body="{ data }">
                    <span class="text-slate-300">{{ data.assignedTo || '—' }}</span>
                </template>
            </Column>

            <Column field="dueDate" header="Due Date" sortable style="min-width:110px">
                <template #body="{ data }">
                    <span :class="data.isOverdue ? 'text-red-400 font-semibold' : 'text-slate-300'">
                        {{ data.dueDate || '—' }}
                        <i v-if="data.isOverdue" class="pi pi-exclamation-triangle ml-1 text-xs" />
                    </span>
                </template>
            </Column>

            <Column field="status" header="Status" sortable style="min-width:120px">
                <template #body="{ data }">
                    <Tag :value="data.status" :severity="statusSeverity(data)" :class="{ 'tag-overdue-pulse': data.isOverdue }" />
                </template>
            </Column>

            <Column header="" style="min-width:100px; text-align:right" frozen align-frozen="right">
                <template #body="{ data }">
                    <div class="ca-row-actions">
                        <Button
                            icon="pi pi-eye"
                            size="small"
                            text
                            v-tooltip.top="'View Audit'"
                            @click="goToAudit(data.auditId)"
                        />
                        <Button
                            v-if="data.status !== 'Closed'"
                            icon="pi pi-check"
                            size="small"
                            severity="success"
                            text
                            v-tooltip.top="'Close Action'"
                            @click="openCloseDialog(data)"
                        />
                    </div>
                </template>
            </Column>
        </DataTable>

        <!-- Close CA Dialog -->
        <Dialog
            v-model:visible="showCloseDialog"
            modal
            header="Close Corrective Action"
            :style="{ width: '480px' }"
        >
            <div v-if="selectedItem" class="flex flex-col gap-4">
                <div class="text-sm text-slate-300 bg-slate-800 rounded p-3">
                    <div class="font-medium text-white mb-1">{{ selectedItem.description }}</div>
                    <div class="text-xs text-slate-400">Assigned to: {{ selectedItem.assignedTo || 'Unassigned' }}</div>
                </div>

                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Resolution Notes</label>
                    <Textarea
                        v-model="closeNotes"
                        placeholder="Describe what was done to resolve this action..."
                        rows="3"
                        class="w-full"
                    />
                </div>

                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Completed Date</label>
                    <InputText v-model="closeCompletedDate" type="date" class="w-full" />
                </div>

                <div class="flex justify-end gap-2 mt-2">
                    <Button label="Cancel" text @click="showCloseDialog = false" />
                    <Button
                        label="Close Action"
                        icon="pi pi-check"
                        severity="success"
                        :loading="closeSaving"
                        @click="submitClose"
                    />
                </div>
            </div>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import type { CorrectiveActionListItemDto } from '@/apiclient/auditClient';
import { AuditClient } from '@/apiclient/auditClient';
import { useApiStore } from '@/stores/apiStore';

const router = useRouter();
const toast = useToast();
const apiStore = useApiStore();

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}

// ── State ──────────────────────────────────────────────────────────────────────
const loading = ref(false);
const exportingXlsx = ref(false);
const items = ref<CorrectiveActionListItemDto[]>([]);
const filterDivisionId = ref<number | null>(null);
const filterStatus = ref<string | null>(null);

const divisionOptions = ref<{ label: string; value: number }[]>([]);
const divisionsLoaded = ref(false);

const statusOptions = [
    { label: 'Open', value: 'Open' },
    { label: 'In Progress', value: 'InProgress' },
    { label: 'Overdue', value: 'Overdue' },
    { label: 'Closed', value: 'Closed' },
];

// ── Close dialog ───────────────────────────────────────────────────────────────
const showCloseDialog = ref(false);
const selectedItem = ref<CorrectiveActionListItemDto | null>(null);
const closeNotes = ref('');
const closeCompletedDate = ref('');
const closeSaving = ref(false);

// ── Computed ───────────────────────────────────────────────────────────────────
const openCount = computed(() => items.value.filter(i => i.status === 'Open' || i.status === 'InProgress').length);
const overdueCount = computed(() => items.value.filter(i => i.isOverdue).length);
const closedCount = computed(() => items.value.filter(i => i.status === 'Closed').length);

// ── Count-up animation ─────────────────────────────────────────────────────────
function useCountUp(source: () => number, duration = 700) {
    const display = ref(0);
    watch(source, (to) => {
        const from = display.value;
        const start = performance.now();
        function tick(now: number) {
            const t = Math.min((now - start) / duration, 1);
            const ease = 1 - Math.pow(1 - t, 3);
            display.value = Math.round(from + (to - from) * ease);
            if (t < 1) requestAnimationFrame(tick);
        }
        requestAnimationFrame(tick);
    }, { immediate: true });
    return display;
}

const displayTotal   = useCountUp(() => items.value.length);
const displayOpen    = useCountUp(() => openCount.value);
const displayOverdue = useCountUp(() => overdueCount.value);
const displayClosed  = useCountUp(() => closedCount.value);

// ── Methods ────────────────────────────────────────────────────────────────────
async function load() {
    loading.value = true;
    try {
        const client = getClient();
        const [caList, divisions] = await Promise.all([
            client.getCorrectiveActions(filterDivisionId.value, filterStatus.value),
            divisionsLoaded.value ? Promise.resolve(null) : client.getDivisions(),
        ]);
        items.value = caList;
        if (divisions) {
            divisionOptions.value = divisions.map(d => ({ label: `${d.code} — ${d.name}`, value: d.id }));
            divisionsLoaded.value = true;
        }
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load corrective actions', life: 4000 });
    } finally {
        loading.value = false;
    }
}

function rowClass(data: CorrectiveActionListItemDto) {
    if (data.isOverdue) return 'ca-row-overdue';
    if (data.status === 'Closed') return 'ca-row-closed';
    return '';
}

function statusSeverity(data: CorrectiveActionListItemDto) {
    if (data.isOverdue) return 'danger';
    if (data.status === 'Closed') return 'success';
    if (data.status === 'InProgress') return 'warning';
    return 'info';
}

function goToAudit(auditId: number) {
    router.push({ name: 'audit-management-review', params: { id: String(auditId) } });
}

function openCloseDialog(item: CorrectiveActionListItemDto) {
    selectedItem.value = item;
    closeNotes.value = '';
    closeCompletedDate.value = new Date().toISOString().split('T')[0];
    showCloseDialog.value = true;
}

async function submitClose() {
    if (!selectedItem.value) return;
    closeSaving.value = true;
    try {
        await getClient().closeCorrectiveAction(selectedItem.value.id, {
            notes: closeNotes.value,
            completedDate: closeCompletedDate.value || null,
        });
        toast.add({ severity: 'success', summary: 'Closed', detail: 'Corrective action closed.', life: 3000 });
        showCloseDialog.value = false;
        await load();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to close corrective action.', life: 4000 });
    } finally {
        closeSaving.value = false;
    }
}

// ── Lifecycle ──────────────────────────────────────────────────────────────────
onMounted(load);

// ── Excel export ───────────────────────────────────────────────────────────────
async function exportExcel() {
    exportingXlsx.value = true;
    try {
        const params: Record<string, string> = {};
        if (filterDivisionId.value) params.divisionId = String(filterDivisionId.value);
        const res = await apiStore.api.get('/v1/corrective-actions/export', {
            params,
            responseType: 'blob',
        });
        const blobUrl = URL.createObjectURL(new Blob([res.data], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        }));
        const link = document.createElement('a');
        link.href = blobUrl;
        link.download = 'corrective-actions.xlsx';
        link.click();
        URL.revokeObjectURL(blobUrl);
    } finally {
        exportingXlsx.value = false;
    }
}
</script>

<style scoped>
:deep(.ca-row-overdue td) {
    background-color: rgba(220, 38, 38, 0.08) !important;
}
:deep(.ca-row-closed td) {
    opacity: 0.6;
}
:deep(.ca-line-clamp-2) {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
:deep(.ca-line-clamp-1) {
    display: -webkit-box;
    -webkit-line-clamp: 1;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.ca-row-actions {
    display: flex;
    gap: 2px;
    justify-content: flex-end;
    opacity: 0;
    transition: opacity 0.15s ease;
}
:deep(tr:hover) .ca-row-actions {
    opacity: 1;
}

.kpi-card {
    transition: box-shadow 0.2s ease, transform 0.2s ease, border-color 0.2s ease;
    cursor: default;
}
.kpi-card:hover {
    box-shadow: 0 0 0 1px rgba(99, 179, 237, 0.3), 0 8px 24px rgba(0, 0, 0, 0.4);
    transform: translateY(-2px);
}
.kpi-card--danger:hover {
    box-shadow: 0 0 0 1px rgba(239, 68, 68, 0.35), 0 8px 24px rgba(0, 0, 0, 0.4);
}
</style>

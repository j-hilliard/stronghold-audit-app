<template>
    <div>
        <BasePageHeader
            icon="pi pi-clipboard"
            title="Compliance Audits"
            subtitle="View and manage all compliance audits"
        >
            <Button
                v-if="deleteSelection.length > 0"
                :label="`Delete ${deleteSelection.length} Audit${deleteSelection.length !== 1 ? 's' : ''}`"
                icon="pi pi-trash"
                severity="danger"
                outlined
                size="small"
                @click="onBulkDelete"
            />
            <Button
                label="Print Blank Form"
                icon="pi pi-print"
                severity="secondary"
                outlined
                size="small"
                @click="printBlankForm"
            />
            <BaseButtonCreate label="New Audit" @click="router.push('/audit-management/audits/new')" />
        </BasePageHeader>

        <BaseDataTable
            :value="store.audits"
            :loading="loading"
            emptyMessage="No audits found."
            v-model:selection="selectedAudits"
            dataKey="id"
            @row-dblclick="onRowDblClick"
        >
            <template #filters>
                <Dropdown
                    v-model="store.filterDivisionId"
                    :options="store.divisions"
                    option-label="code"
                    option-value="id"
                    placeholder="All Divisions"
                    class="w-full md:w-44"
                    :show-clear="!!store.filterDivisionId"
                    @change="store.loadAuditList()"
                />
                <Dropdown
                    v-model="store.filterStatus"
                    :options="STATUS_OPTIONS"
                    option-label="label"
                    option-value="value"
                    placeholder="All Statuses"
                    class="w-full md:w-36"
                    :show-clear="!!store.filterStatus"
                    @change="store.loadAuditList()"
                />
                <InputText
                    v-model="store.filterAuditor"
                    placeholder="Auditor…"
                    class="w-full md:w-36"
                    @keydown.enter="store.loadAuditList()"
                    @change="store.filterAuditor = ($event.target as HTMLInputElement).value || null; store.loadAuditList()"
                />
                <div class="flex flex-col gap-0.5 w-full md:w-auto">
                    <label class="text-[10px] text-slate-500 font-medium uppercase tracking-wide px-0.5">From</label>
                    <InputText
                        v-model="store.filterDateFrom"
                        type="date"
                        class="w-full md:w-36"
                        @change="store.loadAuditList()"
                    />
                </div>
                <div class="flex flex-col gap-0.5 w-full md:w-auto">
                    <label class="text-[10px] text-slate-500 font-medium uppercase tracking-wide px-0.5">To</label>
                    <InputText
                        v-model="store.filterDateTo"
                        type="date"
                        class="w-full md:w-36"
                        @change="store.loadAuditList()"
                    />
                </div>
                <Button icon="pi pi-search" severity="secondary" :loading="loading" @click="store.loadAuditList()" title="Apply filters" />
                <Button icon="pi pi-times" severity="secondary" text :loading="loading" @click="clearFilters" title="Clear filters" />
            </template>

            <Column selection-mode="multiple" style="width: 44px;" />
            <Column field="trackingNumber" header="Audit #" style="min-width: 120px;" sortable>
                <template #body="{ data }">
                    <span v-if="data.trackingNumber" class="font-mono text-xs font-semibold text-blue-300">{{ data.trackingNumber }}</span>
                    <span v-else class="text-slate-500">—</span>
                </template>
            </Column>
            <Column field="id" header="#" style="width: 60px;" sortable />
            <Column field="divisionCode" header="Division" sortable />
            <Column field="auditDate" header="Date" sortable>
                <template #body="{ data }">
                    {{ data.auditDate ?? '—' }}
                </template>
            </Column>
            <Column field="auditor" header="Auditor" sortable>
                <template #body="{ data }">
                    {{ data.auditor ?? '—' }}
                </template>
            </Column>
            <Column field="jobNumber" header="Job #">
                <template #body="{ data }">
                    {{ data.jobNumber ?? '—' }}
                </template>
            </Column>
            <Column field="location" header="Location">
                <template #body="{ data }">
                    {{ data.location ?? '—' }}
                </template>
            </Column>
            <Column field="status" header="Status" sortable>
                <template #body="{ data }">
                    <Tag :value="data.status" :severity="statusSeverity(data.status)" />
                </template>
            </Column>
            <Column header="" style="width: 130px; text-align: right;">
                <template #body="{ data }">
                    <div class="audit-row-actions">
                        <Button
                            v-if="data.status === 'Draft' || data.status === 'Reopened'"
                            icon="pi pi-pencil"
                            label="Edit"
                            size="small"
                            text
                            class="audit-action-btn"
                            @click.stop="router.push(`/audit-management/audits/${data.id}`)"
                        />
                        <Button
                            v-if="data.status !== 'Draft' && data.status !== 'Reopened'"
                            icon="pi pi-eye"
                            label="View"
                            size="small"
                            text
                            class="audit-action-btn"
                            @click.stop="router.push(`/audit-management/audits/${data.id}/review`)"
                        />
                    </div>
                </template>
            </Column>
        </BaseDataTable>
    </div>



    <!-- Print Blank Form dialog -->
    <Dialog
        v-model:visible="showPrintDialog"
        header="Print Blank Form"
        :modal="true"
        :style="{ width: '420px' }"
    >
        <div class="space-y-4 py-2">
            <p class="text-sm text-slate-400">Choose a division to print a blank audit form.</p>
            <div class="space-y-1">
                <label class="block text-sm font-medium">Division</label>
                <Dropdown
                    v-model="printDivisionId"
                    :options="store.divisions"
                    option-label="code"
                    option-value="id"
                    placeholder="— Select a division —"
                    class="w-full"
                />
            </div>
            <div v-if="printError" class="flex items-center gap-2 text-sm text-red-400 bg-red-950/40 border border-red-800/50 rounded px-3 py-2">
                <i class="pi pi-exclamation-triangle shrink-0" />
                {{ printError }}
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" text @click="showPrintDialog = false" />
            <Button
                label="Print"
                icon="pi pi-print"
                :loading="printLoading"
                :disabled="!printDivisionId || printLoading"
                @click="doPrint"
            />
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import Tag from 'primevue/tag';
import Dropdown from 'primevue/dropdown';
import InputText from 'primevue/inputtext';
import Column from 'primevue/column';
import Dialog from 'primevue/dialog';
import { useConfirm } from 'primevue/useconfirm';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import BaseButtonCreate from '@/components/buttons/BaseButtonCreate.vue';
import BaseButtonIconEdit from '@/components/buttons/BaseButtonIconEdit.vue';
import BaseButtonIconView from '@/components/buttons/BaseButtonIconView.vue';
import BaseDataTable from '@/components/tables/BaseDataTable.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useUserStore } from '@/stores/userStore';
import { usePermissions } from '@/modules/audit-management/composables/usePermissions';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import type { AuditListItemDto } from '@/apiclient/auditClient';

const router = useRouter();
const store = useAuditStore();
const userStore = useUserStore();
const { hasPermission } = usePermissions();
const confirm = useConfirm();
const loading = ref(false);
const selectedAudits = ref<AuditListItemDto[]>([]);

const showPrintDialog = ref(false);
const printDivisionId = ref<number | null>(null);
const printLoading = ref(false);
const printError = ref<string | null>(null);

function printBlankForm() {
    printDivisionId.value = store.filterDivisionId ?? null;
    printError.value = null;
    showPrintDialog.value = true;
}

async function doPrint() {
    if (!printDivisionId.value) return;
    printLoading.value = true;
    printError.value = null;
    try {
        // Fetch the template in the authenticated main-app context, then pass
        // it to the print tab via sessionStorage — the print view has no auth token.
        const template = await useAuditService().getActiveTemplate(printDivisionId.value);
        sessionStorage.setItem('print-blank-form-data', JSON.stringify(template));
        window.open(`/audit-management/print/${printDivisionId.value}`, '_blank');
        showPrintDialog.value = false;
    } catch {
        printError.value = 'No active template found for this division.';
    } finally {
        printLoading.value = false;
    }
}

const deleteSelection = computed(() =>
    hasPermission('admin.access')
        ? selectedAudits.value
        : selectedAudits.value.filter(a => a.status === 'Draft')
);

const STATUS_OPTIONS = [
    { label: 'Draft',        value: 'Draft'       },
    { label: 'Submitted',    value: 'Submitted'   },
    { label: 'Reopened',     value: 'Reopened'    },
    { label: 'Under Review', value: 'UnderReview' },
    { label: 'Approved',     value: 'Approved'    },
    { label: 'Distributed',  value: 'Distributed' },
    { label: 'Closed',       value: 'Closed'      },
];

function clearFilters() {
    store.filterDivisionId = null;
    store.filterStatus = null;
    store.filterAuditor = null;
    store.filterDateFrom = null;
    store.filterDateTo = null;
    store.loadAuditList();
}

async function loadData() {
    loading.value = true;
    selectedAudits.value = [];
    try {
        await Promise.all([store.loadDivisions(), store.loadAuditList()]);
    } finally {
        loading.value = false;
    }
}

onMounted(loadData);

function statusSeverity(status: string): string {
    const map: Record<string, string> = {
        Draft: 'warning', Submitted: 'info', Reopened: 'warning',
        UnderReview: 'contrast', Approved: 'success', Distributed: 'secondary', Closed: 'secondary',
    };
    return map[status] ?? 'secondary';
}

function onRowDblClick(event: { data: AuditListItemDto }) {
    const d = event.data;
    if (d.status === 'Draft' || d.status === 'Reopened') {
        router.push(`/audit-management/audits/${d.id}`);
    } else {
        router.push(`/audit-management/audits/${d.id}/review`);
    }
}

function onBulkDelete() {
    const toDelete = deleteSelection.value;

    if (toDelete.length === 0) return;
    const skipped = selectedAudits.value.length - toDelete.length;
    const msg = skipped > 0
        ? `Delete ${toDelete.length} draft audit${toDelete.length !== 1 ? 's' : ''}? (${skipped} non-draft selected will be skipped)`
        : `Delete ${toDelete.length} audit${toDelete.length !== 1 ? 's' : ''}? This cannot be undone.`;

    confirm.require({
        message: msg,
        header: 'Delete Audits',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Delete',
        rejectLabel: 'Cancel',
        acceptClass: 'p-button-danger',
        accept: () => {
            const ids = toDelete.map(a => a.id);
            store.bulkDeleteAudits(ids).then(() => {
                selectedAudits.value = [];
            });
        },
    });
}
</script>

<style scoped>
.audit-row-actions {
    display: flex;
    gap: 2px;
    justify-content: flex-end;
    opacity: 0;
    transition: opacity 0.15s ease;
}

:deep(tr:hover) .audit-row-actions {
    opacity: 1;
}

:deep(.audit-action-btn) {
    padding: 3px 8px !important;
    font-size: 0.72rem !important;
}
</style>

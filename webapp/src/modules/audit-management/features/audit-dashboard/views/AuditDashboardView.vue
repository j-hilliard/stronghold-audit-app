<template>
    <div>
        <BasePageHeader
            icon="pi pi-clipboard"
            title="Compliance Audits"
            subtitle="View and manage all compliance audits"
        >
            <Button
                v-if="selectedAudits.length > 0"
                :label="`Delete Selected (${draftSelection.length})`"
                icon="pi pi-trash"
                severity="danger"
                outlined
                size="small"
                :disabled="draftSelection.length === 0"
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
                <InputText
                    v-model="store.filterDateFrom"
                    placeholder="From (YYYY-MM-DD)"
                    class="w-full md:w-36"
                    @keydown.enter="store.loadAuditList()"
                />
                <InputText
                    v-model="store.filterDateTo"
                    placeholder="To (YYYY-MM-DD)"
                    class="w-full md:w-36"
                    @keydown.enter="store.loadAuditList()"
                />
                <Button icon="pi pi-search" severity="secondary" :loading="loading" @click="store.loadAuditList()" title="Apply filters" />
                <Button icon="pi pi-times" severity="secondary" text :loading="loading" @click="clearFilters" title="Clear filters" />
            </template>

            <Column selection-mode="multiple" style="width: 44px;" />
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
            <Column header="Actions" style="width: 100px;">
                <template #body="{ data }">
                    <BaseButtonIconEdit @click="router.push(`/audit-management/audits/${data.id}`)" />
                    <BaseButtonIconView
                        v-if="data.status === 'Submitted' || data.status === 'Closed'"
                        @click="router.push(`/audit-management/audits/${data.id}/review`)"
                    />
                </template>
            </Column>
        </BaseDataTable>
    </div>

    <ConfirmDialog />

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
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" text @click="showPrintDialog = false" />
            <Button
                label="Print"
                icon="pi pi-print"
                :disabled="!printDivisionId"
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
import ConfirmDialog from 'primevue/confirmdialog';
import Dialog from 'primevue/dialog';
import { useConfirm } from 'primevue/useconfirm';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import BaseButtonCreate from '@/components/buttons/BaseButtonCreate.vue';
import BaseButtonIconEdit from '@/components/buttons/BaseButtonIconEdit.vue';
import BaseButtonIconView from '@/components/buttons/BaseButtonIconView.vue';
import BaseDataTable from '@/components/tables/BaseDataTable.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient } from '@/apiclient/auditClient';
import type { AuditListItemDto } from '@/apiclient/auditClient';

const router = useRouter();
const store = useAuditStore();
const apiStore = useApiStore();
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
        const client = new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
        const template = await client.getActiveTemplate(printDivisionId.value);
        sessionStorage.setItem('print-blank-form-data', JSON.stringify(template));
        window.open(`/audit-management/print/${printDivisionId.value}`, '_blank');
        showPrintDialog.value = false;
    } catch {
        printError.value = 'No active template found for this division.';
    } finally {
        printLoading.value = false;
    }
}

const draftSelection = computed(() => selectedAudits.value.filter(a => a.status === 'Draft'));

const STATUS_OPTIONS = [
    { label: 'Draft', value: 'Draft' },
    { label: 'Submitted', value: 'Submitted' },
    { label: 'Reopened', value: 'Reopened' },
    { label: 'Closed', value: 'Closed' },
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
        Draft: 'warning',
        Submitted: 'info',
        Reopened: 'warning',
        Closed: 'success',
    };
    return map[status] ?? 'secondary';
}

function onBulkDelete() {
    const drafts = draftSelection.value;
    if (drafts.length === 0) return;
    const nonDraft = selectedAudits.value.length - drafts.length;
    const msg = nonDraft > 0
        ? `Delete ${drafts.length} draft audit${drafts.length !== 1 ? 's' : ''}? (${nonDraft} non-draft selected will be skipped)`
        : `Delete ${drafts.length} draft audit${drafts.length !== 1 ? 's' : ''}? This cannot be undone.`;

    confirm.require({
        message: msg,
        header: 'Delete Drafts',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Delete',
        rejectLabel: 'Cancel',
        acceptClass: 'p-button-danger',
        accept: async () => {
            const ids = drafts.map(a => a.id);
            await store.bulkDeleteAudits(ids);
            selectedAudits.value = [];
        },
    });
}
</script>

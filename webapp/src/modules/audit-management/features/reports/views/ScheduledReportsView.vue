<template>
    <div>
        <BasePageHeader
            title="Scheduled Reports"
            subtitle="Automatically generate and deliver reports on a recurring schedule"
            icon="pi pi-calendar"
        >
            <Button
                label="Back to Gallery"
                icon="pi pi-arrow-left"
                severity="secondary"
                outlined
                size="small"
                @click="router.push('/audit-management/reports/gallery')"
            />
            <Button
                label="New Schedule"
                icon="pi pi-plus"
                size="small"
                @click="openNew"
            />
        </BasePageHeader>

        <div class="p-4">
            <div v-if="loading" class="flex justify-center py-12">
                <ProgressSpinner />
            </div>

            <div v-else-if="schedules.length === 0" class="text-center py-12 text-slate-400">
                <i class="pi pi-calendar text-4xl mb-3 block opacity-30" />
                <p class="text-sm">No scheduled reports yet.</p>
                <Button label="Create your first schedule" class="mt-4" size="small" @click="openNew" />
            </div>

            <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                <div
                    v-for="s in schedules"
                    :key="s.id"
                    class="scheduled-card"
                >
                    <div class="flex items-start justify-between gap-3 mb-3">
                        <div>
                            <div class="text-white font-semibold text-sm">{{ s.title }}</div>
                            <div class="text-xs text-slate-400 mt-0.5">
                                {{ templateName(s.templateId) }}
                                <span v-if="s.divisionCode"> · {{ s.divisionCode }}</span>
                            </div>
                        </div>
                        <Tag :value="s.frequency" severity="info" />
                    </div>
                    <div class="flex flex-wrap gap-x-4 gap-y-1 text-xs text-slate-400 mb-4">
                        <span><i class="pi pi-clock mr-1" />{{ s.timeUtc }} UTC</span>
                        <span v-if="s.recipients.length">
                            <i class="pi pi-envelope mr-1" />{{ s.recipients.length }} recipient{{ s.recipients.length !== 1 ? 's' : '' }}
                        </span>
                        <span v-if="s.lastRunAt">
                            <i class="pi pi-check mr-1" />Last: {{ fmtDate(s.lastRunAt) }}
                        </span>
                        <span>
                            <i class="pi pi-forward mr-1" />Next: {{ fmtDate(s.nextRunAt) }}
                        </span>
                    </div>
                    <div class="flex gap-2">
                        <Button icon="pi pi-pencil" size="small" severity="secondary" outlined @click="openEdit(s)" />
                        <Button icon="pi pi-trash" size="small" severity="danger" outlined @click="confirmDelete(s)" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Edit/Create dialog -->
        <Dialog
            v-model:visible="showEditor"
            :header="editing?.id ? 'Edit Schedule' : 'New Schedule'"
            modal
            :style="{ width: '500px' }"
        >
            <div v-if="editing" class="space-y-4 py-2">
                <div>
                    <label class="field-label">Template</label>
                    <Dropdown
                        v-model="editing.templateId"
                        :options="templateOptions"
                        option-label="name"
                        option-value="id"
                        class="w-full"
                        placeholder="Select template"
                    />
                </div>
                <div>
                    <label class="field-label">Title</label>
                    <InputText v-model="editing.title" class="w-full" placeholder="Report title" />
                </div>
                <div>
                    <label class="field-label">Division</label>
                    <Dropdown
                        v-model="editing.divisionId"
                        :options="[{ id: null, code: 'All Divisions' }, ...store.divisions]"
                        option-label="code"
                        option-value="id"
                        class="w-full"
                        placeholder="All Divisions"
                    />
                </div>
                <div class="grid grid-cols-2 gap-3">
                    <div>
                        <label class="field-label">Frequency</label>
                        <Dropdown
                            v-model="editing.frequency"
                            :options="['Daily', 'Weekly', 'Monthly', 'Quarterly']"
                            class="w-full"
                        />
                    </div>
                    <div>
                        <label class="field-label">Send Time (UTC)</label>
                        <InputText v-model="editing.timeUtc" class="w-full" placeholder="07:00" />
                    </div>
                </div>
                <div>
                    <label class="field-label">Date Range</label>
                    <Dropdown
                        v-model="editing.dateRangePreset"
                        :options="dateRangeOptions"
                        option-label="label"
                        option-value="value"
                        class="w-full"
                        placeholder="All time"
                    />
                </div>
                <div>
                    <label class="field-label">Recipients (comma-separated emails)</label>
                    <Textarea
                        v-model="recipientsText"
                        class="w-full font-mono text-xs"
                        rows="2"
                        placeholder="user@company.com, manager@company.com"
                        auto-resize
                    />
                </div>
            </div>
            <template #footer>
                <div class="flex justify-end gap-2">
                    <Button label="Cancel" severity="secondary" outlined @click="showEditor = false" />
                    <Button label="Save" icon="pi pi-check" :loading="saving" @click="save" />
                </div>
            </template>
        </Dialog>


        <Toast />
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue';
import { useRouter } from 'vue-router';
import Button from 'primevue/button';
import Dialog from 'primevue/dialog';
import Dropdown from 'primevue/dropdown';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import Tag from 'primevue/tag';
import ProgressSpinner from 'primevue/progressspinner';
import Toast from 'primevue/toast';
import { useConfirm } from 'primevue/useconfirm';
import { useToast } from 'primevue/usetoast';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';

const router   = useRouter();
const store    = useAuditStore();
const service  = useAuditService();
const confirm  = useConfirm();
const toast    = useToast();

const loading   = ref(false);
const saving    = ref(false);
const schedules = ref<any[]>([]);
const showEditor = ref(false);
const editing    = ref<any | null>(null);
const recipientsText = ref('');

const templateOptions = [
    { id: 'annual-review',      name: 'Annual Review' },
    { id: 'quarterly-summary',  name: 'Quarterly Summary' },
    { id: 'post-audit-summary', name: 'Post-Audit Summary' },
    { id: 'ncr-report',         name: 'NCR Report' },
    { id: 'executive-dashboard', name: 'Executive Dashboard' },
    { id: 'ca-aging',           name: 'CA Aging Report' },
];

const dateRangeOptions = [
    { label: 'All time',      value: null },
    { label: 'Last 30 days',  value: 'last30days' },
    { label: 'This quarter',  value: 'thisquarter' },
    { label: 'Last quarter',  value: 'lastquarter' },
    { label: 'This year',     value: 'thisyear' },
    { label: 'Last year',     value: 'lastyear' },
];

function templateName(id: string) {
    return templateOptions.find(t => t.id === id)?.name ?? id;
}

function fmtDate(d: string | Date | null) {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

async function load() {
    loading.value = true;
    try {
        schedules.value = await service.getScheduledReports();
    } finally {
        loading.value = false;
    }
}

function openNew() {
    editing.value = {
        id: null,
        templateId: 'quarterly-summary',
        title: 'Quarterly Summary',
        divisionId: null,
        frequency: 'Quarterly',
        timeUtc: '07:00',
        dateRangePreset: 'lastquarter',
        recipients: [],
        isActive: true,
    };
    recipientsText.value = '';
    showEditor.value = true;
}

function openEdit(s: any) {
    editing.value = { ...s };
    recipientsText.value = s.recipients.join(', ');
    showEditor.value = true;
}

async function save() {
    if (!editing.value?.templateId || !editing.value?.title) return;
    saving.value = true;
    try {
        const payload = {
            ...editing.value,
            recipients: recipientsText.value.split(',').map((e: string) => e.trim()).filter(Boolean),
        };
        await service.saveScheduledReport(payload);
        toast.add({ severity: 'success', summary: 'Saved', life: 3000 });
        showEditor.value = false;
        await load();
    } catch {
        toast.add({ severity: 'error', summary: 'Save failed', life: 5000 });
    } finally {
        saving.value = false;
    }
}

function confirmDelete(s: any) {
    confirm.require({
        message: `Delete the scheduled report "${s.title}"?`,
        header: 'Delete Schedule',
        icon: 'pi pi-exclamation-triangle',
        acceptClass: 'p-button-danger',
        accept: () => {
            service.deleteScheduledReport(s.id)
                .then(() => load())
                .then(() => toast.add({ severity: 'success', summary: 'Deleted', life: 3000 }));
        },
    });
}

onMounted(async () => {
    await Promise.all([store.loadDivisions(), load()]);
});
</script>

<style scoped>
.scheduled-card {
    background: rgb(30, 41, 59);
    border: 1px solid rgb(51, 65, 85);
    border-radius: 10px;
    padding: 16px;
    transition: border-color 0.2s;
}
.scheduled-card:hover { border-color: rgba(99, 179, 237, 0.4); }
.field-label {
    display: block;
    font-size: 11px;
    font-weight: 600;
    color: #94a3b8;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    margin-bottom: 6px;
}
</style>

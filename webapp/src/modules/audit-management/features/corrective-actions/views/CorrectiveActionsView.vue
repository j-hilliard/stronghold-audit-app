<template>
    <div class="flex flex-col gap-4 p-4 min-h-0">

        <!-- ── Filter Bar ─────────────────────────────────────────────────────── -->
        <div class="flex flex-wrap gap-2 items-end">
            <!-- Search -->
            <div class="flex flex-col gap-1 flex-1 min-w-[200px]">
                <label class="text-xs text-slate-400 font-medium">Search</label>
                <IconField icon-position="left">
                    <InputIcon class="pi pi-search" />
                    <InputText
                        v-model="filterSearch"
                        placeholder="Description, assignee, question…"
                        class="w-full"
                        @keyup.enter="load"
                    />
                </IconField>
            </div>

            <!-- Division -->
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">Division</label>
                <Dropdown
                    v-model="filterDivisionId"
                    :options="divisionOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="All Divisions"
                    show-clear
                    class="w-48"
                    @change="load"
                />
            </div>

            <!-- Status -->
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">Status</label>
                <Dropdown
                    v-model="filterStatus"
                    :options="statusOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="All Statuses"
                    show-clear
                    class="w-40"
                    @change="load"
                />
            </div>

            <!-- Source -->
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">Source</label>
                <Dropdown
                    v-model="filterSource"
                    :options="sourceOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="All Sources"
                    show-clear
                    class="w-40"
                    @change="load"
                />
            </div>

            <!-- Overdue toggle -->
            <Button
                :label="filterOverdueOnly ? 'Overdue Only ✕' : 'Overdue Only'"
                :severity="filterOverdueOnly ? 'danger' : 'secondary'"
                size="small"
                :outlined="!filterOverdueOnly"
                class="self-end"
                @click="filterOverdueOnly = !filterOverdueOnly; load()"
            />

            <div class="flex gap-2 self-end ml-auto">
                <Button icon="pi pi-refresh" outlined size="small" :loading="loading" @click="load" />
                <Button icon="pi pi-file-excel" label="Export" outlined size="small" :loading="exportingXlsx" @click="exportExcel" />
            </div>
        </div>

        <!-- ── KPI Cards ──────────────────────────────────────────────────────── -->
        <div class="grid grid-cols-2 md:grid-cols-5 gap-3">
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4">
                <div class="text-2xl font-bold text-white">{{ displayTotal }}</div>
                <div class="text-xs text-slate-400 mt-1">Total</div>
            </div>
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4">
                <div class="text-2xl font-bold text-blue-400">{{ displayOpen }}</div>
                <div class="text-xs text-slate-400 mt-1">Open</div>
            </div>
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4">
                <div class="text-2xl font-bold text-amber-400">{{ displayInProgress }}</div>
                <div class="text-xs text-slate-400 mt-1">In Progress</div>
            </div>
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4 kpi-card--danger">
                <div class="text-2xl font-bold text-red-400">{{ displayOverdue }}</div>
                <div class="text-xs text-slate-400 mt-1">Overdue</div>
            </div>
            <div class="kpi-card bg-slate-800 border border-slate-700 rounded-lg p-4">
                <div class="text-2xl font-bold text-emerald-400">{{ displayClosed }}</div>
                <div class="text-xs text-slate-400 mt-1">Closed</div>
            </div>
        </div>

        <!-- ── Bulk Action Toolbar (visible when rows selected) ───────────────── -->
        <Transition name="bulk-bar">
            <div v-if="selectedItems.length > 0" class="flex items-center gap-3 px-4 py-2.5 bg-blue-900/40 border border-blue-700/50 rounded-lg">
                <span class="text-sm font-medium text-blue-300">
                    {{ selectedItems.length }} action{{ selectedItems.length === 1 ? '' : 's' }} selected
                </span>
                <div class="flex gap-2 ml-auto flex-wrap">
                    <Button label="Mark In Progress" size="small" severity="warning" outlined @click="bulkChangeStatus('InProgress')" />
                    <Button label="Close All" size="small" severity="success" outlined @click="openBulkCloseDialog" />
                    <Button label="Void All" size="small" severity="danger" outlined @click="bulkChangeStatus('Voided')" />
                    <Button label="Reassign" size="small" outlined @click="openBulkReassignDialog" />
                    <Button icon="pi pi-times" size="small" text severity="secondary" @click="selectedItems = []" />
                </div>
            </div>
        </Transition>

        <!-- ── Data Table ─────────────────────────────────────────────────────── -->
        <DataTable
            v-model:selection="selectedItems"
            :value="items"
            :loading="loading"
            sort-field="createdAt"
            :sort-order="-1"
            :row-class="rowClass"
            scrollable
            scroll-height="flex"
            class="text-sm flex-1 min-h-0"
            data-key="id"
            @row-dblclick="(e: any) => goToAudit(e.data.auditId)"
        >
            <template #empty>
                <div class="text-center text-slate-400 py-8">No corrective actions found.</div>
            </template>

            <!-- Checkbox column -->
            <Column selection-mode="multiple" style="width:44px; padding: 0 8px" />

            <!-- Division -->
            <Column field="divisionCode" header="Division" sortable style="min-width:100px">
                <template #body="{ data }">
                    <span class="font-medium text-white">{{ data.divisionCode }}</span>
                    <div class="text-xs text-slate-400">{{ data.divisionName }}</div>
                </template>
            </Column>

            <!-- Description -->
            <Column field="description" header="Description" style="min-width:220px">
                <template #body="{ data }">
                    <div class="line-clamp-2 text-slate-200 text-sm">{{ data.description }}</div>
                    <div v-if="data.questionText" class="text-xs text-slate-500 mt-0.5 line-clamp-1">{{ data.questionText }}</div>
                    <!-- Source badge -->
                    <span
                        v-if="data.source === 'AutoGenerated'"
                        class="mt-1 inline-block text-[10px] px-1.5 py-0.5 rounded bg-purple-900/60 text-purple-300 font-medium"
                    >Auto-CA</span>
                </template>
            </Column>

            <!-- Assigned To -->
            <Column field="assignedTo" header="Assigned To" sortable style="min-width:130px">
                <template #body="{ data }">
                    <span class="text-slate-300">{{ data.assignedTo || '—' }}</span>
                </template>
            </Column>

            <!-- Due Date -->
            <Column field="dueDate" header="Due Date" sortable style="min-width:110px">
                <template #body="{ data }">
                    <span :class="data.isOverdue ? 'text-red-400 font-semibold' : 'text-slate-300'">
                        {{ data.dueDate || '—' }}
                        <i v-if="data.isOverdue" class="pi pi-exclamation-triangle ml-1 text-xs" />
                    </span>
                </template>
            </Column>

            <!-- Days Open -->
            <Column field="daysOpen" header="Days Open" sortable style="min-width:90px">
                <template #body="{ data }">
                    <span
                        :class="[
                            'font-mono text-sm',
                            data.status === 'Closed' || data.status === 'Voided'
                                ? 'text-slate-500'
                                : data.daysOpen >= 30 ? 'text-red-400 font-bold'
                                : data.daysOpen >= 14 ? 'text-amber-400'
                                : 'text-slate-300'
                        ]"
                    >{{ data.status === 'Closed' || data.status === 'Voided' ? '—' : data.daysOpen + 'd' }}</span>
                </template>
            </Column>

            <!-- Status -->
            <Column field="status" header="Status" sortable style="min-width:120px">
                <template #body="{ data }">
                    <Tag :value="statusLabel(data.status)" :severity="statusSeverity(data)" />
                </template>
            </Column>

            <!-- Audit Date -->
            <Column field="auditDate" header="Audit Date" sortable style="min-width:100px">
                <template #body="{ data }">
                    <span class="text-slate-400 text-xs">{{ data.auditDate || '—' }}</span>
                </template>
            </Column>

            <!-- Actions -->
            <Column header="" style="min-width:90px; text-align:right" frozen align-frozen="right">
                <template #body="{ data }">
                    <div class="ca-row-actions">
                        <Button
                            icon="pi pi-pencil"
                            size="small"
                            text
                            v-tooltip.top="'Edit'"
                            :disabled="data.status === 'Closed' || data.status === 'Voided'"
                            @click="openEditDialog(data)"
                        />
                        <Button
                            icon="pi pi-eye"
                            size="small"
                            text
                            v-tooltip.top="'View Audit'"
                            @click="goToAudit(data.auditId)"
                        />
                        <Button
                            v-if="data.status !== 'Closed' && data.status !== 'Voided'"
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

        <!-- ── Close CA Dialog ───────────────────────────────────────────────── -->
        <Dialog v-model:visible="showCloseDialog" modal header="Close Corrective Action" :style="{ width: '520px' }">
            <div v-if="selectedItem" class="flex flex-col gap-4">
                <div class="text-sm text-slate-300 bg-slate-800 rounded p-3">
                    <div class="font-medium text-white mb-1">{{ selectedItem.description }}</div>
                    <div class="text-xs text-slate-400">Assigned to: {{ selectedItem.assignedTo || 'Unassigned' }}</div>
                </div>

                <!-- Closure photo requirement -->
                <div v-if="selectedItem.requireClosurePhoto" class="flex flex-col gap-2">
                    <label class="text-sm font-medium">
                        Closure Photo <span class="text-red-400">*</span>
                        <span class="text-xs text-slate-400 ml-1 font-normal">(required by division policy)</span>
                    </label>
                    <div
                        v-if="!closurePhoto"
                        class="border-2 border-dashed rounded-lg p-4 text-center cursor-pointer transition-colors"
                        :class="closurePhotoDragging ? 'border-blue-500 bg-blue-500/10' : 'border-slate-600 hover:border-slate-500'"
                        @dragover.prevent="closurePhotoDragging = true"
                        @dragleave="closurePhotoDragging = false"
                        @drop.prevent="onClosurePhotoDrop($event)"
                        @click="closurePhotoInput?.click()"
                    >
                        <i class="pi pi-camera text-2xl text-slate-500 mb-2 block" />
                        <p class="text-sm text-slate-400">Click or drag a photo here</p>
                        <p class="text-xs text-slate-500 mt-1">JPG, PNG, HEIC, WebP — max 25 MB</p>
                        <input ref="closurePhotoInput" type="file" class="hidden" accept=".jpg,.jpeg,.png,.heic,.gif,.webp,.bmp" @change="onClosurePhotoSelect($event)" />
                    </div>
                    <div v-else class="flex items-center gap-3 bg-slate-800 rounded-lg p-3">
                        <i class="pi pi-image text-green-400 text-xl" />
                        <div class="flex-1 min-w-0">
                            <p class="text-sm text-white truncate">{{ closurePhoto.name }}</p>
                            <p class="text-xs text-slate-400">{{ (closurePhoto.size / 1024).toFixed(0) }} KB</p>
                        </div>
                        <div v-if="closurePhotoUploading" class="text-xs text-blue-400 flex items-center gap-1">
                            <i class="pi pi-spin pi-spinner" /> Uploading…
                        </div>
                        <div v-else-if="closurePhotoUploaded" class="text-xs text-green-400 flex items-center gap-1">
                            <i class="pi pi-check-circle" /> Uploaded
                        </div>
                        <button v-if="!closurePhotoUploading && !closurePhotoUploaded" class="text-slate-500 hover:text-red-400 transition-colors" @click="closurePhoto = null">
                            <i class="pi pi-times" />
                        </button>
                    </div>
                </div>

                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Resolution Notes <span class="text-red-400">*</span></label>
                    <Textarea v-model="closeNotes" placeholder="Describe what was done to resolve this action…" rows="3" class="w-full" />
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
                        :disabled="!closeNotes.trim() || (selectedItem.requireClosurePhoto && !closurePhotoUploaded)"
                        @click="submitClose"
                    />
                </div>
            </div>
        </Dialog>

        <!-- ── Edit CA Dialog ────────────────────────────────────────────────── -->
        <Dialog v-model:visible="showEditDialog" modal header="Edit Corrective Action" :style="{ width: '520px' }">
            <div v-if="editItem" class="flex flex-col gap-4">
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Description <span class="text-red-400">*</span></label>
                    <Textarea v-model="editDescription" rows="3" class="w-full" />
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Assigned To</label>
                    <InputText v-model="editAssignedTo" placeholder="Name or email" class="w-full" />
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Due Date</label>
                    <InputText v-model="editDueDate" type="date" class="w-full" />
                </div>
                <div class="flex justify-end gap-2 mt-2">
                    <Button label="Cancel" text @click="showEditDialog = false" />
                    <Button label="Save Changes" icon="pi pi-save" :loading="editSaving" :disabled="!editDescription.trim()" @click="submitEdit" />
                </div>
            </div>
        </Dialog>

        <!-- ── Bulk Close Dialog ─────────────────────────────────────────────── -->
        <Dialog v-model:visible="showBulkCloseDialog" modal :header="`Close ${selectedItems.length} Corrective Actions`" :style="{ width: '480px' }">
            <div class="flex flex-col gap-4">
                <div class="text-sm text-slate-400">
                    All selected actions will be marked Closed. This cannot be undone.
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Resolution Notes <span class="text-red-400">*</span></label>
                    <Textarea v-model="bulkCloseNotes" placeholder="Describe the resolution for all selected actions…" rows="3" class="w-full" />
                </div>
                <div class="flex justify-end gap-2 mt-2">
                    <Button label="Cancel" text @click="showBulkCloseDialog = false" />
                    <Button label="Close All" icon="pi pi-check" severity="success" :loading="bulkSaving" :disabled="!bulkCloseNotes.trim()" @click="submitBulkClose" />
                </div>
            </div>
        </Dialog>

        <!-- ── Bulk Reassign Dialog ──────────────────────────────────────────── -->
        <Dialog v-model:visible="showBulkReassignDialog" modal :header="`Reassign ${selectedItems.length} Corrective Actions`" :style="{ width: '420px' }">
            <div class="flex flex-col gap-4">
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">New Assignee</label>
                    <InputText v-model="bulkNewAssignee" placeholder="Name or email" class="w-full" />
                </div>
                <div class="flex justify-end gap-2 mt-2">
                    <Button label="Cancel" text @click="showBulkReassignDialog = false" />
                    <Button label="Reassign All" icon="pi pi-user" :loading="bulkSaving" @click="submitBulkReassign" />
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

const router   = useRouter();
const toast    = useToast();
const apiStore = useApiStore();
function getClient() { return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api); }

// ── Filter state ──────────────────────────────────────────────────────────────
const loading          = ref(false);
const exportingXlsx    = ref(false);
const items            = ref<CorrectiveActionListItemDto[]>([]);
const filterSearch     = ref('');
const filterDivisionId = ref<number | null>(null);
const filterStatus     = ref<string | null>(null);
const filterSource     = ref<string | null>(null);
const filterOverdueOnly = ref(false);

const divisionOptions = ref<{ label: string; value: number }[]>([]);
const divisionsLoaded = ref(false);

const statusOptions = [
    { label: 'Open',        value: 'Open' },
    { label: 'In Progress', value: 'InProgress' },
    { label: 'Closed',      value: 'Closed' },
    { label: 'Voided',      value: 'Voided' },
];

const sourceOptions = [
    { label: 'Manual',         value: 'Manual' },
    { label: 'Auto-Generated', value: 'AutoGenerated' },
];

// ── Selection ─────────────────────────────────────────────────────────────────
const selectedItems = ref<CorrectiveActionListItemDto[]>([]);

// ── Close dialog ──────────────────────────────────────────────────────────────
const showCloseDialog    = ref(false);
const selectedItem       = ref<CorrectiveActionListItemDto | null>(null);
const closeNotes         = ref('');
const closeCompletedDate = ref('');
const closeSaving        = ref(false);

// Closure photo upload
const closurePhoto          = ref<File | null>(null);
const closurePhotoUploading = ref(false);
const closurePhotoUploaded  = ref(false);
const closurePhotoDragging  = ref(false);
const closurePhotoInput     = ref<HTMLInputElement | null>(null);

// ── Edit dialog ───────────────────────────────────────────────────────────────
const showEditDialog  = ref(false);
const editItem        = ref<CorrectiveActionListItemDto | null>(null);
const editDescription = ref('');
const editAssignedTo  = ref('');
const editDueDate     = ref('');
const editSaving      = ref(false);

// ── Bulk action dialogs ───────────────────────────────────────────────────────
const bulkSaving            = ref(false);
const showBulkCloseDialog   = ref(false);
const bulkCloseNotes        = ref('');
const showBulkReassignDialog = ref(false);
const bulkNewAssignee       = ref('');

// ── KPI Counts ────────────────────────────────────────────────────────────────
const openCount       = computed(() => items.value.filter(i => i.status === 'Open').length);
const inProgressCount = computed(() => items.value.filter(i => i.status === 'InProgress').length);
const overdueCount    = computed(() => items.value.filter(i => i.isOverdue).length);
const closedCount     = computed(() => items.value.filter(i => i.status === 'Closed').length);

function useCountUp(source: () => number, duration = 700) {
    const display = ref(0);
    watch(source, (to) => {
        const from  = display.value;
        const start = performance.now();
        function tick(now: number) {
            const t    = Math.min((now - start) / duration, 1);
            const ease = 1 - Math.pow(1 - t, 3);
            display.value = Math.round(from + (to - from) * ease);
            if (t < 1) requestAnimationFrame(tick);
        }
        requestAnimationFrame(tick);
    }, { immediate: true });
    return display;
}

const displayTotal      = useCountUp(() => items.value.length);
const displayOpen       = useCountUp(() => openCount.value);
const displayInProgress = useCountUp(() => inProgressCount.value);
const displayOverdue    = useCountUp(() => overdueCount.value);
const displayClosed     = useCountUp(() => closedCount.value);

// ── Load ──────────────────────────────────────────────────────────────────────
async function load() {
    loading.value = true;
    try {
        const client = getClient();
        const [caList, divisions] = await Promise.all([
            client.getCorrectiveActions({
                divisionId:  filterDivisionId.value,
                status:      filterStatus.value,
                searchText:  filterSearch.value || null,
                source:      filterSource.value,
                overdueOnly: filterOverdueOnly.value,
            }),
            divisionsLoaded.value ? Promise.resolve(null) : client.getDivisions(),
        ]);
        items.value = caList;
        if (divisions) {
            divisionOptions.value = divisions.map(d => ({ label: `${d.code} — ${d.name}`, value: d.id }));
            divisionsLoaded.value = true;
        }
        // Clear selection when data reloads
        selectedItems.value = [];
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load corrective actions.', life: 4000 });
    } finally {
        loading.value = false;
    }
}

// ── Table helpers ─────────────────────────────────────────────────────────────
function rowClass(data: CorrectiveActionListItemDto) {
    if (data.isOverdue)             return 'ca-row-overdue';
    if (data.status === 'Closed')   return 'ca-row-closed';
    if (data.status === 'Voided')   return 'ca-row-voided';
    return '';
}

function statusSeverity(data: CorrectiveActionListItemDto) {
    if (data.isOverdue)               return 'danger';
    if (data.status === 'Closed')     return 'success';
    if (data.status === 'InProgress') return 'warning';
    if (data.status === 'Voided')     return 'secondary';
    return 'info';
}

function statusLabel(status: string) {
    return status === 'InProgress' ? 'In Progress' : status;
}

function goToAudit(auditId: number) {
    router.push({ name: 'audit-management-review', params: { id: String(auditId) } });
}

// ── Close dialog ──────────────────────────────────────────────────────────────
function openCloseDialog(item: CorrectiveActionListItemDto) {
    selectedItem.value       = item;
    closeNotes.value         = '';
    closeCompletedDate.value = new Date().toISOString().split('T')[0];
    // Reset closure photo state
    closurePhoto.value          = null;
    closurePhotoUploading.value = false;
    closurePhotoUploaded.value  = false;
    closurePhotoDragging.value  = false;
    showCloseDialog.value    = true;
}

async function onClosurePhotoSelect(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) await setClosurePhoto(file);
}

async function onClosurePhotoDrop(event: DragEvent) {
    closurePhotoDragging.value = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) await setClosurePhoto(file);
}

async function setClosurePhoto(file: File) {
    closurePhoto.value = file;
    closurePhotoUploading.value = true;
    closurePhotoUploaded.value  = false;
    try {
        await getClient().uploadCaClosurePhoto(selectedItem.value!.id, file);
        closurePhotoUploaded.value = true;
    } catch {
        toast.add({ severity: 'error', summary: 'Upload Failed', detail: 'Could not upload closure photo. Please try again.', life: 4000 });
        closurePhoto.value = null;
    } finally {
        closurePhotoUploading.value = false;
    }
}

async function submitClose() {
    if (!selectedItem.value) return;
    closeSaving.value = true;
    try {
        await getClient().closeCorrectiveAction(selectedItem.value.id, {
            notes:         closeNotes.value,
            completedDate: closeCompletedDate.value || null,
        });
        toast.add({ severity: 'success', summary: 'Closed', detail: 'Corrective action closed.', life: 3000 });
        showCloseDialog.value = false;
        await load();
    } catch (err: any) {
        const msg = err?.response?.data ?? 'Failed to close corrective action.';
        toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 5000 });
    } finally {
        closeSaving.value = false;
    }
}

// ── Edit dialog ───────────────────────────────────────────────────────────────
function openEditDialog(item: CorrectiveActionListItemDto) {
    editItem.value        = item;
    editDescription.value = item.description;
    editAssignedTo.value  = item.assignedTo ?? '';
    editDueDate.value     = item.dueDate ?? '';
    showEditDialog.value  = true;
}

async function submitEdit() {
    if (!editItem.value) return;
    editSaving.value = true;
    try {
        await getClient().updateCorrectiveAction(editItem.value.id, {
            description:     editDescription.value,
            assignedTo:      editAssignedTo.value || null,
            assignedToUserId: editItem.value.assignedToUserId ?? null,
            dueDate:         editDueDate.value || '',
        });
        toast.add({ severity: 'success', summary: 'Saved', detail: 'Corrective action updated.', life: 3000 });
        showEditDialog.value = false;
        await load();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to update corrective action.', life: 4000 });
    } finally {
        editSaving.value = false;
    }
}

// ── Bulk actions ──────────────────────────────────────────────────────────────
function openBulkCloseDialog() {
    bulkCloseNotes.value    = '';
    showBulkCloseDialog.value = true;
}

function openBulkReassignDialog() {
    bulkNewAssignee.value        = '';
    showBulkReassignDialog.value = true;
}

async function bulkChangeStatus(newStatus: string) {
    if (!selectedItems.value.length) return;
    bulkSaving.value = true;
    try {
        const result = await getClient().bulkUpdateCorrectiveActions({
            correctiveActionIds: selectedItems.value.map(i => i.id),
            action:              'status',
            newStatus,
        });
        const msg = `${result.successCount} updated${result.failedIds.length ? `, ${result.failedIds.length} failed` : ''}.`;
        toast.add({ severity: result.failedIds.length ? 'warn' : 'success', summary: 'Bulk Update', detail: msg, life: 4000 });
        await load();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Bulk update failed.', life: 4000 });
    } finally {
        bulkSaving.value = false;
    }
}

async function submitBulkClose() {
    bulkSaving.value = true;
    try {
        const result = await getClient().bulkUpdateCorrectiveActions({
            correctiveActionIds: selectedItems.value.map(i => i.id),
            action:              'status',
            newStatus:           'Closed',
            closureNotes:        bulkCloseNotes.value,
        });
        const msg = `${result.successCount} closed${result.failedIds.length ? `, ${result.failedIds.length} failed` : ''}.`;
        toast.add({ severity: result.failedIds.length ? 'warn' : 'success', summary: 'Bulk Close', detail: msg, life: 4000 });
        showBulkCloseDialog.value = false;
        await load();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Bulk close failed.', life: 4000 });
    } finally {
        bulkSaving.value = false;
    }
}

async function submitBulkReassign() {
    bulkSaving.value = true;
    try {
        const result = await getClient().bulkUpdateCorrectiveActions({
            correctiveActionIds: selectedItems.value.map(i => i.id),
            action:              'reassign',
            newAssignee:         bulkNewAssignee.value || null,
        });
        const msg = `${result.successCount} reassigned${result.failedIds.length ? `, ${result.failedIds.length} failed` : ''}.`;
        toast.add({ severity: result.failedIds.length ? 'warn' : 'success', summary: 'Bulk Reassign', detail: msg, life: 4000 });
        showBulkReassignDialog.value = false;
        await load();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Bulk reassign failed.', life: 4000 });
    } finally {
        bulkSaving.value = false;
    }
}

// ── Excel export ──────────────────────────────────────────────────────────────
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
        const link    = document.createElement('a');
        link.href     = blobUrl;
        link.download = `corrective-actions-${new Date().toISOString().split('T')[0]}.xlsx`;
        link.click();
        URL.revokeObjectURL(blobUrl);
    } finally {
        exportingXlsx.value = false;
    }
}

onMounted(load);
</script>

<style scoped>
:deep(.ca-row-overdue td) { background-color: rgba(220, 38, 38, 0.08) !important; }
:deep(.ca-row-closed td)  { opacity: 0.55; }
:deep(.ca-row-voided td)  { opacity: 0.4; }

.ca-row-actions {
    display: flex;
    gap: 2px;
    justify-content: flex-end;
    opacity: 0;
    transition: opacity 0.15s ease;
}
:deep(tr:hover) .ca-row-actions { opacity: 1; }

.line-clamp-2 {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
.line-clamp-1 {
    display: -webkit-box;
    -webkit-line-clamp: 1;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.kpi-card {
    transition: box-shadow 0.2s ease, transform 0.2s ease;
    cursor: default;
}
.kpi-card:hover {
    box-shadow: 0 0 0 1px rgba(99, 179, 237, 0.3), 0 8px 24px rgba(0, 0, 0, 0.4);
    transform: translateY(-2px);
}
.kpi-card--danger:hover {
    box-shadow: 0 0 0 1px rgba(239, 68, 68, 0.35), 0 8px 24px rgba(0, 0, 0, 0.4);
}

.bulk-bar-enter-active,
.bulk-bar-leave-active { transition: all 0.2s ease; }
.bulk-bar-enter-from,
.bulk-bar-leave-to     { opacity: 0; transform: translateY(-6px); }
</style>

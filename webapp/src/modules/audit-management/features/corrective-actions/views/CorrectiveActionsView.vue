<template>
    <div class="flex flex-col min-h-0">

        <!-- ── Page Header ────────────────────────────────────────────────────── -->
        <BasePageHeader
            icon="pi pi-check-square"
            title="Corrective Actions"
            subtitle="Track and resolve non-conformance corrective actions"
        >
            <Button icon="pi pi-refresh" outlined size="small" :loading="loading" @click="() => load()" />
            <Button icon="pi pi-file-excel" label="Export" outlined size="small" :loading="exportingXlsx" @click="exportExcel" />
        </BasePageHeader>

        <div class="flex flex-col gap-4 p-4 min-h-0 flex-1">

        <!-- ── Filter Bar ─────────────────────────────────────────────────────── -->
        <div class="flex flex-wrap gap-2 items-end">
            <!-- Search -->
            <div class="flex flex-col gap-1 flex-1 min-w-[200px]">
                <label class="text-xs text-slate-400 font-medium">Search</label>
                <div class="relative w-full">
                    <i class="pi pi-search absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm pointer-events-none z-10" />
                    <InputText
                        v-model="filterSearch"
                        placeholder="Description, assignee, question…"
                        class="w-full pl-9"
                        @keyup.enter="loadFiltered"
                    />
                </div>
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
                    @change="loadFiltered"
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
                    @change="loadFiltered"
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
                    @change="loadFiltered"
                />
            </div>

            <!-- Priority -->
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium">Priority</label>
                <Dropdown
                    v-model="filterPriority"
                    :options="priorityOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="All"
                    show-clear
                    class="w-32"
                    @change="loadFiltered"
                />
            </div>

        </div>

        <!-- ── KPI Cards (click to filter) ──────────────────────────────────── -->
        <div class="grid grid-cols-3 sm:grid-cols-5 gap-2 sm:gap-3">
            <MetricCard label="Total"       :value="displayTotal"      variant="default"  interactive :active="activeKpiFilter === 'Total'"       @click="onKpiTotalClick" />
            <MetricCard label="Open"        :value="displayOpen"       variant="info"     interactive :active="activeKpiFilter === 'Open'"        @click="() => onKpiClick('Open')" />
            <MetricCard label="In Progress" :value="displayInProgress" variant="warning"  interactive :active="activeKpiFilter === 'InProgress'"  @click="() => onKpiClick('InProgress')" />
            <MetricCard label="Overdue"     :value="displayOverdue"    variant="danger"   interactive :active="activeKpiFilter === 'Overdue'"     @click="() => onKpiClick('Overdue')" />
            <MetricCard label="Closed"      :value="displayClosed"     variant="success"  interactive :active="activeKpiFilter === 'Closed'"     @click="() => onKpiClick('Closed')" />
        </div>

        <!-- ── Bulk Action Toolbar (visible when rows selected) ───────────────── -->
        <Transition name="bulk-bar">
            <div v-if="selectedItems.length > 0" class="flex items-center gap-3 px-4 py-2.5 rounded-lg" style="background:rgba(59,130,246,0.12); border:1px solid rgba(59,130,246,0.3)">
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
            class="stronghold-table text-sm flex-1 min-h-0"
            data-key="id"
            @row-dblclick="(e: any) => goToAudit(e.data.auditId)"
        >
            <template #empty>
                <div class="flex flex-col items-center justify-center py-16 gap-3">
                    <i class="pi pi-check-circle text-4xl text-slate-600" />
                    <p class="text-slate-400 font-medium">No corrective actions found</p>
                    <p class="text-xs text-slate-500">Try adjusting your filters or search terms</p>
                </div>
            </template>

            <!-- Checkbox column -->
            <Column selection-mode="multiple" style="width:44px; padding: 0 8px" />

            <!-- Division -->
            <Column field="divisionCode" header="Division" sortable style="min-width:100px">
                <template #body="{ data }">
                    <span class="font-medium text-white">{{ data.divisionCode }}</span>
                </template>
            </Column>

            <!-- Audit Tracking # -->
            <Column field="auditTrackingNumber" header="Audit #" sortable style="min-width:120px">
                <template #body="{ data }">
                    <span v-if="data.auditTrackingNumber" class="font-mono text-xs font-semibold text-blue-300">{{ data.auditTrackingNumber }}</span>
                    <span v-else class="text-slate-500 text-xs">—</span>
                </template>
            </Column>

            <!-- Audit Date -->
            <Column field="auditDate" header="Audit Date" sortable style="min-width:100px">
                <template #body="{ data }">
                    <span class="text-slate-400 text-xs">{{ data.auditDate || '—' }}</span>
                </template>
            </Column>

            <!-- Status -->
            <Column field="status" header="Status" sortable style="min-width:120px">
                <template #body="{ data }">
                    <Tag :value="statusLabel(data.status)" :severity="statusSeverity(data)" />
                </template>
            </Column>

            <!-- Description -->
            <Column field="description" header="Description" style="min-width:260px">
                <template #body="{ data }">
                    <div class="flex items-center gap-2">
                        <span class="line-clamp-1 text-slate-200 text-sm flex-1 min-w-0">{{ data.description }}</span>
                        <span
                            v-if="data.priority === 'Urgent'"
                            class="shrink-0 text-[10px] px-1.5 py-0.5 rounded bg-red-900/60 text-red-300 font-bold uppercase tracking-wide"
                        >Urgent</span>
                        <span
                            v-if="data.source === 'AutoGenerated'"
                            class="shrink-0 text-[10px] px-1.5 py-0.5 rounded bg-slate-700 text-slate-400 font-medium"
                        >Auto</span>
                    </div>
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

        <!-- ── Pagination ────────────────────────────────────────────────────── -->
        <div v-if="totalCount > pageSize" class="flex items-center justify-between pt-2 border-t border-slate-700/50">
            <span class="text-xs text-slate-400">
                Showing {{ (currentPage - 1) * pageSize + 1 }}–{{ Math.min(currentPage * pageSize, totalCount) }}
                of {{ totalCount.toLocaleString() }} corrective actions
            </span>
            <Paginator
                :rows="pageSize"
                :total-records="totalCount"
                :first="(currentPage - 1) * pageSize"
                :rows-per-page-options="[25, 50, 100]"
                template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                @page="onPageChange"
            />
        </div>

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
                    <label class="text-sm font-medium">Root Cause <span class="text-slate-400 text-xs font-normal">(optional — fills in if not already set)</span></label>
                    <Textarea v-model="closeRootCause" placeholder="What was the root cause of this finding?" rows="2" class="w-full" />
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
                    <label class="text-sm font-medium">Root Cause</label>
                    <Textarea v-model="editRootCause" placeholder="What was the root cause of this finding?" rows="2" class="w-full" />
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Assigned To</label>
                    <AutoComplete
                        v-model="editAssignedTo"
                        :suggestions="filteredUsers"
                        :option-label="userDisplayName"
                        placeholder="Type name or email…"
                        class="w-full"
                        input-class="w-full"
                        @complete="searchUsers"
                        @item-select="(e) => { editAssignedTo = userDisplayName(e.value); editAssignedToUserId = e.value.userId; }"
                        @clear="editAssignedToUserId = null"
                    />
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
                    <AutoComplete
                        v-model="bulkNewAssignee"
                        :suggestions="filteredUsers"
                        :option-label="userDisplayName"
                        placeholder="Type name or email…"
                        class="w-full"
                        input-class="w-full"
                        force-selection
                        @complete="searchUsers"
                        @item-select="(e) => { bulkNewAssignee = userDisplayName(e.value); bulkNewAssigneeUserId = e.value.userId; }"
                        @clear="bulkNewAssigneeUserId = null"
                    />
                </div>
                <div class="flex justify-end gap-2 mt-2">
                    <Button label="Cancel" text @click="showBulkReassignDialog = false" />
                    <Button label="Reassign All" icon="pi pi-user" :loading="bulkSaving" @click="submitBulkReassign" />
                </div>
            </div>
        </Dialog>
        </div><!-- /.inner content -->
    </div><!-- /.outer wrapper -->
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useToast } from 'primevue/usetoast';
import type { CorrectiveActionListItemDto, UserAuditRoleDto } from '@/apiclient/auditClient';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { MetricCard } from '@/design-system';
import {
    useCorrectiveActions,
    STATUS_OPTIONS as statusOptions,
    SOURCE_OPTIONS as sourceOptions,
    PRIORITY_OPTIONS as priorityOptions,
} from '../composables/useCorrectiveActions';

const toast   = useToast();
const service = useAuditService();

const {
    loading, exportingXlsx, items,
    filterSearch, filterDivisionId, filterStatus, filterSource, filterPriority, filterOverdueOnly,
    currentPage, pageSize, totalCount,
    divisionOptions,
    displayTotal, displayOpen, displayInProgress, displayOverdue, displayClosed,
    load, onPageChange, loadFiltered,
    activeKpiFilter, onKpiClick, onKpiTotalClick,
    rowClass, statusSeverity, statusLabel, goToAudit,
    exportExcel,
} = useCorrectiveActions();

// ── Selection ─────────────────────────────────────────────────────────────────
const selectedItems = ref<CorrectiveActionListItemDto[]>([]);
watch(items, () => { selectedItems.value = []; });

// ── Close dialog ──────────────────────────────────────────────────────────────
const showCloseDialog    = ref(false);
const selectedItem       = ref<CorrectiveActionListItemDto | null>(null);
const closeNotes         = ref('');
const closeRootCause     = ref('');
const closeCompletedDate = ref('');
const closeSaving        = ref(false);

// Closure photo upload
const closurePhoto          = ref<File | null>(null);
const closurePhotoUploading = ref(false);
const closurePhotoUploaded  = ref(false);
const closurePhotoDragging  = ref(false);
const closurePhotoInput     = ref<HTMLInputElement | null>(null);

// ── Edit dialog ───────────────────────────────────────────────────────────────
const showEditDialog       = ref(false);
const editItem             = ref<CorrectiveActionListItemDto | null>(null);
const editDescription      = ref('');
const editRootCause        = ref('');
const editAssignedTo       = ref('');
const editAssignedToUserId = ref<number | null>(null);
const editDueDate          = ref('');
const editSaving           = ref(false);

// ── Bulk action dialogs ───────────────────────────────────────────────────────
const bulkSaving            = ref(false);
const showBulkCloseDialog   = ref(false);
const bulkCloseNotes        = ref('');
const showBulkReassignDialog = ref(false);
const bulkNewAssignee       = ref('');
const bulkNewAssigneeUserId = ref<number | null>(null);

// ── Assignee autocomplete ─────────────────────────────────────────────────────
const auditUsers    = ref<UserAuditRoleDto[]>([]);
const filteredUsers = ref<UserAuditRoleDto[]>([]);

function userDisplayName(u: UserAuditRoleDto): string {
    const name = [u.firstName, u.lastName].filter(Boolean).join(' ');
    return name || u.email || '';
}

function searchUsers(event: { query: string }) {
    const q = event.query.toLowerCase();
    filteredUsers.value = q
        ? auditUsers.value.filter(u =>
            userDisplayName(u).toLowerCase().includes(q) ||
            (u.email ?? '').toLowerCase().includes(q))
        : auditUsers.value.slice(0, 20);
}

async function loadAuditUsers() {
    try {
        auditUsers.value = await service.getUsersWithAuditRoles();
    } catch {
        // Non-admin roles may not have access; autocomplete degrades to free-text
    }
}

// ── Close dialog ──────────────────────────────────────────────────────────────
function openCloseDialog(item: CorrectiveActionListItemDto) {
    selectedItem.value       = item;
    closeNotes.value         = '';
    closeRootCause.value     = item.rootCause ?? '';
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
        await service.uploadCaClosurePhoto(selectedItem.value!.id, file);
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
        await service.closeCorrectiveAction(selectedItem.value.id, {
            notes:         closeNotes.value,
            completedDate: closeCompletedDate.value || null,
            rootCause:     closeRootCause.value || null,
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
    editItem.value             = item;
    editDescription.value      = item.description;
    editRootCause.value        = item.rootCause ?? '';
    editAssignedTo.value       = item.assignedTo ?? '';
    editAssignedToUserId.value = item.assignedToUserId ?? null;
    editDueDate.value          = item.dueDate ?? '';
    showEditDialog.value       = true;
}

async function submitEdit() {
    if (!editItem.value) return;
    editSaving.value = true;
    try {
        await service.updateCorrectiveAction(editItem.value.id, {
            description:      editDescription.value,
            rootCause:        editRootCause.value || null,
            assignedTo:       editAssignedTo.value || null,
            assignedToUserId: editAssignedToUserId.value,
            dueDate:          editDueDate.value || '',
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
    const requiresPhoto = selectedItems.value.filter(i => i.requireClosurePhoto);
    if (requiresPhoto.length) {
        toast.add({
            severity: 'warn',
            summary: 'Bulk Close Blocked',
            detail: `${requiresPhoto.length} selected item${requiresPhoto.length > 1 ? 's require' : ' requires'} a closure photo and cannot be bulk-closed. Remove ${requiresPhoto.length > 1 ? 'them' : 'it'} from your selection or close ${requiresPhoto.length > 1 ? 'them' : 'it'} individually.`,
            life: 6000,
        });
        return;
    }
    bulkCloseNotes.value    = '';
    showBulkCloseDialog.value = true;
}

function openBulkReassignDialog() {
    bulkNewAssignee.value       = '';
    bulkNewAssigneeUserId.value = null;
    showBulkReassignDialog.value = true;
}

async function bulkChangeStatus(newStatus: string) {
    if (!selectedItems.value.length) return;
    bulkSaving.value = true;
    try {
        const result = await service.bulkUpdateCorrectiveActions({
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
        const result = await service.bulkUpdateCorrectiveActions({
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
        const result = await service.bulkUpdateCorrectiveActions({
            correctiveActionIds: selectedItems.value.map(i => i.id),
            action:              'reassign',
            newAssignee:         bulkNewAssignee.value || null,
            newAssigneeUserId:   bulkNewAssigneeUserId.value,
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

onMounted(() => { load(); loadAuditUsers(); });
</script>

<style scoped>
/* Row state (overdue/closed/voided) and opacity are in global style.css.
   Only action-reveal behavior lives here (scoped to this component). */
.ca-row-actions {
    display: flex;
    gap: 2px;
    justify-content: flex-end;
    opacity: 1;
    transition: opacity 0.15s ease;
}
@media (hover: hover) and (pointer: fine) {
    .ca-row-actions { opacity: 0; }
    :deep(tr:hover) .ca-row-actions { opacity: 1; }
}

.line-clamp-1 {
    display: -webkit-box;
    -webkit-line-clamp: 1;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.bulk-bar-enter-active,
.bulk-bar-leave-active { transition: all 0.2s ease; }
.bulk-bar-enter-from,
.bulk-bar-leave-to     { opacity: 0; transform: translateY(-6px); }
</style>

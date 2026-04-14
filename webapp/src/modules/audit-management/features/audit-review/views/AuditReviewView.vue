<template>
    <div>
        <BasePageHeader
            icon="pi pi-eye"
            :title="review ? `${review.divisionCode} Audit Review` : 'Audit Review'"
            :subtitle="review ? `${review.divisionName} — ${review.status}` : ''"
        >
            <Button
                label="Back to Form"
                icon="pi pi-arrow-left"
                severity="secondary"
                outlined
                @click="router.push(`/audit-management/audits/${route.params.id}`)"
            />
            <Button
                v-if="review"
                label="Print / PDF"
                icon="pi pi-print"
                severity="secondary"
                @click="printPage"
            />
            <Button
                v-if="review && review.reviewEmailRouting.length > 0"
                label="Send for Review"
                icon="pi pi-envelope"
                @click="openEmailClient"
            />
            <Button
                v-if="review && (review.status === 'Submitted' || review.status === 'Closed')"
                label="Reopen"
                icon="pi pi-refresh"
                severity="warning"
                outlined
                @click="showReopenDialog = true"
            />
            <Button
                v-if="review && (review.status === 'Submitted' || review.status === 'Reopened')"
                label="Close Audit"
                icon="pi pi-check-circle"
                severity="success"
                @click="showCloseAuditDialog = true"
            />
        </BasePageHeader>

        <!-- Reopen Audit Dialog -->
        <Dialog v-model:visible="showReopenDialog" modal header="Reopen Audit" :style="{ width: '420px' }">
            <div class="space-y-3 py-2">
                <p class="text-sm text-slate-300">This will set the audit back to <strong>Reopened</strong> so responses can be edited.</p>
                <div>
                    <label class="text-xs text-slate-400 block mb-1">Reason (optional)</label>
                    <Textarea v-model="reopenReason" rows="3" class="w-full text-sm" placeholder="Reason for reopening…" autoResize />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" severity="secondary" text @click="showReopenDialog = false" />
                <Button label="Reopen" icon="pi pi-refresh" severity="warning" :loading="auditActionSaving" @click="submitReopen" />
            </template>
        </Dialog>

        <!-- Close Audit Dialog -->
        <Dialog v-model:visible="showCloseAuditDialog" modal header="Close Audit" :style="{ width: '420px' }">
            <div class="space-y-3 py-2">
                <p class="text-sm text-slate-300">Closing the audit marks it as <strong>Closed</strong>. All corrective actions should be resolved first.</p>
                <div>
                    <label class="text-xs text-slate-400 block mb-1">Closing Notes (optional)</label>
                    <Textarea v-model="closeAuditNotes" rows="3" class="w-full text-sm" placeholder="Any final notes…" autoResize />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" severity="secondary" text @click="showCloseAuditDialog = false" />
                <Button label="Close Audit" icon="pi pi-check-circle" severity="success" :loading="auditActionSaving" @click="submitCloseAudit" />
            </template>
        </Dialog>

        <div v-if="store.reviewLoading" class="flex justify-center py-12">
            <ProgressSpinner />
        </div>

        <div v-else-if="review" class="p-4 space-y-4">

            <!-- Score summary card -->
            <Card>
                <template #title>
                    <span class="text-base font-semibold text-white">Score Summary</span>
                </template>
                <template #content>
                    <div class="flex flex-wrap gap-6 items-center">
                        <!-- SVG score ring -->
                        <div class="relative flex-shrink-0" style="width:100px;height:100px;">
                            <svg width="100" height="100" viewBox="0 0 100 100">
                                <circle cx="50" cy="50" r="42" fill="none" stroke="rgba(100,116,139,0.25)" stroke-width="9" />
                                <circle
                                    cx="50" cy="50" r="42"
                                    fill="none"
                                    :stroke="ringColor"
                                    stroke-width="9"
                                    stroke-linecap="round"
                                    :stroke-dasharray="ringCircumference"
                                    :stroke-dashoffset="ringDashoffset"
                                    transform="rotate(-90 50 50)"
                                    style="transition: stroke-dashoffset 1.1s cubic-bezier(0.4,0,0.2,1), stroke 0.5s ease"
                                />
                            </svg>
                            <div class="absolute inset-0 flex flex-col items-center justify-center">
                                <span :class="['text-lg font-bold leading-none', scoreColor]">{{ scoreDisplay }}</span>
                                <span class="text-[9px] text-slate-400 mt-0.5 tracking-wide uppercase">Score</span>
                            </div>
                        </div>
                        <div class="flex flex-wrap gap-3 flex-1">
                            <div class="flex flex-col items-center bg-emerald-900/50 border border-emerald-700 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-emerald-300">{{ review.conformingCount }}</span>
                                <span class="text-xs text-emerald-400">Conforming</span>
                            </div>
                            <div class="flex flex-col items-center bg-red-900/50 border border-red-700 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-red-300">{{ review.nonConformingCount }}</span>
                                <span class="text-xs text-red-400">Non-Conforming</span>
                            </div>
                            <div class="flex flex-col items-center bg-amber-900/50 border border-amber-700 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-amber-300">{{ review.warningCount }}</span>
                                <span class="text-xs text-amber-400">Warning</span>
                            </div>
                            <div class="flex flex-col items-center bg-slate-700 border border-slate-600 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-slate-300">{{ review.naCount }}</span>
                                <span class="text-xs text-slate-400">N/A</span>
                            </div>
                            <div v-if="review.unansweredCount > 0" class="flex flex-col items-center bg-slate-800 border border-slate-700 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-slate-500">{{ review.unansweredCount }}</span>
                                <span class="text-xs text-slate-500">Unanswered</span>
                            </div>
                        </div>
                    </div>
                    <div v-if="review.scorePercent != null" class="mt-4">
                        <div class="w-full h-3 bg-slate-700 rounded-full overflow-hidden">
                            <div class="h-full rounded-full transition-all duration-700" :class="barColor" :style="{ width: `${review.scorePercent}%` }" />
                        </div>
                    </div>
                </template>
            </Card>

            <!-- Audit header info -->
            <Card v-if="review.header">
                <template #title>
                    <span class="text-base font-semibold text-white">Audit Information</span>
                </template>
                <template #content>
                    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                        <div v-if="review.header.auditDate"><p class="text-slate-400 text-xs">Date</p><p class="text-white">{{ review.header.auditDate }}</p></div>
                        <div v-if="review.header.auditor"><p class="text-slate-400 text-xs">Auditor</p><p class="text-white">{{ review.header.auditor }}</p></div>
                        <div v-if="review.header.jobNumber"><p class="text-slate-400 text-xs">Job Number</p><p class="text-white">{{ review.header.jobNumber }}</p></div>
                        <div v-if="review.header.client"><p class="text-slate-400 text-xs">Client</p><p class="text-white">{{ review.header.client }}</p></div>
                        <div v-if="review.header.location"><p class="text-slate-400 text-xs">Location</p><p class="text-white">{{ review.header.location }}</p></div>
                        <div v-if="review.header.pm"><p class="text-slate-400 text-xs">Project Manager</p><p class="text-white">{{ review.header.pm }}</p></div>
                        <div v-if="review.header.company1"><p class="text-slate-400 text-xs">Company</p><p class="text-white">{{ review.header.company1 }}</p></div>
                        <div v-if="review.header.responsibleParty"><p class="text-slate-400 text-xs">Responsible Party</p><p class="text-white">{{ review.header.responsibleParty }}</p></div>
                    </div>
                </template>
            </Card>

            <!-- Non-conforming findings with CA workflow -->
            <Card v-if="review.nonConformingItems.length > 0">
                <template #title>
                    <span class="text-base font-semibold text-red-300">
                        Non-Conforming Items ({{ review.nonConformingItems.length }})
                    </span>
                </template>
                <template #content>
                    <div class="space-y-4">
                        <div
                            v-for="(item, idx) in review.nonConformingItems"
                            :key="item.id"
                            class="border border-red-800 rounded-lg p-3 bg-red-950/20"
                        >
                            <!-- Finding header -->
                            <div class="flex items-start justify-between gap-2">
                                <p class="text-sm text-slate-200 flex-1">
                                    <span class="text-slate-500 mr-1">{{ idx + 1 }}.</span>
                                    {{ item.questionText }}
                                </p>
                                <div class="flex items-center gap-2 shrink-0">
                                    <span v-if="item.correctedOnSite" class="text-xs bg-emerald-900 border border-emerald-700 text-emerald-300 rounded px-1.5 py-0.5">Corrected On-Site</span>
                                    <span v-else class="text-xs bg-slate-700 border border-slate-600 text-slate-400 rounded px-1.5 py-0.5">Not Corrected</span>
                                    <Button
                                        v-if="!item.correctedOnSite"
                                        label="Assign CA"
                                        icon="pi pi-plus"
                                        size="small"
                                        severity="warning"
                                        outlined
                                        @click="openAssignModal(item)"
                                    />
                                </div>
                            </div>
                            <p v-if="item.comment" class="text-xs text-slate-400 mt-1 italic">"{{ item.comment }}"</p>

                            <!-- Corrective Actions list -->
                            <div v-if="item.correctiveActions.length > 0" class="mt-3 space-y-2">
                                <div
                                    v-for="ca in item.correctiveActions"
                                    :key="ca.id"
                                    class="flex items-center justify-between gap-2 bg-slate-800/60 border border-slate-700 rounded px-3 py-2 text-xs"
                                >
                                    <div class="flex-1">
                                        <p class="text-slate-200 font-medium">{{ ca.description }}</p>
                                        <p class="text-slate-400 mt-0.5">
                                            <span v-if="ca.assignedTo">Assigned to: <span class="text-slate-300">{{ ca.assignedTo }}</span></span>
                                            <span v-if="ca.dueDate" class="ml-2">Due: <span class="text-slate-300">{{ ca.dueDate }}</span></span>
                                        </p>
                                    </div>
                                    <div class="flex items-center gap-2">
                                        <Tag :value="ca.status" :severity="caSeverity(ca.status)" />
                                        <Button
                                            v-if="ca.status !== 'Closed'"
                                            label="Close"
                                            size="small"
                                            severity="success"
                                            outlined
                                            @click="openCloseModal(ca)"
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </template>
            </Card>

            <div v-else class="bg-emerald-900/20 border border-emerald-800 rounded-lg p-4 text-center">
                <i class="pi pi-check-circle text-2xl text-emerald-400 mb-2 block" />
                <p class="text-emerald-300 font-medium">No non-conforming items</p>
            </div>

            <!-- Warning items -->
            <Card v-if="(review.warningItems?.length ?? 0) > 0">
                <template #title>
                    <span class="text-base font-semibold text-amber-300">
                        Warnings ({{ review.warningItems.length }})
                    </span>
                </template>
                <template #content>
                    <div class="space-y-2">
                        <div
                            v-for="(item, idx) in review.warningItems"
                            :key="item.questionId"
                            class="border border-amber-800 rounded-lg p-3 bg-amber-950/20"
                        >
                            <p class="text-sm text-slate-200">
                                <span class="text-slate-500 mr-1">{{ idx + 1 }}.</span>
                                {{ item.questionText }}
                            </p>
                            <p v-if="item.comment" class="text-xs text-slate-400 mt-1 italic">"{{ item.comment }}"</p>
                        </div>
                    </div>
                </template>
            </Card>

            <!-- Full audit record (collapsible) -->
            <Card v-if="(review.sections?.length ?? 0) > 0">
                <template #title>
                    <div class="flex items-center justify-between">
                        <span class="text-base font-semibold text-white">Full Audit Record</span>
                        <button
                            class="text-xs text-slate-400 hover:text-slate-200 flex items-center gap-1"
                            @click="showFullRecord = !showFullRecord"
                        >
                            <i :class="showFullRecord ? 'pi pi-chevron-up' : 'pi pi-chevron-down'" />
                            {{ showFullRecord ? 'Collapse' : 'Expand' }}
                        </button>
                    </div>
                </template>
                <template #content>
                    <div v-if="showFullRecord" class="space-y-4">
                        <div v-for="section in review.sections" :key="section.sectionName">
                            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide mb-2">{{ section.sectionName }}</p>
                            <div class="divide-y divide-slate-700 border border-slate-700 rounded-lg overflow-hidden">
                                <div
                                    v-for="item in section.items"
                                    :key="item.questionId"
                                    :class="[
                                        'flex items-start gap-3 px-3 py-2 text-sm',
                                        item.status === 'NonConforming' ? 'bg-red-950/30' :
                                        item.status === 'Warning' ? 'bg-amber-950/20' : ''
                                    ]"
                                >
                                    <span :class="['shrink-0 w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold mt-0.5', statusDotClass(item.status)]" />
                                    <span class="flex-1 text-slate-300">{{ item.questionText }}</span>
                                    <span :class="['text-xs font-semibold shrink-0 mt-0.5', statusTextClass(item.status)]">
                                        {{ item.status ?? 'Unanswered' }}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <p v-else class="text-slate-500 text-sm italic">Click Expand to view all {{ review.sections.reduce((n, s) => n + s.items.length, 0) }} responses</p>
                </template>
            </Card>

            <!-- Email routing info -->
            <Card v-if="review.reviewEmailRouting.length > 0">
                <template #title><span class="text-base font-semibold text-white">Review Recipients</span></template>
                <template #content>
                    <ul class="text-sm text-slate-300 space-y-1">
                        <li v-for="r in review.reviewEmailRouting" :key="r.emailAddress" class="flex items-center gap-2">
                            <i class="pi pi-envelope text-slate-500 text-xs" />{{ r.emailAddress }}
                        </li>
                    </ul>
                </template>
            </Card>
        </div>

        <div v-else class="p-4 text-center text-slate-400">Review not available.</div>
    </div>

    <!-- Assign CA modal -->
    <Dialog v-model:visible="showAssign" header="Assign Corrective Action" modal style="width: 480px">
        <div class="space-y-4 pt-2">
            <div>
                <label class="text-xs text-slate-400 block mb-1">Finding</label>
                <p class="text-sm text-slate-200">{{ assignTarget?.questionText }}</p>
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Description / Action Required *</label>
                <Textarea v-model="assignForm.description" rows="3" class="w-full text-sm" placeholder="Describe the corrective action required..." autoResize />
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Assign To</label>
                <InputText v-model="assignForm.assignedTo" class="w-full text-sm" placeholder="Name or email" />
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Due Date</label>
                <InputText v-model="assignForm.dueDate" class="w-full text-sm" placeholder="YYYY-MM-DD" />
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" @click="showAssign = false" />
            <Button label="Assign" icon="pi pi-check" :loading="saving" :disabled="!assignForm.description.trim()" @click="submitAssign" />
        </template>
    </Dialog>

    <!-- Close CA modal -->
    <Dialog v-model:visible="showClose" header="Close Corrective Action" modal style="width: 420px">
        <div class="space-y-4 pt-2">
            <div>
                <label class="text-xs text-slate-400 block mb-1">Resolution Notes *</label>
                <Textarea v-model="closeForm.notes" rows="3" class="w-full text-sm" placeholder="Describe how this was resolved..." autoResize />
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Completion Date</label>
                <InputText v-model="closeForm.completedDate" class="w-full text-sm" placeholder="YYYY-MM-DD (leave blank for today)" />
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" @click="showClose = false" />
            <Button label="Close CA" icon="pi pi-check" severity="success" :loading="saving" :disabled="!closeForm.notes.trim()" @click="submitClose" />
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import ProgressSpinner from 'primevue/progressspinner';
import Card from 'primevue/card';
import Dialog from 'primevue/dialog';
import Tag from 'primevue/tag';
import Textarea from 'primevue/textarea';
import InputText from 'primevue/inputtext';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type AuditFindingDto, type CorrectiveActionDto } from '@/apiclient/auditClient';

const router = useRouter();
const route = useRoute();
const store = useAuditStore();
const apiStore = useApiStore();
const toast = useToast();

const review = computed(() => store.review);
const saving = ref(false);
const showFullRecord = ref(false);

// ── Assign modal ──────────────────────────────────────────────────────────────
const showAssign = ref(false);
const assignTarget = ref<AuditFindingDto | null>(null);
const assignForm = ref({ description: '', assignedTo: '', dueDate: '' });

function openAssignModal(item: AuditFindingDto) {
    assignTarget.value = item;
    assignForm.value = { description: '', assignedTo: '', dueDate: '' };
    showAssign.value = true;
}

// ── Reopen / Close audit ──────────────────────────────────────────────────────
const showReopenDialog = ref(false);
const showCloseAuditDialog = ref(false);
const reopenReason = ref('');
const closeAuditNotes = ref('');
const auditActionSaving = ref(false);

async function submitReopen() {
    const id = Number(route.params.id);
    auditActionSaving.value = true;
    try {
        await getClient().reopenAudit(id, reopenReason.value || null);
        toast.add({ severity: 'warn', summary: 'Reopened', detail: 'Audit has been reopened.', life: 3000 });
        showReopenDialog.value = false;
        reopenReason.value = '';
        await store.loadReview(id);
    } catch (e: unknown) {
        toast.add({ severity: 'error', summary: 'Error', detail: (e as Error)?.message ?? 'Failed to reopen.', life: 4000 });
    } finally {
        auditActionSaving.value = false;
    }
}

async function submitCloseAudit() {
    const id = Number(route.params.id);
    auditActionSaving.value = true;
    try {
        await getClient().closeAudit(id, closeAuditNotes.value || null);
        toast.add({ severity: 'success', summary: 'Closed', detail: 'Audit has been closed.', life: 3000 });
        showCloseAuditDialog.value = false;
        closeAuditNotes.value = '';
        await store.loadReview(id);
    } catch (e: unknown) {
        toast.add({ severity: 'error', summary: 'Error', detail: (e as Error)?.message ?? 'Failed to close.', life: 4000 });
    } finally {
        auditActionSaving.value = false;
    }
}

// ── Close CA modal ─────────────────────────────────────────────────────────────
const showClose = ref(false);
const closeTarget = ref<CorrectiveActionDto | null>(null);
const closeForm = ref({ notes: '', completedDate: '' });

function openCloseModal(ca: CorrectiveActionDto) {
    closeTarget.value = ca;
    closeForm.value = { notes: '', completedDate: '' };
    showClose.value = true;
}

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}

async function submitAssign() {
    if (!assignTarget.value) return;
    saving.value = true;
    try {
        await getClient().assignCorrectiveAction({
            findingId: assignTarget.value.id,
            description: assignForm.value.description,
            assignedTo: assignForm.value.assignedTo || null,
            dueDate: assignForm.value.dueDate || null,
        });
        toast.add({ severity: 'success', summary: 'Assigned', detail: 'Corrective action assigned.', life: 2500 });
        showAssign.value = false;
        // Reload review to get updated CA list
        const id = Number(route.params.id);
        await store.loadReview(id);
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign corrective action.', life: 4000 });
    } finally {
        saving.value = false;
    }
}

async function submitClose() {
    if (!closeTarget.value) return;
    saving.value = true;
    try {
        await getClient().closeCorrectiveAction(closeTarget.value.id, {
            notes: closeForm.value.notes,
            completedDate: closeForm.value.completedDate || null,
        });
        toast.add({ severity: 'success', summary: 'Closed', detail: 'Corrective action closed.', life: 2500 });
        showClose.value = false;
        const id = Number(route.params.id);
        await store.loadReview(id);
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to close corrective action.', life: 4000 });
    } finally {
        saving.value = false;
    }
}

onMounted(async () => {
    const id = Number(route.params.id);
    if (!isNaN(id)) await store.loadReview(id);
});

const scoreDisplay = computed(() => {
    const pct = review.value?.scorePercent;
    if (pct == null) return '—';
    return `${Math.round(pct)}%`;
});

const scoreColor = computed(() => {
    const pct = review.value?.scorePercent;
    if (pct == null) return 'text-slate-400';
    if (pct >= 90) return 'text-emerald-400';
    if (pct >= 75) return 'text-amber-400';
    return 'text-red-400';
});

const barColor = computed(() => {
    const pct = review.value?.scorePercent;
    if (pct == null) return 'bg-slate-500';
    if (pct >= 90) return 'bg-emerald-500';
    if (pct >= 75) return 'bg-amber-500';
    return 'bg-red-500';
});

// ── Score ring ────────────────────────────────────────────────────────────────
const ringCircumference = 2 * Math.PI * 42; // r=42
const ringDashoffset = computed(() => {
    const pct = review.value?.scorePercent ?? 0;
    return ringCircumference - (pct / 100) * ringCircumference;
});
const ringColor = computed(() => {
    const pct = review.value?.scorePercent;
    if (pct == null) return '#475569';
    if (pct >= 90) return '#34d399';
    if (pct >= 75) return '#fbbf24';
    return '#f87171';
});

function caSeverity(status: string): string {
    const map: Record<string, string> = { Open: 'danger', InProgress: 'warning', Closed: 'success' };
    return map[status] ?? 'secondary';
}

function statusDotClass(status: string | null | undefined): string {
    switch (status) {
        case 'Conforming':    return 'bg-emerald-500';
        case 'NonConforming': return 'bg-red-500';
        case 'Warning':       return 'bg-amber-500';
        case 'NA':            return 'bg-slate-500';
        default:              return 'bg-slate-700 border border-slate-500';
    }
}

function statusTextClass(status: string | null | undefined): string {
    switch (status) {
        case 'Conforming':    return 'text-emerald-400';
        case 'NonConforming': return 'text-red-400';
        case 'Warning':       return 'text-amber-400';
        case 'NA':            return 'text-slate-500';
        default:              return 'text-slate-600';
    }
}

async function printPage() {
    const wasOpen = showFullRecord.value;
    showFullRecord.value = true;
    await nextTick();
    window.print();
    showFullRecord.value = wasOpen;
}

function openEmailClient() {
    if (!review.value) return;
    const r = review.value;
    const to = r.reviewEmailRouting.map(x => x.emailAddress).join(';');
    const subject = encodeURIComponent(`[For Review] ${r.divisionCode} Compliance Audit`);
    const hdr = r.header;
    const headerLines = [
        hdr?.auditDate ? `Date: ${hdr.auditDate}` : '',
        hdr?.auditor ? `Auditor: ${hdr.auditor}` : '',
        hdr?.jobNumber ? `Job #: ${hdr.jobNumber}` : '',
        hdr?.location ? `Location: ${hdr.location}` : '',
    ].filter(Boolean).join('\n');
    const ncLines = r.nonConformingItems
        .map((item, i) => `${i + 1}. ${item.questionText}` + (item.correctedOnSite ? ' [Corrected On-Site]' : '') + (item.comment ? `\n   Comment: ${item.comment}` : ''))
        .join('\n');
    const body = encodeURIComponent(
        `${r.divisionName} Compliance Audit\nScore: ${scoreDisplay.value}\nStatus: ${r.status}\n\n${headerLines}\n\n` +
        (r.nonConformingItems.length > 0 ? `Non-Conforming Items (${r.nonConformingItems.length}):\n${ncLines}\n` : 'No non-conforming items.\n'),
    );
    window.open(`mailto:${to}?subject=${subject}&body=${body}`, '_blank');
}
</script>

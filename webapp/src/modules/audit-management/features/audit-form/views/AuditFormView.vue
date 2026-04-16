<template>
    <div class="pb-20">
        <!-- Page header -->
        <BasePageHeader
            icon="pi pi-clipboard"
            :title="store.template ? `${store.template.divisionCode} Compliance Audit` : 'Audit Form'"
            :subtitle="store.template ? store.template.divisionName : ''"
        >
            <Tag
                v-if="store.auditStatus"
                :value="store.auditStatus"
                :severity="statusSeverity(store.auditStatus)"
                class="mr-2"
            />
            <Button
                v-if="!store.isSubmitted"
                label="Collapse All"
                icon="pi pi-minus"
                severity="secondary"
                size="small"
                outlined
                @click="collapseAll"
            />
            <Button
                v-if="!store.isSubmitted"
                label="Expand All"
                icon="pi pi-plus"
                severity="secondary"
                size="small"
                outlined
                @click="expandAll"
            />
            <BaseButtonSave
                v-if="!store.isSubmitted"
                label="Save Draft"
                :loading="store.saving"
                @click="store.saveDraft()"
            />
            <Button
                v-if="!store.isSubmitted"
                label="Submit for Review"
                icon="pi pi-send"
                :loading="store.saving"
                @click="onSubmit"
            />
            <Button
                v-if="!store.isSubmitted"
                label="Delete Draft"
                icon="pi pi-trash"
                severity="danger"
                outlined
                :loading="store.saving"
                @click="onDeleteDraft"
            />
            <Button
                v-if="store.isSubmitted"
                label="View Review"
                icon="pi pi-eye"
                severity="info"
                @click="router.push(`/audit-management/audits/${store.auditId}/review`)"
            />
        </BasePageHeader>

        <!-- Loading state -->
        <div v-if="store.loading" class="flex justify-center py-16">
            <ProgressSpinner />
        </div>

        <template v-else-if="store.template">
            <!-- Draft restore modal -->
            <Dialog
                v-model:visible="store.hasPendingDraft"
                header="Unsaved Draft Found"
                modal
                :closable="false"
                style="width: 400px"
            >
                <p class="text-sm text-slate-300 mb-4">
                    An unsaved local draft was found for this audit. Would you like to restore it?
                </p>
                <template #footer>
                    <Button label="Discard" severity="secondary" @click="store.discardPendingDraft()" />
                    <Button label="Restore Draft" @click="store.acceptPendingDraft()" />
                </template>
            </Dialog>

            <!-- Audit Header fields -->
            <div class="px-4 pt-2">
                <AuditHeader
                    :header="store.header"
                    :audit-type="store.template.auditType"
                    :disabled="store.isSubmitted"
                />
            </div>

            <!-- Attachments -->
            <div class="px-4">
                <AuditAttachments
                    v-if="store.auditId"
                    :audit-id="store.auditId"
                    :readonly="store.isSubmitted"
                />
            </div>

            <!-- Sections (filtered to enabled optional groups) -->
            <div class="px-4">
                <AuditSection
                    v-for="section in store.visibleSections"
                    :key="section.id"
                    :ref="el => { if (el) sectionRefs.set(section.id, el as unknown as SectionRef) }"
                    :section="section"
                    :start-open="true"
                    :disabled="store.isSubmitted"
                />
            </div>
        </template>

        <!-- Score bar (always visible when form is loaded) -->
        <ScoreSummaryBar
            v-if="store.template"
            :counts="store.score.counts"
            :score-percent="store.score.scorePercent"
        />
    </div>

    <!-- Submit confirmation -->
    <ConfirmDialog />

    <!-- Post-submit summary modal -->
    <Dialog
        v-model:visible="showSummary"
        header="Audit Submitted"
        modal
        :closable="false"
        style="width: 560px; max-width: 96vw;"
    >
        <!-- Score banner -->
        <div class="flex items-center justify-center gap-6 mb-5 p-4 rounded-lg"
            :class="summaryScoreClass">
            <div class="text-center">
                <div class="text-5xl font-bold">
                    {{ store.score.scorePercent !== null ? `${store.score.scorePercent.toFixed(1)}%` : '—' }}
                </div>
                <div class="text-sm mt-1 opacity-80">Conformance Score</div>
            </div>
            <div class="grid grid-cols-2 gap-x-6 gap-y-1 text-sm">
                <span class="text-green-300">Conforming</span>
                <span class="font-semibold">{{ store.score.counts.conforming }}</span>
                <span class="text-red-300">Non-Conforming</span>
                <span class="font-semibold">{{ store.score.counts.nonConforming }}</span>
                <span class="text-amber-300">Warning</span>
                <span class="font-semibold">{{ store.score.counts.warning }}</span>
                <span class="text-slate-400">N/A</span>
                <span class="font-semibold">{{ store.score.counts.na }}</span>
            </div>
        </div>

        <!-- NC items -->
        <div v-if="summaryNcItems.length" class="mb-4">
            <p class="text-xs font-bold uppercase text-red-400 tracking-wide mb-1">Non-Conforming Items</p>
            <div class="space-y-1 max-h-40 overflow-y-auto pr-1">
                <div v-for="item in summaryNcItems" :key="item.questionId"
                    class="bg-red-950/40 border border-red-800/50 rounded px-2 py-1.5 text-xs">
                    <p class="text-slate-200">{{ item.questionTextSnapshot }}</p>
                    <p v-if="item.comment" class="text-slate-400 mt-0.5 italic">{{ item.comment }}</p>
                </div>
            </div>
        </div>

        <!-- Warning items -->
        <div v-if="summaryWarningItems.length" class="mb-4">
            <p class="text-xs font-bold uppercase text-amber-400 tracking-wide mb-1">Warnings</p>
            <div class="space-y-1 max-h-32 overflow-y-auto pr-1">
                <div v-for="item in summaryWarningItems" :key="item.questionId"
                    class="bg-amber-950/40 border border-amber-800/50 rounded px-2 py-1.5 text-xs">
                    <p class="text-slate-200">{{ item.questionTextSnapshot }}</p>
                    <p v-if="item.comment" class="text-slate-400 mt-0.5 italic">{{ item.comment }}</p>
                </div>
            </div>
        </div>

        <p v-if="!summaryNcItems.length && !summaryWarningItems.length"
            class="text-sm text-green-400 mb-4">
            All items are conforming. Great work!
        </p>

        <template #footer>
            <Button label="Back to Audits" severity="secondary" outlined @click="goBackToAudits" />
            <Button label="View Full Review" icon="pi pi-eye" @click="goToReview" />
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm';
import Tag from 'primevue/tag';
import Dialog from 'primevue/dialog';
import ConfirmDialog from 'primevue/confirmdialog';
import ProgressSpinner from 'primevue/progressspinner';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import BaseButtonSave from '@/components/buttons/BaseButtonSave.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import AuditHeader from '../components/AuditHeader.vue';
import AuditSection from '../components/AuditSection.vue';
import ScoreSummaryBar from '../components/ScoreSummaryBar.vue';
import AuditAttachments from '../components/AuditAttachments.vue';

interface SectionRef {
    toggleOpen(): void;
    isOpen: boolean;
}

const router = useRouter();
const route = useRoute();
const store = useAuditStore();
const confirm = useConfirm();
const sectionRefs = ref<Map<number, SectionRef>>(new Map());
const showSummary = ref(false);

const summaryNcItems = computed(() =>
    Array.from(store.responses.values()).filter(r => r.status === 'NonConforming'),
);
const summaryWarningItems = computed(() =>
    Array.from(store.responses.values()).filter(r => r.status === 'Warning'),
);
const summaryScoreClass = computed(() => {
    const pct = store.score.scorePercent;
    if (pct === null) return 'bg-slate-700/60 text-slate-200';
    if (pct >= 90) return 'bg-green-900/60 text-green-100';
    if (pct >= 70) return 'bg-amber-900/60 text-amber-100';
    return 'bg-red-900/60 text-red-100';
});

onMounted(async () => {
    const id = Number(route.params.id);
    if (!isNaN(id)) {
        await store.loadAudit(id);
    }
});

function collapseAll() {
    for (const ref of sectionRefs.value.values()) {
        if (ref.isOpen) ref.toggleOpen();
    }
}

function expandAll() {
    for (const ref of sectionRefs.value.values()) {
        if (!ref.isOpen) ref.toggleOpen();
    }
}

function statusSeverity(status: string): string {
    const map: Record<string, string> = {
        Draft: 'warning',
        Submitted: 'info',
        Reopened: 'warning',
        Closed: 'success',
    };
    return map[status] ?? 'secondary';
}

function onDeleteDraft() {
    confirm.require({
        message: 'Permanently delete this draft audit? This cannot be undone.',
        header: 'Delete Draft',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Delete',
        rejectLabel: 'Cancel',
        acceptClass: 'p-button-danger',
        accept: async () => {
            if (!store.auditId) return;
            const id = store.auditId;
            const ok = await store.deleteAudit(id);
            if (ok) {
                store.resetForm();
                router.push('/audit-management/audits');
            }
        },
    });
}

function goToReview() {
    showSummary.value = false;
    router.push(`/audit-management/audits/${store.auditId}/review`);
}

function goBackToAudits() {
    showSummary.value = false;
    router.push('/audit-management/audits');
}

function onSubmit() {
    const ncCount = store.score.counts.nonConforming;
    confirm.require({
        message: ncCount > 0
            ? `This audit has ${ncCount} non-conforming item${ncCount > 1 ? 's' : ''}. Submit anyway?`
            : 'Submit this audit for review?',
        header: 'Confirm Submit',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Submit',
        rejectLabel: 'Cancel',
        accept: async () => {
            const ok = await store.submitAudit();
            if (ok) {
                showSummary.value = true;
            }
        },
    });
}
</script>

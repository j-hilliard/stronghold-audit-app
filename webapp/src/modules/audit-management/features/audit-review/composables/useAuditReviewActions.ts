import { computed, ref, watch } from 'vue';
import type { Ref } from 'vue';
import { useRoute } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import { useUserStore } from '@/stores/userStore';
import type {
    AuditFindingDto,
    CorrectiveActionDto,
    EmailRoutingRuleDto,
    DistributionPreviewDto,
    AuditReviewDto,
} from '@/apiclient/auditClient';

interface ActionOptions {
    review:            Ref<AuditReviewDto | null>;
    allRoutingEntries: Ref<EmailRoutingRuleDto[]>;
}

export function useAuditReviewActions({ review, allRoutingEntries }: ActionOptions) {
    const route     = useRoute();
    const store     = useAuditStore();
    const userStore = useUserStore();
    const service   = useAuditService();
    const toast     = useToast();

    const saving           = ref(false);
    const auditActionSaving = ref(false);

    // ── Review summary ────────────────────────────────────────────────────────
    const reviewSummaryDraft = ref('');
    const summarySaving      = ref(false);

    watch(() => review.value?.reviewSummary, (val) => {
        reviewSummaryDraft.value = val ?? '';
    }, { immediate: true });

    async function submitSaveSummary() {
        const id = Number(route.params.id);
        summarySaving.value = true;
        try {
            await store.saveReviewSummary(id, reviewSummaryDraft.value || null);
            toast.add({ severity: 'success', summary: 'Saved', detail: 'Findings summary saved.', life: 2500 });
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save summary.', life: 4000 });
        } finally {
            summarySaving.value = false;
        }
    }

    // ── Reopen / Close audit ──────────────────────────────────────────────────
    const showReopenDialog    = ref(false);
    const showCloseAuditDialog = ref(false);
    const reopenReason        = ref('');
    const closeAuditNotes     = ref('');

    async function submitReopen() {
        const id = Number(route.params.id);
        auditActionSaving.value = true;
        try {
            await service.reopenAudit(id, reopenReason.value || null);
            toast.add({ severity: 'warn', summary: 'Reopened', detail: 'Audit has been reopened.', life: 3000 });
            showReopenDialog.value = false;
            reopenReason.value     = '';
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
            await service.closeAudit(id, closeAuditNotes.value || null);
            toast.add({ severity: 'success', summary: 'Closed', detail: 'Audit has been closed.', life: 3000 });
            showCloseAuditDialog.value = false;
            closeAuditNotes.value      = '';
            await store.loadReview(id);
        } catch (e: unknown) {
            const axiosErr  = e as { response?: { status?: number; data?: unknown } };
            const serverMsg = typeof axiosErr?.response?.data === 'string'
                ? axiosErr.response.data
                : (e as Error)?.message ?? 'Failed to close audit.';
            toast.add({ severity: 'error', summary: 'Cannot Close Audit', detail: serverMsg, life: 6000 });
        } finally {
            auditActionSaving.value = false;
        }
    }

    // ── Assign CA modal ───────────────────────────────────────────────────────
    const showAssign   = ref(false);
    const assignTarget = ref<AuditFindingDto | null>(null);
    const assignForm   = ref({ description: '', assignedTo: '', dueDate: '' });

    function openAssignModal(item: AuditFindingDto) {
        assignTarget.value = item;
        assignForm.value   = { description: '', assignedTo: '', dueDate: '' };
        showAssign.value   = true;
    }

    async function submitAssign() {
        if (!assignTarget.value) return;
        saving.value = true;
        try {
            await service.assignCorrectiveAction({
                findingId:   assignTarget.value.id,
                description: assignForm.value.description,
                assignedTo:  assignForm.value.assignedTo || null,
                dueDate:     assignForm.value.dueDate    || null,
            });
            toast.add({ severity: 'success', summary: 'Assigned', detail: 'Corrective action assigned.', life: 2500 });
            showAssign.value = false;
            await store.loadReview(Number(route.params.id));
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign corrective action.', life: 4000 });
        } finally {
            saving.value = false;
        }
    }

    // ── Close CA modal ────────────────────────────────────────────────────────
    const showClose   = ref(false);
    const closeTarget = ref<CorrectiveActionDto | null>(null);
    const closeForm   = ref({ notes: '', completedDate: '' });

    function openCloseModal(ca: CorrectiveActionDto) {
        closeTarget.value = ca;
        closeForm.value   = { notes: '', completedDate: '' };
        showClose.value   = true;
    }

    async function submitClose() {
        if (!closeTarget.value) return;
        saving.value = true;
        try {
            await service.closeCorrectiveAction(closeTarget.value.id, {
                notes:          closeForm.value.notes,
                completedDate:  closeForm.value.completedDate || null,
            });
            toast.add({ severity: 'success', summary: 'Closed', detail: 'Corrective action closed.', life: 2500 });
            showClose.value = false;
            await store.loadReview(Number(route.params.id));
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to close corrective action.', life: 4000 });
        } finally {
            saving.value = false;
        }
    }

    // ── Distribution recipients ───────────────────────────────────────────────
    const addingRecipient      = ref(false);
    const removingRecipientId  = ref<number | null>(null);
    const showAddRecipientsDialog = ref(false);
    const addRecipientsSearch     = ref('');
    const addRecipientsDivisionFilter = ref('');
    const dialogSelectedEmails    = ref<string[]>([]);
    const manualEmail             = ref('');
    const manualName              = ref('');

    const dialogDivisionOptions = computed(() =>
        [...new Set(allRoutingEntries.value.filter(r => r.isActive).map(r => r.divisionCode))].sort()
    );

    function nameFromEmail(email: string): string {
        const local = email.split('@')[0] ?? '';
        return local.split(/[._-]/).map(p => p.charAt(0).toUpperCase() + p.slice(1)).join(' ');
    }

    const filteredRoutingForDialog = computed(() => {
        const already = new Set((review.value?.distributionRecipients ?? []).map(r => r.emailAddress));
        const q       = addRecipientsSearch.value.toLowerCase();
        const div     = addRecipientsDivisionFilter.value;
        return allRoutingEntries.value.filter(r => {
            if (!r.isActive || already.has(r.emailAddress)) return false;
            if (div && r.divisionCode !== div) return false;
            if (q) {
                const derivedName = nameFromEmail(r.emailAddress).toLowerCase();
                if (!r.emailAddress.toLowerCase().includes(q) && !derivedName.includes(q)) return false;
            }
            return true;
        });
    });

    function openAddRecipientsDialog() {
        addRecipientsDivisionFilter.value = review.value?.divisionCode ?? '';
        showAddRecipientsDialog.value     = true;
    }

    function closeAddRecipientsDialog() {
        showAddRecipientsDialog.value     = false;
        addRecipientsSearch.value         = '';
        addRecipientsDivisionFilter.value = '';
        dialogSelectedEmails.value        = [];
        manualEmail.value                 = '';
        manualName.value                  = '';
    }

    async function submitAddRecipients() {
        const id = Number(route.params.id);
        addingRecipient.value = true;
        try {
            for (const email of dialogSelectedEmails.value) {
                await store.addDistributionRecipient(id, email, undefined);
            }
            if (manualEmail.value.trim()) {
                await store.addDistributionRecipient(id, manualEmail.value.trim(), manualName.value.trim() || undefined);
            }
            const total = dialogSelectedEmails.value.length + (manualEmail.value.trim() ? 1 : 0);
            toast.add({ severity: 'success', summary: 'Added', detail: `${total} recipient(s) added.`, life: 2500 });
            closeAddRecipientsDialog();
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to add recipients.', life: 4000 });
        } finally {
            addingRecipient.value = false;
        }
    }

    async function removeRecipient(recipientId: number) {
        const id = Number(route.params.id);
        removingRecipientId.value = recipientId;
        try {
            await store.removeDistributionRecipient(id, recipientId);
            toast.add({ severity: 'success', summary: 'Removed', detail: 'Recipient removed.', life: 2000 });
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove recipient.', life: 4000 });
        } finally {
            removingRecipientId.value = null;
        }
    }

    // ── Distribution email ────────────────────────────────────────────────────
    const showDistributionDialog    = ref(false);
    const distributionLoadingPreview = ref(false);
    const distributionSending       = ref(false);
    const distributionPreview       = ref<DistributionPreviewDto | null>(null);
    const distributionSummaryEdit   = ref('');
    const editableSubject           = ref('');
    const selectedAttachmentIds     = ref<number[]>([]);

    function escapeHtml(input: string): string {
        return input
            .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;').replaceAll("'", '&#39;');
    }

    function buildFallbackDistributionPreview(auditId: number): DistributionPreviewDto | null {
        if (!review.value) return null;
        const division        = review.value.divisionCode || 'Audit';
        const trackingOrJob   = review.value.trackingNumber || review.value.header?.jobNumber || `Audit ${auditId}`;
        const auditDate       = review.value.header?.auditDate ? ` — ${review.value.header.auditDate}` : '';
        const recipients      = (review.value.distributionRecipients ?? [])
            .map(r => r.emailAddress)
            .filter((v): v is string => Boolean(v && v.trim()))
            .map(v => v.trim());
        const findingsSummary = reviewSummaryDraft.value || review.value.reviewSummary || null;
        const safeSummary     = findingsSummary ? escapeHtml(findingsSummary) : 'No findings summary provided.';
        const location        = review.value.header?.location ? escapeHtml(review.value.header.location) : 'N/A';
        const auditor         = review.value.header?.auditor  ? escapeHtml(review.value.header.auditor)  : 'N/A';
        return {
            subject: `${division} Audit Distribution — ${trackingOrJob}${auditDate}`,
            recipients,
            findingsSummary,
            bodyHtml: `
                <div style="font-family:Segoe UI,Arial,sans-serif;font-size:14px;line-height:1.5;color:#111827">
                    <h2 style="margin:0 0 8px 0;">${escapeHtml(division)} Audit Distribution</h2>
                    <p style="margin:0 0 4px 0;"><strong>Reference:</strong> ${escapeHtml(trackingOrJob)}</p>
                    <p style="margin:0 0 4px 0;"><strong>Location:</strong> ${location}</p>
                    <p style="margin:0 0 12px 0;"><strong>Auditor:</strong> ${auditor}</p>
                    <h3 style="margin:0 0 6px 0;">Findings Summary</h3>
                    <div style="padding:10px;border:1px solid #d1d5db;border-radius:6px;white-space:pre-wrap;">${safeSummary}</div>
                </div>
            `,
        };
    }

    async function openDistributionDialog() {
        const id = Number(route.params.id);
        distributionLoadingPreview.value = true;
        try {
            const preview = await store.getDistributionPreview(id, selectedAttachmentIds.value);
            distributionPreview.value      = preview;
            distributionSummaryEdit.value  = preview.findingsSummary ?? '';
            editableSubject.value          = preview.subject;
            showDistributionDialog.value   = true;
        } catch (e: any) {
            const status = e?.response?.status;
            if (status !== 403 && status !== 401) {
                const fallback = buildFallbackDistributionPreview(id);
                if (fallback) {
                    distributionPreview.value     = fallback;
                    distributionSummaryEdit.value = fallback.findingsSummary ?? '';
                    editableSubject.value         = fallback.subject;
                    showDistributionDialog.value  = true;
                    toast.add({ severity: 'warn', summary: 'Preview Fallback', detail: 'Live preview endpoint unavailable. Using local preview mode.', life: 4500 });
                    return;
                }
            }
            const detail = status === 403
                ? 'You do not have permission to preview distribution emails.'
                : status === 404
                ? 'Audit not found.'
                : (typeof e?.response?.data === 'string' ? e.response.data : null) ?? e?.message ?? 'Failed to load distribution preview.';
            toast.add({ severity: 'error', summary: 'Error', detail, life: 6000 });
        } finally {
            distributionLoadingPreview.value = false;
        }
    }

    async function submitDistributionEmail() {
        const id = Number(route.params.id);
        distributionSending.value = true;
        try {
            await store.saveReviewSummary(id, distributionSummaryEdit.value || null);
            await store.sendDistributionEmail(id, selectedAttachmentIds.value, editableSubject.value || undefined);
            showDistributionDialog.value = false;
            distributionPreview.value    = null;
            selectedAttachmentIds.value  = [];
            toast.add({ severity: 'success', summary: 'Sent', detail: 'Distribution email sent successfully.', life: 4000 });
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to send distribution email.', life: 5000 });
        } finally {
            distributionSending.value = false;
        }
    }

    return {
        saving, auditActionSaving,
        // Summary
        reviewSummaryDraft, summarySaving, submitSaveSummary,
        // Reopen / Close
        showReopenDialog, showCloseAuditDialog, reopenReason, closeAuditNotes,
        submitReopen, submitCloseAudit,
        // Assign CA
        showAssign, assignTarget, assignForm, openAssignModal, submitAssign,
        // Close CA
        showClose, closeTarget, closeForm, openCloseModal, submitClose,
        // Recipients
        addingRecipient, removingRecipientId,
        showAddRecipientsDialog, addRecipientsSearch, addRecipientsDivisionFilter,
        dialogSelectedEmails, manualEmail, manualName,
        dialogDivisionOptions, filteredRoutingForDialog,
        nameFromEmail, openAddRecipientsDialog, closeAddRecipientsDialog,
        submitAddRecipients, removeRecipient,
        // Distribution email
        showDistributionDialog, distributionLoadingPreview, distributionSending,
        distributionPreview, distributionSummaryEdit, editableSubject, selectedAttachmentIds,
        openDistributionDialog, submitDistributionEmail,
    };
}

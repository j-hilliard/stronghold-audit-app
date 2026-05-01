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

    // ── Review mode (set when Start Review succeeds) ──────────────────────────
    const reviewEditMode = ref(false);

    // ── PDF preview modal ─────────────────────────────────────────────────────
    const showPdfPreviewModal = ref(false);

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
            reviewEditMode.value   = false;
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
            reviewEditMode.value       = false;
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

    // ── Workflow: Enter Review Mode / Approve ────────────────────────────────

    async function enterReviewMode() {
        const id = Number(route.params.id);
        if (review.value?.status === 'Submitted') {
            // Submitted → UnderReview requires an API transition first
            auditActionSaving.value = true;
            try {
                await service.startAuditReview(id);
                reviewEditMode.value = true;
                toast.add({
                    severity: 'info',
                    summary: 'Review Mode Active',
                    detail: 'Audit is now under review. Edit findings below, or use Edit Responses to update individual question answers.',
                    life: 4000,
                });
                await store.loadReview(id);
            } catch (e: unknown) {
                const axiosErr = e as { response?: { status?: number; data?: unknown } };
                const detail   = typeof axiosErr?.response?.data === 'string'
                    ? axiosErr.response.data
                    : (e as Error)?.message ?? 'Failed to enter review mode. Please try again.';
                toast.add({ severity: 'error', summary: 'Cannot Enter Review Mode', detail, life: 6000 });
            } finally {
                auditActionSaving.value = false;
            }
        } else {
            // Already UnderReview — activate local edit mode without an API call
            reviewEditMode.value = true;
        }
    }

    async function approveAudit() {
        const id = Number(route.params.id);
        auditActionSaving.value = true;
        try {
            await service.approveAudit(id);
            reviewEditMode.value = false;
            toast.add({ severity: 'success', summary: 'Audit Approved', detail: 'Audit approved and locked. Ready for distribution.', life: 4000 });
            await store.loadReview(id);
        } catch (e: unknown) {
            const axiosErr  = e as { response?: { status?: number; data?: unknown } };
            const serverMsg = typeof axiosErr?.response?.data === 'string'
                ? axiosErr.response.data
                : (e as Error)?.message ?? 'Failed to approve audit.';
            toast.add({ severity: 'error', summary: 'Cannot Approve Audit', detail: serverMsg || 'Ensure the audit is in Under Review status and try again.', life: 6000 });
        } finally {
            auditActionSaving.value = false;
        }
    }

    // ── Assign CA modal ───────────────────────────────────────────────────────
    const showAssign   = ref(false);
    const assignTarget = ref<AuditFindingDto | null>(null);
    const assignForm   = ref({ description: '', assignedTo: '', assignedToEmail: '', dueDate: '', priority: 'Normal' });

    function openAssignModal(item: AuditFindingDto) {
        assignTarget.value = item;
        assignForm.value   = { description: '', assignedTo: '', assignedToEmail: '', dueDate: '', priority: 'Normal' };
        showAssign.value   = true;
    }

    async function submitAssign() {
        if (!assignTarget.value) return;
        const email = assignForm.value.assignedToEmail.trim();
        if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            toast.add({ severity: 'warn', summary: 'Invalid Email', detail: 'Please enter a valid email address for the assignee.', life: 4000 });
            return;
        }
        saving.value = true;
        try {
            await service.assignCorrectiveAction({
                findingId:       assignTarget.value.id,
                description:     assignForm.value.description,
                assignedTo:      assignForm.value.assignedTo      || null,
                assignedToEmail: assignForm.value.assignedToEmail || null,
                dueDate:         assignForm.value.dueDate         || null,
                priority:        assignForm.value.priority        || 'Normal',
            });
            toast.add({ severity: 'success', summary: 'Assigned', detail: 'Corrective action assigned.', life: 2500 });
            showAssign.value = false;
            await store.loadReview(Number(route.params.id));
        } catch (e: unknown) {
            const axiosErr  = e as { response?: { status?: number; data?: unknown } };
            const serverMsg = typeof axiosErr?.response?.data === 'string'
                ? axiosErr.response.data
                : (e as Error)?.message ?? 'Failed to assign corrective action.';
            toast.add({ severity: 'error', summary: 'Cannot Assign CA', detail: serverMsg, life: 5000 });
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
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to close corrective action. Check that resolution notes are filled and try again.', life: 5000 });
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

    // Inline add/remove used by SendDistributionModal — also updates the preview recipients list
    async function quickAddRecipient(email: string, name?: string) {
        const id = Number(route.params.id);
        addingRecipient.value = true;
        try {
            await store.addDistributionRecipient(id, email, name || undefined);
            if (distributionPreview.value) {
                const newDetail = { emailAddress: email, name: name || null, source: 'Manual' as const, manualRecipientId: null };
                distributionPreview.value = {
                    ...distributionPreview.value,
                    recipients: [...distributionPreview.value.recipients, email],
                    recipientDetails: [...distributionPreview.value.recipientDetails, newDetail],
                };
            }
            toast.add({ severity: 'success', summary: 'Recipient Added', detail: email, life: 2000 });
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to add recipient.', life: 4000 });
        } finally {
            addingRecipient.value = false;
        }
    }

    async function quickRemoveRecipient(recipientId: number) {
        const id = Number(route.params.id);
        const detailToRemove = distributionPreview.value?.recipientDetails?.find(r => r.manualRecipientId === recipientId);
        const emailToRemove  = detailToRemove?.emailAddress;
        removingRecipientId.value = recipientId;
        try {
            await store.removeDistributionRecipient(id, recipientId);
            if (distributionPreview.value) {
                distributionPreview.value = {
                    ...distributionPreview.value,
                    recipients: emailToRemove
                        ? distributionPreview.value.recipients.filter(r => r !== emailToRemove)
                        : distributionPreview.value.recipients,
                    recipientDetails: distributionPreview.value.recipientDetails.filter(
                        r => r.manualRecipientId !== recipientId,
                    ),
                };
            }
            toast.add({ severity: 'success', summary: 'Removed', detail: 'Recipient removed.', life: 2000 });
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove recipient.', life: 4000 });
        } finally {
            removingRecipientId.value = null;
        }
    }

    // ── Distribution email ────────────────────────────────────────────────────
    const showDistributionDialog     = ref(false);
    const distributionLoadingPreview = ref(false);
    const distributionSending        = ref(false);
    const distributionPreview        = ref<DistributionPreviewDto | null>(null);
    const distributionSummaryEdit    = ref('');
    const editableSubject            = ref('');
    const selectedAttachmentIds      = ref<number[]>([]);

    // Step tracks compose vs. sent-confirmation state inside the dialog
    const distributionStep          = ref<'compose' | 'sent'>('compose');
    const distributionSentInfo      = ref<{ type: 'api' | 'mailto'; recipients: string[]; items: string[] } | null>(null);
    const distributionExcludedEmails = ref<string[]>([]);

    function closeDistributionDialog() {
        showDistributionDialog.value      = false;
        distributionStep.value            = 'compose';
        distributionSentInfo.value        = null;
        distributionExcludedEmails.value  = [];
    }

    function buildSentItemsList(): string[] {
        const r = review.value;
        return [
            ...(includeAuditPdf.value ? ['Audit PDF'] : []),
            ...(includeCas.value && (r?.nonConformingCount ?? 0) > 0
                ? [includeOpenCasOnly.value ? 'Corrective Actions (open CAs only)' : 'All Corrective Actions']
                : []),
            ...(includeAttachments.value && selectedAttachmentIds.value.length > 0
                ? [`${selectedAttachmentIds.value.length} file attachment${selectedAttachmentIds.value.length !== 1 ? 's' : ''}`]
                : []),
        ];
    }

    // Include options
    const includeAuditPdf    = ref(true);
    const includeCas         = ref(true);
    const includeOpenCasOnly = ref(false);
    const includeAttachments = ref(false);

    // Seed include defaults when dialog opens
    function seedIncludeDefaults() {
        includeAuditPdf.value    = true;
        includeCas.value         = (review.value?.nonConformingCount ?? 0) > 0;
        includeOpenCasOnly.value = false;
        includeAttachments.value = (review.value?.attachments?.length ?? 0) > 0;
        if (!includeAttachments.value) selectedAttachmentIds.value = [];
    }

    async function openDistributionDialog() {
        const id = Number(route.params.id);
        distributionStep.value     = 'compose';
        distributionSentInfo.value = null;
        distributionLoadingPreview.value = true;
        seedIncludeDefaults();
        try {
            const preview = await store.getDistributionPreview(id, selectedAttachmentIds.value);
            distributionPreview.value      = preview;
            distributionSummaryEdit.value  = preview.findingsSummary ?? '';
            editableSubject.value          = preview.subject;
            showDistributionDialog.value   = true;
        } catch (e: any) {
            const status = e?.response?.status;
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

    // Build a plain-text email body for mailto
    function buildMailtoBody(): string {
        const r = review.value;
        if (!r) return '';
        const id          = route.params.id;
        const division    = r.divisionCode || 'Audit';
        const ref_        = r.trackingNumber || r.header?.jobNumber || `Audit ${id}`;
        const sep         = '─'.repeat(40);

        let body = `${division} AUDIT DISTRIBUTION\n${sep}\n\n`;
        body += `Reference : ${ref_}\n`;
        body += `Date      : ${r.header?.auditDate || 'N/A'}\n`;
        body += `Location  : ${r.header?.location  || 'N/A'}\n`;
        body += `Auditor   : ${r.header?.auditor   || 'N/A'}\n\n`;

        if (r.scorePercent != null) {
            body += `SCORE: ${Math.round(r.scorePercent)}%\n`;
            body += `Conforming: ${r.conformingCount}  |  Non-Conforming: ${r.nonConformingCount}  |  Warning: ${r.warningCount}\n\n`;
        }

        if (distributionSummaryEdit.value.trim()) {
            body += `FINDINGS SUMMARY\n${sep}\n${distributionSummaryEdit.value.trim()}\n\n`;
        }

        if (includeCas.value && (r.nonConformingItems?.length ?? 0) > 0) {
            const items = includeOpenCasOnly.value
                ? r.nonConformingItems.filter(item =>
                    item.correctiveActions?.some(ca => ca.status !== 'Closed'))
                : r.nonConformingItems;

            if (items.length > 0) {
                body += `CORRECTIVE ACTIONS\n${sep}\n`;
                items.forEach((item, i) => {
                    body += `${i + 1}. ${item.questionText}\n`;
                    (item.correctiveActions ?? []).forEach(ca => {
                        body += `   • ${ca.description}`;
                        if (ca.assignedTo) body += ` — Assigned: ${ca.assignedTo}`;
                        if (ca.dueDate)    body += ` — Due: ${ca.dueDate}`;
                        body += ` [${ca.status}]\n`;
                    });
                });
                body += '\n';
            }
        }

        const attList = includeAttachments.value
            ? (r.attachments ?? []).filter(a => selectedAttachmentIds.value.includes(a.id) && a.hasFile)
            : [];
        if (attList.length > 0) {
            body += `ATTACHMENTS\n${sep}\n`;
            attList.forEach(a => { body += `• ${a.fileName}\n`; });
            body += '\n';
        }

        if (includeAuditPdf.value) {
            body += `[Audit PDF will be attached when sent via Stronghold]\n\n`;
        }

        body += `${'─'.repeat(40)}\nSent via Stronghold Audit System\n`;
        return body;
    }

    function openMailtoFallback() {
        const excluded   = new Set(distributionExcludedEmails.value.map(e => e.toLowerCase()));
        const recipients = (distributionPreview.value?.recipientDetails ?? [])
            .filter(r => !excluded.has(r.emailAddress.toLowerCase()))
            .map(r => r.emailAddress);
        if (recipients.length === 0) {
            toast.add({ severity: 'warn', summary: 'No Recipients', detail: 'Add recipients before sending.', life: 4000 });
            return;
        }
        const to      = recipients.join(',');
        const subject = encodeURIComponent(editableSubject.value);
        const body    = encodeURIComponent(buildMailtoBody());
        window.location.href = `mailto:${to}?subject=${subject}&body=${body}`;
    }

    async function submitDistributionEmail() {
        const id = Number(route.params.id);
        distributionSending.value = true;
        try {
            // Only save the review summary when the status allows it (not on resend from Distributed)
            const status = review.value?.status;
            if (status === 'UnderReview' || status === 'Approved') {
                await store.saveReviewSummary(id, distributionSummaryEdit.value || null);
            }

            const excluded       = new Set(distributionExcludedEmails.value.map(e => e.toLowerCase()));
            const sentRecipients = (distributionPreview.value?.recipientDetails ?? [])
                .filter(r => !excluded.has(r.emailAddress.toLowerCase()))
                .map(r => r.emailAddress);
            const sentItems      = buildSentItemsList();

            await store.sendDistributionEmail(
                id,
                selectedAttachmentIds.value,
                editableSubject.value || undefined,
                includeCas.value,
                includeOpenCasOnly.value,
                distributionSummaryEdit.value || null,
                includeAuditPdf.value,
                distributionExcludedEmails.value.length > 0 ? distributionExcludedEmails.value : undefined,
            );
            selectedAttachmentIds.value = [];
            distributionSentInfo.value  = { type: 'api', recipients: sentRecipients, items: sentItems };
            distributionStep.value      = 'sent';
        } catch {
            toast.add({ severity: 'error', summary: 'Distribution Failed', detail: 'Failed to prepare distribution. Please check recipients and try again.', life: 5000 });
        } finally {
            distributionSending.value = false;
        }
    }

    return {
        saving, auditActionSaving,
        // Review mode
        reviewEditMode,
        // PDF preview
        showPdfPreviewModal,
        // Summary
        reviewSummaryDraft, summarySaving, submitSaveSummary,
        // Reopen / Close / Workflow
        showReopenDialog, showCloseAuditDialog, reopenReason, closeAuditNotes,
        submitReopen, submitCloseAudit, enterReviewMode, approveAudit,
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
        submitAddRecipients, removeRecipient, quickAddRecipient, quickRemoveRecipient,
        // Distribution email
        showDistributionDialog, distributionLoadingPreview, distributionSending,
        distributionPreview, distributionSummaryEdit, editableSubject, selectedAttachmentIds,
        includeAuditPdf, includeCas, includeOpenCasOnly, includeAttachments,
        distributionStep, distributionSentInfo, distributionExcludedEmails,
        openDistributionDialog, closeDistributionDialog, submitDistributionEmail, openMailtoFallback,
    };
}

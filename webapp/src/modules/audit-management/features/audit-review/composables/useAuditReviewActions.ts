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
                    detail: 'Audit is now under review. Edit findings below, or use the form to update individual responses.',
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

    // ── Distribution email ────────────────────────────────────────────────────
    const showDistributionDialog     = ref(false);
    const distributionLoadingPreview = ref(false);
    const distributionSending        = ref(false);
    const distributionPreview        = ref<DistributionPreviewDto | null>(null);
    const distributionSummaryEdit    = ref('');
    const editableSubject            = ref('');
    const selectedAttachmentIds      = ref<number[]>([]);

    // Step tracks compose vs. sent-confirmation state inside the dialog
    const distributionStep     = ref<'compose' | 'sent'>('compose');
    const distributionSentInfo = ref<{ type: 'api' | 'mailto'; recipients: string[]; items: string[] } | null>(null);

    function closeDistributionDialog() {
        showDistributionDialog.value = false;
        distributionStep.value       = 'compose';
        distributionSentInfo.value   = null;
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
        const recipients      = [
            ...(review.value.reviewEmailRouting ?? []).map(r => r.emailAddress),
            ...(review.value.distributionRecipients ?? []).map(r => r.emailAddress),
        ].filter((v): v is string => Boolean(v?.trim())).map(v => v.trim());
        const findingsSummary = reviewSummaryDraft.value || review.value.reviewSummary || null;
        const safeSummary     = findingsSummary ? escapeHtml(findingsSummary) : 'No findings summary provided.';
        const location        = review.value.header?.location ? escapeHtml(review.value.header.location) : 'N/A';
        const auditor         = review.value.header?.auditor  ? escapeHtml(review.value.header.auditor)  : 'N/A';
        const r       = review.value;
        const score   = r.scorePercent != null ? Math.round(r.scorePercent) : null;
        const scoreBg = score == null ? '#e5e7eb' : score >= 90 ? '#dcfce7' : score >= 75 ? '#fef9c3' : '#fee2e2';
        const scoreClr= score == null ? '#374151' : score >= 90 ? '#166534' : score >= 75 ? '#92400e' : '#991b1b';
        const nc      = r.nonConformingCount ?? 0;
        const warn    = r.warningCount ?? 0;
        const conf    = r.conformingCount ?? 0;

        const scoreRow = score != null ? `
            <tr>
                <td colspan="4" style="padding:0 0 12px 0;">
                    <div style="display:flex;gap:8px;align-items:stretch;">
                        <div style="background:${scoreBg};border-radius:8px;padding:12px 16px;text-align:center;min-width:72px;">
                            <div style="font-size:24px;font-weight:700;color:${scoreClr};line-height:1;">${score}%</div>
                            <div style="font-size:11px;color:${scoreClr};margin-top:2px;">Score</div>
                        </div>
                        <div style="background:#dcfce7;border-radius:8px;padding:10px 14px;text-align:center;flex:1;">
                            <div style="font-size:20px;font-weight:700;color:#166534;">${conf}</div>
                            <div style="font-size:11px;color:#166534;">Conforming</div>
                        </div>
                        <div style="background:#fee2e2;border-radius:8px;padding:10px 14px;text-align:center;flex:1;">
                            <div style="font-size:20px;font-weight:700;color:#991b1b;">${nc}</div>
                            <div style="font-size:11px;color:#991b1b;">Non-Conforming</div>
                        </div>
                        ${warn > 0 ? `<div style="background:#fef9c3;border-radius:8px;padding:10px 14px;text-align:center;flex:1;">
                            <div style="font-size:20px;font-weight:700;color:#92400e;">${warn}</div>
                            <div style="font-size:11px;color:#92400e;">Warning</div>
                        </div>` : ''}
                    </div>
                </td>
            </tr>` : '';

        return {
            subject: `[${division}] Safety &amp; Compliance Audit — ${escapeHtml(trackingOrJob)}${auditDate}`,
            recipients,
            findingsSummary,
            bodyHtml: `
<div style="font-family:Segoe UI,Arial,sans-serif;max-width:620px;color:#111827;border-radius:10px;overflow:hidden;border:1px solid #e2e8f0;">
  <div style="background:#1a3a5c;padding:22px 24px;">
    <h1 style="color:#ffffff;margin:0 0 4px;font-size:18px;font-weight:700;letter-spacing:-0.2px;">Safety &amp; Compliance Audit</h1>
    <p style="color:#93c5fd;margin:0;font-size:13px;font-weight:500;">${escapeHtml(division)} Division &nbsp;·&nbsp; ${escapeHtml(trackingOrJob)}</p>
  </div>
  <div style="background:#f8fafc;padding:0 24px 4px;">
    <table style="width:100%;border-collapse:collapse;font-size:13px;padding-top:16px;">
      <tbody>
        <tr><td style="height:14px;" colspan="4"></td></tr>
        ${scoreRow}
        <tr>
          <td style="padding:3px 12px 3px 0;color:#6b7280;white-space:nowrap;font-weight:500;">Location</td>
          <td style="padding:3px 16px 3px 4px;color:#111827;">${location}</td>
          <td style="padding:3px 12px 3px 0;color:#6b7280;white-space:nowrap;font-weight:500;">Auditor</td>
          <td style="padding:3px 0;color:#111827;">${auditor}</td>
        </tr>
        <tr>
          <td style="padding:3px 12px 3px 0;color:#6b7280;white-space:nowrap;font-weight:500;">Date</td>
          <td style="padding:3px 16px 3px 4px;color:#111827;">${escapeHtml(r.header?.auditDate ?? 'N/A')}</td>
          <td style="padding:3px 12px 3px 0;color:#6b7280;white-space:nowrap;font-weight:500;">Status</td>
          <td style="padding:3px 0;color:#111827;">${escapeHtml(r.status ?? '')}</td>
        </tr>
        <tr><td style="height:16px;" colspan="4"></td></tr>
      </tbody>
    </table>
  </div>
  ${findingsSummary ? `
  <div style="padding:16px 24px;background:#ffffff;border-top:1px solid #e2e8f0;">
    <h3 style="margin:0 0 8px;font-size:13px;font-weight:700;color:#1a3a5c;text-transform:uppercase;letter-spacing:0.05em;">Findings Summary</h3>
    <p style="margin:0;font-size:13px;line-height:1.6;color:#374151;white-space:pre-wrap;">${safeSummary}</p>
  </div>` : ''}
  <div style="padding:16px 24px;background:#f8fafc;border-top:1px solid #e2e8f0;font-size:12px;color:#9ca3af;">
    Sent via Stronghold Audit System &nbsp;·&nbsp; Safety &amp; Compliance
  </div>
</div>
            `,
        };
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
            if (status !== 403 && status !== 401) {
                const fallback = buildFallbackDistributionPreview(id);
                if (fallback) {
                    distributionPreview.value     = fallback;
                    distributionSummaryEdit.value = fallback.findingsSummary ?? '';
                    editableSubject.value         = fallback.subject;
                    showDistributionDialog.value  = true;
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
            body += `[Audit PDF attached]\n\n`;
        }

        body += `${'─'.repeat(40)}\nSent via Stronghold Audit System\n`;
        return body;
    }

    function openMailtoFallback() {
        const recipients = distributionPreview.value?.recipients ?? [];
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
            await store.saveReviewSummary(id, distributionSummaryEdit.value || null);

            const sentRecipients = distributionPreview.value?.recipients ?? [];
            const sentItems      = buildSentItemsList();

            try {
                await store.sendDistributionEmail(
                    id,
                    selectedAttachmentIds.value,
                    editableSubject.value || undefined,
                    includeCas.value,
                    includeOpenCasOnly.value,
                    distributionSummaryEdit.value || null,
                );
                selectedAttachmentIds.value = [];
                distributionSentInfo.value  = { type: 'api', recipients: sentRecipients, items: sentItems };
                distributionStep.value      = 'sent';
            } catch {
                // API send failed — open email client as fallback
                openMailtoFallback();
                distributionSentInfo.value = { type: 'mailto', recipients: sentRecipients, items: sentItems };
                distributionStep.value     = 'sent';
            }
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
        submitAddRecipients, removeRecipient,
        // Distribution email
        showDistributionDialog, distributionLoadingPreview, distributionSending,
        distributionPreview, distributionSummaryEdit, editableSubject, selectedAttachmentIds,
        includeAuditPdf, includeCas, includeOpenCasOnly, includeAttachments,
        distributionStep, distributionSentInfo,
        openDistributionDialog, closeDistributionDialog, submitDistributionEmail, openMailtoFallback,
    };
}

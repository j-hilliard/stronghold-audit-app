import { defineStore } from 'pinia';
import { ref, computed, watch } from 'vue';
import { useToast } from 'primevue/usetoast';
import { useApiStore } from '@/stores/apiStore';
import { useUserStore } from '@/stores/userStore';
import {
    AuditClient,
    type DivisionDto,
    type TemplateDto,
    type TemplateSectionDto,
    type AuditHeaderDto,
    type AuditListItemDto,
    type AuditReviewDto,
    type LogicRuleDto,
    type RepeatFindingDto,
} from '@/apiclient/auditClient';

// ── Local types ───────────────────────────────────────────────────────────────

export interface ResponseState {
    questionId: number;
    questionTextSnapshot: string;
    /** "Conforming" | "NonConforming" | "Warning" | "NA" | null */
    status: string | null;
    comment: string | null;
    correctedOnSite: boolean;
    // From the template question (for UI rules)
    allowNA: boolean;
    requireCommentOnNC: boolean;
    isScoreable: boolean;
    /** When true, a photo must be attached before submit if status = NonConforming */
    requirePhotoOnNc: boolean;
    /** When true, a NonConforming answer triggers auto-CA creation on submit */
    autoCreateCa: boolean;
}

interface LocalDraft {
    auditId: number;
    header: AuditHeaderDto;
    responses: Record<number, ResponseState>;
    savedAt: string;
}

// ── Score calculation (exported for tests) ────────────────────────────────────

export interface ScoreCounts {
    conforming: number;
    nonConforming: number;
    warning: number;
    na: number;
    unanswered: number;
}

export function calculateScore(responses: ResponseState[]): { counts: ScoreCounts; scorePercent: number | null } {
    const counts: ScoreCounts = { conforming: 0, nonConforming: 0, warning: 0, na: 0, unanswered: 0 };
    for (const r of responses) {
        if (!r.isScoreable) continue;
        if (r.status === 'Conforming') counts.conforming++;
        else if (r.status === 'NonConforming') counts.nonConforming++;
        else if (r.status === 'Warning') counts.warning++;
        else if (r.status === 'NA') counts.na++;
        else counts.unanswered++;
    }
    const denominator = counts.conforming + counts.nonConforming + counts.warning;
    const scorePercent = denominator > 0 ? (counts.conforming / denominator) * 100 : null;
    return { counts, scorePercent };
}

// ── Store ─────────────────────────────────────────────────────────────────────

const DRAFT_KEY = (id: number) => `audit-draft-${id}`;

export const useAuditStore = defineStore('audit', () => {
    const apiStore = useApiStore();
    const toast = useToast();

    function getClient() {
        return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
    }

    // ── State ──────────────────────────────────────────────────────────────────

    const divisions = ref<DivisionDto[]>([]);
    const template = ref<TemplateDto | null>(null);
    const auditId = ref<number | null>(null);
    const auditStatus = ref<string>('Draft');

    /** Optional section group keys enabled at audit creation. Null = audit not yet loaded. */
    const enabledOptionalGroupKeys = ref<string[]>([]);

    const header = ref<AuditHeaderDto>({});
    // Keyed by questionId
    const responses = ref<Map<number, ResponseState>>(new Map());

    const audits = ref<AuditListItemDto[]>([]);
    const review = ref<AuditReviewDto | null>(null);

    /** questionIds that are repeat findings for the current audit's division (last 180 days). */
    const repeatFindingQuestionIds = ref<Set<number>>(new Set());

    const loading = ref(false);      // audit form loading (loadAudit)
    const listLoading = ref(false);  // dashboard list loading
    const reviewLoading = ref(false); // review page loading
    const saving = ref(false);
    const isDirty = ref(false);

    // List filters
    const filterDivisionId = ref<number | null>(null);
    const filterStatus = ref<string | null>(null);
    const filterAuditor = ref<string | null>(null);
    const filterDateFrom = ref<string | null>(null);
    const filterDateTo = ref<string | null>(null);

    // Draft restore modal
    const hasPendingDraft = ref(false);

    // ── Computed ───────────────────────────────────────────────────────────────

    const allResponses = computed(() => Array.from(responses.value.values()));

    const score = computed(() => calculateScore(allResponses.value));

    const isSubmitted = computed(() =>
        auditStatus.value === 'Submitted' || auditStatus.value === 'Closed',
    );

    /**
     * Template sections visible for this audit.
     * Non-optional sections are always shown unless hidden by a logic rule.
     * Optional sections are shown when their group key is enabled AND not hidden by a logic rule.
     *
     * Logic rule evaluation (section-level):
     *   - "HideSection" fires when triggerVersionQuestionId's current response matches triggerResponse
     *   - "ShowSection" fires when triggerVersionQuestionId's current response matches triggerResponse
     *   - Rules are additive: any matching HideSection hides the section; any matching ShowSection shows it
     *   - HideSection takes precedence over ShowSection when both fire
     */
    const visibleSections = computed((): TemplateSectionDto[] => {
        if (!template.value) return [];
        const enabledKeys = enabledOptionalGroupKeys.value;
        const rules: LogicRuleDto[] = template.value.logicRules ?? [];

        // Compute which sections are hidden/force-shown by logic rules
        const hiddenByRule = new Set<number>();
        const shownByRule  = new Set<number>();

        for (const rule of rules) {
            if (!rule.targetSectionId) continue;
            // Find the current response status for this trigger question
            const respState = responses.value.get(rule.triggerVersionQuestionId);
            const currentStatus = respState?.status ?? null;

            const fires = rule.triggerResponse === 'AnyAnswer'
                ? currentStatus !== null
                : currentStatus === rule.triggerResponse;

            if (fires) {
                if (rule.action === 'HideSection') hiddenByRule.add(rule.targetSectionId);
                else if (rule.action === 'ShowSection') shownByRule.add(rule.targetSectionId);
            }
        }

        return template.value.sections.filter(s => {
            // Logic rule hide takes absolute priority
            if (hiddenByRule.has(s.id)) return false;

            // Optional section visibility
            if (s.isOptional && s.optionalGroupKey) {
                if (!enabledKeys.includes(s.optionalGroupKey) && !shownByRule.has(s.id)) return false;
            }

            return true;
        });
    });

    // ── Private helpers ────────────────────────────────────────────────────────

    function saveDraftToLocalStorage() {
        if (!auditId.value) return;
        const draft: LocalDraft = {
            auditId: auditId.value,
            header: { ...header.value },
            responses: Object.fromEntries(responses.value),
            savedAt: new Date().toISOString(),
        };
        localStorage.setItem(DRAFT_KEY(auditId.value), JSON.stringify(draft));
    }

    function clearLocalDraft() {
        if (auditId.value) localStorage.removeItem(DRAFT_KEY(auditId.value));
        hasPendingDraft.value = false;
    }

    function checkForLocalDraft(id: number): LocalDraft | null {
        const raw = localStorage.getItem(DRAFT_KEY(id));
        if (!raw) return null;
        try {
            return JSON.parse(raw) as LocalDraft;
        } catch {
            return null;
        }
    }

    function applyLocalDraft(draft: LocalDraft) {
        header.value = { ...draft.header };
        for (const [qid, r] of Object.entries(draft.responses)) {
            responses.value.set(Number(qid), r);
        }
        hasPendingDraft.value = false;
        isDirty.value = true;
    }

    // Initialize responses from a loaded template (only visible sections; preserves existing responses)
    function initResponsesFromTemplate(tpl: TemplateDto, enabledKeys: string[]) {
        for (const section of tpl.sections) {
            // Skip optional sections whose group is not enabled
            if (section.isOptional && section.optionalGroupKey && !enabledKeys.includes(section.optionalGroupKey)) {
                continue;
            }
            for (const q of section.questions) {
                if (!responses.value.has(q.questionId)) {
                    responses.value.set(q.questionId, {
                        questionId: q.questionId,
                        questionTextSnapshot: q.questionText,
                        status: null,
                        comment: null,
                        correctedOnSite: false,
                        allowNA: q.allowNA,
                        requireCommentOnNC: q.requireCommentOnNC,
                        isScoreable: q.isScoreable,
                        requirePhotoOnNc: q.requirePhotoOnNc ?? false,
                        autoCreateCa: q.autoCreateCa ?? false,
                    });
                }
            }
        }
    }

    // ── Auto-save debounce ─────────────────────────────────────────────────────

    let _autosaveTimer: ReturnType<typeof setTimeout> | null = null;

    function scheduleAutosave() {
        if (_autosaveTimer) clearTimeout(_autosaveTimer);
        _autosaveTimer = setTimeout(() => {
            saveDraftToLocalStorage();
        }, 800);
    }

    watch(
        [header, responses],
        () => {
            if (auditId.value && !isSubmitted.value) {
                isDirty.value = true;
                scheduleAutosave();
            }
        },
        { deep: true },
    );

    // ── Actions ────────────────────────────────────────────────────────────────

    async function loadDivisions(force = false) {
        if (!force && divisions.value.length > 0) return;
        try {
            const raw = await getClient().getDivisions() as unknown as Record<string, unknown>[];
            const seen = new Set<number>();
            divisions.value = raw
                .map((r): DivisionDto | null => {
                    const id = Number(r.id ?? r.Id ?? 0);
                    if (!id) return null;
                    return {
                        id,
                        code:      String(r.code      ?? r.Code      ?? ''),
                        name:      String(r.name      ?? r.Name      ?? ''),
                        auditType: String(r.auditType ?? r.AuditType ?? ''),
                    };
                })
                .filter((d): d is DivisionDto => d !== null && !seen.has(d.id) && !!seen.add(d.id));
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load divisions.', life: 4000 });
        }
    }

    async function createAudit(divisionId: number, enabledKeys: string[] = []): Promise<number | null> {
        saving.value = true;
        try {
            const created = await getClient().createAudit(divisionId, enabledKeys);

            // Defensive handling: API contract may return either an ID or a detail DTO.
            if (typeof created === 'number' && Number.isFinite(created) && created > 0) {
                return created;
            }

            const id = Number((created as { id?: unknown })?.id);
            if (Number.isFinite(id) && id > 0) {
                return id;
            }

            throw new Error('CreateAudit response did not include a valid ID.');
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to create audit.', life: 4000 });
            return null;
        } finally {
            saving.value = false;
        }
    }

    async function loadAudit(id: number) {
        loading.value = true;
        resetForm();
        try {
            const [audit, tpl] = await Promise.all([
                getClient().getAudit(id),
                // We need divisionId to load the template; it comes from the audit
                getClient().getAudit(id).then(a => getClient().getActiveTemplate(a.divisionId)),
            ]);

            auditId.value = audit.id;
            auditStatus.value = audit.status;
            header.value = audit.header ?? {};

            // 2A-5: Pre-fill audit metadata on new Draft audits
            if (audit.status === 'Draft') {
                const userStore = useUserStore();
                const now = new Date();
                if (!header.value.auditDate) {
                    const yyyy = now.getFullYear();
                    const mm = String(now.getMonth() + 1).padStart(2, '0');
                    const dd = String(now.getDate()).padStart(2, '0');
                    header.value = { ...header.value, auditDate: `${yyyy}-${mm}-${dd}` };
                }
                if (!header.value.time) {
                    const hh = String(now.getHours()).padStart(2, '0');
                    const min = String(now.getMinutes()).padStart(2, '0');
                    header.value = { ...header.value, time: `${hh}:${min}` };
                }
                if (!header.value.auditor) {
                    const fullName = userStore.userFullName.trim();
                    if (fullName) {
                        header.value = { ...header.value, auditor: fullName };
                    }
                }
            }

            template.value = tpl;
            enabledOptionalGroupKeys.value = audit.enabledOptionalGroupKeys ?? [];

            // Seed response map from template (visible sections only)
            initResponsesFromTemplate(tpl, enabledOptionalGroupKeys.value);

            // Overwrite with saved server responses
            for (const r of audit.responses) {
                const existing = responses.value.get(r.questionId);
                if (existing) {
                    existing.status = r.status ?? null;
                    existing.comment = r.comment ?? null;
                    existing.correctedOnSite = r.correctedOnSite;
                }
            }

            // Check for a newer local draft
            const draft = checkForLocalDraft(id);
            if (draft && draft.savedAt > (audit.submittedAt ?? audit.createdAt)) {
                hasPendingDraft.value = true;
                // Store the draft for the user to accept/discard
                _pendingDraft.value = draft;
            }

            isDirty.value = false;

            // Load repeat findings (non-blocking — badge is supplementary)
            getClient().getRepeatFindings(id)
                .then((findings: RepeatFindingDto[]) => {
                    repeatFindingQuestionIds.value = new Set(findings.map(f => f.questionId));
                })
                .catch(() => {
                    repeatFindingQuestionIds.value = new Set();
                });
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load audit.', life: 4000 });
        } finally {
            loading.value = false;
        }
    }

    // Holds a pending local draft until user decides
    const _pendingDraft = ref<LocalDraft | null>(null);

    function acceptPendingDraft() {
        if (_pendingDraft.value) {
            applyLocalDraft(_pendingDraft.value);
            _pendingDraft.value = null;
        }
    }

    function discardPendingDraft() {
        if (auditId.value) clearLocalDraft();
        hasPendingDraft.value = false;
        _pendingDraft.value = null;
    }

    function setResponse(
        questionId: number,
        status: string | null,
        comment?: string | null,
        correctedOnSite?: boolean,
    ) {
        const r = responses.value.get(questionId);
        if (!r) return;
        r.status = status;
        r.comment = comment !== undefined ? comment : r.comment;
        // Corrected-on-site only valid for NonConforming
        if (status !== 'NonConforming') {
            r.correctedOnSite = false;
        } else if (correctedOnSite !== undefined) {
            r.correctedOnSite = correctedOnSite;
        }
    }

    function setComment(questionId: number, comment: string | null) {
        const r = responses.value.get(questionId);
        if (r) r.comment = comment;
    }

    function setCorrectedOnSite(questionId: number, value: boolean) {
        const r = responses.value.get(questionId);
        if (r && r.status === 'NonConforming') r.correctedOnSite = value;
    }

    async function saveDraft(): Promise<boolean> {
        if (!auditId.value) return false;
        saving.value = true;
        try {
            await getClient().saveResponses(auditId.value, {
                header: { ...header.value },
                responses: allResponses.value.map(r => ({
                    questionId: r.questionId,
                    questionTextSnapshot: r.questionTextSnapshot,
                    status: r.status,
                    comment: r.comment,
                    correctedOnSite: r.correctedOnSite,
                })),
            });
            clearLocalDraft();
            isDirty.value = false;
            toast.add({ severity: 'success', summary: 'Saved', detail: 'Audit draft saved.', life: 2500 });
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save audit.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function submitAudit(): Promise<boolean> {
        if (!auditId.value) return false;
        // Save responses first
        const saved = await saveDraft();
        if (!saved) return false;
        saving.value = true;
        try {
            await getClient().submitAudit(auditId.value);
            auditStatus.value = 'Submitted';
            toast.add({ severity: 'success', summary: 'Submitted', detail: 'Audit submitted successfully.', life: 3000 });
            return true;
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: string } })?.response?.data ?? 'Failed to submit audit.';
            toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 5000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function deleteAudit(id: number): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().deleteAudit(id);
            toast.add({ severity: 'success', summary: 'Deleted', detail: 'Audit draft deleted.', life: 2500 });
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete audit.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function bulkDeleteAudits(ids: number[]): Promise<number> {
        saving.value = true;
        let deleted = 0;
        try {
            const results = await Promise.allSettled(ids.map(id => getClient().deleteAudit(id)));
            deleted = results.filter(r => r.status === 'fulfilled').length;
            const failed = results.length - deleted;
            if (deleted > 0) {
                toast.add({ severity: 'success', summary: 'Deleted', detail: `${deleted} draft${deleted !== 1 ? 's' : ''} deleted.`, life: 2500 });
            }
            if (failed > 0) {
                toast.add({ severity: 'warn', summary: 'Partial', detail: `${failed} could not be deleted.`, life: 4000 });
            }
            await loadAuditList();
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete audits.', life: 4000 });
        } finally {
            saving.value = false;
        }
        return deleted;
    }

    async function loadAuditList() {
        listLoading.value = true;
        try {
            audits.value = await getClient().getAuditList(
                filterDivisionId.value,
                filterStatus.value,
                filterAuditor.value || null,
                filterDateFrom.value || null,
                filterDateTo.value || null,
            );
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load audits.', life: 4000 });
        } finally {
            listLoading.value = false;
        }
    }

    async function loadReview(id: number) {
        reviewLoading.value = true;
        try {
            review.value = await getClient().getAuditReview(id);
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load review.', life: 4000 });
        } finally {
            reviewLoading.value = false;
        }
    }

    function resetForm() {
        auditId.value = null;
        auditStatus.value = 'Draft';
        template.value = null;
        enabledOptionalGroupKeys.value = [];
        header.value = {};
        responses.value = new Map();
        isDirty.value = false;
        hasPendingDraft.value = false;
        _pendingDraft.value = null;
        review.value = null;
        repeatFindingQuestionIds.value = new Set();
    }

    return {
        // State
        divisions,
        template,
        auditId,
        auditStatus,
        header,
        responses,
        audits,
        review,
        repeatFindingQuestionIds,
        loading,
        listLoading,
        reviewLoading,
        saving,
        isDirty,
        filterDivisionId,
        filterStatus,
        filterAuditor,
        filterDateFrom,
        filterDateTo,
        hasPendingDraft,
        // Computed
        allResponses,
        score,
        isSubmitted,
        visibleSections,
        enabledOptionalGroupKeys,
        // Actions
        loadDivisions,
        createAudit,
        deleteAudit,
        bulkDeleteAudits,
        loadAudit,
        acceptPendingDraft,
        discardPendingDraft,
        setResponse,
        setComment,
        setCorrectedOnSite,
        saveDraft,
        submitAudit,
        loadAuditList,
        loadReview,
        resetForm,
    };
});

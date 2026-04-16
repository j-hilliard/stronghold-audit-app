import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useToast } from 'primevue/usetoast';
import { useApiStore } from '@/stores/apiStore';
import {
    AuditClient,
    type TemplateVersionListItemDto,
    type DraftVersionDetailDto,
    type DraftSectionDto,
    type AddQuestionRequest,
    type AddSectionRequest,
    type UpdateSectionRequest,
    type UpdateQuestionRequest,
    type QuestionWeightItem,
    type SectionLibraryItemDto,
    type EmailRoutingRuleDto,
    type EmailRoutingRuleUpsertDto,
    type UserAuditRoleDto,
} from '@/apiclient/auditClient';

export type { DraftSectionDto };

export const useAdminStore = defineStore('auditAdmin', () => {
    const apiStore = useApiStore();
    const toast = useToast();

    function getClient() {
        return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
    }

    // ── State ──────────────────────────────────────────────────────────────────

    const templates = ref<TemplateVersionListItemDto[]>([]);
    const draftDetail = ref<DraftVersionDetailDto | null>(null);
    const sectionLibrary = ref<SectionLibraryItemDto[]>([]);
    const emailRules = ref<EmailRoutingRuleDto[]>([]);
    const userAuditRoles = ref<UserAuditRoleDto[]>([]);

    const loading = ref(false);
    const saving = ref(false);

    // ── Template list ──────────────────────────────────────────────────────────

    async function loadTemplates(force = false) {
        if (!force && templates.value.length > 0) return;
        loading.value = true;
        try {
            templates.value = await getClient().getTemplates();
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load templates.', life: 4000 });
        } finally {
            loading.value = false;
        }
    }

    async function cloneVersion(versionId: number): Promise<number | null> {
        saving.value = true;
        try {
            const newId = await getClient().cloneTemplateVersion(versionId);
            await loadTemplates(true);
            toast.add({ severity: 'success', summary: 'Draft created', detail: 'A new draft version has been created.', life: 3000 });
            return newId;
        } catch (err: any) {
            const msg = err?.response?.data?.detail ?? err?.response?.data ?? err?.message ?? 'Failed to create draft version.';
            toast.add({ severity: 'error', summary: 'Error', detail: String(msg), life: 6000 });
            return null;
        } finally {
            saving.value = false;
        }
    }

    // ── Draft editor ───────────────────────────────────────────────────────────

    async function loadDraft(draftId: number) {
        loading.value = true;
        draftDetail.value = null;
        try {
            draftDetail.value = await getClient().getDraftVersionDetail(draftId);
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load draft version.', life: 4000 });
        } finally {
            loading.value = false;
        }
    }

    async function addQuestion(draftId: number, request: AddQuestionRequest): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().addQuestion(draftId, request);
            await loadDraft(draftId);
            toast.add({ severity: 'success', summary: 'Question added', detail: 'Question added to the draft.', life: 2500 });
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to add question.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function updateQuestion(draftId: number, versionQuestionId: number, request: UpdateQuestionRequest): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().updateQuestion(draftId, versionQuestionId, request);
            // Update local state immediately — no need for full reload
            const section = draftDetail.value?.sections.find(s => s.questions.some(q => q.versionQuestionId === versionQuestionId));
            const q = section?.questions.find(q => q.versionQuestionId === versionQuestionId);
            if (q) {
                q.questionText = request.questionText;
                q.weight = request.weight;
                q.isLifeCritical = request.isLifeCritical;
                q.allowNA = request.allowNA;
                q.requireCommentOnNC = request.requireCommentOnNC;
                q.isScoreable = request.isScoreable;
            }
            return true;
        } catch (err: any) {
            const msg = err?.response?.data ?? err?.message ?? 'Failed to update question.';
            toast.add({ severity: 'error', summary: 'Error', detail: String(msg), life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function batchUpdateQuestionWeights(draftId: number, weights: QuestionWeightItem[]): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().batchUpdateQuestionWeights(draftId, weights);
            // Update local state immediately
            if (draftDetail.value) {
                for (const w of weights) {
                    for (const s of draftDetail.value.sections) {
                        const q = s.questions.find(q => q.versionQuestionId === w.versionQuestionId);
                        if (q) { q.weight = w.weight; break; }
                    }
                }
            }
            return true;
        } catch (err: any) {
            const msg = err?.response?.data ?? err?.message ?? 'Failed to save weights.';
            toast.add({ severity: 'error', summary: 'Error', detail: String(msg), life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function removeQuestion(draftId: number, versionQuestionId: number): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().removeQuestion(draftId, versionQuestionId);
            await loadDraft(draftId);
            toast.add({ severity: 'success', summary: 'Removed', detail: 'Question removed from draft.', life: 2500 });
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove question.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function reorderQuestions(draftId: number, versionQuestionIds: number[]): Promise<void> {
        // Optimistic update — actual reload happens on error
        try {
            await getClient().reorderQuestions(draftId, versionQuestionIds);
        } catch {
            // Reload to restore server order on failure
            await loadDraft(draftId);
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save new question order.', life: 4000 });
        }
    }

    async function publishDraft(draftId: number): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().publishTemplateVersion(draftId);
            await loadTemplates(true);
            draftDetail.value = null;
            toast.add({ severity: 'success', summary: 'Published', detail: 'Template version is now Active.', life: 3000 });
            return true;
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: string } })?.response?.data ?? 'Failed to publish version.';
            toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 5000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    // ── Section library ────────────────────────────────────────────────────────

    async function loadSectionLibrary(force = false) {
        if (!force && sectionLibrary.value.length > 0) return;
        loading.value = true;
        try {
            sectionLibrary.value = await getClient().getSectionLibrary();
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load section library.', life: 4000 });
        } finally {
            loading.value = false;
        }
    }

    async function copySection(draftId: number, sourceSectionId: number): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().copySection(draftId, sourceSectionId);
            await loadDraft(draftId);
            toast.add({ severity: 'success', summary: 'Section added', detail: 'Section copied into draft.', life: 2500 });
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to copy section.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    // ── Section management ─────────────────────────────────────────────────────

    async function addSection(draftId: number, name: string): Promise<boolean> {
        saving.value = true;
        try {
            const request: AddSectionRequest = { name };
            await getClient().addSection(draftId, request);
            await loadDraft(draftId);
            toast.add({ severity: 'success', summary: 'Section added', detail: `Section "${name}" added to the draft.`, life: 2500 });
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to add section.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function updateSection(draftId: number, sectionId: number, request: UpdateSectionRequest): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().updateSection(draftId, sectionId, request);
            await loadDraft(draftId);
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to update section.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function removeSection(draftId: number, sectionId: number): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().removeSection(draftId, sectionId);
            await loadDraft(draftId);
            toast.add({ severity: 'success', summary: 'Removed', detail: 'Section removed from draft.', life: 2500 });
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove section.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function reorderSections(draftId: number, sectionIds: number[]): Promise<void> {
        try {
            await getClient().reorderSections(draftId, sectionIds);
        } catch {
            await loadDraft(draftId);
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save new section order.', life: 4000 });
        }
    }

    // ── Email routing ──────────────────────────────────────────────────────────

    async function loadEmailRouting() {
        loading.value = true;
        try {
            emailRules.value = await getClient().getEmailRouting();
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load email routing.', life: 4000 });
        } finally {
            loading.value = false;
        }
    }

    // ── User audit roles ───────────────────────────────────────────────────────

    async function loadUserAuditRoles() {
        loading.value = true;
        try {
            userAuditRoles.value = await getClient().getUsersWithAuditRoles();
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load user roles.', life: 4000 });
        } finally {
            loading.value = false;
        }
    }

    async function setUserAuditRole(userId: number, roleName: string | null): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().setUserAuditRole(userId, roleName);
            // Update local state immediately
            const row = userAuditRoles.value.find(u => u.userId === userId);
            if (row) row.auditRole = roleName ?? undefined;
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to update user role.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    async function saveEmailRouting(rules: EmailRoutingRuleUpsertDto[]): Promise<boolean> {
        saving.value = true;
        try {
            await getClient().updateEmailRouting(rules);
            await loadEmailRouting();
            toast.add({ severity: 'success', summary: 'Saved', detail: 'Email routing rules saved.', life: 2500 });
            return true;
        } catch {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save email routing.', life: 4000 });
            return false;
        } finally {
            saving.value = false;
        }
    }

    return {
        // State
        templates,
        draftDetail,
        sectionLibrary,
        emailRules,
        userAuditRoles,
        loading,
        saving,
        // Actions
        loadTemplates,
        cloneVersion,
        loadDraft,
        addQuestion,
        updateQuestion,
        batchUpdateQuestionWeights,
        removeQuestion,
        reorderQuestions,
        publishDraft,
        loadSectionLibrary,
        copySection,
        addSection,
        updateSection,
        removeSection,
        reorderSections,
        loadEmailRouting,
        saveEmailRouting,
        loadUserAuditRoles,
        setUserAuditRole,
    };
});

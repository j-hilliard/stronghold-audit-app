/**
 * Handwritten TypeScript API client for the Audit module.
 * Mirrors the shape of the NSwag-generated IncidentReportClient but written manually
 * because the NSwag post-build step cannot run on the OneDrive path (comma in folder name).
 *
 * Keep in sync with Api/Models/Audit/*.cs
 */

import type { AxiosInstance, CancelToken } from 'axios';

// â”€â”€ DTOs (mirror Api/Models/Audit/*.cs) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

export interface DivisionDto {
    id: number;
    code: string;
    name: string;
    /** "JobSite" | "Facility" */
    auditType: string;
}

export interface TemplateQuestionDto {
    /** AuditVersionQuestion.Id */
    versionQuestionId: number;
    /** AuditQuestion.Id — stored on AuditResponse */
    questionId: number;
    questionText: string;
    displayOrder: number;
    allowNA: boolean;
    requireCommentOnNC: boolean;
    isScoreable: boolean;
    /** When true, a NonConforming answer causes the entire audit to auto-fail */
    isLifeCritical: boolean;
}

export interface TemplateSectionDto {
    id: number;
    name: string;
    displayOrder: number;
    /** When true, only shown when explicitly enabled at audit creation */
    isOptional: boolean;
    /** Groups optional sections toggled together (e.g. “RADIOGRAPHY”) */
    optionalGroupKey?: string | null;
    questions: TemplateQuestionDto[];
}

export interface LogicRuleDto {
    id: number;
    triggerVersionQuestionId: number;
    /** "NonConforming" | "Conforming" | "Warning" | "NA" | "AnyAnswer" */
    triggerResponse: string;
    /** "HideSection" | "ShowSection" */
    action: string;
    targetSectionId?: number | null;
}

export interface TemplateDto {
    versionId: number;
    versionNumber: number;
    divisionCode: string;
    divisionName: string;
    /** "JobSite" | "Facility" */
    auditType: string;
    sections: TemplateSectionDto[];
    /** Section-level skip-logic rules evaluated client-side */
    logicRules: LogicRuleDto[];
}

export interface AuditHeaderDto {
    id?: number;
    jobNumber?: string;
    client?: string;
    pm?: string;
    unit?: string;
    time?: string;
    shift?: string;
    workDescription?: string;
    company1?: string;
    company2?: string;
    company3?: string;
    responsibleParty?: string;
    location?: string;
    /** ISO date string "YYYY-MM-DD" */
    auditDate?: string;
    auditor?: string;
}

export interface AuditResponseDto {
    id: number;
    questionId: number;
    questionTextSnapshot: string;
    /** "Conforming" | "NonConforming" | "Warning" | "NA" | null (unanswered) */
    status?: string | null;
    comment?: string | null;
    correctedOnSite: boolean;
}

export interface AuditResponseUpsertDto {
    questionId: number;
    questionTextSnapshot: string;
    status?: string | null;
    comment?: string | null;
    correctedOnSite: boolean;
}

export interface SaveResponsesRequest {
    header?: AuditHeaderDto;
    responses: AuditResponseUpsertDto[];
}

export interface AuditDetailDto {
    id: number;
    divisionId: number;
    divisionCode: string;
    divisionName: string;
    templateVersionId: number;
    /** "JobSite" | "Facility" */
    auditType: string;
    /** "Draft" | "Submitted" | "Reopened" | "Closed" */
    status: string;
    createdAt: string;
    createdBy: string;
    submittedAt?: string | null;
    header?: AuditHeaderDto | null;
    responses: AuditResponseDto[];
    /** Optional section group keys that were enabled at audit creation (immutable). */
    enabledOptionalGroupKeys: string[];
}

export interface AuditListItemDto {
    id: number;
    divisionCode: string;
    divisionName: string;
    auditType: string;
    status: string;
    createdBy: string;
    createdAt: string;
    submittedAt?: string | null;
    auditor?: string | null;
    auditDate?: string | null;
    jobNumber?: string | null;
    location?: string | null;
}

export interface AuditFindingDto {
    id: number;
    questionText: string;
    comment?: string | null;
    correctedOnSite: boolean;
    correctiveActions: CorrectiveActionDto[];
}

export interface ReviewResponseItemDto {
    questionId: number;
    questionText: string;
    status?: string | null;
    comment?: string | null;
    correctedOnSite: boolean;
    sortOrder: number;
}

export interface ReviewSectionDto {
    sectionName: string;
    items: ReviewResponseItemDto[];
}

export interface EmailRoutingDto {
    emailAddress: string;
}

// ── Admin DTOs (mirror Api/Models/Audit/AdminDto.cs) ──────────────────────────

export interface TemplateVersionListItemDto {
    id: number;
    templateId: number;
    divisionCode: string;
    divisionName: string;
    versionNumber: number;
    /** "Draft" | "Active" | "Superseded" */
    status: string;
    publishedAt?: string | null;
    publishedBy?: string | null;
    clonedFromVersionId?: number | null;
    questionCount: number;
}

export interface DraftQuestionDto {
    versionQuestionId: number;
    questionId: number;
    questionText: string;
    shortLabel?: string | null;
    helpText?: string | null;
    displayOrder: number;
    allowNA: boolean;
    requireCommentOnNC: boolean;
    isScoreable: boolean;
    isArchived: boolean;
    responseTypeId?: number | null;
    responseTypeCode?: string | null;
    weight: number;
    isLifeCritical: boolean;
}

export interface DraftSectionDto {
    id: number;
    name: string;
    sectionCode?: string | null;
    displayOrder: number;
    isRequired: boolean;
    weight: number;
    isOptional: boolean;
    optionalGroupKey?: string | null;
    reportingCategoryId?: number | null;
    reportingCategoryName?: string | null;
    questions: DraftQuestionDto[];
}

export interface DraftVersionDetailDto {
    id: number;
    versionNumber: number;
    divisionCode: string;
    divisionName: string;
    sections: DraftSectionDto[];
}

export interface AddQuestionRequest {
    sectionId: number;
    questionText: string;
    allowNA?: boolean;
    requireCommentOnNC?: boolean;
    isScoreable?: boolean;
}

export interface ReorderQuestionsRequest {
    versionQuestionIds: number[];
}

export interface EmailRoutingRuleDto {
    id: number;
    divisionId: number;
    divisionCode: string;
    divisionName: string;
    emailAddress: string;
    isActive: boolean;
}

export interface EmailRoutingRuleUpsertDto {
    id?: number | null;
    divisionId: number;
    emailAddress: string;
    isActive: boolean;
}

export interface UpdateEmailRoutingRequest {
    rules: EmailRoutingRuleUpsertDto[];
}

export interface UserAuditRoleDto {
    userId: number;
    firstName?: string | null;
    lastName?: string | null;
    email?: string | null;
    /** Current audit role name, or null if none assigned */
    auditRole?: string | null;
}

export interface SectionLibraryItemDto {
    sectionId: number;
    name: string;
    sectionCode?: string | null;
    divisionCode: string;
    divisionName: string;
    questionCount: number;
}

export interface CopySectionRequest {
    sourceSectionId: number;
}

export interface AddSectionRequest {
    name: string;
    weight?: number;
    isOptional?: boolean;
    optionalGroupKey?: string | null;
}

export interface UpdateSectionRequest {
    name: string;
    isRequired: boolean;
    weight: number;
    isOptional: boolean;
    optionalGroupKey?: string | null;
    reportingCategoryId?: number | null;
}

export interface UpdateQuestionRequest {
    questionText: string;
    weight: number;
    isLifeCritical: boolean;
    allowNA: boolean;
    requireCommentOnNC: boolean;
    isScoreable: boolean;
}

export interface QuestionWeightItem {
    versionQuestionId: number;
    weight: number;
}

export interface BatchUpdateQuestionWeightsRequest {
    weights: QuestionWeightItem[];
}

export interface ReorderSectionsRequest {
    sectionIds: number[];
}

export interface CorrectiveActionDto {
    id: number;
    findingId: number;
    auditId?: number | null;
    description: string;
    assignedTo?: string | null;
    dueDate?: string | null;
    completedDate?: string | null;
    status: string;
    createdBy: string;
    createdAt: string;
}

export interface CorrectiveActionListItemDto {
    id: number;
    auditId: number;
    divisionCode: string;
    divisionName: string;
    jobNumber?: string | null;
    auditDate?: string | null;
    description: string;
    status: string;
    assignedTo?: string | null;
    dueDate?: string | null;
    completedDate?: string | null;
    isOverdue: boolean;
    questionText: string;
    sectionName: string;
    createdAt: string;
}

export interface AssignCorrectiveActionRequest {
    findingId: number;
    description: string;
    assignedTo?: string | null;
    dueDate?: string | null;
}

export interface CloseCorrectiveActionRequest {
    notes: string;
    completedDate?: string | null;
}

export interface AuditTrendPointDto {
    week: string;
    avgScore: number | null;
    auditCount: number;
}

export interface AuditReportRowDto {
    id: number;
    divisionCode: string;
    status: string;
    auditDate?: string | null;
    auditor?: string | null;
    jobNumber?: string | null;
    location?: string | null;
    scorePercent?: number | null;
    nonConformingCount: number;
    warningCount: number;
}

export interface SectionNcBreakdownDto {
    sectionName: string;
    ncCount: number;
}

export interface OpenCorrectiveActionSummaryDto {
    id: number;
    auditId: number;
    description: string;
    assignedTo?: string | null;
    dueDate?: string | null;
    status: string;
    isOverdue: boolean;
    daysOpen: number;
}

export interface AuditReportDto {
    totalAudits: number;
    avgScorePercent?: number | null;
    totalNonConforming: number;
    totalWarnings: number;
    correctedOnSiteCount: number;
    trend: AuditTrendPointDto[];
    sectionBreakdown: SectionNcBreakdownDto[];
    openCorrectiveActions: OpenCorrectiveActionSummaryDto[];
    rows: AuditReportRowDto[];
}

export interface SectionTrendPointDto {
    quarter: string;
    findingsPerAudit: number;
    auditCount: number;
    ncCount: number;
}

export interface SectionTrendDto {
    sectionName: string;
    divisionTrend: SectionTrendPointDto[];
    companyTrend: SectionTrendPointDto[];
}

export interface SectionTrendsReportDto {
    quarters: string[];
    sections: SectionTrendDto[];
}

export interface NewsletterAiSummaryRequest {
    divisionCode: string;
    quarter: number;
    year: number;
    avgScore?: number | null;
    totalAudits: number;
    totalNcs: number;
    topSections: { sectionName: string; ncCount: number }[];
    openCaCount: number;
    overdueCaCount: number;
}

export interface NewsletterAiSummaryResult {
    success: boolean;
    text: string;
}

export interface NewsletterSendRequest {
    divisionId: number;
    subject: string;
    htmlBody: string;
}

export interface NewsletterSendResult {
    sent: number;
    dryRun: boolean;
    recipients: string[];
}

// ── Newsletter Templates ──────────────────────────────────────────────────────

export interface NewsletterTemplateDto {
    id?: number | null;
    divisionId: number;
    name: string;
    primaryColor: string;
    accentColor: string;
    coverImageUrl?: string | null;
    visibleSections?: string[] | null;
    isDefault: boolean;
}

export interface SaveNewsletterTemplateRequest {
    divisionId: number;
    name: string;
    primaryColor: string;
    accentColor: string;
    coverImageUrl?: string | null;
    visibleSections?: string[] | null;
    isDefault: boolean;
}

// ── Report Drafts ─────────────────────────────────────────────────────────────

export interface ReportDraftListItemDto {
    id: number;
    divisionId: number;
    divisionCode: string;
    title: string;
    period: string;
    /** "YYYY-MM-DD" or null */
    dateFrom?: string | null;
    /** "YYYY-MM-DD" or null */
    dateTo?: string | null;
    createdAt: string;
    updatedAt?: string | null;
    createdBy: string;
}

export interface ReportDraftDto {
    id: number;
    divisionId: number;
    divisionCode: string;
    title: string;
    period: string;
    dateFrom?: string | null;
    dateTo?: string | null;
    /** Serialized ReportBlock[] JSON. Only useReportDraft.ts may deserialize this. */
    blocksJson: string;
    /** Base64-encoded row version. Must be round-tripped on every PUT. */
    rowVersion: string;
    createdAt: string;
    updatedAt?: string | null;
    createdBy: string;
}

export interface CreateReportDraftRequest {
    divisionId: number;
    title: string;
    period: string;
    dateFrom?: string | null;
    dateTo?: string | null;
    blocksJson: string;
}

export interface UpdateReportDraftRequest {
    title: string;
    period: string;
    dateFrom?: string | null;
    dateTo?: string | null;
    blocksJson: string;
    /** Base64-encoded row version from the last GET. */
    rowVersion: string;
}

// ── Attachments ──────────────────────────────────────────────────────────────

export interface AuditAttachmentDto {
    id: number;
    fileName: string;
    uploadedBy: string;
    uploadedAt: string;
    fileSizeBytes: number;
    downloadUrl: string;
}

export interface AuditReviewDto {
    id: number;
    divisionCode: string;
    divisionName: string;
    auditType: string;
    status: string;
    header?: AuditHeaderDto | null;
    conformingCount: number;
    nonConformingCount: number;
    warningCount: number;
    naCount: number;
    unansweredCount: number;
    /** Null if no scored items answered */
    scorePercent?: number | null;
    /** True when at least one life-critical question was answered NonConforming — entire audit auto-fails */
    hasLifeCriticalFailure: boolean;
    /** Question texts of life-critical NC responses */
    lifeCriticalFailures: string[];
    /** AI-generated plain-language summary, null when not available */
    aiSummary?: string | null;
    nonConformingItems: AuditFindingDto[];
    warningItems: ReviewResponseItemDto[];
    sections: ReviewSectionDto[];
    reviewEmailRouting: EmailRoutingDto[];
}

type JsonObject = Record<string, unknown>;

function readString(input: unknown): string {
    return typeof input === 'string' ? input : '';
}

function readNumber(input: unknown): number | null {
    if (typeof input === 'number' && Number.isFinite(input)) return input;
    if (typeof input === 'string' && input.trim() !== '') {
        const parsed = Number(input);
        if (Number.isFinite(parsed)) return parsed;
    }
    return null;
}

function normalizeDivision(raw: unknown): DivisionDto | null {
    if (!raw || typeof raw !== 'object') return null;

    const data = raw as JsonObject;
    const id = readNumber(data['id'] ?? data['Id']);
    if (id === null || id <= 0) return null;

    const code = readString(data['code'] ?? data['Code']).trim();
    const name = readString(data['name'] ?? data['Name']).trim();
    const auditType = readString(data['auditType'] ?? data['AuditType']).trim();

    return {
        id,
        code,
        name,
        auditType,
    };
}

// â”€â”€ Client class â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

export class AuditClient {
    private instance: AxiosInstance;
    private baseUrl: string;

    constructor(baseUrl?: string, instance?: AxiosInstance) {
        if (!instance) throw new Error('AxiosInstance is required');
        this.instance = instance;
        this.baseUrl = baseUrl ?? '';
    }

    // â”€â”€ Divisions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    getDivisions(cancelToken?: CancelToken): Promise<DivisionDto[]> {
        return this.instance
            .get<unknown>(`${this.baseUrl}/v1/divisions`, { cancelToken })
            .then(r => {
                const rows = Array.isArray(r.data) ? r.data : [];
                const normalized = rows
                    .map(normalizeDivision)
                    .filter((div): div is DivisionDto => div !== null);

                // Deduplicate by code so that seeder duplicates in the DB
                // don't produce hundreds of identical cards in the UI.
                // Keeps the first (lowest Id) record for each division code.
                const unique = new Map<string, DivisionDto>();
                for (const div of normalized) {
                    if (div.code && !unique.has(div.code)) {
                        unique.set(div.code, div);
                    }
                }
                return Array.from(unique.values()).sort((a, b) => a.code.localeCompare(b.code));
            });
    }

    // â”€â”€ Templates â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    getActiveTemplate(divisionId: number, cancelToken?: CancelToken): Promise<TemplateDto> {
        return this.instance
            .get<TemplateDto>(`${this.baseUrl}/v1/templates/active`, { params: { divisionId }, cancelToken })
            .then(r => r.data);
    }

    // â”€â”€ Audits â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    createAudit(divisionId: number, enabledOptionalGroupKeys: string[] = [], cancelToken?: CancelToken): Promise<number> {
        return this.instance
            .post<number>(`${this.baseUrl}/v1/audits`, { divisionId, enabledOptionalGroupKeys }, { cancelToken })
            .then(r => r.data);
    }

    getAudit(id: number, cancelToken?: CancelToken): Promise<AuditDetailDto> {
        return this.instance
            .get<AuditDetailDto>(`${this.baseUrl}/v1/audits/${id}`, { cancelToken })
            .then(r => r.data);
    }

    getAuditList(
        divisionId?: number | null,
        status?: string | null,
        auditor?: string | null,
        dateFrom?: string | null,
        dateTo?: string | null,
        cancelToken?: CancelToken,
    ): Promise<AuditListItemDto[]> {
        const params: Record<string, unknown> = {};
        if (divisionId != null) params['divisionId'] = divisionId;
        if (status) params['status'] = status;
        if (auditor) params['auditor'] = auditor;
        if (dateFrom) params['dateFrom'] = dateFrom;
        if (dateTo) params['dateTo'] = dateTo;
        return this.instance
            .get<AuditListItemDto[]>(`${this.baseUrl}/v1/audits`, { params, cancelToken })
            .then(r => r.data);
    }

    deleteAudit(id: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/audits/${id}`, { cancelToken })
            .then(() => undefined);
    }

    saveResponses(id: number, request: SaveResponsesRequest, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/audits/${id}/responses`, request, { cancelToken })
            .then(() => undefined);
    }

    submitAudit(id: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .post(`${this.baseUrl}/v1/audits/${id}/submit`, null, { cancelToken })
            .then(() => undefined);
    }

    assignCorrectiveAction(request: AssignCorrectiveActionRequest, cancelToken?: CancelToken): Promise<number> {
        return this.instance
            .post<number>(`${this.baseUrl}/v1/audits/corrective-actions`, request, { cancelToken })
            .then(r => r.data);
    }

    closeCorrectiveAction(id: number, request: CloseCorrectiveActionRequest, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/audits/corrective-actions/${id}/close`, request, { cancelToken })
            .then(() => undefined);
    }

    getAuditReport(
        divisionId?: number | null,
        status?: string | null,
        dateFrom?: string | null,
        dateTo?: string | null,
        sectionFilter?: string | null,
        cancelToken?: CancelToken,
    ): Promise<AuditReportDto> {
        const params: Record<string, unknown> = {};
        if (divisionId != null) params['divisionId'] = divisionId;
        if (status) params['status'] = status;
        if (dateFrom) params['dateFrom'] = dateFrom;
        if (dateTo) params['dateTo'] = dateTo;
        if (sectionFilter) params['sectionFilter'] = sectionFilter;
        return this.instance
            .get<AuditReportDto>(`${this.baseUrl}/v1/audits/report`, { params, cancelToken })
            .then(r => r.data);
    }

    getSectionTrends(
        divisionId?: number | null,
        dateFrom?: string | null,
        dateTo?: string | null,
        cancelToken?: CancelToken,
    ): Promise<SectionTrendsReportDto> {
        const params: Record<string, unknown> = {};
        if (divisionId != null) params['divisionId'] = divisionId;
        if (dateFrom) params['dateFrom'] = dateFrom;
        if (dateTo) params['dateTo'] = dateTo;

        return this.instance
            .get<SectionTrendsReportDto>(`${this.baseUrl}/v1/audits/section-trends`, { params, cancelToken })
            .then(r => r.data);
    }

    getAuditReview(id: number, cancelToken?: CancelToken): Promise<AuditReviewDto> {
        return this.instance
            .get<AuditReviewDto>(`${this.baseUrl}/v1/audits/${id}/review`, { cancelToken })
            .then(r => r.data);
    }

    // ── Admin — Templates ─────────────────────────────────────────────────────

    getTemplates(cancelToken?: CancelToken): Promise<TemplateVersionListItemDto[]> {
        return this.instance
            .get<TemplateVersionListItemDto[]>(`${this.baseUrl}/v1/admin/templates`, { cancelToken })
            .then(r => r.data);
    }

    getDraftVersionDetail(draftId: number, cancelToken?: CancelToken): Promise<DraftVersionDetailDto> {
        return this.instance
            .get<DraftVersionDetailDto>(`${this.baseUrl}/v1/admin/versions/${draftId}`, { cancelToken })
            .then(r => r.data);
    }

    cloneTemplateVersion(versionId: number, cancelToken?: CancelToken): Promise<number> {
        return this.instance
            .post<number>(`${this.baseUrl}/v1/admin/templates/${versionId}/clone`, null, { cancelToken })
            .then(r => r.data);
    }

    // ── Admin — Questions ─────────────────────────────────────────────────────

    addQuestion(draftId: number, request: AddQuestionRequest, cancelToken?: CancelToken): Promise<number> {
        return this.instance
            .post<number>(`${this.baseUrl}/v1/admin/versions/${draftId}/questions`, request, { cancelToken })
            .then(r => r.data);
    }

    updateQuestion(draftId: number, versionQuestionId: number, request: UpdateQuestionRequest, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/admin/versions/${draftId}/questions/${versionQuestionId}`, request, { cancelToken })
            .then(() => undefined);
    }

    batchUpdateQuestionWeights(draftId: number, weights: QuestionWeightItem[], cancelToken?: CancelToken): Promise<void> {
        const request: BatchUpdateQuestionWeightsRequest = { weights };
        return this.instance
            .put(`${this.baseUrl}/v1/admin/versions/${draftId}/questions/weights`, request, { cancelToken })
            .then(() => undefined);
    }

    removeQuestion(draftId: number, versionQuestionId: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/admin/versions/${draftId}/questions/${versionQuestionId}`, { cancelToken })
            .then(() => undefined);
    }

    reorderQuestions(draftId: number, versionQuestionIds: number[], cancelToken?: CancelToken): Promise<void> {
        const request: ReorderQuestionsRequest = { versionQuestionIds };
        return this.instance
            .put(`${this.baseUrl}/v1/admin/versions/${draftId}/questions/reorder`, request, { cancelToken })
            .then(() => undefined);
    }

    publishTemplateVersion(draftId: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/admin/versions/${draftId}/publish`, null, { cancelToken })
            .then(() => undefined);
    }

    // ── Admin — Email Routing ─────────────────────────────────────────────────

    // ── Admin — Section Library ───────────────────────────────────────────────

    getSectionLibrary(cancelToken?: CancelToken): Promise<SectionLibraryItemDto[]> {
        return this.instance
            .get<SectionLibraryItemDto[]>(`${this.baseUrl}/v1/admin/section-library`, { cancelToken })
            .then(r => r.data);
    }

    copySection(draftId: number, sourceSectionId: number, cancelToken?: CancelToken): Promise<number> {
        const request: CopySectionRequest = { sourceSectionId };
        return this.instance
            .post<number>(`${this.baseUrl}/v1/admin/versions/${draftId}/sections/copy`, request, { cancelToken })
            .then(r => r.data);
    }

    // ── Admin — Sections ──────────────────────────────────────────────────────

    addSection(draftId: number, request: AddSectionRequest, cancelToken?: CancelToken): Promise<number> {
        return this.instance
            .post<number>(`${this.baseUrl}/v1/admin/versions/${draftId}/sections`, request, { cancelToken })
            .then(r => r.data);
    }

    updateSection(draftId: number, sectionId: number, request: UpdateSectionRequest, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/admin/versions/${draftId}/sections/${sectionId}`, request, { cancelToken })
            .then(() => undefined);
    }

    removeSection(draftId: number, sectionId: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/admin/versions/${draftId}/sections/${sectionId}`, { cancelToken })
            .then(() => undefined);
    }

    reorderSections(draftId: number, sectionIds: number[], cancelToken?: CancelToken): Promise<void> {
        const request: ReorderSectionsRequest = { sectionIds };
        return this.instance
            .put(`${this.baseUrl}/v1/admin/versions/${draftId}/sections/reorder`, request, { cancelToken })
            .then(() => undefined);
    }

    // ── Admin — Email Routing ─────────────────────────────────────────────────

    getEmailRouting(cancelToken?: CancelToken): Promise<EmailRoutingRuleDto[]> {
        return this.instance
            .get<EmailRoutingRuleDto[]>(`${this.baseUrl}/v1/admin/email-routing`, { cancelToken })
            .then(r => r.data);
    }

    updateEmailRouting(rules: EmailRoutingRuleUpsertDto[], cancelToken?: CancelToken): Promise<void> {
        const request: UpdateEmailRoutingRequest = { rules };
        return this.instance
            .put(`${this.baseUrl}/v1/admin/email-routing`, request, { cancelToken })
            .then(() => undefined);
    }

    // ── Admin — User Audit Roles ──────────────────────────────────────────────

    getUsersWithAuditRoles(cancelToken?: CancelToken): Promise<UserAuditRoleDto[]> {
        return this.instance
            .get<UserAuditRoleDto[]>(`${this.baseUrl}/v1/admin/users/audit-roles`, { cancelToken })
            .then(r => r.data);
    }

    setUserAuditRole(userId: number, roleName: string | null, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/admin/users/${userId}/audit-role`, { roleName }, { cancelToken })
            .then(() => undefined);
    }

    // ── Newsletter ─────────────────────────────────────────────────────────────

    generateNewsletterSummary(
        request: NewsletterAiSummaryRequest,
        cancelToken?: CancelToken,
    ): Promise<NewsletterAiSummaryResult> {
        return this.instance
            .post<NewsletterAiSummaryResult>(`${this.baseUrl}/v1/audits/newsletter/ai-summary`, request, { cancelToken })
            .then(r => r.data);
    }

    sendNewsletter(
        request: NewsletterSendRequest,
        cancelToken?: CancelToken,
    ): Promise<NewsletterSendResult> {
        return this.instance
            .post<NewsletterSendResult>(`${this.baseUrl}/v1/audits/newsletter/send`, request, { cancelToken })
            .then(r => r.data);
    }

    // ── Report Drafts ─────────────────────────────────────────────────────────

    getReportDrafts(divisionId?: number | null, cancelToken?: CancelToken): Promise<ReportDraftListItemDto[]> {
        const params: Record<string, unknown> = {};
        if (divisionId != null) params['divisionId'] = divisionId;
        return this.instance
            .get<ReportDraftListItemDto[]>(`${this.baseUrl}/v1/audits/report-drafts`, { params, cancelToken })
            .then(r => r.data);
    }

    getReportDraft(id: number, cancelToken?: CancelToken): Promise<ReportDraftDto> {
        return this.instance
            .get<ReportDraftDto>(`${this.baseUrl}/v1/audits/report-drafts/${id}`, { cancelToken })
            .then(r => r.data);
    }

    createReportDraft(request: CreateReportDraftRequest, cancelToken?: CancelToken): Promise<number> {
        return this.instance
            .post<number>(`${this.baseUrl}/v1/audits/report-drafts`, request, { cancelToken })
            .then(r => r.data);
    }

    updateReportDraft(id: number, request: UpdateReportDraftRequest, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/audits/report-drafts/${id}`, request, { cancelToken })
            .then(() => undefined);
    }

    deleteReportDraft(id: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/audits/report-drafts/${id}`, { cancelToken })
            .then(() => undefined);
    }

    getCorrectiveActions(
        divisionId?: number | null,
        status?: string | null,
        cancelToken?: CancelToken,
    ): Promise<CorrectiveActionListItemDto[]> {
        const params: Record<string, unknown> = {};
        if (divisionId != null) params['divisionId'] = divisionId;
        if (status) params['status'] = status;
        return this.instance
            .get<CorrectiveActionListItemDto[]>(`${this.baseUrl}/v1/audits/corrective-actions`, { params, cancelToken })
            .then(r => r.data);
    }

    reopenAudit(id: number, reason?: string | null, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .post(`${this.baseUrl}/v1/audits/${id}/reopen`, { reason }, { cancelToken })
            .then(() => undefined);
    }

    closeAudit(id: number, notes?: string | null, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .post(`${this.baseUrl}/v1/audits/${id}/close`, { notes }, { cancelToken })
            .then(() => undefined);
    }

    // ── Newsletter Templates ──────────────────────────────────────────────────

    getNewsletterTemplate(divisionId: number, cancelToken?: CancelToken): Promise<NewsletterTemplateDto | null> {
        return this.instance
            .get<NewsletterTemplateDto>(`${this.baseUrl}/v1/audits/newsletter-template`, { params: { divisionId }, cancelToken })
            .then(r => r.data)
            .catch(err => {
                if (err?.response?.status === 404) return null;
                throw err;
            });
    }

    saveNewsletterTemplate(request: SaveNewsletterTemplateRequest, cancelToken?: CancelToken): Promise<NewsletterTemplateDto> {
        return this.instance
            .put<NewsletterTemplateDto>(`${this.baseUrl}/v1/audits/newsletter-template`, request, { cancelToken })
            .then(r => r.data);
    }

    // ── Attachments ───────────────────────────────────────────────────────────

    getAttachments(auditId: number, cancelToken?: CancelToken): Promise<AuditAttachmentDto[]> {
        return this.instance
            .get<AuditAttachmentDto[]>(`${this.baseUrl}/v1/audits/${auditId}/attachments`, { cancelToken })
            .then(r => r.data);
    }

    uploadAttachment(auditId: number, file: File, cancelToken?: CancelToken): Promise<AuditAttachmentDto> {
        const form = new FormData();
        form.append('file', file, file.name);
        return this.instance
            .post<AuditAttachmentDto>(`${this.baseUrl}/v1/audits/${auditId}/attachments`, form, {
                headers: { 'Content-Type': 'multipart/form-data' },
                cancelToken,
            })
            .then(r => r.data);
    }

    downloadAttachment(auditId: number, attachmentId: number, cancelToken?: CancelToken): Promise<{ blob: Blob; fileName: string }> {
        return this.instance
            .get(`${this.baseUrl}/v1/audits/${auditId}/attachments/${attachmentId}/download`, {
                responseType: 'blob',
                cancelToken,
            })
            .then(r => {
                const disposition: string = r.headers['content-disposition'] ?? '';
                const match = disposition.match(/filename\*?=(?:UTF-8'')?["']?([^;"'\n]+)["']?/i);
                const fileName = match ? decodeURIComponent(match[1].trim()) : `attachment-${attachmentId}`;
                return { blob: r.data as Blob, fileName };
            });
    }

    deleteAttachment(auditId: number, attachmentId: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/audits/${auditId}/attachments/${attachmentId}`, { cancelToken })
            .then(() => undefined);
    }

    // ── Logic rules ───────────────────────────────────────────────────────────

    getLogicRules(templateVersionId: number, cancelToken?: CancelToken): Promise<LogicRuleDto[]> {
        return this.instance
            .get<LogicRuleDto[]>(`${this.baseUrl}/v1/admin/templates/${templateVersionId}/logic-rules`, { cancelToken })
            .then(r => r.data);
    }

    upsertLogicRule(rule: {
        id?: number | null;
        templateVersionId: number;
        triggerVersionQuestionId: number;
        triggerResponse: string;
        action: string;
        targetSectionId?: number | null;
    }, cancelToken?: CancelToken): Promise<LogicRuleDto> {
        return this.instance
            .put<LogicRuleDto>(`${this.baseUrl}/v1/admin/templates/logic-rules`, rule, { cancelToken })
            .then(r => r.data);
    }

    deleteLogicRule(id: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/admin/templates/logic-rules/${id}`, { cancelToken })
            .then(() => undefined);
    }
}

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
    /** When true, auditor must attach at least one photo when marking NonConforming */
    requirePhotoOnNc: boolean;
    /** When true, a NonConforming response auto-creates a CorrectiveAction at submit time */
    autoCreateCa: boolean;
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
    /** 3-char auditor-entered site code, e.g. "IPT" */
    siteCode?: string;
}

export interface DivisionJobPrefixDto {
    id: number;
    prefix: string;
    label: string;
    isDefault: boolean;
}

export interface DivisionJobPrefixUpsertDto {
    prefix: string;
    label: string;
    isDefault: boolean;
    sortOrder: number;
}

export interface SaveDivisionJobPrefixesRequest {
    prefixes: DivisionJobPrefixUpsertDto[];
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

export interface SectionNaOverrideDto {
    sectionId: number;
    reason: string;
}

export interface SaveResponsesRequest {
    header?: AuditHeaderDto;
    responses: AuditResponseUpsertDto[];
    /** Current section N/A overrides — full replace on save */
    sectionNaOverrides: SectionNaOverrideDto[];
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
    /** Auto-generated audit number, e.g. "H26-003-IPT" */
    trackingNumber?: string | null;
    /** Sections the auditor has marked N/A for this audit (mutable while Draft/Reopened). */
    sectionNaOverrides: SectionNaOverrideDto[];
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
    trackingNumber?: string | null;
}

export interface AuditFindingDto {
    id: number;
    /** AuditQuestion.Id — used to cross-reference repeat finding badges */
    questionId: number;
    questionText: string;
    comment?: string | null;
    correctedOnSite: boolean;
    correctiveActions: CorrectiveActionDto[];
}

export interface FindingPhotoDto {
    id: number;
    auditId: number;
    questionId: number;
    fileName: string;
    fileSizeBytes: number;
    uploadedAt: string;
    uploadedBy: string;
    caption?: string | null;
    /** Convenience URL populated server-side — proxy through Axios for auth */
    downloadUrl: string;
}

export interface RepeatFindingDto {
    questionId: number;
    questionTextSnapshot: string;
    sectionNameSnapshot?: string | null;
    occurrenceCount: number;
}

export interface DivisionScoreTargetDto {
    divisionId: number;
    divisionCode: string;
    divisionName: string;
    /** 0–100, null if not set */
    scoreTarget?: number | null;
    /** Days between expected audits, null if no schedule set */
    auditFrequencyDays?: number | null;
    /** When true, closing a CA in this division requires at least one closure photo */
    requireClosurePhoto?: boolean;
    /** Days until a Normal-priority CA is due. Null = system default (14). */
    slaNormalDays?: number | null;
    /** Days until an Urgent-priority CA is due. Null = system default (7). */
    slaUrgentDays?: number | null;
    /** Days open before escalation. Null = no escalation. */
    slaEscalateAfterDays?: number | null;
    /** Email to notify on escalation. Null = no escalation email. */
    escalationEmail?: string | null;
}

export interface CorrectiveActionPhotoDto {
    id: number;
    correctiveActionId: number;
    fileName: string;
    fileSizeBytes: number;
    uploadedBy: string;
    uploadedAt: string;
    caption?: string | null;
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
    requirePhotoOnNc: boolean;
    autoCreateCa: boolean;
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
    requirePhotoOnNc: boolean;
    autoCreateCa: boolean;
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
    auditTrackingNumber?: string | null;
    description: string;
    rootCause?: string | null;
    status: string;
    /** "Manual" | "AutoGenerated" */
    source: string;
    assignedTo?: string | null;
    assignedToUserId?: number | null;
    /** "Normal" | "Urgent" */
    priority: string;
    dueDate?: string | null;
    completedDate?: string | null;
    isOverdue: boolean;
    daysOpen: number;
    questionText: string;
    sectionName: string;
    createdAt: string;
    /** True when the CA's division requires at least one closure photo before closing */
    requireClosurePhoto: boolean;
    /** Number of closure photos already attached to this CA */
    closurePhotoCount: number;
}

export interface UpdateCorrectiveActionRequest {
    description?: string | null;
    assignedTo?: string | null;
    assignedToUserId?: number | null;
    /** "Normal" | "Urgent" */
    priority?: string | null;
    /** ISO "YYYY-MM-DD". Send "" to clear. */
    dueDate?: string | null;
    rootCause?: string | null;
}

export interface BulkUpdateCorrectiveActionsRequest {
    correctiveActionIds: number[];
    /** "status" | "reassign" */
    action: string;
    /** For action="status": "InProgress" | "Closed" | "Voided" */
    newStatus?: string | null;
    /** Required when newStatus="Closed" */
    closureNotes?: string | null;
    /** For action="reassign" */
    newAssignee?: string | null;
    newAssigneeUserId?: number | null;
}

export interface BulkUpdateCorrectiveActionsResult {
    successCount: number;
    failedIds: number[];
    errors: string[];
}

export interface GetCorrectiveActionsParams {
    divisionId?: number | null;
    status?: string | null;
    searchText?: string | null;
    assignedTo?: string | null;
    source?: string | null;
    priority?: string | null;
    overdueOnly?: boolean;
    pageNumber?: number;
    pageSize?: number;
}

export interface PagedCorrectiveActionsResult {
    items: CorrectiveActionListItemDto[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    openCount: number;
    inProgressCount: number;
    overdueCount: number;
    closedCount: number;
}

export interface SubmitAuditResult {
    recipients: string[];
    subject: string;
    reviewUrl: string;
    score: string;
    ncCount: number;
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
    rootCause?: string | null;
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

export interface QuestionHistoryItemDto {
    auditId: number;
    auditDate?: string | null;
    auditor?: string | null;
    /** "Conforming" | "NonConforming" | "Warning" | "NA" */
    status?: string | null;
    comment?: string | null;
    jobNumber?: string | null;
}

export interface PriorAuditPrefillDto {
    hasPrior: boolean;
    /** ISO date "YYYY-MM-DD" of the prior audit, or null */
    auditDate?: string | null;
    /** Map of questionId → "Conforming" */
    responses: Record<number, string>;
}

export interface ComplianceStatusDto {
    divisionId: number;
    divisionCode: string;
    divisionName: string;
    /** ISO date string "YYYY-MM-DD" or null if no audit exists */
    lastAuditDate: string | null;
    daysSinceLastAudit: number | null;
    frequencyDays: number | null;
    daysUntilDue: number | null;
    /** "OnTrack" | "DueSoon" | "Overdue" | "NoSchedule" | "NeverAudited" */
    status: 'OnTrack' | 'DueSoon' | 'Overdue' | 'NoSchedule' | 'NeverAudited';
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
    trackingNumber?: string | null;
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
    /** Average score of the last 10 submitted audits in this division. Null if insufficient data. */
    divisionAvgScore?: number | null;
    /** Division compliance target (0–100), null if not set. */
    divisionScoreTarget?: number | null;
    /** questionIds for questions NonConforming in 2+ audits within 180 days. */
    repeatFindingQuestionIds: number[];
    nonConformingItems: AuditFindingDto[];
    warningItems: ReviewResponseItemDto[];
    sections: ReviewSectionDto[];
    reviewEmailRouting: EmailRoutingDto[];
    reviewSummary?: string | null;
    reviewedAt?: string | null;
    reviewedBy?: string | null;
    distributionRecipients: DistributionRecipientDto[];
    attachments: AuditAttachmentRefDto[];
}

export interface DistributionRecipientDto {
    id: number;
    emailAddress: string;
    name?: string | null;
}

export interface AuditAttachmentRefDto {
    id: number;
    fileName: string;
    fileSizeBytes: number;
    hasFile: boolean;
}

export interface AddDistributionRecipientRequest {
    email: string;
    name?: string | null;
}

export interface SendDistributionEmailRequest {
    attachmentIds: number[];
    subjectOverride?: string | null;
}

export interface DistributionPreviewDto {
    subject: string;
    recipients: string[];
    bodyHtml: string;
    findingsSummary: string | null;
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

    createAudit(
        divisionId: number,
        enabledOptionalGroupKeys: string[] = [],
        jobPrefixId?: number | null,
        siteCode?: string | null,
        cancelToken?: CancelToken,
    ): Promise<number> {
        return this.instance
            .post<number>(`${this.baseUrl}/v1/audits`, { divisionId, enabledOptionalGroupKeys, jobPrefixId, siteCode }, { cancelToken })
            .then(r => r.data);
    }

    getDivisionJobPrefixes(divisionId: number, cancelToken?: CancelToken): Promise<DivisionJobPrefixDto[]> {
        return this.instance
            .get<DivisionJobPrefixDto[]>(`${this.baseUrl}/v1/divisions/${divisionId}/job-prefixes`, { cancelToken })
            .then(r => r.data);
    }

    saveDivisionJobPrefixes(divisionId: number, prefixes: DivisionJobPrefixUpsertDto[], cancelToken?: CancelToken): Promise<void> {
        const request: SaveDivisionJobPrefixesRequest = { prefixes };
        return this.instance
            .put(`${this.baseUrl}/v1/admin/divisions/${divisionId}/job-prefixes`, request, { cancelToken })
            .then(() => undefined);
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

    submitAudit(id: number, cancelToken?: CancelToken): Promise<SubmitAuditResult> {
        return this.instance
            .post<SubmitAuditResult>(`${this.baseUrl}/v1/audits/${id}/submit`, null, { cancelToken })
            .then(r => r.data);
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

    getComplianceStatus(cancelToken?: CancelToken): Promise<ComplianceStatusDto[]> {
        return this.instance
            .get<ComplianceStatusDto[]>(`${this.baseUrl}/v1/reports/compliance-status`, { cancelToken })
            .then(r => r.data);
    }

    getAuditReview(id: number, cancelToken?: CancelToken): Promise<AuditReviewDto> {
        return this.instance
            .get<AuditReviewDto>(`${this.baseUrl}/v1/audits/${id}/review`, { cancelToken })
            .then(r => r.data);
    }

    saveReviewSummary(auditId: number, summary: string | null, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/audits/${auditId}/review-summary`, { summary }, { cancelToken })
            .then(() => undefined);
    }

    addDistributionRecipient(auditId: number, body: AddDistributionRecipientRequest, cancelToken?: CancelToken): Promise<DistributionRecipientDto> {
        return this.instance
            .post<DistributionRecipientDto>(`${this.baseUrl}/v1/audits/${auditId}/distribution-recipients`, body, { cancelToken })
            .then(r => r.data);
    }

    removeDistributionRecipient(auditId: number, recipientId: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/audits/${auditId}/distribution-recipients/${recipientId}`, { cancelToken })
            .then(() => undefined);
    }

    getDistributionPreview(auditId: number, attachmentIds: number[], cancelToken?: CancelToken): Promise<DistributionPreviewDto> {
        const qs = attachmentIds.length ? `?attachmentIds=${attachmentIds.join(',')}` : '';
        return this.instance
            .get<DistributionPreviewDto>(`${this.baseUrl}/v1/audits/${auditId}/distribution-preview${qs}`, { cancelToken })
            .then(r => r.data);
    }

    sendDistributionEmail(auditId: number, body: SendDistributionEmailRequest, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .post(`${this.baseUrl}/v1/audits/${auditId}/send-distribution`, body, { cancelToken })
            .then(() => undefined);
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
        filters?: GetCorrectiveActionsParams | null,
        cancelToken?: CancelToken,
    ): Promise<PagedCorrectiveActionsResult> {
        const params: Record<string, unknown> = {};
        if (filters?.divisionId != null) params['divisionId'] = filters.divisionId;
        if (filters?.status)             params['status']      = filters.status;
        if (filters?.searchText)         params['searchText']  = filters.searchText;
        if (filters?.assignedTo)         params['assignedTo']  = filters.assignedTo;
        if (filters?.source)             params['source']      = filters.source;
        if (filters?.priority)           params['priority']    = filters.priority;
        if (filters?.overdueOnly)        params['overdueOnly'] = true;
        if (filters?.pageNumber != null) params['pageNumber']  = filters.pageNumber;
        if (filters?.pageSize != null)   params['pageSize']    = filters.pageSize;
        return this.instance
            .get<PagedCorrectiveActionsResult>(`${this.baseUrl}/v1/audits/corrective-actions`, { params, cancelToken })
            .then(r => r.data);
    }

    updateCorrectiveAction(id: number, request: UpdateCorrectiveActionRequest, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/audits/corrective-actions/${id}`, request, { cancelToken })
            .then(() => undefined);
    }

    bulkUpdateCorrectiveActions(
        request: BulkUpdateCorrectiveActionsRequest,
        cancelToken?: CancelToken,
    ): Promise<BulkUpdateCorrectiveActionsResult> {
        return this.instance
            .post<BulkUpdateCorrectiveActionsResult>(`${this.baseUrl}/v1/audits/corrective-actions/bulk`, request, { cancelToken })
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

    // ── Finding Photos ────────────────────────────────────────────────────────

    getFindingPhotos(auditId: number, questionId: number, cancelToken?: CancelToken): Promise<FindingPhotoDto[]> {
        return this.instance
            .get<FindingPhotoDto[]>(`${this.baseUrl}/v1/audits/${auditId}/questions/${questionId}/photos`, { cancelToken })
            .then(r => r.data);
    }

    uploadFindingPhoto(
        auditId: number,
        questionId: number,
        file: File,
        caption?: string | null,
        cancelToken?: CancelToken,
    ): Promise<FindingPhotoDto> {
        const form = new FormData();
        form.append('file', file, file.name);
        if (caption) form.append('caption', caption);
        return this.instance
            .post<FindingPhotoDto>(`${this.baseUrl}/v1/audits/${auditId}/questions/${questionId}/photos`, form, {
                headers: { 'Content-Type': 'multipart/form-data' },
                cancelToken,
            })
            .then(r => r.data);
    }

    downloadFindingPhoto(auditId: number, questionId: number, photoId: number, cancelToken?: CancelToken): Promise<Blob> {
        return this.instance
            .get(`${this.baseUrl}/v1/audits/${auditId}/questions/${questionId}/photos/${photoId}/download`, {
                responseType: 'blob',
                cancelToken,
            })
            .then(r => r.data as Blob);
    }

    deleteFindingPhoto(auditId: number, questionId: number, photoId: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/audits/${auditId}/questions/${questionId}/photos/${photoId}`, { cancelToken })
            .then(() => undefined);
    }

    // ── Question History ──────────────────────────────────────────────────────

    getQuestionHistory(questionId: number, divisionId: number, limit = 3, cancelToken?: CancelToken): Promise<QuestionHistoryItemDto[]> {
        return this.instance
            .get<QuestionHistoryItemDto[]>(`${this.baseUrl}/v1/audits/question-history`, {
                params: { questionId, divisionId, limit },
                cancelToken,
            })
            .then(r => r.data);
    }

    // ── Prior Audit Prefill ───────────────────────────────────────────────────

    getPriorAuditPrefill(divisionId: number, templateVersionId: number, cancelToken?: CancelToken): Promise<PriorAuditPrefillDto> {
        return this.instance
            .get<PriorAuditPrefillDto>(`${this.baseUrl}/v1/audits/prior-prefill`, {
                params: { divisionId, templateVersionId },
                cancelToken,
            })
            .then(r => r.data);
    }

    // ── Repeat Findings ───────────────────────────────────────────────────────

    getRepeatFindings(auditId: number, cancelToken?: CancelToken): Promise<RepeatFindingDto[]> {
        return this.instance
            .get<RepeatFindingDto[]>(`${this.baseUrl}/v1/audits/${auditId}/repeat-findings`, { cancelToken })
            .then(r => r.data);
    }

    // ── Division Score Targets ────────────────────────────────────────────────

    getDivisionScoreTargets(cancelToken?: CancelToken): Promise<DivisionScoreTargetDto[]> {
        return this.instance
            .get<DivisionScoreTargetDto[]>(`${this.baseUrl}/v1/admin/divisions/score-targets`, { cancelToken })
            .then(r => r.data);
    }

    setDivisionScoreTarget(divisionId: number, scoreTarget: number | null, cancelToken?: CancelToken): Promise<DivisionScoreTargetDto> {
        return this.instance
            .put<DivisionScoreTargetDto>(`${this.baseUrl}/v1/admin/divisions/${divisionId}/score-target`, { scoreTarget }, { cancelToken })
            .then(r => r.data);
    }

    setDivisionAuditFrequency(divisionId: number, auditFrequencyDays: number | null, cancelToken?: CancelToken): Promise<DivisionScoreTargetDto> {
        return this.instance
            .put<DivisionScoreTargetDto>(`${this.baseUrl}/v1/admin/divisions/${divisionId}/audit-frequency`, { auditFrequencyDays }, { cancelToken })
            .then(r => r.data);
    }

    setDivisionRequireClosurePhoto(divisionId: number, requireClosurePhoto: boolean, cancelToken?: CancelToken): Promise<DivisionScoreTargetDto> {
        return this.instance
            .put<DivisionScoreTargetDto>(`${this.baseUrl}/v1/audits/divisions/${divisionId}/require-closure-photo`, { requireClosurePhoto }, { cancelToken })
            .then(r => r.data);
    }

    setDivisionSla(
        divisionId: number,
        sla: { slaNormalDays: number | null; slaUrgentDays: number | null; slaEscalateAfterDays: number | null; escalationEmail: string | null },
        cancelToken?: CancelToken,
    ): Promise<DivisionScoreTargetDto> {
        return this.instance
            .put<DivisionScoreTargetDto>(`${this.baseUrl}/v1/admin/divisions/${divisionId}/sla`, sla, { cancelToken })
            .then(r => r.data);
    }

    uploadCaClosurePhoto(
        correctiveActionId: number,
        file: File,
        caption?: string | null,
        cancelToken?: CancelToken,
    ): Promise<CorrectiveActionPhotoDto> {
        const form = new FormData();
        form.append('file', file, file.name);
        if (caption) form.append('caption', caption);
        return this.instance
            .post<CorrectiveActionPhotoDto>(
                `${this.baseUrl}/v1/audits/corrective-actions/${correctiveActionId}/photos`,
                form,
                { headers: { 'Content-Type': 'multipart/form-data' }, cancelToken },
            )
            .then(r => r.data);
    }

    getAuditLogs(params: {
        dateFrom?: string | null;
        dateTo?: string | null;
        userEmail?: string | null;
        entityType?: string | null;
        action?: string | null;
        search?: string | null;
        page?: number;
        pageSize?: number;
    }, cancelToken?: CancelToken): Promise<AuditLogsResult> {
        return this.instance
            .get<AuditLogsResult>(`${this.baseUrl}/v1/admin/audit-logs`, { params, cancelToken })
            .then(r => r.data);
    }

    // ── Scheduled Reports ─────────────────────────────────────────────────────

    getScheduledReports(cancelToken?: CancelToken): Promise<ScheduledReportDto[]> {
        return this.instance
            .get<ScheduledReportDto[]>(`${this.baseUrl}/v1/reports/scheduled`, { cancelToken })
            .then(r => r.data);
    }

    saveScheduledReport(payload: ScheduledReportPayload, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .put(`${this.baseUrl}/v1/reports/scheduled`, payload, { cancelToken })
            .then(() => undefined);
    }

    deleteScheduledReport(id: number, cancelToken?: CancelToken): Promise<void> {
        return this.instance
            .delete(`${this.baseUrl}/v1/reports/scheduled/${id}`, { cancelToken })
            .then(() => undefined);
    }

    // ── Audits by Employee ────────────────────────────────────────────────────

    getAuditsByEmployee(
        params?: { divisionId?: string; dateFrom?: string; dateTo?: string },
        cancelToken?: CancelToken,
    ): Promise<EmployeeAuditRowDto[]> {
        return this.instance
            .get<EmployeeAuditRowDto[]>(`${this.baseUrl}/v1/audits/by-employee`, { params, cancelToken })
            .then(r => r.data);
    }

    // ── Blob helpers (Excel exports, PDF generation) ──────────────────────────

    downloadBlob(
        endpoint: string,
        params?: Record<string, unknown>,
        cancelToken?: CancelToken,
    ): Promise<Blob> {
        return this.instance
            .get(`${this.baseUrl}${endpoint}`, { params, responseType: 'blob', cancelToken })
            .then(r => r.data as Blob);
    }

    postBlob(
        endpoint: string,
        data: unknown,
        cancelToken?: CancelToken,
    ): Promise<Blob> {
        return this.instance
            .post(`${this.baseUrl}${endpoint}`, data, { responseType: 'blob', cancelToken })
            .then(r => r.data as Blob);
    }
}

// ── Scheduled Reports ─────────────────────────────────────────────────────────

export interface ScheduledReportDto {
    id: number;
    templateId: string;
    title: string;
    divisionId: number | null;
    frequency: string;
    timeUtc: string;
    dateRangePreset: string;
    recipients: string[];
    isActive: boolean;
}

export interface ScheduledReportPayload {
    id?: number | null;
    templateId: string;
    title: string;
    divisionId: number | null;
    frequency: string;
    timeUtc: string;
    dateRangePreset: string;
    recipients: string[];
    isActive: boolean;
}

// ── Audits by Employee ────────────────────────────────────────────────────────

export interface EmployeeAuditRowDto {
    auditor: string;
    auditCount: number;
    avgScorePercent: number | null;
    totalNonConforming: number;
    totalWarnings: number;
    lastAuditDate: string | null;
    lastDivisionCode: string | null;
}

export interface AuditLogsResult {
    actionLogs: AuditActionLogDto[];
    trailLogs:  AuditTrailLogDto[];
    totalActionLogs: number;
    totalTrailLogs:  number;
}

export interface AuditActionLogDto {
    id:          number;
    timestamp:   string;
    performedBy: string;
    action:      string;
    entityType:  string;
    entityId:    string | null;
    description: string;
    severity:    'Info' | 'Warning' | 'Error';
    ipAddress:   string | null;
}

export interface AuditTrailLogDto {
    id:             number;
    timestamp:      string;
    userEmail:      string;
    action:         'Insert' | 'Update' | 'Delete';
    entityType:     string;
    entityId:       string;
    oldValues:      string | null;
    newValues:      string | null;
    changedColumns: string | null;
    ipAddress:      string | null;
}

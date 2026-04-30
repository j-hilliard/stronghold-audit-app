// ─────────────────────────────────────────────────────────────────────────────
// Structured Report System — PR6
//
// Replaces the freeform block canvas with controlled, template-driven reports.
// Admin edits content (summary text, highlights, notes) — NOT layout.
// ─────────────────────────────────────────────────────────────────────────────

export type ReportType =
    | 'QuarterlySummary'
    | 'Newsletter'
    | 'AuditSummary'
    | 'ExecutiveDashboard';

export type SectionType =
    | 'cover'
    | 'kpis'
    | 'trend'
    | 'category-breakdown'
    | 'findings-examples'
    | 'ca-table'
    | 'summary-text'
    | 'highlights';

export const EDITABLE_SECTIONS: SectionType[] = ['summary-text', 'highlights', 'findings-examples'];

// ── Per-section state (persisted in draft) ────────────────────────────────────

export interface StructuredSection {
    type: SectionType;
    enabled: boolean;
    /** editable: plain text narrative summary */
    editedText?: string;
    /** editable: newline-separated highlight bullets */
    editedHighlights?: string;
    /** editable: newline-separated findings examples (per category, JSON-encoded map) */
    editedNotes?: string;
}

// ── The draft payload stored in ReportDraft.blocksJson ────────────────────────

export interface StructuredReport {
    /** Version marker — distinguishes from legacy ReportBlock[] format (schemaVersion: 1 is old) */
    schemaVersion: 2;
    templateType: ReportType;
    divisionId: number;
    divisionCode: string;
    period: string;
    dateFrom: string | null;
    dateTo: string | null;
    sections: StructuredSection[];
}

// ── Template definitions ──────────────────────────────────────────────────────

export interface ReportTemplateDefinition {
    type: ReportType;
    label: string;
    description: string;
    icon: string;
    /** Always included and cannot be disabled */
    requiredSections: SectionType[];
    /** Shown but off by default; user can enable */
    optionalSections: SectionType[];
}

export const REPORT_TEMPLATES: ReportTemplateDefinition[] = [
    {
        type: 'QuarterlySummary',
        label: 'Quarterly Summary',
        description: 'Comprehensive quarterly performance report with KPIs, trends, and corrective action status.',
        icon: 'pi pi-chart-bar',
        requiredSections: ['cover', 'kpis', 'ca-table'],
        optionalSections: ['trend', 'category-breakdown', 'summary-text', 'highlights'],
    },
    {
        type: 'Newsletter',
        label: 'Newsletter',
        description: 'Division compliance newsletter with findings by category and key highlights.',
        icon: 'pi pi-envelope',
        requiredSections: ['cover', 'highlights', 'findings-examples'],
        optionalSections: ['kpis', 'summary-text', 'ca-table'],
    },
    {
        type: 'AuditSummary',
        label: 'Audit Summary',
        description: 'Single-period summary with key findings, corrective actions, and executive notes.',
        icon: 'pi pi-file-check',
        requiredSections: ['cover', 'kpis', 'summary-text'],
        optionalSections: ['category-breakdown', 'findings-examples', 'ca-table'],
    },
    {
        type: 'ExecutiveDashboard',
        label: 'Executive Dashboard',
        description: 'High-level executive overview with company-wide KPIs and division comparison.',
        icon: 'pi pi-briefcase',
        requiredSections: ['cover', 'kpis', 'trend', 'highlights'],
        optionalSections: ['summary-text', 'category-breakdown'],
    },
];

// ── Display metadata ──────────────────────────────────────────────────────────

export const SECTION_LABELS: Record<SectionType, string> = {
    'cover':               'Cover Page',
    'kpis':                'Key Performance Indicators',
    'trend':               'Conformance Trend',
    'category-breakdown':  'Category Breakdown',
    'findings-examples':   'Findings Examples',
    'ca-table':            'Corrective Actions',
    'summary-text':        'Summary',
    'highlights':          'Highlights',
};

export const SECTION_ICONS: Record<SectionType, string> = {
    'cover':              'pi pi-file',
    'kpis':               'pi pi-chart-bar',
    'trend':              'pi pi-chart-line',
    'category-breakdown': 'pi pi-sitemap',
    'findings-examples':  'pi pi-list',
    'ca-table':           'pi pi-exclamation-triangle',
    'summary-text':       'pi pi-align-left',
    'highlights':         'pi pi-star',
};

// ── Factory helpers ───────────────────────────────────────────────────────────

export function buildDefaultSections(template: ReportTemplateDefinition): StructuredSection[] {
    const all: SectionType[] = [...template.requiredSections, ...template.optionalSections];
    const seen = new Set<SectionType>();
    return all
        .filter(s => { if (seen.has(s)) return false; seen.add(s); return true; })
        .map(type => ({
            type,
            enabled: template.requiredSections.includes(type),
        }));
}

export function buildDefaultReport(
    template: ReportTemplateDefinition,
    divisionId: number,
    divisionCode: string,
    period: string,
    dateFrom: string | null,
    dateTo: string | null,
): StructuredReport {
    return {
        schemaVersion: 2,
        templateType: template.type,
        divisionId,
        divisionCode,
        period,
        dateFrom,
        dateTo,
        sections: buildDefaultSections(template),
    };
}

export function isStructuredReport(blocksJson: string): boolean {
    try { return (JSON.parse(blocksJson) as Record<string, unknown>)?.schemaVersion === 2; }
    catch { return false; }
}

export function getTemplateDefinition(type: ReportType): ReportTemplateDefinition {
    return REPORT_TEMPLATES.find(t => t.type === type) ?? REPORT_TEMPLATES[0];
}

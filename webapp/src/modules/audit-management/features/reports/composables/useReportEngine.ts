/**
 * useReportEngine — data fetch + block generation + field-level merge.
 *
 * Responsibilities:
 *   1. Fetch AuditReportDto and SectionTrendsReportDto from the API.
 *   2. Build a fresh ReportBlock[] from that data.
 *   3. Merge fresh blocks over existing blocks, preserving:
 *        - block.style (always)
 *        - caption fields (always)
 *        - authored text (when isEdited === true)
 *        - cover.primaryColor, cover.backgroundImageUrl (always)
 *        - chart titles (always, after initial creation)
 *        - narrative.aiPromptContext (when isEdited === true)
 *
 * This composable does NOT touch BlocksJson serialization — that is
 * exclusively the responsibility of useReportDraft.ts.
 */
import { ref } from 'vue';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient } from '@/apiclient/auditClient';
import type { AuditReportDto, SectionTrendsReportDto } from '@/apiclient/auditClient';
import type {
    ReportBlock, BlockStyle,
    CoverBlock, HeadingBlock, KpiGridBlock,
    BarChartBlock, LineChartBlock, NarrativeBlock,
    CalloutBlock, CaTableBlock,
    KpiCard, CaTableRow,
} from '../types/report-block';

const COLORS = {
    division: 'rgba(59, 130, 246, 0.75)',    // blue-500
    company: 'rgba(148, 163, 184, 0.6)',      // slate-400
    divisionLine: '#3b82f6',
    companyLine: '#94a3b8',
};

const DEFAULT_STYLE: BlockStyle = {};

// ── Helpers ───────────────────────────────────────────────────────────────────

function uuid(): string {
    return crypto.randomUUID();
}

function findExisting(blocks: ReportBlock[], id: string): ReportBlock | undefined {
    return blocks.find(b => b.id === id);
}

function kpiVariant(value: number, goodThreshold: number, warnThreshold: number): KpiCard['variant'] {
    if (value >= goodThreshold) return 'good';
    if (value >= warnThreshold) return 'warn';
    return 'bad';
}

// ── Block builders (produce fresh blocks from API data) ───────────────────────

function buildCover(
    divisionCode: string,
    divisionName: string,
    period: string,
    preparedBy: string,
    existingBlock?: ReportBlock,
): CoverBlock {
    const existing = existingBlock?.type === 'cover' ? existingBlock : undefined;
    return {
        id: existing?.id ?? uuid(),
        type: 'cover',
        isEdited: false,
        style: existing?.style ?? DEFAULT_STYLE,
        content: {
            // always regenerated
            divisionName,
            divisionCode,
            period,
            preparedBy,
            // always preserved after first creation
            primaryColor: existing?.content.primaryColor ?? '#1e3a5f',
            backgroundImageUrl: existing?.content.backgroundImageUrl ?? '',
        },
    };
}

function buildKpiGrid(
    report: AuditReportDto,
    companyReport: AuditReportDto,
    trends: SectionTrendsReportDto,
    existingBlock?: ReportBlock,
): KpiGridBlock {
    const existing = existingBlock?.type === 'kpi-grid' ? existingBlock : undefined;
    const existingCards = existing?.content.cards ?? [];

    // Preserve enabled state by matching on label (summary) or sectionName (section cards)
    function wasEnabled(label: string, sectionName?: string): boolean {
        if (!existing) return true;
        const prev = sectionName
            ? existingCards.find(c => c.sectionName === sectionName)
            : existingCards.find(c => c.label === label && !c.sectionName);
        return prev?.enabled ?? true;
    }

    const avg = report.avgScorePercent;
    const coAvg = companyReport.avgScorePercent;

    const summaryCards: KpiCard[] = [
        {
            label: 'Total Audits',
            value: report.totalAudits,
            companyValue: companyReport.totalAudits,
            variant: report.totalAudits > 0 ? 'good' : 'neutral',
            enabled: wasEnabled('Total Audits'),
        },
        {
            label: 'Avg Conformance',
            value: avg != null ? `${avg.toFixed(1)}%` : '—',
            companyValue: coAvg != null ? `${coAvg.toFixed(1)}%` : '—',
            variant: avg == null ? 'neutral' : kpiVariant(avg, 90, 75),
            enabled: wasEnabled('Avg Conformance'),
        },
        {
            label: 'Non-Conformances',
            value: report.totalNonConforming,
            companyValue: companyReport.totalNonConforming,
            variant: report.totalNonConforming === 0 ? 'good' : report.totalNonConforming < 10 ? 'warn' : 'bad',
            enabled: wasEnabled('Non-Conformances'),
        },
        {
            label: 'Warnings',
            value: report.totalWarnings,
            companyValue: companyReport.totalWarnings,
            variant: report.totalWarnings === 0 ? 'good' : 'warn',
            enabled: wasEnabled('Warnings'),
        },
        {
            label: 'Corrected On Site',
            value: report.correctedOnSiteCount,
            companyValue: companyReport.correctedOnSiteCount,
            variant: 'neutral',
            enabled: wasEnabled('Corrected On Site'),
        },
        {
            label: 'Open CAs',
            value: report.openCorrectiveActions.length,
            companyValue: companyReport.openCorrectiveActions.length,
            variant: report.openCorrectiveActions.length === 0 ? 'good'
                : report.openCorrectiveActions.some(ca => ca.isOverdue) ? 'bad' : 'warn',
            enabled: wasEnabled('Open CAs'),
        },
    ];

    // Per-section cards: findings-per-audit rate, division vs company.
    // Use ALL sections from trends history so zero-current-period sections still appear.
    const divTotal = Math.max(report.totalAudits, 1);
    const coTotal = Math.max(companyReport.totalAudits, 1);
    const allSectionNames = trends.sections.length
        ? trends.sections.map(s => s.sectionName)
        : report.sectionBreakdown.map(s => s.sectionName);
    const sectionCards: KpiCard[] = allSectionNames.map(sn => {
        const divSection = report.sectionBreakdown.find(s => s.sectionName === sn);
        const coSection = companyReport.sectionBreakdown.find(s => s.sectionName === sn);
        const rate = (divSection?.ncCount ?? 0) / divTotal;
        const coRate = (coSection?.ncCount ?? 0) / coTotal;
        return {
            label: sn,
            value: rate.toFixed(2),
            companyValue: coRate.toFixed(2),
            unit: 'NC/audit',
            variant: rate === 0 ? 'good' : rate < 0.5 ? 'warn' : 'bad',
            sectionName: sn,
            enabled: wasEnabled(sn, sn),
        };
    });

    return {
        id: existing?.id ?? uuid(),
        type: 'kpi-grid',
        isEdited: false,
        style: existing?.style ?? DEFAULT_STYLE,
        content: {
            cards: [...summaryCards, ...sectionCards],
            showComparison: existing?.content.showComparison ?? true,
        },
    };
}

function buildBarChart(
    report: AuditReportDto,
    companyReport: AuditReportDto,
    trends: SectionTrendsReportDto,
    existingBlock?: ReportBlock,
): BarChartBlock {
    const existing = existingBlock?.type === 'chart-bar' ? existingBlock : undefined;

    // Use all sections from trends (authoritative — includes zero-finding sections)
    const allSections = trends.sections.length
        ? trends.sections.map(s => s.sectionName)
        : report.sectionBreakdown.map(s => s.sectionName);

    const divData = allSections.map(sn =>
        report.sectionBreakdown.find(s => s.sectionName === sn)?.ncCount ?? 0,
    );
    const coData = allSections.map(sn =>
        companyReport.sectionBreakdown.find(s => s.sectionName === sn)?.ncCount ?? 0,
    );

    return {
        id: existing?.id ?? uuid(),
        type: 'chart-bar',
        isEdited: false,
        style: existing?.style ?? DEFAULT_STYLE,
        content: {
            title: existing?.content.title ?? 'Non-Conformances by Section — Division vs Company',
            labels: allSections,
            datasets: [
                { label: 'Division', data: divData, backgroundColor: COLORS.division },
                { label: 'Company', data: coData, backgroundColor: COLORS.company },
            ],
            caption: existing?.content.caption ?? '',
        },
    };
}

function buildLineChart(
    sectionName: string,
    trends: SectionTrendsReportDto,
    divisionCode: string,
    existingBlock?: ReportBlock,
): LineChartBlock {
    const existing = existingBlock?.type === 'chart-line' ? existingBlock : undefined;
    const section = trends.sections.find(s => s.sectionName === sectionName);

    const labels = trends.quarters;
    const divData = labels.map(q => section?.divisionTrend.find(p => p.quarter === q)?.findingsPerAudit ?? 0);
    const coData = labels.map(q => section?.companyTrend.find(p => p.quarter === q)?.findingsPerAudit ?? 0);

    return {
        id: existing?.id ?? uuid(),
        type: 'chart-line',
        isEdited: false,
        style: existing?.style ?? DEFAULT_STYLE,
        content: {
            sectionName,
            title: existing?.content.title ?? `${sectionName} — Findings per Audit`,
            labels,
            datasets: [
                { label: divisionCode, data: divData, borderColor: COLORS.divisionLine },
                { label: 'Company', data: coData, borderColor: COLORS.companyLine, borderDash: [5, 3] },
            ],
            caption: existing?.content.caption ?? '',
            // Always preserved — user fills in real finding examples for this section
            examples: existing?.type === 'chart-line' ? (existing.content.examples ?? '') : '',
        },
    };
}

function buildNarrative(existingBlock?: ReportBlock): NarrativeBlock {
    const existing = existingBlock?.type === 'narrative' ? existingBlock : undefined;
    return {
        id: existing?.id ?? uuid(),
        type: 'narrative',
        isEdited: existing?.isEdited ?? false,
        style: existing?.style ?? DEFAULT_STYLE,
        content: {
            text: existing?.isEdited ? (existing.content.text ?? '') : '',
            aiPromptContext: existing?.isEdited ? existing.content.aiPromptContext : undefined,
        },
    };
}

function buildCaTable(report: AuditReportDto, existingBlock?: ReportBlock): CaTableBlock {
    const existing = existingBlock?.type === 'ca-table' ? existingBlock : undefined;
    const rows: CaTableRow[] = report.openCorrectiveActions.map(ca => ({
        id: ca.id,
        auditId: ca.auditId,
        description: ca.description,
        assignedTo: ca.assignedTo ?? '',
        dueDate: ca.dueDate ?? '',
        status: ca.status,
        isOverdue: ca.isOverdue,
        daysOpen: ca.daysOpen,
    }));

    return {
        id: existing?.id ?? uuid(),
        type: 'ca-table',
        isEdited: false,
        style: existing?.style ?? DEFAULT_STYLE,
        content: { rows },
    };
}

// ── Default block layout (used when creating a new draft) ─────────────────────

function buildDefaultLayout(
    report: AuditReportDto,
    companyReport: AuditReportDto,
    trends: SectionTrendsReportDto,
    divisionCode: string,
    divisionName: string,
    period: string,
    preparedBy: string,
): ReportBlock[] {
    const blocks: ReportBlock[] = [];

    blocks.push(buildCover(divisionCode, divisionName, period, preparedBy));
    blocks.push({
        id: uuid(), type: 'heading', isEdited: false, style: DEFAULT_STYLE,
        content: { text: 'Performance at a Glance', level: 2 },
    } as HeadingBlock);
    blocks.push(buildKpiGrid(report, companyReport, trends));
    blocks.push(buildBarChart(report, companyReport, trends));

    // One line chart per section — ALL sections, matching the legacy newsletter format
    for (const section of trends.sections) {
        blocks.push(buildLineChart(section.sectionName, trends, divisionCode));
    }

    blocks.push({
        id: uuid(), type: 'heading', isEdited: false, style: DEFAULT_STYLE,
        content: { text: 'Narrative Summary', level: 2 },
    } as HeadingBlock);
    blocks.push(buildNarrative());

    if (report.openCorrectiveActions.length > 0) {
        blocks.push({
            id: uuid(), type: 'heading', isEdited: false, style: DEFAULT_STYLE,
            content: { text: 'Open Corrective Actions', level: 2 },
        } as HeadingBlock);
        blocks.push(buildCaTable(report));
    }

    return blocks;
}

// ── Merge engine — regenerates data, preserves annotation/style, syncs charts ──
//
// Line chart blocks are driven by which section KPI cards are enabled.
// Enabled sections with no existing chart-line block get one appended.
// Existing chart-line blocks for disabled sections are removed.

function mergeBlocks(
    existing: ReportBlock[],
    report: AuditReportDto,
    companyReport: AuditReportDto,
    trends: SectionTrendsReportDto,
    divisionCode: string,
    divisionName: string,
    period: string,
    preparedBy: string,
): ReportBlock[] {
    // Determine which sections are enabled via KPI grid cards
    const kpiBlock = existing.find(b => b.type === 'kpi-grid') as KpiGridBlock | undefined;
    const enabledSections = new Set<string>();
    if (kpiBlock) {
        kpiBlock.content.cards
            .filter(c => c.enabled && c.sectionName)
            .forEach(c => enabledSections.add(c.sectionName!));
    } else {
        // No KPI block yet: default to sections with findings
        report.sectionBreakdown.filter(s => s.ncCount > 0).forEach(s => enabledSections.add(s.sectionName));
    }

    const handledSections = new Set<string>();
    const result: ReportBlock[] = [];

    for (const block of existing) {
        switch (block.type) {
            case 'cover':
                result.push(buildCover(divisionCode, divisionName, period, preparedBy, block));
                break;
            case 'kpi-grid':
                result.push(buildKpiGrid(report, companyReport, trends, block));
                break;
            case 'chart-bar':
                result.push(buildBarChart(report, companyReport, trends, block));
                break;
            case 'chart-line': {
                const section = block.content.sectionName;
                if (enabledSections.has(section)) {
                    result.push(buildLineChart(section, trends, divisionCode, block));
                    handledSections.add(section);
                }
                // else: section card is disabled — block is removed
                break;
            }
            case 'ca-table':
                result.push(buildCaTable(report, block));
                break;
            default:
                // heading, narrative, callout: preserve as-is
                result.push(block);
        }
    }

    // Add chart-line blocks for newly enabled sections that have no existing block
    for (const section of enabledSections) {
        if (!handledSections.has(section)) {
            result.push(buildLineChart(section, trends, divisionCode));
        }
    }

    return result;
}

// ── Composable ────────────────────────────────────────────────────────────────

export function useReportEngine() {
    const apiStore = useApiStore();
    const loading = ref(false);
    const error = ref<string | null>(null);
    /** Available section names — populated after generateBlocks runs. Used by the toolbar picker. */
    const sections = ref<string[]>([]);

    async function generateBlocks(params: {
        divisionId: number;
        divisionCode: string;
        divisionName: string;
        period: string;
        dateFrom: string | null;
        dateTo: string | null;
        preparedBy: string;
        existingBlocks: ReportBlock[];
    }): Promise<ReportBlock[]> {
        loading.value = true;
        error.value = null;
        try {
            const client = new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);

            const [report, companyReport, trends] = await Promise.all([
                client.getAuditReport(params.divisionId, null, params.dateFrom, params.dateTo),
                // Company-wide report: no divisionId filter — gives comparison baseline
                client.getAuditReport(null, null, params.dateFrom, params.dateTo),
                // Full history for trend lines (no date filter)
                client.getSectionTrends(params.divisionId, null, null),
            ]);

            // Populate section picker list
            sections.value = trends.sections.map(s => s.sectionName);

            if (params.existingBlocks.length === 0) {
                return buildDefaultLayout(
                    report, companyReport, trends,
                    params.divisionCode, params.divisionName,
                    params.period, params.preparedBy,
                );
            }

            return mergeBlocks(
                params.existingBlocks, report, companyReport, trends,
                params.divisionCode, params.divisionName,
                params.period, params.preparedBy,
            );
        } catch (e: unknown) {
            error.value = e instanceof Error ? e.message : 'Failed to load report data.';
            return params.existingBlocks;
        } finally {
            loading.value = false;
        }
    }

    /**
     * Create a single new block of the given type.
     * For chart-bar and chart-line, sectionName controls which section is shown.
     * chart-bar with sectionName=undefined → all-sections overview.
     * chart-bar with sectionName set → two-bar chart for that section only.
     * chart-line requires a sectionName.
     */
    async function buildSingleBlock(
        type: ReportBlock['type'],
        divisionId: number,
        divisionCode: string,
        period: string,
        preparedBy: string,
        sectionName?: string,
    ): Promise<ReportBlock> {
        const client = new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);

        switch (type) {
            case 'cover':
                return buildCover(divisionCode, '', period, preparedBy);
            case 'heading':
                return { id: uuid(), type: 'heading', isEdited: false, style: DEFAULT_STYLE, content: { text: 'New Section', level: 2 } } as HeadingBlock;
            case 'kpi-grid': {
                const [report, companyReport, trends] = await Promise.all([
                    client.getAuditReport(divisionId),
                    client.getAuditReport(null),
                    client.getSectionTrends(divisionId, null, null),
                ]);
                return buildKpiGrid(report, companyReport, trends);
            }
            case 'chart-bar': {
                const [report, companyReport, trends] = await Promise.all([
                    client.getAuditReport(divisionId),
                    client.getAuditReport(null),
                    client.getSectionTrends(divisionId, null, null),
                ]);
                if (sectionName) {
                    // Single-section bar: Division vs Company for this section only
                    const divCount = report.sectionBreakdown.find(s => s.sectionName === sectionName)?.ncCount ?? 0;
                    const coCount = companyReport.sectionBreakdown.find(s => s.sectionName === sectionName)?.ncCount ?? 0;
                    return {
                        id: uuid(), type: 'chart-bar', isEdited: false, style: DEFAULT_STYLE,
                        content: {
                            title: `${sectionName} — Division vs Company`,
                            labels: ['Division', 'Company'],
                            datasets: [
                                { label: 'Division', data: [divCount], backgroundColor: COLORS.division },
                                { label: 'Company',  data: [coCount],  backgroundColor: COLORS.company },
                            ],
                            caption: '',
                        },
                    } as BarChartBlock;
                }
                return buildBarChart(report, companyReport, trends);
            }
            case 'chart-line': {
                const trends = await client.getSectionTrends(divisionId, null, null);
                // Update available sections list whenever we fetch trends
                sections.value = trends.sections.map(s => s.sectionName);
                const name = sectionName ?? trends.sections[0]?.sectionName ?? 'Section';
                return buildLineChart(name, trends, divisionCode);
            }
            case 'narrative':
                return buildNarrative();
            case 'callout':
                return { id: uuid(), type: 'callout', isEdited: false, style: DEFAULT_STYLE, content: { title: 'Note', body: '', variant: 'info' } } as CalloutBlock;
            case 'ca-table': {
                const report = await client.getAuditReport(divisionId);
                return buildCaTable(report);
            }
            case 'image':
                return { id: uuid(), type: 'image', isEdited: false, style: DEFAULT_STYLE, content: { url: '', alt: '', caption: '', width: 'full' } } as import('../types/report-block').ImageBlock;
            case 'column-row':
                return { id: uuid(), type: 'column-row', isEdited: false, style: DEFAULT_STYLE, content: { ratio: '50/50', gap: 'md', leftBlocks: [], rightBlocks: [] } } as import('../types/report-block').ColumnRowBlock;
            case 'divider':
                return { id: uuid(), type: 'divider', isEdited: false, style: DEFAULT_STYLE, content: { thickness: 1, variant: 'solid', color: '#475569', marginY: 'md' } } as import('../types/report-block').DividerBlock;
            case 'spacer':
                return { id: uuid(), type: 'spacer', isEdited: false, style: DEFAULT_STYLE, content: { height: 'md' } } as import('../types/report-block').SpacerBlock;
            case 'toc-sidebar':
                return { id: uuid(), type: 'toc-sidebar', isEdited: false, style: DEFAULT_STYLE, content: { title: 'INSIDE', items: [], darkBackground: true } } as import('../types/report-block').TocSidebarBlock;
            case 'oval-callout':
                return { id: uuid(), type: 'oval-callout', isEdited: false, style: DEFAULT_STYLE, content: { title: 'strong-hold', phonetic: "/'strôNG.hōld/ noun.", body: 'A place where a particular cause or belief is strongly defended or upheld.', backgroundColor: '#1e3a5f', textColor: '#ffffff' } } as import('../types/report-block').OvalCalloutBlock;
            case 'findings-category':
                return {
                    id: uuid(), type: 'findings-category', isEdited: false, style: DEFAULT_STYLE,
                    content: {
                        sectionName: sectionName ?? 'Section Name',
                        findings: '<ul><li>Example finding 1</li><li>Example finding 2</li></ul>',
                        showExamplesLabel: true,
                        accentColor: '#862633',
                    },
                } as import('../types/report-block').FindingsCategoryBlock;
        }
    }

    return { loading, error, sections, generateBlocks, buildSingleBlock };
}

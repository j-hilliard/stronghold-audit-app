/**
 * useReportBuilder — fetches audit data and builds computed section data.
 *
 * This replaces useReportEngine's block-generation logic with structured
 * section data suitable for the new template-driven report system.
 *
 * Does NOT handle draft persistence — that stays in useReportDraft / the composer.
 */

import { ref, computed } from 'vue';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import type {
    AuditReportDto,
    SectionTrendsReportDto,
    DivisionDto,
} from '@/apiclient/auditClient';

// ── Output types ──────────────────────────────────────────────────────────────

export interface ReportKpi {
    label: string;
    value: string;
    companyValue?: string;
    unit?: string;
    variant: 'good' | 'warn' | 'bad' | 'neutral';
}

export interface ReportCategoryRow {
    sectionName: string;
    ncCount: number;
    /** NC events per audit for the period */
    rate: number;
    variant: 'good' | 'warn' | 'bad';
}

export interface ReportTrendPoint {
    quarter: string;
    divisionNcRate: number;
    companyNcRate: number | null;
}

export interface ReportCaRow {
    id: number;
    description: string;
    assignedTo: string;
    dueDate: string;
    status: string;
    isOverdue: boolean;
    daysOpen: number;
}

export interface BuiltReportData {
    divisionCode: string;
    period: string;
    kpis: ReportKpi[];
    categories: ReportCategoryRow[];
    trends: ReportTrendPoint[];
    caRows: ReportCaRow[];
    totalAudits: number;
    avgScorePercent: number | null;
    /** True when build() has been called at least once */
    hasData: boolean;
}

// ── Composable ────────────────────────────────────────────────────────────────

export function useReportBuilder() {
    const service = useAuditService();

    const loading = ref(false);
    const error   = ref<string | null>(null);

    const divReport     = ref<AuditReportDto | null>(null);
    const coReport      = ref<AuditReportDto | null>(null);
    const sectionTrends = ref<SectionTrendsReportDto | null>(null);
    const divisionInfo  = ref<DivisionDto | null>(null);

    // ── Build ─────────────────────────────────────────────────────────────────

    async function build(params: {
        divisionId: number;
        division: DivisionDto;
        dateFrom: string | null;
        dateTo: string | null;
    }) {
        loading.value = true;
        error.value   = null;
        divisionInfo.value = params.division;
        try {
            const [div, co, trends] = await Promise.all([
                service.getAuditReport(
                    params.divisionId,
                    undefined,
                    params.dateFrom ?? undefined,
                    params.dateTo ?? undefined,
                ),
                service.getAuditReport(
                    undefined,
                    undefined,
                    params.dateFrom ?? undefined,
                    params.dateTo ?? undefined,
                ),
                service.getSectionTrends(
                    params.divisionId,
                    params.dateFrom ?? undefined,
                    params.dateTo ?? undefined,
                ),
            ]);
            divReport.value     = div;
            coReport.value      = co;
            sectionTrends.value = trends;
        } catch (e: unknown) {
            error.value = (e as Error)?.message ?? 'Failed to load report data.';
        } finally {
            loading.value = false;
        }
    }

    // ── KPIs ──────────────────────────────────────────────────────────────────

    const kpis = computed<ReportKpi[]>(() => {
        const div = divReport.value;
        const co  = coReport.value;
        if (!div) return [];

        const score   = div.avgScorePercent != null ? Math.round(div.avgScorePercent) : null;
        const coScore = co?.avgScorePercent != null ? Math.round(co.avgScorePercent) : null;
        const openCAs = div.openCorrectiveActions.length;
        const overdue = div.openCorrectiveActions.filter(ca => ca.isOverdue).length;
        const cosPct  = div.totalNonConforming > 0
            ? Math.round((div.correctedOnSiteCount / div.totalNonConforming) * 100)
            : 0;

        return [
            {
                label:        'Total Audits',
                value:        String(div.totalAudits),
                companyValue: co ? String(co.totalAudits) : undefined,
                variant:      'neutral',
            },
            {
                label:        'Avg Conformance',
                value:        score != null ? `${score}%` : '—',
                companyValue: coScore != null ? `${coScore}%` : undefined,
                variant:      score == null ? 'neutral'
                            : score >= 85  ? 'good'
                            : score >= 70  ? 'warn'
                            :                'bad',
            },
            {
                label:        'Non-Conformances',
                value:        String(div.totalNonConforming),
                companyValue: co ? String(co.totalNonConforming) : undefined,
                variant:      div.totalNonConforming === 0 ? 'good'
                            : div.totalNonConforming <= 5  ? 'warn'
                            :                                'bad',
            },
            {
                label:        'Warnings',
                value:        String(div.totalWarnings),
                companyValue: co ? String(co.totalWarnings) : undefined,
                variant:      div.totalWarnings === 0 ? 'good' : 'warn',
            },
            {
                label:        'Corrected On Site',
                value:        `${cosPct}%`,
                unit:         'of NCs',
                variant:      cosPct >= 80 ? 'good' : cosPct >= 50 ? 'warn' : 'neutral',
            },
            {
                label:        'Open CAs',
                value:        String(openCAs),
                variant:      openCAs === 0 ? 'good' : overdue > 0 ? 'bad' : 'warn',
            },
        ];
    });

    // ── Category breakdown ────────────────────────────────────────────────────

    const categories = computed<ReportCategoryRow[]>(() => {
        const div = divReport.value;
        if (!div || div.totalAudits === 0) return [];
        return div.sectionBreakdown
            .map(s => {
                const rate = s.ncCount / div.totalAudits;
                return {
                    sectionName: s.sectionName,
                    ncCount:     s.ncCount,
                    rate,
                    variant: rate < 0.1 ? 'good' : rate < 0.3 ? 'warn' : 'bad',
                } as ReportCategoryRow;
            })
            .filter(s => s.ncCount > 0)
            .sort((a, b) => b.ncCount - a.ncCount);
    });

    // ── Trend (quarterly NC rate) ─────────────────────────────────────────────

    const trends = computed<ReportTrendPoint[]>(() => {
        const trends = sectionTrends.value;
        if (!trends || !trends.quarters.length) return [];

        return trends.quarters.map((quarter, qi) => {
            const divTotalNc = trends.sections.reduce((sum, sec) => {
                return sum + (sec.divisionTrend[qi]?.ncCount ?? 0);
            }, 0);
            const divTotalAudits = trends.sections.length > 0
                ? (trends.sections[0].divisionTrend[qi]?.auditCount ?? 0)
                : 0;
            const coTotalNc = trends.sections.reduce((sum, sec) => {
                return sum + (sec.companyTrend[qi]?.ncCount ?? 0);
            }, 0);
            const coTotalAudits = trends.sections.length > 0
                ? (trends.sections[0].companyTrend[qi]?.auditCount ?? 0)
                : 0;

            return {
                quarter,
                divisionNcRate: divTotalAudits > 0 ? divTotalNc / divTotalAudits : 0,
                companyNcRate:  coTotalAudits > 0  ? coTotalNc  / coTotalAudits  : null,
            };
        });
    });

    // ── Open CAs ──────────────────────────────────────────────────────────────

    const caRows = computed<ReportCaRow[]>(() => {
        return (divReport.value?.openCorrectiveActions ?? []).map(ca => ({
            id:          ca.id,
            description: ca.description,
            assignedTo:  ca.assignedTo ?? '—',
            dueDate:     ca.dueDate    ?? '—',
            status:      ca.status,
            isOverdue:   ca.isOverdue,
            daysOpen:    ca.daysOpen,
        }));
    });

    // ── Top-level summary stats ───────────────────────────────────────────────

    const totalAudits      = computed(() => divReport.value?.totalAudits ?? 0);
    const avgScorePercent  = computed(() => divReport.value?.avgScorePercent ?? null);
    const hasData          = computed(() => divReport.value !== null);
    const sectionNames     = computed(() =>
        (divReport.value?.sectionBreakdown ?? []).map(s => s.sectionName),
    );

    return {
        loading,
        error,
        kpis,
        categories,
        trends,
        caRows,
        totalAudits,
        avgScorePercent,
        hasData,
        sectionNames,
        build,
    };
}

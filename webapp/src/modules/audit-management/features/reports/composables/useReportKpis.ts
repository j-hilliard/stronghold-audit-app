import { computed, reactive, ref, watch } from 'vue';
import type { Ref } from 'vue';
import type { AuditReportDto } from '@/apiclient/auditClient';

function useCountUp(source: () => number, duration = 900) {
    const display = ref(0);
    watch(source, (to) => {
        if (!Number.isFinite(to)) { display.value = 0; return; }
        const from  = display.value;
        const start = performance.now();
        function tick(now: number) {
            const t    = Math.min((now - start) / duration, 1);
            const ease = 1 - Math.pow(1 - t, 3);
            display.value = Math.round(from + (to - from) * ease);
            if (t < 1) requestAnimationFrame(tick);
        }
        requestAnimationFrame(tick);
    }, { immediate: true });
    return display;
}

export const SECTION_SHORT: Record<string, string> = {
    'Personal Protective Equipment':          'PPE',
    'Equipment & Equipment Inspection':       'Equipment',
    'Job Site & Confined Space Condition':    'Job Site / CSE',
    'Sign-In / Sign-Out Rosters - Toolbox Safety': 'Sign-In / Toolbox',
    'Lock-Out / Tag-Out':                     'LOTO',
    'Culture / Attitudes':                    'Culture',
    'Training / Dispatch':                    'Training',
    'Daily Job Logs':                         'Daily Logs',
    'QA / QC Documentation':                 'QA / QC',
};

export function useReportKpis(report: Ref<AuditReportDto | null>) {
    // ── Count-up animated displays ────────────────────────────────────────────
    const displayTotalAudits   = useCountUp(() => report.value?.totalAudits      ?? 0);
    const displayNonConforming = useCountUp(() => report.value?.totalNonConforming ?? 0);
    const displayWarnings      = useCountUp(() => report.value?.totalWarnings     ?? 0);
    const displayAvgScore      = useCountUp(() => report.value?.avgScorePercent   ?? 0);

    // ── KPI visibility ────────────────────────────────────────────────────────
    const KPI_CARDS = [
        { key: 'kpiTotalAudits',     label: 'Total Audits'         },
        { key: 'kpiAvgConformance',  label: 'Avg Conformance'      },
        { key: 'kpiNonConformances', label: 'Non-Conformances'     },
        { key: 'kpiWarnings',        label: 'Warnings'             },
        { key: 'kpiCorrectedOnSite', label: 'Corrected On Site'    },
        { key: 'kpiCaAging',         label: 'CAs Past 14-Day Rule' },
    ];

    const hidden = reactive<Record<string, boolean>>(
        (() => { try { return JSON.parse(localStorage.getItem('dashboard-hidden') ?? '{}'); } catch { return {}; } })()
    );
    watch(hidden, val => localStorage.setItem('dashboard-hidden', JSON.stringify(val)), { deep: true });

    function hideSection(key: string)  { hidden[key] = true;                      }
    function toggleCard(key: string)   { hidden[key] = !hidden[key];              }
    function showAll()                 { KPI_CARDS.forEach(c => delete hidden[c.key]); }
    function hideAll()                 { KPI_CARDS.forEach(c => { hidden[c.key] = true; }); }

    const customizePanelRef = ref<any>(null);
    function toggleCustomize(event: Event) { customizePanelRef.value?.toggle(event); }

    // ── Trend deltas ──────────────────────────────────────────────────────────
    const trendDeltas = computed(() => {
        const rows = report.value?.rows ?? [];
        if (rows.length < 2) return { scoreDelta: null, ncDelta: null, warnDelta: null, auditCountDelta: null };
        const dated = rows.filter(r => r.auditDate).sort((a, b) => (a.auditDate ?? '').localeCompare(b.auditDate ?? ''));
        if (dated.length < 4) return { scoreDelta: null, ncDelta: null, warnDelta: null, auditCountDelta: null };
        const mid     = Math.floor(dated.length / 2);
        const prior   = dated.slice(0, mid);
        const current = dated.slice(mid);
        function avg(arr: typeof dated) {
            const scored = arr.filter(r => r.scorePercent != null);
            return scored.length ? scored.reduce((s, r) => s + r.scorePercent!, 0) / scored.length : null;
        }
        const curScore   = avg(current);
        const prvScore   = avg(prior);
        const scoreDelta = curScore != null && prvScore != null ? Math.round((curScore - prvScore) * 10) / 10 : null;
        const ncDelta    = current.reduce((s, r) => s + r.nonConformingCount, 0) - prior.reduce((s, r) => s + r.nonConformingCount, 0);
        const warnDelta  = current.reduce((s, r) => s + r.warningCount, 0)        - prior.reduce((s, r) => s + r.warningCount, 0);
        const auditCountDelta = current.length - prior.length;
        return { scoreDelta, ncDelta, warnDelta, auditCountDelta };
    });

    const correctedOnSitePct = computed(() => {
        if (!report.value || report.value.totalNonConforming === 0) return 0;
        return Math.round(report.value.correctedOnSiteCount / report.value.totalNonConforming * 100);
    });

    const caAgingStats = computed(() => {
        const cas = report.value?.openCorrectiveActions ?? [];
        if (!cas.length) return { overdueCount: 0, avgDaysOpen: 0 };
        const overdueCount = cas.filter(ca => ca.daysOpen > 14).length;
        const avgDaysOpen  = Math.round(cas.reduce((s, ca) => s + ca.daysOpen, 0) / cas.length);
        return { overdueCount, avgDaysOpen };
    });

    const displayCorrectedPct = useCountUp(() => correctedOnSitePct.value);
    const displayCaAging      = useCountUp(() => caAgingStats.value.overdueCount);

    // ── Score / status formatting ─────────────────────────────────────────────
    const scoreColor = computed(() => {
        const pct = report.value?.avgScorePercent;
        if (pct == null) return 'text-slate-400';
        if (pct >= 90)   return 'text-emerald-400';
        if (pct >= 75)   return 'text-amber-400';
        return 'text-red-400';
    });

    const scoreVariant = computed<'success' | 'warning' | 'danger' | 'default'>(() => {
        const pct = report.value?.avgScorePercent;
        if (pct == null) return 'default';
        if (pct >= 90)   return 'success';
        if (pct >= 75)   return 'warning';
        return 'danger';
    });

    function rowScoreColor(pct: number | null | undefined): string {
        if (pct == null) return 'text-slate-400';
        if (pct >= 90)   return 'text-emerald-400 font-semibold';
        if (pct >= 75)   return 'text-amber-400 font-semibold';
        return 'text-red-400 font-semibold';
    }

    function statusSeverity(status: string): string {
        const map: Record<string, string> = {
            Draft: 'warning', Submitted: 'info', Reopened: 'warning',
            UnderReview: 'contrast', Approved: 'success', Distributed: 'secondary', Closed: 'secondary',
        };
        return map[status] ?? 'secondary';
    }

    function auditStatusSeverity(status: string): string {
        const map: Record<string, string> = {
            Draft: 'warning', Submitted: 'info', Reopened: 'warning',
            UnderReview: 'contrast', Approved: 'success', Distributed: 'secondary', Closed: 'secondary',
        };
        return map[status] ?? 'secondary';
    }

    function sectionRateColor(rate: number): string {
        if (rate === 0)  return 'text-emerald-400';
        if (rate < 0.2)  return 'text-yellow-400';
        if (rate < 0.5)  return 'text-amber-400';
        return 'text-red-400';
    }

    function sectionRateBorder(rate: number, isActive = false): string {
        if (isActive)   return '';
        if (rate === 0) return 'border-slate-700';
        if (rate < 0.2) return 'border-yellow-900/50';
        if (rate < 0.5) return 'border-amber-900/50';
        return 'border-red-900/50';
    }

    // ── Section KPI cards ─────────────────────────────────────────────────────
    const sectionKpiCards = computed(() => {
        if (!report.value || report.value.totalAudits === 0) return [];
        const total = report.value.totalAudits;
        return report.value.sectionBreakdown
            .map(s => ({
                name:      s.sectionName,
                shortName: SECTION_SHORT[s.sectionName] ?? s.sectionName,
                ncCount:   s.ncCount,
                rate:      Math.round(s.ncCount / total * 100) / 100,
            }))
            .sort((a, b) => b.rate - a.rate);
    });

    return {
        displayTotalAudits, displayNonConforming, displayWarnings, displayAvgScore,
        displayCorrectedPct, displayCaAging,
        KPI_CARDS, hidden, hideSection, toggleCard, showAll, hideAll,
        customizePanelRef, toggleCustomize,
        trendDeltas, correctedOnSitePct, caAgingStats,
        scoreColor, scoreVariant, rowScoreColor, statusSeverity, auditStatusSeverity,
        sectionRateColor, sectionRateBorder,
        SECTION_SHORT, sectionKpiCards,
    };
}

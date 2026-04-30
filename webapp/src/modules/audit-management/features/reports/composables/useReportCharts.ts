import { computed, ref, watch } from 'vue';
import type { Ref } from 'vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import type { AuditReportDto } from '@/apiclient/auditClient';

type ChartKind = 'bar' | 'line';

interface ChartOptions {
    report:          Ref<AuditReportDto | null>;
    filterDivisionId: Ref<number | null>;
    filterSection:   Ref<string | null>;
    filterDateFrom:  Ref<Date | null>;
    filterDateTo:    Ref<Date | null>;
}

export function useReportCharts({
    report, filterDivisionId, filterSection, filterDateFrom, filterDateTo,
}: ChartOptions) {
    const store = useAuditStore();

    // ── Chart type toggles (persisted) ────────────────────────────────────────
    const chartTypes = ref<{ conformance: ChartKind; quarterly: ChartKind; ncSection: ChartKind }>(
        (() => { try { return JSON.parse(localStorage.getItem('report-chart-types') ?? '') as any; } catch { return null; } })()
        ?? { conformance: 'bar', quarterly: 'line', ncSection: 'bar' }
    );
    watch(chartTypes, val => localStorage.setItem('report-chart-types', JSON.stringify(val)), { deep: true });

    // ── Location chart ────────────────────────────────────────────────────────
    const locationStats = computed(() => {
        const map = new Map<string, number>();
        for (const row of report.value?.rows ?? []) {
            const loc = row.location?.trim();
            if (!loc) continue;
            map.set(loc, (map.get(loc) ?? 0) + row.nonConformingCount);
        }
        return Array.from(map.entries())
            .map(([location, ncs]) => ({ location, ncs }))
            .filter(x => x.ncs > 0)
            .sort((a, b) => b.ncs - a.ncs)
            .slice(0, 10);
    });

    const locationChartData = computed(() => ({
        labels:   locationStats.value.map(l => l.location),
        datasets: [{ label: 'Non-Conformances', data: locationStats.value.map(l => l.ncs), backgroundColor: 'rgba(239,68,68,0.6)', borderColor: '#ef4444', borderWidth: 1, borderRadius: 4 }],
    }));

    const locationChartOptions = computed(() => ({
        indexAxis: 'y' as const, responsive: true, maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw} NCs` } } },
        scales: {
            x: { beginAtZero: true, ticks: { color: '#94a3b8', stepSize: 1, precision: 0 }, grid: { color: 'rgba(100,116,139,0.2)' } },
            y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
        },
        onClick: (_event: unknown, elements: { index: number }[]) => {
            if (elements.length > 0) {
                const loc = locationStats.value[elements[0].index]?.location;
                if (loc) locationDrillCallback.value?.(loc);
            }
        },
    }));

    // Callback so the view/drilldowns composable can hook into chart click
    const locationDrillCallback = ref<((loc: string) => void) | null>(null);

    // ── Conformance trend chart ───────────────────────────────────────────────
    const primaryLabel = computed(() =>
        filterDivisionId.value
            ? (store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'Selected')
            : 'All Divisions'
    );

    function formatWeek(isoWeek: string): string {
        const [yearStr, wStr] = isoWeek.split('-W');
        const year = +yearStr, w = +wStr;
        const jan4 = new Date(year, 0, 4);
        const mondayW1 = new Date(jan4);
        mondayW1.setDate(jan4.getDate() - ((jan4.getDay() + 6) % 7));
        const mondayOfWeek = new Date(mondayW1);
        mondayOfWeek.setDate(mondayW1.getDate() + (w - 1) * 7);
        const month = mondayOfWeek.toLocaleString('en-US', { month: 'short' });
        return `${month} W${w} '${String(year).slice(-2)}`;
    }

    const trendChartTitle = computed(() => {
        const parts: string[] = [];
        if (filterDivisionId.value) parts.push(store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? '');
        if (filterSection.value)    parts.push(filterSection.value);
        const ctx    = parts.length ? `${parts.join(' · ')} — ` : '';
        const period = (filterDateFrom.value || filterDateTo.value) ? 'Filtered Period' : 'Last 12 Weeks';
        return `${ctx}Conformance Trend (${period})`;
    });

    const chartData = computed(() => {
        const primary = report.value?.trend ?? [];
        const isLine  = chartTypes.value.conformance === 'line';
        function scoreBarColors(t: typeof primary, alpha: boolean) {
            return t.map(p => p.avgScore == null ? `rgba(100,116,139,${alpha ? '0.5' : '1'})`
                : p.avgScore >= 90 ? `rgba(16,185,129,${alpha ? '0.7' : '1'})`
                : p.avgScore >= 75 ? `rgba(245,158,11,${alpha ? '0.7' : '1'})`
                : `rgba(239,68,68,${alpha ? '0.7' : '1'})`);
        }
        return {
            labels:   primary.map(p => formatWeek(p.week)),
            datasets: [{
                label:           primaryLabel.value,
                data:            primary.map(p => p.avgScore),
                backgroundColor: isLine ? 'rgba(99,102,241,0.15)' : scoreBarColors(primary, true),
                borderColor:     isLine ? '#6366f1' : scoreBarColors(primary, false),
                borderWidth:     isLine ? 2 : 1, borderRadius: isLine ? 0 : 4,
                pointRadius:     isLine ? 4 : 0, tension: isLine ? 0.3 : 0, fill: isLine,
            }],
        };
    });

    const chartOptions = computed(() => ({
        responsive: true, maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number | null }) => ctx.raw != null ? `${ctx.raw}%` : 'No data' } } },
        scales: {
            y: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
            x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
        },
    }));

    // ── Division chart ────────────────────────────────────────────────────────
    const divisionStats = computed(() => {
        const map = new Map<string, { scores: number[]; ncs: number; count: number }>();
        for (const row of report.value?.rows ?? []) {
            const key = row.divisionCode;
            if (!map.has(key)) map.set(key, { scores: [], ncs: 0, count: 0 });
            const entry = map.get(key)!;
            if (row.scorePercent != null) entry.scores.push(row.scorePercent);
            entry.ncs += row.nonConformingCount;
            entry.count++;
        }
        return Array.from(map.entries()).map(([division, s]) => ({
            division, auditCount: s.count,
            avgScore: s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
            totalNcs: s.ncs,
        })).filter(r => r.avgScore != null).sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
    });

    const divisionChartData = computed(() => ({
        labels:   divisionStats.value.map(d => `${d.division} (${d.auditCount})`),
        datasets: [{
            label:           'Avg Score %',
            data:            divisionStats.value.map(d => d.avgScore),
            backgroundColor: divisionStats.value.map(d => (d.avgScore ?? 0) >= 90 ? 'rgba(16,185,129,0.7)' : (d.avgScore ?? 0) >= 75 ? 'rgba(245,158,11,0.7)' : 'rgba(239,68,68,0.7)'),
            borderColor:     divisionStats.value.map(d => (d.avgScore ?? 0) >= 90 ? '#10b981' : (d.avgScore ?? 0) >= 75 ? '#f59e0b' : '#ef4444'),
            borderWidth: 1, borderRadius: 4,
        }],
    }));

    const divisionChartOptions = {
        indexAxis: 'y' as const, responsive: true, maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw}%` } } },
        scales: {
            x: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
            y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
        },
    };

    // ── Quarterly trend chart ─────────────────────────────────────────────────
    const quarterlyTrendData = computed(() => {
        const byQuarter = new Map<string, number[]>();
        for (const row of report.value?.rows ?? []) {
            if (!row.auditDate || row.scorePercent == null) continue;
            const d = new Date(row.auditDate);
            const q = Math.ceil((d.getMonth() + 1) / 3);
            const key = `${d.getFullYear()} Q${q}`;
            if (!byQuarter.has(key)) byQuarter.set(key, []);
            byQuarter.get(key)!.push(row.scorePercent);
        }
        return Array.from(byQuarter.entries()).sort(([a], [b]) => a.localeCompare(b)).map(([quarter, scores]) => ({
            quarter, avgScore: Math.round(scores.reduce((a, b) => a + b, 0) / scores.length * 10) / 10, count: scores.length,
        }));
    });

    const quarterlyChartData = computed(() => {
        const primary = quarterlyTrendData.value;
        const isLine  = chartTypes.value.quarterly === 'line';
        return {
            labels:   primary.map(p => p.quarter),
            datasets: [{
                label:                primaryLabel.value,
                data:                 primary.map(p => p.avgScore),
                borderColor:          '#6366f1',
                backgroundColor:      isLine ? 'rgba(99,102,241,0.15)' : primary.map(p =>
                    p.avgScore >= 90 ? 'rgba(16,185,129,0.7)' : p.avgScore >= 75 ? 'rgba(245,158,11,0.7)' : 'rgba(239,68,68,0.7)'),
                borderWidth:          2, pointRadius: isLine ? 5 : 0,
                pointBackgroundColor: primary.map(p => p.avgScore >= 90 ? '#10b981' : p.avgScore >= 75 ? '#f59e0b' : '#ef4444'),
                tension: 0.3, fill: isLine, borderRadius: isLine ? 0 : 4,
            }],
        };
    });

    const quarterlyChartOptions = computed(() => ({
        responsive: true, maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw}%` } } },
        scales: {
            y: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
            x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
        },
    }));

    // ── Auditor performance ───────────────────────────────────────────────────
    const auditorStats = computed(() => {
        const map = new Map<string, { scores: number[]; ncs: number; warnings: number; count: number }>();
        for (const row of report.value?.rows ?? []) {
            const key = row.auditor?.trim() || 'Unknown';
            if (!map.has(key)) map.set(key, { scores: [], ncs: 0, warnings: 0, count: 0 });
            const entry = map.get(key)!;
            entry.count++;
            if (row.scorePercent != null) entry.scores.push(row.scorePercent);
            entry.ncs      += row.nonConformingCount;
            entry.warnings += row.warningCount;
        }
        return Array.from(map.entries()).map(([auditor, s]) => ({
            auditor, auditCount: s.count,
            avgScore:     s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
            totalNcs:     s.ncs,
            totalWarnings: s.warnings,
        })).filter(r => r.auditCount > 0).sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
    });

    // ── NC by section chart ───────────────────────────────────────────────────
    const ncCategoryChartData = computed(() => {
        const primary = report.value?.sectionBreakdown ?? [] as { sectionName: string; ncCount: number }[];
        return {
            labels:   primary.map(x => x.sectionName),
            datasets: [{
                label: 'Non-Conformances', data: primary.map(x => x.ncCount),
                backgroundColor: 'rgba(239,68,68,0.7)', borderColor: '#ef4444',
                borderWidth: 1, borderRadius: chartTypes.value.ncSection === 'line' ? 0 : 4,
            }],
        };
    });

    const ncCategoryChartOptions = computed(() => ({
        indexAxis: (chartTypes.value.ncSection === 'line' ? 'x' : 'y') as 'x' | 'y',
        responsive: true, maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw} NCs` } } },
        scales: {
            x: { beginAtZero: true, ticks: { color: '#94a3b8', stepSize: 1, precision: 0 }, grid: { color: 'rgba(100,116,139,0.2)' } },
            y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
        },
    }));

    return {
        chartTypes,
        locationStats, locationChartData, locationChartOptions, locationDrillCallback,
        primaryLabel, trendChartTitle, chartData, chartOptions,
        divisionStats, divisionChartData, divisionChartOptions,
        quarterlyTrendData, quarterlyChartData, quarterlyChartOptions,
        auditorStats,
        ncCategoryChartData, ncCategoryChartOptions,
    };
}

<template>
    <div class="newsletter-page">
        <div class="toolbar no-print">
            <div class="toolbar-title">
                <h1>Compliance Newsletter</h1>
                <p>Quarterly division summary with trend lines, findings, and corrective actions.</p>
            </div>
            <div class="toolbar-fields">
                <label>
                    Division
                    <select v-model="selectedDivisionId" @change="loadData">
                        <option value="">All Divisions</option>
                        <option v-for="d in divisions" :key="d.id" :value="d.id">
                            {{ d.code }} - {{ d.name }}
                        </option>
                    </select>
                </label>
                <label>
                    Year
                    <select v-model="selectedYear" @change="onQuarterChange">
                        <option v-for="y in yearOptions" :key="y" :value="y">{{ y }}</option>
                    </select>
                </label>
                <label>
                    Quarter
                    <select v-model="selectedQuarter" @change="onQuarterChange">
                        <option :value="1">Q1</option>
                        <option :value="2">Q2</option>
                        <option :value="3">Q3</option>
                        <option :value="4">Q4</option>
                    </select>
                </label>
                <span class="toolbar-sep">or</span>
                <label>
                    From
                    <input type="date" v-model="dateFrom" @change="loadData" class="toolbar-date" />
                </label>
                <label>
                    To
                    <input type="date" v-model="dateTo" @change="loadData" class="toolbar-date" />
                </label>
                <button class="btn secondary" @click="router.push('/audit-management/reports')">Back to Reports</button>
                <button class="btn secondary" @click="router.push('/audit-management/newsletter/template-editor')">
                    <i class="pi pi-palette" style="margin-right:4px;font-size:11px;" />Template Settings
                </button>
                <button class="btn" @click="generateNarrative">Generate with AI (Draft)</button>
                <button class="btn" @click="printPage">Print / PDF</button>
            </div>
        </div>

        <div v-if="loading" class="loading">Loading newsletter data...</div>

        <template v-else-if="report">
            <section class="cover">
                <div class="cover-overlay">
                    <div class="cover-subtitle">Stronghold Compliance Newsletter</div>
                    <h1>{{ divisionLabel }}</h1>
                    <p>{{ periodLabel }} | Generated {{ todayLabel }}</p>
                </div>
            </section>

            <section class="kpi-grid">
                <article class="kpi">
                    <h3>Total Audits</h3>
                    <div class="kpi-value">{{ report.totalAudits }}</div>
                </article>
                <article class="kpi">
                    <h3>Avg Conformance</h3>
                    <div class="kpi-value">{{ report.avgScorePercent != null ? `${report.avgScorePercent}%` : '-' }}</div>
                </article>
                <article class="kpi">
                    <h3>Non-Conformances</h3>
                    <div class="kpi-value">{{ report.totalNonConforming }}</div>
                </article>
                <article class="kpi">
                    <h3>Warnings</h3>
                    <div class="kpi-value">{{ report.totalWarnings }}</div>
                </article>
                <article class="kpi">
                    <h3>Corrected On Site</h3>
                    <div class="kpi-value">{{ correctedOnSitePct }}%</div>
                </article>
                <article class="kpi">
                    <h3>Open Corrective Actions</h3>
                    <div class="kpi-value">{{ report.openCorrectiveActions.length }}</div>
                </article>
            </section>

            <section class="panel">
                <header class="panel-head">
                    <h2>Findings Overview</h2>
                    <p>Findings per audit for {{ periodLabel }} (division vs company).</p>
                </header>
                <Chart type="bar" :data="findingsOverviewData" :options="findingsOverviewOptions" style="height: 280px;" />
            </section>

            <section class="panel" v-if="trendSections.length > 0">
                <header class="panel-head">
                    <h2>Per-Section Trend Lines</h2>
                    <p>Quarterly findings-per-audit trend over time.</p>
                </header>
                <div class="trend-grid">
                    <article v-for="section in trendSections" :key="section.sectionName" class="trend-card">
                        <h3>{{ section.sectionName }}</h3>
                        <Chart type="line" :data="section.chartData" :options="sectionTrendOptions" style="height: 220px;" />
                    </article>
                </div>
            </section>

            <section class="panel">
                <header class="panel-head">
                    <h2>Auditor Performance</h2>
                </header>
                <table class="table">
                    <thead>
                        <tr>
                            <th>Auditor</th>
                            <th>Audits</th>
                            <th>Avg Score</th>
                            <th>NCs</th>
                            <th>Warnings</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="row in auditorRows" :key="row.auditor">
                            <td>{{ row.auditor }}</td>
                            <td>{{ row.count }}</td>
                            <td>{{ row.avgScore != null ? `${row.avgScore}%` : '-' }}</td>
                            <td>{{ row.ncs }}</td>
                            <td>{{ row.warnings }}</td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <section class="panel">
                <header class="panel-head">
                    <h2>Corrective Actions Log</h2>
                </header>
                <table class="table">
                    <thead>
                        <tr>
                            <th>CA #</th>
                            <th>Audit #</th>
                            <th>Description</th>
                            <th>Assigned To</th>
                            <th>Due Date</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="report.openCorrectiveActions.length === 0">
                            <td colspan="6" class="empty">No open corrective actions for this period.</td>
                        </tr>
                        <tr v-for="ca in report.openCorrectiveActions" :key="ca.id">
                            <td>{{ ca.id }}</td>
                            <td>{{ ca.auditId }}</td>
                            <td>{{ ca.description }}</td>
                            <td>{{ ca.assignedTo ?? '-' }}</td>
                            <td>{{ ca.dueDate ?? '-' }}</td>
                            <td>{{ ca.isOverdue ? 'Overdue' : ca.status }}</td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <section class="panel">
                <header class="panel-head">
                    <h2>Narrative Summary</h2>
                    <p>Editable executive summary text.</p>
                </header>
                <textarea
                    v-model="summaryText"
                    rows="6"
                    class="narrative"
                    placeholder="Click 'Generate with AI (Draft)' to draft this section."
                    @input="summaryEdited = true"
                />
            </section>

            <section class="signature">
                <div class="sig-item">
                    <span>Prepared By</span>
                    <div class="line"></div>
                </div>
                <div class="sig-item">
                    <span>Reviewed By</span>
                    <div class="line"></div>
                </div>
                <div class="sig-item">
                    <span>Date</span>
                    <div class="line"></div>
                </div>
            </section>
        </template>

        <div v-else class="loading">No report data for this quarter yet.</div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useApiStore } from '@/stores/apiStore';
import {
    AuditClient,
    type AuditReportDto,
    type DivisionDto,
    type SectionTrendsReportDto,
} from '@/apiclient/auditClient';

const route = useRoute();
const router = useRouter();
const apiStore = useApiStore();

const loading = ref(false);
const report = ref<AuditReportDto | null>(null);
const sectionTrends = ref<SectionTrendsReportDto | null>(null);
const divisions = ref<DivisionDto[]>([]);

const now = new Date();
const selectedDivisionId = ref<number | ''>(Number(route.query.divisionId) || '');
const selectedYear = ref(Number(route.query.year) || now.getFullYear());
const selectedQuarter = ref(Number(route.query.quarter) || Math.ceil((now.getMonth() + 1) / 3));

const summaryText = ref('');
const summaryEdited = ref(false);

const yearOptions = computed(() => {
    const year = now.getFullYear();
    return [year, year - 1, year - 2, year - 3];
});

const todayLabel = now.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}

function quarterDateRange(year: number, quarter: number) {
    const startMonth = (quarter - 1) * 3 + 1;
    const endMonth = startMonth + 2;
    const pad = (n: number) => String(n).padStart(2, '0');
    const daysInMonth = (m: number) => new Date(year, m, 0).getDate();
    return {
        from: `${year}-${pad(startMonth)}-01`,
        to: `${year}-${pad(endMonth)}-${daysInMonth(endMonth)}`,
    };
}

// Live date range — initialized from the current quarter selection
const { from: initFrom, to: initTo } = quarterDateRange(selectedYear.value, selectedQuarter.value);
const dateFrom = ref(initFrom);
const dateTo = ref(initTo);

function onQuarterChange() {
    const { from, to } = quarterDateRange(selectedYear.value, selectedQuarter.value);
    dateFrom.value = from;
    dateTo.value = to;
    loadData();
}

const selectedQuarterKey = computed(() => `${selectedYear.value} Q${selectedQuarter.value}`);

const periodLabel = computed(() => {
    if (!dateFrom.value || !dateTo.value) return selectedQuarterKey.value;
    const { from, to } = quarterDateRange(selectedYear.value, selectedQuarter.value);
    if (dateFrom.value === from && dateTo.value === to) {
        const labels = ['Q1 (Jan–Mar)', 'Q2 (Apr–Jun)', 'Q3 (Jul–Sep)', 'Q4 (Oct–Dec)'];
        return `${selectedYear.value} ${labels[selectedQuarter.value - 1] ?? ''}`;
    }
    const fmt = (s: string) => new Date(s + 'T00:00:00').toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    return `${fmt(dateFrom.value)} – ${fmt(dateTo.value)}`;
});

const divisionLabel = computed(() => {
    if (!selectedDivisionId.value) return 'All Divisions';
    const d = divisions.value.find(x => x.id === selectedDivisionId.value);
    return d ? `${d.code} - ${d.name}` : 'Division';
});

const correctedOnSitePct = computed(() => {
    if (!report.value || report.value.totalNonConforming === 0) return 0;
    return Math.round((report.value.correctedOnSiteCount / report.value.totalNonConforming) * 100);
});

type FindingsRow = {
    sectionName: string;
    divisionRate: number;
    companyRate: number;
};

const findingsRows = computed<FindingsRow[]>(() => {
    // Always derive from the actual report for the selected date range
    const totalAudits = report.value?.totalAudits ?? 0;
    if (totalAudits === 0) return [];

    // Build a map of company-wide rates from the section trends' most recent matching period
    const quarterKey = selectedQuarterKey.value;
    const trendMap = new Map<string, number>();
    for (const s of sectionTrends.value?.sections ?? []) {
        const pt = s.companyTrend.find(p => p.quarter === quarterKey);
        if (pt) trendMap.set(s.sectionName, pt.findingsPerAudit);
    }

    return (report.value?.sectionBreakdown ?? [])
        .map(s => ({
            sectionName: s.sectionName,
            divisionRate: Number((s.ncCount / totalAudits).toFixed(2)),
            companyRate: Number((trendMap.get(s.sectionName) ?? 0).toFixed(2)),
        }))
        .sort((a, b) => b.divisionRate - a.divisionRate);
});

function shortSectionName(name: string) {
    const MAX = 22;
    return name.length > MAX ? `${name.slice(0, MAX - 1)}...` : name;
}

// Always show both lines: division (or all-division aggregate) vs company-wide
// When "All Divisions" is selected, divisionTrend == companyTrend, so we still show
// the single line but skip the duplicate second series.
const showCompanySeries = computed(() => !!selectedDivisionId.value);

const findingsOverviewData = computed(() => {
    const labels = findingsRows.value.map(r => shortSectionName(r.sectionName));
    const datasets: Array<Record<string, unknown>> = [
        {
            label: showCompanySeries.value ? divisionLabel.value : 'Findings / Audit',
            data: findingsRows.value.map(r => Number(r.divisionRate.toFixed(2))),
            backgroundColor: 'rgba(37,99,235,0.75)',
            borderColor: '#2563eb',
            borderWidth: 1,
            borderRadius: 4,
        },
    ];

    if (showCompanySeries.value) {
        datasets.push({
            label: 'Company Wide',
            data: findingsRows.value.map(r => Number(r.companyRate.toFixed(2))),
            backgroundColor: 'rgba(245,158,11,0.75)',
            borderColor: '#f59e0b',
            borderWidth: 1,
            borderRadius: 4,
        });
    }

    return { labels, datasets };
});

const findingsOverviewOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: { labels: { color: '#cbd5e1' } },
        tooltip: {
            callbacks: {
                label: (ctx: { dataset: { label?: string }; raw: number }) =>
                    `${ctx.dataset.label ?? 'Rate'}: ${ctx.raw.toFixed(2)} NCs/audit`,
            },
        },
    },
    scales: {
        y: {
            beginAtZero: true,
            ticks: { color: '#94a3b8' },
            grid: { color: 'rgba(100,116,139,0.2)' },
        },
        x: {
            ticks: { color: '#94a3b8' },
            grid: { color: 'rgba(100,116,139,0.1)' },
        },
    },
};

const trendSections = computed(() => {
    const trends = sectionTrends.value;
    if (!trends || trends.sections.length === 0) return [];

    return trends.sections.map(section => {
        const datasets: Array<Record<string, unknown>> = [
            {
                label: divisionLabel.value,
                data: section.divisionTrend.map(p => Number(p.findingsPerAudit.toFixed(2))),
                borderColor: '#2563eb',
                backgroundColor: 'rgba(37,99,235,0.12)',
                borderWidth: 2,
                pointRadius: 3,
                tension: 0.3,
                fill: false,
            },
        ];
        // Always add the company-wide line when a specific division is selected
        if (showCompanySeries.value) {
            datasets.push({
                label: 'Company Wide',
                data: section.companyTrend.map(p => Number(p.findingsPerAudit.toFixed(2))),
                borderColor: '#f59e0b',
                backgroundColor: 'rgba(245,158,11,0.10)',
                borderWidth: 2,
                pointRadius: 3,
                tension: 0.3,
                fill: false,
                borderDash: [5, 3],
            });
        }
        return {
            sectionName: section.sectionName,
            chartData: { labels: trends.quarters, datasets },
        };
    });
});

const sectionTrendOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: { labels: { color: '#cbd5e1' } },
    },
    scales: {
        y: {
            beginAtZero: true,
            ticks: { color: '#94a3b8' },
            grid: { color: 'rgba(100,116,139,0.2)' },
        },
        x: {
            ticks: { color: '#94a3b8' },
            grid: { color: 'rgba(100,116,139,0.1)' },
        },
    },
};

const auditorRows = computed(() => {
    const map = new Map<string, { scores: number[]; ncs: number; warnings: number }>();
    for (const row of report.value?.rows ?? []) {
        const key = row.auditor?.trim() || 'Unknown';
        if (!map.has(key)) map.set(key, { scores: [], ncs: 0, warnings: 0 });
        const entry = map.get(key)!;
        if (row.scorePercent != null) entry.scores.push(row.scorePercent);
        entry.ncs += row.nonConformingCount;
        entry.warnings += row.warningCount;
    }

    return Array.from(map.entries())
        .map(([auditor, stats]) => ({
            auditor,
            count: stats.scores.length,
            avgScore: stats.scores.length
                ? Math.round((stats.scores.reduce((a, b) => a + b, 0) / stats.scores.length) * 10) / 10
                : null as number | null,
            ncs: stats.ncs,
            warnings: stats.warnings,
        }))
        .filter(x => x.count > 0)
        .sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

function buildNarrativeDraft() {
    if (!report.value) return '';
    const top3 = findingsRows.value.slice(0, 3).filter(x => x.divisionRate > 0);
    const topNames = top3.length > 0 ? top3.map(x => x.sectionName).join(', ') : 'no materially elevated sections';
    const overdueCount = report.value.openCorrectiveActions.filter(x => x.isOverdue).length;

    return `${divisionLabel.value} completed ${report.value.totalAudits} audits in ${periodLabel.value} with an average conformance score of ${report.value.avgScorePercent ?? 0}%. `
        + `The period recorded ${report.value.totalNonConforming} non-conformances and ${report.value.totalWarnings} warnings, with ${correctedOnSitePct.value}% corrected on-site. `
        + `Highest findings-per-audit sections were ${topNames}. `
        + `There are currently ${report.value.openCorrectiveActions.length} open corrective actions, including ${overdueCount} overdue items requiring follow-up.`;
}

function generateNarrative() {
    summaryText.value = buildNarrativeDraft();
    summaryEdited.value = true;
}

async function loadData() {
    loading.value = true;
    try {
        const client = getClient();
        const [nextReport, nextTrends] = await Promise.all([
            client.getAuditReport(selectedDivisionId.value || null, null, dateFrom.value, dateTo.value),
            client.getSectionTrends(selectedDivisionId.value || null, null, null),
        ]);
        report.value = nextReport;
        sectionTrends.value = nextTrends;
        if (!summaryEdited.value) {
            summaryText.value = buildNarrativeDraft();
        }
    } finally {
        loading.value = false;
    }
}

function printPage() {
    window.print();
}

onMounted(async () => {
    divisions.value = await getClient().getDivisions();
    await loadData();
});
</script>

<style scoped>
.newsletter-page {
    max-width: 1180px;
    margin: 0 auto;
    padding: 18px;
    color: #e2e8f0;
}

.toolbar {
    background: linear-gradient(120deg, #0f172a, #1e293b);
    border: 1px solid #334155;
    border-radius: 10px;
    padding: 14px;
    margin-bottom: 16px;
}

.toolbar-title h1 {
    margin: 0;
    font-size: 22px;
    color: #f8fafc;
}

.toolbar-title p {
    margin: 4px 0 0;
    color: #94a3b8;
    font-size: 13px;
}

.toolbar-fields {
    margin-top: 12px;
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    align-items: flex-end;
}

.toolbar-fields label {
    display: flex;
    flex-direction: column;
    gap: 4px;
    color: #94a3b8;
    font-size: 12px;
}

.toolbar-fields select {
    min-width: 150px;
    background: #0f172a;
    color: #f8fafc;
    border: 1px solid #475569;
    border-radius: 6px;
    padding: 7px 10px;
}

.toolbar-date {
    background: #0f172a;
    color: #f8fafc;
    border: 1px solid #475569;
    border-radius: 6px;
    padding: 7px 10px;
    font-size: 13px;
    min-width: 130px;
    color-scheme: dark;
}

.toolbar-sep {
    align-self: flex-end;
    padding-bottom: 8px;
    color: #475569;
    font-size: 12px;
}

.btn {
    border: 1px solid #1d4ed8;
    background: #2563eb;
    color: #fff;
    border-radius: 6px;
    padding: 7px 12px;
    cursor: pointer;
    font-size: 13px;
}

.btn.secondary {
    border-color: #475569;
    background: #1e293b;
}

.loading {
    background: #0f172a;
    border: 1px solid #334155;
    border-radius: 10px;
    padding: 30px;
    text-align: center;
    color: #94a3b8;
}

.cover {
    border-radius: 12px;
    overflow: hidden;
    margin-bottom: 16px;
    min-height: 180px;
    background:
        radial-gradient(circle at 80% 10%, rgba(37, 99, 235, 0.5), transparent 35%),
        radial-gradient(circle at 20% 90%, rgba(14, 116, 144, 0.5), transparent 35%),
        linear-gradient(140deg, #0f172a 15%, #1e293b 55%, #0b1220 100%);
    border: 1px solid #334155;
}

.cover-overlay {
    padding: 22px;
}

.cover-subtitle {
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: #93c5fd;
    font-size: 12px;
    font-weight: 600;
}

.cover h1 {
    margin: 10px 0 0;
    font-size: 34px;
    color: #f8fafc;
}

.cover p {
    margin: 8px 0 0;
    color: #cbd5e1;
}

.kpi-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 12px;
    margin-bottom: 16px;
}

.kpi {
    background: #0f172a;
    border: 1px solid #334155;
    border-radius: 10px;
    padding: 12px;
}

.kpi h3 {
    margin: 0;
    font-size: 12px;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #94a3b8;
}

.kpi-value {
    margin-top: 10px;
    font-size: 32px;
    line-height: 1;
    font-weight: 700;
    color: #f8fafc;
}

.panel {
    background: #0f172a;
    border: 1px solid #334155;
    border-radius: 10px;
    padding: 14px;
    margin-bottom: 14px;
}

.panel-head h2 {
    margin: 0;
    color: #f8fafc;
    font-size: 19px;
}

.panel-head p {
    margin: 4px 0 0;
    color: #94a3b8;
    font-size: 13px;
}

.trend-grid {
    margin-top: 12px;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;
}

.trend-card {
    background: #111b2d;
    border: 1px solid #334155;
    border-radius: 8px;
    padding: 10px;
}

.trend-card h3 {
    margin: 0 0 8px;
    color: #f1f5f9;
    font-size: 14px;
}

.table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 8px;
}

.table th,
.table td {
    border: 1px solid #334155;
    padding: 8px;
    font-size: 12px;
    text-align: left;
}

.table th {
    background: #17253a;
    color: #cbd5e1;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    font-size: 11px;
}

.table td {
    color: #e2e8f0;
}

.empty {
    text-align: center;
    color: #94a3b8;
}

.narrative {
    width: 100%;
    margin-top: 10px;
    background: #111b2d;
    color: #e2e8f0;
    border: 1px solid #334155;
    border-radius: 8px;
    padding: 10px;
    resize: vertical;
    font-family: inherit;
    font-size: 13px;
}

.signature {
    margin-top: 18px;
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    gap: 16px;
}

.sig-item span {
    display: block;
    margin-bottom: 8px;
    font-size: 11px;
    text-transform: uppercase;
    color: #94a3b8;
}

.line {
    border-bottom: 1px solid #64748b;
    min-height: 24px;
}

@media (max-width: 980px) {
    .kpi-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .trend-grid {
        grid-template-columns: 1fr;
    }
}

@media print {
    .no-print {
        display: none !important;
    }

    .newsletter-page {
        max-width: none;
        color: #000;
        padding: 0;
    }

    .cover,
    .kpi,
    .panel {
        background: #fff !important;
        color: #000 !important;
        border-color: #bbb !important;
        page-break-inside: avoid;
    }

    .table th,
    .table td {
        border-color: #bbb;
        color: #000;
    }
}
</style>

<template>
    <div>
        <BasePageHeader
            title="Reports"
            subtitle="Conformance trends and non-conformance analysis"
            icon="pi pi-chart-bar"
        >
            <Button
                v-if="report"
                label="Export CSV"
                icon="pi pi-download"
                severity="secondary"
                outlined
                size="small"
                @click="exportCsv"
            />
            <Button
                label="Quarterly Summary"
                icon="pi pi-print"
                severity="secondary"
                outlined
                size="small"
                @click="openQSummary"
            />
            <Button icon="pi pi-refresh" severity="secondary" :loading="loading" @click="loadReport" />
        </BasePageHeader>

        <!-- Filter bar -->
        <div class="px-4 pt-2 pb-0 flex flex-wrap gap-3 items-center">
            <Dropdown
                v-model="filterDivisionId"
                :options="store.divisions"
                option-label="code"
                option-value="id"
                placeholder="All Divisions"
                class="w-44"
                :show-clear="!!filterDivisionId"
                @change="loadReport"
            />
            <Dropdown
                v-model="filterStatus"
                :options="STATUS_OPTIONS"
                option-label="label"
                option-value="value"
                placeholder="All Statuses"
                class="w-40"
                :show-clear="!!filterStatus"
                @change="loadReport"
            />
            <Calendar
                v-model="filterDateFrom"
                placeholder="From date"
                dateFormat="yy-mm-dd"
                class="w-36"
                :show-clear="!!filterDateFrom"
                @date-select="loadReport"
                @clear-click="loadReport"
            />
            <Calendar
                v-model="filterDateTo"
                placeholder="To date"
                dateFormat="yy-mm-dd"
                class="w-36"
                :show-clear="!!filterDateTo"
                @date-select="loadReport"
                @clear-click="loadReport"
            />
        </div>

        <div v-if="loading" class="flex justify-center py-16">
            <ProgressSpinner />
        </div>

        <div v-else-if="report" class="p-4 space-y-4">

            <!-- KPI cards — row 1 -->
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
                <!-- Total Audits -->
                <div class="bg-slate-800 border border-slate-700 rounded-lg p-4 text-center">
                    <div class="text-3xl font-bold text-white">{{ report.totalAudits }}</div>
                    <div class="text-xs text-slate-400 mt-1">Total Audits</div>
                    <div v-if="trendDeltas.auditCountDelta !== null" class="text-xs mt-1"
                        :class="trendDeltas.auditCountDelta >= 0 ? 'text-emerald-400' : 'text-red-400'">
                        {{ trendDeltas.auditCountDelta >= 0 ? '↑' : '↓' }}
                        {{ Math.abs(trendDeltas.auditCountDelta) }} vs prior period
                    </div>
                </div>
                <!-- Avg Conformance -->
                <div class="bg-slate-800 border border-slate-700 rounded-lg p-4 text-center">
                    <div :class="['text-3xl font-bold', scoreColor]">
                        {{ report.avgScorePercent != null ? `${report.avgScorePercent}%` : '—' }}
                    </div>
                    <div class="text-xs text-slate-400 mt-1">Avg Conformance</div>
                    <div v-if="trendDeltas.scoreDelta !== null" class="text-xs mt-1"
                        :class="trendDeltas.scoreDelta >= 0 ? 'text-emerald-400' : 'text-red-400'">
                        {{ trendDeltas.scoreDelta >= 0 ? '↑' : '↓' }}
                        {{ Math.abs(trendDeltas.scoreDelta) }}% vs prior period
                    </div>
                </div>
                <!-- Non-Conformances -->
                <div class="bg-slate-800 border border-red-900/50 rounded-lg p-4 text-center">
                    <div class="text-3xl font-bold text-red-400">{{ report.totalNonConforming }}</div>
                    <div class="text-xs text-slate-400 mt-1">Non-Conformances</div>
                    <div v-if="trendDeltas.ncDelta !== null" class="text-xs mt-1"
                        :class="trendDeltas.ncDelta <= 0 ? 'text-emerald-400' : 'text-red-400'">
                        {{ trendDeltas.ncDelta <= 0 ? '↓' : '↑' }}
                        {{ Math.abs(trendDeltas.ncDelta) }} vs prior period
                    </div>
                </div>
                <!-- Warnings -->
                <div class="bg-slate-800 border border-amber-900/50 rounded-lg p-4 text-center">
                    <div class="text-3xl font-bold text-amber-400">{{ report.totalWarnings }}</div>
                    <div class="text-xs text-slate-400 mt-1">Warnings</div>
                    <div v-if="trendDeltas.warnDelta !== null" class="text-xs mt-1"
                        :class="trendDeltas.warnDelta <= 0 ? 'text-emerald-400' : 'text-amber-400'">
                        {{ trendDeltas.warnDelta <= 0 ? '↓' : '↑' }}
                        {{ Math.abs(trendDeltas.warnDelta) }} vs prior period
                    </div>
                </div>
            </div>

            <!-- KPI row 2: Corrected on site + CA aging -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <!-- Corrected on site -->
                <div class="bg-slate-800 border border-slate-700 rounded-lg p-4 flex items-center gap-4">
                    <div class="text-3xl font-bold"
                        :class="correctedOnSitePct >= 50 ? 'text-emerald-400' : correctedOnSitePct > 0 ? 'text-amber-400' : 'text-slate-400'">
                        {{ report.totalNonConforming > 0 ? `${correctedOnSitePct}%` : '—' }}
                    </div>
                    <div>
                        <div class="text-sm font-medium text-slate-200">Corrected On Site</div>
                        <div class="text-xs text-slate-400 mt-0.5">
                            {{ report.correctedOnSiteCount }} of {{ report.totalNonConforming }} NCs fixed immediately
                        </div>
                    </div>
                </div>
                <!-- CA aging -->
                <div class="bg-slate-800 border border-slate-700 rounded-lg p-4 flex items-center gap-4">
                    <div class="text-3xl font-bold"
                        :class="caAgingStats.overdueCount > 0 ? 'text-red-400' : 'text-emerald-400'">
                        {{ caAgingStats.overdueCount }}
                    </div>
                    <div>
                        <div class="text-sm font-medium text-slate-200">CAs Past 14-Day Rule</div>
                        <div class="text-xs text-slate-400 mt-0.5">
                            {{ report.openCorrectiveActions.length }} open ·
                            Avg {{ caAgingStats.avgDaysOpen }} days open
                        </div>
                    </div>
                </div>
            </div>

            <!-- Division performance chart -->
            <Card v-if="divisionStats.length > 1">
                <template #title>
                    <span class="text-base font-semibold text-white">Conformance by Division</span>
                </template>
                <template #content>
                    <Chart
                        type="bar"
                        :data="divisionChartData"
                        :options="divisionChartOptions"
                        :style="`height: ${Math.max(160, divisionStats.length * 40)}px;`"
                    />
                </template>
            </Card>

            <!-- Trend chart -->
            <Card v-if="report.trend.length > 0">
                <template #title>
                    <span class="text-base font-semibold text-white">Conformance Trend (Last 12 Weeks)</span>
                </template>
                <template #content>
                    <Chart type="bar" :data="chartData" :options="chartOptions" style="height: 280px;" />
                </template>
            </Card>

            <!-- NC by Section chart -->
            <Card v-if="(report.sectionBreakdown?.length ?? 0) > 0">
                <template #title>
                    <span class="text-base font-semibold text-white">Non-Conformances by Section</span>
                </template>
                <template #content>
                    <Chart
                        type="bar"
                        :data="ncCategoryChartData"
                        :options="ncCategoryChartOptions"
                        :style="`height: ${Math.max(160, (report.sectionBreakdown?.length ?? 0) * 36)}px;`"
                    />
                </template>
            </Card>

            <!-- Top locations by NC -->
            <Card v-if="locationStats.length >= 2">
                <template #title>
                    <span class="text-base font-semibold text-white">Top Locations by Non-Conformances</span>
                </template>
                <template #content>
                    <Chart
                        type="bar"
                        :data="locationChartData"
                        :options="locationChartOptions"
                        :style="`height: ${Math.max(160, locationStats.length * 36)}px;`"
                    />
                </template>
            </Card>

            <!-- Open Corrective Actions -->
            <Card v-if="(report.openCorrectiveActions?.length ?? 0) > 0">
                <template #title>
                    <div class="flex items-center gap-3 flex-wrap">
                        <span class="text-base font-semibold text-white">Open Corrective Actions</span>
                        <Tag :value="String(report.openCorrectiveActions.length)" severity="danger" />
                        <span v-if="caAgingStats.overdueCount > 0" class="text-xs text-red-400 font-medium">
                            {{ caAgingStats.overdueCount }} past 14-day rule
                        </span>
                        <span class="text-xs text-slate-400">· Avg {{ caAgingStats.avgDaysOpen }} days open</span>
                    </div>
                </template>
                <template #content>
                    <DataTable
                        :value="report.openCorrectiveActions"
                        :rows="10"
                        paginator
                        sortField="dueDate"
                        :sortOrder="1"
                        class="text-sm"
                    >
                        <Column field="id" header="CA #" style="width:60px" sortable />
                        <Column field="auditId" header="Audit #" style="width:80px" sortable />
                        <Column field="description" header="Description">
                            <template #body="{ data }">
                                <span class="line-clamp-2">{{ data.description }}</span>
                            </template>
                        </Column>
                        <Column field="assignedTo" header="Assigned To" sortable>
                            <template #body="{ data }">{{ data.assignedTo ?? '—' }}</template>
                        </Column>
                        <Column field="dueDate" header="Due Date" sortable>
                            <template #body="{ data }">
                                <span :class="data.isOverdue ? 'text-red-400 font-semibold' : ''">
                                    {{ data.dueDate ?? '—' }}
                                </span>
                            </template>
                        </Column>
                        <Column field="daysOpen" header="Age" sortable style="width:80px">
                            <template #body="{ data }">
                                <span :class="data.daysOpen > 14 ? 'text-red-400 font-semibold' : data.daysOpen > 7 ? 'text-amber-400' : 'text-emerald-400'">
                                    {{ data.daysOpen }}d
                                </span>
                            </template>
                        </Column>
                        <Column field="status" header="Status" sortable style="width:110px">
                            <template #body="{ data }">
                                <Tag
                                    :value="data.isOverdue ? 'Overdue' : data.status"
                                    :severity="data.isOverdue ? 'danger' : 'warning'"
                                />
                            </template>
                        </Column>
                        <Column header="" style="width:50px">
                            <template #body="{ data }">
                                <Button
                                    icon="pi pi-eye"
                                    size="small"
                                    severity="secondary"
                                    text
                                    @click="router.push(`/audit-management/audits/${data.auditId}/review`)"
                                />
                            </template>
                        </Column>
                    </DataTable>
                </template>
            </Card>

            <!-- Quarterly trend chart -->
            <Card v-if="quarterlyTrendData.length > 1">
                <template #title>
                    <span class="text-base font-semibold text-white">Quarterly Conformance Trend</span>
                </template>
                <template #content>
                    <Chart type="line" :data="quarterlyChartData" :options="quarterlyChartOptions" style="height: 260px;" />
                </template>
            </Card>

            <!-- Auditor performance -->
            <Card v-if="auditorStats.length > 0">
                <template #title>
                    <span class="text-base font-semibold text-white">Auditor Performance</span>
                </template>
                <template #content>
                    <DataTable :value="auditorStats" sortField="avgScore" :sortOrder="-1" class="text-sm">
                        <Column field="auditor" header="Auditor" sortable />
                        <Column field="auditCount" header="Audits" sortable style="width:80px" />
                        <Column field="avgScore" header="Avg Score" sortable style="width:110px">
                            <template #body="{ data }">
                                <span :class="rowScoreColor(data.avgScore)">
                                    {{ data.avgScore != null ? `${data.avgScore}%` : '—' }}
                                </span>
                            </template>
                        </Column>
                        <Column field="totalNcs" header="Total NCs" sortable style="width:100px">
                            <template #body="{ data }">
                                <span :class="data.totalNcs > 0 ? 'text-red-400 font-semibold' : 'text-slate-400'">
                                    {{ data.totalNcs }}
                                </span>
                            </template>
                        </Column>
                        <Column field="totalWarnings" header="Warnings" sortable style="width:100px">
                            <template #body="{ data }">
                                <span :class="data.totalWarnings > 0 ? 'text-amber-400' : 'text-slate-400'">
                                    {{ data.totalWarnings }}
                                </span>
                            </template>
                        </Column>
                    </DataTable>
                </template>
            </Card>

            <!-- Audit table -->
            <Card>
                <template #title>
                    <span class="text-base font-semibold text-white">Audit Detail</span>
                </template>
                <template #content>
                    <DataTable
                        :value="report.rows"
                        :rows="20"
                        paginator
                        sortField="id"
                        :sortOrder="-1"
                        class="text-sm"
                    >
                        <Column field="id" header="#" style="width:60px" sortable />
                        <Column field="divisionCode" header="Division" sortable />
                        <Column field="auditDate" header="Date" sortable>
                            <template #body="{ data }">{{ data.auditDate ?? '—' }}</template>
                        </Column>
                        <Column field="auditor" header="Auditor" sortable>
                            <template #body="{ data }">{{ data.auditor ?? '—' }}</template>
                        </Column>
                        <Column field="jobNumber" header="Job #">
                            <template #body="{ data }">{{ data.jobNumber ?? '—' }}</template>
                        </Column>
                        <Column field="location" header="Location">
                            <template #body="{ data }">{{ data.location ?? '—' }}</template>
                        </Column>
                        <Column field="status" header="Status" sortable>
                            <template #body="{ data }">
                                <Tag :value="data.status" :severity="statusSeverity(data.status)" />
                            </template>
                        </Column>
                        <Column field="scorePercent" header="Score" sortable>
                            <template #body="{ data }">
                                <span :class="rowScoreColor(data.scorePercent)">
                                    {{ data.scorePercent != null ? `${data.scorePercent}%` : '—' }}
                                </span>
                            </template>
                        </Column>
                        <Column field="nonConformingCount" header="NCs" sortable style="width:60px">
                            <template #body="{ data }">
                                <span :class="data.nonConformingCount > 0 ? 'text-red-400 font-semibold' : 'text-slate-400'">
                                    {{ data.nonConformingCount }}
                                </span>
                            </template>
                        </Column>
                        <Column field="warningCount" header="Warn" sortable style="width:60px">
                            <template #body="{ data }">
                                <span :class="data.warningCount > 0 ? 'text-amber-400' : 'text-slate-400'">
                                    {{ data.warningCount }}
                                </span>
                            </template>
                        </Column>
                        <Column header="" style="width:60px">
                            <template #body="{ data }">
                                <Button icon="pi pi-eye" size="small" severity="secondary" text @click="router.push(`/audit-management/audits/${data.id}/review`)" />
                            </template>
                        </Column>
                    </DataTable>
                </template>
            </Card>
        </div>

        <div v-else class="p-4 text-center text-slate-400 py-16">
            No report data available.
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import Card from 'primevue/card';
import Chart from 'primevue/chart';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import Dropdown from 'primevue/dropdown';
import Calendar from 'primevue/calendar';
import ProgressSpinner from 'primevue/progressspinner';
import Button from 'primevue/button';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type AuditReportDto } from '@/apiclient/auditClient';

const router = useRouter();
const store = useAuditStore();
const apiStore = useApiStore();

const loading = ref(false);
const report = ref<AuditReportDto | null>(null);
const filterDivisionId = ref<number | null>(null);
const filterStatus = ref<string | null>(null);
const filterDateFrom = ref<Date | null>(null);
const filterDateTo = ref<Date | null>(null);

const STATUS_OPTIONS = [
    { label: 'Submitted', value: 'Submitted' },
    { label: 'Closed', value: 'Closed' },
    { label: 'Draft', value: 'Draft' },
];

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}

async function loadReport() {
    loading.value = true;
    try {
        const from = filterDateFrom.value ? filterDateFrom.value.toISOString().split('T')[0] : null;
        const to = filterDateTo.value ? filterDateTo.value.toISOString().split('T')[0] : null;
        report.value = await getClient().getAuditReport(
            filterDivisionId.value,
            filterStatus.value,
            from,
            to,
        );
    } finally {
        loading.value = false;
    }
}

onMounted(async () => {
    await Promise.all([store.loadDivisions(), loadReport()]);
});

// ── Item 1: CSV Export ────────────────────────────────────────────────────────

function exportCsv() {
    if (!report.value?.rows.length) return;
    const headers = ['Audit #', 'Division', 'Date', 'Auditor', 'Job #', 'Location', 'Status', 'Score %', 'NCs', 'Warnings'];
    const rows = report.value.rows.map(r => [
        r.id,
        r.divisionCode,
        r.auditDate ?? '',
        r.auditor ?? '',
        r.jobNumber ?? '',
        r.location ?? '',
        r.status,
        r.scorePercent ?? '',
        r.nonConformingCount,
        r.warningCount,
    ]);
    const csv = [headers, ...rows]
        .map(row => row.map(v => `"${String(v).replace(/"/g, '""')}"`).join(','))
        .join('\n');
    const divCode = store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'all';
    const from = filterDateFrom.value?.toISOString().split('T')[0] ?? '';
    const to = filterDateTo.value?.toISOString().split('T')[0] ?? '';
    const fileName = `audit-report-${divCode}${from ? `-${from}` : ''}${to ? `-${to}` : ''}.csv`;
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
}

// ── Item 5: Open quarterly summary ────────────────────────────────────────────

function openQSummary() {
    const divId = filterDivisionId.value ?? '';
    const now = new Date();
    const year = now.getFullYear();
    const quarter = Math.ceil((now.getMonth() + 1) / 3);
    window.open(`/audit-management/reports/quarterly-summary?divisionId=${divId}&year=${year}&quarter=${quarter}`, '_blank');
}

// ── Item 2: KPI trend deltas ──────────────────────────────────────────────────

const trendDeltas = computed(() => {
    const rows = report.value?.rows ?? [];
    if (rows.length < 2) return { scoreDelta: null, ncDelta: null, warnDelta: null, auditCountDelta: null };

    // Split sorted rows into two halves by date
    const dated = rows
        .filter(r => r.auditDate)
        .sort((a, b) => (a.auditDate ?? '').localeCompare(b.auditDate ?? ''));
    if (dated.length < 4) return { scoreDelta: null, ncDelta: null, warnDelta: null, auditCountDelta: null };

    const mid = Math.floor(dated.length / 2);
    const prior = dated.slice(0, mid);
    const current = dated.slice(mid);

    function avg(arr: typeof dated) {
        const scored = arr.filter(r => r.scorePercent != null);
        return scored.length ? scored.reduce((s, r) => s + r.scorePercent!, 0) / scored.length : null;
    }

    const curScore = avg(current);
    const prvScore = avg(prior);
    const scoreDelta = curScore != null && prvScore != null
        ? Math.round((curScore - prvScore) * 10) / 10
        : null;

    const ncDelta = current.reduce((s, r) => s + r.nonConformingCount, 0)
        - prior.reduce((s, r) => s + r.nonConformingCount, 0);
    const warnDelta = current.reduce((s, r) => s + r.warningCount, 0)
        - prior.reduce((s, r) => s + r.warningCount, 0);
    const auditCountDelta = current.length - prior.length;

    return { scoreDelta, ncDelta, warnDelta, auditCountDelta };
});

// ── Item 3: Corrected on site + CA aging ─────────────────────────────────────

const correctedOnSitePct = computed(() => {
    if (!report.value || report.value.totalNonConforming === 0) return 0;
    return Math.round(report.value.correctedOnSiteCount / report.value.totalNonConforming * 100);
});

const caAgingStats = computed(() => {
    const cas = report.value?.openCorrectiveActions ?? [];
    if (!cas.length) return { overdueCount: 0, avgDaysOpen: 0 };
    const overdueCount = cas.filter(ca => ca.daysOpen > 14).length;
    const avgDaysOpen = Math.round(cas.reduce((s, ca) => s + ca.daysOpen, 0) / cas.length);
    return { overdueCount, avgDaysOpen };
});

// ── Item 4: Top locations by NC ───────────────────────────────────────────────

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
    labels: locationStats.value.map(l => l.location),
    datasets: [{
        label: 'Non-Conformances',
        data: locationStats.value.map(l => l.ncs),
        backgroundColor: 'rgba(239,68,68,0.6)',
        borderColor: '#ef4444',
        borderWidth: 1,
        borderRadius: 4,
    }],
}));

const locationChartOptions = {
    indexAxis: 'y' as const,
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: { display: false },
        tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw} NCs` } },
    },
    scales: {
        x: {
            beginAtZero: true,
            ticks: { color: '#94a3b8', stepSize: 1, precision: 0 },
            grid: { color: 'rgba(100,116,139,0.2)' },
        },
        y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
};

// ── Existing chart/table computeds ────────────────────────────────────────────

const scoreColor = computed(() => {
    const pct = report.value?.avgScorePercent;
    if (pct == null) return 'text-slate-400';
    if (pct >= 90) return 'text-emerald-400';
    if (pct >= 75) return 'text-amber-400';
    return 'text-red-400';
});

function rowScoreColor(pct: number | null | undefined): string {
    if (pct == null) return 'text-slate-400';
    if (pct >= 90) return 'text-emerald-400 font-semibold';
    if (pct >= 75) return 'text-amber-400 font-semibold';
    return 'text-red-400 font-semibold';
}

function statusSeverity(status: string): string {
    const map: Record<string, string> = {
        Draft: 'warning', Submitted: 'info', Reopened: 'warning', Closed: 'success',
    };
    return map[status] ?? 'secondary';
}

const chartData = computed(() => {
    const t = report.value?.trend ?? [];
    return {
        labels: t.map(p => p.week),
        datasets: [{
            label: 'Avg Score %',
            data: t.map(p => p.avgScore),
            backgroundColor: t.map(p =>
                p.avgScore == null ? 'rgba(100,116,139,0.5)'
                : p.avgScore >= 90 ? 'rgba(16,185,129,0.7)'
                : p.avgScore >= 75 ? 'rgba(245,158,11,0.7)'
                : 'rgba(239,68,68,0.7)',
            ),
            borderColor: t.map(p =>
                p.avgScore == null ? '#64748b'
                : p.avgScore >= 90 ? '#10b981'
                : p.avgScore >= 75 ? '#f59e0b'
                : '#ef4444',
            ),
            borderWidth: 1,
            borderRadius: 4,
        }],
    };
});

const chartOptions = {
    responsive: true, maintainAspectRatio: false,
    plugins: {
        legend: { display: false },
        tooltip: { callbacks: { label: (ctx: { raw: number | null }) => ctx.raw != null ? `${ctx.raw}%` : 'No data' } },
    },
    scales: {
        y: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
        x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
};

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
    return Array.from(map.entries())
        .map(([division, s]) => ({
            division,
            auditCount: s.count,
            avgScore: s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
            totalNcs: s.ncs,
        }))
        .filter(r => r.avgScore != null)
        .sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

const divisionChartData = computed(() => ({
    labels: divisionStats.value.map(d => `${d.division} (${d.auditCount})`),
    datasets: [{
        label: 'Avg Score %',
        data: divisionStats.value.map(d => d.avgScore),
        backgroundColor: divisionStats.value.map(d =>
            (d.avgScore ?? 0) >= 90 ? 'rgba(16,185,129,0.7)' : (d.avgScore ?? 0) >= 75 ? 'rgba(245,158,11,0.7)' : 'rgba(239,68,68,0.7)',
        ),
        borderColor: divisionStats.value.map(d =>
            (d.avgScore ?? 0) >= 90 ? '#10b981' : (d.avgScore ?? 0) >= 75 ? '#f59e0b' : '#ef4444',
        ),
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
    return Array.from(byQuarter.entries())
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([quarter, scores]) => ({
            quarter,
            avgScore: Math.round(scores.reduce((a, b) => a + b, 0) / scores.length * 10) / 10,
            count: scores.length,
        }));
});

const quarterlyChartData = computed(() => ({
    labels: quarterlyTrendData.value.map(p => p.quarter),
    datasets: [{
        label: 'Avg Score %',
        data: quarterlyTrendData.value.map(p => p.avgScore),
        borderColor: '#6366f1', backgroundColor: 'rgba(99,102,241,0.15)',
        borderWidth: 2, pointRadius: 5,
        pointBackgroundColor: quarterlyTrendData.value.map(p =>
            p.avgScore >= 90 ? '#10b981' : p.avgScore >= 75 ? '#f59e0b' : '#ef4444',
        ),
        tension: 0.3, fill: true,
    }],
}));

const quarterlyChartOptions = {
    responsive: true, maintainAspectRatio: false,
    plugins: {
        legend: { display: false },
        tooltip: { callbacks: { label: (ctx: { raw: number; dataIndex: number }) => { const pt = quarterlyTrendData.value[ctx.dataIndex]; return `${ctx.raw}%  (${pt?.count} audit${pt?.count !== 1 ? 's' : ''})`; } } },
    },
    scales: {
        y: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
        x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
};

const auditorStats = computed(() => {
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
        .map(([auditor, s]) => ({
            auditor,
            auditCount: s.scores.length,
            avgScore: s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
            totalNcs: s.ncs,
            totalWarnings: s.warnings,
        }))
        .filter(r => r.auditCount > 0)
        .sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

const ncCategoryChartData = computed(() => {
    const bd = report.value?.sectionBreakdown ?? [] as { sectionName: string; ncCount: number }[];
    return {
        labels: bd.map(x => x.sectionName),
        datasets: [{ label: 'Non-Conformances', data: bd.map(x => x.ncCount), backgroundColor: 'rgba(239,68,68,0.7)', borderColor: '#ef4444', borderWidth: 1, borderRadius: 4 }],
    };
});

const ncCategoryChartOptions = {
    indexAxis: 'y' as const, responsive: true, maintainAspectRatio: false,
    plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw} NCs` } } },
    scales: {
        x: { beginAtZero: true, ticks: { color: '#94a3b8', stepSize: 1, precision: 0 }, grid: { color: 'rgba(100,116,139,0.2)' } },
        y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
};
</script>

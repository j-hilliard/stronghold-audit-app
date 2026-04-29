<template>
    <div class="summary-page" ref="pageEl">

        <!-- Picker (screen only, hidden on print) -->
        <div class="picker no-print">
            <div class="picker-inner">
                <h2>Quarterly Compliance Summary</h2>
                <div class="picker-fields">
                    <label>
                        Division
                        <select v-model="selectedDivisionId" @change="loadData">
                            <option value="">All Divisions</option>
                            <option v-for="d in divisions" :key="d.id" :value="d.id">
                                {{ d.code }} — {{ d.name }}
                            </option>
                        </select>
                    </label>
                    <label>
                        Year
                        <select v-model="selectedYear" @change="loadData">
                            <option v-for="y in yearOptions" :key="y" :value="y">{{ y }}</option>
                        </select>
                    </label>
                    <label>
                        Quarter
                        <select v-model="selectedQuarter" @change="loadData">
                            <option :value="1">Q1 (Jan–Mar)</option>
                            <option :value="2">Q2 (Apr–Jun)</option>
                            <option :value="3">Q3 (Jul–Sep)</option>
                            <option :value="4">Q4 (Oct–Dec)</option>
                        </select>
                    </label>
                    <button @click="printPage" class="print-btn">Print / Save as PDF</button>
                </div>
            </div>
        </div>

        <div v-if="loading" class="loading-msg">Loading…</div>

        <template v-else-if="report">
            <!-- Header -->
            <div class="report-header">
                <div class="report-header-top">
                    <h1>{{ divisionLabel }} — Compliance Audit Summary</h1>
                    <p class="report-subtitle">{{ quarterLabel }} · Generated {{ todayStr }}</p>
                </div>
            </div>

            <!-- Summary KPIs -->
            <div class="kpi-grid">
                <div class="kpi-box">
                    <div class="kpi-value">{{ report.totalAudits }}</div>
                    <div class="kpi-label">Total Audits</div>
                </div>
                <div class="kpi-box">
                    <div class="kpi-value" :class="scoreClass">
                        {{ report.avgScorePercent != null ? `${report.avgScorePercent}%` : '—' }}
                    </div>
                    <div class="kpi-label">Avg Conformance</div>
                </div>
                <div class="kpi-box">
                    <div class="kpi-value kpi-red">{{ report.totalNonConforming }}</div>
                    <div class="kpi-label">Non-Conformances</div>
                </div>
                <div class="kpi-box">
                    <div class="kpi-value kpi-amber">{{ report.totalWarnings }}</div>
                    <div class="kpi-label">Warnings</div>
                </div>
                <div class="kpi-box">
                    <div class="kpi-value kpi-green">{{ correctedOnSitePct }}%</div>
                    <div class="kpi-label">Corrected On Site</div>
                </div>
                <div class="kpi-box">
                    <div class="kpi-value" :class="openCaCount > 0 ? 'kpi-red' : 'kpi-green'">
                        {{ openCaCount }}
                    </div>
                    <div class="kpi-label">Open CAs</div>
                </div>
            </div>

            <!-- Section breakdown table -->
            <div v-if="sectionRows.length" class="section-block">
                <h2 class="section-title">Non-Conformances by Section</h2>
                <table class="summary-table">
                    <thead>
                        <tr>
                            <th class="col-section">Section</th>
                            <th class="col-num">NC Count</th>
                            <th class="col-num">% of Total NCs</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="row in sectionRows" :key="row.sectionName">
                            <td>{{ row.sectionName }}</td>
                            <td class="text-center font-bold text-red">{{ row.ncCount }}</td>
                            <td class="text-center">{{ row.pct }}%</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Auditor breakdown table -->
            <div v-if="auditorRows.length" class="section-block">
                <h2 class="section-title">Auditor Performance</h2>
                <table class="summary-table">
                    <thead>
                        <tr>
                            <th class="col-section">Auditor</th>
                            <th class="col-num">Audits</th>
                            <th class="col-num">Avg Score</th>
                            <th class="col-num">NCs</th>
                            <th class="col-num">Warnings</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="row in auditorRows" :key="row.auditor">
                            <td>{{ row.auditor }}</td>
                            <td class="text-center">{{ row.count }}</td>
                            <td class="text-center font-bold">
                                {{ row.avgScore != null ? `${row.avgScore}%` : '—' }}
                            </td>
                            <td class="text-center">{{ row.ncs }}</td>
                            <td class="text-center">{{ row.warnings }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Open corrective actions -->
            <div v-if="report.openCorrectiveActions.length" class="section-block">
                <h2 class="section-title">Open Corrective Actions</h2>
                <table class="summary-table">
                    <thead>
                        <tr>
                            <th style="width:50px">CA #</th>
                            <th style="width:60px">Audit #</th>
                            <th>Description</th>
                            <th style="width:120px">Assigned To</th>
                            <th style="width:90px">Due Date</th>
                            <th style="width:80px">Age (days)</th>
                            <th style="width:80px">Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="ca in report.openCorrectiveActions" :key="ca.id"
                            :class="{ 'row-overdue': ca.isOverdue }">
                            <td class="text-center">{{ ca.id }}</td>
                            <td class="text-center">{{ ca.auditId }}</td>
                            <td>{{ ca.description }}</td>
                            <td>{{ ca.assignedTo ?? '—' }}</td>
                            <td class="text-center">{{ ca.dueDate ?? '—' }}</td>
                            <td class="text-center font-bold">{{ ca.daysOpen }}</td>
                            <td class="text-center">{{ ca.isOverdue ? 'OVERDUE' : ca.status }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Audit list -->
            <div class="section-block">
                <h2 class="section-title">Audit Log</h2>
                <table class="summary-table">
                    <thead>
                        <tr>
                            <th style="width:50px">#</th>
                            <th style="width:70px">Division</th>
                            <th style="width:90px">Date</th>
                            <th>Auditor</th>
                            <th>Job # / Location</th>
                            <th style="width:70px">Score</th>
                            <th style="width:50px">NCs</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="row in report.rows" :key="row.id">
                            <td class="text-center">{{ row.id }}</td>
                            <td class="text-center">{{ row.divisionCode }}</td>
                            <td class="text-center">{{ row.auditDate ?? '—' }}</td>
                            <td>{{ row.auditor ?? '—' }}</td>
                            <td>{{ [row.jobNumber, row.location].filter(Boolean).join(' · ') || '—' }}</td>
                            <td class="text-center font-bold">
                                {{ row.scorePercent != null ? `${row.scorePercent}%` : '—' }}
                            </td>
                            <td class="text-center">{{ row.nonConformingCount }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Signature block -->
            <div class="sig-block">
                <div class="sig-line">
                    <span class="sig-label">Prepared By</span>
                    <div class="sig-space"></div>
                </div>
                <div class="sig-line">
                    <span class="sig-label">Reviewed By</span>
                    <div class="sig-space"></div>
                </div>
                <div class="sig-line">
                    <span class="sig-label">Date</span>
                    <div class="sig-space"></div>
                </div>
            </div>
        </template>

        <div v-else-if="!loading" class="loading-msg">No audit data for this period.</div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type AuditReportDto, type DivisionDto } from '@/apiclient/auditClient';

const route = useRoute();
const apiStore = useApiStore();
const pageEl = ref<HTMLElement | null>(null);

const loading = ref(false);
const report = ref<AuditReportDto | null>(null);
const divisions = ref<DivisionDto[]>([]);

// Picker state (pre-seeded from query params)
const now = new Date();
const selectedDivisionId = ref<number | ''>(Number(route.query.divisionId) || '');
const selectedYear = ref(Number(route.query.year) || now.getFullYear());
const selectedQuarter = ref(Number(route.query.quarter) || Math.ceil((now.getMonth() + 1) / 3));

const yearOptions = computed(() => {
    const cur = now.getFullYear();
    return [cur, cur - 1, cur - 2, cur - 3];
});

function quarterDateRange(year: number, q: number) {
    const startMonth = (q - 1) * 3 + 1;
    const endMonth = startMonth + 2;
    const pad = (n: number) => String(n).padStart(2, '0');
    const daysInMonth = (m: number) => new Date(year, m, 0).getDate();
    return {
        from: `${year}-${pad(startMonth)}-01`,
        to: `${year}-${pad(endMonth)}-${daysInMonth(endMonth)}`,
    };
}

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}

async function loadData() {
    loading.value = true;
    try {
        const { from, to } = quarterDateRange(selectedYear.value, selectedQuarter.value);
        report.value = await getClient().getAuditReport(
            selectedDivisionId.value || null,
            null,
            from,
            to,
        );
    } finally {
        loading.value = false;
    }
}

onMounted(async () => {
    divisions.value = await getClient().getDivisions();
    await loadData();
});

const todayStr = now.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });

const quarterLabel = computed(() => {
    const qNames = ['Q1 (Jan–Mar)', 'Q2 (Apr–Jun)', 'Q3 (Jul–Sep)', 'Q4 (Oct–Dec)'];
    return `${selectedYear.value} ${qNames[selectedQuarter.value - 1] ?? ''}`;
});

const divisionLabel = computed(() => {
    if (!selectedDivisionId.value) return 'All Divisions';
    const d = divisions.value.find(x => x.id === selectedDivisionId.value);
    return d ? `${d.code} — ${d.name}` : 'Division';
});

const scoreClass = computed(() => {
    const pct = report.value?.avgScorePercent;
    if (pct == null) return '';
    if (pct >= 90) return 'kpi-green';
    if (pct >= 75) return 'kpi-amber';
    return 'kpi-red';
});

const correctedOnSitePct = computed(() => {
    if (!report.value || report.value.totalNonConforming === 0) return 0;
    return Math.round(report.value.correctedOnSiteCount / report.value.totalNonConforming * 100);
});

const openCaCount = computed(() => report.value?.openCorrectiveActions.length ?? 0);

const sectionRows = computed(() => {
    const bd = report.value?.sectionBreakdown ?? [];
    const total = bd.reduce((s, x) => s + x.ncCount, 0);
    return bd.map(x => ({
        sectionName: x.sectionName,
        ncCount: x.ncCount,
        pct: total > 0 ? Math.round(x.ncCount / total * 100) : 0,
    }));
});

const auditorRows = computed(() => {
    const map = new Map<string, { total: number; scores: number[]; ncs: number; warnings: number }>();
    for (const row of report.value?.rows ?? []) {
        const key = row.auditor?.trim() || 'Unknown';
        if (!map.has(key)) map.set(key, { total: 0, scores: [], ncs: 0, warnings: 0 });
        const e = map.get(key)!;
        e.total++;
        if (row.scorePercent != null) e.scores.push(row.scorePercent);
        e.ncs += row.nonConformingCount;
        e.warnings += row.warningCount;
    }
    return Array.from(map.entries()).map(([auditor, s]) => ({
        auditor,
        count: s.total,
        avgScore: s.scores.length ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
        ncs: s.ncs,
        warnings: s.warnings,
    })).sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

function printPage() {
    window.print();
}
</script>

<style scoped>
.summary-page {
    font-family: Arial, sans-serif;
    font-size: 11px;
    color: #000;
    background: #fff;
    max-width: 1100px;
    margin: 0 auto;
    padding: 16px;
}

/* Screen picker */
.picker {
    background: #1e293b;
    color: #f1f5f9;
    border-radius: 8px;
    padding: 16px 20px;
    margin-bottom: 20px;
}
.picker h2 { font-size: 16px; font-weight: bold; margin: 0 0 12px; }
.picker-fields { display: flex; gap: 16px; align-items: flex-end; flex-wrap: wrap; }
.picker-fields label { display: flex; flex-direction: column; gap: 4px; font-size: 12px; color: #94a3b8; }
.picker-fields select {
    background: #0f172a; color: #f1f5f9; border: 1px solid #334155;
    border-radius: 4px; padding: 6px 10px; font-size: 13px;
}
.print-btn {
    background: #2563eb; color: #fff; border: none; border-radius: 4px;
    padding: 8px 16px; font-size: 13px; cursor: pointer;
}
.print-btn:hover { background: #1d4ed8; }

/* Report header */
.report-header { border: 2px solid #1a3a5c; border-radius: 4px; margin-bottom: 12px; overflow: hidden; }
.report-header-top { background: #1a3a5c; color: #fff; padding: 10px 14px; }
.report-header-top h1 { font-size: 16px; font-weight: bold; margin: 0; }
.report-subtitle { font-size: 11px; margin: 2px 0 0; opacity: 0.85; }

/* KPI grid */
.kpi-grid { display: grid; grid-template-columns: repeat(6, 1fr); gap: 8px; margin-bottom: 12px; }
.kpi-box { border: 1px solid #ccc; border-radius: 4px; padding: 8px; text-align: center; }
.kpi-value { font-size: 22px; font-weight: bold; color: #000; }
.kpi-label { font-size: 9px; color: #555; text-transform: uppercase; letter-spacing: 0.5px; margin-top: 2px; }
.kpi-green { color: #059669; }
.kpi-amber { color: #d97706; }
.kpi-red { color: #dc2626; }

/* Section blocks */
.section-block { margin-bottom: 14px; break-inside: avoid; }
.section-title {
    font-size: 12px; font-weight: bold; text-transform: uppercase;
    letter-spacing: 0.5px; background: #1a3a5c; color: #fff;
    padding: 4px 8px; margin: 0 0 0; border-radius: 2px 2px 0 0;
}

/* Summary table */
.summary-table { width: 100%; border-collapse: collapse; border: 1px solid #ccc; }
.summary-table th {
    background: #e8edf2; border: 1px solid #bbb;
    padding: 3px 6px; font-size: 9px; font-weight: bold; text-align: left;
    text-transform: uppercase; letter-spacing: 0.4px;
}
.summary-table td { border: 1px solid #ddd; padding: 3px 6px; font-size: 10px; }
.col-section { width: auto; }
.col-num { width: 80px; text-align: center; }
.text-center { text-align: center; }
.font-bold { font-weight: bold; }
.text-red { color: #dc2626; }
.row-overdue td { background: #fff0f0; }

/* Signature block */
.sig-block {
    display: grid; grid-template-columns: 2fr 2fr 1fr; gap: 16px;
    margin-top: 16px; break-inside: avoid;
}
.sig-line { display: flex; flex-direction: column; gap: 2px; }
.sig-label { font-size: 9px; font-weight: bold; text-transform: uppercase; color: #555; }
.sig-space { border-bottom: 1px solid #999; min-height: 22px; }

.loading-msg { padding: 40px; text-align: center; color: #555; }

/* Print media */
@media screen {
    body { background: #334 !important; }
    .summary-page { box-shadow: 0 4px 20px rgba(0,0,0,0.4); border-radius: 4px; min-height: 100vh; }
}

@media print {
    .no-print { display: none !important; }
    body { margin: 0 !important; background: #fff !important; }
    .summary-page { padding: 6px 8px; box-shadow: none; }
    .section-block { break-inside: avoid; }
    .kpi-grid { grid-template-columns: repeat(6, 1fr); }
}
</style>

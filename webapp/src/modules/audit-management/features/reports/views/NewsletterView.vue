<template>
    <div class="newsletter-page">

        <!-- ── Toolbar ─────────────────────────────────────────────────────── -->
        <div class="toolbar no-print">
            <div class="toolbar-left">
                <div class="toolbar-title">Compliance Newsletter</div>
                <div class="toolbar-sub">Publication-quality division report — data-driven, editable narrative</div>
            </div>
            <div class="toolbar-controls">
                <label class="ctrl-label">
                    Division
                    <select v-model="selectedDivisionId" @change="onDivisionChange" class="ctrl-select">
                        <option value="">All Divisions</option>
                        <option v-for="d in divisions" :key="d.id" :value="d.id">{{ d.code }} — {{ d.name }}</option>
                    </select>
                </label>
                <label class="ctrl-label">
                    Year
                    <select v-model="selectedYear" @change="onQuarterChange" class="ctrl-select w-20">
                        <option v-for="y in yearOptions" :key="y" :value="y">{{ y }}</option>
                    </select>
                </label>
                <label class="ctrl-label">
                    Quarter
                    <select v-model="selectedQuarter" @change="onQuarterChange" class="ctrl-select w-16">
                        <option :value="1">Q1</option>
                        <option :value="2">Q2</option>
                        <option :value="3">Q3</option>
                        <option :value="4">Q4</option>
                    </select>
                </label>
                <span class="ctrl-sep">or</span>
                <label class="ctrl-label">
                    From
                    <input type="date" v-model="dateFrom" @change="onDateRangeChange" class="ctrl-input" />
                </label>
                <label class="ctrl-label">
                    To
                    <input type="date" v-model="dateTo" @change="onDateRangeChange" class="ctrl-input" />
                </label>
                <div class="toolbar-actions">
                    <button class="tb-btn secondary" @click="router.push('/audit-management/reports')">← Back</button>
                    <button class="tb-btn secondary" @click="autoDraft">Auto-Draft</button>
                    <button
                        class="tb-btn"
                        :class="editMode ? 'tb-btn-done' : 'tb-btn-edit'"
                        @click="toggleEditMode"
                    >
                        <i :class="editMode ? 'pi pi-eye' : 'pi pi-pencil'" />
                        {{ editMode ? 'Preview' : 'Edit Content' }}
                    </button>
                    <button class="tb-btn primary" @click="printPage">
                        <i class="pi pi-file-pdf" /> Print / PDF
                    </button>
                </div>
            </div>
        </div>

        <!-- ── Edit Mode Banner ────────────────────────────────────────────── -->
        <div v-if="editMode" class="edit-banner no-print">
            <i class="pi pi-pencil-square" />
            <span>You are editing narrative content. Charts and KPI data are locked.</span>
            <span v-if="savedAt" class="save-indicator"><i class="pi pi-check-circle" /> Saved {{ savedAt }}</span>
            <button class="tb-btn-done small" @click="toggleEditMode">Done Editing</button>
        </div>

        <div v-if="loading" class="nl-loading">
            <i class="pi pi-spin pi-spinner" /> Loading newsletter data…
        </div>

        <template v-else-if="report">

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 1: COVER                                               -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section class="nl-cover">
                <div class="nl-cover-main">
                    <div class="nl-cover-eyebrow">
                        <span class="eyebrow-brand">Stronghold Companies</span>
                        <span class="eyebrow-dot">·</span>
                        <span>Compliance &amp; Safety Newsletter</span>
                    </div>
                    <h1 class="nl-cover-headline">{{ divisionLabel }}</h1>
                    <div class="nl-cover-meta">
                        {{ periodLabel }}
                        <span class="meta-sep">·</span>
                        Generated {{ todayLabel }}
                    </div>
                    <div class="nl-cover-rule"></div>
                    <div class="nl-cover-tagline">{{ report.totalAudits }} Audit{{ report.totalAudits !== 1 ? 's' : '' }} Reviewed &nbsp;·&nbsp; {{ report.totalNonConforming }} Non-Conformances &nbsp;·&nbsp; {{ report.openCorrectiveActions.length }} Open Actions</div>
                </div>
                <div class="nl-cover-sidebar">
                    <div class="sidebar-label">In This Issue</div>
                    <ul class="sidebar-toc">
                        <li><span class="toc-num">01</span> KPI Summary</li>
                        <li><span class="toc-num">02</span> Findings by Category</li>
                        <li v-if="trendSections.length"><span class="toc-num">03</span> Trend Analysis</li>
                        <li><span class="toc-num">04</span> Recognition</li>
                        <li><span class="toc-num">05</span> Key Insights</li>
                        <li><span class="toc-num">06</span> Executive Summary</li>
                        <li v-if="report.openCorrectiveActions.length"><span class="toc-num">07</span> Corrective Actions</li>
                        <li v-if="auditorRows.length"><span class="toc-num">08</span> Auditor Performance</li>
                    </ul>
                </div>
            </section>

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 2: KPI SUMMARY                                         -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section class="nl-section">
                <div class="nl-section-head">
                    <span class="section-num">01</span>
                    <h2>KPI Summary</h2>
                    <span class="data-badge no-print">System Data — Read Only</span>
                </div>
                <div class="kpi-grid">
                    <div class="kpi-tile" :class="kpiVariant(report.totalAudits, 'neutral')">
                        <div class="kpi-num">{{ report.totalAudits }}</div>
                        <div class="kpi-lbl">Total Audits</div>
                    </div>
                    <div class="kpi-tile" :class="scoreVariant">
                        <div class="kpi-num">{{ report.avgScorePercent != null ? Math.round(report.avgScorePercent) + '%' : '—' }}</div>
                        <div class="kpi-lbl">Avg Conformance</div>
                        <div v-if="companyAvg != null" class="kpi-comp">Co. avg: {{ Math.round(companyAvg) }}%</div>
                    </div>
                    <div class="kpi-tile kpi-danger">
                        <div class="kpi-num">{{ report.totalNonConforming }}</div>
                        <div class="kpi-lbl">Non-Conformances</div>
                    </div>
                    <div class="kpi-tile kpi-warn">
                        <div class="kpi-num">{{ report.totalWarnings }}</div>
                        <div class="kpi-lbl">Warnings</div>
                    </div>
                    <div class="kpi-tile" :class="correctedOnSitePct >= 80 ? 'kpi-good' : 'kpi-warn'">
                        <div class="kpi-num">{{ correctedOnSitePct }}%</div>
                        <div class="kpi-lbl">Corrected On Site</div>
                    </div>
                    <div class="kpi-tile" :class="report.openCorrectiveActions.length === 0 ? 'kpi-good' : overdueCount > 0 ? 'kpi-danger' : 'kpi-warn'">
                        <div class="kpi-num">{{ report.openCorrectiveActions.length }}</div>
                        <div class="kpi-lbl">Open CAs</div>
                        <div v-if="overdueCount > 0" class="kpi-comp kpi-comp-danger">{{ overdueCount }} overdue</div>
                    </div>
                </div>
            </section>

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 3: FINDINGS BY CATEGORY                                -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section class="nl-section">
                <div class="nl-section-head">
                    <span class="section-num">02</span>
                    <h2>Findings by Category</h2>
                    <span class="data-badge no-print">Chart Data — Read Only</span>
                </div>

                <div v-if="findingsRows.length === 0" class="no-data-notice">
                    No findings data for this period. Generate data using the filters above.
                </div>

                <div v-else class="findings-layout">
                    <div class="findings-chart-area">
                        <p class="chart-caption">Non-conformances per audit — {{ divisionLabel }} vs company-wide.</p>
                        <Chart type="bar" :data="findingsChartData" :options="barChartOptions" style="height:260px;" />
                    </div>
                    <div class="findings-insights">
                        <div class="insights-header">
                            Category Insights
                            <span v-if="editMode" class="editable-tag">editable</span>
                        </div>
                        <div v-for="row in findingsRows.slice(0, 6)" :key="row.sectionName" class="insight-row">
                            <div class="insight-name">{{ row.sectionName }}</div>
                            <template v-if="editMode">
                                <textarea
                                    v-model="content.categoryNotes[row.sectionName]"
                                    class="insight-input"
                                    rows="2"
                                    :placeholder="`Describe the ${row.ncCount} finding${row.ncCount !== 1 ? 's' : ''} in this area — contributing factors, patterns, or corrective steps taken.`"
                                />
                            </template>
                            <template v-else>
                                <p class="insight-text">
                                    {{ content.categoryNotes[row.sectionName] || autoInsight(row) }}
                                </p>
                            </template>
                        </div>
                    </div>
                </div>
            </section>

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 4: TREND ANALYSIS                                      -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section v-if="trendSections.length" class="nl-section">
                <div class="nl-section-head">
                    <span class="section-num">03</span>
                    <h2>Trend Analysis</h2>
                    <span class="data-badge no-print">Chart Data — Read Only</span>
                </div>
                <p class="chart-caption">Quarterly findings-per-audit trend by category — division vs. company-wide average.</p>
                <div class="trend-grid">
                    <div v-for="sec in trendSections" :key="sec.sectionName" class="trend-card">
                        <div class="trend-card-title">{{ sec.sectionName }}</div>
                        <Chart type="line" :data="sec.chartData" :options="lineChartOptions" style="height:180px;" />
                    </div>
                </div>
            </section>

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 5: RECOGNITION                                         -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section class="nl-section recognition-section">
                <div class="nl-section-head">
                    <span class="section-num">{{ trendSections.length ? '04' : '03' }}</span>
                    <h2>Recognition</h2>
                    <span v-if="editMode" class="editable-tag large no-print">editable</span>
                </div>

                <!-- Edit mode -->
                <div v-if="editMode" class="recognition-edit">
                    <div class="rec-photo-upload" @click="triggerPhotoUpload" :class="{ 'has-photo': !!recognitionImage }">
                        <img v-if="recognitionImage" :src="recognitionImage" alt="Recognition photo" class="rec-photo-thumb" />
                        <div v-else class="rec-upload-placeholder">
                            <i class="pi pi-camera" />
                            <span>Upload Photo</span>
                            <span class="rec-upload-hint">Team, site, or individual</span>
                        </div>
                        <input ref="photoInputRef" type="file" accept="image/*" class="hidden" @change="onPhotoSelect" />
                        <button v-if="recognitionImage" class="rec-photo-clear" @click.stop="recognitionImage = null">
                            <i class="pi pi-times" />
                        </button>
                    </div>
                    <div class="rec-fields">
                        <label class="rec-field-label">Title / Award Name
                            <input v-model="content.recognitionTitle" type="text" class="rec-input"
                                placeholder="e.g. Q1 Safety Excellence Award — ETS Division" />
                        </label>
                        <div class="rec-row-2">
                            <label class="rec-field-label flex-1">Site / Team
                                <input v-model="content.recognitionSite" type="text" class="rec-input"
                                    placeholder="e.g. Chambers County, TX" />
                            </label>
                            <label class="rec-field-label w-36">Date
                                <input v-model="content.recognitionDate" type="date" class="rec-input" />
                            </label>
                        </div>
                        <label class="rec-field-label">Narrative
                            <textarea v-model="content.recognitionNarrative" class="rec-textarea" rows="4"
                                placeholder="Describe the achievement, the people involved, and why this recognition matters. What behaviors or results stood out? What can others learn from this example?" />
                        </label>
                    </div>
                </div>

                <!-- Preview mode -->
                <div v-else class="recognition-preview">
                    <div v-if="recognitionImage || content.recognitionTitle || content.recognitionNarrative" class="rec-card">
                        <div class="rec-card-photo">
                            <img v-if="recognitionImage" :src="recognitionImage" alt="Recognition photo" class="rec-img" />
                            <div v-else class="rec-img-placeholder">
                                <i class="pi pi-star text-amber-400 text-3xl" />
                            </div>
                        </div>
                        <div class="rec-card-body">
                            <div v-if="content.recognitionTitle" class="rec-card-title">{{ content.recognitionTitle }}</div>
                            <div class="rec-card-meta">
                                <span v-if="content.recognitionSite">{{ content.recognitionSite }}</span>
                                <span v-if="content.recognitionSite && content.recognitionDate"> · </span>
                                <span v-if="content.recognitionDate">{{ formatRecDate(content.recognitionDate) }}</span>
                            </div>
                            <div v-if="content.recognitionNarrative" class="rec-card-narrative">{{ content.recognitionNarrative }}</div>
                            <div v-else class="empty-field-hint no-print">No narrative written. Click <strong>Edit Content</strong> to add.</div>
                        </div>
                    </div>
                    <div v-else class="rec-empty">
                        <i class="pi pi-star text-2xl text-slate-600" />
                        <p>No recognition written yet.</p>
                        <p class="rec-empty-hint">Click <strong>Edit Content</strong> to add a photo, title, and narrative for this period's recognition spotlight.</p>
                    </div>
                </div>
            </section>

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 6: KEY INSIGHTS                                        -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section class="nl-section">
                <div class="nl-section-head">
                    <span class="section-num">{{ sectionNum(5) }}</span>
                    <h2>Key Insights</h2>
                    <span v-if="editMode" class="editable-tag large no-print">editable</span>
                </div>
                <div v-if="editMode">
                    <div class="guided-hint">One insight per line. Start each with "•" for bullets, or write full sentences. Focus on patterns, risks, and positive behaviors.</div>
                    <textarea
                        v-model="content.keyInsights"
                        class="nl-textarea"
                        rows="7"
                        placeholder="• [Positive trend] — Corrective action closure rate improved to 92% this quarter.&#10;• [Risk area] — PPE compliance remains below target in two sites; follow-up scheduled.&#10;• [Pattern] — Repeat findings in Equipment Inspection suggest need for refresher training.&#10;• [Highlight] — Zero life-safety violations recorded for the third consecutive quarter."
                    />
                </div>
                <div v-else>
                    <ul v-if="content.keyInsights" class="insights-list">
                        <li v-for="(line, i) in parsedInsights" :key="i">{{ line }}</li>
                    </ul>
                    <div v-else class="nl-empty-state">
                        <i class="pi pi-lightbulb" />
                        <div>
                            <strong>No key insights written.</strong>
                            <span> Highlight major trends, risks, and improvements across this period. Use <strong>Edit Content</strong> to add them.</span>
                        </div>
                    </div>
                </div>
            </section>

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 7: EXECUTIVE SUMMARY                                   -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section class="nl-section">
                <div class="nl-section-head">
                    <span class="section-num">{{ sectionNum(6) }}</span>
                    <h2>Executive Summary</h2>
                    <span v-if="editMode" class="editable-tag large no-print">editable</span>
                </div>
                <div v-if="editMode">
                    <div class="guided-hint">Summarize overall performance for this period — conformance level, notable findings, corrective action status, and management priorities. Use <strong>Auto-Draft</strong> to generate a starting point from data.</div>
                    <textarea
                        v-model="content.executiveSummary"
                        class="nl-textarea"
                        rows="6"
                        placeholder="Summarize the division's overall compliance performance for this period. Include: total audit activity, conformance trend, significant findings, corrective action status, and any management actions taken or planned."
                    />
                </div>
                <div v-else>
                    <p v-if="content.executiveSummary" class="nl-prose">{{ content.executiveSummary }}</p>
                    <div v-else class="nl-empty-state">
                        <i class="pi pi-align-left" />
                        <div>
                            <strong>No executive summary written.</strong>
                            <span> Use <strong>Auto-Draft</strong> to generate a data-driven draft, then refine it in <strong>Edit Content</strong> mode.</span>
                        </div>
                    </div>
                </div>
            </section>

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 8: OPEN CORRECTIVE ACTIONS (data only)                 -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section v-if="report.openCorrectiveActions.length" class="nl-section">
                <div class="nl-section-head">
                    <span class="section-num">{{ sectionNum(7) }}</span>
                    <h2>Open Corrective Actions</h2>
                    <span class="data-badge no-print">System Data — Read Only</span>
                </div>
                <table class="nl-table">
                    <thead>
                        <tr>
                            <th>CA #</th>
                            <th>Description</th>
                            <th>Assigned To</th>
                            <th>Due Date</th>
                            <th>Days Open</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="ca in report.openCorrectiveActions" :key="ca.id" :class="{ 'tr-overdue': ca.isOverdue }">
                            <td class="mono">{{ ca.id }}</td>
                            <td>{{ ca.description }}</td>
                            <td>{{ ca.assignedTo ?? '—' }}</td>
                            <td class="mono">{{ ca.dueDate ?? '—' }}</td>
                            <td class="mono" :class="{ 'td-danger': ca.daysOpen >= 30 }">{{ ca.daysOpen }}d</td>
                            <td :class="ca.isOverdue ? 'td-danger font-bold' : ''">{{ ca.isOverdue ? 'OVERDUE' : ca.status }}</td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <!-- ══════════════════════════════════════════════════════════════ -->
            <!-- SECTION 9: AUDITOR PERFORMANCE (data only)                     -->
            <!-- ══════════════════════════════════════════════════════════════ -->
            <section v-if="auditorRows.length" class="nl-section">
                <div class="nl-section-head">
                    <span class="section-num">{{ sectionNum(8) }}</span>
                    <h2>Auditor Performance</h2>
                    <span class="data-badge no-print">System Data — Read Only</span>
                </div>
                <table class="nl-table">
                    <thead>
                        <tr>
                            <th>Auditor</th>
                            <th>Audits</th>
                            <th>Avg Score</th>
                            <th>Non-Conformances</th>
                            <th>Warnings</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="row in auditorRows" :key="row.auditor">
                            <td>{{ row.auditor }}</td>
                            <td class="mono">{{ row.count }}</td>
                            <td class="mono" :class="row.avgScore != null && row.avgScore >= 90 ? 'td-good' : row.avgScore != null && row.avgScore < 75 ? 'td-danger' : ''">
                                {{ row.avgScore != null ? row.avgScore + '%' : '—' }}
                            </td>
                            <td class="mono">{{ row.ncs }}</td>
                            <td class="mono">{{ row.warnings }}</td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <!-- ── Signature ─────────────────────────────────────────────── -->
            <div class="nl-signature">
                <div class="sig-col"><span class="sig-label">Prepared By</span><div class="sig-line"></div></div>
                <div class="sig-col"><span class="sig-label">Reviewed By</span><div class="sig-line"></div></div>
                <div class="sig-col"><span class="sig-label">Date</span><div class="sig-line"></div></div>
            </div>

        </template>

        <div v-else class="nl-loading">No report data for this period. Select a division and date range above.</div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import type { AuditReportDto, DivisionDto, SectionTrendsReportDto } from '@/apiclient/auditClient';

const router  = useRouter();
const service = useAuditService();

// ── Data ───────────────────────────────────────────────────────────────────────
const loading         = ref(false);
const report          = ref<AuditReportDto | null>(null);
const coReport        = ref<AuditReportDto | null>(null);
const sectionTrends   = ref<SectionTrendsReportDto | null>(null);
const divisions       = ref<DivisionDto[]>([]);

// ── Filters ────────────────────────────────────────────────────────────────────
const now                = new Date();
const selectedDivisionId = ref<number | ''>('');
const selectedYear       = ref(now.getFullYear());
const selectedQuarter    = ref(Math.ceil((now.getMonth() + 1) / 3));

const yearOptions = computed(() => {
    const y = now.getFullYear();
    return [y, y - 1, y - 2, y - 3];
});

function quarterRange(year: number, q: number) {
    const sm = (q - 1) * 3 + 1;
    const em = sm + 2;
    const pad = (n: number) => String(n).padStart(2, '0');
    const dim = (m: number) => new Date(year, m, 0).getDate();
    return { from: `${year}-${pad(sm)}-01`, to: `${year}-${pad(em)}-${dim(em)}` };
}

const { from: initFrom, to: initTo } = quarterRange(selectedYear.value, selectedQuarter.value);
const dateFrom = ref(initFrom);
const dateTo   = ref(initTo);

// ── Edit mode ──────────────────────────────────────────────────────────────────
const editMode = ref(false);
const savedAt  = ref('');

function toggleEditMode() {
    editMode.value = !editMode.value;
}

// ── Editable content (auto-persisted to localStorage per period) ───────────────
const content = reactive({
    executiveSummary:    '',
    keyInsights:         '',
    recognitionTitle:    '',
    recognitionSite:     '',
    recognitionDate:     '',
    recognitionNarrative: '',
    categoryNotes:       {} as Record<string, string>,
});

const recognitionImage = ref<string | null>(null);
const photoInputRef    = ref<HTMLInputElement | null>(null);

function storageKey() {
    return `stronghold_nl_${selectedDivisionId.value}_${dateFrom.value}_${dateTo.value}`;
}

function saveContent() {
    const key = storageKey();
    localStorage.setItem(key, JSON.stringify({
        executiveSummary:     content.executiveSummary,
        keyInsights:          content.keyInsights,
        recognitionTitle:     content.recognitionTitle,
        recognitionSite:      content.recognitionSite,
        recognitionDate:      content.recognitionDate,
        recognitionNarrative: content.recognitionNarrative,
        categoryNotes:        content.categoryNotes,
    }));
    if (recognitionImage.value && recognitionImage.value.length < 600_000) {
        localStorage.setItem(key + '_photo', recognitionImage.value);
    }
    const t = new Date();
    savedAt.value = t.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function loadContent() {
    const key   = storageKey();
    const saved = localStorage.getItem(key);
    if (saved) {
        const parsed = JSON.parse(saved);
        content.executiveSummary     = parsed.executiveSummary    ?? '';
        content.keyInsights          = parsed.keyInsights         ?? '';
        content.recognitionTitle     = parsed.recognitionTitle    ?? '';
        content.recognitionSite      = parsed.recognitionSite     ?? '';
        content.recognitionDate      = parsed.recognitionDate     ?? '';
        content.recognitionNarrative = parsed.recognitionNarrative ?? '';
        content.categoryNotes        = parsed.categoryNotes       ?? {};
    } else {
        content.executiveSummary     = '';
        content.keyInsights          = '';
        content.recognitionTitle     = '';
        content.recognitionSite      = '';
        content.recognitionDate      = '';
        content.recognitionNarrative = '';
        content.categoryNotes        = {};
    }
    recognitionImage.value = localStorage.getItem(key + '_photo') ?? null;
    savedAt.value = '';
}

// Debounced auto-save whenever editable content changes
let saveTimer: ReturnType<typeof setTimeout> | null = null;
watch(() => JSON.stringify({ ...content }), () => {
    if (saveTimer) clearTimeout(saveTimer);
    saveTimer = setTimeout(saveContent, 600);
});
watch(recognitionImage, () => {
    if (saveTimer) clearTimeout(saveTimer);
    saveTimer = setTimeout(saveContent, 600);
});

// ── Photo upload ───────────────────────────────────────────────────────────────
function triggerPhotoUpload() {
    photoInputRef.value?.click();
}

function onPhotoSelect(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = ev => { recognitionImage.value = ev.target?.result as string ?? null; };
    reader.readAsDataURL(file);
}

// ── Display helpers ────────────────────────────────────────────────────────────
const todayLabel  = now.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });

const periodLabel = computed(() => {
    if (!dateFrom.value || !dateTo.value) return `${selectedYear.value} Q${selectedQuarter.value}`;
    const { from, to } = quarterRange(selectedYear.value, selectedQuarter.value);
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
    return d ? `${d.code} — ${d.name}` : 'Division';
});

const correctedOnSitePct = computed(() => {
    if (!report.value || report.value.totalNonConforming === 0) return 0;
    return Math.round(report.value.correctedOnSiteCount / report.value.totalNonConforming * 100);
});

const overdueCount = computed(() =>
    report.value?.openCorrectiveActions.filter(x => x.isOverdue).length ?? 0,
);

const companyAvg = computed(() => coReport.value?.avgScorePercent ?? null);

const scoreVariant = computed(() => {
    const s = report.value?.avgScorePercent;
    if (s == null) return 'kpi-neutral';
    if (s >= 90) return 'kpi-good';
    if (s >= 75) return 'kpi-warn';
    return 'kpi-danger';
});

function kpiVariant(_: number, v: 'neutral' | 'good' | 'warn' | 'danger') {
    return `kpi-${v}`;
}

function formatRecDate(d: string) {
    if (!d) return '';
    return new Date(d + 'T00:00:00').toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
}

// Dynamic section numbering (shifts when optional data sections appear)
function sectionNum(base: number): string {
    const offset = trendSections.value.length > 0 ? 0 : -1;
    return String(Math.max(1, base + offset)).padStart(2, '0');
}

// ── KPI data ───────────────────────────────────────────────────────────────────
const selectedQuarterKey = computed(() => `${selectedYear.value} Q${selectedQuarter.value}`);
const showCompany        = computed(() => !!selectedDivisionId.value);

const findingsRows = computed(() => {
    const total = report.value?.totalAudits ?? 0;
    if (total === 0) return [];
    const qKey  = selectedQuarterKey.value;
    const tMap  = new Map<string, number>();
    for (const s of sectionTrends.value?.sections ?? []) {
        const pt = s.companyTrend.find(p => p.quarter === qKey);
        if (pt) tMap.set(s.sectionName, pt.findingsPerAudit);
    }
    return (report.value?.sectionBreakdown ?? [])
        .map(s => ({
            sectionName:  s.sectionName,
            ncCount:      s.ncCount,
            divRate:      Number((s.ncCount / total).toFixed(2)),
            coRate:       Number((tMap.get(s.sectionName) ?? 0).toFixed(2)),
        }))
        .sort((a, b) => b.divRate - a.divRate);
});

function autoInsight(row: { sectionName: string; ncCount: number; divRate: number }) {
    if (row.ncCount === 0) return `No findings recorded in ${row.sectionName} this period.`;
    return `${row.ncCount} non-conformance${row.ncCount > 1 ? 's' : ''} recorded (${row.divRate.toFixed(2)} per audit). Review root causes and corrective steps.`;
}

function shortName(name: string, max = 20) {
    return name.length > max ? name.slice(0, max - 1) + '…' : name;
}

const findingsChartData = computed(() => ({
    labels: findingsRows.value.map(r => shortName(r.sectionName)),
    datasets: [
        {
            label: showCompany.value ? divisionLabel.value : 'Findings / Audit',
            data: findingsRows.value.map(r => r.divRate),
            backgroundColor: 'rgba(37,99,235,0.75)',
            borderColor: '#2563eb',
            borderWidth: 1,
            borderRadius: 4,
        },
        ...(showCompany.value ? [{
            label: 'Company Wide',
            data: findingsRows.value.map(r => r.coRate),
            backgroundColor: 'rgba(245,158,11,0.75)',
            borderColor: '#f59e0b',
            borderWidth: 1,
            borderRadius: 4,
        }] : []),
    ],
}));

const barChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { labels: { color: '#cbd5e1', font: { size: 11 } } } },
    scales: {
        y: { beginAtZero: true, ticks: { color: '#94a3b8', font: { size: 10 } }, grid: { color: 'rgba(100,116,139,0.2)' } },
        x: { ticks: { color: '#94a3b8', font: { size: 10 } }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
};

// ── Trend charts ───────────────────────────────────────────────────────────────
const trendSections = computed(() => {
    const trends = sectionTrends.value;
    if (!trends?.sections.length) return [];
    return trends.sections.map(sec => ({
        sectionName: sec.sectionName,
        chartData: {
            labels: trends.quarters,
            datasets: [
                {
                    label: divisionLabel.value,
                    data: sec.divisionTrend.map(p => Number(p.findingsPerAudit.toFixed(2))),
                    borderColor: '#2563eb',
                    backgroundColor: 'rgba(37,99,235,0.10)',
                    borderWidth: 2,
                    pointRadius: 3,
                    tension: 0.35,
                    fill: false,
                },
                ...(showCompany.value ? [{
                    label: 'Company Wide',
                    data: sec.companyTrend.map(p => Number(p.findingsPerAudit.toFixed(2))),
                    borderColor: '#f59e0b',
                    backgroundColor: 'rgba(245,158,11,0.08)',
                    borderWidth: 2,
                    pointRadius: 3,
                    tension: 0.35,
                    fill: false,
                    borderDash: [5, 3],
                }] : []),
            ],
        },
    }));
});

const lineChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { labels: { color: '#cbd5e1', font: { size: 10 } } } },
    scales: {
        y: { beginAtZero: true, ticks: { color: '#94a3b8', font: { size: 10 } }, grid: { color: 'rgba(100,116,139,0.2)' } },
        x: { ticks: { color: '#94a3b8', font: { size: 10 } }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
};

// ── Auditor rows ───────────────────────────────────────────────────────────────
const auditorRows = computed(() => {
    const map = new Map<string, { total: number; scores: number[]; ncs: number; warnings: number }>();
    for (const row of report.value?.rows ?? []) {
        const key = row.auditor?.trim() || 'Unknown';
        if (!map.has(key)) map.set(key, { total: 0, scores: [], ncs: 0, warnings: 0 });
        const e = map.get(key)!;
        e.total++;
        if (row.scorePercent != null) e.scores.push(row.scorePercent);
        e.ncs      += row.nonConformingCount;
        e.warnings += row.warningCount;
    }
    return Array.from(map.entries())
        .map(([auditor, s]) => ({
            auditor,
            count:    s.total,
            avgScore: s.scores.length ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
            ncs:      s.ncs,
            warnings: s.warnings,
        }))
        .sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

// ── Key insights parsed ────────────────────────────────────────────────────────
const parsedInsights = computed(() =>
    content.keyInsights
        .split('\n')
        .map(l => l.replace(/^[•\-*]\s*/, '').trim())
        .filter(Boolean),
);

// ── Auto-draft ─────────────────────────────────────────────────────────────────
function autoDraft() {
    if (!report.value) return;
    const top3   = findingsRows.value.slice(0, 3).filter(r => r.ncCount > 0);
    const topStr = top3.length ? top3.map(r => r.sectionName).join(', ') : 'no materially elevated sections';
    const od     = overdueCount.value;
    content.executiveSummary =
        `${divisionLabel.value} completed ${report.value.totalAudits} audit${report.value.totalAudits !== 1 ? 's' : ''} in ${periodLabel.value} ` +
        `with an average conformance score of ${report.value.avgScorePercent != null ? Math.round(report.value.avgScorePercent) : 0}%. ` +
        `The period recorded ${report.value.totalNonConforming} non-conformance${report.value.totalNonConforming !== 1 ? 's' : ''} ` +
        `and ${report.value.totalWarnings} warning${report.value.totalWarnings !== 1 ? 's' : ''}, with ${correctedOnSitePct.value}% corrected on-site. ` +
        `Highest-finding categories were ${topStr}. ` +
        `There are ${report.value.openCorrectiveActions.length} open corrective action${report.value.openCorrectiveActions.length !== 1 ? 's' : ''}` +
        (od > 0 ? `, including ${od} overdue item${od > 1 ? 's' : ''} requiring immediate follow-up` : '') + '.';
    editMode.value = true;
}

// ── Load ───────────────────────────────────────────────────────────────────────
async function loadData() {
    loading.value = true;
    try {
        const [div, co, trends] = await Promise.all([
            service.getAuditReport(selectedDivisionId.value || null, null, dateFrom.value, dateTo.value),
            service.getAuditReport(null, null, dateFrom.value, dateTo.value),
            service.getSectionTrends(selectedDivisionId.value || null, dateFrom.value, dateTo.value),
        ]);
        report.value        = div;
        coReport.value      = co;
        sectionTrends.value = trends;
    } finally {
        loading.value = false;
    }
    loadContent();
}

function onDivisionChange() {
    loadData();
}

function onQuarterChange() {
    const { from, to } = quarterRange(selectedYear.value, selectedQuarter.value);
    dateFrom.value = from;
    dateTo.value   = to;
    loadData();
}

function onDateRangeChange() {
    loadData();
}

function printPage() { window.print(); }

onMounted(async () => {
    divisions.value = await service.getDivisions();
    await loadData();
});
</script>

<style scoped>
/* ── Page container ──────────────────────────────────────────────────────────── */
.newsletter-page {
    max-width: 1160px;
    margin: 0 auto;
    padding: 16px;
    color: #e2e8f0;
    font-family: 'Inter', system-ui, sans-serif;
}

/* ── Toolbar ─────────────────────────────────────────────────────────────────── */
.toolbar {
    display: flex;
    gap: 16px;
    align-items: flex-start;
    flex-wrap: wrap;
    background: linear-gradient(120deg, #0f172a, #1e293b);
    border: 1px solid #334155;
    border-radius: 10px;
    padding: 14px 16px;
    margin-bottom: 14px;
}
.toolbar-left    { flex-shrink: 0; }
.toolbar-title   { font-size: 18px; font-weight: 700; color: #f8fafc; }
.toolbar-sub     { font-size: 12px; color: #64748b; margin-top: 2px; }
.toolbar-controls { display: flex; flex-wrap: wrap; gap: 8px; align-items: flex-end; flex: 1; }
.ctrl-label {
    display: flex; flex-direction: column; gap: 3px;
    font-size: 11px; font-weight: 600; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em;
}
.ctrl-select, .ctrl-input {
    background: #0f172a; color: #f1f5f9; border: 1px solid #475569;
    border-radius: 5px; padding: 5px 8px; font-size: 12px; color-scheme: dark;
}
.ctrl-select:focus, .ctrl-input:focus { outline: none; border-color: #3b82f6; }
.w-20  { width: 5rem; }
.w-16  { width: 4rem; }
.w-36  { width: 9rem; }
.ctrl-sep { align-self: flex-end; padding-bottom: 6px; color: #334155; font-size: 11px; }
.toolbar-actions { display: flex; gap: 6px; align-items: flex-end; flex-wrap: wrap; }

.tb-btn {
    padding: 6px 12px; font-size: 12px; border-radius: 6px; cursor: pointer;
    border: 1px solid #475569; background: #1e293b; color: #cbd5e1;
    transition: all 0.15s; display: flex; align-items: center; gap: 5px;
}
.tb-btn:hover       { background: #334155; color: #f1f5f9; }
.tb-btn.primary     { background: #2563eb; border-color: #1d4ed8; color: #fff; font-weight: 600; }
.tb-btn.primary:hover { background: #1d4ed8; }
.tb-btn.secondary   { background: #0f172a; }
.tb-btn-edit        { background: #1e293b; border-color: #475569; color: #cbd5e1; }
.tb-btn-done        { background: #78350f; border-color: #92400e; color: #fde68a; font-weight: 600; }
.tb-btn-done.small  { padding: 4px 10px; font-size: 11px; }

/* ── Edit banner ─────────────────────────────────────────────────────────────── */
.edit-banner {
    display: flex; align-items: center; gap: 10px; flex-wrap: wrap;
    padding: 8px 14px;
    background: rgba(120, 53, 15, 0.35);
    border: 1px solid rgba(180, 83, 9, 0.5);
    border-radius: 8px;
    margin-bottom: 14px;
    font-size: 12px; color: #fbbf24;
}
.edit-banner i      { font-size: 13px; }
.save-indicator     { margin-left: auto; color: #6ee7b7; font-size: 11px; }

/* ── Loading ─────────────────────────────────────────────────────────────────── */
.nl-loading {
    text-align: center; padding: 48px; color: #64748b;
    background: #0f172a; border: 1px solid #1e293b; border-radius: 10px;
}

/* ── Cover ───────────────────────────────────────────────────────────────────── */
.nl-cover {
    display: flex; align-items: stretch;
    border-radius: 12px; overflow: hidden;
    margin-bottom: 14px;
    background:
        radial-gradient(circle at 75% 15%, rgba(37, 99, 235, 0.45), transparent 40%),
        radial-gradient(circle at 25% 85%, rgba(14, 116, 144, 0.4), transparent 40%),
        linear-gradient(150deg, #0b1628 0%, #0f2040 40%, #0b1628 100%);
    border: 1px solid rgba(37, 99, 235, 0.3);
    min-height: 200px;
}
.nl-cover-main {
    flex: 1; padding: 32px 28px;
    display: flex; flex-direction: column; justify-content: center;
}
.nl-cover-eyebrow {
    display: flex; align-items: center; gap: 8px;
    font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.15em; color: #93c5fd;
    margin-bottom: 14px;
}
.eyebrow-brand { color: #bfdbfe; }
.eyebrow-dot   { color: #334155; }
.nl-cover-headline {
    margin: 0; font-size: 38px; font-weight: 800; color: #f8fafc; line-height: 1.1;
    letter-spacing: -0.02em;
}
.nl-cover-meta    { margin-top: 10px; font-size: 13px; color: #94a3b8; }
.meta-sep         { margin: 0 8px; color: #334155; }
.nl-cover-rule    { width: 48px; height: 3px; background: #2563eb; border-radius: 2px; margin: 16px 0 12px; }
.nl-cover-tagline { font-size: 12px; color: #64748b; font-weight: 500; }

.nl-cover-sidebar {
    width: 210px; flex-shrink: 0;
    padding: 28px 18px;
    background: rgba(10, 18, 35, 0.6);
    border-left: 1px solid rgba(37, 99, 235, 0.2);
    backdrop-filter: blur(6px);
}
.sidebar-label {
    font-size: 9px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.15em;
    color: #2563eb; margin-bottom: 14px; padding-bottom: 8px;
    border-bottom: 1px solid rgba(37, 99, 235, 0.25);
}
.sidebar-toc { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 9px; }
.sidebar-toc li { display: flex; align-items: center; gap: 8px; font-size: 12px; color: #94a3b8; }
.toc-num {
    display: inline-flex; align-items: center; justify-content: center;
    width: 20px; height: 20px; border-radius: 50%;
    background: rgba(37, 99, 235, 0.15); border: 1px solid rgba(37, 99, 235, 0.3);
    font-size: 9px; font-weight: 700; color: #60a5fa; flex-shrink: 0;
}

/* ── Section wrapper ─────────────────────────────────────────────────────────── */
.nl-section {
    background: rgba(15, 23, 42, 0.8);
    border: 1px solid #1e293b;
    border-radius: 10px;
    padding: 18px 20px;
    margin-bottom: 12px;
}
.nl-section-head {
    display: flex; align-items: center; gap: 10px;
    margin-bottom: 14px;
    padding-bottom: 10px;
    border-bottom: 1px solid #1e293b;
}
.nl-section-head h2 { margin: 0; font-size: 16px; font-weight: 700; color: #f1f5f9; }
.section-num {
    display: inline-flex; align-items: center; justify-content: center;
    width: 24px; height: 24px; border-radius: 6px;
    background: rgba(37, 99, 235, 0.15); border: 1px solid rgba(37, 99, 235, 0.25);
    font-size: 10px; font-weight: 700; color: #60a5fa; flex-shrink: 0;
}
.data-badge {
    margin-left: auto; font-size: 10px; font-weight: 600; color: #475569;
    background: rgba(15, 23, 42, 0.8); border: 1px solid #1e293b;
    border-radius: 4px; padding: 2px 7px; letter-spacing: 0.03em;
}
.editable-tag {
    font-size: 10px; font-weight: 600; color: #fbbf24;
    background: rgba(120, 53, 15, 0.25); border: 1px solid rgba(180, 83, 9, 0.4);
    border-radius: 4px; padding: 2px 7px;
}
.editable-tag.large { margin-left: auto; }

/* ── KPI grid ────────────────────────────────────────────────────────────────── */
.kpi-grid {
    display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px;
}
.kpi-tile {
    background: #0b1628; border: 1px solid #1e293b;
    border-radius: 8px; padding: 14px; position: relative;
    border-left: 3px solid #1e293b;
}
.kpi-tile.kpi-good    { border-left-color: #059669; }
.kpi-tile.kpi-warn    { border-left-color: #d97706; }
.kpi-tile.kpi-danger  { border-left-color: #dc2626; }
.kpi-tile.kpi-neutral { border-left-color: #334155; }
.kpi-num  { font-size: 28px; font-weight: 800; color: #f8fafc; line-height: 1; }
.kpi-good .kpi-num    { color: #34d399; }
.kpi-warn .kpi-num    { color: #fbbf24; }
.kpi-danger .kpi-num  { color: #f87171; }
.kpi-lbl  { font-size: 11px; font-weight: 500; color: #64748b; margin-top: 5px; text-transform: uppercase; letter-spacing: 0.04em; }
.kpi-comp { font-size: 10px; color: #475569; margin-top: 3px; }
.kpi-comp-danger { color: #f87171; font-weight: 600; }

/* ── Findings ────────────────────────────────────────────────────────────────── */
.chart-caption { font-size: 12px; color: #64748b; margin: 0 0 10px; }
.no-data-notice { font-size: 13px; color: #475569; padding: 16px 0; font-style: italic; }
.findings-layout {
    display: grid; grid-template-columns: 1fr 280px; gap: 18px; align-items: start;
}
.findings-insights { display: flex; flex-direction: column; gap: 12px; }
.insights-header {
    display: flex; align-items: center; gap: 8px;
    font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.08em;
    color: #334155; margin-bottom: 2px;
}
.insight-row { display: flex; flex-direction: column; gap: 3px; }
.insight-name { font-size: 11px; font-weight: 600; color: #64748b; }
.insight-text { font-size: 12px; color: #94a3b8; line-height: 1.45; margin: 0; }
.insight-input {
    width: 100%; background: #0b1628; color: #e2e8f0; border: 1px solid rgba(180, 83, 9, 0.4);
    border-radius: 5px; padding: 6px 8px; font-size: 12px; resize: vertical;
    font-family: inherit; box-sizing: border-box;
}
.insight-input:focus { outline: none; border-color: #f59e0b; }

/* ── Trend charts ────────────────────────────────────────────────────────────── */
.trend-grid {
    display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px; margin-top: 4px;
}
.trend-card {
    background: #0b1628; border: 1px solid #1e293b; border-radius: 7px; padding: 10px 12px;
}
.trend-card-title { font-size: 12px; font-weight: 600; color: #94a3b8; margin-bottom: 8px; }

/* ── Recognition ─────────────────────────────────────────────────────────────── */
.recognition-section { border-left: 3px solid #d97706; }

/* Edit mode */
.recognition-edit {
    display: flex; gap: 16px; align-items: flex-start;
}
.rec-photo-upload {
    width: 160px; min-height: 160px; flex-shrink: 0;
    border: 2px dashed rgba(180, 83, 9, 0.5); border-radius: 8px;
    background: rgba(120, 53, 15, 0.1); cursor: pointer; overflow: hidden;
    position: relative; display: flex; flex-direction: column;
    align-items: center; justify-content: center; transition: border-color 0.15s;
}
.rec-photo-upload:hover { border-color: #d97706; }
.rec-photo-upload.has-photo { border-style: solid; border-color: #92400e; }
.rec-photo-thumb { width: 100%; height: 100%; object-fit: cover; }
.rec-upload-placeholder {
    display: flex; flex-direction: column; align-items: center; gap: 6px;
    padding: 16px; text-align: center; pointer-events: none;
}
.rec-upload-placeholder i  { font-size: 24px; color: #92400e; }
.rec-upload-placeholder span:first-of-type { font-size: 13px; font-weight: 600; color: #d97706; }
.rec-upload-hint { font-size: 11px; color: #78350f; }
.rec-photo-clear {
    position: absolute; top: 4px; right: 4px;
    background: rgba(0,0,0,0.6); border: none; color: #fff; border-radius: 50%;
    width: 20px; height: 20px; cursor: pointer; font-size: 10px;
    display: flex; align-items: center; justify-content: center;
}
.rec-fields { flex: 1; display: flex; flex-direction: column; gap: 10px; }
.rec-field-label {
    display: flex; flex-direction: column; gap: 4px;
    font-size: 11px; font-weight: 600; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em;
}
.rec-input {
    background: #0b1628; color: #f1f5f9; border: 1px solid rgba(180, 83, 9, 0.4);
    border-radius: 5px; padding: 7px 10px; font-size: 13px; font-family: inherit;
}
.rec-input:focus { outline: none; border-color: #f59e0b; }
.rec-row-2 { display: flex; gap: 10px; }
.flex-1    { flex: 1; }
.rec-textarea {
    background: #0b1628; color: #f1f5f9; border: 1px solid rgba(180, 83, 9, 0.4);
    border-radius: 5px; padding: 8px 10px; font-size: 13px; resize: vertical;
    font-family: inherit; width: 100%; box-sizing: border-box;
}
.rec-textarea:focus { outline: none; border-color: #f59e0b; }

/* Preview mode */
.recognition-preview { }
.rec-card {
    display: flex; gap: 20px; align-items: flex-start;
    background: rgba(120, 53, 15, 0.08); border: 1px solid rgba(180, 83, 9, 0.2);
    border-radius: 8px; padding: 16px; overflow: hidden;
}
.rec-card-photo { width: 160px; flex-shrink: 0; }
.rec-img        { width: 160px; height: 160px; border-radius: 6px; object-fit: cover; border: 2px solid rgba(180, 83, 9, 0.3); }
.rec-img-placeholder {
    width: 160px; height: 160px; border-radius: 6px; background: rgba(120, 53, 15, 0.15);
    border: 2px solid rgba(180, 83, 9, 0.2); display: flex; align-items: center; justify-content: center;
}
.rec-card-body   { flex: 1; }
.rec-card-title  { font-size: 16px; font-weight: 700; color: #fde68a; margin-bottom: 4px; }
.rec-card-meta   { font-size: 12px; color: #92400e; margin-bottom: 12px; font-weight: 500; }
.rec-card-narrative { font-size: 14px; color: #e2e8f0; line-height: 1.65; }
.empty-field-hint { font-size: 12px; color: #475569; font-style: italic; }
.rec-empty {
    text-align: center; padding: 28px; color: #475569;
    display: flex; flex-direction: column; align-items: center; gap: 8px;
}
.rec-empty p    { margin: 0; font-size: 14px; color: #475569; }
.rec-empty-hint { font-size: 12px; color: #334155 !important; }

/* ── Editable text areas ─────────────────────────────────────────────────────── */
.guided-hint {
    font-size: 12px; color: #475569; margin-bottom: 8px; line-height: 1.5;
    border-left: 3px solid rgba(180, 83, 9, 0.4); padding-left: 10px;
}
.nl-textarea {
    width: 100%; background: #0b1628; color: #e2e8f0;
    border: 1px solid rgba(180, 83, 9, 0.4); border-radius: 6px;
    padding: 10px 12px; font-size: 13px; resize: vertical;
    font-family: inherit; line-height: 1.6; box-sizing: border-box;
}
.nl-textarea:focus { outline: none; border-color: #f59e0b; box-shadow: 0 0 0 2px rgba(245,158,11,0.15); }

/* ── Preview typography ──────────────────────────────────────────────────────── */
.nl-prose { font-size: 14px; color: #e2e8f0; line-height: 1.7; margin: 0; white-space: pre-wrap; }
.insights-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 8px; }
.insights-list li {
    display: flex; align-items: flex-start; gap: 10px;
    font-size: 14px; color: #e2e8f0; line-height: 1.55; padding: 6px 0;
    border-bottom: 1px solid rgba(51, 65, 85, 0.4);
}
.insights-list li::before {
    content: '◆'; color: #2563eb; font-size: 8px; flex-shrink: 0; margin-top: 5px;
}
.nl-empty-state {
    display: flex; align-items: flex-start; gap: 10px;
    padding: 12px 14px;
    background: rgba(15, 23, 42, 0.5); border: 1px dashed #1e293b;
    border-radius: 6px; color: #475569; font-size: 13px;
}
.nl-empty-state i { flex-shrink: 0; font-size: 18px; margin-top: 1px; }

/* ── Tables ──────────────────────────────────────────────────────────────────── */
.nl-table { width: 100%; border-collapse: collapse; margin-top: 2px; }
.nl-table th {
    background: #0b1628; border: 1px solid #1e293b;
    padding: 6px 10px; font-size: 10px; font-weight: 700; color: #64748b;
    text-transform: uppercase; letter-spacing: 0.05em; text-align: left;
}
.nl-table td { border: 1px solid #1e293b; padding: 6px 10px; font-size: 12px; color: #cbd5e1; }
.nl-table tr:nth-child(even) td { background: rgba(15,23,42,0.5); }
.tr-overdue td { background: rgba(220, 38, 38, 0.07) !important; }
.td-danger { color: #f87171 !important; font-weight: 700; }
.td-good   { color: #34d399 !important; font-weight: 700; }
.font-bold { font-weight: 700; }
.mono { font-family: ui-monospace, monospace; }

/* ── Signature ───────────────────────────────────────────────────────────────── */
.nl-signature {
    display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 20px;
    margin-top: 16px; padding-top: 16px;
    border-top: 1px solid #1e293b;
}
.sig-col   { display: flex; flex-direction: column; gap: 4px; }
.sig-label { font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.08em; color: #475569; }
.sig-line  { border-bottom: 1px solid #334155; min-height: 28px; }

/* ── Hidden file input ───────────────────────────────────────────────────────── */
.hidden { display: none; }

/* ── Responsive ──────────────────────────────────────────────────────────────── */
@media (max-width: 1000px) {
    .kpi-grid        { grid-template-columns: repeat(2, 1fr); }
    .trend-grid      { grid-template-columns: 1fr; }
    .findings-layout { grid-template-columns: 1fr; }
    .nl-cover        { flex-direction: column; }
    .nl-cover-sidebar { width: auto; border-left: none; border-top: 1px solid rgba(37, 99, 235, 0.2); }
}

/* ── Print ───────────────────────────────────────────────────────────────────── */
@media print {
    .no-print { display: none !important; }

    .newsletter-page { max-width: none; padding: 0; color: #000; background: #fff; }

    /* Cover: preserve dark branding */
    .nl-cover {
        background: #0f2040 !important;
        -webkit-print-color-adjust: exact; print-color-adjust: exact;
        color: #fff !important;
        border-color: #0f2040 !important;
        page-break-after: always;
    }
    .nl-cover-headline { color: #fff !important; }
    .nl-cover-meta, .nl-cover-tagline { color: #94a3b8 !important; }
    .nl-cover-eyebrow, .nl-cover-rule { color: #93c5fd !important; }
    .nl-cover-sidebar { background: rgba(0,0,0,0.3) !important; border-color: rgba(255,255,255,0.15) !important; }
    .sidebar-label, .toc-num { color: #93c5fd !important; }
    .sidebar-toc li { color: #cbd5e1 !important; }

    .nl-section {
        background: #fff !important; border-color: #dde3ea !important;
        page-break-inside: avoid;
        -webkit-print-color-adjust: exact; print-color-adjust: exact;
    }
    .nl-section-head { border-bottom-color: #dde3ea !important; }
    .nl-section-head h2 { color: #0f172a !important; }
    .section-num { background: #eff6ff !important; border-color: #bfdbfe !important; color: #1d4ed8 !important; }

    .kpi-tile { background: #f8fafc !important; border-color: #dde3ea !important; }
    .kpi-num  { color: #0f172a !important; }
    .kpi-good .kpi-num   { color: #047857 !important; }
    .kpi-warn .kpi-num   { color: #b45309 !important; }
    .kpi-danger .kpi-num { color: #b91c1c !important; }
    .kpi-lbl, .kpi-comp  { color: #64748b !important; }

    .nl-table th { background: #f1f5f9 !important; color: #0f172a !important; border-color: #cbd5e1 !important; }
    .nl-table td { color: #0f172a !important; border-color: #dde3ea !important; background: #fff !important; }
    .nl-table tr:nth-child(even) td { background: #f8fafc !important; }
    .tr-overdue td { background: #fff5f5 !important; }

    .nl-prose, .insights-list li { color: #0f172a !important; }
    .insights-list li::before    { color: #1d4ed8 !important; }
    .insight-text { color: #334155 !important; }
    .insights-list li { border-bottom-color: #dde3ea !important; }

    .rec-card { background: #fffbeb !important; border-color: #fde68a !important; }
    .rec-card-title { color: #92400e !important; }
    .rec-card-meta  { color: #b45309 !important; }
    .rec-card-narrative { color: #0f172a !important; }
    .rec-img        { border-color: #fde68a !important; }

    .sig-line  { border-color: #94a3b8 !important; }
    .sig-label { color: #475569 !important; }

    .chart-caption { color: #64748b !important; }
    .trend-card { background: #f8fafc !important; border-color: #dde3ea !important; }
    .trend-card-title { color: #334155 !important; }

    .nl-signature { border-top-color: #dde3ea !important; }
}
</style>

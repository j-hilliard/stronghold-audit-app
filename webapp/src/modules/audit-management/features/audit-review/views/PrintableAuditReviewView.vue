<template>
    <div class="print-doc" ref="docEl">
        <div v-if="loading" class="loading-msg">Loading audit data…</div>

        <template v-else-if="review">
            <!-- ══ MAIN AUDIT TABLE — thead repeats on every printed page ══════════ -->
            <table class="audit-table">
                <thead>
                    <!-- Row 0: Logo + title banner — repeats on every page -->
                    <tr class="hdr-row-banner">
                        <td colspan="10" class="banner-cell">
                            <div class="banner-inner">
                                <img v-if="logoUrl" :src="logoUrl" alt="" class="banner-logo" />
                                <span v-else class="banner-division-code">{{ review.divisionCode }}</span>
                                <span class="banner-title">Safety &amp; Compliance Audit</span>
                            </div>
                        </td>
                    </tr>
                    <!-- Row 1: Job # | Time | Client | Location -->
                    <tr class="hdr-row-1">
                        <td class="hdr-cell hdr-label">Job #</td>
                        <td class="hdr-cell hdr-value tracking-num" colspan="2">
                            {{ review.trackingNumber || review.header?.jobNumber || '—' }}
                        </td>
                        <td class="hdr-cell hdr-label">Time</td>
                        <td class="hdr-cell hdr-value">{{ review.header?.time || '' }}</td>
                        <td class="hdr-cell hdr-label">Client</td>
                        <td class="hdr-cell hdr-value" colspan="2">{{ review.header?.client || '' }}</td>
                        <td class="hdr-cell hdr-label">Location</td>
                        <td class="hdr-cell hdr-value">{{ review.header?.location || '' }}</td>
                    </tr>
                    <!-- Row 2: Date | Unit | Shift -->
                    <tr class="hdr-row-2">
                        <td class="hdr-cell hdr-label">Date</td>
                        <td class="hdr-cell hdr-value" colspan="2">{{ review.header?.auditDate || '' }}</td>
                        <td class="hdr-cell hdr-label">Unit</td>
                        <td class="hdr-cell hdr-value" colspan="3">{{ review.header?.unit || '' }}</td>
                        <td class="hdr-cell hdr-label">Shift</td>
                        <td class="hdr-cell hdr-value" colspan="2">{{ review.header?.shift || '' }}</td>
                    </tr>
                    <!-- Row 3: Work Description | PM/Superintendent | Auditor -->
                    <tr class="hdr-row-3">
                        <td class="hdr-cell hdr-label" colspan="2">Work Description</td>
                        <td class="hdr-cell hdr-value" colspan="3">{{ review.header?.workDescription || '' }}</td>
                        <td class="hdr-cell hdr-label" colspan="2">PM / Superintendent</td>
                        <td class="hdr-cell hdr-value">{{ review.header?.pm || '' }}</td>
                        <td class="hdr-cell hdr-label">Auditor</td>
                        <td class="hdr-cell hdr-value">{{ review.header?.auditor || '' }}</td>
                    </tr>
                </thead>

                <tbody>
                    <!-- ── Score summary row — appears once, below the header info ── -->
                    <tr class="score-row">
                        <td colspan="10" class="score-td">
                            <div class="score-block">
                                <div class="score-circle" :class="scoreColorClass">
                                    <span class="score-pct">{{ scoreDisplay }}</span>
                                    <span class="score-label">Score</span>
                                </div>
                                <div class="score-stats">
                                    <div class="stat-item stat-conforming">
                                        <span class="stat-count">{{ review.conformingCount }}</span>
                                        <span class="stat-name">Conforming</span>
                                    </div>
                                    <div class="stat-item stat-nc">
                                        <span class="stat-count">{{ review.nonConformingCount }}</span>
                                        <span class="stat-name">Non-Conforming</span>
                                    </div>
                                    <div class="stat-item stat-warning">
                                        <span class="stat-count">{{ review.warningCount }}</span>
                                        <span class="stat-name">Warning</span>
                                    </div>
                                    <div class="stat-item stat-na">
                                        <span class="stat-count">{{ review.naCount }}</span>
                                        <span class="stat-name">N/A</span>
                                    </div>
                                </div>
                            </div>
                        </td>
                    </tr>
                    <!-- ── Life-critical banner row — appears once if applicable ── -->
                    <tr v-if="review.hasLifeCriticalFailure" class="life-critical-row">
                        <td colspan="10" class="life-critical-td">
                            ⚠ LIFE-CRITICAL FAILURE — This audit contains one or more life-critical non-conforming findings.
                            <ul v-if="review.lifeCriticalFailures?.length">
                                <li v-for="f in review.lifeCriticalFailures" :key="f">{{ f }}</li>
                            </ul>
                        </td>
                    </tr>

                    <!-- ── Sections + questions ── -->
                    <template v-for="section in review.sections" :key="section.sectionName">
                        <!-- Section header = column label row for this section -->
                        <tr class="section-header-row">
                            <td class="sec-col-num">#</td>
                            <td colspan="5" class="section-name-cell">{{ section.sectionName }}</td>
                            <td colspan="2" class="sec-col-status">Status</td>
                            <td colspan="2" class="sec-col-comment">Comments / Corrective Actions</td>
                        </tr>
                        <!-- Question rows -->
                        <tr
                            v-for="(item, idx) in section.items"
                            :key="idx"
                            class="question-row"
                            :class="{ 'row-alt': (idx as number) % 2 === 1 }"
                        >
                            <td class="q-num">{{ item.questionNumber ?? ((idx as number) + 1) }}</td>
                            <td class="q-text" colspan="5">{{ item.questionText }}</td>
                            <td class="q-status" :class="statusCellClass(item.status)" colspan="2">
                                {{ formatStatus(item.status) }}
                            </td>
                            <td class="q-comment" colspan="2">{{ item.comment || '' }}</td>
                        </tr>
                    </template>

                    <!-- ── Comments / findings narrative ── -->
                    <tr v-if="review.reviewSummary" class="comments-header-row">
                        <td colspan="10" class="comments-heading">Comments</td>
                    </tr>
                    <tr v-if="review.reviewSummary" class="comments-body-row">
                        <td colspan="10" class="comments-text">{{ review.reviewSummary }}</td>
                    </tr>
                </tbody>
            </table>
        </template>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';

const route = useRoute();
const docEl = ref<HTMLElement | null>(null);
const loading = ref(true);
const review = ref<any>(null);

const LOGO_MAP: Record<string, string> = {
    ETS:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/ETS-Logo-color-full.png',
    STS:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/STS-Logo-Full-Clr@2x-2.png',
    STG:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/STG-Logo-Full-Clr@2x-3.png',
    SHI:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/SHI-Logo-Full-color.png',
    TKIE: 'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/TKIE-Logo-Full-Clr@2x-2.png',
    CSL:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/CSL-Logo-Full-Clr@2x-3.png',
    CC:   'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/CC-Logo-Full-Clr@2x-1.png',
};

const logoUrl = computed(() => {
    const code = review.value?.divisionCode?.toUpperCase() ?? '';
    return LOGO_MAP[code] ?? null;
});

const scoreDisplay = computed(() => {
    const s = review.value?.scorePercent;
    return s != null ? `${Math.round(s)}%` : '—';
});

const scoreColorClass = computed(() => {
    const s = review.value?.scorePercent;
    if (s == null) return '';
    if (s >= 90) return 'score-green';
    if (s >= 75) return 'score-amber';
    return 'score-red';
});

function statusCellClass(status: string | null | undefined): string {
    switch (status) {
        case 'Conforming':    return 'status-conforming';
        case 'NonConforming': return 'status-nc';
        case 'Warning':       return 'status-warning';
        case 'NA':            return 'status-na';
        default:              return 'status-unanswered';
    }
}

function formatStatus(status: string | null | undefined): string {
    switch (status) {
        case 'Conforming':    return 'Conforming';
        case 'NonConforming': return 'Non-Conforming';
        case 'Warning':       return 'Warning';
        case 'NA':            return 'N/A';
        default:              return status ?? '';
    }
}

onMounted(async () => {
    try {
        const cached = sessionStorage.getItem('print-review-data');
        if (cached) {
            review.value = JSON.parse(cached);
        } else {
            const auditId = Number(route.params.auditId);
            review.value = await useAuditService().getAuditReview(auditId);
        }
    } catch (e) {
        console.error('Failed to load audit review for print:', e);
    } finally {
        loading.value = false;
    }

    await nextTick();

    // Wait for all images (logos) to finish loading before printing
    const imgs = Array.from(document.querySelectorAll<HTMLImageElement>('img'));
    await Promise.all(
        imgs.map(img =>
            img.complete
                ? Promise.resolve()
                : new Promise<void>(resolve => { img.onload = () => resolve(); img.onerror = () => resolve(); })
        )
    );

    // Move into #print-root so PrimeVue's print visibility rules apply correctly
    const root = document.getElementById('print-root');
    if (root && docEl.value) {
        root.appendChild(docEl.value);
    }

    window.print();
});
</script>

<style scoped>
/* ── Base ─────────────────────────────────────────────────────────────────── */
* {
    font-family: Arial, Helvetica, sans-serif;
    font-size: 9pt;
    box-sizing: border-box;
}

.print-doc {
    background: #fff;
    color: #000;
    padding: 0;
    margin: 0;
}

.loading-msg {
    padding: 20px;
    font-size: 12pt;
    color: #666;
}

/* ── Main audit table ─────────────────────────────────────────────────────── */
.audit-table {
    width: 100%;
    border-collapse: collapse;
    table-layout: auto;
}

.audit-table td,
.audit-table th {
    border: 1px solid #000;
    padding: 3px 5px;
    vertical-align: middle;
    overflow-wrap: break-word;
    word-break: break-word;
}

/* ── THEAD header rows ────────────────────────────────────────────────────── */
thead {
    display: table-header-group; /* repeats on every printed page */
}

.hdr-label {
    background: #f0f0f0;
    font-weight: bold;
    font-size: 8pt;
    white-space: nowrap;
    width: 8%;
}

.hdr-value {
    font-weight: normal;
}

.tracking-num {
    font-weight: bold;
    font-size: 10pt;
    color: #1a3a5c;
}

/* ── Logo / title banner row ──────────────────────────────────────────────── */
.banner-cell {
    background: #1a3a5c;
    padding: 6px 10px;
    border: none;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.banner-inner {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
}

.banner-logo {
    max-height: 44px;
    max-width: 180px;
    object-fit: contain;
    filter: brightness(0) invert(1);
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.banner-division-code {
    font-size: 18pt;
    font-weight: bold;
    color: #fff;
}

.banner-title {
    font-size: 13pt;
    font-weight: bold;
    color: #fff;
    text-align: right;
    opacity: 0.92;
}

/* Column header row */
.col-header-row th {
    background: #1a3a5c;
    color: #fff;
    font-weight: bold;
    font-size: 9pt;
    text-align: left;
    padding: 4px 5px;
}

.col-num   { width: 3%; text-align: center; }
.col-status { width: 12%; }
.col-comment { width: 30%; }

/* ── Score summary row (inside table, first tbody row) ───────────────────── */
.score-td {
    background: #f8fafc;
    padding: 8px 10px;
    border-bottom: 2px solid #1a3a5c;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.score-block {
    display: flex;
    align-items: center;
    gap: 16px;
}

.score-circle {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 60px;
    height: 60px;
    border-radius: 50%;
    border: 3px solid currentColor;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.score-circle.score-green { color: #166534; border-color: #166534; background: #dcfce7; }
.score-circle.score-amber { color: #854d0e; border-color: #ca8a04; background: #fef9c3; }
.score-circle.score-red   { color: #991b1b; border-color: #dc2626; background: #fee2e2; }

.score-pct   { font-size: 14pt; font-weight: bold; line-height: 1; }
.score-label { font-size: 7pt; }

.score-stats {
    display: flex;
    gap: 12px;
}

.stat-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    min-width: 56px;
    padding: 4px 8px;
    border-radius: 4px;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.stat-conforming { background: #dcfce7; color: #166534; }
.stat-nc         { background: #fee2e2; color: #991b1b; }
.stat-warning    { background: #fef9c3; color: #854d0e; }
.stat-na         { background: #f3f4f6; color: #374151; }

.stat-count { font-size: 16pt; font-weight: bold; line-height: 1; }
.stat-name  { font-size: 7pt; text-align: center; }

/* ── Life-critical banner row ─────────────────────────────────────────────── */
.life-critical-td {
    background: #991b1b;
    color: #fff;
    font-weight: bold;
    padding: 6px 10px;
    font-size: 9pt;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.life-critical-td ul {
    margin: 4px 0 0 16px;
    font-weight: normal;
}

/* ── Section header row ───────────────────────────────────────────────────── */
.section-header-row td {
    background: #1a3a5c;
    color: #fff;
    font-weight: bold;
    padding: 4px 5px;
    break-after: avoid;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.sec-col-num     { font-size: 8pt; font-weight: bold; text-align: center; width: 3%; }
.section-name-cell { font-size: 9pt; letter-spacing: 0.3px; }
.sec-col-status  { font-size: 8pt; font-weight: bold; width: 12%; }
.sec-col-comment { font-size: 8pt; font-weight: bold; width: 30%; }

/* ── Question rows ────────────────────────────────────────────────────────── */
.question-row {
    break-inside: avoid;
}

.question-row.row-alt td {
    background: #f9fafb;
}

.q-num {
    text-align: center;
    font-weight: bold;
    color: #6b7280;
    font-size: 8pt;
    width: 3%;
}

.q-text {
    font-size: 9pt;
}

/* Status cells — vivid, print-safe colors */
.q-status {
    font-weight: bold;
    font-size: 8pt;
    text-align: center;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.status-conforming { background: #dcfce7 !important; color: #166534 !important; }
.status-nc         { background: #fee2e2 !important; color: #991b1b !important; }
.status-warning    { background: #fef9c3 !important; color: #854d0e !important; }
.status-na         { background: #f3f4f6 !important; color: #6b7280 !important; }
.status-unanswered { background: #fff !important;    color: #9ca3af !important; }

.q-comment {
    font-size: 8.5pt;
    color: #374151;
}

/* ── Comments / findings summary ─────────────────────────────────────────── */
.comments-header-row td {
    background: #1a3a5c;
    color: #fff;
    font-weight: bold;
    padding: 4px 8px;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.comments-heading { font-size: 9pt; }

.comments-body-row {
    break-inside: avoid;
}

.comments-text {
    padding: 8px 10px;
    white-space: pre-wrap;
    font-size: 9pt;
    line-height: 1.5;
}

/* ── Print-specific overrides ─────────────────────────────────────────────── */
@media print {
    * {
        print-color-adjust: exact !important;
        -webkit-print-color-adjust: exact !important;
    }

    .print-doc {
        width: 100%;
    }

    .audit-table {
        page-break-inside: auto;
    }

    .question-row {
        page-break-inside: avoid;
        break-inside: avoid;
    }

    .section-header-row {
        page-break-after: avoid;
        break-after: avoid;
    }

    .score-row {
        page-break-inside: avoid;
        break-inside: avoid;
        page-break-after: avoid;
        break-after: avoid;
    }

    .life-critical-row {
        page-break-inside: avoid;
        break-inside: avoid;
    }

    .comments-body-row {
        page-break-inside: avoid;
        break-inside: avoid;
    }
}

@media screen {
    .print-doc {
        max-width: 1100px;
        margin: 0 auto;
        padding: 20px;
        background: #fff;
        box-shadow: 0 0 20px rgba(0,0,0,0.15);
    }
}
</style>

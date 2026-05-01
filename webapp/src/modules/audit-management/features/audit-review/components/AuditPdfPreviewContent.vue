<template>
    <div :class="mode === 'email' ? 'email-preview-doc' : 'pdf-preview-doc'">
        <div v-if="!review" class="loading-msg">No audit data available.</div>

        <!-- ── EMAIL MODE — clean distribution preview ───────────────────────── -->
        <template v-else-if="mode === 'email'">
            <div class="email-card">
                <!-- Banner -->
                <div class="email-banner">
                    <div class="email-banner-inner">
                        <img v-if="logoUrl" :src="logoUrl" alt="" class="email-logo" />
                        <span v-else class="email-division-code">{{ review.divisionCode }}</span>
                        <span class="email-title">Safety &amp; Compliance Audit</span>
                    </div>
                </div>
                <!-- Metadata grid -->
                <div class="email-meta-grid">
                    <div class="email-meta-item">
                        <span class="email-meta-label">Job #</span>
                        <span class="email-meta-val">{{ review.trackingNumber || review.header?.jobNumber || '—' }}</span>
                    </div>
                    <div class="email-meta-item">
                        <span class="email-meta-label">Date</span>
                        <span class="email-meta-val">{{ review.header?.auditDate || '—' }}</span>
                    </div>
                    <div class="email-meta-item">
                        <span class="email-meta-label">Location</span>
                        <span class="email-meta-val">{{ review.header?.location || '—' }}</span>
                    </div>
                    <div class="email-meta-item">
                        <span class="email-meta-label">Auditor</span>
                        <span class="email-meta-val">{{ review.header?.auditor || '—' }}</span>
                    </div>
                </div>
                <!-- Score block -->
                <div class="email-score-row">
                    <div class="email-score-circle" :class="scoreColorClass">
                        <span class="score-pct">{{ scoreDisplay }}</span>
                        <span class="score-label">Score</span>
                    </div>
                    <div class="email-stat-list">
                        <div class="email-stat stat-conforming">
                            <span class="stat-count">{{ review.conformingCount }}</span>
                            <span class="stat-name">Conforming</span>
                        </div>
                        <div class="email-stat stat-nc">
                            <span class="stat-count">{{ review.nonConformingCount }}</span>
                            <span class="stat-name">Non-Conforming</span>
                        </div>
                        <div class="email-stat stat-warning">
                            <span class="stat-count">{{ review.warningCount }}</span>
                            <span class="stat-name">Warning</span>
                        </div>
                        <div class="email-stat stat-na">
                            <span class="stat-count">{{ review.naCount }}</span>
                            <span class="stat-name">N/A</span>
                        </div>
                    </div>
                </div>
                <!-- Life-critical warning -->
                <div v-if="review.hasLifeCriticalFailure" class="email-life-critical">
                    ⚠ LIFE-CRITICAL FAILURE — This audit contains one or more life-critical non-conforming findings.
                </div>
                <!-- Findings summary / message -->
                <div v-if="effectiveSummary" class="email-summary-block">
                    <p class="email-summary-heading">Findings Summary</p>
                    <p class="email-summary-body">{{ effectiveSummary }}</p>
                </div>
                <div v-else class="email-summary-empty">
                    No findings summary — add one in the Message field before sending.
                </div>
                <!-- Non-conforming findings + CAs (only when includeCas) -->
                <template v-if="includeCas && review.nonConformingItems?.length">
                    <div class="email-section-heading">Corrective Actions</div>
                    <div v-for="(item, idx) in review.nonConformingItems" :key="item.id" class="email-nc-item" :class="{ 'email-nc-alt': idx % 2 === 1 }">
                        <div class="email-nc-question">{{ item.questionText }}</div>
                        <div v-if="item.comment" class="email-nc-comment">{{ item.comment }}</div>
                        <div v-if="item.correctiveActions?.length" class="email-ca-list">
                            <div
                                v-for="ca in (includeOpenCasOnly ? item.correctiveActions.filter(c => c.status !== 'Closed') : item.correctiveActions)"
                                :key="ca.id"
                                class="email-ca-row"
                            >
                                <span class="email-ca-desc">{{ ca.description }}</span>
                                <span class="email-ca-meta">
                                    {{ ca.assignedTo || 'Unassigned' }}
                                    <template v-if="ca.dueDate"> · Due {{ ca.dueDate }}</template>
                                    · <span :class="ca.status === 'Closed' ? 'email-ca-closed' : 'email-ca-open'">{{ ca.status }}</span>
                                </span>
                            </div>
                        </div>
                    </div>
                </template>
                <!-- Footer -->
                <div class="email-footer-bar">
                    Sent via Stronghold Audit System &nbsp;·&nbsp; Safety &amp; Compliance
                </div>
            </div>
        </template>

        <!-- ── PDF / PREVIEW MODE — full audit table ─────────────────────────── -->
        <template v-else>
            <table class="audit-table">
                <thead>
                    <tr class="hdr-row-banner">
                        <td colspan="10" class="banner-cell">
                            <div class="banner-inner">
                                <img v-if="logoUrl" :src="logoUrl" alt="" class="banner-logo" />
                                <span v-else class="banner-division-code">{{ review.divisionCode }}</span>
                                <span class="banner-title">Safety &amp; Compliance Audit</span>
                            </div>
                        </td>
                    </tr>
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
                    <tr class="hdr-row-2">
                        <td class="hdr-cell hdr-label">Date</td>
                        <td class="hdr-cell hdr-value" colspan="2">{{ review.header?.auditDate || '' }}</td>
                        <td class="hdr-cell hdr-label">Unit</td>
                        <td class="hdr-cell hdr-value" colspan="3">{{ review.header?.unit || '' }}</td>
                        <td class="hdr-cell hdr-label">Shift</td>
                        <td class="hdr-cell hdr-value" colspan="2">{{ review.header?.shift || '' }}</td>
                    </tr>
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

                    <tr v-if="review.hasLifeCriticalFailure" class="life-critical-row">
                        <td colspan="10" class="life-critical-td">
                            ⚠ LIFE-CRITICAL FAILURE — This audit contains one or more life-critical non-conforming findings.
                            <ul v-if="review.lifeCriticalFailures?.length">
                                <li v-for="f in review.lifeCriticalFailures" :key="f">{{ f }}</li>
                            </ul>
                        </td>
                    </tr>

                    <template v-for="section in review.sections" :key="section.sectionName">
                        <tr class="section-header-row">
                            <td class="sec-col-num">#</td>
                            <td colspan="5" class="section-name-cell">{{ section.sectionName }}</td>
                            <td colspan="2" class="sec-col-status">Status</td>
                            <td colspan="2" class="sec-col-comment">Comments / Corrective Actions</td>
                        </tr>
                        <tr
                            v-for="(item, idx) in section.items"
                            :key="idx"
                            class="question-row"
                            :class="{ 'row-alt': (idx as number) % 2 === 1 }"
                        >
                            <td class="q-num">{{ item.sortOrder ?? ((idx as number) + 1) }}</td>
                            <td class="q-text" colspan="5">{{ item.questionText }}</td>
                            <td class="q-status" :class="statusCellClass(item.status)" colspan="2">
                                {{ formatStatus(item.status) }}
                            </td>
                            <td class="q-comment" colspan="2">{{ item.comment || '' }}</td>
                        </tr>
                    </template>

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
import { computed } from 'vue';
import type { AuditReviewDto } from '@/apiclient/auditClient';
import { useAuditPresentation } from '../composables/useAuditPresentation';

const props = withDefaults(defineProps<{
    review: AuditReviewDto | null;
    mode?: 'preview' | 'pdf' | 'email';
    summaryOverride?: string | null;
    includeCas?: boolean;
    includeOpenCasOnly?: boolean;
}>(), {
    mode: 'preview',
    includeCas: false,
    includeOpenCasOnly: false,
});

const effectiveSummary = computed(() =>
    props.summaryOverride?.trim() || props.review?.reviewSummary?.trim() || null
);

const { logoUrl, scoreDisplay, scoreColorClass, statusCellClass, formatStatus } =
    useAuditPresentation(() => props.review);
</script>

<style scoped>
* {
    font-family: Arial, Helvetica, sans-serif;
    font-size: 9pt;
    box-sizing: border-box;
}

.pdf-preview-doc {
    background: #fff;
    color: #000;
    padding: 20px;
}

.loading-msg {
    padding: 20px;
    font-size: 12pt;
    color: #666;
}

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

.hdr-label {
    background: #f0f0f0;
    font-weight: bold;
    font-size: 8pt;
    white-space: nowrap;
    width: 8%;
}

.hdr-value { font-weight: normal; }

.tracking-num {
    font-weight: bold;
    font-size: 10pt;
    color: #1a3a5c;
}

.banner-cell {
    background: #1a3a5c;
    padding: 6px 10px;
    border: none;
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

.score-td {
    background: #f8fafc;
    padding: 8px 10px;
    border-bottom: 2px solid #1a3a5c;
}

.score-block { display: flex; align-items: center; gap: 16px; }

.score-circle {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 60px;
    height: 60px;
    border-radius: 50%;
    border: 3px solid currentColor;
}

.score-circle.score-green { color: #166534; border-color: #166534; background: #dcfce7; }
.score-circle.score-amber { color: #854d0e; border-color: #ca8a04; background: #fef9c3; }
.score-circle.score-red   { color: #991b1b; border-color: #dc2626; background: #fee2e2; }

.score-pct   { font-size: 14pt; font-weight: bold; line-height: 1; }
.score-label { font-size: 7pt; }

.score-stats { display: flex; gap: 12px; }

.stat-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    min-width: 56px;
    padding: 4px 8px;
    border-radius: 4px;
}

.stat-conforming { background: #dcfce7; color: #166534; }
.stat-nc         { background: #fee2e2; color: #991b1b; }
.stat-warning    { background: #fef9c3; color: #854d0e; }
.stat-na         { background: #f3f4f6; color: #374151; }

.stat-count { font-size: 16pt; font-weight: bold; line-height: 1; }
.stat-name  { font-size: 7pt; text-align: center; }

.life-critical-td {
    background: #991b1b;
    color: #fff;
    font-weight: bold;
    padding: 6px 10px;
    font-size: 9pt;
}
.life-critical-td ul { margin: 4px 0 0 16px; font-weight: normal; }

.section-header-row td {
    background: #1a3a5c;
    color: #fff;
    font-weight: bold;
    padding: 4px 5px;
}

.sec-col-num     { font-size: 8pt; text-align: center; width: 3%; }
.section-name-cell { font-size: 9pt; letter-spacing: 0.3px; }
.sec-col-status  { font-size: 8pt; font-weight: bold; width: 12%; }
.sec-col-comment { font-size: 8pt; font-weight: bold; width: 30%; }

.question-row.row-alt td { background: #f9fafb; }

.q-num {
    text-align: center;
    font-weight: bold;
    color: #6b7280;
    font-size: 8pt;
    width: 3%;
}
.q-text   { font-size: 9pt; }
.q-status { font-weight: bold; font-size: 8pt; text-align: center; }
.q-comment { font-size: 8.5pt; color: #374151; }

.status-conforming { background: #dcfce7 !important; color: #166534 !important; }
.status-nc         { background: #fee2e2 !important; color: #991b1b !important; }
.status-warning    { background: #fef9c3 !important; color: #854d0e !important; }
.status-na         { background: #f3f4f6 !important; color: #6b7280 !important; }
.status-unanswered { background: #fff    !important; color: #9ca3af !important; }

.comments-header-row td {
    background: #1a3a5c;
    color: #fff;
    font-weight: bold;
    padding: 4px 8px;
}
.comments-heading { font-size: 9pt; }
.comments-text { padding: 8px 10px; white-space: pre-wrap; font-size: 9pt; line-height: 1.5; }

/* ── EMAIL MODE ────────────────────────────────────────────────────────────── */
.email-preview-doc { background: #fff; color: #111827; padding: 16px; font-family: Arial, Helvetica, sans-serif; font-size: 13px; }

.email-card { max-width: 600px; margin: 0 auto; border-radius: 10px; overflow: hidden; border: 1px solid #e2e8f0; }

.email-banner { background: #1a3a5c; padding: 18px 20px; }
.email-banner-inner { display: flex; align-items: center; justify-content: space-between; gap: 12px; }
.email-logo { max-height: 40px; max-width: 160px; object-fit: contain; filter: brightness(0) invert(1); }
.email-division-code { font-size: 16pt; font-weight: bold; color: #fff; }
.email-title { font-size: 13pt; font-weight: bold; color: #fff; opacity: 0.92; }

.email-meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0; background: #f8fafc; padding: 12px 16px 8px; border-bottom: 1px solid #e2e8f0; }
.email-meta-item { display: flex; flex-direction: column; padding: 4px 8px 4px 0; }
.email-meta-label { font-size: 10px; font-weight: 600; color: #6b7280; text-transform: uppercase; letter-spacing: 0.04em; }
.email-meta-val { font-size: 13px; color: #111827; margin-top: 2px; }

.email-score-row { display: flex; align-items: center; gap: 14px; padding: 14px 16px; background: #f8fafc; border-bottom: 1px solid #e2e8f0; }
.email-score-circle { display: flex; flex-direction: column; align-items: center; justify-content: center; width: 58px; height: 58px; border-radius: 50%; border: 3px solid currentColor; flex-shrink: 0; }
.email-stat-list { display: flex; gap: 10px; flex-wrap: wrap; }
.email-stat { display: flex; flex-direction: column; align-items: center; min-width: 52px; padding: 4px 8px; border-radius: 4px; }

.email-life-critical { background: #991b1b; color: #fff; font-weight: bold; padding: 8px 16px; font-size: 12px; }

.email-summary-block { padding: 14px 16px; background: #fff; border-bottom: 1px solid #e2e8f0; }
.email-summary-heading { font-size: 11px; font-weight: 700; color: #1a3a5c; text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 6px; }
.email-summary-body { font-size: 13px; line-height: 1.65; color: #374151; white-space: pre-wrap; margin: 0; }
.email-summary-empty { padding: 12px 16px; font-size: 12px; color: #9ca3af; font-style: italic; border-bottom: 1px solid #e2e8f0; background: #f8fafc; }

.email-section-heading { font-size: 11px; font-weight: 700; color: #1a3a5c; text-transform: uppercase; letter-spacing: 0.05em; padding: 10px 16px 4px; background: #f8fafc; border-bottom: 1px solid #e2e8f0; border-top: 1px solid #e2e8f0; }

.email-nc-item { padding: 10px 16px; border-bottom: 1px solid #f1f5f9; background: #fff; }
.email-nc-alt { background: #f9fafb; }
.email-nc-question { font-size: 13px; font-weight: 600; color: #dc2626; margin-bottom: 4px; }
.email-nc-comment { font-size: 12px; color: #475569; margin-bottom: 6px; font-style: italic; }
.email-ca-list { display: flex; flex-direction: column; gap: 4px; margin-top: 6px; padding-left: 12px; border-left: 3px solid #fca5a5; }
.email-ca-row { display: flex; flex-direction: column; gap: 1px; }
.email-ca-desc { font-size: 12px; color: #374151; }
.email-ca-meta { font-size: 11px; color: #6b7280; }
.email-ca-closed { color: #16a34a; }
.email-ca-open   { color: #d97706; }

.email-footer-bar { padding: 10px 16px; background: #f8fafc; border-top: 1px solid #e2e8f0; font-size: 11px; color: #9ca3af; text-align: center; }
</style>

<template>
    <table class="audit-table">
        <thead>
            <tr class="hdr-row-banner">
                <td colspan="10" class="banner-cell">
                    <div class="banner-inner">
                        <img v-if="logoUrl" :src="logoUrl" alt="" class="banner-logo" />
                        <span v-else class="banner-division-code">{{ review?.divisionCode }}</span>
                        <span class="banner-title">Safety &amp; Compliance Audit</span>
                    </div>
                </td>
            </tr>
            <tr class="hdr-row-1">
                <td class="hdr-cell hdr-label">Job #</td>
                <td class="hdr-cell hdr-value tracking-num" colspan="2">
                    {{ review?.trackingNumber || review?.header?.jobNumber || '—' }}
                </td>
                <td class="hdr-cell hdr-label">Time</td>
                <td class="hdr-cell hdr-value">{{ review?.header?.time || '' }}</td>
                <td class="hdr-cell hdr-label">Client</td>
                <td class="hdr-cell hdr-value" colspan="2">{{ review?.header?.client || '' }}</td>
                <td class="hdr-cell hdr-label">Location</td>
                <td class="hdr-cell hdr-value">{{ review?.header?.location || '' }}</td>
            </tr>
            <tr class="hdr-row-2">
                <td class="hdr-cell hdr-label">Date</td>
                <td class="hdr-cell hdr-value" colspan="2">{{ review?.header?.auditDate || '' }}</td>
                <td class="hdr-cell hdr-label">Unit</td>
                <td class="hdr-cell hdr-value" colspan="3">{{ review?.header?.unit || '' }}</td>
                <td class="hdr-cell hdr-label">Shift</td>
                <td class="hdr-cell hdr-value" colspan="2">{{ review?.header?.shift || '' }}</td>
            </tr>
            <tr class="hdr-row-3">
                <td class="hdr-cell hdr-label" colspan="2">Work Description</td>
                <td class="hdr-cell hdr-value" colspan="3">{{ review?.header?.workDescription || '' }}</td>
                <td class="hdr-cell hdr-label" colspan="2">PM / Superintendent</td>
                <td class="hdr-cell hdr-value">{{ review?.header?.pm || '' }}</td>
                <td class="hdr-cell hdr-label">Auditor</td>
                <td class="hdr-cell hdr-value">{{ review?.header?.auditor || '' }}</td>
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
                                <span class="stat-count">{{ review?.conformingCount }}</span>
                                <span class="stat-name">Conforming</span>
                            </div>
                            <div class="stat-item stat-nc">
                                <span class="stat-count">{{ review?.nonConformingCount }}</span>
                                <span class="stat-name">Non-Conforming</span>
                            </div>
                            <div class="stat-item stat-warning">
                                <span class="stat-count">{{ review?.warningCount }}</span>
                                <span class="stat-name">Warning</span>
                            </div>
                            <div class="stat-item stat-na">
                                <span class="stat-count">{{ review?.naCount }}</span>
                                <span class="stat-name">N/A</span>
                            </div>
                        </div>
                    </div>
                </td>
            </tr>

            <tr v-if="review?.hasLifeCriticalFailure" class="life-critical-row">
                <td colspan="10" class="life-critical-td">
                    ⚠ LIFE-CRITICAL FAILURE — This audit contains one or more life-critical non-conforming findings.
                    <ul v-if="review?.lifeCriticalFailures?.length">
                        <li v-for="f in review.lifeCriticalFailures" :key="f">{{ f }}</li>
                    </ul>
                </td>
            </tr>

            <template v-for="section in review?.sections" :key="section.sectionName">
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

            <tr v-if="review?.reviewSummary" class="comments-header-row">
                <td colspan="10" class="comments-heading">Comments</td>
            </tr>
            <tr v-if="review?.reviewSummary" class="comments-body-row">
                <td colspan="10" class="comments-text">{{ review.reviewSummary }}</td>
            </tr>
        </tbody>
    </table>
</template>

<script setup lang="ts">
import type { AuditReviewDto } from '@/apiclient/auditClient';
import { useAuditPresentation } from '../composables/useAuditPresentation';

const props = defineProps<{
    review: AuditReviewDto | null | undefined;
}>();

const { logoUrl, scoreDisplay, scoreColorClass, statusCellClass, formatStatus } =
    useAuditPresentation(() => props.review);
</script>

<style scoped>
* {
    font-family: Arial, Helvetica, sans-serif;
    font-size: 9pt;
    box-sizing: border-box;
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

thead {
    display: table-header-group;
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

.score-td {
    background: #f8fafc;
    padding: 8px 10px;
    border-bottom: 2px solid #1a3a5c;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
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
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
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
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
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
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}
.life-critical-td ul { margin: 4px 0 0 16px; font-weight: normal; }

.section-header-row td {
    background: #1a3a5c;
    color: #fff;
    font-weight: bold;
    padding: 4px 5px;
    break-after: avoid;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.sec-col-num     { font-size: 8pt; text-align: center; width: 3%; }
.section-name-cell { font-size: 9pt; letter-spacing: 0.3px; }
.sec-col-status  { font-size: 8pt; font-weight: bold; width: 12%; }
.sec-col-comment { font-size: 8pt; font-weight: bold; width: 30%; }

.question-row { break-inside: avoid; }
.question-row.row-alt td { background: #f9fafb; }

.q-num {
    text-align: center;
    font-weight: bold;
    color: #6b7280;
    font-size: 8pt;
    width: 3%;
}
.q-text    { font-size: 9pt; }
.q-status  { font-weight: bold; font-size: 8pt; text-align: center; print-color-adjust: exact; -webkit-print-color-adjust: exact; }
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
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}
.comments-heading { font-size: 9pt; }
.comments-body-row { break-inside: avoid; }
.comments-text { padding: 8px 10px; white-space: pre-wrap; font-size: 9pt; line-height: 1.5; }

@media print {
    * {
        print-color-adjust: exact !important;
        -webkit-print-color-adjust: exact !important;
    }
    .audit-table { page-break-inside: auto; }
    .question-row { page-break-inside: avoid; break-inside: avoid; }
    .section-header-row { page-break-after: avoid; break-after: avoid; }
    .score-row { page-break-inside: avoid; break-inside: avoid; page-break-after: avoid; break-after: avoid; }
    .life-critical-row { page-break-inside: avoid; break-inside: avoid; }
    .comments-body-row { page-break-inside: avoid; break-inside: avoid; }
}
</style>

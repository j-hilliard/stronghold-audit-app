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
            <AuditTableContent :review="review" />
        </template>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { AuditReviewDto } from '@/apiclient/auditClient';
import { useAuditPresentation } from '../composables/useAuditPresentation';
import AuditTableContent from './AuditTableContent.vue';

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

// logoUrl / scoreDisplay / scoreColorClass used by email mode
const { logoUrl, scoreDisplay, scoreColorClass } = useAuditPresentation(() => props.review);
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

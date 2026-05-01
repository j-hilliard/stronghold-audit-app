<template>
    <div>
        <BasePageHeader
            icon="pi pi-eye"
            :title="review ? `${review.divisionCode} Audit Review` : 'Audit Review'"
            :subtitle="review ? `${review.divisionName} — ${review.status}${review.trackingNumber ? ` · ${review.trackingNumber}` : ''}` : ''"
        >
            <!-- ── Step 1: Enter Review Mode — admin only, Submitted or UnderReview ── -->
            <Button
                v-if="review && hasPermission('audit.review') && (review.status === 'Submitted' || (review.status === 'UnderReview' && !reviewEditMode))"
                label="Enter Review Mode"
                icon="pi pi-search"
                severity="info"
                :loading="auditActionSaving"
                @click="enterReviewMode"
            />

            <!-- ── Step 2: Save Changes — visible when in active review edit mode ── -->
            <Button
                v-if="review && hasPermission('audit.review') && reviewEditMode"
                label="Save Changes"
                icon="pi pi-save"
                severity="info"
                outlined
                :loading="summarySaving"
                :disabled="reviewSummaryDraft === (review.reviewSummary ?? '')"
                @click="submitSaveSummary"
            />
            <Button
                v-if="reviewEditMode"
                label="Exit Review Mode"
                icon="pi pi-times"
                severity="secondary"
                text
                @click="reviewEditMode = false"
            />

            <!-- ── Step 3: Preview PDF ── -->
            <Button
                v-if="review"
                label="Preview PDF"
                icon="pi pi-eye"
                severity="secondary"
                outlined
                @click="showPdfPreviewModal = true"
            />

            <!-- Print / PDF ── -->
            <Button
                v-if="review"
                label="Print / PDF"
                icon="pi pi-print"
                severity="secondary"
                @click="printPage"
            />

            <!-- ── Step 4: Approve Audit — UnderReview only ── -->
            <Button
                v-if="review && hasPermission('audit.review') && review.status === 'UnderReview'"
                label="Approve Audit"
                icon="pi pi-check"
                severity="success"
                :loading="auditActionSaving"
                @click="approveAudit"
            />

            <!-- ── Step 5: Send Distribution — visible only when Approved or already Distributed ── -->
            <Button
                v-if="review && hasPermission('audit.review') && (review.status === 'Approved' || review.status === 'Distributed')"
                :label="review.status === 'Distributed' ? 'Resend Distribution' : 'Send Distribution'"
                icon="pi pi-send"
                :loading="distributionLoadingPreview"
                @click="openDistributionDialog"
            />

            <!-- ── Secondary: Reopen ── -->
            <Button
                v-if="review && hasPermission('audit.reopen') && ['Submitted','UnderReview','Approved','Distributed','Closed'].includes(review.status)"
                label="Reopen"
                icon="pi pi-refresh"
                severity="warning"
                outlined
                @click="showReopenDialog = true"
            />

            <!-- ── Submit for Review — auditor/non-admin only, when Reopened ── -->
            <Button
                v-if="review && review.status === 'Reopened' && !hasPermission('audit.review')"
                label="Submit for Review"
                icon="pi pi-send"
                severity="info"
                @click="router.push(`/audit-management/audits/${route.params.id}`)"
            />

            <!-- ── Close Audit — only after workflow completes (Approved or Distributed) ── -->
            <Button
                v-if="review && hasPermission('audit.close') && ['Approved','Distributed'].includes(review.status)"
                label="Close Audit"
                icon="pi pi-check-circle"
                severity="secondary"
                outlined
                @click="showCloseAuditDialog = true"
            />
        </BasePageHeader>

        <!-- ── Reopen Audit Dialog ──────────────────────────────────────────── -->
        <Dialog v-model:visible="showReopenDialog" modal header="Reopen Audit" :style="{ width: '420px' }">
            <div class="space-y-3 py-2">
                <p class="text-sm text-slate-300">This will set the audit back to <strong>Reopened</strong> so responses can be edited.</p>
                <div>
                    <label class="text-xs text-slate-400 block mb-1">Reason (optional)</label>
                    <Textarea v-model="reopenReason" rows="3" class="w-full text-sm" placeholder="Reason for reopening…" autoResize />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" severity="secondary" text @click="showReopenDialog = false" />
                <Button label="Reopen" icon="pi pi-refresh" severity="warning" :loading="auditActionSaving" @click="submitReopen" />
            </template>
        </Dialog>

        <!-- ── Close Audit Dialog ───────────────────────────────────────────── -->
        <Dialog v-model:visible="showCloseAuditDialog" modal header="Close Audit" :style="{ width: '420px' }">
            <div class="space-y-3 py-2">
                <p class="text-sm text-slate-300">Closing the audit marks it as <strong>Closed</strong>. All corrective actions should be resolved first.</p>
                <div>
                    <label class="text-xs text-slate-400 block mb-1">Closing Notes (optional)</label>
                    <Textarea v-model="closeAuditNotes" rows="3" class="w-full text-sm" placeholder="Any final notes…" autoResize />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" severity="secondary" text @click="showCloseAuditDialog = false" />
                <Button label="Close Audit" icon="pi pi-check-circle" severity="success" :loading="auditActionSaving" @click="submitCloseAudit" />
            </template>
        </Dialog>

        <div v-if="reviewLoading" class="flex justify-center py-12">
            <ProgressSpinner />
        </div>

        <div v-else-if="review" class="p-4 space-y-4 audit-review-content">

            <!-- ── Review Mode Banner ─────────────────────────────────────────── -->
            <div v-if="reviewEditMode || review.status === 'UnderReview'" class="review-mode-banner">
                <i class="pi pi-pencil text-blue-400 text-sm shrink-0" />
                <p class="text-sm flex-1">
                    <span class="font-semibold text-blue-300">Review Mode</span>
                    <span class="text-blue-400/80"> — You can edit this audit. Make updates, preview the PDF, then approve.</span>
                </p>
                <Button
                    v-if="review.status === 'UnderReview' && hasPermission('audit.review')"
                    label="Edit Responses"
                    icon="pi pi-pencil"
                    severity="info"
                    size="small"
                    outlined
                    @click="router.push(`/audit-management/audits/${route.params.id}?reviewerMode=1`)"
                />
            </div>

            <!-- ── Life-Critical Failure Banner ───────────────────────────────── -->
            <div
                v-if="review.hasLifeCriticalFailure"
                class="rounded-lg border-2 border-red-500 bg-red-950/70 p-4 flex items-start gap-3"
                role="alert"
            >
                <i class="pi pi-exclamation-triangle text-red-400 text-2xl mt-0.5 flex-shrink-0" />
                <div>
                    <p class="text-red-300 font-bold text-base tracking-wide uppercase">AUTOMATIC FAIL — Life Critical Violation</p>
                    <p class="text-red-400 text-sm mt-1">
                        The following life-critical question(s) received a Non-Conforming response.
                        Regardless of overall score, this audit is an automatic failure.
                    </p>
                    <ul class="mt-2 space-y-1">
                        <li
                            v-for="(item, idx) in review.lifeCriticalFailures"
                            :key="idx"
                            class="flex items-start gap-2 text-sm text-red-300"
                        >
                            <i class="pi pi-times-circle text-red-500 mt-0.5 flex-shrink-0" />
                            {{ item }}
                        </li>
                    </ul>
                </div>
            </div>

            <!-- ── AI Audit Summary ────────────────────────────────────────────── -->
            <div
                v-if="review.aiSummary"
                class="rounded-lg border border-sky-800 bg-sky-950/40 overflow-hidden"
            >
                <button
                    class="w-full flex items-center justify-between gap-3 px-4 py-3 text-left hover:bg-sky-900/20 transition-colors no-print"
                    @click="showAiSummary = !showAiSummary"
                >
                    <div class="flex items-center gap-2">
                        <i class="pi pi-sparkles text-sky-400 text-sm" />
                        <span class="text-sm font-semibold text-sky-300">AI Audit Summary</span>
                        <span class="text-xs text-sky-500 italic">Generated at submission</span>
                    </div>
                    <i :class="['pi text-sky-500 text-xs', showAiSummary ? 'pi-chevron-up' : 'pi-chevron-down']" />
                </button>
                <div v-if="showAiSummary" class="px-4 pb-4">
                    <p class="text-sm text-slate-300 leading-relaxed">{{ review.aiSummary }}</p>
                </div>
            </div>

            <!-- Score summary card -->
            <Card>
                <template #title>
                    <span class="text-base font-semibold text-white">Score Summary</span>
                </template>
                <template #content>
                    <div class="flex flex-wrap gap-6 items-center">
                        <div class="relative flex-shrink-0" style="width:100px;height:100px;">
                            <svg width="100" height="100" viewBox="0 0 100 100">
                                <circle cx="50" cy="50" r="42" fill="none" stroke="rgba(100,116,139,0.25)" stroke-width="9" />
                                <circle
                                    cx="50" cy="50" r="42"
                                    fill="none"
                                    :stroke="ringColor"
                                    stroke-width="9"
                                    stroke-linecap="round"
                                    :stroke-dasharray="ringCircumference"
                                    :stroke-dashoffset="ringDashoffset"
                                    transform="rotate(-90 50 50)"
                                    style="transition: stroke-dashoffset 1.1s cubic-bezier(0.4,0,0.2,1), stroke 0.5s ease"
                                />
                            </svg>
                            <div class="absolute inset-0 flex flex-col items-center justify-center">
                                <span :class="['text-lg font-bold leading-none', scoreColor]">{{ scoreDisplay }}</span>
                                <span class="text-[9px] text-slate-400 mt-0.5 tracking-wide uppercase">Score</span>
                            </div>
                        </div>
                        <div class="flex flex-wrap gap-3 flex-1">
                            <div class="flex flex-col items-center bg-emerald-900/50 border border-emerald-700 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-emerald-300">{{ review.conformingCount }}</span>
                                <span class="text-xs text-emerald-400">Conforming</span>
                            </div>
                            <div class="flex flex-col items-center bg-red-900/50 border border-red-700 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-red-300">{{ review.nonConformingCount }}</span>
                                <span class="text-xs text-red-400">Non-Conforming</span>
                            </div>
                            <div class="flex flex-col items-center bg-amber-900/50 border border-amber-700 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-amber-300">{{ review.warningCount }}</span>
                                <span class="text-xs text-amber-400">Warning</span>
                            </div>
                            <div class="flex flex-col items-center bg-slate-700 border border-slate-600 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-slate-300">{{ review.naCount }}</span>
                                <span class="text-xs text-slate-400">N/A</span>
                            </div>
                            <div v-if="review.unansweredCount > 0" class="flex flex-col items-center bg-slate-800 border border-slate-700 rounded-lg px-4 py-2 min-w-16">
                                <span class="text-2xl font-bold text-slate-500">{{ review.unansweredCount }}</span>
                                <span class="text-xs text-slate-500">Unanswered</span>
                            </div>
                        </div>
                    </div>
                    <div v-if="review.scorePercent != null" class="mt-4">
                        <div class="w-full h-3 bg-slate-700 rounded-full overflow-hidden">
                            <div class="h-full rounded-full transition-all duration-700" :class="barColor" :style="{ width: `${review.scorePercent}%` }" />
                        </div>
                    </div>
                </template>
            </Card>

            <!-- Findings Summary -->
            <Card v-if="hasPermission('audit.review') || review.reviewSummary">
                <template #title>
                    <div class="flex items-center justify-between">
                        <span class="text-base font-semibold text-white">Findings Summary</span>
                        <span v-if="!hasPermission('audit.review')" class="text-xs text-slate-500 italic">Read-only</span>
                    </div>
                </template>
                <template #content>
                    <div v-if="hasPermission('audit.review')" class="space-y-3 no-print">
                        <Textarea
                            v-model="reviewSummaryDraft"
                            rows="5"
                            class="w-full text-sm"
                            placeholder="Write a findings narrative to include in the distribution email…"
                            autoResize
                        />
                        <div class="flex items-center gap-3">
                            <Button
                                label="Save Summary"
                                icon="pi pi-save"
                                size="small"
                                :loading="summarySaving"
                                :disabled="reviewSummaryDraft === (review.reviewSummary ?? '')"
                                @click="submitSaveSummary"
                            />
                            <span class="text-xs text-slate-500">Saved text appears in the distribution email.</span>
                        </div>
                    </div>
                    <p v-if="hasPermission('audit.review')" class="print-only text-sm leading-relaxed whitespace-pre-wrap">{{ reviewSummaryDraft || 'No findings summary written.' }}</p>
                    <p v-else class="text-sm text-slate-300 whitespace-pre-wrap leading-relaxed">{{ review.reviewSummary }}</p>
                </template>
            </Card>

            <!-- Benchmark comparison card -->
            <Card v-if="review.divisionAvgScore != null || review.divisionScoreTarget != null">
                <template #title>
                    <span class="text-base font-semibold text-white">Division Benchmark</span>
                </template>
                <template #content>
                    <div class="flex flex-wrap gap-3 items-stretch text-sm">
                        <div class="flex flex-col items-center bg-slate-700/60 border border-slate-600 rounded-lg px-4 py-2 min-w-20">
                            <span class="text-xl font-bold text-slate-200">
                                {{ review.divisionAvgScore != null ? `${review.divisionAvgScore.toFixed(1)}%` : '—' }}
                            </span>
                            <span class="text-xs text-slate-400 mt-0.5">Div Avg (last 10)</span>
                        </div>
                        <div v-if="review.divisionScoreTarget != null" class="flex flex-col items-center bg-slate-700/60 border border-slate-600 rounded-lg px-4 py-2 min-w-20">
                            <span class="text-xl font-bold text-slate-200">{{ review.divisionScoreTarget }}%</span>
                            <span class="text-xs text-slate-400 mt-0.5">Target</span>
                        </div>
                        <div
                            v-if="review.scorePercent != null"
                            class="flex flex-col items-center border rounded-lg px-4 py-2 min-w-20"
                            :class="benchmarkStatusClass"
                        >
                            <span class="text-xl font-bold">{{ scoreDisplay }}</span>
                            <span class="text-xs mt-0.5">This Audit</span>
                            <span v-if="review.divisionScoreTarget != null" class="text-xs font-semibold mt-0.5">
                                {{ review.scorePercent >= Number(review.divisionScoreTarget) ? '✓ Above Target' : '✗ Below Target' }}
                            </span>
                        </div>
                    </div>
                </template>
            </Card>

            <!-- Audit header info -->
            <Card v-if="review.header">
                <template #title>
                    <span class="text-base font-semibold text-white">Audit Information</span>
                </template>
                <template #content>
                    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                        <div v-if="review.header.auditDate"><p class="text-slate-400 text-xs">Date</p><p class="text-white">{{ review.header.auditDate }}</p></div>
                        <div v-if="review.header.auditor"><p class="text-slate-400 text-xs">Auditor</p><p class="text-white">{{ review.header.auditor }}</p></div>
                        <div v-if="review.header.jobNumber"><p class="text-slate-400 text-xs">Job Number</p><p class="text-white">{{ review.header.jobNumber }}</p></div>
                        <div v-if="review.header.client"><p class="text-slate-400 text-xs">Client</p><p class="text-white">{{ review.header.client }}</p></div>
                        <div v-if="review.header.location"><p class="text-slate-400 text-xs">Location</p><p class="text-white">{{ review.header.location }}</p></div>
                        <div v-if="review.header.pm"><p class="text-slate-400 text-xs">Project Manager</p><p class="text-white">{{ review.header.pm }}</p></div>
                        <div v-if="review.header.company1"><p class="text-slate-400 text-xs">Company</p><p class="text-white">{{ review.header.company1 }}</p></div>
                        <div v-if="review.header.responsibleParty"><p class="text-slate-400 text-xs">Responsible Party</p><p class="text-white">{{ review.header.responsibleParty }}</p></div>
                    </div>
                </template>
            </Card>

            <!-- Attachments -->
            <Card>
                <template #title>
                    <div class="flex items-center justify-between">
                        <span class="text-base font-semibold text-white">Attachments</span>
                        <span v-if="reviewEditMode || review.status === 'UnderReview'" class="text-xs text-blue-400 font-medium">
                            <i class="pi pi-info-circle mr-1 text-xs" />Edit via form
                        </span>
                    </div>
                </template>
                <template #content>
                    <AuditAttachments :audit-id="Number(route.params.id)" :readonly="true" />
                </template>
            </Card>

            <!-- Non-conforming findings -->
            <Card v-if="review.nonConformingItems.length > 0">
                <template #title>
                    <span class="text-base font-semibold text-red-300">
                        Non-Conforming Items ({{ review.nonConformingItems.length }})
                    </span>
                </template>
                <template #content>
                    <div class="space-y-4">
                        <div
                            v-for="(item, idx) in review.nonConformingItems"
                            :key="item.id"
                            class="finding-card border border-red-800 rounded-lg p-3 bg-red-950/20"
                        >
                            <div class="flex items-start justify-between gap-2">
                                <div class="flex-1 min-w-0">
                                    <p class="text-sm text-slate-200">
                                        <span class="text-slate-500 mr-1">{{ idx + 1 }}.</span>
                                        {{ item.questionText }}
                                    </p>
                                    <span
                                        v-if="repeatFindingIdSet.has(item.questionId)"
                                        class="inline-flex items-center gap-1 mt-1 text-[10px] font-semibold bg-amber-900/50 border border-amber-700/60 text-amber-300 rounded px-1.5 py-0.5"
                                    >
                                        <i class="pi pi-exclamation-circle text-[9px]" />
                                        Repeat Finding
                                    </span>
                                </div>
                                <div class="flex items-center gap-2 shrink-0">
                                    <span v-if="item.correctedOnSite" class="text-xs bg-emerald-900 border border-emerald-700 text-emerald-300 rounded px-1.5 py-0.5">Corrected On-Site</span>
                                    <span v-else class="text-xs bg-slate-700 border border-slate-600 text-slate-400 rounded px-1.5 py-0.5">Not Corrected</span>
                                    <Button
                                        v-if="!item.correctedOnSite"
                                        :label="item.correctiveActions.length > 0 ? 'Reassign CA' : 'Assign CA'"
                                        :icon="item.correctiveActions.length > 0 ? 'pi pi-refresh' : 'pi pi-plus'"
                                        size="small"
                                        severity="warning"
                                        outlined
                                        class="no-print"
                                        @click="openAssignModal(item)"
                                    />
                                </div>
                            </div>
                            <p v-if="item.comment" class="text-xs text-slate-400 mt-1 italic">"{{ item.comment }}"</p>
                            <div v-if="item.correctiveActions.length > 0" class="mt-3 space-y-2">
                                <div
                                    v-for="ca in item.correctiveActions"
                                    :key="ca.id"
                                    class="flex items-center justify-between gap-2 bg-slate-800/60 border border-slate-700 rounded px-3 py-2 text-xs"
                                >
                                    <div class="flex-1">
                                        <p class="text-slate-200 font-medium">{{ ca.description }}</p>
                                        <p class="text-slate-400 mt-0.5">
                                            <span v-if="ca.assignedTo">Assigned to: <span class="text-slate-300">{{ ca.assignedTo }}</span></span>
                                            <span v-if="ca.dueDate" class="ml-2">Due: <span class="text-slate-300">{{ ca.dueDate }}</span></span>
                                        </p>
                                    </div>
                                    <div class="flex items-center gap-2">
                                        <Tag :value="ca.status" :severity="caSeverity(ca.status)" />
                                        <Button
                                            v-if="ca.status !== 'Closed' && ca.status !== 'Voided' && hasPermission('audit.review')"
                                            :label="ca.status === 'Submitted' ? 'Approve' : 'Close'"
                                            size="small"
                                            :severity="ca.status === 'Submitted' ? 'warning' : 'success'"
                                            outlined
                                            class="no-print"
                                            @click="openCloseModal(ca)"
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </template>
            </Card>

            <div v-else class="bg-emerald-900/20 border border-emerald-800 rounded-lg p-4 text-center">
                <i class="pi pi-check-circle text-2xl text-emerald-400 mb-2 block" />
                <p class="text-emerald-300 font-medium">No non-conforming items</p>
            </div>

            <!-- Warning items -->
            <Card v-if="(review.warningItems?.length ?? 0) > 0">
                <template #title>
                    <span class="text-base font-semibold text-amber-300">
                        Warnings ({{ review.warningItems.length }})
                    </span>
                </template>
                <template #content>
                    <div class="space-y-2">
                        <div
                            v-for="(item, idx) in review.warningItems"
                            :key="item.questionId"
                            class="warning-card border border-amber-800 rounded-lg p-3 bg-amber-950/20"
                        >
                            <p class="text-sm text-slate-200">
                                <span class="text-slate-500 mr-1">{{ idx + 1 }}.</span>
                                {{ item.questionText }}
                            </p>
                            <p v-if="item.comment" class="text-xs text-slate-400 mt-1 italic">"{{ item.comment }}"</p>
                        </div>
                    </div>
                </template>
            </Card>

            <!-- Full audit record -->
            <Card v-if="(review.sections?.length ?? 0) > 0">
                <template #title>
                    <div class="flex items-center justify-between">
                        <span class="text-base font-semibold text-white">Full Audit Record</span>
                        <div class="flex items-center gap-2">
                            <button
                                v-if="reviewEditMode || review.status === 'UnderReview'"
                                class="text-xs text-blue-400 hover:text-blue-300 flex items-center gap-1 no-print transition-colors"
                                @click="router.push(`/audit-management/audits/${route.params.id}`)"
                            >
                                <i class="pi pi-pencil text-[10px]" />Edit in Form
                            </button>
                            <button
                                class="text-xs text-slate-400 hover:text-slate-200 flex items-center gap-1 no-print"
                                @click="showFullRecord = !showFullRecord"
                            >
                                <i :class="showFullRecord ? 'pi pi-chevron-up' : 'pi pi-chevron-down'" />
                                {{ showFullRecord ? 'Collapse' : 'Expand' }}
                            </button>
                        </div>
                    </div>
                </template>
                <template #content>
                    <div v-if="showFullRecord" class="space-y-4">
                        <div v-for="section in review.sections" :key="section.sectionName">
                            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide mb-2">{{ section.sectionName }}</p>
                            <div class="divide-y divide-slate-700 border border-slate-700 rounded-lg overflow-hidden">
                                <div
                                    v-for="item in section.items"
                                    :key="item.questionId"
                                    :class="[
                                        'flex items-start gap-3 px-3 py-2 text-sm',
                                        item.status === 'NonConforming' ? 'bg-red-950/30' :
                                        item.status === 'Warning' ? 'bg-amber-950/20' : ''
                                    ]"
                                >
                                    <span :class="['shrink-0 w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold mt-0.5', statusDotClass(item.status)]" />
                                    <span class="flex-1 text-slate-300">{{ item.questionText }}</span>
                                    <span :class="['text-xs font-semibold shrink-0 mt-0.5', statusTextClass(item.status)]">
                                        {{ item.status ?? 'Unanswered' }}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <p v-else class="text-slate-500 text-sm italic">Click Expand to view all {{ review.sections.reduce((n, s) => n + s.items.length, 0) }} responses</p>
                </template>
            </Card>

            <!-- Distribution Recipients -->
            <Card v-if="review.reviewEmailRouting.length > 0 || (review.distributionRecipients?.length ?? 0) > 0 || hasPermission('audit.review')" class="distribution-section">
                <template #title>
                    <span class="text-base font-semibold text-white">Distribution Recipients</span>
                </template>
                <template #content>
                    <div class="space-y-4">
                        <div v-if="review.reviewEmailRouting.length > 0">
                            <p class="text-xs text-slate-500 uppercase tracking-wide mb-1">Auto (Division Routing)</p>
                            <ul class="space-y-1">
                                <li v-for="r in review.reviewEmailRouting" :key="r.emailAddress" class="flex items-center gap-2 text-sm text-slate-300">
                                    <i class="pi pi-envelope text-slate-500 text-xs" />{{ r.emailAddress }}
                                </li>
                            </ul>
                        </div>
                        <div v-if="(review.distributionRecipients?.length ?? 0) > 0 || hasPermission('audit.review')">
                            <div class="flex items-center justify-between mb-1">
                                <p class="text-xs text-slate-500 uppercase tracking-wide">Additional Recipients</p>
                                <Button
                                    v-if="hasPermission('audit.review')"
                                    label="Add People"
                                    icon="pi pi-plus"
                                    size="small"
                                    text
                                    class="!py-0 !text-xs no-print"
                                    @click="openAddRecipientsDialog"
                                />
                            </div>
                            <ul v-if="review.distributionRecipients?.length" class="space-y-1">
                                <li v-for="r in review.distributionRecipients" :key="r.id" class="flex items-center gap-2 text-sm text-slate-300">
                                    <i class="pi pi-user text-slate-500 text-xs" />
                                    <span class="flex-1">{{ r.emailAddress }}<span v-if="r.name" class="text-slate-500"> — {{ r.name }}</span></span>
                                    <Button
                                        v-if="hasPermission('audit.review')"
                                        icon="pi pi-times"
                                        severity="danger"
                                        text
                                        size="small"
                                        class="!p-1"
                                        :loading="removingRecipientId === r.id"
                                        @click="removeRecipient(r.id)"
                                    />
                                </li>
                            </ul>
                            <p v-else-if="!hasPermission('audit.review')" class="text-sm text-slate-500 italic">No additional recipients added.</p>
                        </div>
                    </div>
                </template>
            </Card>
        </div>

        <div v-else class="p-4 text-center text-slate-400">Review not available.</div>
    </div>

    <!-- ══ PART 2: PDF Preview Modal ════════════════════════════════════════════ -->
    <Dialog
        v-model:visible="showPdfPreviewModal"
        modal
        :header="`PDF Preview — ${review?.divisionCode ?? ''} Audit`"
        :style="{ width: '95vw', maxWidth: '1100px', height: '92vh' }"
        :contentStyle="{ padding: 0, overflow: 'auto', background: '#f1f5f9' }"
    >
        <div class="pdf-modal-body">
            <AuditPdfPreviewContent :review="review" />
        </div>
        <template #footer>
            <Button label="Close" severity="secondary" text @click="showPdfPreviewModal = false" />
            <Button
                label="Print / Download"
                icon="pi pi-print"
                severity="secondary"
                @click="printPage"
            />
        </template>
    </Dialog>

    <!-- ══ Add Recipients dialog ════════════════════════════════════════════════ -->
    <Dialog v-model:visible="showAddRecipientsDialog" modal header="Add Distribution Recipients" :style="{ width: '540px' }">
        <div class="space-y-4 pt-1">
            <div class="flex gap-2">
                <InputText
                    v-model="addRecipientsSearch"
                    placeholder="Search by name or email..."
                    class="text-sm flex-1"
                    autofocus
                />
                <select
                    v-model="addRecipientsDivisionFilter"
                    class="bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 focus:outline-none focus:border-blue-500 w-36"
                >
                    <option value="">All Companies</option>
                    <option v-for="div in dialogDivisionOptions" :key="div" :value="div">{{ div }}</option>
                </select>
            </div>
            <div class="max-h-64 overflow-y-auto space-y-1 border border-slate-700 rounded-lg p-2 bg-slate-900/50">
                <p v-if="filteredRoutingForDialog.length === 0" class="text-sm text-slate-500 italic py-2 text-center">No matches found.</p>
                <label
                    v-for="entry in filteredRoutingForDialog"
                    :key="entry.emailAddress"
                    class="flex items-center gap-3 px-3 py-2 rounded cursor-pointer hover:bg-slate-700/50 transition-colors"
                    :class="{ 'bg-slate-700/30': dialogSelectedEmails.includes(entry.emailAddress) }"
                >
                    <input type="checkbox" :value="entry.emailAddress" v-model="dialogSelectedEmails" class="accent-blue-500 w-4 h-4 shrink-0" />
                    <div class="flex-1 min-w-0">
                        <p class="text-sm text-slate-200 truncate">{{ nameFromEmail(entry.emailAddress) }}</p>
                        <p class="text-xs text-slate-500 truncate">{{ entry.emailAddress }} &middot; {{ entry.divisionCode }}</p>
                    </div>
                </label>
            </div>
            <div class="border-t border-slate-700 pt-3 space-y-2">
                <p class="text-xs text-slate-400 font-semibold uppercase tracking-wide">Not in the list? Add manually:</p>
                <div class="flex gap-2">
                    <InputText v-model="manualEmail" placeholder="email@example.com" class="text-sm flex-1" />
                    <InputText v-model="manualName" placeholder="Name (optional)" class="text-sm w-36" />
                </div>
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" text @click="closeAddRecipientsDialog" />
            <Button
                label="Add Selected"
                icon="pi pi-check"
                :loading="addingRecipient"
                :disabled="dialogSelectedEmails.length === 0 && !manualEmail.trim()"
                @click="submitAddRecipients"
            />
        </template>
    </Dialog>

    <!-- ══ Distribution Modal ═══════════════════════════════════════════════════ -->
    <SendDistributionModal
        v-model:show="showDistributionDialog"
        v-model:summaryEdit="distributionSummaryEdit"
        v-model:subject="editableSubject"
        v-model:selectedAttachmentIds="selectedAttachmentIds"
        v-model:includeAuditPdf="includeAuditPdf"
        v-model:includeCas="includeCas"
        v-model:includeOpenCasOnly="includeOpenCasOnly"
        v-model:includeAttachments="includeAttachments"
        v-model:excludedEmails="distributionExcludedEmails"
        :review="review"
        :distribution-preview="distributionPreview"
        :distribution-step="distributionStep"
        :distribution-sent-info="distributionSentInfo"
        :distribution-sending="distributionSending"
        :adding-recipient="addingRecipient"
        :removing-recipient-id="removingRecipientId"
        @send="submitDistributionEmail"
        @open-mailto="openMailtoFallback"
        @close="closeDistributionDialog"
        @add-recipient="(email, name) => quickAddRecipient(email, name)"
        @remove-recipient="quickRemoveRecipient"
    />

    <!-- ══ Assign CA modal ══════════════════════════════════════════════════════ -->
    <Dialog v-model:visible="showAssign" header="Assign Corrective Action" modal style="width: 500px">
        <div class="space-y-4 pt-2">
            <div>
                <label class="text-xs text-slate-400 block mb-1">Finding</label>
                <p class="text-sm text-slate-200">{{ assignTarget?.questionText }}</p>
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Description / Action Required <span class="text-red-400">*</span></label>
                <Textarea v-model="assignForm.description" rows="3" class="w-full text-sm" placeholder="Describe the corrective action required..." autoResize />
            </div>
            <div class="grid grid-cols-2 gap-3">
                <div>
                    <label class="text-xs text-slate-400 block mb-1">Assignee Name <span class="text-red-400">*</span></label>
                    <InputText v-model="assignForm.assignedTo" class="w-full text-sm" placeholder="Full name" />
                </div>
                <div>
                    <label class="text-xs text-slate-400 block mb-1">Assignee Email <span class="text-red-400">*</span></label>
                    <InputText v-model="assignForm.assignedToEmail" type="email" class="w-full text-sm" placeholder="email@example.com" />
                </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
                <div>
                    <label class="text-xs text-slate-400 block mb-1">Due Date</label>
                    <InputText v-model="assignForm.dueDate" type="date" class="w-full text-sm" />
                    <p class="text-xs text-slate-500 mt-0.5">Leave blank to use division SLA default</p>
                </div>
                <div>
                    <label class="text-xs text-slate-400 block mb-1">Priority</label>
                    <div class="flex gap-2 mt-1">
                        <button
                            :class="['flex-1 py-1.5 rounded text-sm font-medium border transition-colors', assignForm.priority === 'Normal' ? 'bg-blue-600 border-blue-600 text-white' : 'bg-transparent border-slate-600 text-slate-400 hover:border-slate-500']"
                            @click="assignForm.priority = 'Normal'"
                            type="button"
                        >Normal</button>
                        <button
                            :class="['flex-1 py-1.5 rounded text-sm font-medium border transition-colors', assignForm.priority === 'Urgent' ? 'bg-red-600 border-red-600 text-white' : 'bg-transparent border-slate-600 text-slate-400 hover:border-slate-500']"
                            @click="assignForm.priority = 'Urgent'"
                            type="button"
                        >Urgent</button>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" @click="showAssign = false" />
            <Button
                label="Assign"
                icon="pi pi-check"
                :loading="saving"
                :disabled="!assignForm.description.trim() || !assignForm.assignedTo.trim() || !assignForm.assignedToEmail.trim()"
                @click="submitAssign"
            />
        </template>
    </Dialog>

    <!-- ══ Close CA modal ═══════════════════════════════════════════════════════ -->
    <Dialog v-model:visible="showClose" header="Close Corrective Action" modal style="width: 420px">
        <div class="space-y-4 pt-2">
            <div>
                <label class="text-xs text-slate-400 block mb-1">Resolution Notes *</label>
                <Textarea v-model="closeForm.notes" rows="3" class="w-full text-sm" placeholder="Describe how this was resolved..." autoResize />
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Completion Date <span class="text-slate-600">(defaults to today)</span></label>
                <InputText v-model="closeForm.completedDate" type="date" class="w-full text-sm" />
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" @click="showClose = false" />
            <Button label="Close CA" icon="pi pi-check" severity="success" :loading="saving" :disabled="!closeForm.notes.trim()" @click="submitClose" />
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import ProgressSpinner from 'primevue/progressspinner';
import Card from 'primevue/card';
import Dialog from 'primevue/dialog';
import Tag from 'primevue/tag';
import Textarea from 'primevue/textarea';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import AuditAttachments from '@/modules/audit-management/features/audit-form/components/AuditAttachments.vue';
import AuditPdfPreviewContent from '../components/AuditPdfPreviewContent.vue';
import SendDistributionModal from '../components/SendDistributionModal.vue';
import { usePermissions } from '@/modules/audit-management/composables/usePermissions';
import { useAuditReviewData } from '../composables/useAuditReviewData';
import { useAuditReviewActions } from '../composables/useAuditReviewActions';
import { useAuditReviewFormatting } from '../composables/useAuditReviewFormatting';
import { useAuditReviewNavigation } from '../composables/useAuditReviewNavigation';

const router = useRouter();
const route  = useRoute();
const { hasPermission } = usePermissions();

const { review, reviewLoading, repeatFindingIdSet, allRoutingEntries } = useAuditReviewData();

const {
    saving, auditActionSaving,
    reviewEditMode,
    showPdfPreviewModal,
    reviewSummaryDraft, summarySaving, submitSaveSummary,
    showReopenDialog, showCloseAuditDialog, reopenReason, closeAuditNotes,
    submitReopen, submitCloseAudit, enterReviewMode, approveAudit,
    showAssign, assignTarget, assignForm, openAssignModal, submitAssign,
    showClose, closeTarget, closeForm, openCloseModal, submitClose,
    addingRecipient, removingRecipientId,
    showAddRecipientsDialog, addRecipientsSearch, addRecipientsDivisionFilter,
    dialogSelectedEmails, manualEmail, manualName,
    dialogDivisionOptions, filteredRoutingForDialog,
    nameFromEmail, openAddRecipientsDialog, closeAddRecipientsDialog,
    submitAddRecipients, removeRecipient, quickAddRecipient, quickRemoveRecipient,
    showDistributionDialog, distributionLoadingPreview, distributionSending,
    distributionPreview, distributionSummaryEdit, editableSubject, selectedAttachmentIds,
    includeAuditPdf, includeCas, includeOpenCasOnly, includeAttachments,
    distributionStep, distributionSentInfo, distributionExcludedEmails,
    openDistributionDialog, closeDistributionDialog, submitDistributionEmail, openMailtoFallback,
} = useAuditReviewActions({ review, allRoutingEntries });

const {
    scoreDisplay, scoreColor, barColor,
    ringCircumference, ringDashoffset, ringColor,
    benchmarkStatusClass, caSeverity, statusDotClass, statusTextClass,
} = useAuditReviewFormatting(review);

const { showFullRecord, showAiSummary, printPage } = useAuditReviewNavigation(review);


</script>

<style scoped>
/* ── Review mode banner ────────────────────────────────────────────────────── */
.review-mode-banner {
    display: flex;
    align-items: center;
    gap: 10px;
    border-radius: 10px;
    border: 1px solid rgba(59, 130, 246, 0.35);
    background: rgba(59, 130, 246, 0.07);
    padding: 10px 14px;
}

/* ── PDF preview modal body ────────────────────────────────────────────────── */
.pdf-modal-body {
    background: #f1f5f9;
    min-height: 100%;
    padding: 0;
}

</style>

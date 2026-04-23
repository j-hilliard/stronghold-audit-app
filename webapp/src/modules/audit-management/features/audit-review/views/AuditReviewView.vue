<template>
    <div>
        <BasePageHeader
            icon="pi pi-eye"
            :title="review ? `${review.divisionCode} Audit Review` : 'Audit Review'"
            :subtitle="review ? `${review.divisionName} — ${review.status}${review.trackingNumber ? ` · ${review.trackingNumber}` : ''}` : ''"
        >
            <Tag v-if="review?.reviewedAt" value="Distributed" severity="success" class="text-xs" />
            <Button
                label="Back to Form"
                icon="pi pi-arrow-left"
                severity="secondary"
                outlined
                @click="router.push(`/audit-management/audits/${route.params.id}`)"
            />
            <Button
                v-if="review"
                label="Print / PDF"
                icon="pi pi-print"
                severity="secondary"
                @click="printPage"
            />
            <Button
                v-if="review && userStore.isAuditAdmin"
                label="Send Distribution"
                icon="pi pi-send"
                :loading="distributionLoadingPreview"
                @click="openDistributionDialog"
            />
            <Button
                v-if="review && userStore.isAuditAdmin && (review.status === 'Submitted' || review.status === 'Closed')"
                label="Reopen"
                icon="pi pi-refresh"
                severity="warning"
                outlined
                @click="showReopenDialog = true"
            />
            <Button
                v-if="review && review.status === 'Reopened'"
                label="Submit for Review"
                icon="pi pi-send"
                severity="info"
                @click="router.push(`/audit-management/audits/${route.params.id}`)"
            />
            <Button
                v-if="review && userStore.isAuditAdmin && (review.status === 'Submitted' || review.status === 'Reopened')"
                label="Close Audit"
                icon="pi pi-check-circle"
                severity="success"
                @click="showCloseAuditDialog = true"
            />
        </BasePageHeader>

        <!-- Reopen Audit Dialog -->
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

        <!-- Close Audit Dialog -->
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

        <div v-if="store.reviewLoading" class="flex justify-center py-12">
            <ProgressSpinner />
        </div>

        <div v-else-if="review" class="p-4 space-y-4 audit-review-content">

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
                        <!-- SVG score ring -->
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

            <!-- ── Findings Summary (AuditAdmin editable, others read-only) ──── -->
            <Card v-if="userStore.isAuditAdmin || review.reviewSummary">
                <template #title>
                    <div class="flex items-center justify-between">
                        <span class="text-base font-semibold text-white">Findings Summary</span>
                        <span v-if="!userStore.isAuditAdmin" class="text-xs text-slate-500 italic">Read-only</span>
                    </div>
                </template>
                <template #content>
                    <div v-if="userStore.isAuditAdmin" class="space-y-3 no-print">
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
                    <p v-if="userStore.isAuditAdmin" class="print-only text-sm leading-relaxed whitespace-pre-wrap">{{ reviewSummaryDraft || 'No findings summary written.' }}</p>
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
                        <!-- Division avg -->
                        <div class="flex flex-col items-center bg-slate-700/60 border border-slate-600 rounded-lg px-4 py-2 min-w-20">
                            <span class="text-xl font-bold text-slate-200">
                                {{ review.divisionAvgScore != null ? `${review.divisionAvgScore.toFixed(1)}%` : '—' }}
                            </span>
                            <span class="text-xs text-slate-400 mt-0.5">Div Avg (last 10)</span>
                        </div>
                        <!-- Target -->
                        <div v-if="review.divisionScoreTarget != null" class="flex flex-col items-center bg-slate-700/60 border border-slate-600 rounded-lg px-4 py-2 min-w-20">
                            <span class="text-xl font-bold text-slate-200">{{ review.divisionScoreTarget }}%</span>
                            <span class="text-xs text-slate-400 mt-0.5">Target</span>
                        </div>
                        <!-- This audit vs target -->
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

            <!-- Attachments (read-only view) -->
            <Card>
                <template #content>
                    <AuditAttachments :audit-id="Number(route.params.id)" :readonly="true" />
                </template>
            </Card>

            <!-- Non-conforming findings with CA workflow -->
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
                            <!-- Finding header -->
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

                            <!-- Corrective Actions list -->
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
                                            v-if="ca.status !== 'Closed'"
                                            label="Close"
                                            size="small"
                                            severity="success"
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

            <!-- Full audit record (collapsible) -->
            <Card v-if="(review.sections?.length ?? 0) > 0">
                <template #title>
                    <div class="flex items-center justify-between">
                        <span class="text-base font-semibold text-white">Full Audit Record</span>
                        <button
                            class="text-xs text-slate-400 hover:text-slate-200 flex items-center gap-1 no-print"
                            @click="showFullRecord = !showFullRecord"
                        >
                            <i :class="showFullRecord ? 'pi pi-chevron-up' : 'pi pi-chevron-down'" />
                            {{ showFullRecord ? 'Collapse' : 'Expand' }}
                        </button>
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

            <!-- ── Distribution Recipients ─────────────────────────────────── -->
            <Card v-if="review.reviewEmailRouting.length > 0 || (review.distributionRecipients?.length ?? 0) > 0 || userStore.isAuditAdmin" class="distribution-section">
                <template #title>
                    <span class="text-base font-semibold text-white">Distribution Recipients</span>
                </template>
                <template #content>
                    <div class="space-y-4">
                        <!-- Auto recipients -->
                        <div v-if="review.reviewEmailRouting.length > 0">
                            <p class="text-xs text-slate-500 uppercase tracking-wide mb-1">Auto (Division Routing)</p>
                            <ul class="space-y-1">
                                <li v-for="r in review.reviewEmailRouting" :key="r.emailAddress" class="flex items-center gap-2 text-sm text-slate-300">
                                    <i class="pi pi-envelope text-slate-500 text-xs" />{{ r.emailAddress }}
                                </li>
                            </ul>
                        </div>
                        <!-- Per-audit additions -->
                        <div v-if="(review.distributionRecipients?.length ?? 0) > 0 || userStore.isAuditAdmin">
                            <div class="flex items-center justify-between mb-1">
                                <p class="text-xs text-slate-500 uppercase tracking-wide">Additional Recipients</p>
                                <Button
                                    v-if="userStore.isAuditAdmin"
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
                                        v-if="userStore.isAuditAdmin"
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
                            <p v-else-if="!userStore.isAuditAdmin" class="text-sm text-slate-500 italic">No additional recipients added.</p>
                        </div>
                    </div>
                </template>
            </Card>
        </div>

        <div v-else class="p-4 text-center text-slate-400">Review not available.</div>
    </div>

    <!-- Add Recipients dialog -->
    <Dialog v-model:visible="showAddRecipientsDialog" modal header="Add Distribution Recipients" :style="{ width: '540px' }">
        <div class="space-y-4 pt-1">
            <!-- Search + Division filter -->
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

            <!-- Routing list -->
            <div class="max-h-64 overflow-y-auto space-y-1 border border-slate-700 rounded-lg p-2 bg-slate-900/50">
                <p v-if="filteredRoutingForDialog.length === 0" class="text-sm text-slate-500 italic py-2 text-center">No matches found.</p>
                <label
                    v-for="entry in filteredRoutingForDialog"
                    :key="entry.emailAddress"
                    class="flex items-center gap-3 px-3 py-2 rounded cursor-pointer hover:bg-slate-700/50 transition-colors"
                    :class="{ 'bg-slate-700/30': dialogSelectedEmails.includes(entry.emailAddress) }"
                >
                    <input
                        type="checkbox"
                        :value="entry.emailAddress"
                        v-model="dialogSelectedEmails"
                        class="accent-blue-500 w-4 h-4 shrink-0"
                    />
                    <div class="flex-1 min-w-0">
                        <p class="text-sm text-slate-200 truncate">{{ nameFromEmail(entry.emailAddress) }}</p>
                        <p class="text-xs text-slate-500 truncate">{{ entry.emailAddress }} &middot; {{ entry.divisionCode }}</p>
                    </div>
                </label>
            </div>

            <!-- Manual entry for someone not in the list -->
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

    <!-- Send Distribution Email dialog — preview-first -->
    <Dialog v-model:visible="showDistributionDialog" modal header="Send Distribution Email" :style="{ width: '720px', maxHeight: '90vh' }">
        <div class="flex flex-col gap-4 pt-2" v-if="review && distributionPreview">

            <!-- Subject — editable -->
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Email Subject</label>
                <InputText v-model="editableSubject" class="w-full font-mono text-sm" />
            </div>

            <!-- Recipients -->
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Recipients ({{ distributionPreview.recipients.length }})</label>
                <div class="bg-slate-800/50 rounded p-3 text-xs text-slate-400 space-y-1 max-h-28 overflow-y-auto">
                    <div v-for="email in distributionPreview.recipients" :key="email" class="flex items-center gap-2">
                        <i class="pi pi-envelope text-xs" />{{ email }}
                    </div>
                    <p v-if="distributionPreview.recipients.length === 0" class="text-amber-400 text-xs">
                        No recipients configured. Add recipients in the Distribution Recipients section below before sending.
                    </p>
                </div>
            </div>

            <!-- Findings Summary — editable -->
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Findings Summary <span class="text-slate-500 normal-case font-normal">(edit before sending)</span></label>
                <Textarea
                    v-model="distributionSummaryEdit"
                    rows="3"
                    class="w-full text-sm"
                    placeholder="Write a findings narrative to include in the distribution email…"
                    autoResize
                />
            </div>

            <!-- Attachment selection -->
            <div v-if="review.attachments?.length" class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Attachments</label>
                <div class="space-y-1 max-h-32 overflow-y-auto bg-slate-800/30 rounded p-2">
                    <label
                        v-for="att in review.attachments"
                        :key="att.id"
                        class="flex items-center gap-2 text-sm text-slate-300 cursor-pointer hover:text-white"
                    >
                        <input
                            type="checkbox"
                            :value="att.id"
                            v-model="selectedAttachmentIds"
                            :disabled="!att.hasFile"
                            class="accent-sky-500"
                        />
                        <span :class="{ 'text-slate-500': !att.hasFile }">
                            {{ att.fileName }}
                            <span class="text-xs text-slate-500 ml-1">({{ formatBytes(att.fileSizeBytes) }})</span>
                            <span v-if="!att.hasFile" class="text-xs text-red-500 ml-1">file not found</span>
                        </span>
                    </label>
                </div>
            </div>

            <!-- Email body preview — always visible -->
            <div class="flex flex-col gap-1">
                <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Email Preview</label>
                <div
                    class="border border-slate-700 rounded overflow-auto bg-white"
                    style="max-height: 340px; min-height: 120px;"
                    v-html="distributionPreview.bodyHtml"
                />
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" text @click="showDistributionDialog = false" />
            <Button
                label="Send Distribution Email"
                icon="pi pi-send"
                severity="success"
                :loading="distributionSending"
                :disabled="(distributionPreview?.recipients.length ?? 0) === 0"
                @click="submitDistributionEmail"
            />
        </template>
    </Dialog>

    <!-- Assign CA modal -->
    <Dialog v-model:visible="showAssign" header="Assign Corrective Action" modal style="width: 480px">
        <div class="space-y-4 pt-2">
            <div>
                <label class="text-xs text-slate-400 block mb-1">Finding</label>
                <p class="text-sm text-slate-200">{{ assignTarget?.questionText }}</p>
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Description / Action Required *</label>
                <Textarea v-model="assignForm.description" rows="3" class="w-full text-sm" placeholder="Describe the corrective action required..." autoResize />
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Assign To</label>
                <InputText v-model="assignForm.assignedTo" class="w-full text-sm" placeholder="Name or email" />
            </div>
            <div>
                <label class="text-xs text-slate-400 block mb-1">Due Date</label>
                <InputText v-model="assignForm.dueDate" type="date" class="w-full text-sm" />
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" severity="secondary" @click="showAssign = false" />
            <Button label="Assign" icon="pi pi-check" :loading="saving" :disabled="!assignForm.description.trim()" @click="submitAssign" />
        </template>
    </Dialog>

    <!-- Close CA modal -->
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
import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import ProgressSpinner from 'primevue/progressspinner';
import Card from 'primevue/card';
import Dialog from 'primevue/dialog';
import Tag from 'primevue/tag';
import Textarea from 'primevue/textarea';
import InputText from 'primevue/inputtext';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import AuditAttachments from '@/modules/audit-management/features/audit-form/components/AuditAttachments.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useApiStore } from '@/stores/apiStore';
import { useUserStore } from '@/stores/userStore';
import { AuditClient, type AuditFindingDto, type CorrectiveActionDto, type EmailRoutingRuleDto, type DistributionPreviewDto } from '@/apiclient/auditClient';

const router = useRouter();
const route = useRoute();
const store = useAuditStore();
const apiStore = useApiStore();
const userStore = useUserStore();
const toast = useToast();

const review = computed(() => store.review);
const saving = ref(false);
const showFullRecord = ref(false);
const showAiSummary = ref(true); // expanded by default

// ── Review Summary ────────────────────────────────────────────────────────────
const reviewSummaryDraft = ref('');
const summarySaving = ref(false);

watch(() => review.value?.reviewSummary, (val) => {
    reviewSummaryDraft.value = val ?? '';
}, { immediate: true });

async function submitSaveSummary() {
    const id = Number(route.params.id);
    summarySaving.value = true;
    try {
        await store.saveReviewSummary(id, reviewSummaryDraft.value || null);
        toast.add({ severity: 'success', summary: 'Saved', detail: 'Findings summary saved.', life: 2500 });
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save summary.', life: 4000 });
    } finally {
        summarySaving.value = false;
    }
}

// ── Distribution Recipients ───────────────────────────────────────────────────
const addingRecipient = ref(false);
const removingRecipientId = ref<number | null>(null);

// Add recipients dialog
const showAddRecipientsDialog = ref(false);
const addRecipientsSearch = ref('');
const addRecipientsDivisionFilter = ref('');
const dialogSelectedEmails = ref<string[]>([]);
const manualEmail = ref('');
const manualName = ref('');

// All active routing entries (loaded on mount) — source for the dialog list
const allRoutingEntries = ref<EmailRoutingRuleDto[]>([]);

// Unique sorted division codes from the routing table for the filter dropdown
const dialogDivisionOptions = computed(() =>
    [...new Set(allRoutingEntries.value.filter(r => r.isActive).map(r => r.divisionCode))].sort()
);

// Derive a human-readable name from an email address (brian.smith@... → "Brian Smith")
function nameFromEmail(email: string): string {
    const local = email.split('@')[0] ?? '';
    return local.split(/[._-]/).map(p => p.charAt(0).toUpperCase() + p.slice(1)).join(' ');
}

const filteredRoutingForDialog = computed(() => {
    const already = new Set((review.value?.distributionRecipients ?? []).map(r => r.emailAddress));
    const q = addRecipientsSearch.value.toLowerCase();
    const div = addRecipientsDivisionFilter.value;
    return allRoutingEntries.value.filter(r => {
        if (!r.isActive || already.has(r.emailAddress)) return false;
        if (div && r.divisionCode !== div) return false;
        if (q) {
            const derivedName = nameFromEmail(r.emailAddress).toLowerCase();
            if (!r.emailAddress.toLowerCase().includes(q) && !derivedName.includes(q)) return false;
        }
        return true;
    });
});

function closeAddRecipientsDialog() {
    showAddRecipientsDialog.value = false;
    addRecipientsSearch.value = '';
    addRecipientsDivisionFilter.value = '';
    dialogSelectedEmails.value = [];
    manualEmail.value = '';
    manualName.value = '';
}

function openAddRecipientsDialog() {
    // Default to the current audit's division so the list is pre-filtered
    addRecipientsDivisionFilter.value = review.value?.divisionCode ?? '';
    showAddRecipientsDialog.value = true;
}

async function submitAddRecipients() {
    const id = Number(route.params.id);
    addingRecipient.value = true;
    try {
        // Add all checked routing entries
        for (const email of dialogSelectedEmails.value) {
            await store.addDistributionRecipient(id, email, undefined);
        }
        // Add manual entry if provided
        if (manualEmail.value.trim()) {
            await store.addDistributionRecipient(id, manualEmail.value.trim(), manualName.value.trim() || undefined);
        }
        const total = dialogSelectedEmails.value.length + (manualEmail.value.trim() ? 1 : 0);
        toast.add({ severity: 'success', summary: 'Added', detail: `${total} recipient(s) added.`, life: 2500 });
        closeAddRecipientsDialog();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to add recipients.', life: 4000 });
    } finally {
        addingRecipient.value = false;
    }
}

async function removeRecipient(recipientId: number) {
    const id = Number(route.params.id);
    removingRecipientId.value = recipientId;
    try {
        await store.removeDistributionRecipient(id, recipientId);
        toast.add({ severity: 'success', summary: 'Removed', detail: 'Recipient removed.', life: 2000 });
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove recipient.', life: 4000 });
    } finally {
        removingRecipientId.value = null;
    }
}

// ── Send Distribution Email ───────────────────────────────────────────────────
const showDistributionDialog = ref(false);
const distributionLoadingPreview = ref(false);
const distributionSending = ref(false);
const distributionPreview = ref<DistributionPreviewDto | null>(null);
const distributionSummaryEdit = ref('');
const editableSubject = ref('');
const selectedAttachmentIds = ref<number[]>([]);

function escapeHtml(input: string): string {
    return input
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}

function buildFallbackDistributionPreview(auditId: number): DistributionPreviewDto | null {
    if (!review.value) return null;

    const division = review.value.divisionCode || 'Audit';
    const trackingOrJob = review.value.trackingNumber || review.value.header?.jobNumber || `Audit ${auditId}`;
    const auditDate = review.value.header?.auditDate ? ` — ${review.value.header.auditDate}` : '';
    const recipients = (review.value.distributionRecipients ?? [])
        .map(r => r.emailAddress)
        .filter((v): v is string => Boolean(v && v.trim()))
        .map(v => v.trim());
    const findingsSummary = reviewSummaryDraft.value || review.value.reviewSummary || null;
    const safeSummary = findingsSummary ? escapeHtml(findingsSummary) : 'No findings summary provided.';
    const location = review.value.header?.location ? escapeHtml(review.value.header.location) : 'N/A';
    const auditor = review.value.header?.auditor ? escapeHtml(review.value.header.auditor) : 'N/A';

    return {
        subject: `${division} Audit Distribution — ${trackingOrJob}${auditDate}`,
        recipients,
        findingsSummary,
        bodyHtml: `
            <div style="font-family:Segoe UI,Arial,sans-serif;font-size:14px;line-height:1.5;color:#111827">
                <h2 style="margin:0 0 8px 0;">${escapeHtml(division)} Audit Distribution</h2>
                <p style="margin:0 0 4px 0;"><strong>Reference:</strong> ${escapeHtml(trackingOrJob)}</p>
                <p style="margin:0 0 4px 0;"><strong>Location:</strong> ${location}</p>
                <p style="margin:0 0 12px 0;"><strong>Auditor:</strong> ${auditor}</p>
                <h3 style="margin:0 0 6px 0;">Findings Summary</h3>
                <div style="padding:10px;border:1px solid #d1d5db;border-radius:6px;white-space:pre-wrap;">${safeSummary}</div>
            </div>
        `,
    };
}

async function openDistributionDialog() {
    const id = Number(route.params.id);
    distributionLoadingPreview.value = true;
    try {
        const preview = await store.getDistributionPreview(id, selectedAttachmentIds.value);
        distributionPreview.value = preview;
        distributionSummaryEdit.value = preview.findingsSummary ?? '';
        editableSubject.value = preview.subject;
        showDistributionDialog.value = true;
    } catch (e: any) {
        const status = e?.response?.status;
        // Backward compatibility: if backend is not yet running the new preview endpoint,
        // build a local preview so users can still send distribution emails.
        if (status !== 403 && status !== 401) {
            const fallback = buildFallbackDistributionPreview(id);
            if (fallback) {
                distributionPreview.value = fallback;
                distributionSummaryEdit.value = fallback.findingsSummary ?? '';
                editableSubject.value = fallback.subject;
                showDistributionDialog.value = true;
                toast.add({
                    severity: 'warn',
                    summary: 'Preview Fallback',
                    detail: 'Live preview endpoint unavailable. Using local preview mode.',
                    life: 4500,
                });
                return;
            }
        }

        const detail = status === 403
            ? 'You do not have permission to preview distribution emails.'
            : status === 404
            ? 'Audit not found.'
            : (typeof e?.response?.data === 'string' ? e.response.data : null)
              ?? e?.message
              ?? 'Failed to load distribution preview.';
        toast.add({ severity: 'error', summary: 'Error', detail, life: 6000 });
    } finally {
        distributionLoadingPreview.value = false;
    }
}

async function submitDistributionEmail() {
    const id = Number(route.params.id);
    distributionSending.value = true;
    try {
        // Save edited summary back to the audit so the email uses the updated text
        await store.saveReviewSummary(id, distributionSummaryEdit.value || null);
        await store.sendDistributionEmail(id, selectedAttachmentIds.value, editableSubject.value || undefined);
        showDistributionDialog.value = false;
        distributionPreview.value = null;
        selectedAttachmentIds.value = [];
        toast.add({ severity: 'success', summary: 'Sent', detail: 'Distribution email sent successfully.', life: 4000 });
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to send distribution email.', life: 5000 });
    } finally {
        distributionSending.value = false;
    }
}

function formatBytes(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/** O(1) set for repeat-finding badge lookups */
const repeatFindingIdSet = computed(() => new Set(review.value?.repeatFindingQuestionIds ?? []));

// ── Assign modal ──────────────────────────────────────────────────────────────
const showAssign = ref(false);
const assignTarget = ref<AuditFindingDto | null>(null);
const assignForm = ref({ description: '', assignedTo: '', dueDate: '' });

function openAssignModal(item: AuditFindingDto) {
    assignTarget.value = item;
    assignForm.value = { description: '', assignedTo: '', dueDate: '' };
    showAssign.value = true;
}

// ── Reopen / Close audit ──────────────────────────────────────────────────────
const showReopenDialog = ref(false);
const showCloseAuditDialog = ref(false);
const reopenReason = ref('');
const closeAuditNotes = ref('');
const auditActionSaving = ref(false);

async function submitReopen() {
    const id = Number(route.params.id);
    auditActionSaving.value = true;
    try {
        await getClient().reopenAudit(id, reopenReason.value || null);
        toast.add({ severity: 'warn', summary: 'Reopened', detail: 'Audit has been reopened.', life: 3000 });
        showReopenDialog.value = false;
        reopenReason.value = '';
        await store.loadReview(id);
    } catch (e: unknown) {
        toast.add({ severity: 'error', summary: 'Error', detail: (e as Error)?.message ?? 'Failed to reopen.', life: 4000 });
    } finally {
        auditActionSaving.value = false;
    }
}

async function submitCloseAudit() {
    const id = Number(route.params.id);
    auditActionSaving.value = true;
    try {
        await getClient().closeAudit(id, closeAuditNotes.value || null);
        toast.add({ severity: 'success', summary: 'Closed', detail: 'Audit has been closed.', life: 3000 });
        showCloseAuditDialog.value = false;
        closeAuditNotes.value = '';
        await store.loadReview(id);
    } catch (e: unknown) {
        // Extract the server error message for the open-CA gate (400 BadRequest with plain string body)
        const axiosErr = e as { response?: { status?: number; data?: unknown } };
        const serverMsg = typeof axiosErr?.response?.data === 'string'
            ? axiosErr.response.data
            : (e as Error)?.message ?? 'Failed to close audit.';
        toast.add({ severity: 'error', summary: 'Cannot Close Audit', detail: serverMsg, life: 6000 });
    } finally {
        auditActionSaving.value = false;
    }
}

// ── Close CA modal ─────────────────────────────────────────────────────────────
const showClose = ref(false);
const closeTarget = ref<CorrectiveActionDto | null>(null);
const closeForm = ref({ notes: '', completedDate: '' });

function openCloseModal(ca: CorrectiveActionDto) {
    closeTarget.value = ca;
    closeForm.value = { notes: '', completedDate: '' };
    showClose.value = true;
}

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}

async function submitAssign() {
    if (!assignTarget.value) return;
    saving.value = true;
    try {
        await getClient().assignCorrectiveAction({
            findingId: assignTarget.value.id,
            description: assignForm.value.description,
            assignedTo: assignForm.value.assignedTo || null,
            dueDate: assignForm.value.dueDate || null,
        });
        toast.add({ severity: 'success', summary: 'Assigned', detail: 'Corrective action assigned.', life: 2500 });
        showAssign.value = false;
        // Reload review to get updated CA list
        const id = Number(route.params.id);
        await store.loadReview(id);
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign corrective action.', life: 4000 });
    } finally {
        saving.value = false;
    }
}

async function submitClose() {
    if (!closeTarget.value) return;
    saving.value = true;
    try {
        await getClient().closeCorrectiveAction(closeTarget.value.id, {
            notes: closeForm.value.notes,
            completedDate: closeForm.value.completedDate || null,
        });
        toast.add({ severity: 'success', summary: 'Closed', detail: 'Corrective action closed.', life: 2500 });
        showClose.value = false;
        const id = Number(route.params.id);
        await store.loadReview(id);
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to close corrective action.', life: 4000 });
    } finally {
        saving.value = false;
    }
}

onMounted(async () => {
    const id = Number(route.params.id);
    if (!isNaN(id)) await store.loadReview(id);

    // Load email routing entries for the recipient autocomplete suggestions
    if (userStore.isAuditAdmin) {
        try {
            allRoutingEntries.value = await getClient().getEmailRouting();
        } catch { /* non-fatal */ }
    }
});

const scoreDisplay = computed(() => {
    const pct = review.value?.scorePercent;
    if (pct == null) return '—';
    return `${Math.round(pct)}%`;
});

const scoreColor = computed(() => {
    const pct = review.value?.scorePercent;
    if (pct == null) return 'text-slate-400';
    if (pct >= 90) return 'text-emerald-400';
    if (pct >= 75) return 'text-amber-400';
    return 'text-red-400';
});

const barColor = computed(() => {
    const pct = review.value?.scorePercent;
    if (pct == null) return 'bg-slate-500';
    if (pct >= 90) return 'bg-emerald-500';
    if (pct >= 75) return 'bg-amber-500';
    return 'bg-red-500';
});

// ── Score ring ────────────────────────────────────────────────────────────────
const ringCircumference = 2 * Math.PI * 42; // r=42
const ringDashoffset = computed(() => {
    const pct = review.value?.scorePercent ?? 0;
    return ringCircumference - (pct / 100) * ringCircumference;
});
const ringColor = computed(() => {
    const pct = review.value?.scorePercent;
    if (pct == null) return '#475569';
    if (pct >= 90) return '#34d399';
    if (pct >= 75) return '#fbbf24';
    return '#f87171';
});

const benchmarkStatusClass = computed(() => {
    const pct = review.value?.scorePercent;
    const target = review.value?.divisionScoreTarget;
    if (pct == null) return 'bg-slate-700/60 border-slate-600 text-slate-300';
    if (target == null) return 'bg-slate-700/60 border-slate-600 text-slate-300';
    return pct >= Number(target)
        ? 'bg-emerald-900/50 border-emerald-700 text-emerald-300'
        : 'bg-red-900/50 border-red-700 text-red-300';
});

function caSeverity(status: string): string {
    const map: Record<string, string> = { Open: 'danger', InProgress: 'warning', Closed: 'success' };
    return map[status] ?? 'secondary';
}

function statusDotClass(status: string | null | undefined): string {
    switch (status) {
        case 'Conforming':    return 'bg-emerald-500';
        case 'NonConforming': return 'bg-red-500';
        case 'Warning':       return 'bg-amber-500';
        case 'NA':            return 'bg-slate-500';
        default:              return 'bg-slate-700 border border-slate-500';
    }
}

function statusTextClass(status: string | null | undefined): string {
    switch (status) {
        case 'Conforming':    return 'text-emerald-400';
        case 'NonConforming': return 'text-red-400';
        case 'Warning':       return 'text-amber-400';
        case 'NA':            return 'text-slate-500';
        default:              return 'text-slate-600';
    }
}

async function printPage() {
    const wasOpen = showFullRecord.value;
    showFullRecord.value = true;
    await nextTick();
    sessionStorage.setItem('print-review-data', JSON.stringify(review.value));
    window.open(`/audit-management/print-review/${route.params.id}`, '_blank');
    showFullRecord.value = wasOpen;
}

</script>

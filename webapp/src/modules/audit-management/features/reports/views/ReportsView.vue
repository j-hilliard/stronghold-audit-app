<template>
    <div>
        <!-- Page header — 3 buttons only -->
        <BasePageHeader
            title="Audit Dashboard"
            subtitle="Conformance trends and non-conformance analysis"
            icon="pi pi-chart-bar"
        >
            <Button icon="pi pi-refresh" severity="secondary" outlined size="small" title="Refresh report data" :loading="loading" @click="loadReport" />
            <Button
                label="Reports"
                icon="pi pi-file-edit"
                severity="secondary"
                outlined
                size="small"
                @click="toggleReportsMenu"
            />
            <Menu ref="reportsMenuRef" :model="reportsMenuItems" popup />
            <SplitButton
                v-if="report"
                label="Export"
                icon="pi pi-download"
                severity="secondary"
                outlined
                size="small"
                :model="exportMenuItems"
                @click="exportCsv"
            />
            <Button icon="pi pi-sliders-h" severity="secondary" outlined size="small" title="Customize widgets" @click="toggleCustomize" />
            <OverlayPanel ref="customizePanelRef">
                <div class="flex flex-col gap-1 min-w-[200px]">
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">Visible Widgets</div>
                    <label v-for="card in KPI_CARDS" :key="card.key"
                        class="flex items-center gap-3 py-1.5 px-1 rounded hover:bg-slate-700/40 cursor-pointer transition-colors">
                        <input type="checkbox"
                            :checked="!hidden[card.key]"
                            @change="toggleCard(card.key)"
                            class="accent-blue-500 w-4 h-4 cursor-pointer"
                        />
                        <span class="text-sm text-slate-200">{{ card.label }}</span>
                    </label>
                    <div class="border-t border-slate-700 mt-2 pt-2 flex gap-2">
                        <button @click="showAll" class="text-xs text-blue-400 hover:text-blue-300 transition-colors">Show all</button>
                        <span class="text-slate-600">·</span>
                        <button @click="hideAll" class="text-xs text-slate-400 hover:text-slate-300 transition-colors">Hide all</button>
                    </div>
                </div>
            </OverlayPanel>
        </BasePageHeader>

        <!-- Filter bar — flex-wrap so fields stack on narrow screens -->
        <div class="px-4 pt-3 pb-2 flex flex-wrap gap-3 items-end border-b border-slate-700/40">
            <div class="flex flex-col gap-1 flex-1 min-w-[160px]">
                <label class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">Division</label>
                <Dropdown
                    v-model="filterDivisionId"
                    :options="store.divisions"
                    option-label="code"
                    option-value="id"
                    placeholder="All Divisions"
                    class="w-full"
                    :show-clear="!!filterDivisionId"
                    @change="loadReport"
                    data-testid="report-filter-division"
                />
            </div>
            <div class="flex flex-col gap-1 flex-1 min-w-[140px]">
                <label class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">Status</label>
                <Dropdown
                    v-model="filterStatus"
                    :options="STATUS_OPTIONS"
                    option-label="label"
                    option-value="value"
                    placeholder="All Statuses"
                    class="w-full"
                    :show-clear="!!filterStatus"
                    @change="loadReport"
                    data-testid="report-filter-status"
                />
            </div>
            <div class="flex flex-col gap-1 flex-1 min-w-[120px]">
                <label class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">From</label>
                <Calendar
                    v-model="filterDateFrom"
                    placeholder="From date"
                    dateFormat="yy-mm-dd"
                    class="w-full"
                    :show-clear="!!filterDateFrom"
                    @date-select="loadReport"
                    @clear-click="loadReport"
                    data-testid="report-filter-from"
                />
            </div>
            <div class="flex flex-col gap-1 flex-1 min-w-[120px]">
                <label class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">To</label>
                <Calendar
                    v-model="filterDateTo"
                    placeholder="To date"
                    dateFormat="yy-mm-dd"
                    class="w-full"
                    :show-clear="!!filterDateTo"
                    @date-select="loadReport"
                    @clear-click="loadReport"
                    data-testid="report-filter-to"
                />
            </div>
        </div>

        <!-- Active filter context bar -->
        <div v-if="activeFilterChips.length > 0 && report" class="px-4 py-2 flex items-center gap-2 flex-wrap border-b border-slate-700/50">
            <span class="text-xs text-slate-500">Showing:</span>
            <template v-for="chip in activeFilterChips" :key="chip.key">
                <span v-if="chip.key !== 'section'"
                    class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-slate-700 border border-slate-600 text-slate-300">
                    {{ chip.label }}
                </span>
                <button v-else @click="clearSectionFilter"
                    class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-blue-700/30 border border-blue-500/40 text-blue-300 hover:bg-blue-700/50 transition-colors">
                    {{ chip.label }}
                    <i class="pi pi-times text-[10px]" />
                </button>
            </template>
            <span class="text-xs text-slate-500">·</span>
            <span class="text-xs text-slate-400">
                {{ report.totalAudits }} audit{{ report.totalAudits !== 1 ? 's' : '' }}<template v-if="filterSection"> with NCs in this section</template>
            </span>
        </div>

        <div v-if="loading" class="flex justify-center py-16">
            <ProgressSpinner />
        </div>

        <div v-else-if="report" class="p-4 space-y-4">

            <!-- ── KPI Row 1: 4 headline numbers (first thing you see) ────────── -->
            <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
                <MetricCard
                    v-if="!hidden.kpiTotalAudits"
                    label="Total Audits"
                    :value="displayTotalAudits"
                    variant="default"
                    interactive
                    :trend="trendDeltas.auditCountDelta"
                    :trend-label="trendDeltas.auditCountDelta !== null ? (trendDeltas.auditCountDelta >= 0 ? '↑' : '↓') + ' ' + Math.abs(trendDeltas.auditCountDelta) + ' vs prior' : undefined"
                    data-testid="report-kpi-total-audits"
                    @click="router.push('/audit-management/audits')"
                >
                    <template #top-right>
                        <button class="kpi-hide-btn" title="Hide" @click.stop="hideSection('kpiTotalAudits')"><i class="pi pi-times" /></button>
                    </template>
                </MetricCard>

                <MetricCard
                    v-if="!hidden.kpiAvgConformance"
                    label="Avg Conformance"
                    :value="report.avgScorePercent != null ? `${displayAvgScore}%` : '—'"
                    :variant="scoreVariant"
                    interactive
                    :trend="trendDeltas.scoreDelta"
                    :trend-label="trendDeltas.scoreDelta !== null ? (trendDeltas.scoreDelta >= 0 ? '↑' : '↓') + ' ' + Math.abs(trendDeltas.scoreDelta) + '% vs prior' : undefined"
                    @click="drillAllAudits"
                >
                    <template #top-right>
                        <button class="kpi-hide-btn" title="Hide" @click.stop="hideSection('kpiAvgConformance')"><i class="pi pi-times" /></button>
                    </template>
                </MetricCard>

                <MetricCard
                    v-if="!hidden.kpiNonConformances"
                    label="Non-Conformances"
                    :value="displayNonConforming"
                    variant="danger"
                    interactive
                    :trend="trendDeltas.ncDelta"
                    trend-positive-direction="down"
                    :trend-label="trendDeltas.ncDelta !== null ? (trendDeltas.ncDelta <= 0 ? '↓' : '↑') + ' ' + Math.abs(trendDeltas.ncDelta) + ' vs prior' : undefined"
                    data-testid="report-kpi-nc-count"
                    @click="drillByNcOnly"
                >
                    <template #top-right>
                        <button class="kpi-hide-btn" title="Hide" @click.stop="hideSection('kpiNonConformances')"><i class="pi pi-times" /></button>
                    </template>
                </MetricCard>

                <MetricCard
                    v-if="!hidden.kpiWarnings"
                    label="Warnings"
                    :value="displayWarnings"
                    variant="warning"
                    interactive
                    :trend="trendDeltas.warnDelta"
                    trend-positive-direction="down"
                    :trend-label="trendDeltas.warnDelta !== null ? (trendDeltas.warnDelta <= 0 ? '↓' : '↑') + ' ' + Math.abs(trendDeltas.warnDelta) + ' vs prior' : undefined"
                    data-testid="report-kpi-warning-count"
                    @click="drillByWarnOnly"
                >
                    <template #top-right>
                        <button class="kpi-hide-btn" title="Hide" @click.stop="hideSection('kpiWarnings')"><i class="pi pi-times" /></button>
                    </template>
                </MetricCard>
            </div>

            <!-- ── KPI Row 2: Wide summary cards ──────────────────────────────── -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                <div
                    v-if="!hidden.kpiCorrectedOnSite"
                    class="kpi-card kpi-card--wide group relative"
                    @click="drillByNcOnly"
                    title="Filter to audits with non-conformances"
                >
                    <button @click.stop="hideSection('kpiCorrectedOnSite')" class="kpi-hide-btn" title="Hide"><i class="pi pi-times" /></button>
                    <div class="flex items-center gap-4">
                        <div class="text-3xl font-bold"
                            :class="correctedOnSitePct >= 50 ? 'text-emerald-400' : correctedOnSitePct > 0 ? 'text-amber-400' : 'text-slate-400'">
                            {{ report.totalNonConforming > 0 ? `${displayCorrectedPct}%` : '—' }}
                        </div>
                        <div>
                            <div class="text-sm font-medium text-slate-200">Corrected On Site</div>
                            <div class="text-xs text-slate-400 mt-0.5">
                                {{ report.correctedOnSiteCount }} of {{ report.totalNonConforming }} NCs fixed immediately
                            </div>
                        </div>
                    </div>
                    <div class="kpi-tap-hint"><i class="pi pi-arrow-up-right" /></div>
                </div>
                <div
                    v-if="!hidden.kpiCaAging"
                    class="kpi-card kpi-card--wide kpi-card--danger group relative"
                    @click="router.push('/audit-management/corrective-actions')"
                    data-testid="report-kpi-overdue-corrective-actions"
                    title="View corrective actions"
                >
                    <button @click.stop="hideSection('kpiCaAging')" class="kpi-hide-btn" title="Hide"><i class="pi pi-times" /></button>
                    <div class="flex items-center gap-4">
                        <div class="text-3xl font-bold"
                            :class="caAgingStats.overdueCount > 0 ? 'text-red-400' : 'text-emerald-400'">
                            {{ displayCaAging }}
                        </div>
                        <div>
                            <div class="text-sm font-medium text-slate-200">CAs Past 14-Day Rule</div>
                            <div class="text-xs text-slate-400 mt-0.5">
                                {{ report.openCorrectiveActions.length }} open ·
                                Avg {{ caAgingStats.avgDaysOpen }} days open
                            </div>
                        </div>
                    </div>
                    <div class="kpi-tap-hint"><i class="pi pi-arrow-up-right" /></div>
                </div>
            </div>

            <!-- ── Overdue CA Alert Banner ────────────────────────────────────── -->
            <div
                v-if="overdueAlertCas.length > 0"
                class="overdue-alert-banner"
                @click="router.push('/audit-management/corrective-actions?overdueOnly=true')"
            >
                <i class="pi pi-exclamation-circle text-red-400 text-lg mt-0.5 shrink-0" />
                <div class="flex-1 min-w-0">
                    <div class="text-sm font-semibold text-red-300">
                        {{ overdueAlertCas.length }} Corrective Action{{ overdueAlertCas.length === 1 ? '' : 's' }} Overdue
                    </div>
                    <div class="text-xs text-red-400/80 mt-0.5 line-clamp-1">
                        <span v-for="(ca, i) in overdueAlertCas.slice(0, 3)" :key="ca.id">
                            {{ ca.description.length > 50 ? ca.description.slice(0, 50) + '…' : ca.description }}
                            <span v-if="i < Math.min(overdueAlertCas.length, 3) - 1"> · </span>
                        </span>
                        <span v-if="overdueAlertCas.length > 3" class="opacity-70"> and {{ overdueAlertCas.length - 3 }} more</span>
                    </div>
                </div>
                <span class="text-xs text-red-400 font-medium shrink-0 flex items-center gap-1 mt-0.5">
                    View all <i class="pi pi-arrow-right text-[10px]" />
                </span>
            </div>

            <!-- ── Tab strip (immediately below KPIs) ─────────────────────────── -->
            <div ref="tabBarEl" class="border-b border-slate-700 flex items-center gap-1 -mx-4 px-4 pt-1 overflow-x-auto">
                <button
                    v-for="tab in TABS"
                    :key="tab.key"
                    @click="activeTab = tab.key"
                    :class="['tab-btn', activeTab === tab.key ? 'tab-btn--active' : '']"
                >
                    {{ tab.label }}
                    <span v-if="tab.key === 'action-items' && caAgingStats.overdueCount > 0"
                        class="ml-1.5 inline-flex items-center justify-center w-4 h-4 rounded-full bg-red-500 text-white text-[10px] font-bold leading-none">
                        {{ caAgingStats.overdueCount }}
                    </span>
                </button>
            </div>

            <!-- ── Tab: Overview ─────────────────────────────────────────────── -->
            <template v-if="activeTab === 'overview'">

                <!-- Empty state -->
                <div v-if="divisionHealthCards.length === 0 && recentAuditFeed.length === 0"
                    class="text-center py-12 text-slate-400">
                    <i class="pi pi-chart-bar text-4xl mb-3 block opacity-30" />
                    <p class="text-sm">No audit data yet.</p>
                    <p class="text-xs mt-1 opacity-70">Create an audit to see data here.</p>
                </div>

                <!-- Two-column layout: Division Health (left) + Recent Audits (right) -->
                <div v-else class="grid grid-cols-1 xl:grid-cols-3 gap-4 items-start">

                    <!-- Left: Division Health (2/3 width on xl+) -->
                    <div class="xl:col-span-2 space-y-2">
                        <div v-if="divisionHealthCards.length > 0">
                            <button
                                class="w-full flex items-center justify-between px-1 py-1.5 hover:bg-slate-800/40 rounded-lg transition-colors"
                                @click="divisionHealthCollapsed = !divisionHealthCollapsed"
                            >
                                <div class="flex items-center gap-2">
                                    <span class="text-xs font-semibold text-slate-300 uppercase tracking-wider">Division Health</span>
                                    <span class="text-xs text-slate-500">{{ divisionHealthCards.length }} division{{ divisionHealthCards.length === 1 ? '' : 's' }}</span>
                                    <span v-if="divisionHealthCollapsed" class="text-xs text-blue-400/60">· tap to expand</span>
                                </div>
                                <i :class="['text-slate-400 text-xs transition-transform duration-200', divisionHealthCollapsed ? 'pi pi-chevron-down' : 'pi pi-chevron-up']" />
                            </button>
                            <div v-show="!divisionHealthCollapsed"
                                class="grid gap-3 mt-2"
                                :class="divisionHealthCards.length <= 2 ? 'grid-cols-1 sm:grid-cols-2' : divisionHealthCards.length <= 4 ? 'grid-cols-2' : 'grid-cols-2 md:grid-cols-3'">
                                <div
                                    v-for="div in divisionHealthCards"
                                    :key="div.divisionCode"
                                    class="division-health-card rounded-xl border bg-slate-800 p-4 flex flex-col gap-3 cursor-pointer hover:shadow-lg transition-all"
                                    :class="{
                                        'border-emerald-700/50 hover:border-emerald-600': div.complianceStatus === 'OnTrack',
                                        'border-amber-700/50 hover:border-amber-600': div.complianceStatus === 'DueSoon',
                                        'border-red-700/60 hover:border-red-600': div.complianceStatus === 'Overdue',
                                        'border-slate-700 hover:border-slate-600': div.complianceStatus === 'NoSchedule' || div.complianceStatus === 'NeverAudited',
                                    }"
                                    @click="filterByDivisionCode(div.divisionCode)"
                                >
                                    <div class="flex items-start justify-between gap-2">
                                        <div>
                                            <div class="text-base font-bold text-white">{{ div.divisionCode }}</div>
                                            <div class="text-xs text-slate-400 mt-0.5 line-clamp-1">{{ div.divisionName }}</div>
                                        </div>
                                        <span
                                            v-if="div.complianceStatus !== 'NoSchedule'"
                                            class="shrink-0 text-[10px] px-2 py-0.5 rounded-full font-semibold uppercase tracking-wide"
                                            :class="{
                                                'bg-emerald-900/50 text-emerald-300 border border-emerald-700/40': div.complianceStatus === 'OnTrack',
                                                'bg-amber-900/50 text-amber-300 border border-amber-700/40': div.complianceStatus === 'DueSoon',
                                                'bg-red-900/50 text-red-300 border border-red-700/40': div.complianceStatus === 'Overdue',
                                                'bg-slate-700/50 text-slate-400 border border-slate-600/40': div.complianceStatus === 'NeverAudited',
                                            }"
                                        >
                                            <i class="mr-0.5" :class="{
                                                'pi pi-check-circle': div.complianceStatus === 'OnTrack',
                                                'pi pi-clock': div.complianceStatus === 'DueSoon',
                                                'pi pi-exclamation-triangle': div.complianceStatus === 'Overdue',
                                                'pi pi-minus-circle': div.complianceStatus === 'NeverAudited',
                                            }" />
                                            {{ { OnTrack: 'On Track', DueSoon: 'Due Soon', Overdue: 'Overdue', NeverAudited: 'Never Audited' }[div.complianceStatus] ?? div.complianceStatus }}
                                        </span>
                                    </div>
                                    <!-- Score row: large number + prominent bar -->
                                    <div class="flex items-center gap-3">
                                        <div
                                            class="text-2xl font-bold leading-none tabular-nums shrink-0"
                                            :class="div.avgScore == null ? 'text-slate-500' : div.avgScore >= 90 ? 'text-emerald-400' : div.avgScore >= 75 ? 'text-amber-400' : 'text-red-400'"
                                        >
                                            {{ div.avgScore != null ? `${div.avgScore}%` : '—' }}
                                        </div>
                                        <!-- Score gauge — h-3 so it reads as a real bar, not a hairline -->
                                        <div class="relative flex-1 h-3 rounded-full bg-slate-700 overflow-hidden">
                                            <div
                                                v-if="div.avgScore != null"
                                                class="h-full rounded-full transition-all duration-500"
                                                :style="`width: ${div.avgScore}%`"
                                                :class="div.avgScore >= 90 ? 'bg-emerald-500' : div.avgScore >= 75 ? 'bg-amber-500' : 'bg-red-500'"
                                            />
                                            <!-- Threshold markers at 75% and 90% -->
                                            <div class="absolute top-0 bottom-0 w-px bg-white/20" style="left: 75%;" />
                                            <div class="absolute top-0 bottom-0 w-px bg-white/20" style="left: 90%;" />
                                        </div>
                                    </div>
                                    <!-- Audit frequency dots — visual density indicator -->
                                    <div class="flex items-center gap-1" title="Audit frequency (each dot = 1 audit)">
                                        <div
                                            v-for="n in Math.min(div.auditCount, 10)"
                                            :key="n"
                                            class="h-1.5 flex-1 rounded-sm"
                                            :class="div.avgScore == null ? 'bg-slate-600' : div.avgScore >= 90 ? 'bg-emerald-500/60' : div.avgScore >= 75 ? 'bg-amber-500/60' : 'bg-red-500/60'"
                                        />
                                        <div
                                            v-for="n in Math.max(0, 10 - div.auditCount)"
                                            :key="'e' + n"
                                            class="h-1.5 flex-1 rounded-sm bg-slate-700/50"
                                        />
                                    </div>
                                    <div class="grid grid-cols-3 gap-1 border-t border-slate-700/60 pt-2.5">
                                        <div class="text-center">
                                            <div class="text-sm font-semibold text-white">{{ div.auditCount }}</div>
                                            <div class="text-[10px] text-slate-500">Audits</div>
                                        </div>
                                        <div class="text-center">
                                            <div class="text-sm font-semibold" :class="div.totalNcs > 0 ? 'text-red-400' : 'text-slate-400'">{{ div.totalNcs }}</div>
                                            <div class="text-[10px] text-slate-500">NCs</div>
                                        </div>
                                        <div class="text-center">
                                            <div class="text-sm font-semibold" :class="div.openCaCount > 0 ? 'text-amber-400' : 'text-slate-400'">{{ div.openCaCount }}</div>
                                            <div class="text-[10px] text-slate-500">Open CAs</div>
                                        </div>
                                    </div>
                                    <div class="text-[11px] text-slate-500 -mt-1">
                                        Last audit: {{ div.lastAuditDate ?? 'Never' }}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Right: Recent Audits (1/3 width on xl+) -->
                    <div class="space-y-2">
                        <div v-if="recentAuditFeed.length > 0">
                            <div class="flex items-center justify-between px-1 mb-2">
                                <span class="text-xs font-semibold text-slate-300 uppercase tracking-wider">Recent Audits</span>
                                <button @click="router.push('/audit-management/audits')" class="text-xs text-blue-400 hover:text-blue-300 transition-colors">View all →</button>
                            </div>
                            <div class="recent-audits-list rounded-xl border border-slate-700/60 overflow-hidden">
                                <div
                                    v-for="(audit, i) in recentAuditFeed"
                                    :key="audit.id"
                                    class="recent-audit-row flex items-center gap-3 px-4 py-3 cursor-pointer transition-colors group"
                                    :class="i > 0 ? 'border-t border-slate-700/40' : ''"
                                    @click="router.push(audit.status === 'Draft' || audit.status === 'Reopened' ? `/audit-management/audits/${audit.id}` : `/audit-management/audits/${audit.id}/review`)"
                                >
                                    <!-- Score badge -->
                                    <div
                                        class="w-10 h-10 rounded-lg flex flex-col items-center justify-center shrink-0 border"
                                        :class="audit.scorePercent == null
                                            ? 'bg-slate-800 border-slate-600/50 text-slate-500'
                                            : audit.scorePercent >= 90 ? 'bg-emerald-900/50 border-emerald-700/50 text-emerald-300'
                                            : audit.scorePercent >= 75 ? 'bg-amber-900/50 border-amber-700/50 text-amber-300'
                                            : 'bg-red-900/50 border-red-700/50 text-red-300'"
                                    >
                                        <span class="text-xs font-bold leading-none">{{ audit.scorePercent != null ? `${audit.scorePercent}%` : '—' }}</span>
                                    </div>

                                    <!-- Main content -->
                                    <div class="flex-1 min-w-0">
                                        <div class="flex items-center gap-1.5 flex-wrap">
                                            <span class="text-sm font-semibold text-slate-100">{{ audit.divisionCode }}</span>
                                            <Tag :value="auditStatusLabel(audit.status)" :severity="auditStatusSeverity(audit.status)" class="!text-[10px] !px-1.5 !py-0.5" />
                                            <span v-if="audit.nonConformingCount > 0"
                                                class="inline-flex items-center gap-0.5 text-[11px] font-semibold text-red-400 bg-red-950/40 border border-red-800/30 rounded px-1.5 py-0.5">
                                                <i class="pi pi-exclamation-triangle text-[9px]" />
                                                {{ audit.nonConformingCount }} NC
                                            </span>
                                        </div>
                                        <div class="text-[11px] text-slate-500 mt-0.5 tabular-nums">{{ audit.auditDate ?? '—' }}</div>
                                    </div>

                                    <!-- Arrow -->
                                    <i class="pi pi-chevron-right text-[10px] text-slate-600 group-hover:text-blue-400 group-hover:translate-x-0.5 transition-all shrink-0" />
                                </div>
                            </div>
                        </div>
                    </div>

                </div><!-- /two-column -->

            </template><!-- /overview -->

            <!-- ── Tab: Analysis ─────────────────────────────────────────────── -->
            <template v-if="activeTab === 'analysis'">

                <!-- Section NC filter cards -->
                <div v-if="sectionKpiCards.length > 0" class="space-y-2">
                    <div class="flex items-center justify-between gap-3 px-1">
                        <div class="flex items-center gap-3 flex-wrap">
                            <div class="text-xs font-semibold text-slate-300 uppercase tracking-wider">
                                NC by Section<template v-if="filterDivisionId"> · {{ store.divisions.find(d => d.id === filterDivisionId)?.code }}</template>
                            </div>
                            <button
                                v-if="filterSection"
                                @click="clearSectionFilter"
                                data-testid="report-section-clear"
                                class="flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-blue-600/20 border border-blue-500/40 text-blue-300 hover:bg-blue-600/30 transition-colors"
                            >
                                <i class="pi pi-times text-xs" />
                                Clear: {{ filterSection }}
                            </button>
                        </div>
                        <span class="text-xs text-slate-500">Click a section to filter</span>
                    </div>
                    <div class="grid gap-2" style="grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));">
                        <button
                            v-for="card in sectionKpiCards"
                            :key="card.name"
                            @click="toggleSectionFilter(card.name)"
                            :title="card.name"
                            :data-testid="`report-section-card-${card.name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '')}`"
                            :class="[
                                'rounded-lg p-3 border text-left transition-all focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-400 flex flex-col',
                                filterSection === card.name
                                    ? 'bg-blue-900/40 border-blue-500 ring-1 ring-blue-500/60'
                                    : 'bg-slate-800 hover:border-slate-500',
                                filterSection && filterSection !== card.name ? 'opacity-40' : '',
                                sectionRateBorder(card.rate, filterSection === card.name),
                            ]"
                        >
                            <div class="text-xs text-slate-200 font-semibold leading-tight line-clamp-2 flex-1">
                                {{ card.shortName }}
                            </div>
                            <div class="text-xs text-slate-500 mt-1.5">
                                {{ card.ncCount }} NCs across {{ report?.totalAudits ?? 0 }} audits
                            </div>
                            <div class="flex items-baseline gap-1 mt-1">
                                <span :class="['text-lg font-bold', sectionRateColor(card.rate)]">{{ card.rate.toFixed(2) }}</span>
                                <span class="text-xs text-slate-400">avg NCs/audit</span>
                            </div>
                        </button>
                    </div>
                </div>

                <!-- Conformance Trend chart -->
                <Card v-if="report.trend.length > 0">
                    <template #title>
                        <div class="flex items-center justify-between w-full">
                            <div class="flex flex-col min-w-0">
                                <span class="text-base font-semibold text-white">{{ trendChartTitle }}</span>
                                <span v-if="activeFilterDesc" class="text-xs text-slate-400 mt-0.5">{{ activeFilterDesc }}</span>
                            </div>
                            <div class="flex items-center gap-2">
                                <div class="flex rounded overflow-hidden border border-slate-600 text-xs">
                                    <button :class="['px-2 py-0.5 transition-colors', chartTypes.conformance === 'bar' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']" @click="chartTypes.conformance = 'bar'">Bar</button>
                                    <button :class="['px-2 py-0.5 transition-colors', chartTypes.conformance === 'line' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']" @click="chartTypes.conformance = 'line'">Line</button>
                                </div>
                                <button @click.stop="toggleSection('conformanceTrend')" class="section-collapse-btn">
                                    <i :class="collapsed.conformanceTrend ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                                </button>
                            </div>
                        </div>
                    </template>
                    <template #content>
                        <div v-show="!collapsed.conformanceTrend">
                            <Chart :type="chartTypes.conformance" :key="chartTypes.conformance" :data="chartData" :options="chartOptions" style="height: 280px;" />
                        </div>
                    </template>
                </Card>

                <!-- Conformance by Division chart -->
                <Card v-if="divisionStats.length > 1">
                    <template #title>
                        <div class="flex items-center justify-between w-full">
                            <div class="flex flex-col min-w-0">
                                <span class="text-base font-semibold text-white">Conformance by Division</span>
                                <span v-if="activeFilterDesc" class="text-xs text-slate-400 mt-0.5">{{ activeFilterDesc }}</span>
                            </div>
                            <button @click.stop="toggleSection('divisionChart')" class="section-collapse-btn">
                                <i :class="collapsed.divisionChart ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </template>
                    <template #content>
                        <div v-show="!collapsed.divisionChart">
                            <Chart type="bar" :data="divisionChartData" :options="divisionChartOptions" :style="`height: ${Math.max(160, divisionStats.length * 40)}px;`" />
                        </div>
                    </template>
                </Card>

                <!-- NC by Section chart -->
                <Card v-if="(report.sectionBreakdown?.length ?? 0) > 0">
                    <template #title>
                        <div class="flex items-center justify-between w-full">
                            <div class="flex flex-col min-w-0">
                                <span class="text-base font-semibold text-white">Non-Conformances by Section</span>
                                <span v-if="activeFilterDesc" class="text-xs text-slate-400 mt-0.5">{{ activeFilterDesc }}</span>
                            </div>
                            <div class="flex items-center gap-2">
                                <div class="flex rounded overflow-hidden border border-slate-600 text-xs">
                                    <button :class="['px-2 py-0.5 transition-colors', chartTypes.ncSection === 'bar' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']" @click="chartTypes.ncSection = 'bar'">Bar</button>
                                    <button :class="['px-2 py-0.5 transition-colors', chartTypes.ncSection === 'line' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']" @click="chartTypes.ncSection = 'line'">Line</button>
                                </div>
                                <button @click.stop="toggleSection('ncSection')" class="section-collapse-btn">
                                    <i :class="collapsed.ncSection ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                                </button>
                            </div>
                        </div>
                    </template>
                    <template #content>
                        <div v-show="!collapsed.ncSection">
                            <Chart :type="chartTypes.ncSection" :key="chartTypes.ncSection" :data="ncCategoryChartData" :options="ncCategoryChartOptions" :style="`height: ${Math.max(160, (report.sectionBreakdown?.length ?? 0) * 36)}px;`" />
                        </div>
                    </template>
                </Card>

                <!-- Top locations by NC -->
                <Card v-if="locationStats.length >= 2">
                    <template #title>
                        <div class="flex items-center justify-between w-full">
                            <div class="flex flex-col min-w-0">
                                <span class="text-base font-semibold text-white">Top Locations by Non-Conformances</span>
                                <span v-if="activeFilterDesc" class="text-xs text-slate-400 mt-0.5">{{ activeFilterDesc }}</span>
                            </div>
                            <button @click.stop="toggleSection('topLocations')" class="section-collapse-btn">
                                <i :class="collapsed.topLocations ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </template>
                    <template #content>
                        <div v-show="!collapsed.topLocations">
                            <Chart type="bar" :data="locationChartData" :options="locationChartOptions" :style="`height: ${Math.max(160, locationStats.length * 36)}px;`" />
                        </div>
                    </template>
                </Card>

                <!-- Quarterly Trend chart -->
                <Card v-if="quarterlyTrendData.length > 1">
                    <template #title>
                        <div class="flex items-center justify-between w-full">
                            <div class="flex flex-col min-w-0">
                                <span class="text-base font-semibold text-white">Quarterly Conformance Trend</span>
                                <span v-if="activeFilterDesc" class="text-xs text-slate-400 mt-0.5">{{ activeFilterDesc }}</span>
                            </div>
                            <div class="flex items-center gap-2">
                                <div class="flex rounded overflow-hidden border border-slate-600 text-xs">
                                    <button :class="['px-2 py-0.5 transition-colors', chartTypes.quarterly === 'bar' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']" @click="chartTypes.quarterly = 'bar'">Bar</button>
                                    <button :class="['px-2 py-0.5 transition-colors', chartTypes.quarterly === 'line' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']" @click="chartTypes.quarterly = 'line'">Line</button>
                                </div>
                                <button @click.stop="toggleSection('quarterlyTrend')" class="section-collapse-btn">
                                    <i :class="collapsed.quarterlyTrend ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                                </button>
                            </div>
                        </div>
                    </template>
                    <template #content>
                        <div v-show="!collapsed.quarterlyTrend">
                            <Chart :type="chartTypes.quarterly" :key="chartTypes.quarterly" :data="quarterlyChartData" :options="quarterlyChartOptions" style="height: 260px;" />
                        </div>
                    </template>
                </Card>

            </template><!-- /analysis -->

            <!-- ── Tab: Action Items ─────────────────────────────────────────── -->
            <template v-if="activeTab === 'action-items'">

                <Card v-if="(report.openCorrectiveActions?.length ?? 0) > 0">
                    <template #title>
                        <div class="flex items-center justify-between w-full">
                            <div class="flex items-center gap-3 flex-wrap">
                                <span class="text-base font-semibold text-white">Open Corrective Actions</span>
                                <Tag :value="String(report.openCorrectiveActions.length)" severity="danger" />
                                <span v-if="caAgingStats.overdueCount > 0" class="text-xs text-red-400 font-medium">
                                    {{ caAgingStats.overdueCount }} past 14-day rule
                                </span>
                                <span class="text-xs text-slate-400">· Avg {{ caAgingStats.avgDaysOpen }} days open</span>
                            </div>
                            <button @click.stop="toggleSection('openCAs')" class="section-collapse-btn">
                                <i :class="collapsed.openCAs ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </template>
                    <template #content>
                        <div v-show="!collapsed.openCAs">
                        <!-- Phone card list for open CAs -->
                        <div v-if="isNarrow" class="space-y-2 py-2">
                            <div
                                v-for="ca in report.openCorrectiveActions"
                                :key="ca.id"
                                class="rounded-lg border px-3 py-2.5 flex flex-col gap-1.5 cursor-pointer transition-colors"
                                :class="ca.isOverdue ? 'border-red-700/50 bg-red-950/20' : 'border-slate-700/50 bg-slate-800/40'"
                                @click="router.push(`/audit-management/audits/${ca.auditId}/review`)"
                            >
                                <div class="flex items-center justify-between gap-2">
                                    <Tag :value="ca.isOverdue ? 'Overdue' : ca.status" :severity="ca.isOverdue ? 'danger' : 'warning'" class="!text-[11px]" />
                                    <span class="text-xs font-mono" :class="ca.daysOpen > 14 ? 'text-red-400 font-semibold' : ca.daysOpen > 7 ? 'text-amber-400' : 'text-emerald-400'">{{ ca.daysOpen }}d</span>
                                </div>
                                <p class="text-sm text-slate-200 line-clamp-2 leading-snug">{{ ca.description }}</p>
                                <div class="flex items-center justify-between text-xs text-slate-400">
                                    <span v-if="ca.assignedTo"><i class="pi pi-user mr-1 text-[10px]" />{{ ca.assignedTo }}</span>
                                    <span v-else class="text-slate-500">Unassigned</span>
                                    <span :class="ca.isOverdue ? 'text-red-400 font-semibold' : ''">{{ ca.dueDate ?? '—' }}</span>
                                </div>
                            </div>
                        </div>
                        <!-- Desktop table for open CAs -->
                        <DataTable v-else :value="report.openCorrectiveActions" :rows="10" paginator sortField="dueDate" :sortOrder="1" class="stronghold-table text-sm" :row-class="(r: any) => r.isOverdue ? 'row--overdue' : ''" scrollable>
                            <Column field="id" header="CA #" style="width:60px" sortable />
                            <Column field="auditId" header="Audit #" style="width:80px" sortable />
                            <Column field="description" header="Description">
                                <template #body="{ data }"><span class="line-clamp-2">{{ data.description }}</span></template>
                            </Column>
                            <Column field="assignedTo" header="Assigned To" sortable>
                                <template #body="{ data }">{{ data.assignedTo ?? '—' }}</template>
                            </Column>
                            <Column field="dueDate" header="Due Date" sortable>
                                <template #body="{ data }">
                                    <span :class="data.isOverdue ? 'text-[color:var(--color-danger)] font-semibold' : ''">{{ data.dueDate ?? '—' }}</span>
                                </template>
                            </Column>
                            <Column field="daysOpen" header="Age" sortable style="width:80px">
                                <template #body="{ data }">
                                    <span :class="data.daysOpen > 14 ? 'text-red-400 font-semibold' : data.daysOpen > 7 ? 'text-amber-400' : 'text-emerald-400'">{{ data.daysOpen }}d</span>
                                </template>
                            </Column>
                            <Column field="status" header="Status" sortable style="width:110px">
                                <template #body="{ data }">
                                    <Tag :value="data.isOverdue ? 'Overdue' : data.status" :severity="data.isOverdue ? 'danger' : 'warning'" />
                                </template>
                            </Column>
                            <Column header="" style="width:50px">
                                <template #body="{ data }">
                                    <Button icon="pi pi-eye" size="small" severity="secondary" text @click="router.push(`/audit-management/audits/${data.auditId}/review`)" />
                                </template>
                            </Column>
                        </DataTable>
                        </div>
                    </template>
                </Card>

                <div v-else class="text-center py-12 text-slate-400">
                    <i class="pi pi-check-circle text-4xl mb-3 block opacity-30 text-emerald-400" />
                    <p class="text-sm text-emerald-400">No open corrective actions.</p>
                </div>

            </template><!-- /action-items -->

            <!-- ── Tab: Performance ──────────────────────────────────────────── -->
            <template v-if="activeTab === 'performance'">

                <Card v-if="auditorStats.length > 0">
                    <template #title>
                        <div class="flex items-center justify-between w-full">
                            <span class="text-base font-semibold text-white">Auditor Performance</span>
                            <button @click.stop="toggleSection('auditorPerf')" class="section-collapse-btn">
                                <i :class="collapsed.auditorPerf ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </template>
                    <template #content>
                        <div v-show="!collapsed.auditorPerf">
                        <!-- Phone card list for auditor performance -->
                        <div v-if="isNarrow" class="space-y-2 py-2">
                            <div
                                v-for="stat in auditorStats"
                                :key="stat.auditor"
                                class="rounded-lg border border-slate-700/50 bg-slate-800/40 px-3 py-2.5 flex flex-col gap-1.5 cursor-pointer transition-colors hover:border-blue-500/30"
                                @click="drillByAuditor(stat.auditor)"
                            >
                                <div class="flex items-center justify-between gap-2">
                                    <span class="font-semibold text-blue-300 text-sm">{{ stat.auditor }}</span>
                                    <span :class="['text-sm font-bold', rowScoreColor(stat.avgScore)]">
                                        {{ stat.avgScore != null ? `${stat.avgScore}%` : '—' }}
                                    </span>
                                </div>
                                <div class="flex flex-wrap items-center gap-x-3 gap-y-0.5 text-xs text-slate-400">
                                    <span>{{ stat.auditCount }} audit{{ stat.auditCount !== 1 ? 's' : '' }}</span>
                                    <span v-if="stat.totalNcs > 0" class="text-red-400 font-semibold">{{ stat.totalNcs }} NC</span>
                                    <span v-if="stat.totalWarnings > 0" class="text-amber-400">{{ stat.totalWarnings }} warn</span>
                                </div>
                            </div>
                        </div>
                        <!-- Desktop table for auditor performance -->
                        <DataTable v-else :value="auditorStats" sortField="avgScore" :sortOrder="-1" class="stronghold-table text-sm">
                            <Column field="auditor" header="Auditor" sortable>
                                <template #body="{ data }">
                                    <button class="auditor-link" @click="drillByAuditor(data.auditor)" :title="`Show ${data.auditor}'s audits`">{{ data.auditor }}</button>
                                </template>
                            </Column>
                            <Column field="auditCount" header="Audits" sortable style="width:80px" />
                            <Column field="avgScore" header="Avg Score" sortable style="width:110px">
                                <template #body="{ data }">
                                    <span :class="rowScoreColor(data.avgScore)">{{ data.avgScore != null ? `${data.avgScore}%` : '—' }}</span>
                                </template>
                            </Column>
                            <Column field="totalNcs" header="Total NCs" sortable style="width:100px">
                                <template #body="{ data }">
                                    <span :class="data.totalNcs > 0 ? 'text-red-400 font-semibold' : 'text-slate-400'">{{ data.totalNcs }}</span>
                                </template>
                            </Column>
                            <Column field="totalWarnings" header="Warnings" sortable style="width:100px">
                                <template #body="{ data }">
                                    <span :class="data.totalWarnings > 0 ? 'text-amber-400' : 'text-slate-400'">{{ data.totalWarnings }}</span>
                                </template>
                            </Column>
                        </DataTable>
                        </div>
                    </template>
                </Card>

            </template><!-- /performance -->

            <!-- ── Tab: History ──────────────────────────────────────────────── -->
            <template v-if="activeTab === 'history'">

                <Card ref="auditDetailCard">
                    <template #title>
                        <div class="flex items-center justify-between w-full">
                            <div class="flex items-center gap-3 flex-wrap">
                                <span class="text-base font-semibold text-white">Audit History</span>
                                <button v-if="drillAuditor" @click="drillAuditor = null"
                                    class="flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-blue-600/20 border border-blue-500/40 text-blue-300 hover:bg-blue-600/30 transition-colors">
                                    <i class="pi pi-times text-xs" /> {{ drillAuditor }}
                                </button>
                                <button v-if="drillLocation" @click="drillLocation = null"
                                    class="flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-red-600/20 border border-red-500/40 text-red-300 hover:bg-red-600/30 transition-colors">
                                    <i class="pi pi-times text-xs" /> {{ drillLocation }}
                                </button>
                                <button v-if="drillNcOnly" @click="drillNcOnly = false"
                                    class="flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-red-600/20 border border-red-500/40 text-red-300 hover:bg-red-600/30 transition-colors">
                                    <i class="pi pi-times text-xs" /> NCs only
                                </button>
                                <button v-if="drillWarnOnly" @click="drillWarnOnly = false"
                                    class="flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-amber-600/20 border border-amber-500/40 text-amber-300 hover:bg-amber-600/30 transition-colors">
                                    <i class="pi pi-times text-xs" /> Warnings only
                                </button>
                            </div>
                            <button @click.stop="toggleSection('auditDetail')" class="section-collapse-btn">
                                <i :class="collapsed.auditDetail ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </template>
                    <template #content>
                        <div v-show="!collapsed.auditDetail">
                        <!-- Phone card list for audit history -->
                        <div v-if="isNarrow" class="space-y-2 py-2">
                            <div
                                v-for="row in filteredAuditRows"
                                :key="row.id"
                                class="rounded-lg border border-slate-700/50 bg-slate-800/40 px-3 py-2.5 flex flex-col gap-1.5 cursor-pointer transition-colors hover:border-blue-500/30"
                                @click="router.push(`/audit-management/audits/${row.id}/review`)"
                                :data-testid="`report-grid-row`"
                            >
                                <div class="flex items-center justify-between gap-2">
                                    <div class="flex items-center gap-2 flex-wrap">
                                        <span class="font-semibold text-white text-sm">{{ row.divisionCode }}</span>
                                        <Tag :value="row.status" :severity="statusSeverity(row.status)" class="!text-[11px]" />
                                    </div>
                                    <span :class="['text-sm font-bold', rowScoreColor(row.scorePercent)]">
                                        {{ row.scorePercent != null ? `${row.scorePercent}%` : '—' }}
                                    </span>
                                </div>
                                <div class="flex flex-wrap items-center gap-x-3 gap-y-0.5 text-xs text-slate-400">
                                    <span>{{ row.auditDate ?? '—' }}</span>
                                    <span v-if="row.auditor">{{ row.auditor }}</span>
                                    <span v-if="row.nonConformingCount > 0" class="text-red-400 font-semibold">{{ row.nonConformingCount }} NC</span>
                                    <span v-if="row.warningCount > 0" class="text-amber-400">{{ row.warningCount }} warn</span>
                                </div>
                            </div>
                        </div>
                        <!-- Desktop table for audit history -->
                        <DataTable v-else :value="filteredAuditRows" :rows="20" paginator sortField="id" :sortOrder="-1" class="stronghold-table text-sm" scrollable>
                            <Column field="id" header="#" style="width:60px" sortable>
                                <template #body="{ data }"><span data-testid="report-grid-row">{{ data.id }}</span></template>
                            </Column>
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
                                <template #body="{ data }"><Tag :value="data.status" :severity="statusSeverity(data.status)" /></template>
                            </Column>
                            <Column field="scorePercent" header="Score" sortable>
                                <template #body="{ data }">
                                    <span :class="rowScoreColor(data.scorePercent)">{{ data.scorePercent != null ? `${data.scorePercent}%` : '—' }}</span>
                                </template>
                            </Column>
                            <Column field="nonConformingCount" header="NCs" sortable style="width:60px">
                                <template #body="{ data }">
                                    <span :class="data.nonConformingCount > 0 ? 'text-red-400 font-semibold' : 'text-slate-400'">{{ data.nonConformingCount }}</span>
                                </template>
                            </Column>
                            <Column field="warningCount" header="Warn" sortable style="width:60px">
                                <template #body="{ data }">
                                    <span :class="data.warningCount > 0 ? 'text-amber-400' : 'text-slate-400'">{{ data.warningCount }}</span>
                                </template>
                            </Column>
                            <Column header="" style="width:60px">
                                <template #body="{ data }">
                                    <Button icon="pi pi-eye" size="small" severity="secondary" text @click="router.push(`/audit-management/audits/${data.id}/review`)" />
                                </template>
                            </Column>
                        </DataTable>
                        </div>
                    </template>
                </Card>

            </template><!-- /history -->

        </div>

        <div v-else class="p-4 text-center text-slate-400 py-16">
            No report data available.
        </div>
    </div>
</template>

<script setup lang="ts">
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
import SplitButton from 'primevue/splitbutton';
import Menu from 'primevue/menu';
import OverlayPanel from 'primevue/overlaypanel';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import MetricCard from '@/components/ui/MetricCard.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useAuditReportData } from '../composables/useAuditReportData';
import { useReportKpis } from '../composables/useReportKpis';
import { useReportCharts } from '../composables/useReportCharts';
import { useReportDrilldowns } from '../composables/useReportDrilldowns';

import { useNarrowScreen } from '@/composables/useNarrowScreen';

const router = useRouter();
const store  = useAuditStore();
const { isNarrow } = useNarrowScreen();

// ── Composables ───────────────────────────────────────────────────────────────
const {
    loading, report, complianceStatus, exportingQs, exportingNcr,
    filterDivisionId, filterStatus, filterDateFrom, filterDateTo, filterSection,
    STATUS_OPTIONS, activeFilterChips, activeFilterDesc,
    divisionHealthCards, recentAuditFeed, overdueAlertCas, exportMenuItems,
    loadReport, toggleSectionFilter, clearSectionFilter,
    filterByDivisionCode, complianceStatusLabel,
    exportCsv, exportQuarterlySummary, exportNcrReport, openQSummary,
} = useAuditReportData();

const {
    displayTotalAudits, displayNonConforming, displayWarnings, displayAvgScore,
    displayCorrectedPct, displayCaAging,
    KPI_CARDS, hidden, hideSection, toggleCard, showAll, hideAll,
    customizePanelRef, toggleCustomize,
    trendDeltas, correctedOnSitePct, caAgingStats,
    scoreColor, scoreVariant, rowScoreColor, statusSeverity, auditStatusSeverity,
    sectionRateColor, sectionRateBorder, SECTION_SHORT, sectionKpiCards,
} = useReportKpis(report);

const {
    chartTypes,
    locationStats, locationChartData, locationChartOptions, locationDrillCallback,
    primaryLabel, trendChartTitle, chartData, chartOptions,
    divisionStats, divisionChartData, divisionChartOptions,
    quarterlyTrendData, quarterlyChartData, quarterlyChartOptions,
    auditorStats, ncCategoryChartData, ncCategoryChartOptions,
} = useReportCharts({ report, filterDivisionId, filterSection, filterDateFrom, filterDateTo });

const {
    TABS, activeTab, tabBarEl, scrollToTabs,
    collapsed, toggleSection, divisionHealthCollapsed,
    drillAuditor, drillLocation, drillNcOnly, drillWarnOnly, auditDetailCard,
    scrollToAuditDetail,
    drillByAuditor, drillByLocation, drillByNcOnly, drillByWarnOnly, drillAllAudits,
    filteredAuditRows,
    reportsMenuRef, reportsMenuItems, toggleReportsMenu,
} = useReportDrilldowns({ report, openQSummary });

// Wire location chart click → drilldown
locationDrillCallback.value = drillByLocation;

function auditStatusLabel(status: string): string {
    const labels: Record<string, string> = {
        Draft: 'Draft', Submitted: 'Submitted', Reopened: 'Reopened',
        UnderReview: 'In Review', Approved: 'Approved', Distributed: 'Distributed', Closed: 'Closed',
    };
    return labels[status] ?? status;
}
</script>

<style scoped>
/* ── Overdue CA alert banner ─────────────────────────────────────────────────── */
.overdue-alert-banner {
    display: flex;
    align-items: flex-start;
    gap: 0.75rem;
    border-radius: var(--radius-card);
    border: 1px solid rgba(239, 68, 68, 0.35);
    background: rgba(239, 68, 68, 0.08);
    padding: 0.75rem 1rem;
    cursor: pointer;
    transition: background var(--transition-base);
}
.overdue-alert-banner:hover {
    background: rgba(239, 68, 68, 0.13);
}

/* ── Wide summary KPI cards (Row 2 only — headline Row 1 uses MetricCard) ────── */
.kpi-card {
    background: var(--surface-2);
    border: 1px solid var(--border);
    border-radius: var(--radius-card);
    padding: 1rem;
    text-align: center;
    cursor: pointer;
    transition: box-shadow var(--transition-slow), transform var(--transition-slow), border-color var(--transition-slow);
}
.kpi-card:hover {
    box-shadow: 0 0 0 1px rgba(99, 179, 237, 0.4), 0 8px 28px rgba(0, 0, 0, 0.55);
    transform: translateY(-3px);
    border-color: rgba(99, 179, 237, 0.45);
}
.kpi-card--danger { border-color: rgba(var(--color-danger), 0.3); }
.kpi-card--danger:hover {
    box-shadow: 0 0 0 1px rgba(239, 68, 68, 0.45), 0 8px 28px rgba(0, 0, 0, 0.55);
    border-color: rgba(239, 68, 68, 0.5);
}
.kpi-card--warn { border-color: rgba(120, 53, 15, 0.5); }
.kpi-card--warn:hover {
    box-shadow: 0 0 0 1px rgba(245, 158, 11, 0.45), 0 8px 28px rgba(0, 0, 0, 0.55);
    border-color: rgba(245, 158, 11, 0.5);
}
.kpi-card--wide { text-align: left; }

/* ── Hide button (appears on hover) ─────────────────────────────────────────── */
.kpi-hide-btn {
    position: absolute;
    top: 5px;
    right: 5px;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    border: none;
    background: transparent;
    color: rgba(148, 163, 184, 0.3);
    font-size: 0.6rem;
    cursor: pointer;
    opacity: 1;
    transition: background 0.15s ease, color 0.15s ease;
    display: flex;
    align-items: center;
    justify-content: center;
}
.kpi-hide-btn:hover {
    background: rgba(239, 68, 68, 0.18);
    color: #f87171;
}

/* ── Drill hint arrow (bottom-right corner) ─────────────────────────────────── */
.kpi-tap-hint {
    position: absolute;
    bottom: 6px;
    right: 8px;
    font-size: 0.6rem;
    color: rgba(148, 163, 184, 0.2);
    transition: color 0.15s ease;
    pointer-events: none;
}
.kpi-card:hover .kpi-tap-hint { color: rgba(99, 179, 237, 0.6); }

.section-collapse-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    border-radius: 6px;
    border: 1px solid rgba(100, 116, 139, 0.3);
    background: transparent;
    color: #94a3b8;
    font-size: 0.75rem;
    cursor: pointer;
    transition: background 0.15s ease, color 0.15s ease, border-color 0.15s ease;
    flex-shrink: 0;
}
.section-collapse-btn:hover {
    background: rgba(99, 179, 237, 0.12);
    color: #63b3ed;
    border-color: rgba(99, 179, 237, 0.4);
}

.auditor-link {
    background: none;
    border: none;
    padding: 0;
    color: #63b3ed;
    font-size: inherit;
    cursor: pointer;
    text-decoration: underline;
    text-decoration-color: rgba(99, 179, 237, 0.4);
    text-underline-offset: 2px;
    transition: color 0.15s ease, text-decoration-color 0.15s ease;
}
.auditor-link:hover {
    color: #90cdf4;
    text-decoration-color: #90cdf4;
}

/* ── Tab strip ───────────────────────────────────────────────────────────────── */
.tab-btn {
    display: inline-flex;
    align-items: center;
    padding: 0.45rem 0.9rem;
    font-size: 0.8rem;
    font-weight: 500;
    color: #94a3b8;
    background: transparent;
    border: none;
    border-bottom: 2px solid transparent;
    margin-bottom: -1px;
    cursor: pointer;
    transition: color 0.15s ease, border-color 0.15s ease;
    white-space: nowrap;
}
.tab-btn:hover { color: #e2e8f0; }
.tab-btn--active {
    color: #63b3ed;
    border-bottom-color: #63b3ed;
    font-weight: 600;
}

/* ── Division health cards ────────────────────────────────────────────────────── */
.division-health-card {
    transition: box-shadow 0.2s ease, transform 0.15s ease;
}
.division-health-card:hover {
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
    transform: translateY(-2px);
}

/* ── Recent Audits list ───────────────────────────────────────────────────────── */
.recent-audits-list {
    background: var(--surface-2, #1e293b);
}
.recent-audit-row {
    background: transparent;
    transition: background 0.12s ease;
}
.recent-audit-row:hover {
    background: rgba(99, 179, 237, 0.06);
}
.recent-audit-row:first-child:hover {
    border-radius: 0.75rem 0.75rem 0 0;
}
.recent-audit-row:last-child:hover {
    border-radius: 0 0 0.75rem 0.75rem;
}
</style>

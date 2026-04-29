<template>
    <div>
        <!-- Page header — 3 buttons only -->
        <BasePageHeader
            title="Audit Dashboard"
            subtitle="Conformance trends and non-conformance analysis"
            icon="pi pi-chart-bar"
        >
            <Button icon="pi pi-refresh" severity="secondary" outlined size="small" :loading="loading" @click="loadReport" />
            <Button icon="pi pi-sliders-h" severity="secondary" outlined size="small" title="Customize widgets" @click="toggleCustomize" />
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
            <Button
                label="Reports"
                icon="pi pi-file-edit"
                severity="secondary"
                outlined
                size="small"
                @click="toggleReportsMenu"
            />
            <Menu ref="reportsMenuRef" :model="reportsMenuItems" popup />
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

        <!-- Filter bar -->
        <div class="px-4 pt-3 pb-2 flex flex-wrap gap-3 items-end border-b border-slate-700/40">
            <div class="flex flex-col gap-1">
                <label class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">Division</label>
                <Dropdown
                    v-model="filterDivisionId"
                    :options="store.divisions"
                    option-label="code"
                    option-value="id"
                    placeholder="All Divisions"
                    class="w-44"
                    :show-clear="!!filterDivisionId"
                    @change="loadReport"
                    data-testid="report-filter-division"
                />
            </div>
            <div class="flex flex-col gap-1">
                <label class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">Status</label>
                <Dropdown
                    v-model="filterStatus"
                    :options="STATUS_OPTIONS"
                    option-label="label"
                    option-value="value"
                    placeholder="All Statuses"
                    class="w-40"
                    :show-clear="!!filterStatus"
                    @change="loadReport"
                    data-testid="report-filter-status"
                />
            </div>
            <div class="flex flex-col gap-1">
                <label class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">From</label>
                <Calendar
                    v-model="filterDateFrom"
                    placeholder="From date"
                    dateFormat="yy-mm-dd"
                    class="w-36"
                    :show-clear="!!filterDateFrom"
                    @date-select="loadReport"
                    @clear-click="loadReport"
                    data-testid="report-filter-from"
                />
            </div>
            <div class="flex flex-col gap-1">
                <label class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">To</label>
                <Calendar
                    v-model="filterDateTo"
                    placeholder="To date"
                    dateFormat="yy-mm-dd"
                    class="w-36"
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
                <div
                    v-if="!hidden.kpiTotalAudits"
                    class="kpi-card group relative"
                    @click="router.push('/audit-management/audits')"
                    data-testid="report-kpi-total-audits"
                    title="View all audits"
                >
                    <button @click.stop="hideSection('kpiTotalAudits')" class="kpi-hide-btn" title="Hide"><i class="pi pi-times" /></button>
                    <div class="text-3xl font-bold text-white">{{ displayTotalAudits }}</div>
                    <div class="text-xs text-slate-400 mt-1">Total Audits</div>
                    <div v-if="trendDeltas.auditCountDelta !== null" class="text-xs mt-1"
                        :class="trendDeltas.auditCountDelta >= 0 ? 'text-emerald-400' : 'text-red-400'">
                        {{ trendDeltas.auditCountDelta >= 0 ? '↑' : '↓' }}
                        {{ Math.abs(trendDeltas.auditCountDelta) }} vs prior period
                    </div>
                    <div class="kpi-tap-hint"><i class="pi pi-arrow-up-right" /></div>
                </div>
                <div
                    v-if="!hidden.kpiAvgConformance"
                    class="kpi-card group relative"
                    @click="drillAllAudits"
                    title="View all audits ranked by score"
                >
                    <button @click.stop="hideSection('kpiAvgConformance')" class="kpi-hide-btn" title="Hide"><i class="pi pi-times" /></button>
                    <div :class="['text-3xl font-bold', scoreColor]">
                        {{ report.avgScorePercent != null ? `${displayAvgScore}%` : '—' }}
                    </div>
                    <div class="text-xs text-slate-400 mt-1">Avg Conformance</div>
                    <div v-if="trendDeltas.scoreDelta !== null" class="text-xs mt-1"
                        :class="trendDeltas.scoreDelta >= 0 ? 'text-emerald-400' : 'text-red-400'">
                        {{ trendDeltas.scoreDelta >= 0 ? '↑' : '↓' }}
                        {{ Math.abs(trendDeltas.scoreDelta) }}% vs prior period
                    </div>
                    <div class="kpi-tap-hint"><i class="pi pi-arrow-up-right" /></div>
                </div>
                <div
                    v-if="!hidden.kpiNonConformances"
                    class="kpi-card kpi-card--danger group relative"
                    @click="drillByNcOnly"
                    data-testid="report-kpi-nc-count"
                    title="Filter to audits with non-conformances"
                >
                    <button @click.stop="hideSection('kpiNonConformances')" class="kpi-hide-btn" title="Hide"><i class="pi pi-times" /></button>
                    <div class="text-3xl font-bold text-red-400">{{ displayNonConforming }}</div>
                    <div class="text-xs text-slate-400 mt-1">Non-Conformances</div>
                    <div v-if="trendDeltas.ncDelta !== null" class="text-xs mt-1"
                        :class="trendDeltas.ncDelta <= 0 ? 'text-emerald-400' : 'text-red-400'">
                        {{ trendDeltas.ncDelta <= 0 ? '↓' : '↑' }}
                        {{ Math.abs(trendDeltas.ncDelta) }} vs prior period
                    </div>
                    <div class="kpi-tap-hint"><i class="pi pi-arrow-up-right" /></div>
                </div>
                <div
                    v-if="!hidden.kpiWarnings"
                    class="kpi-card kpi-card--warn group relative"
                    @click="drillByWarnOnly"
                    data-testid="report-kpi-warning-count"
                    title="Filter to audits with warnings"
                >
                    <button @click.stop="hideSection('kpiWarnings')" class="kpi-hide-btn" title="Hide"><i class="pi pi-times" /></button>
                    <div class="text-3xl font-bold text-amber-400">{{ displayWarnings }}</div>
                    <div class="text-xs text-slate-400 mt-1">Warnings</div>
                    <div v-if="trendDeltas.warnDelta !== null" class="text-xs mt-1"
                        :class="trendDeltas.warnDelta <= 0 ? 'text-emerald-400' : 'text-amber-400'">
                        {{ trendDeltas.warnDelta <= 0 ? '↓' : '↑' }}
                        {{ Math.abs(trendDeltas.warnDelta) }} vs prior period
                    </div>
                    <div class="kpi-tap-hint"><i class="pi pi-arrow-up-right" /></div>
                </div>
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
                class="flex items-start gap-3 rounded-lg border border-red-700/50 bg-red-900/20 px-4 py-3 cursor-pointer hover:bg-red-900/30 transition-colors"
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
            <div ref="tabBarEl" class="border-b border-slate-700 flex items-center gap-1 -mx-4 px-4 pt-1">
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

                <!-- Division Health: collapsed by default -->
                <div v-if="divisionHealthCards.length > 0" class="space-y-2">
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
                        class="grid gap-3"
                        :class="divisionHealthCards.length <= 2 ? 'grid-cols-1 sm:grid-cols-2' : divisionHealthCards.length <= 4 ? 'grid-cols-2 lg:grid-cols-4' : 'grid-cols-2 md:grid-cols-3 xl:grid-cols-5'">
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
                            <div class="flex items-center gap-3">
                                <div
                                    class="text-2xl font-bold leading-none"
                                    :class="div.avgScore == null ? 'text-slate-500' : div.avgScore >= 90 ? 'text-emerald-400' : div.avgScore >= 75 ? 'text-amber-400' : 'text-red-400'"
                                >
                                    {{ div.avgScore != null ? `${div.avgScore}%` : '—' }}
                                </div>
                                <div class="flex-1 h-1.5 rounded-full bg-slate-700 overflow-hidden">
                                    <div
                                        v-if="div.avgScore != null"
                                        class="h-full rounded-full transition-all"
                                        :style="`width: ${div.avgScore}%`"
                                        :class="div.avgScore >= 90 ? 'bg-emerald-500' : div.avgScore >= 75 ? 'bg-amber-500' : 'bg-red-500'"
                                    />
                                </div>
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

                <!-- Recent Audits -->
                <div v-if="recentAuditFeed.length > 0" class="space-y-2">
                    <div class="flex items-center justify-between px-1">
                        <span class="text-xs font-semibold text-slate-300 uppercase tracking-wider">Recent Audits</span>
                        <button @click="router.push('/audit-management/audits')" class="text-xs text-blue-400 hover:text-blue-300 transition-colors">View all →</button>
                    </div>
                    <div class="rounded-xl border border-slate-700 bg-slate-800 overflow-hidden">
                        <div
                            v-for="(audit, i) in recentAuditFeed"
                            :key="audit.id"
                            class="flex items-center gap-3 px-4 py-3 hover:bg-slate-750 cursor-pointer transition-colors group"
                            :class="i > 0 ? 'border-t border-slate-700/50' : ''"
                            @click="router.push(audit.status === 'Draft' ? `/audit-management/audits/${audit.id}` : `/audit-management/audits/${audit.id}/review`)"
                        >
                            <div
                                class="w-11 h-11 rounded-lg flex items-center justify-center text-sm font-bold shrink-0"
                                :class="audit.scorePercent == null ? 'bg-slate-700 text-slate-400'
                                    : audit.scorePercent >= 90 ? 'bg-emerald-900/60 text-emerald-300 border border-emerald-700/40'
                                    : audit.scorePercent >= 75 ? 'bg-amber-900/60 text-amber-300 border border-amber-700/40'
                                    : 'bg-red-900/60 text-red-300 border border-red-700/40'"
                            >
                                {{ audit.scorePercent != null ? `${audit.scorePercent}%` : '—' }}
                            </div>
                            <div class="flex-1 min-w-0">
                                <div class="flex items-center gap-2 flex-wrap">
                                    <span class="text-sm font-medium text-white">{{ audit.divisionCode }}</span>
                                    <Tag :value="audit.status" :severity="auditStatusSeverity(audit.status)" class="text-[11px]" />
                                    <span v-if="audit.nonConformingCount > 0" class="text-[11px] text-red-400">
                                        {{ audit.nonConformingCount }} NC{{ audit.nonConformingCount === 1 ? '' : 's' }}
                                    </span>
                                </div>
                                <div class="text-xs text-slate-400 mt-0.5">
                                    {{ audit.auditor ?? 'No auditor' }}
                                    <span v-if="audit.jobNumber"> · {{ audit.jobNumber }}</span>
                                </div>
                            </div>
                            <div class="text-right shrink-0">
                                <div class="text-xs text-slate-400">{{ audit.auditDate ?? '—' }}</div>
                                <i class="pi pi-arrow-right text-xs text-slate-600 group-hover:text-slate-400 transition-colors mt-1" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Empty state -->
                <div v-if="divisionHealthCards.length === 0 && recentAuditFeed.length === 0"
                    class="text-center py-12 text-slate-400">
                    <i class="pi pi-chart-bar text-4xl mb-3 block opacity-30" />
                    <p class="text-sm">No audit data yet.</p>
                    <p class="text-xs mt-1 opacity-70">Create an audit to see data here.</p>
                </div>

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
                        <DataTable :value="report.openCorrectiveActions" :rows="10" paginator sortField="dueDate" :sortOrder="1" class="text-sm">
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
                                    <span :class="data.isOverdue ? 'text-red-400 font-semibold' : ''">{{ data.dueDate ?? '—' }}</span>
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
                        <DataTable :value="auditorStats" sortField="avgScore" :sortOrder="-1" class="text-sm">
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
                        <DataTable :value="filteredAuditRows" :rows="20" paginator sortField="id" :sortOrder="-1" class="text-sm">
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
import { computed, nextTick, onMounted, reactive, ref, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
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
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type AuditReportDto, type ComplianceStatusDto } from '@/apiclient/auditClient';

const router = useRouter();
const route = useRoute();
const store = useAuditStore();
const apiStore = useApiStore();

const loading = ref(false);
const report = ref<AuditReportDto | null>(null);
const complianceStatus = ref<ComplianceStatusDto[]>([]);
const exportingQs = ref(false);
const exportingNcr = ref(false);
const filterDivisionId = ref<number | null>(null);
const filterStatus = ref<string | null>(null);
const filterDateFrom = ref<Date | null>(null);
const filterDateTo = ref<Date | null>(null);
const filterSection = ref<string | null>((route.query.section as string) || null);

const STATUS_OPTIONS = [
    { label: 'Submitted', value: 'Submitted' },
    { label: 'Closed', value: 'Closed' },
    { label: 'Draft', value: 'Draft' },
];

// ── Reports menu (header) ─────────────────────────────────────────────────────
const reportsMenuRef = ref<any>(null);
const reportsMenuItems = [
    { label: 'Report Composer', icon: 'pi pi-file-edit', command: () => router.push('/audit-management/reports/composer') },
    { label: 'Generate PDF', icon: 'pi pi-file-pdf', command: () => router.push('/audit-management/reports/gallery') },
    { separator: true },
    { label: 'By Employee', icon: 'pi pi-users', command: () => router.push('/audit-management/reports/by-employee') },
    { label: 'Quarterly Summary', icon: 'pi pi-print', command: () => openQSummary() },
];
function toggleReportsMenu(event: Event) {
    reportsMenuRef.value?.toggle(event);
}

// ── Section collapse state ─────────────────────────────────────────────────────
const collapsed = reactive<Record<string, boolean>>({});
function toggleSection(key: string) {
    collapsed[key] = !collapsed[key];
}

// ── Division health collapsed by default ──────────────────────────────────────
const divisionHealthCollapsed = ref(true);

// ── Drill-down filters (auditor / location → History tab) ─────────────────────
const drillAuditor = ref<string | null>(null);
const drillLocation = ref<string | null>(null);
const auditDetailCard = ref<any>(null);

function scrollToAuditDetail() {
    nextTick(() => {
        const el = auditDetailCard.value?.$el ?? auditDetailCard.value;
        el?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
}

function drillByAuditor(auditor: string) {
    drillAuditor.value = auditor;
    drillLocation.value = null;
    collapsed.auditDetail = false;
    activeTab.value = 'history';
    scrollToTabs();
}

function drillByLocation(location: string) {
    drillLocation.value = location;
    drillAuditor.value = null;
    collapsed.auditDetail = false;
    activeTab.value = 'history';
    scrollToTabs();
}

const drillNcOnly = ref(false);
const drillWarnOnly = ref(false);

function drillByNcOnly() {
    drillNcOnly.value = true;
    drillWarnOnly.value = false;
    drillAuditor.value = null;
    drillLocation.value = null;
    collapsed.auditDetail = false;
    activeTab.value = 'history';
    scrollToTabs();
}

function drillByWarnOnly() {
    drillWarnOnly.value = true;
    drillNcOnly.value = false;
    drillAuditor.value = null;
    drillLocation.value = null;
    collapsed.auditDetail = false;
    activeTab.value = 'history';
    scrollToTabs();
}

function drillAllAudits() {
    drillNcOnly.value = false;
    drillWarnOnly.value = false;
    drillAuditor.value = null;
    drillLocation.value = null;
    collapsed.auditDetail = false;
    activeTab.value = 'history';
    scrollToTabs();
}

const filteredAuditRows = computed(() => {
    let rows = report.value?.rows ?? [];
    if (drillAuditor.value) rows = rows.filter(r => r.auditor === drillAuditor.value);
    if (drillLocation.value) rows = rows.filter(r => r.location === drillLocation.value);
    if (drillNcOnly.value) rows = rows.filter(r => r.nonConformingCount > 0);
    if (drillWarnOnly.value) rows = rows.filter(r => r.warningCount > 0);
    return rows;
});

// ── KPI count-up animations ───────────────────────────────────────────────────
function useCountUp(source: () => number, duration = 900) {
    const display = ref(0);
    watch(source, (to) => {
        if (!Number.isFinite(to)) { display.value = 0; return; }
        const from = display.value;
        const start = performance.now();
        function tick(now: number) {
            const t = Math.min((now - start) / duration, 1);
            const ease = 1 - Math.pow(1 - t, 3);
            display.value = Math.round(from + (to - from) * ease);
            if (t < 1) requestAnimationFrame(tick);
        }
        requestAnimationFrame(tick);
    }, { immediate: true });
    return display;
}
const displayTotalAudits   = useCountUp(() => report.value?.totalAudits ?? 0);
const displayNonConforming = useCountUp(() => report.value?.totalNonConforming ?? 0);
const displayWarnings      = useCountUp(() => report.value?.totalWarnings ?? 0);
const displayAvgScore      = useCountUp(() => report.value?.avgScorePercent ?? 0);

// ── Dashboard section visibility (KPI tiles only) ─────────────────────────────
const KPI_CARDS = [
    { key: 'kpiTotalAudits',      label: 'Total Audits' },
    { key: 'kpiAvgConformance',   label: 'Avg Conformance' },
    { key: 'kpiNonConformances',  label: 'Non-Conformances' },
    { key: 'kpiWarnings',         label: 'Warnings' },
    { key: 'kpiCorrectedOnSite',  label: 'Corrected On Site' },
    { key: 'kpiCaAging',          label: 'CAs Past 14-Day Rule' },
];

const hidden = reactive<Record<string, boolean>>(
    (() => { try { return JSON.parse(localStorage.getItem('dashboard-hidden') ?? '{}'); } catch { return {}; } })()
);
watch(hidden, val => localStorage.setItem('dashboard-hidden', JSON.stringify(val)), { deep: true });
function hideSection(key: string) { hidden[key] = true; }
function toggleCard(key: string) { hidden[key] = !hidden[key]; }
function showAll() { KPI_CARDS.forEach(c => delete hidden[c.key]); }
function hideAll() { KPI_CARDS.forEach(c => { hidden[c.key] = true; }); }

const customizePanelRef = ref<any>(null);
function toggleCustomize(event: Event) { customizePanelRef.value?.toggle(event); }

// ── Chart type toggles (persisted) ────────────────────────────────────────────
type ChartKind = 'bar' | 'line';
const chartTypes = ref<{ conformance: ChartKind; quarterly: ChartKind; ncSection: ChartKind }>(
    (() => { try { return JSON.parse(localStorage.getItem('report-chart-types') ?? '') as any; } catch { return null; } })()
    ?? { conformance: 'bar', quarterly: 'line', ncSection: 'bar' }
);
watch(chartTypes, val => localStorage.setItem('report-chart-types', JSON.stringify(val)), { deep: true });

// ── Active filter chips for context bar ───────────────────────────────────────
const activeFilterChips = computed(() => {
    const chips: { label: string; key: string }[] = [];
    if (filterDivisionId.value) {
        const div = store.divisions.find(d => d.id === filterDivisionId.value);
        if (div) chips.push({ label: `${div.code} division`, key: 'division' });
    }
    if (filterStatus.value) chips.push({ label: filterStatus.value, key: 'status' });
    if (filterDateFrom.value || filterDateTo.value) {
        const fmt = (d: Date) => d.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
        const from = filterDateFrom.value ? fmt(filterDateFrom.value) : '';
        const to = filterDateTo.value ? fmt(filterDateTo.value) : '';
        const label = from && to ? `${from} – ${to}` : from ? `From ${from}` : `To ${to}`;
        chips.push({ label, key: 'dates' });
    }
    if (filterSection.value) chips.push({ label: filterSection.value, key: 'section' });
    return chips;
});

// ── Tabs ──────────────────────────────────────────────────────────────────────
const TABS = [
    { key: 'overview',      label: 'Overview' },
    { key: 'action-items',  label: 'Action Items' },
    { key: 'history',       label: 'Audit History' },
    { key: 'analysis',      label: 'Analysis' },
    { key: 'performance',   label: 'Performance' },
] as const;

const activeTab = ref<'overview' | 'action-items' | 'history' | 'analysis' | 'performance'>('overview');
const tabBarEl = ref<HTMLElement | null>(null);

function scrollToTabs() {
    nextTick(() => tabBarEl.value?.scrollIntoView({ behavior: 'smooth', block: 'start' }));
}

// ── Active filter description for chart subtitles ─────────────────────────────
const activeFilterDesc = computed(() => {
    const parts: string[] = [];
    if (filterDivisionId.value)
        parts.push(store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? '');
    if (filterSection.value) parts.push(`Section: ${filterSection.value}`);
    if (filterDateFrom.value || filterDateTo.value) {
        const fmt = (d: Date) => d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
        const from = filterDateFrom.value ? fmt(filterDateFrom.value) : '';
        const to   = filterDateTo.value   ? fmt(filterDateTo.value)   : '';
        parts.push(from && to ? `${from} – ${to}` : from ? `From ${from}` : `To ${to}`);
    }
    return parts.join(' · ');
});

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
            filterSection.value,
        );
    } finally {
        loading.value = false;
    }
}

function toggleSectionFilter(sectionName: string) {
    filterSection.value = filterSection.value === sectionName ? null : sectionName;
    const q = { ...route.query };
    if (filterSection.value) q.section = filterSection.value;
    else delete q.section;
    router.replace({ query: q });
    loadReport();
}

function clearSectionFilter() {
    filterSection.value = null;
    const q = { ...route.query };
    delete q.section;
    router.replace({ query: q });
    loadReport();
}

function complianceStatusLabel(div: ComplianceStatusDto): string {
    if (div.status === 'NeverAudited') return 'Never audited';
    if (div.daysUntilDue == null || div.daysSinceLastAudit == null) return '';
    if (div.status === 'Overdue') return `${Math.abs(div.daysUntilDue)}d overdue`;
    if (div.status === 'DueSoon') return `Due in ${div.daysUntilDue}d`;
    return `Due in ${div.daysUntilDue}d`;
}

async function loadComplianceStatus() {
    try {
        complianceStatus.value = await getClient().getComplianceStatus();
    } catch {
        // Non-blocking
    }
}

onMounted(async () => {
    await Promise.all([store.loadDivisions(), loadReport(), loadComplianceStatus()]);
});

// ── CSV Export ────────────────────────────────────────────────────────────────
function exportCsv() {
    if (!report.value?.rows.length) return;
    const headers = ['Audit #', 'Division', 'Date', 'Auditor', 'Job #', 'Location', 'Status', 'Score %', 'NCs', 'Warnings'];
    const rows = report.value.rows.map(r => [
        r.id, r.divisionCode, r.auditDate ?? '', r.auditor ?? '', r.jobNumber ?? '',
        r.location ?? '', r.status, r.scorePercent ?? '', r.nonConformingCount, r.warningCount,
    ]);
    const csv = [headers, ...rows].map(row => row.map(v => `"${String(v).replace(/"/g, '""')}"`).join(',')).join('\n');
    const divCode = store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'all';
    const from = filterDateFrom.value?.toISOString().split('T')[0] ?? '';
    const to = filterDateTo.value?.toISOString().split('T')[0] ?? '';
    const fileName = `audit-report-${divCode}${from ? `-${from}` : ''}${to ? `-${to}` : ''}.csv`;
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = fileName; a.click();
    URL.revokeObjectURL(url);
}

// ── Excel exports ─────────────────────────────────────────────────────────────
async function downloadExcel(endpoint: string, fileName: string, loadingRef: { value: boolean }) {
    loadingRef.value = true;
    try {
        const params: Record<string, string> = {};
        if (filterDivisionId.value) params.divisionId = String(filterDivisionId.value);
        if (filterDateFrom.value) params.dateFrom = filterDateFrom.value.toISOString();
        if (filterDateTo.value) params.dateTo = filterDateTo.value.toISOString();
        const res = await apiStore.api.get(endpoint, { params, responseType: 'blob' });
        const blobUrl = URL.createObjectURL(new Blob([res.data], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        }));
        const link = document.createElement('a');
        link.href = blobUrl; link.download = fileName; link.click();
        URL.revokeObjectURL(blobUrl);
    } finally {
        loadingRef.value = false;
    }
}

function exportQuarterlySummary() {
    const divCode = store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'all';
    downloadExcel('/v1/audits/export/quarterly-summary', `quarterly-summary-${divCode}.xlsx`, exportingQs);
}

function exportNcrReport() {
    const divCode = store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'all';
    downloadExcel('/v1/audits/export/ncr-report', `ncr-report-${divCode}.xlsx`, exportingNcr);
}

const exportMenuItems = computed(() => [
    { label: 'Audit CSV', icon: 'pi pi-file', command: exportCsv },
    { label: 'Quarterly Excel', icon: 'pi pi-file-excel', command: exportQuarterlySummary },
    { label: 'NCR Excel', icon: 'pi pi-file-excel', command: exportNcrReport },
]);

function openQSummary() {
    const divId = filterDivisionId.value ?? '';
    const now = new Date();
    const year = now.getFullYear();
    const quarter = Math.ceil((now.getMonth() + 1) / 3);
    window.open(`/audit-management/reports/quarterly-summary?divisionId=${divId}&year=${year}&quarter=${quarter}`, '_blank');
}

// ── KPI trend deltas ──────────────────────────────────────────────────────────
const trendDeltas = computed(() => {
    const rows = report.value?.rows ?? [];
    if (rows.length < 2) return { scoreDelta: null, ncDelta: null, warnDelta: null, auditCountDelta: null };
    const dated = rows.filter(r => r.auditDate).sort((a, b) => (a.auditDate ?? '').localeCompare(b.auditDate ?? ''));
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
    const scoreDelta = curScore != null && prvScore != null ? Math.round((curScore - prvScore) * 10) / 10 : null;
    const ncDelta = current.reduce((s, r) => s + r.nonConformingCount, 0) - prior.reduce((s, r) => s + r.nonConformingCount, 0);
    const warnDelta = current.reduce((s, r) => s + r.warningCount, 0) - prior.reduce((s, r) => s + r.warningCount, 0);
    const auditCountDelta = current.length - prior.length;
    return { scoreDelta, ncDelta, warnDelta, auditCountDelta };
});

// ── Corrected on site + CA aging ──────────────────────────────────────────────
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

const displayCorrectedPct = useCountUp(() => correctedOnSitePct.value);
const displayCaAging      = useCountUp(() => caAgingStats.value.overdueCount);

// ── Top locations by NC ───────────────────────────────────────────────────────
const locationStats = computed(() => {
    const map = new Map<string, number>();
    for (const row of report.value?.rows ?? []) {
        const loc = row.location?.trim();
        if (!loc) continue;
        map.set(loc, (map.get(loc) ?? 0) + row.nonConformingCount);
    }
    return Array.from(map.entries()).map(([location, ncs]) => ({ location, ncs })).filter(x => x.ncs > 0).sort((a, b) => b.ncs - a.ncs).slice(0, 10);
});

const locationChartData = computed(() => ({
    labels: locationStats.value.map(l => l.location),
    datasets: [{ label: 'Non-Conformances', data: locationStats.value.map(l => l.ncs), backgroundColor: 'rgba(239,68,68,0.6)', borderColor: '#ef4444', borderWidth: 1, borderRadius: 4 }],
}));

const locationChartOptions = computed(() => ({
    indexAxis: 'y' as const, responsive: true, maintainAspectRatio: false,
    plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw} NCs` } } },
    scales: {
        x: { beginAtZero: true, ticks: { color: '#94a3b8', stepSize: 1, precision: 0 }, grid: { color: 'rgba(100,116,139,0.2)' } },
        y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
    onClick: (_event: unknown, elements: { index: number }[]) => {
        if (elements.length > 0) { const loc = locationStats.value[elements[0].index]?.location; if (loc) drillByLocation(loc); }
    },
}));

// ── Score colors ──────────────────────────────────────────────────────────────
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
    const map: Record<string, string> = { Draft: 'warning', Submitted: 'info', Reopened: 'warning', Closed: 'success' };
    return map[status] ?? 'secondary';
}

// ── Conformance trend chart ───────────────────────────────────────────────────
const primaryLabel = computed(() =>
    filterDivisionId.value ? (store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'Selected') : 'All Divisions'
);

function formatWeek(isoWeek: string): string {
    const [yearStr, wStr] = isoWeek.split('-W');
    const year = +yearStr, w = +wStr;
    const jan4 = new Date(year, 0, 4);
    const mondayW1 = new Date(jan4);
    mondayW1.setDate(jan4.getDate() - ((jan4.getDay() + 6) % 7));
    const mondayOfWeek = new Date(mondayW1);
    mondayOfWeek.setDate(mondayW1.getDate() + (w - 1) * 7);
    const month = mondayOfWeek.toLocaleString('en-US', { month: 'short' });
    return `${month} W${w} '${String(year).slice(-2)}`;
}

const trendChartTitle = computed(() => {
    const parts: string[] = [];
    if (filterDivisionId.value) parts.push(store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? '');
    if (filterSection.value) parts.push(filterSection.value);
    const ctx = parts.length ? `${parts.join(' · ')} — ` : '';
    const period = (filterDateFrom.value || filterDateTo.value) ? 'Filtered Period' : 'Last 12 Weeks';
    return `${ctx}Conformance Trend (${period})`;
});

const chartData = computed(() => {
    const primary = report.value?.trend ?? [];
    const isLine = chartTypes.value.conformance === 'line';
    function scoreBarColors(t: typeof primary, alpha: boolean) {
        return t.map(p => p.avgScore == null ? `rgba(100,116,139,${alpha ? '0.5' : '1'})`
            : p.avgScore >= 90 ? `rgba(16,185,129,${alpha ? '0.7' : '1'})`
            : p.avgScore >= 75 ? `rgba(245,158,11,${alpha ? '0.7' : '1'})`
            : `rgba(239,68,68,${alpha ? '0.7' : '1'})`);
    }
    return {
        labels: primary.map(p => formatWeek(p.week)),
        datasets: [{
            label: primaryLabel.value,
            data: primary.map(p => p.avgScore),
            backgroundColor: isLine ? 'rgba(99,102,241,0.15)' : scoreBarColors(primary, true),
            borderColor: isLine ? '#6366f1' : scoreBarColors(primary, false),
            borderWidth: isLine ? 2 : 1, borderRadius: isLine ? 0 : 4,
            pointRadius: isLine ? 4 : 0, tension: isLine ? 0.3 : 0, fill: isLine,
        }],
    };
});

const chartOptions = computed(() => ({
    responsive: true, maintainAspectRatio: false,
    plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number | null }) => ctx.raw != null ? `${ctx.raw}%` : 'No data' } } },
    scales: {
        y: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
        x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
}));

// ── Division stats + chart ────────────────────────────────────────────────────
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
    return Array.from(map.entries()).map(([division, s]) => ({
        division, auditCount: s.count,
        avgScore: s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
        totalNcs: s.ncs,
    })).filter(r => r.avgScore != null).sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

const divisionHealthCards = computed(() => {
    const allDivisions = store.divisions;
    const openCaByDiv = new Map<string, number>();
    const lastAuditByDiv = new Map<string, string>();
    for (const row of report.value?.rows ?? []) {
        const current = lastAuditByDiv.get(row.divisionCode);
        if (!current || (row.auditDate && row.auditDate > current)) {
            if (row.auditDate) lastAuditByDiv.set(row.divisionCode, row.auditDate);
        }
    }
    const auditDivMap = new Map<number, string>();
    for (const row of report.value?.rows ?? []) auditDivMap.set(row.id, row.divisionCode);
    for (const ca of report.value?.openCorrectiveActions ?? []) {
        const divCode = auditDivMap.get(ca.auditId);
        if (divCode) openCaByDiv.set(divCode, (openCaByDiv.get(divCode) ?? 0) + 1);
    }
    const compStatusByCode = new Map<string, string>();
    for (const cs of complianceStatus.value) compStatusByCode.set(cs.divisionCode, cs.status);
    return divisionStats.value.map(ds => ({
        divisionCode: ds.division,
        divisionName: allDivisions.find(d => d.code === ds.division)?.name ?? '',
        auditCount: ds.auditCount,
        avgScore: ds.avgScore,
        totalNcs: ds.totalNcs,
        openCaCount: openCaByDiv.get(ds.division) ?? 0,
        lastAuditDate: lastAuditByDiv.get(ds.division) ?? null,
        complianceStatus: compStatusByCode.get(ds.division) ?? 'NoSchedule',
    }));
});

const recentAuditFeed = computed(() =>
    [...(report.value?.rows ?? [])].sort((a, b) => (b.auditDate ?? '').localeCompare(a.auditDate ?? '')).slice(0, 8)
);

const overdueAlertCas = computed(() => (report.value?.openCorrectiveActions ?? []).filter(ca => ca.isOverdue));

function filterByDivisionCode(code: string) {
    const div = store.divisions.find(d => d.code === code);
    if (div) { filterDivisionId.value = div.id; loadReport(); }
}

function auditStatusSeverity(status: string) {
    const map: Record<string, string> = { Draft: 'warning', Submitted: 'info', Reopened: 'warning', Closed: 'success' };
    return map[status] ?? 'secondary';
}

const divisionChartData = computed(() => ({
    labels: divisionStats.value.map(d => `${d.division} (${d.auditCount})`),
    datasets: [{
        label: 'Avg Score %',
        data: divisionStats.value.map(d => d.avgScore),
        backgroundColor: divisionStats.value.map(d => (d.avgScore ?? 0) >= 90 ? 'rgba(16,185,129,0.7)' : (d.avgScore ?? 0) >= 75 ? 'rgba(245,158,11,0.7)' : 'rgba(239,68,68,0.7)'),
        borderColor: divisionStats.value.map(d => (d.avgScore ?? 0) >= 90 ? '#10b981' : (d.avgScore ?? 0) >= 75 ? '#f59e0b' : '#ef4444'),
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

// ── Quarterly trend chart ─────────────────────────────────────────────────────
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
    return Array.from(byQuarter.entries()).sort(([a], [b]) => a.localeCompare(b)).map(([quarter, scores]) => ({
        quarter, avgScore: Math.round(scores.reduce((a, b) => a + b, 0) / scores.length * 10) / 10, count: scores.length,
    }));
});

const quarterlyChartData = computed(() => {
    const primary = quarterlyTrendData.value;
    const isLine = chartTypes.value.quarterly === 'line';
    return {
        labels: primary.map(p => p.quarter),
        datasets: [{
            label: primaryLabel.value,
            data: primary.map(p => p.avgScore),
            borderColor: '#6366f1',
            backgroundColor: isLine ? 'rgba(99,102,241,0.15)' : primary.map(p =>
                p.avgScore >= 90 ? 'rgba(16,185,129,0.7)' : p.avgScore >= 75 ? 'rgba(245,158,11,0.7)' : 'rgba(239,68,68,0.7)'),
            borderWidth: 2, pointRadius: isLine ? 5 : 0,
            pointBackgroundColor: primary.map(p => p.avgScore >= 90 ? '#10b981' : p.avgScore >= 75 ? '#f59e0b' : '#ef4444'),
            tension: 0.3, fill: isLine, borderRadius: isLine ? 0 : 4,
        }],
    };
});

const quarterlyChartOptions = computed(() => ({
    responsive: true, maintainAspectRatio: false,
    plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw}%` } } },
    scales: {
        y: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
        x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
}));

// ── Auditor performance ───────────────────────────────────────────────────────
const auditorStats = computed(() => {
    const map = new Map<string, { scores: number[]; ncs: number; warnings: number; count: number }>();
    for (const row of report.value?.rows ?? []) {
        const key = row.auditor?.trim() || 'Unknown';
        if (!map.has(key)) map.set(key, { scores: [], ncs: 0, warnings: 0, count: 0 });
        const entry = map.get(key)!;
        entry.count++;
        if (row.scorePercent != null) entry.scores.push(row.scorePercent);
        entry.ncs += row.nonConformingCount;
        entry.warnings += row.warningCount;
    }
    return Array.from(map.entries()).map(([auditor, s]) => ({
        auditor, auditCount: s.count,
        avgScore: s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
        totalNcs: s.ncs, totalWarnings: s.warnings,
    })).filter(r => r.auditCount > 0).sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

// ── NC by Section chart ───────────────────────────────────────────────────────
const ncCategoryChartData = computed(() => {
    const primary = report.value?.sectionBreakdown ?? [] as { sectionName: string; ncCount: number }[];
    const isLine = chartTypes.value.ncSection === 'line';
    return {
        labels: primary.map(x => x.sectionName),
        datasets: [{
            label: 'Non-Conformances', data: primary.map(x => x.ncCount),
            backgroundColor: 'rgba(239,68,68,0.7)', borderColor: '#ef4444',
            borderWidth: 1, borderRadius: isLine ? 0 : 4,
        }],
    };
});

// ── Per-section KPI cards ─────────────────────────────────────────────────────
const SECTION_SHORT: Record<string, string> = {
    'Personal Protective Equipment': 'PPE',
    'Equipment & Equipment Inspection': 'Equipment',
    'Job Site & Confined Space Condition': 'Job Site / CSE',
    'Sign-In / Sign-Out Rosters - Toolbox Safety': 'Sign-In / Toolbox',
    'Lock-Out / Tag-Out': 'LOTO',
    'Culture / Attitudes': 'Culture',
    'Training / Dispatch': 'Training',
    'Daily Job Logs': 'Daily Logs',
    'QA / QC Documentation': 'QA / QC',
};

const sectionKpiCards = computed(() => {
    if (!report.value || report.value.totalAudits === 0) return [];
    const total = report.value.totalAudits;
    return report.value.sectionBreakdown
        .map(s => ({
            name: s.sectionName,
            shortName: SECTION_SHORT[s.sectionName] ?? s.sectionName,
            ncCount: s.ncCount,
            rate: Math.round(s.ncCount / total * 100) / 100,
        }))
        .sort((a, b) => b.rate - a.rate);
});

function sectionRateColor(rate: number): string {
    if (rate === 0) return 'text-emerald-400';
    if (rate < 0.2) return 'text-yellow-400';
    if (rate < 0.5) return 'text-amber-400';
    return 'text-red-400';
}
function sectionRateBorder(rate: number, isActive = false): string {
    if (isActive) return '';
    if (rate === 0) return 'border-slate-700';
    if (rate < 0.2) return 'border-yellow-900/50';
    if (rate < 0.5) return 'border-amber-900/50';
    return 'border-red-900/50';
}

const ncCategoryChartOptions = computed(() => ({
    indexAxis: (chartTypes.value.ncSection === 'line' ? 'x' : 'y') as 'x' | 'y',
    responsive: true, maintainAspectRatio: false,
    plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw} NCs` } } },
    scales: {
        x: { beginAtZero: true, ticks: { color: '#94a3b8', stepSize: 1, precision: 0 }, grid: { color: 'rgba(100,116,139,0.2)' } },
        y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
}));
</script>

<style scoped>
/* ── Interactive KPI cards ───────────────────────────────────────────────────── */
.kpi-card {
    background: rgb(30, 41, 59);
    border: 1px solid rgb(51, 65, 85);
    border-radius: 0.5rem;
    padding: 1rem;
    text-align: center;
    cursor: pointer;
    transition: box-shadow 0.2s ease, transform 0.2s ease, border-color 0.2s ease;
}
.kpi-card:hover {
    box-shadow: 0 0 0 1px rgba(99, 179, 237, 0.4), 0 8px 28px rgba(0, 0, 0, 0.55);
    transform: translateY(-3px);
    border-color: rgba(99, 179, 237, 0.45);
}
.kpi-card--danger { border-color: rgba(127, 29, 29, 0.5); }
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

/* ── Recent activity hover ────────────────────────────────────────────────────── */
.bg-slate-750 {
    background-color: rgba(51, 65, 85, 0.6);
}
</style>

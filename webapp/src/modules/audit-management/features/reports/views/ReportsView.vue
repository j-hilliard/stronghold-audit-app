<template>
    <div>
        <BasePageHeader
            title="Dashboard"
            subtitle="Conformance trends and non-conformance analysis"
            icon="pi pi-chart-bar"
        >
            <Button
                v-if="report"
                label="Export CSV"
                icon="pi pi-download"
                severity="secondary"
                outlined
                size="small"
                @click="exportCsv"
            />
            <Button
                label="Report Composer"
                icon="pi pi-file-edit"
                severity="secondary"
                outlined
                size="small"
                @click="router.push('/audit-management/reports/composer')"
            />
            <Button
                label="Quarterly Summary"
                icon="pi pi-print"
                severity="secondary"
                outlined
                size="small"
                @click="openQSummary"
            />
            <Button icon="pi pi-refresh" severity="secondary" :loading="loading" @click="loadReport" />
        </BasePageHeader>

        <!-- Filter bar -->
        <div class="px-4 pt-2 pb-0 flex flex-wrap gap-3 items-center">
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
            <div class="border-l border-slate-600 pl-3 flex items-center gap-2">
                <span class="text-xs text-slate-400 whitespace-nowrap">Compare vs:</span>
                <Dropdown
                    v-model="compareAgainst"
                    :options="compareOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="None"
                    class="w-40"
                    data-testid="report-filter-compare"
                />
            </div>
        </div>

        <div v-if="loading" class="flex justify-center py-16">
            <ProgressSpinner />
        </div>

        <div v-else-if="report" class="p-4 space-y-4">

            <!-- Hidden widgets restore banner -->
            <div v-if="hiddenCount > 0" class="flex items-center gap-3 px-1">
                <i class="pi pi-eye-slash text-slate-500 text-xs" />
                <span class="text-xs text-slate-400">{{ hiddenCount }} widget{{ hiddenCount > 1 ? 's' : '' }} hidden</span>
                <button @click="showAllSections" class="text-xs text-blue-400 hover:text-blue-300 underline underline-offset-2 transition-colors">
                    Restore all
                </button>
            </div>

            <!-- KPI cards — row 1 -->
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
                <!-- Total Audits -->
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
                <!-- Avg Conformance -->
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
                <!-- Non-Conformances -->
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
                <!-- Warnings -->
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

            <!-- KPI row 2: Corrected on site + CA aging -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <!-- Corrected on site -->
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
                <!-- CA aging -->
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

            <!-- Per-section KPI cards -->
            <div v-if="sectionKpiCards.length > 0 && !hidden.sectionCards" class="space-y-2">
                <div class="flex items-center justify-between gap-3 px-1">
                    <div class="flex items-center gap-3 flex-wrap">
                        <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider">Findings Per Audit — by Section</div>
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
                    <button @click="hideSection('sectionCards')" class="section-collapse-btn" title="Hide section">
                        <i class="pi pi-eye-slash" />
                    </button>
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
                                : 'bg-slate-800 border-slate-700 hover:border-slate-500',
                            sectionRateBorder(card.rate, filterSection === card.name),
                        ]"
                    >
                        <div :class="['text-xl font-bold', sectionRateColor(card.rate)]">
                            {{ card.rate.toFixed(2) }}
                        </div>
                        <div class="text-xs text-slate-400 mt-0.5">NCs / audit</div>
                        <div class="text-xs text-slate-200 mt-1.5 font-medium leading-tight line-clamp-2 flex-1">
                            {{ card.shortName }}
                        </div>
                        <div class="text-xs text-slate-500 mt-0.5">{{ card.ncCount }} NCs</div>
                    </button>
                </div>
            </div>

            <!-- Division performance chart -->
            <Card v-if="divisionStats.length > 1 && !hidden.divisionChart">
                <template #title>
                    <div class="flex items-center justify-between w-full">
                        <span class="text-base font-semibold text-white">Conformance by Division</span>
                        <div class="flex items-center gap-1">
                            <button @click.stop="hideSection('divisionChart')" class="section-collapse-btn" title="Hide section"><i class="pi pi-eye-slash" /></button>
                            <button @click.stop="toggleSection('divisionChart')" class="section-collapse-btn">
                                <i :class="collapsed.divisionChart ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </div>
                </template>
                <template #content>
                    <div v-show="!collapsed.divisionChart">
                        <Chart
                            type="bar"
                            :data="divisionChartData"
                            :options="divisionChartOptions"
                            :style="`height: ${Math.max(160, divisionStats.length * 40)}px;`"
                        />
                    </div>
                </template>
            </Card>

            <!-- Trend chart -->
            <Card v-if="report.trend.length > 0 && !hidden.conformanceTrend">
                <template #title>
                    <div class="flex items-center justify-between w-full">
                        <span class="text-base font-semibold text-white">Conformance Trend (Last 12 Weeks)</span>
                        <div class="flex items-center gap-2">
                            <div class="flex rounded overflow-hidden border border-slate-600 text-xs">
                                <button
                                    :class="['px-2 py-0.5 transition-colors', chartTypes.conformance === 'bar' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']"
                                    @click="chartTypes.conformance = 'bar'">Bar</button>
                                <button
                                    :class="['px-2 py-0.5 transition-colors', chartTypes.conformance === 'line' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']"
                                    @click="chartTypes.conformance = 'line'">Line</button>
                            </div>
                            <button @click.stop="hideSection('conformanceTrend')" class="section-collapse-btn" title="Hide section"><i class="pi pi-eye-slash" /></button>
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

            <!-- NC by Section chart -->
            <Card v-if="(report.sectionBreakdown?.length ?? 0) > 0 && !hidden.ncSection">
                <template #title>
                    <div class="flex items-center justify-between w-full">
                        <span class="text-base font-semibold text-white">Non-Conformances by Section</span>
                        <div class="flex items-center gap-2">
                            <div class="flex rounded overflow-hidden border border-slate-600 text-xs">
                                <button
                                    :class="['px-2 py-0.5 transition-colors', chartTypes.ncSection === 'bar' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']"
                                    @click="chartTypes.ncSection = 'bar'">Bar</button>
                                <button
                                    :class="['px-2 py-0.5 transition-colors', chartTypes.ncSection === 'line' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']"
                                    @click="chartTypes.ncSection = 'line'">Line</button>
                            </div>
                            <button @click.stop="hideSection('ncSection')" class="section-collapse-btn" title="Hide section"><i class="pi pi-eye-slash" /></button>
                            <button @click.stop="toggleSection('ncSection')" class="section-collapse-btn">
                                <i :class="collapsed.ncSection ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </div>
                </template>
                <template #content>
                    <div v-show="!collapsed.ncSection">
                        <Chart
                            :type="chartTypes.ncSection"
                            :key="chartTypes.ncSection"
                            :data="ncCategoryChartData"
                            :options="ncCategoryChartOptions"
                            :style="`height: ${Math.max(160, (report.sectionBreakdown?.length ?? 0) * 36)}px;`"
                        />
                    </div>
                </template>
            </Card>

            <!-- Top locations by NC -->
            <Card v-if="locationStats.length >= 2 && !hidden.topLocations">
                <template #title>
                    <div class="flex items-center justify-between w-full">
                        <span class="text-base font-semibold text-white">Top Locations by Non-Conformances</span>
                        <div class="flex items-center gap-1">
                            <button @click.stop="hideSection('topLocations')" class="section-collapse-btn" title="Hide section"><i class="pi pi-eye-slash" /></button>
                            <button @click.stop="toggleSection('topLocations')" class="section-collapse-btn">
                                <i :class="collapsed.topLocations ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </div>
                </template>
                <template #content>
                    <div v-show="!collapsed.topLocations">
                        <Chart
                            type="bar"
                            :data="locationChartData"
                            :options="locationChartOptions"
                            :style="`height: ${Math.max(160, locationStats.length * 36)}px;`"
                        />
                    </div>
                </template>
            </Card>

            <!-- Open Corrective Actions -->
            <Card v-if="(report.openCorrectiveActions?.length ?? 0) > 0 && !hidden.openCAs">
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
                        <div class="flex items-center gap-1">
                            <button @click.stop="hideSection('openCAs')" class="section-collapse-btn" title="Hide section"><i class="pi pi-eye-slash" /></button>
                            <button @click.stop="toggleSection('openCAs')" class="section-collapse-btn">
                                <i :class="collapsed.openCAs ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </div>
                </template>
                <template #content>
                    <div v-show="!collapsed.openCAs">
                    <DataTable
                        :value="report.openCorrectiveActions"
                        :rows="10"
                        paginator
                        sortField="dueDate"
                        :sortOrder="1"
                        class="text-sm"
                    >
                        <Column field="id" header="CA #" style="width:60px" sortable />
                        <Column field="auditId" header="Audit #" style="width:80px" sortable />
                        <Column field="description" header="Description">
                            <template #body="{ data }">
                                <span class="line-clamp-2">{{ data.description }}</span>
                            </template>
                        </Column>
                        <Column field="assignedTo" header="Assigned To" sortable>
                            <template #body="{ data }">{{ data.assignedTo ?? '—' }}</template>
                        </Column>
                        <Column field="dueDate" header="Due Date" sortable>
                            <template #body="{ data }">
                                <span :class="data.isOverdue ? 'text-red-400 font-semibold' : ''">
                                    {{ data.dueDate ?? '—' }}
                                </span>
                            </template>
                        </Column>
                        <Column field="daysOpen" header="Age" sortable style="width:80px">
                            <template #body="{ data }">
                                <span :class="data.daysOpen > 14 ? 'text-red-400 font-semibold' : data.daysOpen > 7 ? 'text-amber-400' : 'text-emerald-400'">
                                    {{ data.daysOpen }}d
                                </span>
                            </template>
                        </Column>
                        <Column field="status" header="Status" sortable style="width:110px">
                            <template #body="{ data }">
                                <Tag
                                    :value="data.isOverdue ? 'Overdue' : data.status"
                                    :severity="data.isOverdue ? 'danger' : 'warning'"
                                />
                            </template>
                        </Column>
                        <Column header="" style="width:50px">
                            <template #body="{ data }">
                                <Button
                                    icon="pi pi-eye"
                                    size="small"
                                    severity="secondary"
                                    text
                                    @click="router.push(`/audit-management/audits/${data.auditId}/review`)"
                                />
                            </template>
                        </Column>
                    </DataTable>
                    </div>
                </template>
            </Card>

            <!-- Quarterly trend chart -->
            <Card v-if="quarterlyTrendData.length > 1 && !hidden.quarterlyTrend">
                <template #title>
                    <div class="flex items-center justify-between w-full">
                        <span class="text-base font-semibold text-white">Quarterly Conformance Trend</span>
                        <div class="flex items-center gap-2">
                            <div class="flex rounded overflow-hidden border border-slate-600 text-xs">
                                <button
                                    :class="['px-2 py-0.5 transition-colors', chartTypes.quarterly === 'bar' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']"
                                    @click="chartTypes.quarterly = 'bar'">Bar</button>
                                <button
                                    :class="['px-2 py-0.5 transition-colors', chartTypes.quarterly === 'line' ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-white']"
                                    @click="chartTypes.quarterly = 'line'">Line</button>
                            </div>
                            <button @click.stop="hideSection('quarterlyTrend')" class="section-collapse-btn" title="Hide section"><i class="pi pi-eye-slash" /></button>
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

            <!-- Auditor performance -->
            <Card v-if="auditorStats.length > 0 && !hidden.auditorPerf">
                <template #title>
                    <div class="flex items-center justify-between w-full">
                        <span class="text-base font-semibold text-white">Auditor Performance</span>
                        <div class="flex items-center gap-1">
                            <button @click.stop="hideSection('auditorPerf')" class="section-collapse-btn" title="Hide section"><i class="pi pi-eye-slash" /></button>
                            <button @click.stop="toggleSection('auditorPerf')" class="section-collapse-btn">
                                <i :class="collapsed.auditorPerf ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </div>
                </template>
                <template #content>
                    <div v-show="!collapsed.auditorPerf">
                    <DataTable :value="auditorStats" sortField="avgScore" :sortOrder="-1" class="text-sm">
                        <Column field="auditor" header="Auditor" sortable>
                            <template #body="{ data }">
                                <button
                                    class="auditor-link"
                                    @click="drillByAuditor(data.auditor)"
                                    :title="`Show ${data.auditor}'s audits`"
                                >{{ data.auditor }}</button>
                            </template>
                        </Column>
                        <Column field="auditCount" header="Audits" sortable style="width:80px" />
                        <Column field="avgScore" header="Avg Score" sortable style="width:110px">
                            <template #body="{ data }">
                                <span :class="rowScoreColor(data.avgScore)">
                                    {{ data.avgScore != null ? `${data.avgScore}%` : '—' }}
                                </span>
                            </template>
                        </Column>
                        <Column field="totalNcs" header="Total NCs" sortable style="width:100px">
                            <template #body="{ data }">
                                <span :class="data.totalNcs > 0 ? 'text-red-400 font-semibold' : 'text-slate-400'">
                                    {{ data.totalNcs }}
                                </span>
                            </template>
                        </Column>
                        <Column field="totalWarnings" header="Warnings" sortable style="width:100px">
                            <template #body="{ data }">
                                <span :class="data.totalWarnings > 0 ? 'text-amber-400' : 'text-slate-400'">
                                    {{ data.totalWarnings }}
                                </span>
                            </template>
                        </Column>
                    </DataTable>
                    </div>
                </template>
            </Card>

            <!-- Audit table -->
            <Card v-if="!hidden.auditDetail" ref="auditDetailCard">
                <template #title>
                    <div class="flex items-center justify-between w-full">
                        <div class="flex items-center gap-3 flex-wrap">
                            <span class="text-base font-semibold text-white">Audit Detail</span>
                            <!-- Active drill-down filter chips -->
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
                        <div class="flex items-center gap-1">
                            <button @click.stop="hideSection('auditDetail')" class="section-collapse-btn" title="Hide section"><i class="pi pi-eye-slash" /></button>
                            <button @click.stop="toggleSection('auditDetail')" class="section-collapse-btn">
                                <i :class="collapsed.auditDetail ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" />
                            </button>
                        </div>
                    </div>
                </template>
                <template #content>
                    <div v-show="!collapsed.auditDetail">
                    <DataTable
                        :value="filteredAuditRows"
                        :rows="20"
                        paginator
                        sortField="id"
                        :sortOrder="-1"
                        class="text-sm"
                    >
                        <Column field="id" header="#" style="width:60px" sortable>
                            <template #body="{ data }">
                                <span data-testid="report-grid-row">{{ data.id }}</span>
                            </template>
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
                            <template #body="{ data }">
                                <Tag :value="data.status" :severity="statusSeverity(data.status)" />
                            </template>
                        </Column>
                        <Column field="scorePercent" header="Score" sortable>
                            <template #body="{ data }">
                                <span :class="rowScoreColor(data.scorePercent)">
                                    {{ data.scorePercent != null ? `${data.scorePercent}%` : '—' }}
                                </span>
                            </template>
                        </Column>
                        <Column field="nonConformingCount" header="NCs" sortable style="width:60px">
                            <template #body="{ data }">
                                <span :class="data.nonConformingCount > 0 ? 'text-red-400 font-semibold' : 'text-slate-400'">
                                    {{ data.nonConformingCount }}
                                </span>
                            </template>
                        </Column>
                        <Column field="warningCount" header="Warn" sortable style="width:60px">
                            <template #body="{ data }">
                                <span :class="data.warningCount > 0 ? 'text-amber-400' : 'text-slate-400'">
                                    {{ data.warningCount }}
                                </span>
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
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type AuditReportDto } from '@/apiclient/auditClient';

const router = useRouter();
const route = useRoute();
const store = useAuditStore();
const apiStore = useApiStore();

const loading = ref(false);
const report = ref<AuditReportDto | null>(null);
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

// ── Section collapse state ─────────────────────────────────────────────────────
const collapsed = reactive<Record<string, boolean>>({});
function toggleSection(key: string) {
    collapsed[key] = !collapsed[key];
}

// ── Drill-down filters (auditor / location → Audit Detail) ────────────────────
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
    scrollToAuditDetail();
}

function drillByLocation(location: string) {
    drillLocation.value = location;
    drillAuditor.value = null;
    collapsed.auditDetail = false;
    scrollToAuditDetail();
}

const drillNcOnly = ref(false);
const drillWarnOnly = ref(false);

function drillByNcOnly() {
    drillNcOnly.value = true;
    drillWarnOnly.value = false;
    drillAuditor.value = null;
    drillLocation.value = null;
    collapsed.auditDetail = false;
    scrollToAuditDetail();
}

function drillByWarnOnly() {
    drillWarnOnly.value = true;
    drillNcOnly.value = false;
    drillAuditor.value = null;
    drillLocation.value = null;
    collapsed.auditDetail = false;
    scrollToAuditDetail();
}

function drillAllAudits() {
    drillNcOnly.value = false;
    drillWarnOnly.value = false;
    drillAuditor.value = null;
    drillLocation.value = null;
    collapsed.auditDetail = false;
    scrollToAuditDetail();
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

// ── Dashboard section visibility ─────────────────────────────────────────────
const hidden = reactive<Record<string, boolean>>(
    (() => { try { return JSON.parse(localStorage.getItem('dashboard-hidden') ?? '{}'); } catch { return {}; } })()
);
watch(hidden, val => localStorage.setItem('dashboard-hidden', JSON.stringify(val)), { deep: true });
function hideSection(key: string) { hidden[key] = true; }
function showAllSections() { for (const key of Object.keys(hidden)) delete hidden[key]; }
const hiddenCount = computed(() => Object.values(hidden).filter(Boolean).length);

// ── Chart type toggles (persisted to localStorage) ────────────────────────────

type ChartKind = 'bar' | 'line';
const chartTypes = ref<{ conformance: ChartKind; quarterly: ChartKind; ncSection: ChartKind }>(
    (() => { try { return JSON.parse(localStorage.getItem('report-chart-types') ?? '') as any; } catch { return null; } })()
    ?? { conformance: 'bar', quarterly: 'line', ncSection: 'bar' }
);
watch(chartTypes, val => localStorage.setItem('report-chart-types', JSON.stringify(val)), { deep: true });

// ── Compare Against ───────────────────────────────────────────────────────────

const compareAgainst = ref<null | 'company' | number>(null);
const compareReport = ref<AuditReportDto | null>(null);

const compareOptions = computed(() => [
    { label: 'None', value: null },
    { label: 'Company Average', value: 'company' as 'company' },
    ...store.divisions
        .filter(d => d.id !== filterDivisionId.value)
        .map(d => ({ label: d.code, value: d.id as number })),
]);

const primaryLabel = computed(() =>
    filterDivisionId.value
        ? (store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'Selected')
        : 'All Divisions'
);

const compareLabel = computed(() => {
    if (!compareAgainst.value) return '';
    if (compareAgainst.value === 'company') return 'Company Avg';
    return store.divisions.find(d => d.id === compareAgainst.value)?.code ?? 'Compare';
});

async function loadCompareReport() {
    if (!compareAgainst.value) { compareReport.value = null; return; }
    // Comparing "All Divisions" against "Company Average" would load identical data — skip
    if (compareAgainst.value === 'company' && !filterDivisionId.value) {
        compareReport.value = null;
        return;
    }
    const from = filterDateFrom.value ? filterDateFrom.value.toISOString().split('T')[0] : null;
    const to = filterDateTo.value ? filterDateTo.value.toISOString().split('T')[0] : null;
    const divId = compareAgainst.value === 'company' ? null : compareAgainst.value as number;
    compareReport.value = await getClient().getAuditReport(divId, filterStatus.value, from, to);
}

watch(compareAgainst, loadCompareReport);

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
        await loadCompareReport();
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

onMounted(async () => {
    await Promise.all([store.loadDivisions(), loadReport()]);
});

// ── Item 1: CSV Export ────────────────────────────────────────────────────────

function exportCsv() {
    if (!report.value?.rows.length) return;
    const headers = ['Audit #', 'Division', 'Date', 'Auditor', 'Job #', 'Location', 'Status', 'Score %', 'NCs', 'Warnings'];
    const rows = report.value.rows.map(r => [
        r.id,
        r.divisionCode,
        r.auditDate ?? '',
        r.auditor ?? '',
        r.jobNumber ?? '',
        r.location ?? '',
        r.status,
        r.scorePercent ?? '',
        r.nonConformingCount,
        r.warningCount,
    ]);
    const csv = [headers, ...rows]
        .map(row => row.map(v => `"${String(v).replace(/"/g, '""')}"`).join(','))
        .join('\n');
    const divCode = store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'all';
    const from = filterDateFrom.value?.toISOString().split('T')[0] ?? '';
    const to = filterDateTo.value?.toISOString().split('T')[0] ?? '';
    const fileName = `audit-report-${divCode}${from ? `-${from}` : ''}${to ? `-${to}` : ''}.csv`;
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
}

// ── Item 5: Open quarterly summary ────────────────────────────────────────────

function openQSummary() {
    const divId = filterDivisionId.value ?? '';
    const now = new Date();
    const year = now.getFullYear();
    const quarter = Math.ceil((now.getMonth() + 1) / 3);
    window.open(`/audit-management/reports/quarterly-summary?divisionId=${divId}&year=${year}&quarter=${quarter}`, '_blank');
}


// ── Item 2: KPI trend deltas ──────────────────────────────────────────────────

const trendDeltas = computed(() => {
    const rows = report.value?.rows ?? [];
    if (rows.length < 2) return { scoreDelta: null, ncDelta: null, warnDelta: null, auditCountDelta: null };

    // Split sorted rows into two halves by date
    const dated = rows
        .filter(r => r.auditDate)
        .sort((a, b) => (a.auditDate ?? '').localeCompare(b.auditDate ?? ''));
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
    const scoreDelta = curScore != null && prvScore != null
        ? Math.round((curScore - prvScore) * 10) / 10
        : null;

    const ncDelta = current.reduce((s, r) => s + r.nonConformingCount, 0)
        - prior.reduce((s, r) => s + r.nonConformingCount, 0);
    const warnDelta = current.reduce((s, r) => s + r.warningCount, 0)
        - prior.reduce((s, r) => s + r.warningCount, 0);
    const auditCountDelta = current.length - prior.length;

    return { scoreDelta, ncDelta, warnDelta, auditCountDelta };
});

// ── Item 3: Corrected on site + CA aging ─────────────────────────────────────

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

// ── Item 4: Top locations by NC ───────────────────────────────────────────────

const locationStats = computed(() => {
    const map = new Map<string, number>();
    for (const row of report.value?.rows ?? []) {
        const loc = row.location?.trim();
        if (!loc) continue;
        map.set(loc, (map.get(loc) ?? 0) + row.nonConformingCount);
    }
    return Array.from(map.entries())
        .map(([location, ncs]) => ({ location, ncs }))
        .filter(x => x.ncs > 0)
        .sort((a, b) => b.ncs - a.ncs)
        .slice(0, 10);
});

const locationChartData = computed(() => ({
    labels: locationStats.value.map(l => l.location),
    datasets: [{
        label: 'Non-Conformances',
        data: locationStats.value.map(l => l.ncs),
        backgroundColor: 'rgba(239,68,68,0.6)',
        borderColor: '#ef4444',
        borderWidth: 1,
        borderRadius: 4,
    }],
}));

const locationChartOptions = computed(() => ({
    indexAxis: 'y' as const,
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: { display: false },
        tooltip: { callbacks: { label: (ctx: { raw: number }) => `${ctx.raw} NCs` } },
    },
    scales: {
        x: {
            beginAtZero: true,
            ticks: { color: '#94a3b8', stepSize: 1, precision: 0 },
            grid: { color: 'rgba(100,116,139,0.2)' },
        },
        y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
    onClick: (_event: unknown, elements: { index: number }[]) => {
        if (elements.length > 0) {
            const loc = locationStats.value[elements[0].index]?.location;
            if (loc) drillByLocation(loc);
        }
    },
}));

// ── Existing chart/table computeds ────────────────────────────────────────────

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
    const map: Record<string, string> = {
        Draft: 'warning', Submitted: 'info', Reopened: 'warning', Closed: 'success',
    };
    return map[status] ?? 'secondary';
}

const chartData = computed(() => {
    const primary = report.value?.trend ?? [];
    const compare = compareReport.value?.trend ?? [];
    const isLine = chartTypes.value.conformance === 'line';

    function scoreBarColors(t: typeof primary, alpha: boolean) {
        return t.map(p => p.avgScore == null ? `rgba(100,116,139,${alpha ? '0.5' : '1'})`
            : p.avgScore >= 90 ? `rgba(16,185,129,${alpha ? '0.7' : '1'})`
            : p.avgScore >= 75 ? `rgba(245,158,11,${alpha ? '0.7' : '1'})`
            : `rgba(239,68,68,${alpha ? '0.7' : '1'})`);
    }

    if (!compareReport.value) {
        return {
            labels: primary.map(p => p.week),
            datasets: [{
                label: primaryLabel.value,
                data: primary.map(p => p.avgScore),
                backgroundColor: isLine ? 'rgba(99,102,241,0.15)' : scoreBarColors(primary, true),
                borderColor: isLine ? '#6366f1' : scoreBarColors(primary, false),
                borderWidth: isLine ? 2 : 1,
                borderRadius: isLine ? 0 : 4,
                pointRadius: isLine ? 4 : 0,
                tension: isLine ? 0.3 : 0,
                fill: isLine,
            }],
        };
    }

    const allWeeks = [...new Set([...primary.map(p => p.week), ...compare.map(p => p.week)])].sort();
    return {
        labels: allWeeks,
        datasets: [
            {
                label: primaryLabel.value,
                data: allWeeks.map(w => primary.find(p => p.week === w)?.avgScore ?? null),
                borderColor: '#6366f1', backgroundColor: 'rgba(99,102,241,0.15)',
                borderWidth: 2.5, pointRadius: 5, tension: 0.3, fill: false, spanGaps: true,
            },
            {
                label: compareLabel.value,
                data: allWeeks.map(w => compare.find(p => p.week === w)?.avgScore ?? null),
                borderColor: '#f59e0b', backgroundColor: 'rgba(245,158,11,0.1)',
                borderWidth: 2, pointRadius: 4, tension: 0.3, fill: false, spanGaps: true,
                borderDash: [6, 3],
            },
        ],
    };
});

const chartOptions = computed(() => ({
    responsive: true, maintainAspectRatio: false,
    plugins: {
        legend: { display: !!compareReport.value, labels: { color: '#94a3b8' } },
        tooltip: { callbacks: { label: (ctx: { raw: number | null }) => ctx.raw != null ? `${ctx.raw}%` : 'No data' } },
    },
    scales: {
        y: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
        x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
}));

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
    return Array.from(map.entries())
        .map(([division, s]) => ({
            division,
            auditCount: s.count,
            avgScore: s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
            totalNcs: s.ncs,
        }))
        .filter(r => r.avgScore != null)
        .sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

const divisionChartData = computed(() => ({
    labels: divisionStats.value.map(d => `${d.division} (${d.auditCount})`),
    datasets: [{
        label: 'Avg Score %',
        data: divisionStats.value.map(d => d.avgScore),
        backgroundColor: divisionStats.value.map(d =>
            (d.avgScore ?? 0) >= 90 ? 'rgba(16,185,129,0.7)' : (d.avgScore ?? 0) >= 75 ? 'rgba(245,158,11,0.7)' : 'rgba(239,68,68,0.7)',
        ),
        borderColor: divisionStats.value.map(d =>
            (d.avgScore ?? 0) >= 90 ? '#10b981' : (d.avgScore ?? 0) >= 75 ? '#f59e0b' : '#ef4444',
        ),
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
    return Array.from(byQuarter.entries())
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([quarter, scores]) => ({
            quarter,
            avgScore: Math.round(scores.reduce((a, b) => a + b, 0) / scores.length * 10) / 10,
            count: scores.length,
        }));
});

const compareQuarterlyTrendData = computed(() => {
    if (!compareReport.value) return [] as { quarter: string; avgScore: number; count: number }[];
    const byQuarter = new Map<string, number[]>();
    for (const row of compareReport.value.rows) {
        if (!row.auditDate || row.scorePercent == null) continue;
        const d = new Date(row.auditDate);
        const q = Math.ceil((d.getMonth() + 1) / 3);
        const key = `${d.getFullYear()} Q${q}`;
        if (!byQuarter.has(key)) byQuarter.set(key, []);
        byQuarter.get(key)!.push(row.scorePercent);
    }
    return Array.from(byQuarter.entries())
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([quarter, scores]) => ({
            quarter,
            avgScore: Math.round(scores.reduce((a, b) => a + b, 0) / scores.length * 10) / 10,
            count: scores.length,
        }));
});

const quarterlyChartData = computed(() => {
    const primary = quarterlyTrendData.value;
    const compare = compareQuarterlyTrendData.value;
    const isLine = chartTypes.value.quarterly === 'line';

    if (!compareReport.value) {
        return {
            labels: primary.map(p => p.quarter),
            datasets: [{
                label: primaryLabel.value,
                data: primary.map(p => p.avgScore),
                borderColor: '#6366f1', backgroundColor: isLine ? 'rgba(99,102,241,0.15)' : primary.map(p =>
                    p.avgScore >= 90 ? 'rgba(16,185,129,0.7)' : p.avgScore >= 75 ? 'rgba(245,158,11,0.7)' : 'rgba(239,68,68,0.7)'),
                borderWidth: 2,
                pointRadius: isLine ? 5 : 0,
                pointBackgroundColor: primary.map(p => p.avgScore >= 90 ? '#10b981' : p.avgScore >= 75 ? '#f59e0b' : '#ef4444'),
                tension: 0.3, fill: isLine, borderRadius: isLine ? 0 : 4,
            }],
        };
    }

    const allQuarters = [...new Set([...primary.map(p => p.quarter), ...compare.map(p => p.quarter)])].sort();
    return {
        labels: allQuarters,
        datasets: [
            {
                label: primaryLabel.value,
                data: allQuarters.map(q => primary.find(p => p.quarter === q)?.avgScore ?? null),
                borderColor: '#6366f1', backgroundColor: 'rgba(99,102,241,0.15)', spanGaps: true,
                borderWidth: 2, pointRadius: 5, tension: 0.3, fill: true,
            },
            {
                label: compareLabel.value,
                data: allQuarters.map(q => compare.find(p => p.quarter === q)?.avgScore ?? null),
                borderColor: '#f59e0b', backgroundColor: 'rgba(245,158,11,0.1)',
                borderWidth: 2, pointRadius: 5, tension: 0.3, fill: true, spanGaps: true,
                borderDash: [6, 3],
            },
        ],
    };
});

const quarterlyChartOptions = computed(() => ({
    responsive: true, maintainAspectRatio: false,
    plugins: {
        legend: { display: !!compareReport.value, labels: { color: '#94a3b8' } },
        tooltip: { callbacks: { label: (ctx: { raw: number; dataIndex: number }) => `${ctx.raw}%` } },
    },
    scales: {
        y: { min: 0, max: 100, ticks: { color: '#94a3b8', callback: (v: number) => `${v}%` }, grid: { color: 'rgba(100,116,139,0.2)' } },
        x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(100,116,139,0.1)' } },
    },
}));

const auditorStats = computed(() => {
    const map = new Map<string, { scores: number[]; ncs: number; warnings: number }>();
    for (const row of report.value?.rows ?? []) {
        const key = row.auditor?.trim() || 'Unknown';
        if (!map.has(key)) map.set(key, { scores: [], ncs: 0, warnings: 0 });
        const entry = map.get(key)!;
        if (row.scorePercent != null) entry.scores.push(row.scorePercent);
        entry.ncs += row.nonConformingCount;
        entry.warnings += row.warningCount;
    }
    return Array.from(map.entries())
        .map(([auditor, s]) => ({
            auditor,
            auditCount: s.scores.length,
            avgScore: s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
            totalNcs: s.ncs,
            totalWarnings: s.warnings,
        }))
        .filter(r => r.auditCount > 0)
        .sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
});

const ncCategoryChartData = computed(() => {
    const primary = report.value?.sectionBreakdown ?? [] as { sectionName: string; ncCount: number }[];
    const compare = compareReport.value?.sectionBreakdown;
    const isLine = chartTypes.value.ncSection === 'line';

    if (!compare) {
        return {
            labels: primary.map(x => x.sectionName),
            datasets: [{
                label: 'Non-Conformances',
                data: primary.map(x => x.ncCount),
                backgroundColor: 'rgba(239,68,68,0.7)', borderColor: '#ef4444',
                borderWidth: 1, borderRadius: isLine ? 0 : 4,
            }],
        };
    }

    // Comparison: normalize to findings-per-audit rate
    const pTotal = report.value?.totalAudits || 1;
    const cTotal = compareReport.value?.totalAudits || 1;
    const allSections = [...new Set([...primary.map(x => x.sectionName), ...compare.map(x => x.sectionName)])];
    return {
        labels: allSections,
        datasets: [
            {
                label: primaryLabel.value,
                data: allSections.map(s => {
                    const item = primary.find(x => x.sectionName === s);
                    return item ? Math.round(item.ncCount / pTotal * 100) / 100 : 0;
                }),
                backgroundColor: 'rgba(99,102,241,0.7)', borderColor: '#6366f1',
                borderWidth: 1, borderRadius: isLine ? 0 : 4,
            },
            {
                label: compareLabel.value,
                data: allSections.map(s => {
                    const item = compare.find(x => x.sectionName === s);
                    return item ? Math.round(item.ncCount / cTotal * 100) / 100 : 0;
                }),
                backgroundColor: 'rgba(245,158,11,0.7)', borderColor: '#f59e0b',
                borderWidth: 1, borderRadius: isLine ? 0 : 4,
            },
        ],
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
    if (isActive) return ''; // active state handled by the parent class binding
    if (rate === 0) return 'border-slate-700';
    if (rate < 0.2) return 'border-yellow-900/50';
    if (rate < 0.5) return 'border-amber-900/50';
    return 'border-red-900/50';
}

const ncCategoryChartOptions = computed(() => ({
    indexAxis: (chartTypes.value.ncSection === 'line' ? 'x' : 'y') as 'x' | 'y',
    responsive: true, maintainAspectRatio: false,
    plugins: {
        legend: { display: !!compareReport.value, labels: { color: '#94a3b8' } },
        tooltip: {
            callbacks: {
                label: (ctx: { raw: number }) =>
                    compareReport.value ? `${ctx.raw.toFixed(2)} NCs/audit` : `${ctx.raw} NCs`,
            },
        },
    },
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
</style>

<template>
    <div class="flex flex-col h-full composer-view">
        <BasePageHeader
            title="Report Composer"
            subtitle="Structured compliance reports — template-driven, admin-controlled"
            icon="pi pi-file-edit"
        >
            <button
                @click="saveDraft"
                :disabled="saving"
                class="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-slate-600 hover:border-slate-500 text-slate-300 hover:text-white bg-transparent hover:bg-slate-700/50 rounded-lg disabled:opacity-50 transition-colors"
            >
                <i class="pi pi-save text-xs" />
                {{ saving ? 'Saving…' : 'Save Draft' }}
            </button>
            <button
                @click="printReport"
                :disabled="!builder.hasData.value || pdfGenerating"
                class="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-blue-600 hover:bg-blue-500 text-white font-medium rounded-lg disabled:opacity-50 transition-colors"
            >
                <i :class="pdfGenerating ? 'pi pi-spin pi-spinner' : 'pi pi-file-pdf'" class="text-xs" />
                {{ pdfGenerating ? 'Generating…' : 'Export PDF' }}
            </button>
        </BasePageHeader>

        <!-- Context bar -->
        <div class="flex items-center gap-2 px-4 py-2.5 bg-surface-2 border-b border-slate-700/60 flex-wrap shadow-elevation-1">
            <div class="flex flex-col gap-0.5">
                <span class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider leading-none">Division</span>
                <select
                    v-model="selectedDivisionId"
                    data-testid="composer-filter-division"
                    class="composer-select"
                >
                    <option :value="null" disabled>Select division…</option>
                    <option v-for="d in divisions" :key="d.id" :value="d.id">{{ d.code }} — {{ d.name }}</option>
                </select>
            </div>

            <div class="flex flex-col gap-0.5">
                <span class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider leading-none">From</span>
                <input type="date" v-model="dateFrom" data-testid="composer-filter-from" class="composer-input w-36" />
            </div>
            <span class="text-slate-600 text-xs font-medium self-end mb-1">→</span>
            <div class="flex flex-col gap-0.5">
                <span class="text-[10px] font-semibold text-slate-500 uppercase tracking-wider leading-none">To</span>
                <input type="date" v-model="dateTo" data-testid="composer-filter-to" class="composer-input w-36" />
            </div>

            <button
                @click="generate"
                :disabled="!selectedDivisionId || builder.loading.value"
                data-testid="composer-generate"
                class="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-blue-700 hover:bg-blue-600 text-white rounded-lg disabled:opacity-50 transition-colors"
            >
                <i :class="builder.loading.value ? 'pi pi-spin pi-spinner' : 'pi pi-refresh'" class="text-xs" />
                {{ builder.loading.value ? 'Loading…' : 'Generate' }}
            </button>

            <div class="h-5 w-px bg-slate-700 mx-1 hidden sm:block" />

            <input
                v-model="draftTitle"
                type="text"
                placeholder="Draft title…"
                data-testid="composer-draft-title"
                class="composer-input w-44"
            />

            <select
                v-model="selectedDraftId"
                @change="onLoadDraft"
                data-testid="composer-draft-select"
                class="composer-select"
            >
                <option :value="null">New draft</option>
                <option v-for="d in draftList" :key="d.id" :value="d.id">
                    {{ d.divisionCode }} — {{ d.title }} ({{ d.period }})
                </option>
            </select>

            <button
                v-if="draftId !== null"
                @click="deleteDraft"
                class="px-2 py-1.5 text-xs text-red-500 hover:text-red-400 hover:bg-red-900/20 rounded transition-colors"
                title="Delete current draft"
            >
                <i class="pi pi-trash" />
            </button>
        </div>

        <!-- Error banners -->
        <div v-if="saveError" class="px-4 py-2 text-sm text-red-400 bg-red-900/20 border-b border-red-800">
            {{ saveError }}
        </div>
        <div v-if="builder.error.value" class="px-4 py-2 text-sm text-amber-400 bg-amber-900/20 border-b border-amber-800">
            {{ builder.error.value }}
        </div>
        <div v-if="pdfError" class="px-4 py-2 text-sm text-red-400 bg-red-900/20 border-b border-red-800">
            {{ pdfError }}
        </div>

        <!-- Main layout -->
        <div class="flex flex-1 min-h-0">

            <!-- Left sidebar: template selector + section toggles -->
            <div class="w-64 shrink-0 border-r border-slate-700/60 overflow-y-auto bg-slate-900/50 flex flex-col gap-5 p-4">

                <!-- Template selector -->
                <div>
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wide mb-2">Template</div>
                    <div class="flex flex-col gap-1.5">
                        <label
                            v-for="tmpl in REPORT_TEMPLATES"
                            :key="tmpl.type"
                            class="flex items-start gap-2.5 p-2.5 rounded-lg border cursor-pointer transition-colors"
                            :class="report.templateType === tmpl.type
                                ? 'border-blue-500/60 bg-blue-900/20'
                                : 'border-slate-700/60 hover:border-slate-600 bg-slate-800/30'"
                        >
                            <input
                                type="radio"
                                :value="tmpl.type"
                                v-model="report.templateType"
                                @change="selectTemplate(tmpl.type as ReportType)"
                                class="mt-0.5 accent-blue-500 cursor-pointer"
                            />
                            <div>
                                <div class="text-xs font-medium text-slate-200 flex items-center gap-1.5">
                                    <i :class="tmpl.icon + ' text-[11px] text-blue-400'" />
                                    {{ tmpl.label }}
                                </div>
                                <div class="text-[11px] text-slate-500 mt-0.5 leading-tight">{{ tmpl.description }}</div>
                            </div>
                        </label>
                    </div>
                </div>

                <!-- Section toggles -->
                <div>
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wide mb-2">Sections</div>
                    <div class="flex flex-col gap-1">
                        <label
                            v-for="sec in report.sections"
                            :key="sec.type"
                            class="flex items-center gap-2 px-2.5 py-2 rounded-lg text-xs transition-colors"
                            :class="sec.enabled ? 'bg-slate-800/50' : 'opacity-50'"
                        >
                            <input
                                type="checkbox"
                                :checked="sec.enabled"
                                :disabled="isRequired(sec.type)"
                                @change="toggleSection(sec.type)"
                                class="accent-blue-500 cursor-pointer disabled:cursor-not-allowed shrink-0"
                            />
                            <i :class="SECTION_ICONS[sec.type] + ' text-[10px] text-slate-400 shrink-0'" />
                            <span class="text-slate-300 truncate">{{ SECTION_LABELS[sec.type] }}</span>
                            <span v-if="isRequired(sec.type)" class="ml-auto text-[10px] text-slate-600 shrink-0">req</span>
                            <span v-else-if="EDITABLE_SECTIONS.includes(sec.type)" class="ml-auto text-[10px] text-amber-500/70 shrink-0">edit</span>
                        </label>
                    </div>
                </div>

                <!-- Save status -->
                <div v-if="lastSavedAt" class="text-[11px] text-slate-600 mt-auto pt-2 border-t border-slate-700/50">
                    <i class="pi pi-check text-emerald-600 mr-1" />
                    Saved {{ lastSavedAt.toLocaleTimeString() }}
                </div>
            </div>

            <!-- Preview / Block canvas area -->
            <div class="flex-1 flex flex-col min-h-0 bg-slate-950" id="report-preview">

                <!-- Mode tab bar -->
                <div class="flex items-center gap-1 px-4 pt-3 pb-0 border-b border-slate-700/60 flex-shrink-0 bg-surface-2">
                    <button
                        class="composer-tab"
                        :class="composerMode === 'structured' ? 'composer-tab--active' : ''"
                        @click="composerMode = 'structured'"
                    >
                        <i class="pi pi-list text-[10px]" /> Structured
                    </button>
                    <button
                        class="composer-tab"
                        :class="composerMode === 'blocks' ? 'composer-tab--active' : ''"
                        @click="composerMode = 'blocks'"
                    >
                        <i class="pi pi-th-large text-[10px]" /> Block Builder
                    </button>
                </div>

                <!-- ── STRUCTURED mode ──────────────────────────────────────── -->
                <div
                    v-show="composerMode === 'structured'"
                    class="flex-1 overflow-y-auto p-6"
                >
                    <div
                        v-if="!builder.hasData.value"
                        class="flex flex-col items-center justify-center h-full gap-6 text-center px-6 py-8"
                    >
                        <div class="w-16 h-16 rounded-2xl bg-slate-800 border border-slate-700 flex items-center justify-center shadow-elevation-1">
                            <i class="pi pi-file-edit text-3xl text-slate-500" />
                        </div>
                        <div class="max-w-xs">
                            <p class="text-base font-semibold text-slate-300 mb-1.5">Ready to build</p>
                            <p class="text-sm text-slate-500 leading-relaxed">Select a division and date range above, then generate to load audit data into your report.</p>
                        </div>
                        <button
                            @click="generate"
                            :disabled="!selectedDivisionId || builder.loading.value"
                            class="flex items-center gap-2 px-6 py-2.5 text-sm font-semibold bg-blue-600 hover:bg-blue-500 disabled:opacity-40 text-white rounded-lg transition-colors shadow-lg"
                        >
                            <i :class="builder.loading.value ? 'pi pi-spin pi-spinner' : 'pi pi-play'" />
                            {{ builder.loading.value ? 'Generating…' : 'Generate Report' }}
                        </button>

                        <!-- Saved drafts — quick resume -->
                        <div v-if="draftList.length > 0" class="w-full max-w-sm mt-2">
                            <p class="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">Saved Drafts</p>
                            <div class="flex flex-col gap-1.5">
                                <button
                                    v-for="d in draftList"
                                    :key="d.id"
                                    class="flex items-center gap-3 px-3 py-2.5 rounded-lg border border-slate-700/60 bg-slate-800/50 hover:bg-slate-700/60 hover:border-slate-600 text-left transition-colors w-full"
                                    @click="selectedDraftId = d.id; onLoadDraft()"
                                >
                                    <i class="pi pi-file text-slate-500 text-sm shrink-0" />
                                    <div class="flex-1 min-w-0">
                                        <p class="text-sm text-slate-200 font-medium truncate">{{ d.title }}</p>
                                        <p class="text-xs text-slate-500 mt-0.5">{{ d.divisionCode }} · {{ d.period }}</p>
                                    </div>
                                    <i class="pi pi-arrow-right text-[10px] text-slate-600 shrink-0" />
                                </button>
                            </div>
                        </div>
                    </div>

                    <div v-else class="max-w-4xl mx-auto flex flex-col gap-5">
                        <template v-for="sec in enabledSections" :key="sec.type">
                            <div
                                class="report-section-card relative"
                                :class="editingSection === sec.type ? 'ring-2 ring-amber-400/60' : ''"
                            >
                                <button
                                    v-if="EDITABLE_SECTIONS.includes(sec.type)"
                                    @click="toggleEdit(sec.type)"
                                    class="absolute top-3 right-3 z-10 flex items-center gap-1 px-2 py-1 text-[11px] rounded transition-colors border"
                                    :class="editingSection === sec.type
                                        ? 'bg-amber-500/20 text-amber-400 border-amber-500/40'
                                        : 'bg-slate-700/60 text-slate-400 hover:bg-slate-700 border-slate-600'"
                                >
                                    <i :class="editingSection === sec.type ? 'pi pi-check' : 'pi pi-pencil'" class="text-[10px]" />
                                    {{ editingSection === sec.type ? 'Done' : 'Edit' }}
                                </button>
                                <component
                                    v-if="EDITABLE_SECTIONS.includes(sec.type)"
                                    :is="sectionComponent(sec.type)"
                                    v-bind="sectionProps(sec)"
                                    :is-editable="editingSection === sec.type"
                                    @update="updateSection"
                                />
                                <component
                                    v-else
                                    :is="sectionComponent(sec.type)"
                                    v-bind="sectionProps(sec)"
                                />
                            </div>
                        </template>
                    </div>
                </div>

                <!-- ── BLOCK BUILDER mode ───────────────────────────────────── -->
                <FlowCanvas
                    v-show="composerMode === 'blocks'"
                    :blocks="blockComposer.blocks.value"
                    :edit-mode="blockComposer.editMode.value"
                    class="flex-1 min-h-0"
                    @reorder="blockComposer.reorderBlocks"
                    @add="(t) => blockComposer.addBlock(t)"
                    @remove="blockComposer.removeBlock"
                    @duplicate="blockComposer.duplicateBlock"
                    @update-content="blockComposer.updateBlockConfig"
                    @update-is-edited="blockComposer.markEdited"
                    @toggle-edit-mode="blockComposer.toggleEditMode"
                />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import type { DivisionDto, ReportDraftListItemDto } from '@/apiclient/auditClient';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useReportBuilder } from '../composables/useReportBuilder';
import { useBlockComposer } from '../composables/useBlockComposer';
import FlowCanvas from '../components/FlowCanvas.vue';
import {
    REPORT_TEMPLATES,
    SECTION_LABELS,
    SECTION_ICONS,
    EDITABLE_SECTIONS,
    buildDefaultReport,
    buildDefaultSections,
    getTemplateDefinition,
    isStructuredReport,
    type ReportType,
    type SectionType,
    type StructuredSection,
    type StructuredReport,
} from '../types/report-template';
import ReportSectionCover from '../components/sections/ReportSectionCover.vue';
import ReportSectionKpis from '../components/sections/ReportSectionKpis.vue';
import ReportSectionTrend from '../components/sections/ReportSectionTrend.vue';
import ReportSectionCategoryBreakdown from '../components/sections/ReportSectionCategoryBreakdown.vue';
import ReportSectionFindings from '../components/sections/ReportSectionFindings.vue';
import ReportSectionCaTable from '../components/sections/ReportSectionCaTable.vue';
import ReportSectionSummaryText from '../components/sections/ReportSectionSummaryText.vue';
import ReportSectionHighlights from '../components/sections/ReportSectionHighlights.vue';

const service       = useAuditService();
const builder       = useReportBuilder();
const blockComposer = useBlockComposer();

const composerMode = ref<'structured' | 'blocks'>('structured');

// ── Filter state ──────────────────────────────────────────────────────────────

const divisions = ref<DivisionDto[]>([]);
const selectedDivisionId = ref<number | null>(null);
const dateFrom = ref('');
const dateTo = ref('');

// ── Report state ──────────────────────────────────────────────────────────────

const report = ref<StructuredReport>(
    buildDefaultReport(getTemplateDefinition('QuarterlySummary'), 0, '', '', null, null),
);

const editingSection = ref<SectionType | null>(null);

const enabledSections = computed(() =>
    report.value.sections.filter(s => s.enabled),
);

// ── Draft persistence ─────────────────────────────────────────────────────────

const draftList = ref<ReportDraftListItemDto[]>([]);
const selectedDraftId = ref<number | null>(null);
const draftId = ref<number | null>(null);
const draftTitle = ref('');
const draftRowVersion = ref<string | null>(null);
const saving = ref(false);
const saveError = ref<string | null>(null);
const lastSavedAt = ref<Date | null>(null);

async function saveDraft() {
    if (!selectedDivisionId.value) { saveError.value = 'Select a division first.'; return; }
    saving.value = true;
    saveError.value = null;
    const div = divisions.value.find(d => d.id === selectedDivisionId.value);
    report.value.divisionId   = selectedDivisionId.value;
    report.value.divisionCode = div?.code ?? '';
    report.value.period       = buildPeriodLabel(dateFrom.value, dateTo.value);
    report.value.dateFrom     = dateFrom.value || null;
    report.value.dateTo       = dateTo.value || null;
    const blocksJson = JSON.stringify(report.value);
    try {
        if (draftId.value === null) {
            const newId = await service.createReportDraft({
                divisionId: selectedDivisionId.value,
                title: draftTitle.value || 'Untitled Draft',
                period: report.value.period,
                dateFrom: report.value.dateFrom,
                dateTo: report.value.dateTo,
                blocksJson,
            });
            draftId.value = newId;
            selectedDraftId.value = newId;
            const fresh = await service.getReportDraft(newId);
            draftRowVersion.value = fresh.rowVersion;
        } else {
            if (!draftRowVersion.value) throw new Error('rowVersion missing — cannot update draft.');
            await service.updateReportDraft(draftId.value, {
                title: draftTitle.value || 'Untitled Draft',
                period: report.value.period,
                dateFrom: report.value.dateFrom,
                dateTo: report.value.dateTo,
                blocksJson,
                rowVersion: draftRowVersion.value,
            });
            const fresh = await service.getReportDraft(draftId.value);
            draftRowVersion.value = fresh.rowVersion;
        }
        lastSavedAt.value = new Date();
        await loadDraftList();
    } catch (e: unknown) {
        saveError.value = e instanceof Error ? e.message : 'Save failed.';
    } finally {
        saving.value = false;
    }
}

async function deleteDraft() {
    if (draftId.value === null) return;
    if (!confirm('Delete this draft? This cannot be undone.')) return;
    await service.deleteReportDraft(draftId.value);
    draftId.value = null;
    draftRowVersion.value = null;
    selectedDraftId.value = null;
    await loadDraftList();
}

async function loadDraftList() {
    try {
        draftList.value = await service.getReportDrafts(selectedDivisionId.value ?? undefined);
    } catch {
        draftList.value = [];
    }
}

async function onLoadDraft() {
    if (!selectedDraftId.value) return;
    const dto = await service.getReportDraft(selectedDraftId.value);
    if (!isStructuredReport(dto.blocksJson)) {
        alert('This draft was created with the old report builder and cannot be opened here. Starting a new draft instead.');
        selectedDraftId.value = null;
        return;
    }
    const loaded: StructuredReport = JSON.parse(dto.blocksJson);
    report.value      = loaded;
    draftId.value     = dto.id;
    draftTitle.value  = dto.title;
    draftRowVersion.value = dto.rowVersion;
    selectedDivisionId.value = dto.divisionId;
    dateFrom.value = dto.dateFrom ?? '';
    dateTo.value   = dto.dateTo   ?? '';
    editingSection.value = null;
}

// ── Template / section helpers ────────────────────────────────────────────────

function selectTemplate(type: ReportType) {
    const tmpl = getTemplateDefinition(type);
    const newSections = buildDefaultSections(tmpl);
    report.value.sections = newSections.map(s => {
        const existing = report.value.sections.find(e => e.type === s.type);
        return existing ? { ...existing, enabled: s.enabled } : s;
    });
    report.value.templateType = type;
    editingSection.value = null;
}

function isRequired(type: SectionType): boolean {
    return getTemplateDefinition(report.value.templateType).requiredSections.includes(type);
}

function toggleSection(type: SectionType) {
    if (isRequired(type)) return;
    const sec = report.value.sections.find(s => s.type === type);
    if (sec) sec.enabled = !sec.enabled;
    if (editingSection.value === type && !sec?.enabled) editingSection.value = null;
}

function toggleEdit(type: SectionType) {
    editingSection.value = editingSection.value === type ? null : type;
}

function updateSection(updated: StructuredSection) {
    const idx = report.value.sections.findIndex(s => s.type === updated.type);
    if (idx >= 0) Object.assign(report.value.sections[idx], updated);
}

// ── Component dispatch ────────────────────────────────────────────────────────

const SECTION_COMPONENTS = {
    'cover':              ReportSectionCover,
    'kpis':               ReportSectionKpis,
    'trend':              ReportSectionTrend,
    'category-breakdown': ReportSectionCategoryBreakdown,
    'findings-examples':  ReportSectionFindings,
    'ca-table':           ReportSectionCaTable,
    'summary-text':       ReportSectionSummaryText,
    'highlights':         ReportSectionHighlights,
} as const;

function sectionComponent(type: SectionType) {
    return SECTION_COMPONENTS[type];
}

function sectionProps(sec: StructuredSection): Record<string, unknown> {
    const div = divisions.value.find(d => d.id === selectedDivisionId.value);
    switch (sec.type) {
        case 'cover':
            return {
                templateType: report.value.templateType,
                divisionCode: div?.code ?? report.value.divisionCode,
                divisionName: div?.name ?? '',
                period:   report.value.period || buildPeriodLabel(dateFrom.value, dateTo.value),
                dateFrom: report.value.dateFrom,
                dateTo:   report.value.dateTo,
            };
        case 'kpis':
            return { kpis: builder.kpis.value };
        case 'trend':
            return { trends: builder.trends.value };
        case 'category-breakdown':
            return { categories: builder.categories.value };
        case 'findings-examples':
            return { section: sec, sectionNames: builder.sectionNames.value };
        case 'ca-table':
            return { caRows: builder.caRows.value };
        case 'summary-text':
            return { section: sec };
        case 'highlights':
            return { section: sec };
        default:
            return {};
    }
}

// ── Generate ──────────────────────────────────────────────────────────────────

async function generate() {
    const div = divisions.value.find(d => d.id === selectedDivisionId.value);
    if (!div) return;
    await builder.build({
        divisionId: div.id,
        division:   div,
        dateFrom:   dateFrom.value || null,
        dateTo:     dateTo.value   || null,
    });
    report.value.divisionId   = div.id;
    report.value.divisionCode = div.code;
    report.value.period   = buildPeriodLabel(dateFrom.value, dateTo.value);
    report.value.dateFrom = dateFrom.value || null;
    report.value.dateTo   = dateTo.value   || null;
}

// ── PDF export ────────────────────────────────────────────────────────────────

const pdfGenerating = ref(false);
const pdfError = ref<string | null>(null);

async function printReport() {
    if (!builder.hasData.value) return;
    pdfGenerating.value = true;
    pdfError.value = null;
    const div = divisions.value.find(d => d.id === selectedDivisionId.value);
    report.value.divisionId   = selectedDivisionId.value ?? 0;
    report.value.divisionCode = div?.code ?? report.value.divisionCode;
    report.value.period       = buildPeriodLabel(dateFrom.value, dateTo.value);
    report.value.dateFrom     = dateFrom.value || null;
    report.value.dateTo       = dateTo.value   || null;
    try {
        const blob = await service.postBlob('/v1/reports/generate-structured', {
            structuredReportJson: JSON.stringify(report.value),
        });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `report-${report.value.divisionCode}-${report.value.period.replace(/\s/g, '-')}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
    } catch (e: unknown) {
        pdfError.value = e instanceof Error ? e.message : 'PDF generation failed. Please try again.';
    } finally {
        pdfGenerating.value = false;
    }
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function buildPeriodLabel(from: string, to: string): string {
    if (!from && !to) return 'All Time';
    if (!from) return `Up to ${to}`;
    if (!to)   return `From ${from}`;
    const f = new Date(from);
    const t = new Date(to);
    const q = Math.floor(f.getMonth() / 3) + 1;
    const qStart = new Date(f.getFullYear(), (q - 1) * 3, 1);
    const qEnd   = new Date(f.getFullYear(), q * 3, 0);
    if (f.getTime() === qStart.getTime() && t.toDateString() === qEnd.toDateString()) {
        return `Q${q} ${f.getFullYear()}`;
    }
    return `${from} – ${to}`;
}

// ── Lifecycle ─────────────────────────────────────────────────────────────────

onMounted(async () => {
    divisions.value = await service.getDivisions();
    if (divisions.value.length) {
        selectedDivisionId.value = divisions.value[0].id;
        await loadDraftList();
    }
    const now = new Date();
    const q   = Math.floor(now.getMonth() / 3);
    dateFrom.value = new Date(now.getFullYear(), q * 3, 1).toISOString().split('T')[0];
    dateTo.value   = new Date(now.getFullYear(), q * 3 + 3, 0).toISOString().split('T')[0];
});
</script>

<style scoped>
.composer-tab {
    display: flex;
    align-items: center;
    gap: 5px;
    padding: 5px 14px 6px;
    font-size: 0.75rem;
    font-weight: 500;
    color: #64748b;
    background: transparent;
    border: none;
    border-bottom: 2px solid transparent;
    cursor: pointer;
    transition: color 0.12s, border-color 0.12s;
    margin-bottom: -1px;
}
.composer-tab:hover { color: #94a3b8; }
.composer-tab--active {
    color: #93c5fd;
    border-bottom-color: #3b82f6;
}

.composer-select,
.composer-input {
    background: var(--surface-3);
    border: 1px solid var(--border);
    border-radius: var(--radius-sm);
    padding: 0.375rem 0.625rem;
    font-size: 0.8125rem;
    color: var(--text-primary);
    outline: none;
    transition: border-color var(--transition-base);
}
.composer-select:focus,
.composer-input:focus {
    border-color: rgba(59, 130, 246, 0.7);
    box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.2);
}
.composer-input::placeholder { color: var(--text-muted); }

@media print {
    button { display: none !important; }
}
</style>

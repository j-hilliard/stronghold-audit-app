<template>
    <div class="flex flex-col h-full composer-view">
        <BasePageHeader
            title="Report Composer"
            subtitle="Build, edit, and export compliance reports"
            icon="pi pi-file-edit"
        />

        <!-- Top bar: draft selector + filters -->
        <div class="flex items-center gap-2 px-4 py-2.5 bg-surface-2 border-b border-slate-700/60 flex-wrap shadow-elevation-1">
            <!-- Group 1: context filters -->
            <div class="flex items-center gap-2 flex-wrap">
                <!-- Division -->
                <select v-model="selectedDivisionId" @change="onDivisionChange"
                    data-testid="composer-filter-division"
                    class="composer-select">
                    <option :value="null" disabled>Select division…</option>
                    <option v-for="d in divisions" :key="d.id" :value="d.id">{{ d.code }} — {{ d.name }}</option>
                </select>

                <!-- Report type -->
                <select v-model="reportType" class="composer-select">
                    <option value="custom">Custom Report</option>
                    <option value="newsletter">Newsletter</option>
                </select>

                <!-- Newsletter settings toggle -->
                <button
                    v-if="reportType === 'newsletter'"
                    @click="showNewsletterSettings = !showNewsletterSettings"
                    :class="['flex items-center gap-1.5 px-3 py-1.5 rounded-md text-xs font-medium transition-colors border',
                        showNewsletterSettings
                            ? 'bg-blue-700 border-blue-600 text-white'
                            : 'bg-slate-700/60 border-slate-600 text-slate-300 hover:bg-slate-700 hover:text-white']"
                >
                    <i class="pi pi-palette text-[11px]" />
                    Newsletter Settings
                </button>

                <!-- Date range -->
                <input type="date" v-model="dateFrom" data-testid="composer-filter-from"
                    class="composer-input w-36" />
                <span class="text-slate-600 text-xs font-medium">→</span>
                <input type="date" v-model="dateTo" data-testid="composer-filter-to"
                    class="composer-input w-36" />
            </div>

            <!-- Divider -->
            <div class="h-6 w-px bg-slate-700 hidden sm:block mx-1" />

            <!-- Group 2: draft management -->
            <div class="flex items-center gap-2 flex-1 min-w-0 flex-wrap">
                <!-- Draft title -->
                <input
                    v-model="draft.meta.value.title"
                    type="text"
                    placeholder="Draft title…"
                    data-testid="composer-draft-title"
                    class="composer-input flex-1 min-w-36"
                />

                <!-- Load existing draft -->
                <select v-model="selectedDraftId" @change="onLoadDraft"
                    data-testid="composer-draft-select"
                    class="composer-select">
                    <option :value="null">New draft</option>
                    <option v-for="d in draftList" :key="d.id" :value="d.id">
                        {{ d.divisionCode }} — {{ d.title }} ({{ d.period }})
                    </option>
                </select>

                <button v-if="draft.meta.value.id !== null" @click="confirmDelete"
                    class="px-2 py-1.5 text-xs text-red-500 hover:text-red-400 hover:bg-red-900/20 rounded transition-colors"
                    title="Delete current draft">
                    <i class="pi pi-trash" />
                </button>

                <!-- Manage all drafts -->
                <button @click="openManageDrafts"
                    class="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-slate-700/60 border border-slate-600 rounded-md text-slate-300 hover:bg-slate-700 hover:text-white transition-colors"
                    title="View and delete all drafts">
                    <i class="pi pi-list text-[11px]" /> Manage Drafts
                </button>
            </div>
        </div>

        <!-- Manage Drafts dialog -->
        <Dialog v-model:visible="showManageDrafts" modal header="Manage Drafts" :style="{ width: '680px' }">
            <div class="flex flex-col gap-3">
                <!-- Loading -->
                <div v-if="draftsLoading" class="py-6 text-center text-slate-400 text-sm">
                    <i class="pi pi-spin pi-spinner mr-2" />Loading…
                </div>

                <!-- Empty -->
                <div v-else-if="allDrafts.length === 0" class="py-6 text-center text-slate-400 text-sm">
                    No saved drafts found.
                </div>

                <!-- Table -->
                <div v-else class="overflow-auto max-h-96">
                    <table class="w-full text-sm border-collapse">
                        <thead class="sticky top-0 bg-slate-800">
                            <tr class="text-left text-xs text-slate-400 uppercase border-b border-slate-700">
                                <th class="pb-2 pr-2 w-8">
                                    <input type="checkbox"
                                        :checked="selectedDraftIds.length === allDrafts.length"
                                        @change="toggleSelectAll"
                                        class="accent-blue-500 cursor-pointer" />
                                </th>
                                <th class="pb-2 pr-3">Division</th>
                                <th class="pb-2 pr-3">Title</th>
                                <th class="pb-2 pr-3">Period</th>
                                <th class="pb-2 pr-3">Updated</th>
                                <th class="pb-2"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="d in allDrafts" :key="d.id"
                                class="border-b border-slate-700/50 cursor-pointer transition-colors"
                                :class="selectedDraftIds.includes(d.id) ? 'bg-blue-900/30' : 'hover:bg-slate-700/30'"
                                @click="toggleDraftSelection(d.id)">
                                <td class="py-2 pr-2" @click.stop>
                                    <input type="checkbox"
                                        :checked="selectedDraftIds.includes(d.id)"
                                        @change="toggleDraftSelection(d.id)"
                                        class="accent-blue-500 cursor-pointer" />
                                </td>
                                <td class="py-2 pr-3 text-slate-300 text-xs">{{ d.divisionCode }}</td>
                                <td class="py-2 pr-3 text-slate-200">{{ d.title || '(Untitled)' }}</td>
                                <td class="py-2 pr-3 text-slate-400 text-xs whitespace-nowrap">{{ d.dateFrom ?? '?' }} – {{ d.dateTo ?? '?' }}</td>
                                <td class="py-2 pr-3 text-slate-500 text-xs whitespace-nowrap">{{ new Date(d.updatedAt ?? d.createdAt).toLocaleDateString() }}</td>
                                <td class="py-2 text-right" @click.stop>
                                    <button @click="loadDraftFromManager(d)"
                                        class="px-2 py-1 text-xs bg-slate-700 hover:bg-slate-600 text-slate-200 rounded">Load</button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <!-- Footer actions -->
                <div class="flex items-center justify-between pt-1 border-t border-slate-700">
                    <button
                        v-if="selectedDraftIds.length > 0"
                        @click="bulkDeleteDrafts"
                        :disabled="bulkDeleting"
                        class="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-red-800 hover:bg-red-700 text-white rounded disabled:opacity-50"
                    >
                        <i class="pi pi-trash text-xs" />
                        Delete Selected ({{ selectedDraftIds.length }})
                    </button>
                    <span v-else class="text-xs text-slate-500">Select rows to bulk delete</span>
                    <button @click="showManageDrafts = false"
                        class="px-3 py-1.5 text-sm bg-slate-700 hover:bg-slate-600 text-slate-200 rounded">Close</button>
                </div>
            </div>
        </Dialog>

        <!-- Error / loading states -->
        <div v-if="engine.error.value" class="px-4 py-2 text-sm text-red-400 bg-red-900/20 border-b border-red-800">
            {{ engine.error.value }}
        </div>

        <!-- Main composer layout -->
        <div class="flex flex-1 min-h-0 composer-main">
            <ComposerToolbar
                :generating="engine.loading.value"
                :saving="draft.saving.value"
                :is-dirty="draft.isDirty.value"
                :save-error="draft.saveError.value"
                :last-saved-at="draft.lastSavedAt.value"
                :can-undo="draft.canUndo.value"
                :can-redo="draft.canRedo.value"
                :active-theme-id="activeThemeId"
                :sections="engine.sections.value"
                :used-line-sections="usedLineSections"
                :used-bar-sections="usedBarSections"
                @generate="onGenerate"
                @save="draft.save()"
                @print="printReport"
                @undo="draft.undo()"
                @redo="draft.redo()"
                @apply-theme="onApplyTheme"
                @add-block="onAddBlock"
            />

            <ComposerCanvas
                :blocks="draft.blocks.value"
                :selected-id="selectedBlockId"
                @select="selectedBlock = $event"
                @remove="draft.removeBlock($event)"
                @move-up="onMoveUp"
                @move-down="onMoveDown"
                @duplicate="draft.duplicateBlock($event); draft.scheduleAutosave()"
                @update-content="onUpdateContent"
                @update-is-edited="onUpdateIsEdited"
                @update-layout="onUpdateLayout"
            />

            <NewsletterSettingsPanel
                v-if="reportType === 'newsletter' && showNewsletterSettings"
                v-model="newsletterTemplate"
                :available-sections="engine.sections.value"
                :saving="newsletterSaving"
                :save-error="newsletterSaveError"
                :saved-at="newsletterSavedAt"
                @save="saveNewsletterTemplate"
                @close="showNewsletterSettings = false"
            />

            <ComposerPropertyPanel
                v-if="!(reportType === 'newsletter' && showNewsletterSettings)"
                :block="selectedBlock"
                @update="onBlockUpdate"
                @bring-forward="draft.bringForward($event); draft.scheduleAutosave()"
                @send-backward="draft.sendBackward($event); draft.scheduleAutosave()"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue';
import { useUserStore } from '@/stores/userStore';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import type { DivisionDto, ReportDraftListItemDto } from '@/apiclient/auditClient';
import Dialog from 'primevue/dialog';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import ComposerToolbar from '../components/ComposerToolbar.vue';
import ComposerCanvas from '../components/ComposerCanvas.vue';
import ComposerPropertyPanel from '../components/ComposerPropertyPanel.vue';
import NewsletterSettingsPanel from '../components/NewsletterSettingsPanel.vue';
import { useReportEngine } from '../composables/useReportEngine';
import { useReportDraft } from '../composables/useReportDraft';
import type { ReportBlock } from '../types/report-block';
import type { NewsletterTemplateDto } from '@/apiclient/auditClient';
import { REPORT_THEMES } from '../composables/useReportThemes';

const service = useAuditService();
const userStore = useUserStore();
const engine = useReportEngine();
const draft = useReportDraft();

const divisions = ref<DivisionDto[]>([]);
const draftList = ref<ReportDraftListItemDto[]>([]);
const selectedDivisionId = ref<number | null>(null);
const selectedDraftId = ref<number | null>(null);
const selectedBlock = ref<ReportBlock | null>(null);
const dateFrom = ref('');
const dateTo = ref('');

// Newsletter mode
const reportType = ref<'custom' | 'newsletter'>('custom');
const showNewsletterSettings = ref(false);
const newsletterTemplate = ref<NewsletterTemplateDto>({
    divisionId: 0,
    name: '',
    primaryColor: '#1e3a5f',
    accentColor: '#f59e0b',
    coverImageUrl: null,
    visibleSections: null,
    isDefault: true,
});
const newsletterSaving = ref(false);
const newsletterSaveError = ref<string | null>(null);
const newsletterSavedAt = ref<Date | null>(null);

const selectedBlockId = computed(() => selectedBlock.value?.id ?? null);

// Active theme tracking (purely visual — highlights the active swatch)
const activeThemeId = ref<string | null>(null);

function onApplyTheme(themeId: string) {
    draft.applyTheme(themeId);
    activeThemeId.value = themeId;
    draft.scheduleAutosave();
}

// ── Keyboard shortcuts ────────────────────────────────────────────────────────
function onKeyDown(e: KeyboardEvent) {
    const target = e.target as HTMLElement;
    const isTyping = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable;

    if (e.ctrlKey || e.metaKey) {
        if (e.key === 'z' && !e.shiftKey) { e.preventDefault(); draft.undo(); return; }
        if (e.key === 'y' || (e.key === 'z' && e.shiftKey)) { e.preventDefault(); draft.redo(); return; }
        if (e.key === 'd' && selectedBlock.value) {
            e.preventDefault();
            draft.duplicateBlock(selectedBlock.value.id);
            draft.scheduleAutosave();
            return;
        }
    }

    if ((e.key === 'Delete' || e.key === 'Backspace') && !isTyping && selectedBlock.value) {
        e.preventDefault();
        draft.removeBlock(selectedBlock.value.id);
        selectedBlock.value = null;
        draft.scheduleAutosave();
    }
}

onMounted(() => document.addEventListener('keydown', onKeyDown));
onUnmounted(() => document.removeEventListener('keydown', onKeyDown));

/**
 * Sections that already have at least one chart-line block on the canvas.
 * Passed to the toolbar so it can show a green "already added" indicator.
 */
const usedLineSections = computed(() => {
    const s = new Set<string>();
    for (const b of draft.blocks.value) {
        if (b.type === 'chart-line') s.add(b.content.sectionName);
    }
    return s;
});

/**
 * Sections that already have a section-specific chart-bar block.
 * The all-sections overview bar chart has labels.length > 2; single-section has labels = ['Division','Company'].
 */
const usedBarSections = computed(() => {
    const s = new Set<string>();
    for (const b of draft.blocks.value) {
        if (b.type === 'chart-bar' && b.content.labels.length === 2
            && b.content.labels[0] === 'Division') {
            s.add(b.content.title.replace(' — Division vs Company', ''));
        }
    }
    return s;
});

// Keep draft meta in sync with the filter bar
watch([dateFrom, dateTo], ([f, t]) => {
    draft.meta.value.dateFrom = f || null;
    draft.meta.value.dateTo = t || null;
});

watch(selectedDivisionId, (id) => {
    if (id) draft.meta.value.divisionId = id;
});

async function loadDivisions() {
    const client = service;
    divisions.value = await client.getDivisions();
    if (divisions.value.length && !selectedDivisionId.value) {
        selectedDivisionId.value = divisions.value[0].id;
        draft.meta.value.divisionId = divisions.value[0].id;
        draft.meta.value.divisionCode = divisions.value[0].code;
    }
}

async function loadDraftList() {
    const client = service;
    draftList.value = await client.getReportDrafts(selectedDivisionId.value);
}

async function onDivisionChange() {
    const div = divisions.value.find(d => d.id === selectedDivisionId.value);
    if (div) {
        draft.meta.value.divisionId = div.id;
        draft.meta.value.divisionCode = div.code;
    }
    await Promise.all([loadDraftList(), loadNewsletterTemplate()]);
}

async function loadNewsletterTemplate() {
    if (!selectedDivisionId.value) return;
    const client = service;
    try {
        const tmpl = await client.getNewsletterTemplate(selectedDivisionId.value);
        if (tmpl) {
            newsletterTemplate.value = tmpl;
        } else {
            newsletterTemplate.value = {
                divisionId: selectedDivisionId.value,
                name: `${divisions.value.find(d => d.id === selectedDivisionId.value)?.code ?? ''} Newsletter`,
                primaryColor: '#1e3a5f',
                accentColor: '#f59e0b',
                coverImageUrl: null,
                visibleSections: null,
                isDefault: true,
            };
        }
    } catch {
        // non-fatal — defaults already set
    }
}

async function saveNewsletterTemplate() {
    if (!selectedDivisionId.value) return;
    newsletterSaving.value = true;
    newsletterSaveError.value = null;
    try {
        const client = service;
        const saved = await client.saveNewsletterTemplate({
            ...newsletterTemplate.value,
            divisionId: selectedDivisionId.value,
        });
        newsletterTemplate.value = saved;
        newsletterSavedAt.value = new Date();
    } catch (e: unknown) {
        newsletterSaveError.value = (e as Error)?.message ?? 'Save failed';
    } finally {
        newsletterSaving.value = false;
    }
}

async function onLoadDraft() {
    if (!selectedDraftId.value) return;
    await draft.loadDraft(selectedDraftId.value);
    selectedDivisionId.value = draft.meta.value.divisionId;
    dateFrom.value = draft.meta.value.dateFrom ?? '';
    dateTo.value = draft.meta.value.dateTo ?? '';
    selectedBlock.value = null;
}

async function onGenerate() {
    const div = divisions.value.find(d => d.id === selectedDivisionId.value);
    if (!div) return;

    const period = buildPeriodLabel(dateFrom.value, dateTo.value);
    draft.meta.value.period = period;

    const newBlocks = await engine.generateBlocks({
        divisionId: div.id,
        divisionCode: div.code,
        divisionName: div.name,
        period,
        dateFrom: dateFrom.value || null,
        dateTo: dateTo.value || null,
        preparedBy: userStore.userAccountInfo?.name ?? 'Stronghold',
        existingBlocks: draft.blocks.value,
    });

    draft.setBlocks(newBlocks);
    draft.scheduleAutosave();
}

async function onAddBlock(type: ReportBlock['type'], sectionName?: string) {
    const div = divisions.value.find(d => d.id === selectedDivisionId.value);
    const newBlock = await engine.buildSingleBlock(
        type,
        selectedDivisionId.value ?? 0,
        div?.code ?? '',
        draft.meta.value.period,
        userStore.userAccountInfo?.name ?? 'Stronghold',
        sectionName,
        draft.blocks.value,
    );
    draft.blocks.value.push(newBlock);
    selectedBlock.value = newBlock;
    draft.scheduleAutosave();

    // Scroll the new block into view after Vue flushes the DOM
    await nextTick();
    const el = document.querySelector(`[data-testid="composer-block-${newBlock.id}"]`);
    el?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

function onBlockUpdate(updated: ReportBlock) {
    draft.updateBlock(updated);
    selectedBlock.value = updated;
    draft.scheduleAutosave();
}

function onUpdateContent(id: string, content: unknown) {
    const block = draft.blocks.value.find(b => b.id === id);
    if (!block) return;
    const updated = { ...block, content } as ReportBlock;
    draft.updateBlock(updated);
    if (selectedBlock.value?.id === id) selectedBlock.value = updated;
    draft.scheduleAutosave();
}

function onUpdateIsEdited(id: string, value: boolean) {
    const block = draft.blocks.value.find(b => b.id === id);
    if (!block) return;
    const updated = { ...block, isEdited: value };
    draft.updateBlock(updated);
    if (selectedBlock.value?.id === id) selectedBlock.value = updated;
}

function onUpdateLayout(id: string, layout: Partial<import('../types/report-block').BlockLayout>) {
    draft.updateLayout(id, layout);
    // Keep selectedBlock in sync so the property panel shows current coords
    if (selectedBlock.value?.id === id) {
        const found = draft.blocks.value.find(b => b.id === id);
        if (found) selectedBlock.value = found;
    }
    draft.scheduleAutosave();
}

function onMoveUp(id: string) {
    const idx = draft.blocks.value.findIndex(b => b.id === id);
    if (idx > 0) {
        const arr = [...draft.blocks.value];
        [arr[idx - 1], arr[idx]] = [arr[idx], arr[idx - 1]];
        draft.setBlocks(arr);
        draft.scheduleAutosave();
    }
}

function onReorder(orderedIds: string[]) {
    const map = new Map(draft.blocks.value.map(b => [b.id, b]));
    draft.setBlocks(orderedIds.map(id => map.get(id)!).filter(Boolean));
    draft.scheduleAutosave();
}

function onMoveDown(id: string) {
    const idx = draft.blocks.value.findIndex(b => b.id === id);
    if (idx < draft.blocks.value.length - 1) {
        const arr = [...draft.blocks.value];
        [arr[idx], arr[idx + 1]] = [arr[idx + 1], arr[idx]];
        draft.setBlocks(arr);
        draft.scheduleAutosave();
    }
}

async function confirmDelete() {
    if (!confirm('Delete this draft? This cannot be undone.')) return;
    await draft.deleteDraft();
    selectedDraftId.value = null;
    await loadDraftList();
}

// ── Manage Drafts dialog ───────────────────────────────────────────────────────

const showManageDrafts = ref(false);
const allDrafts = ref<ReportDraftListItemDto[]>([]);
const draftsLoading = ref(false);
const selectedDraftIds = ref<number[]>([]);
const bulkDeleting = ref(false);

async function openManageDrafts() {
    allDrafts.value = [];
    selectedDraftIds.value = [];
    draftsLoading.value = true;
    showManageDrafts.value = true;
    try {
        const client = service;
        allDrafts.value = await client.getReportDrafts(selectedDivisionId.value);
    } catch {
        // allDrafts stays empty — dialog still shows "No saved drafts found"
    } finally {
        draftsLoading.value = false;
    }
}

function toggleDraftSelection(id: number) {
    const idx = selectedDraftIds.value.indexOf(id);
    if (idx >= 0) {
        selectedDraftIds.value.splice(idx, 1);
    } else {
        selectedDraftIds.value.push(id);
    }
}

function toggleSelectAll() {
    if (selectedDraftIds.value.length === allDrafts.value.length) {
        selectedDraftIds.value = [];
    } else {
        selectedDraftIds.value = allDrafts.value.map(d => d.id);
    }
}

async function bulkDeleteDrafts() {
    const count = selectedDraftIds.value.length;
    if (!confirm(`Delete ${count} draft${count !== 1 ? 's' : ''}? This cannot be undone.`)) return;
    bulkDeleting.value = true;
    try {
        const client = service;
        const ids = [...selectedDraftIds.value];
        await Promise.all(ids.map(id => client.deleteReportDraft(id)));
        // If the active draft was among the deleted, clear local state only —
        // do not call draft.deleteDraft() since it was already deleted above.
        if (draft.meta.value.id !== null && ids.includes(draft.meta.value.id)) {
            draft.meta.value.id = null;
            draft.meta.value.rowVersion = null;
            selectedDraftId.value = null;
        }
        allDrafts.value = allDrafts.value.filter(d => !ids.includes(d.id));
        selectedDraftIds.value = [];
        await loadDraftList();
    } finally {
        bulkDeleting.value = false;
    }
}

async function loadDraftFromManager(d: ReportDraftListItemDto) {
    showManageDrafts.value = false;
    selectedDivisionId.value = d.divisionId;
    await loadDraftList();
    selectedDraftId.value = d.id;
    await onLoadDraft();
}

async function waitForPrintImages(root: HTMLElement) {
    const images = Array.from(root.querySelectorAll('img')) as HTMLImageElement[];
    await Promise.all(images.map(async (img) => {
        if (img.complete && img.naturalWidth > 0) return;
        try {
            await img.decode();
        } catch {
            await new Promise<void>((resolve) => {
                const done = () => resolve();
                img.addEventListener('load', done, { once: true });
                img.addEventListener('error', done, { once: true });
                setTimeout(done, 1200);
            });
        }
    }));
}

async function printReport() {
    const docPage = document.querySelector('.document-page') as HTMLElement | null;
    if (!docPage) { window.print(); return; }

    // Clone the document page so we can manipulate it without affecting the live DOM
    const clone = docPage.cloneNode(true) as HTMLElement;

    // Chart.js renders to <canvas>; cloned canvases lose bitmap pixels.
    // Convert each canvas to an <img> so print preview preserves chart content.
    const originalCanvases = Array.from(docPage.querySelectorAll('canvas')) as HTMLCanvasElement[];
    const clonedCanvases = Array.from(clone.querySelectorAll('canvas')) as HTMLCanvasElement[];
    originalCanvases.forEach((canvas, i) => {
        const cloneCanvas = clonedCanvases[i];
        if (!cloneCanvas) return;

        const rect = canvas.getBoundingClientRect();
        const width = Math.max(1, Math.round(rect.width || canvas.width || 1));
        const height = Math.max(1, Math.round(rect.height || canvas.height || 1));

        try {
            const img = document.createElement('img');
            img.src = canvas.toDataURL('image/png');
            img.style.width = `${width}px`;
            img.style.height = `${height}px`;
            img.style.display = 'block';
            cloneCanvas.replaceWith(img);
        } catch {
            // Fallback: copy pixels directly into the cloned canvas.
            try {
                cloneCanvas.width = canvas.width;
                cloneCanvas.height = canvas.height;
                cloneCanvas.style.width = `${width}px`;
                cloneCanvas.style.height = `${height}px`;
                const ctx = cloneCanvas.getContext('2d');
                if (ctx) {
                    ctx.clearRect(0, 0, cloneCanvas.width, cloneCanvas.height);
                    ctx.drawImage(canvas, 0, 0);
                }
            } catch {
                // Keep clone canvas as last-resort fallback.
            }
        }
    });

    // Remove block-control buttons (drag handles, up/down, remove) from the clone
    clone.querySelectorAll('.drag-handle, [title="Move up"], [title="Move down"], [title="Remove block"]')
        .forEach(el => el.remove());

    // Mount print root; CSS hides all other body children while this is present
    const printRoot = document.createElement('div');
    printRoot.id = 'print-root';
    printRoot.appendChild(clone);
    document.body.appendChild(printRoot);

    // afterprint fires when the print dialog closes (or is cancelled)
    // More reliable than calling removeChild synchronously since Firefox
    // doesn't block on window.print()
    const cleanup = () => {
        if (document.body.contains(printRoot)) document.body.removeChild(printRoot);
        window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);

    // Ensure converted chart images are decoded before print opens.
    await waitForPrintImages(printRoot);
    await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));
    await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));

    window.print();
}
function buildPeriodLabel(from: string, to: string): string {
    if (!from && !to) return 'All Time';
    if (!from) return `Up to ${to}`;
    if (!to) return `From ${from}`;
    // Try to detect a clean quarter
    const f = new Date(from);
    const t = new Date(to);
    const q = Math.floor(f.getMonth() / 3) + 1;
    const qStart = new Date(f.getFullYear(), (q - 1) * 3, 1);
    const qEnd = new Date(f.getFullYear(), q * 3, 0);
    if (
        f.getTime() === qStart.getTime() &&
        t.toDateString() === qEnd.toDateString()
    ) {
        return `Q${q} ${f.getFullYear()}`;
    }
    return `${from} – ${to}`;
}

onMounted(async () => {
    await loadDivisions();
    await Promise.all([loadDraftList(), loadNewsletterTemplate()]);

    // Default date range: current quarter
    const now = new Date();
    const q = Math.floor(now.getMonth() / 3);
    dateFrom.value = new Date(now.getFullYear(), q * 3, 1).toISOString().split('T')[0];
    dateTo.value = new Date(now.getFullYear(), q * 3 + 3, 0).toISOString().split('T')[0];
});
</script>

<style scoped>
.composer-select {
    background: rgb(51,65,85,0.5);
    border: 1px solid rgba(100,116,139,0.5);
    border-radius: 0.375rem;
    padding: 0.375rem 0.625rem;
    font-size: 0.8125rem;
    color: #e2e8f0;
    outline: none;
    transition: border-color 0.15s ease;
}
.composer-select:focus {
    border-color: rgba(59,130,246,0.7);
    box-shadow: 0 0 0 2px rgba(59,130,246,0.2);
}
.composer-input {
    background: rgb(51,65,85,0.5);
    border: 1px solid rgba(100,116,139,0.5);
    border-radius: 0.375rem;
    padding: 0.375rem 0.625rem;
    font-size: 0.8125rem;
    color: #e2e8f0;
    outline: none;
    transition: border-color 0.15s ease;
}
.composer-input:focus {
    border-color: rgba(59,130,246,0.7);
    box-shadow: 0 0 0 2px rgba(59,130,246,0.2);
}
.composer-input::placeholder {
    color: #475569;
}
</style>


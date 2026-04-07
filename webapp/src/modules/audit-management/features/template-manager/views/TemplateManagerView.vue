<template>
    <div class="flex flex-col h-full overflow-hidden">
        <BasePageHeader
            title="Template Manager"
            subtitle="Manage checklist templates, questions, and versions"
            icon="pi pi-table"
        />

        <!-- ── Filter bar ──────────────────────────────────────────────────── -->
        <div class="px-4 pt-3 pb-2 flex items-end gap-4 border-b border-slate-700 shrink-0">
            <div class="flex flex-col gap-1 min-w-[200px]">
                <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Division</label>
                <select
                    v-model="selectedDivisionCode"
                    class="bg-slate-800 border border-slate-600 rounded px-3 py-2 text-sm text-slate-200 focus:outline-none focus:border-slate-400"
                >
                    <option value="">— Select division —</option>
                    <option v-for="g in divisionGroups" :key="g.divisionCode" :value="g.divisionCode">
                        {{ g.divisionName }}
                    </option>
                </select>
            </div>

            <div v-if="selectedGroup" class="flex flex-col gap-1 min-w-[220px]">
                <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Version</label>
                <select
                    v-model="selectedVersionId"
                    class="bg-slate-800 border border-slate-600 rounded px-3 py-2 text-sm text-slate-200 focus:outline-none focus:border-slate-400"
                >
                    <option v-for="v in selectedGroup.versions" :key="v.id" :value="v.id">
                        Version {{ v.versionNumber }} — {{ v.status }} ({{ v.questionCount }} Qs)
                    </option>
                </select>
            </div>

            <div v-if="selectedVersion" class="ml-auto flex items-end gap-2 pb-0.5">
                <button
                    v-if="selectedVersion.status === 'Active' && !hasDraft"
                    @click="onClone"
                    :disabled="adminStore.saving"
                    class="px-3 py-2 text-sm bg-slate-700 hover:bg-slate-600 text-slate-200 rounded border border-slate-600 disabled:opacity-50"
                >
                    Create Draft
                </button>
                <span
                    v-if="selectedVersion.status === 'Active' && hasDraft"
                    class="text-xs text-amber-400 italic"
                >Draft already exists — select it from the Version dropdown</span>
                <button
                    v-if="selectedVersion.status === 'Draft' && draftDetail"
                    @click="confirmPublish = true"
                    :disabled="adminStore.saving"
                    class="px-3 py-2 text-sm bg-green-700 hover:bg-green-600 text-white rounded disabled:opacity-50"
                >
                    Publish
                </button>
            </div>
        </div>

        <!-- ── Body: library + canvas ──────────────────────────────────────── -->
        <div class="flex flex-1 overflow-hidden">

            <!-- Left: Section Library (Draft mode only) -->
            <div
                v-if="isDraft"
                class="w-56 shrink-0 border-r border-slate-700 flex flex-col overflow-hidden bg-slate-900"
            >
                <div class="px-3 py-2 border-b border-slate-700">
                    <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">Section Library</p>
                    <p class="text-xs text-slate-600 mt-0.5">Drag into template →</p>
                </div>

                <div class="flex-1 overflow-y-auto py-2">
                    <!-- Group by division -->
                    <div v-for="group in libraryGroups" :key="group.divisionCode" class="mb-3">
                        <p class="px-3 pb-1 text-xs text-slate-500 font-medium">{{ group.divisionName }}</p>
                        <draggable
                            :list="group.items"
                            item-key="sectionId"
                            :group="{ name: 'sections', pull: 'clone', put: false }"
                            :sort="false"
                            class="space-y-1 px-2"
                        >
                            <template #item="{ element: item }">
                                <div
                                    :data-section-id="item.sectionId"
                                    class="flex items-center gap-2 px-2 py-1.5 rounded bg-slate-800 border border-slate-700 cursor-grab hover:border-slate-500 hover:bg-slate-750 transition-colors select-none"
                                    :title="`${item.name} — ${item.questionCount} questions`"
                                >
                                    <i class="pi pi-bars text-slate-600 text-xs shrink-0" />
                                    <span class="flex-1 text-xs text-slate-300 leading-tight truncate">{{ item.name }}</span>
                                    <span class="text-[10px] text-slate-500 shrink-0">{{ item.questionCount }}Q</span>
                                </div>
                            </template>
                        </draggable>
                    </div>

                    <div v-if="!adminStore.sectionLibrary.length" class="px-3 py-4 text-xs text-slate-600 italic">
                        No active templates found
                    </div>
                </div>
            </div>

            <!-- Right: Template canvas -->
            <div class="flex-1 overflow-y-auto p-4">

                <!-- Nothing selected -->
                <div v-if="!selectedDivisionCode" class="h-64 flex items-center justify-center text-slate-500 text-sm">
                    Select a division above to view its templates
                </div>

                <div v-else-if="adminStore.loading" class="text-slate-400 text-sm">Loading…</div>

                <div v-else-if="!selectedVersion" class="h-64 flex items-center justify-center text-slate-500 text-sm">
                    No template versions found
                </div>

                <template v-else>
                    <!-- Version title -->
                    <div class="flex items-center gap-3 mb-4">
                        <h2 class="text-lg font-semibold text-slate-200">
                            {{ selectedVersion.divisionName }} — Version {{ selectedVersion.versionNumber }}
                        </h2>
                        <span :class="statusBadgeClass(selectedVersion.status)" class="text-xs px-2 py-0.5 rounded font-medium">
                            {{ selectedVersion.status }}
                        </span>
                    </div>

                    <!-- READ-ONLY: Active / Superseded -->
                    <template v-if="!isDraft">
                        <div
                            v-for="section in draftDetail?.sections"
                            :key="section.id"
                            class="mb-3 bg-slate-800 border border-slate-700 rounded-lg overflow-hidden"
                        >
                            <div class="px-4 py-2.5 bg-slate-750 border-b border-slate-700 flex items-center gap-2">
                                <span class="font-medium text-slate-200">{{ section.name }}</span>
                                <span v-if="section.reportingCategoryName" class="text-xs text-slate-400 bg-slate-700 px-1.5 py-0.5 rounded">
                                    {{ section.reportingCategoryName }}
                                </span>
                                <span class="text-xs text-slate-500 ml-auto">{{ section.questions.length }} question{{ section.questions.length !== 1 ? 's' : '' }}</span>
                            </div>
                            <div class="divide-y divide-slate-700">
                                <div v-for="q in section.questions" :key="q.versionQuestionId" class="px-4 py-2 text-sm text-slate-300 flex items-start gap-2">
                                    <span class="text-slate-500 text-xs mt-0.5 w-5 shrink-0">{{ q.displayOrder }}.</span>
                                    <span>{{ q.questionText }}</span>
                                </div>
                                <div v-if="!section.questions.length" class="px-4 py-2 text-sm text-slate-500 italic">No questions</div>
                            </div>
                        </div>
                    </template>

                    <!-- EDITABLE: Draft -->
                    <template v-else-if="draftDetail">
                        <!-- Drop zone hint when draft is empty -->
                        <div
                            v-if="!draftDetail.sections.length"
                            class="mb-3 rounded-xl border-2 border-dashed border-slate-600 p-8 text-center text-slate-500 text-sm"
                        >
                            <i class="pi pi-arrow-left block text-2xl mb-2 text-slate-600" />
                            Drag sections from the library on the left to start building this template
                        </div>

                        <!-- Sections canvas (drag-drop between library and here) -->
                        <draggable
                            :list="draftDetail.sections"
                            item-key="id"
                            handle=".section-drag-handle"
                            :group="{ name: 'sections', pull: true, put: true }"
                            @add="onSectionDroppedFromLibrary"
                            @end="onReorderSections"
                            class="flex flex-col gap-3"
                            :class="{ 'min-h-[60px]': true }"
                        >
                            <template #item="{ element: section }">
                                <div class="bg-slate-800 border border-slate-700 rounded-lg overflow-hidden">
                                    <!-- Section header -->
                                    <div class="px-3 py-2.5 bg-slate-750 border-b border-slate-700 flex items-center gap-2">
                                        <span class="section-drag-handle cursor-grab text-slate-500 hover:text-slate-300 pi pi-bars shrink-0" title="Drag to reorder" />

                                        <template v-if="editingSectionId === section.id">
                                            <input
                                                v-model="editingSectionName"
                                                @keydown.enter="saveSectionName(section)"
                                                @keydown.escape="editingSectionId = null"
                                                @blur="saveSectionName(section)"
                                                class="flex-1 bg-slate-700 border border-slate-500 rounded px-2 py-0.5 text-sm text-slate-200 focus:outline-none focus:border-blue-400"
                                                ref="sectionNameInput"
                                            />
                                        </template>
                                        <template v-else>
                                            <span
                                                class="flex-1 font-medium text-slate-200 text-sm cursor-pointer hover:text-white"
                                                @click="startEditSectionName(section)"
                                                title="Click to rename"
                                            >{{ section.name }}</span>
                                        </template>

                                        <span v-if="section.reportingCategoryName" class="text-xs text-slate-400 bg-slate-700 px-1.5 py-0.5 rounded shrink-0">{{ section.reportingCategoryName }}</span>
                                        <span class="text-xs text-slate-500 shrink-0">{{ section.questions.length }}Q</span>
                                        <button
                                            v-if="originalQuestionOrder.has(section.id)"
                                            @click="onResetSectionOrder(section)"
                                            class="text-slate-400 hover:text-blue-300 pi pi-refresh text-xs shrink-0"
                                            title="Reset to original order"
                                        />
                                        <button @click="confirmRemoveSection(section)" class="text-red-400 hover:text-red-300 pi pi-trash text-xs shrink-0 ml-1" title="Remove section" />
                                    </div>

                                    <!-- Questions -->
                                    <draggable
                                        :list="section.questions"
                                        item-key="versionQuestionId"
                                        handle=".drag-handle"
                                        @end="onReorderQuestions(section)"
                                        class="divide-y divide-slate-700"
                                    >
                                        <template #item="{ element: q, index: qIndex }">
                                            <div class="px-4 py-2 flex items-center gap-3 group">
                                                <span class="drag-handle cursor-grab text-slate-600 hover:text-slate-400 pi pi-bars shrink-0" />
                                                <span class="text-xs text-slate-500 w-5 shrink-0">{{ qIndex + 1 }}.</span>
                                                <template v-if="editingQuestionId === q.versionQuestionId">
                                                    <input
                                                        v-model="editingQuestionText"
                                                        @keydown.enter="saveQuestionText(section, q)"
                                                        @keydown.escape="editingQuestionId = null"
                                                        @blur="saveQuestionText(section, q)"
                                                        class="flex-1 bg-slate-700 border border-slate-500 rounded px-2 py-0.5 text-sm text-slate-200 focus:outline-none focus:border-blue-400"
                                                    />
                                                </template>
                                                <template v-else>
                                                    <span
                                                        class="flex-1 text-sm text-slate-300 cursor-pointer hover:text-white"
                                                        @click="startEditQuestion(q)"
                                                        title="Click to edit"
                                                    >{{ q.questionText }}</span>
                                                </template>
                                                <div class="flex gap-2 shrink-0 text-xs text-slate-500">
                                                    <span v-if="q.allowNA">N/A</span>
                                                    <span v-if="q.requireCommentOnNC">Cmnt</span>
                                                    <span v-if="q.isScoreable">Scored</span>
                                                </div>
                                                <button @click="startEditQuestion(q)" class="opacity-0 group-hover:opacity-100 text-slate-400 hover:text-blue-300 pi pi-pencil text-xs shrink-0 transition-opacity" title="Edit question" />
                                                <button @click="onRemoveQuestion(section.id, q.versionQuestionId)" class="opacity-0 group-hover:opacity-100 text-red-400 hover:text-red-300 pi pi-trash text-xs shrink-0 transition-opacity" title="Remove question" />
                                            </div>
                                        </template>
                                    </draggable>

                                    <!-- Add question -->
                                    <div class="px-4 py-2 border-t border-slate-700 flex gap-2">
                                        <input
                                            v-model="newQuestionText[section.id]"
                                            @keydown.enter="onAddQuestion(section.id)"
                                            type="text"
                                            placeholder="Type new question and press Enter…"
                                            class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-slate-400"
                                        />
                                        <button
                                            @click="onAddQuestion(section.id)"
                                            :disabled="!newQuestionText[section.id]?.trim() || adminStore.saving"
                                            class="px-2 py-1 text-xs bg-blue-700 hover:bg-blue-600 text-white rounded disabled:opacity-40"
                                        >Add</button>
                                    </div>
                                </div>
                            </template>
                        </draggable>

                        <!-- Add blank section -->
                        <div class="mt-3">
                            <template v-if="addingSectionMode">
                                <div class="flex gap-2">
                                    <input
                                        v-model="newSectionName"
                                        @keydown.enter="onAddSection"
                                        @keydown.escape="addingSectionMode = false"
                                        type="text"
                                        placeholder="Section name…"
                                        class="flex-1 bg-slate-700 border border-slate-500 rounded px-3 py-2 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-400"
                                        ref="newSectionInput"
                                    />
                                    <button @click="onAddSection" :disabled="!newSectionName.trim() || adminStore.saving" class="px-3 py-2 text-sm bg-blue-700 hover:bg-blue-600 text-white rounded disabled:opacity-40">Add Section</button>
                                    <button @click="addingSectionMode = false" class="px-3 py-2 text-sm bg-slate-700 hover:bg-slate-600 text-slate-200 rounded">Cancel</button>
                                </div>
                            </template>
                            <button
                                v-else
                                @click="startAddSection"
                                class="flex items-center gap-2 text-sm text-blue-400 hover:text-blue-300 px-3 py-2 border border-dashed border-slate-600 hover:border-slate-500 rounded-lg w-full transition-colors"
                            >
                                <i class="pi pi-plus text-xs" />
                                Add Blank Section
                            </button>
                        </div>
                    </template>
                </template>
            </div>
        </div>

        <!-- Publish confirmation -->
        <Dialog v-model:visible="confirmPublish" modal header="Publish Template Version" :style="{ width: '28rem' }">
            <p class="text-slate-300 text-sm mb-4">
                This will make <strong>Version {{ draftDetail?.versionNumber }}</strong> Active and mark the current Active version as Superseded.
                New audits will use this version. In-progress audits are unaffected.
            </p>
            <p class="text-amber-400 text-xs mb-4">This action cannot be undone.</p>
            <div class="flex justify-end gap-2">
                <button @click="confirmPublish = false" class="px-3 py-1.5 text-sm bg-slate-700 hover:bg-slate-600 text-slate-200 rounded">Cancel</button>
                <button @click="onPublish" :disabled="adminStore.saving" class="px-3 py-1.5 text-sm bg-green-700 hover:bg-green-600 text-white rounded disabled:opacity-50">Publish</button>
            </div>
        </Dialog>

        <!-- Remove question confirmation -->
        <Dialog v-model:visible="confirmRemoveQuestion.show" modal header="Remove Question" :style="{ width: '26rem' }">
            <p class="text-slate-300 text-sm mb-4">Remove this question from the draft? Historical audit data is not affected.</p>
            <div class="flex justify-end gap-2">
                <button @click="confirmRemoveQuestion.show = false" class="px-3 py-1.5 text-sm bg-slate-700 hover:bg-slate-600 text-slate-200 rounded">Cancel</button>
                <button @click="doRemoveQuestion" :disabled="adminStore.saving" class="px-3 py-1.5 text-sm bg-red-700 hover:bg-red-600 text-white rounded disabled:opacity-50">Remove</button>
            </div>
        </Dialog>

        <!-- Remove section confirmation -->
        <Dialog v-model:visible="confirmRemoveSectionData.show" modal header="Remove Section" :style="{ width: '28rem' }">
            <p class="text-slate-300 text-sm mb-2">Remove <strong>{{ confirmRemoveSectionData.sectionName }}</strong> and all its questions?</p>
            <p class="text-amber-400 text-xs mb-4">Historical audit data is not affected.</p>
            <div class="flex justify-end gap-2">
                <button @click="confirmRemoveSectionData.show = false" class="px-3 py-1.5 text-sm bg-slate-700 hover:bg-slate-600 text-slate-200 rounded">Cancel</button>
                <button @click="doRemoveSection" :disabled="adminStore.saving" class="px-3 py-1.5 text-sm bg-red-700 hover:bg-red-600 text-white rounded disabled:opacity-50">Remove Section</button>
            </div>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, nextTick } from 'vue';
import draggable from 'vuedraggable';
import Dialog from 'primevue/dialog';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAdminStore } from '../../../stores/adminStore';
import type { TemplateVersionListItemDto, DraftVersionDetailDto, DraftSectionDto, DraftQuestionDto, SectionLibraryItemDto } from '@/apiclient/auditClient';

const adminStore = useAdminStore();

onMounted(() => {
    adminStore.loadTemplates();
    adminStore.loadSectionLibrary();
});

// ── Division grouping ──────────────────────────────────────────────────────────

interface DivisionGroup {
    divisionCode: string;
    divisionName: string;
    versions: TemplateVersionListItemDto[];
    activeVersion: TemplateVersionListItemDto | undefined;
}

const divisionGroups = computed((): DivisionGroup[] => {
    const map = new Map<string, DivisionGroup>();
    for (const v of adminStore.templates) {
        if (!map.has(v.divisionCode)) {
            map.set(v.divisionCode, { divisionCode: v.divisionCode, divisionName: v.divisionName, versions: [], activeVersion: undefined });
        }
        const g = map.get(v.divisionCode)!;
        g.versions.push(v);
        if (v.status === 'Active') g.activeVersion = v;
    }
    return Array.from(map.values()).sort((a, b) => a.divisionName.localeCompare(b.divisionName));
});

// ── Library grouping ───────────────────────────────────────────────────────────

interface LibraryGroup {
    divisionCode: string;
    divisionName: string;
    items: SectionLibraryItemDto[];
}

const libraryGroups = computed((): LibraryGroup[] => {
    if (!selectedDivisionCode.value) return [];
    const filtered = adminStore.sectionLibrary.filter(item => item.divisionCode === selectedDivisionCode.value);
    if (!filtered.length) return [];
    const group: LibraryGroup = {
        divisionCode: filtered[0].divisionCode,
        divisionName: filtered[0].divisionName,
        items: filtered,
    };
    return [group];
});

// ── Filter state ───────────────────────────────────────────────────────────────

const selectedDivisionCode = ref('');
const selectedVersionId = ref<number | null>(null);

const selectedGroup = computed(() =>
    divisionGroups.value.find(g => g.divisionCode === selectedDivisionCode.value) ?? null
);

const selectedVersion = computed(() =>
    selectedGroup.value?.versions.find(v => v.id === selectedVersionId.value) ?? null
);

const isDraft = computed(() => selectedVersion.value?.status === 'Draft');
const hasDraft = computed(() => selectedGroup.value?.versions.some(v => v.status === 'Draft') ?? false);

const draftDetail = ref<DraftVersionDetailDto | null>(null);

watch(selectedDivisionCode, (code) => {
    if (!code) { selectedVersionId.value = null; draftDetail.value = null; return; }
    const group = divisionGroups.value.find(g => g.divisionCode === code);
    if (group) {
        selectedVersionId.value = group.activeVersion?.id ?? group.versions[0]?.id ?? null;
    }
});

watch(selectedVersionId, async (id) => {
    if (!id) { draftDetail.value = null; return; }
    draftDetail.value = null;
    await adminStore.loadDraft(id);
    draftDetail.value = adminStore.draftDetail;
});

// Track original question order per section (captured once per version load, for reset)
const originalQuestionOrder = ref(new Map<number, number[]>());
const capturedForVersionId = ref<number | null>(null);

watch(() => adminStore.draftDetail, (val) => {
    draftDetail.value = val;
    // Capture original order only on the first load for this version
    if (val && capturedForVersionId.value !== val.id) {
        capturedForVersionId.value = val.id;
        const map = new Map<number, number[]>();
        for (const s of val.sections) {
            map.set(s.id, s.questions.map(q => q.versionQuestionId));
        }
        originalQuestionOrder.value = map;
    }
});

watch(selectedVersionId, () => {
    capturedForVersionId.value = null;
    originalQuestionOrder.value = new Map();
});

// ── Clone ──────────────────────────────────────────────────────────────────────

async function onClone() {
    if (!selectedVersion.value) return;
    const newId = await adminStore.cloneVersion(selectedVersion.value.id);
    if (newId !== null) {
        await adminStore.loadTemplates(true);
        const newVersion = adminStore.templates.find(v => v.id === newId);
        if (newVersion) {
            selectedDivisionCode.value = newVersion.divisionCode;
            await nextTick();
            selectedVersionId.value = newId;
        }
    }
}

// ── Cross-list drag: section dropped from library ──────────────────────────────

async function onSectionDroppedFromLibrary(event: any) {
    if (!selectedVersionId.value || !draftDetail.value) return;

    // Vue-draggable inserts the library item into draftDetail.sections — remove it.
    // The library item is SectionLibraryItemDto, not DraftSectionDto.
    const newIndex = event.newIndex as number;
    draftDetail.value.sections.splice(newIndex, 1);

    // Read sectionId from the data attribute set on the dragged element — reliable across all environments.
    const idStr = (event.item as HTMLElement).dataset.sectionId;
    const sourceSectionId = idStr ? parseInt(idStr) : 0;

    if (!sourceSectionId) {
        console.error('onSectionDroppedFromLibrary: could not read data-section-id from dragged element', event.item);
        return;
    }

    await adminStore.copySection(selectedVersionId.value, sourceSectionId);
}

// ── Section name editing ───────────────────────────────────────────────────────

const editingSectionId = ref<number | null>(null);
const editingSectionName = ref('');
const sectionNameInput = ref<HTMLInputElement | null>(null);

function startEditSectionName(section: DraftSectionDto) {
    editingSectionId.value = section.id;
    editingSectionName.value = section.name;
    nextTick(() => sectionNameInput.value?.focus());
}

async function saveSectionName(section: DraftSectionDto) {
    const name = editingSectionName.value.trim();
    editingSectionId.value = null;
    if (!name || name === section.name || !selectedVersionId.value) return;
    await adminStore.updateSection(selectedVersionId.value, section.id, {
        name,
        isRequired: section.isRequired,
        reportingCategoryId: section.reportingCategoryId,
    });
}

// ── Add blank section ──────────────────────────────────────────────────────────

const addingSectionMode = ref(false);
const newSectionName = ref('');
const newSectionInput = ref<HTMLInputElement | null>(null);

function startAddSection() {
    addingSectionMode.value = true;
    newSectionName.value = '';
    nextTick(() => newSectionInput.value?.focus());
}

async function onAddSection() {
    const name = newSectionName.value.trim();
    if (!name || !selectedVersionId.value) return;
    const ok = await adminStore.addSection(selectedVersionId.value, name);
    if (ok) { addingSectionMode.value = false; newSectionName.value = ''; }
}

// ── Remove section ─────────────────────────────────────────────────────────────

const confirmRemoveSectionData = ref({ show: false, sectionId: 0, sectionName: '' });

function confirmRemoveSection(section: DraftSectionDto) {
    confirmRemoveSectionData.value = { show: true, sectionId: section.id, sectionName: section.name };
}

async function doRemoveSection() {
    if (!selectedVersionId.value) return;
    const ok = await adminStore.removeSection(selectedVersionId.value, confirmRemoveSectionData.value.sectionId);
    if (ok) confirmRemoveSectionData.value.show = false;
}

// ── Reorder sections ───────────────────────────────────────────────────────────

async function onReorderSections(event: any) {
    // Only fire on reorder within the canvas (not on cross-list add — handled by onSectionDroppedFromLibrary)
    if (event.from !== event.to) return;
    if (!selectedVersionId.value || !draftDetail.value) return;
    const ids = draftDetail.value.sections.map(s => s.id);
    await adminStore.reorderSections(selectedVersionId.value, ids);
}

async function onResetSectionOrder(section: DraftSectionDto) {
    if (!selectedVersionId.value) return;
    const original = originalQuestionOrder.value.get(section.id);
    if (!original) return;
    // Re-sort in place to match original order (questions not in original go to end)
    section.questions.sort((a, b) => {
        const ai = original.indexOf(a.versionQuestionId);
        const bi = original.indexOf(b.versionQuestionId);
        return (ai === -1 ? 9999 : ai) - (bi === -1 ? 9999 : bi);
    });
    await adminStore.reorderQuestions(selectedVersionId.value, section.questions.map(q => q.versionQuestionId));
}

// ── Edit question text ────────────────────────────────────────────────────────

const editingQuestionId = ref<number | null>(null);
const editingQuestionText = ref('');

function startEditQuestion(q: DraftQuestionDto) {
    editingQuestionId.value = q.versionQuestionId;
    editingQuestionText.value = q.questionText;
}

async function saveQuestionText(section: DraftSectionDto, q: DraftQuestionDto) {
    const text = editingQuestionText.value.trim();
    editingQuestionId.value = null;
    if (!text || text === q.questionText || !selectedVersionId.value) return;
    await adminStore.updateQuestion(selectedVersionId.value, q.versionQuestionId, text);
}

// ── Add question ───────────────────────────────────────────────────────────────

const newQuestionText = ref<Record<number, string>>({});

async function onAddQuestion(sectionId: number) {
    const text = newQuestionText.value[sectionId]?.trim();
    if (!text || !selectedVersionId.value) return;
    const ok = await adminStore.addQuestion(selectedVersionId.value, {
        sectionId,
        questionText: text,
        allowNA: true,
        requireCommentOnNC: true,
        isScoreable: true,
    });
    if (ok) newQuestionText.value[sectionId] = '';
}

// ── Remove question ────────────────────────────────────────────────────────────

const confirmRemoveQuestion = ref({ show: false, sectionId: 0, versionQuestionId: 0 });

function onRemoveQuestion(sectionId: number, versionQuestionId: number) {
    confirmRemoveQuestion.value = { show: true, sectionId, versionQuestionId };
}

async function doRemoveQuestion() {
    if (!selectedVersionId.value) return;
    const ok = await adminStore.removeQuestion(selectedVersionId.value, confirmRemoveQuestion.value.versionQuestionId);
    if (ok) confirmRemoveQuestion.value.show = false;
}

// ── Reorder questions ──────────────────────────────────────────────────────────

async function onReorderQuestions(section: DraftSectionDto) {
    if (!selectedVersionId.value) return;
    const ids = section.questions.map(q => q.versionQuestionId);
    await adminStore.reorderQuestions(selectedVersionId.value, ids);
}

// ── Publish ────────────────────────────────────────────────────────────────────

const confirmPublish = ref(false);

async function onPublish() {
    if (!selectedVersionId.value) return;
    const ok = await adminStore.publishDraft(selectedVersionId.value);
    if (ok) {
        confirmPublish.value = false;
        draftDetail.value = null;
        selectedVersionId.value = null;
    }
}

function statusBadgeClass(status: string): string {
    if (status === 'Active') return 'bg-green-800 text-green-300';
    if (status === 'Draft') return 'bg-amber-800 text-amber-300';
    return 'bg-slate-700 text-slate-400';
}
</script>

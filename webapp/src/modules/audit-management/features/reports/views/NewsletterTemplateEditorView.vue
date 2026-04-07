<template>
    <div>
        <BasePageHeader
            title="Newsletter Template Editor"
            subtitle="Customize colors, header image, and section visibility for each division's newsletter"
            icon="pi pi-palette"
        />

        <div class="p-4 flex gap-4 max-w-6xl">

            <!-- Left panel: division list -->
            <div class="w-56 shrink-0 space-y-1">
                <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2 px-1">Divisions</div>
                <button
                    v-for="d in divisions"
                    :key="d.id"
                    @click="selectDivision(d)"
                    :class="[
                        'w-full text-left px-3 py-2 rounded text-sm transition-colors',
                        selectedDivisionId === d.id
                            ? 'bg-blue-700 text-white'
                            : 'text-slate-300 hover:bg-slate-700'
                    ]"
                >
                    {{ d.code }}
                    <span class="text-xs opacity-60 ml-1">{{ d.name }}</span>
                    <span v-if="hasTemplate(d.id)" class="ml-1 text-blue-300 text-xs">●</span>
                </button>
            </div>

            <!-- Right panel: editor -->
            <div class="flex-1 min-w-0">
                <div v-if="!selectedDivisionId" class="text-slate-500 text-sm py-8 text-center">
                    Select a division on the left to configure its newsletter template.
                </div>

                <div v-else class="space-y-5">
                    <!-- Template name + default badge -->
                    <div class="bg-slate-800 border border-slate-700 rounded-lg p-4 space-y-4">
                        <div class="flex items-center justify-between">
                            <h3 class="text-sm font-semibold text-slate-200">
                                {{ selectedDivision?.code }} — Newsletter Template
                            </h3>
                            <label class="flex items-center gap-2 text-sm text-slate-400 cursor-pointer">
                                <input v-model="current.isDefault" type="checkbox" class="accent-blue-500" />
                                Default template for this division
                            </label>
                        </div>

                        <div>
                            <label class="block text-xs text-slate-400 mb-1">Template Name</label>
                            <input
                                v-model="current.name"
                                type="text"
                                placeholder="e.g. ETS Standard Quarterly"
                                class="w-full bg-slate-700 border border-slate-600 rounded px-3 py-2 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                            />
                        </div>
                    </div>

                    <!-- Colors -->
                    <div class="bg-slate-800 border border-slate-700 rounded-lg p-4 space-y-4">
                        <h3 class="text-sm font-semibold text-slate-200">Colors</h3>
                        <div class="grid grid-cols-2 gap-4">
                            <div>
                                <label class="block text-xs text-slate-400 mb-1">Primary Color</label>
                                <div class="flex items-center gap-2">
                                    <input
                                        v-model="current.primaryColor"
                                        type="color"
                                        class="w-10 h-9 rounded cursor-pointer bg-transparent border-0 p-0"
                                    />
                                    <input
                                        v-model="current.primaryColor"
                                        type="text"
                                        placeholder="#1e3a5f"
                                        class="flex-1 bg-slate-700 border border-slate-600 rounded px-3 py-2 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500 font-mono"
                                    />
                                </div>
                            </div>
                            <div>
                                <label class="block text-xs text-slate-400 mb-1">Accent Color</label>
                                <div class="flex items-center gap-2">
                                    <input
                                        v-model="current.accentColor"
                                        type="color"
                                        class="w-10 h-9 rounded cursor-pointer bg-transparent border-0 p-0"
                                    />
                                    <input
                                        v-model="current.accentColor"
                                        type="text"
                                        placeholder="#f59e0b"
                                        class="flex-1 bg-slate-700 border border-slate-600 rounded px-3 py-2 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500 font-mono"
                                    />
                                </div>
                            </div>
                        </div>

                        <!-- Live preview swatch -->
                        <div
                            class="rounded-lg p-4 flex items-center gap-3"
                            :style="`background: ${current.primaryColor}; border: 2px solid ${current.accentColor}`"
                        >
                            <div class="text-white font-semibold text-sm">Preview</div>
                            <div class="text-xs" :style="`color: ${current.accentColor}`">Accent text</div>
                            <div class="flex-1" />
                            <div class="text-xs text-white/70">{{ selectedDivision?.code }} Compliance Newsletter</div>
                        </div>
                    </div>

                    <!-- Cover image -->
                    <div class="bg-slate-800 border border-slate-700 rounded-lg p-4 space-y-3">
                        <h3 class="text-sm font-semibold text-slate-200">Cover Background Image</h3>

                        <!-- Upload or URL row -->
                        <div class="flex items-center gap-2">
                            <!-- File upload button -->
                            <label
                                class="flex items-center gap-1.5 px-3 py-2 text-sm bg-slate-700 hover:bg-slate-600 text-slate-200 rounded cursor-pointer shrink-0 border border-slate-600"
                                title="Upload from your computer"
                            >
                                <i class="pi pi-upload text-xs" /> Upload
                                <input
                                    ref="fileInputRef"
                                    type="file"
                                    accept="image/*"
                                    class="sr-only"
                                    @change="onFileUpload"
                                />
                            </label>

                            <span class="text-xs text-slate-500 shrink-0">or</span>

                            <!-- URL input -->
                            <input
                                v-model="current.coverImageUrl"
                                type="text"
                                placeholder="https://... paste an image URL"
                                class="flex-1 bg-slate-700 border border-slate-600 rounded px-3 py-2 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                            />

                            <!-- Clear -->
                            <button
                                v-if="current.coverImageUrl"
                                @click="current.coverImageUrl = ''"
                                class="px-2 py-2 text-slate-400 hover:text-red-400"
                                title="Remove image — use default gradient"
                            >
                                <i class="pi pi-times text-xs" />
                            </button>
                        </div>

                        <!-- Preview -->
                        <div
                            v-if="current.coverImageUrl"
                            class="h-28 rounded overflow-hidden bg-slate-700 relative"
                            :style="`background: url('${current.coverImageUrl}') center/cover no-repeat;`"
                        >
                            <div class="absolute inset-0 flex items-end p-2">
                                <span class="text-xs text-white/70 bg-black/40 rounded px-2 py-0.5">Cover preview</span>
                            </div>
                        </div>
                        <div v-else class="h-10 rounded flex items-center justify-center text-xs text-slate-500 border border-dashed border-slate-600">
                            No image — default gradient will be used
                        </div>
                    </div>

                    <!-- Section visibility -->
                    <div class="bg-slate-800 border border-slate-700 rounded-lg p-4 space-y-3">
                        <div class="flex items-center justify-between">
                            <h3 class="text-sm font-semibold text-slate-200">Visible Sections</h3>
                            <div class="flex gap-2">
                                <button @click="toggleAllSections(true)" class="text-xs text-blue-400 hover:text-blue-300">Enable All</button>
                                <span class="text-slate-600">|</span>
                                <button @click="toggleAllSections(false)" class="text-xs text-slate-400 hover:text-slate-200">Disable All</button>
                            </div>
                        </div>
                        <p class="text-xs text-slate-500">
                            Toggle which sections appear in the printed newsletter. Disabled sections are still audited — they're just hidden from the report.
                        </p>
                        <div class="grid grid-cols-2 gap-2">
                            <label
                                v-for="section in NEWSLETTER_SECTIONS"
                                :key="section"
                                class="flex items-center gap-2 text-sm text-slate-300 cursor-pointer py-1"
                            >
                                <input
                                    type="checkbox"
                                    :checked="current.enabledSections.includes(section)"
                                    @change="toggleSection(section)"
                                    class="accent-blue-500"
                                />
                                {{ section }}
                            </label>
                        </div>
                    </div>

                    <!-- Action buttons -->
                    <div class="flex items-center justify-between">
                        <button
                            @click="resetToDefaults"
                            class="px-3 py-1.5 text-sm text-slate-400 hover:text-slate-200"
                        >
                            Reset to Defaults
                        </button>
                        <div class="flex gap-2">
                            <button
                                @click="openNewsletter"
                                class="px-4 py-1.5 text-sm bg-slate-700 hover:bg-slate-600 text-white rounded"
                            >
                                <i class="pi pi-eye mr-1" /> Preview Newsletter
                            </button>
                            <button
                                @click="saveTemplate"
                                class="px-4 py-1.5 text-sm bg-blue-700 hover:bg-blue-600 text-white rounded"
                            >
                                <i class="pi pi-save mr-1" /> Save Template
                            </button>
                        </div>
                    </div>

                    <div v-if="saveMessage" class="text-sm text-emerald-400 text-right">{{ saveMessage }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type DivisionDto } from '@/apiclient/auditClient';

const router = useRouter();
const apiStore = useApiStore();

const divisions = ref<DivisionDto[]>([]);
const selectedDivisionId = ref<number | null>(null);
const fileInputRef = ref<HTMLInputElement | null>(null);

const selectedDivision = computed(() =>
    divisions.value.find(d => d.id === selectedDivisionId.value) ?? null
);

// Newsletter sections that can be toggled
const NEWSLETTER_SECTIONS = [
    'Cover Page',
    'KPI Summary',
    'Findings Overview',
    'Per-Section Trend Lines',
    'Auditor Performance',
    'Corrective Actions Log',
    'Narrative Summary',
    'Signature Block',
];

interface NewsletterTemplate {
    divisionId: number;
    name: string;
    primaryColor: string;
    accentColor: string;
    coverImageUrl: string;
    enabledSections: string[];
    isDefault: boolean;
}

function defaultTemplate(divisionId: number): NewsletterTemplate {
    return {
        divisionId,
        name: '',
        primaryColor: '#1e3a5f',
        accentColor: '#f59e0b',
        coverImageUrl: '',
        enabledSections: [...NEWSLETTER_SECTIONS],
        isDefault: true,
    };
}

const STORAGE_KEY = 'audit-newsletter-templates';

function loadAllTemplates(): Record<number, NewsletterTemplate> {
    try {
        return JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '{}');
    } catch {
        return {};
    }
}

function saveAllTemplates(templates: Record<number, NewsletterTemplate>) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(templates));
}

const current = ref<NewsletterTemplate>(defaultTemplate(0));
const saveMessage = ref('');

function hasTemplate(divisionId: number): boolean {
    const all = loadAllTemplates();
    return !!all[divisionId]?.name;
}

function selectDivision(d: DivisionDto) {
    selectedDivisionId.value = d.id;
    const all = loadAllTemplates();
    current.value = all[d.id] ?? defaultTemplate(d.id);
    current.value.divisionId = d.id;
    saveMessage.value = '';
}

function toggleSection(section: string) {
    const idx = current.value.enabledSections.indexOf(section);
    if (idx >= 0) {
        current.value.enabledSections.splice(idx, 1);
    } else {
        current.value.enabledSections.push(section);
    }
}

function toggleAllSections(enable: boolean) {
    current.value.enabledSections = enable ? [...NEWSLETTER_SECTIONS] : [];
}

function onFileUpload(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (e) => {
        current.value.coverImageUrl = e.target?.result as string;
    };
    reader.readAsDataURL(file);
    // Reset input so the same file can be re-selected if cleared
    if (fileInputRef.value) fileInputRef.value.value = '';
}

function resetToDefaults() {
    if (!selectedDivisionId.value) return;
    current.value = defaultTemplate(selectedDivisionId.value);
}

function saveTemplate() {
    if (!selectedDivisionId.value) return;
    const all = loadAllTemplates();
    all[selectedDivisionId.value] = { ...current.value };
    saveAllTemplates(all);
    saveMessage.value = 'Template saved!';
    setTimeout(() => saveMessage.value = '', 3000);
}

function openNewsletter() {
    router.push({
        path: '/audit-management/newsletter',
        query: selectedDivisionId.value ? { divisionId: selectedDivisionId.value } : {},
    });
}

onMounted(async () => {
    const client = new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
    divisions.value = await client.getDivisions();
    if (divisions.value.length > 0) {
        selectDivision(divisions.value[0]);
    }
});
</script>

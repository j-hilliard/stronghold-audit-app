<template>
    <div>
        <BasePageHeader
            icon="pi pi-plus"
            title="New Audit"
            subtitle="Configure and start a new compliance audit"
        />

        <div class="p-4 sm:p-6 max-w-xl mx-auto lg:mx-0">
            <div v-if="loading" class="flex justify-center py-16">
                <ProgressSpinner />
            </div>

            <template v-else>
                <!-- ── Step 1: Division ─────────────────────────────────── -->
                <div class="step-card">
                    <div class="step-header">
                        <span class="step-badge">1</span>
                        <span class="step-label">Select Division</span>
                    </div>
                    <select
                        v-model="selectedDivisionId"
                        class="w-full bg-slate-800 border border-slate-600 rounded-lg px-4 py-3 text-slate-200 text-sm focus:outline-none focus:border-blue-500 transition-colors"
                    >
                        <option :value="null" disabled>— Select a division —</option>
                        <option v-for="div in store.divisions" :key="div.id" :value="div.id">
                            {{ div.name }} ({{ div.auditType === 'Facility' ? 'Facility' : 'Job Site' }})
                        </option>
                    </select>
                </div>

                <!-- Step connector + Template info (shown once division is selected) -->
                <template v-if="selectedDivision">
                    <div class="step-connector" />

                    <!-- Template readout (auto-resolved, not a user step) -->
                    <div v-if="loadingTemplate" class="step-card">
                        <div class="flex items-center gap-2 text-slate-500 text-sm">
                            <i class="pi pi-spin pi-spinner text-xs" /> Loading template…
                        </div>
                    </div>

                    <div v-else-if="activeTemplate" class="template-card mb-1">
                        <div class="flex items-center gap-3">
                            <i class="pi pi-file-edit text-blue-400 text-lg shrink-0" />
                            <div class="flex-1 min-w-0">
                                <p class="text-white font-semibold text-sm truncate">
                                    {{ selectedDivision.name }} — v{{ activeTemplate.versionNumber }}
                                </p>
                                <p class="text-slate-400 text-xs mt-0.5">
                                    {{ activeTemplate.sectionCount }} section{{ activeTemplate.sectionCount !== 1 ? 's' : '' }}
                                    · {{ activeTemplate.questionCount }} questions
                                </p>
                            </div>
                            <span class="text-[10px] bg-emerald-900/60 text-emerald-300 border border-emerald-700/40 px-2 py-0.5 rounded-full font-semibold shrink-0">Active</span>
                        </div>
                    </div>

                    <div v-else class="step-card border-amber-700/40">
                        <div class="flex items-start gap-2 text-amber-300 text-sm">
                            <i class="pi pi-exclamation-triangle mt-0.5 shrink-0" />
                            No active template for this division. Contact your Template Admin to publish a template first.
                        </div>
                    </div>

                    <!-- ── Step 2: Audit Type (only when multiple prefixes) ── -->
                    <template v-if="activeTemplate && jobPrefixes.length > 1">
                        <div class="step-connector" />
                        <div class="step-card">
                            <div class="step-header">
                                <span class="step-badge">2</span>
                                <span class="step-label">Audit Type</span>
                            </div>
                            <select
                                v-model="selectedJobPrefixId"
                                class="w-full bg-slate-800 border border-slate-600 rounded-lg px-4 py-3 text-slate-200 text-sm focus:outline-none focus:border-blue-500 transition-colors"
                            >
                                <option :value="null" disabled>— Select audit type —</option>
                                <option v-for="p in jobPrefixes" :key="p.id" :value="p.id">
                                    {{ p.prefix ? `${p.prefix} — ${p.label}` : p.label }}
                                </option>
                            </select>
                        </div>
                    </template>

                    <!-- ── Step: Site Code (JobSite only) ──────────────────── -->
                    <template v-if="activeTemplate && selectedDivision?.auditType === 'JobSite'">
                        <div class="step-connector" />
                        <div class="step-card">
                            <div class="step-header">
                                <span class="step-badge">{{ jobPrefixes.length > 1 ? 3 : 2 }}</span>
                                <span class="step-label">Site Code</span>
                                <span class="text-slate-500 text-xs font-normal ml-1">(optional · 3 chars)</span>
                            </div>
                            <div class="flex items-center gap-3">
                                <input
                                    v-model="siteCode"
                                    type="text"
                                    maxlength="3"
                                    placeholder="e.g. IPT"
                                    class="w-28 bg-slate-800 border border-slate-600 rounded-lg px-4 py-3 text-slate-200 text-sm font-mono uppercase focus:outline-none focus:border-blue-500 transition-colors"
                                    @input="siteCode = (($event.target as HTMLInputElement).value).toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 3)"
                                />
                                <span class="text-xs text-slate-500">3-character site identifier, e.g. IPT for INEOS Pasadena TX</span>
                            </div>
                        </div>
                    </template>

                    <!-- ── Step: Special Sections ──────────────────────────── -->
                    <template v-if="activeTemplate && optionalGroups.length > 0">
                        <div class="step-connector" />
                        <div class="step-card">
                            <div class="step-header mb-1">
                                <span class="step-badge">{{ lastStepNumber }}</span>
                                <span class="step-label">Special Sections</span>
                                <span class="text-slate-500 text-xs font-normal ml-1">(optional)</span>
                            </div>
                            <p class="text-xs text-slate-500 mb-3 pl-7">
                                Toggle on sections that apply to this audit. Sections left off won't appear on the form or affect scoring.
                            </p>
                            <div class="space-y-2">
                                <div
                                    v-for="group in optionalGroups"
                                    :key="group.key"
                                    class="flex items-start gap-3 rounded-lg border p-3.5 cursor-pointer transition-colors"
                                    :class="enabledGroupKeys.includes(group.key)
                                        ? 'bg-blue-600/15 border-blue-600/50'
                                        : 'bg-slate-800/60 border-slate-700 hover:border-slate-500'"
                                    @click="toggleGroup(group.key)"
                                >
                                    <div
                                        class="mt-0.5 shrink-0 w-9 h-5 rounded-full transition-colors flex items-center px-0.5"
                                        :class="enabledGroupKeys.includes(group.key) ? 'bg-blue-500 justify-end' : 'bg-slate-600 justify-start'"
                                    >
                                        <div class="w-4 h-4 rounded-full bg-white shadow-sm" />
                                    </div>
                                    <div>
                                        <p class="text-sm font-medium text-slate-200">{{ group.label }}</p>
                                        <p class="text-xs text-slate-400 mt-0.5">{{ group.sectionNames.join(' · ') }}</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </template>
                </template>

                <!-- ── Ready summary + primary action ─────────────────────── -->
                <div class="mt-6">
                    <!-- Summary banner when ready -->
                    <div
                        v-if="canStart"
                        class="rounded-xl border border-emerald-700/40 bg-emerald-900/15 px-4 py-3 flex items-center gap-3 mb-4"
                    >
                        <i class="pi pi-check-circle text-emerald-400 text-lg shrink-0" />
                        <div class="flex-1 min-w-0 text-sm text-slate-200">
                            <span class="font-semibold text-white">{{ selectedDivision?.name }}</span>
                            <span class="text-slate-400"> · </span>
                            <span>v{{ activeTemplate?.versionNumber }}</span>
                            <span class="text-slate-400"> · </span>
                            <span>{{ activeTemplate?.sectionCount }} sections, {{ activeTemplate?.questionCount }} questions</span>
                            <template v-if="enabledGroupKeys.length > 0">
                                <span class="text-slate-400"> · </span>
                                <span class="text-blue-300">{{ enabledGroupKeys.length }} special section{{ enabledGroupKeys.length !== 1 ? 's' : '' }}</span>
                            </template>
                        </div>
                    </div>

                    <!-- Action buttons -->
                    <div class="flex gap-3">
                        <Button
                            label="Cancel"
                            severity="secondary"
                            outlined
                            class="shrink-0"
                            @click="router.push('/audit-management/audits')"
                        />
                        <Button
                            label="Start Audit"
                            icon="pi pi-arrow-right"
                            icon-pos="right"
                            class="flex-1"
                            :disabled="!canStart"
                            :loading="creating"
                            @click="startAudit"
                        />
                    </div>
                </div>
            </template>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import ProgressSpinner from 'primevue/progressspinner';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import type { DivisionDto, DivisionJobPrefixDto } from '@/apiclient/auditClient';

const router = useRouter();
const store = useAuditStore();

const loading = ref(false);
const loadingTemplate = ref(false);
const creating = ref(false);

const selectedDivisionId = ref<number | null>(null);
const enabledGroupKeys = ref<string[]>([]);
const jobPrefixes = ref<DivisionJobPrefixDto[]>([]);
const selectedJobPrefixId = ref<number | null>(null);
const siteCode = ref<string>('');

interface TemplateCard {
    versionNumber: number;
    questionCount: number;
    sectionCount: number;
}

interface OptionalGroup {
    key: string;
    label: string;
    sectionNames: string[];
}

const activeTemplate = ref<TemplateCard | null>(null);
const optionalGroups = ref<OptionalGroup[]>([]);

const selectedDivision = computed((): DivisionDto | null =>
    store.divisions.find(d => d.id === selectedDivisionId.value) ?? null
);

const canStart = computed(() =>
    selectedDivision.value !== null && activeTemplate.value !== null && !creating.value && !loadingTemplate.value
);

// Dynamic step number for the last configurable step
const lastStepNumber = computed(() => {
    let n = 2;
    if (activeTemplate.value && jobPrefixes.value.length > 1) n++;
    if (activeTemplate.value && selectedDivision.value?.auditType === 'JobSite') n++;
    return n;
});

onMounted(async () => {
    store.resetForm();
    loading.value = true;
    try {
        await store.loadDivisions();
    } finally {
        loading.value = false;
    }
});

watch(selectedDivisionId, async (divId) => {
    activeTemplate.value = null;
    optionalGroups.value = [];
    enabledGroupKeys.value = [];
    jobPrefixes.value = [];
    selectedJobPrefixId.value = null;
    siteCode.value = '';
    if (!divId) return;

    loadingTemplate.value = true;
    try {
        const svc = useAuditService();
        const [prefixes, template] = await Promise.all([
            svc.getDivisionJobPrefixes(divId).catch(() => [] as DivisionJobPrefixDto[]),
            svc.getActiveTemplate(divId).catch(() => null),
        ]);
        jobPrefixes.value = prefixes;
        const defaultPrefix = prefixes.find(p => p.isDefault) ?? prefixes[0] ?? null;
        selectedJobPrefixId.value = defaultPrefix?.id ?? null;

        if (template) {
            activeTemplate.value = {
                versionNumber: template.versionNumber,
                questionCount: template.sections.reduce((sum, s) => sum + s.questions.length, 0),
                sectionCount: template.sections.length,
            };
            const groupMap = new Map<string, string[]>();
            for (const s of template.sections) {
                if (s.isOptional && s.optionalGroupKey) {
                    const existing = groupMap.get(s.optionalGroupKey) ?? [];
                    existing.push(s.name);
                    groupMap.set(s.optionalGroupKey, existing);
                }
            }
            optionalGroups.value = Array.from(groupMap.entries()).map(([key, names]) => ({
                key,
                label: key.replace(/_/g, ' '),
                sectionNames: names,
            }));
        }
    } finally {
        loadingTemplate.value = false;
    }
});

function toggleGroup(key: string) {
    const idx = enabledGroupKeys.value.indexOf(key);
    if (idx >= 0) {
        enabledGroupKeys.value.splice(idx, 1);
    } else {
        enabledGroupKeys.value.push(key);
    }
}

async function startAudit() {
    if (!selectedDivision.value || !canStart.value) return;
    creating.value = true;
    const sc = siteCode.value.trim().toUpperCase() || null;
    const id = await store.createAudit(selectedDivision.value.id, enabledGroupKeys.value, selectedJobPrefixId.value, sc);
    creating.value = false;
    if (id) {
        router.push(`/audit-management/audits/${id}`);
    }
}
</script>

<style scoped>
.step-card {
    background: rgba(30, 41, 59, 0.6);
    border: 1px solid rgba(71, 85, 105, 0.5);
    border-radius: 12px;
    padding: 1rem 1.25rem;
}

.step-connector {
    width: 2px;
    height: 20px;
    background: linear-gradient(to bottom, rgba(71,85,105,0.6), rgba(71,85,105,0.3));
    margin-left: 1.6rem;
}

.step-header {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin-bottom: 0.75rem;
}

.step-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 1.375rem;
    height: 1.375rem;
    border-radius: 9999px;
    background: #2563eb;
    color: #fff;
    font-size: 10px;
    font-weight: 700;
    flex-shrink: 0;
}

.step-label {
    font-size: 11px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #94a3b8;
}

.template-card {
    background: rgba(15, 23, 42, 0.6);
    border: 1px solid rgba(37, 99, 235, 0.25);
    border-radius: 10px;
    padding: 0.875rem 1.125rem;
}
</style>

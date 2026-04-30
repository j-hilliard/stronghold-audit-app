<template>
    <div>
        <BasePageHeader
            icon="pi pi-plus"
            title="New Audit"
            subtitle="Select a division to begin"
        />

        <div class="p-6 max-w-3xl">
            <div v-if="loading" class="flex justify-center py-16">
                <ProgressSpinner />
            </div>

            <template v-else>
                <!-- Step 1: Division -->
                <div class="mb-6">
                    <label class="block text-xs text-slate-400 font-medium uppercase tracking-wide mb-2">Division</label>
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

                <!-- Template info (read-only, shown once division is selected) -->
                <template v-if="selectedDivision">
                    <div v-if="loadingTemplate" class="mb-6 text-slate-500 text-sm">Loading template info…</div>

                    <div v-else-if="activeTemplate" class="mb-6 rounded-xl border border-slate-700 bg-slate-800/60 px-5 py-4 flex items-center justify-between">
                        <div>
                            <p class="text-white font-semibold text-sm">{{ selectedDivision.name }} — Version {{ activeTemplate.versionNumber }}</p>
                            <p class="text-slate-400 text-xs mt-0.5">
                                {{ activeTemplate.questionCount }} questions · {{ activeTemplate.sectionCount }} section{{ activeTemplate.sectionCount !== 1 ? 's' : '' }}
                            </p>
                        </div>
                        <span class="text-xs bg-green-800 text-green-300 px-2 py-0.5 rounded font-medium shrink-0">Active</span>
                    </div>

                    <div v-else class="mb-6 p-4 bg-amber-900/20 border border-amber-700/40 rounded-lg text-amber-300 text-sm">
                        No active template found for this division. Contact your Template Admin to publish a template first.
                    </div>

                    <!-- Job Prefix picker (only shown when division has multiple prefixes) -->
                    <template v-if="activeTemplate && jobPrefixes.length > 1">
                        <div class="mb-6">
                            <label class="block text-xs text-slate-400 font-medium uppercase tracking-wide mb-2">Audit Type</label>
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

                    <!-- Site Code (always shown for JobSite audits when a template is selected) -->
                    <template v-if="activeTemplate && selectedDivision?.auditType === 'JobSite'">
                        <div class="mb-6">
                            <label class="block text-xs text-slate-400 font-medium uppercase tracking-wide mb-2">
                                Site Code <span class="text-slate-500 font-normal normal-case">(3 chars · optional · e.g. IPT for INEOS Pasadena TX)</span>
                            </label>
                            <input
                                v-model="siteCode"
                                type="text"
                                maxlength="3"
                                placeholder="e.g. IPT"
                                class="w-40 bg-slate-800 border border-slate-600 rounded-lg px-4 py-3 text-slate-200 text-sm font-mono uppercase focus:outline-none focus:border-blue-500 transition-colors"
                                @input="siteCode = (($event.target as HTMLInputElement).value).toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 3)"
                            />
                        </div>
                    </template>

                    <!-- Optional section groups (only shown when optional groups exist) -->
                    <template v-if="activeTemplate && optionalGroups.length > 0">
                        <div class="mb-6">
                            <label class="block text-xs text-slate-400 font-medium uppercase tracking-wide mb-1">Special Sections</label>
                            <p class="text-xs text-slate-500 mb-3">
                                These sections only apply to certain job types. Toggle on any that apply to this audit.
                                Sections left off will not appear on the form or affect scoring.
                            </p>
                            <div class="space-y-2">
                                <div
                                    v-for="group in optionalGroups"
                                    :key="group.key"
                                    class="flex items-start gap-3 rounded-lg border p-4 cursor-pointer transition-colors"
                                    :class="enabledGroupKeys.includes(group.key)
                                        ? 'bg-blue-600/15 border-blue-600/50'
                                        : 'bg-slate-800 border-slate-700 hover:border-slate-500'"
                                    @click="toggleGroup(group.key)"
                                >
                                    <div
                                        class="mt-0.5 shrink-0 w-10 h-5 rounded-full transition-colors flex items-center px-0.5"
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

                <!-- Action row -->
                <div class="flex justify-end gap-3">
                    <Button
                        label="Cancel"
                        severity="secondary"
                        outlined
                        @click="router.push('/audit-management/audits')"
                    />
                    <Button
                        label="Start Audit"
                        icon="pi pi-arrow-right"
                        icon-pos="right"
                        :disabled="!canStart"
                        :loading="creating"
                        @click="startAudit"
                    />
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

// Ready to start as soon as a division with an active template is chosen
const canStart = computed(() =>
    selectedDivision.value !== null && activeTemplate.value !== null && !creating.value && !loadingTemplate.value
);

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

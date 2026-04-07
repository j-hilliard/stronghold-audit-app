<template>
    <div>
        <BasePageHeader
            icon="pi pi-plus"
            title="New Audit"
            subtitle="Choose a division and template to begin"
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

                <!-- Step 2: Template card (shows once division is selected) -->
                <template v-if="selectedDivision">
                    <label class="block text-xs text-slate-400 font-medium uppercase tracking-wide mb-2">Template</label>

                    <div v-if="loadingTemplate" class="text-slate-500 text-sm py-4">Loading template info…</div>

                    <div v-else-if="activeTemplate" class="mb-8">
                        <button
                            type="button"
                            @click="templateSelected = true"
                            :class="[
                                'w-full text-left rounded-xl border-2 p-5 transition-all duration-150 focus:outline-none',
                                templateSelected
                                    ? 'bg-blue-600/20 border-blue-500 shadow-lg shadow-blue-900/30'
                                    : 'bg-slate-800 border-slate-700 hover:border-slate-500',
                            ]"
                            :disabled="creating"
                        >
                            <div class="flex items-start justify-between">
                                <div>
                                    <p class="text-white font-semibold text-base">
                                        {{ selectedDivision.name }} — Version {{ activeTemplate.versionNumber }}
                                    </p>
                                    <p class="text-slate-400 text-sm mt-1">
                                        {{ activeTemplate.questionCount }} questions across {{ activeTemplate.sectionCount }} section{{ activeTemplate.sectionCount !== 1 ? 's' : '' }}
                                    </p>
                                </div>
                                <div class="flex items-center gap-2 shrink-0">
                                    <span class="text-xs bg-green-800 text-green-300 px-2 py-0.5 rounded font-medium">Active</span>
                                    <div
                                        v-if="templateSelected"
                                        class="w-5 h-5 rounded-full bg-blue-500 flex items-center justify-center"
                                    >
                                        <i class="pi pi-check text-white text-[10px]" />
                                    </div>
                                </div>
                            </div>
                        </button>
                    </div>

                    <div v-else class="mb-8 p-4 bg-amber-900/20 border border-amber-700/40 rounded-lg text-amber-300 text-sm">
                        No active template found for this division. Contact your Template Admin to publish a template first.
                    </div>
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
import { useAdminStore } from '@/modules/audit-management/stores/adminStore';
import type { DivisionDto } from '@/apiclient/auditClient';

const router = useRouter();
const store = useAuditStore();
const adminStore = useAdminStore();

const loading = ref(false);
const loadingTemplate = ref(false);
const creating = ref(false);

const selectedDivisionId = ref<number | null>(null);
const templateSelected = ref(false);

interface TemplateCard {
    versionNumber: number;
    questionCount: number;
    sectionCount: number;
}

const activeTemplate = ref<TemplateCard | null>(null);

const selectedDivision = computed((): DivisionDto | null =>
    store.divisions.find(d => d.id === selectedDivisionId.value) ?? null
);

const canStart = computed(() =>
    selectedDivision.value !== null && templateSelected.value && !creating.value
);

onMounted(async () => {
    store.resetForm();
    loading.value = true;
    try {
        await Promise.all([
            store.loadDivisions(),
            adminStore.loadTemplates(),
        ]);
    } finally {
        loading.value = false;
    }
});

watch(selectedDivisionId, async (divId) => {
    templateSelected.value = false;
    activeTemplate.value = null;
    if (!divId) return;

    const div = store.divisions.find(d => d.id === divId);
    if (!div) return;

    loadingTemplate.value = true;
    try {
        // Find active template version for this division from adminStore
        const versions = adminStore.templates.filter(
            t => t.divisionCode === div.code && t.status === 'Active'
        );
        if (versions.length > 0) {
            const v = versions[0];
            // Load draft detail to get section count
            await adminStore.loadDraft(v.id);
            const detail = adminStore.draftDetail;
            activeTemplate.value = {
                versionNumber: v.versionNumber,
                questionCount: v.questionCount,
                sectionCount: detail?.sections.length ?? 0,
            };
        }
    } finally {
        loadingTemplate.value = false;
    }
});

async function startAudit() {
    if (!selectedDivision.value || !canStart.value) return;
    creating.value = true;
    const id = await store.createAudit(selectedDivision.value.id);
    creating.value = false;
    if (id) {
        router.push(`/audit-management/audits/${id}`);
    }
}
</script>

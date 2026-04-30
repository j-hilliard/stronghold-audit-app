<template>
    <div class="min-h-screen bg-slate-950 flex items-center justify-center p-4">
        <div class="w-full max-w-lg">

            <!-- Header -->
            <div class="text-center mb-8">
                <div class="w-14 h-14 rounded-2xl bg-blue-500/15 border border-blue-500/30 flex items-center justify-center mx-auto mb-4">
                    <i class="pi pi-clipboard text-blue-400 text-2xl" />
                </div>
                <h1 class="text-2xl font-bold text-white">Corrective Action Update</h1>
                <p class="text-slate-400 text-sm mt-1">Stronghold Audit System — Secure External Access</p>
            </div>

            <!-- Loading -->
            <div v-if="loading" class="bg-slate-800 border border-slate-700 rounded-2xl p-8 text-center">
                <i class="pi pi-spin pi-spinner text-blue-400 text-3xl mb-3" />
                <p class="text-slate-400">Loading corrective action...</p>
            </div>

            <!-- Access Error -->
            <div v-else-if="accessError" class="bg-slate-800 border border-red-800/50 rounded-2xl p-8 text-center">
                <div class="w-12 h-12 rounded-xl bg-red-500/15 flex items-center justify-center mx-auto mb-4">
                    <i class="pi pi-lock text-red-400 text-xl" />
                </div>
                <h2 class="text-lg font-semibold text-white mb-2">Access Unavailable</h2>
                <p class="text-slate-400 text-sm">{{ accessError }}</p>
            </div>

            <!-- CA Card -->
            <div v-else-if="ca" class="space-y-4">

                <!-- Info card -->
                <div class="bg-slate-800 border border-slate-700 rounded-2xl p-6 space-y-4">
                    <div v-if="ca.auditTitle" class="text-xs text-slate-500 uppercase tracking-wide font-semibold">
                        {{ ca.auditTitle }}
                    </div>

                    <div>
                        <div class="text-xs text-slate-500 mb-1">Description</div>
                        <p class="text-slate-200 text-sm leading-relaxed whitespace-pre-wrap">{{ ca.description }}</p>
                    </div>

                    <div v-if="ca.rootCause">
                        <div class="text-xs text-slate-500 mb-1">Root Cause</div>
                        <p class="text-slate-300 text-sm">{{ ca.rootCause }}</p>
                    </div>

                    <div class="grid grid-cols-3 gap-3">
                        <div class="bg-slate-900/60 rounded-xl p-3">
                            <div class="text-xs text-slate-500 mb-1">Status</div>
                            <span
                                class="text-xs font-semibold"
                                :class="statusColor(ca.status)"
                            >{{ ca.status }}</span>
                        </div>
                        <div class="bg-slate-900/60 rounded-xl p-3">
                            <div class="text-xs text-slate-500 mb-1">Priority</div>
                            <span class="text-xs font-semibold" :class="ca.priority === 'Urgent' ? 'text-red-400' : 'text-slate-300'">
                                {{ ca.priority }}
                            </span>
                        </div>
                        <div class="bg-slate-900/60 rounded-xl p-3">
                            <div class="text-xs text-slate-500 mb-1">Due Date</div>
                            <span class="text-xs text-slate-300">{{ ca.dueDate ?? '—' }}</span>
                        </div>
                    </div>

                    <div v-if="ca.assignedTo" class="text-xs text-slate-500">
                        Assigned to: <span class="text-slate-300">{{ ca.assignedTo }}</span>
                    </div>

                    <div v-if="ca.expiresAt" class="text-xs text-amber-400/80">
                        Link expires: {{ formatDate(ca.expiresAt) }}
                    </div>
                </div>

                <!-- Already InProgress notice -->
                <div
                    v-if="ca.status === 'InProgress'"
                    class="bg-blue-500/10 border border-blue-500/30 rounded-xl p-4 text-sm text-blue-300 text-center"
                >
                    This corrective action is already marked as In Progress.
                    Contact your auditor to close it once work is complete.
                </div>

                <!-- Update Form -->
                <div v-else class="bg-slate-800 border border-slate-700 rounded-2xl p-6 space-y-4">
                    <h2 class="text-base font-semibold text-white">Confirm Work In Progress</h2>

                    <div>
                        <label class="block text-xs text-slate-400 mb-1">Your Name <span class="text-slate-600">(optional)</span></label>
                        <InputText
                            v-model="updaterName"
                            placeholder="Enter your name"
                            size="small"
                            class="w-full"
                        />
                    </div>

                    <div>
                        <label class="block text-xs text-slate-400 mb-1">Notes <span class="text-slate-600">(optional)</span></label>
                        <Textarea
                            v-model="notes"
                            placeholder="Describe what work has been started or any updates..."
                            :rows="3"
                            class="w-full text-sm"
                            auto-resize
                        />
                    </div>

                    <Button
                        label="Mark as In Progress"
                        icon="pi pi-check"
                        class="w-full"
                        :loading="saving"
                        @click="submit"
                    />

                    <p class="text-xs text-slate-500 text-center">
                        Marking this as In Progress notifies the auditor that work has begun.
                        Only an auditor can officially close this action.
                    </p>
                </div>

                <!-- Success -->
                <div
                    v-if="submitted"
                    class="bg-emerald-500/10 border border-emerald-500/30 rounded-xl p-4 text-sm text-emerald-300 text-center"
                >
                    Update submitted. The audit team has been notified.
                </div>

            </div>

        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import type { CaPublicAccessDto } from '@/apiclient/auditClient';
import { PublicCaClient } from '@/apiclient/publicClient';

const route  = useRoute();
const token  = route.params.token as string;
const client = new PublicCaClient();

const loading     = ref(true);
const saving      = ref(false);
const submitted   = ref(false);
const accessError = ref<string | null>(null);
const ca          = ref<CaPublicAccessDto | null>(null);

const updaterName = ref('');
const notes       = ref('');

async function loadCa() {
    loading.value = true;
    try {
        ca.value = await client.getCaByToken(token);
    } catch (err: any) {
        const msg = err?.response?.data?.detail ?? err?.message ?? 'Unknown error';
        accessError.value = msg;
    } finally {
        loading.value = false;
    }
}

async function submit() {
    saving.value = true;
    try {
        await client.updateCaByToken(token, {
            newStatus:     'InProgress',
            notes:         notes.value || null,
            updatedByName: updaterName.value || null,
        });
        submitted.value = true;
        if (ca.value) ca.value.status = 'InProgress';
    } catch (err: any) {
        const msg = err?.response?.data?.detail ?? err?.message ?? 'Failed to submit update.';
        accessError.value = msg;
    } finally {
        saving.value = false;
    }
}

function statusColor(status: string): string {
    if (status === 'Open')       return 'text-blue-400';
    if (status === 'InProgress') return 'text-amber-400';
    if (status === 'Overdue')    return 'text-red-400';
    if (status === 'Closed')     return 'text-emerald-400';
    return 'text-slate-400';
}

function formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

onMounted(loadCa);
</script>

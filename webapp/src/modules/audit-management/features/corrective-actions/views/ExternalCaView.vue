<template>
    <div class="min-h-screen bg-slate-950 py-8 px-4">
        <div class="w-full max-w-lg mx-auto">

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
                            <span class="text-xs font-semibold" :class="statusColor(ca.status)">{{ statusLabel(ca.status) }}</span>
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

                <!-- ── State: Closed or Voided ── -->
                <div
                    v-if="ca.status === 'Closed' || ca.status === 'Voided'"
                    class="bg-emerald-500/10 border border-emerald-500/30 rounded-xl p-4 text-sm text-emerald-300 text-center"
                >
                    <i class="pi pi-check-circle text-emerald-400 text-xl mb-2 block" />
                    This corrective action has been {{ ca.status === 'Voided' ? 'voided' : 'closed' }}.
                    No further action is required.
                </div>

                <!-- ── State: Submitted (pending admin review) ── -->
                <div
                    v-else-if="ca.status === 'Submitted'"
                    class="bg-amber-500/10 border border-amber-500/30 rounded-xl p-4 text-sm text-amber-300 text-center"
                >
                    <i class="pi pi-clock text-amber-400 text-xl mb-2 block" />
                    Work submitted — pending admin review.
                    The audit team has been notified and will approve or follow up.
                </div>

                <!-- ── State: InProgress — mark as submitted ── -->
                <div v-else-if="ca.status === 'InProgress' && !submitted" class="bg-slate-800 border border-slate-700 rounded-2xl p-6 space-y-4">
                    <h2 class="text-base font-semibold text-white">Submit Completed Work</h2>
                    <p class="text-xs text-slate-400">
                        Mark this corrective action as complete. The audit team will review and officially close it.
                    </p>

                    <div>
                        <label class="block text-xs text-slate-400 mb-1">Your Name <span class="text-slate-600">(optional)</span></label>
                        <InputText v-model="updaterName" placeholder="Enter your name" size="small" class="w-full" />
                    </div>

                    <div>
                        <label class="block text-xs text-slate-400 mb-1">Completion Notes <span class="text-slate-600">(optional)</span></label>
                        <Textarea
                            v-model="notes"
                            placeholder="Describe what was done to resolve this corrective action..."
                            :rows="3"
                            class="w-full text-sm"
                            auto-resize
                        />
                    </div>

                    <!-- Photo Upload -->
                    <div>
                        <label class="block text-xs text-slate-400 mb-1">Proof Photo <span class="text-red-400">*</span></label>
                        <div v-if="photoPreviewUrl" class="mb-2 relative">
                            <img :src="photoPreviewUrl" alt="Preview" class="rounded-lg max-h-48 w-full object-cover" />
                            <button
                                class="absolute top-2 right-2 bg-slate-900/80 hover:bg-red-900/80 text-slate-300 hover:text-white rounded-full w-6 h-6 flex items-center justify-center transition-colors"
                                title="Remove photo"
                                @click="clearPhoto"
                            >
                                <i class="pi pi-times text-[10px]" />
                            </button>
                            <p class="text-xs text-slate-500 mt-1">{{ photoFile?.name }}</p>
                        </div>
                        <label v-else class="flex items-center gap-2 px-3 py-2 bg-slate-700/40 border border-slate-600/50 border-dashed rounded-lg cursor-pointer hover:bg-slate-700/60 transition-colors">
                            <i class="pi pi-camera text-slate-400 text-sm" />
                            <span class="text-sm text-slate-400">Attach a photo</span>
                            <input type="file" accept="image/*" class="hidden" @change="onPhotoSelected" />
                        </label>
                    </div>

                    <p v-if="!photoFile" class="text-xs text-amber-400 flex items-center gap-1.5">
                        <i class="pi pi-exclamation-triangle text-[11px]" />
                        A proof photo is required before submitting.
                    </p>

                    <Button
                        label="Submit Work Complete"
                        icon="pi pi-check"
                        class="w-full"
                        :loading="saving"
                        :disabled="!photoFile"
                        @click="submitWork"
                    />

                    <p class="text-xs text-slate-500 text-center">
                        Submitting notifies the audit team to review and close this action.
                    </p>
                </div>

                <!-- ── State: Open / Overdue — confirm work in progress ── -->
                <div v-else-if="(ca.status === 'Open' || ca.status === 'Overdue') && !submitted" class="bg-slate-800 border border-slate-700 rounded-2xl p-6 space-y-4">
                    <h2 class="text-base font-semibold text-white">Confirm Work In Progress</h2>

                    <div>
                        <label class="block text-xs text-slate-400 mb-1">Your Name <span class="text-slate-600">(optional)</span></label>
                        <InputText v-model="updaterName" placeholder="Enter your name" size="small" class="w-full" />
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
                        @click="markInProgress"
                    />

                    <p class="text-xs text-slate-500 text-center">
                        Marking as In Progress notifies the auditor that work has begun.
                    </p>
                </div>

                <!-- Success -->
                <div
                    v-if="submitted"
                    class="bg-emerald-500/10 border border-emerald-500/30 rounded-xl p-4 text-sm text-emerald-300 text-center"
                >
                    <i class="pi pi-check-circle text-emerald-400 text-xl mb-2 block" />
                    {{ submittedMessage }}
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

const updaterName     = ref('');
const notes           = ref('');
const photoFile       = ref<File | null>(null);
const photoPreviewUrl = ref<string | null>(null);
const submittedMessage = ref('Update submitted. The audit team has been notified.');

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

async function markInProgress() {
    saving.value = true;
    try {
        await client.updateCaByToken(token, {
            newStatus:     'InProgress',
            notes:         notes.value || null,
            updatedByName: updaterName.value || null,
        });
        submitted.value = true;
        submittedMessage.value = 'Marked as In Progress. The audit team has been notified.';
        if (ca.value) ca.value.status = 'InProgress';
    } catch (err: any) {
        accessError.value = err?.response?.data?.detail ?? err?.message ?? 'Failed to submit update.';
    } finally {
        saving.value = false;
    }
}

async function submitWork() {
    if (!photoFile.value) return;
    saving.value = true;
    try {
        // Upload photo first — backend requires a photo record to exist before allowing Submitted status
        await client.uploadCaPhoto(token, photoFile.value);
        await client.updateCaByToken(token, {
            newStatus:     'Submitted',
            notes:         notes.value || null,
            updatedByName: updaterName.value || null,
        });
        submitted.value = true;
        submittedMessage.value = 'Work submitted successfully. The audit team will review and close this action.';
        if (ca.value) ca.value.status = 'Submitted';
    } catch (err: any) {
        accessError.value = err?.response?.data?.detail ?? err?.message ?? 'Failed to submit update.';
    } finally {
        saving.value = false;
    }
}

function onPhotoSelected(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;
    photoFile.value = file;
    photoPreviewUrl.value = URL.createObjectURL(file);
}

function clearPhoto() {
    if (photoPreviewUrl.value) URL.revokeObjectURL(photoPreviewUrl.value);
    photoFile.value = null;
    photoPreviewUrl.value = null;
}

function statusColor(status: string): string {
    if (status === 'Open')       return 'text-blue-400';
    if (status === 'InProgress') return 'text-amber-400';
    if (status === 'Submitted')  return 'text-amber-300';
    if (status === 'Overdue')    return 'text-red-400';
    if (status === 'Closed')     return 'text-emerald-400';
    return 'text-slate-400';
}

function statusLabel(status: string): string {
    if (status === 'InProgress') return 'In Progress';
    return status;
}

function formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

onMounted(loadCa);
</script>

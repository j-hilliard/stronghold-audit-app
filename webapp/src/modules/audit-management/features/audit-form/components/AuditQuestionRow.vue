<template>
    <div
        :class="[
            'question-row border-b border-slate-700 last:border-b-0',
            response.status === 'NonConforming' ? 'bg-red-950/40' : '',
            response.status === 'Warning' ? 'bg-amber-950/30' : '',
        ]"
    >
        <!-- Question: badge + text + repeat badge + camera icon -->
        <div class="flex items-start gap-3 px-4 pt-3 pb-2">
            <!-- Circular numbered badge -->
            <div
                :class="[
                    'shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold border-2 transition-colors duration-200',
                    badgeClass,
                ]"
            >
                {{ displayOrder }}
            </div>

            <!-- Question text -->
            <p class="flex-1 text-sm text-slate-100 leading-snug pt-1">{{ response.questionTextSnapshot }}</p>

            <!-- Repeat finding badge -->
            <span
                v-if="isRepeatFinding"
                class="shrink-0 text-[10px] font-semibold bg-amber-900/60 border border-amber-700/60 text-amber-400 rounded px-1.5 py-0.5 self-start mt-1 whitespace-nowrap"
                title="NonConforming in 2+ recent audits for this division"
            >
                &#9888; Repeat
            </span>

            <!-- Camera icon button + hidden file input -->
            <div class="relative shrink-0 self-start mt-0.5">
                <button
                    type="button"
                    class="w-6 h-6 flex items-center justify-center rounded hover:bg-slate-700 transition-colors"
                    :class="uploading ? 'opacity-50 cursor-not-allowed' : ''"
                    title="Attach photo"
                    :disabled="uploading"
                    @click="onCameraClick"
                >
                    <i
                        :class="[
                            'text-xs',
                            uploading ? 'pi pi-spin pi-spinner text-slate-400' : 'pi pi-camera text-slate-400 hover:text-slate-200',
                        ]"
                    />
                </button>
                <!-- Photo count badge -->
                <span
                    v-if="photos.length > 0"
                    class="absolute -top-1 -right-1 text-[9px] bg-sky-600 text-white rounded-full min-w-[14px] h-[14px] flex items-center justify-center px-0.5 leading-none pointer-events-none"
                >
                    {{ photos.length }}
                </span>
                <input
                    ref="cameraInput"
                    type="file"
                    class="hidden"
                    accept="image/*"
                    @change="onFileChange"
                />
            </div>
        </div>

        <!-- Status buttons row -->
        <div class="px-4 pb-3">
            <StatusButtons
                :model-value="response.status"
                :allow-n-a="response.allowNA"
                :disabled="disabled"
                @update:model-value="onStatusChange"
            />
        </div>

        <!-- Photo strip (always visible when photos exist) -->
        <div v-if="photos.length > 0 || uploading" class="px-4 pb-2 border-t border-slate-700/30 pt-2">
            <div class="flex items-center gap-2 flex-wrap">
                <span class="text-[10px] font-semibold text-slate-400 uppercase tracking-wide">Photos</span>
                <div class="flex gap-1.5 flex-wrap">
                    <div
                        v-for="photo in photos"
                        :key="photo.id"
                        class="relative"
                    >
                        <!-- Loaded thumbnail -->
                        <img
                            v-if="blobUrls[photo.id]"
                            :src="blobUrls[photo.id]"
                            :alt="photo.fileName"
                            class="w-12 h-12 rounded object-cover border border-slate-600 cursor-pointer hover:border-slate-400 transition-colors"
                            :title="photo.fileName"
                            @click="openPreview(photo)"
                        />
                        <!-- Loading placeholder -->
                        <div
                            v-else
                            class="w-12 h-12 rounded bg-slate-700 border border-slate-600 flex items-center justify-center"
                        >
                            <i class="pi pi-spin pi-spinner text-slate-500 text-xs" />
                        </div>
                        <!-- Delete button -->
                        <button
                            v-if="!disabled"
                            type="button"
                            class="absolute -top-1 -right-1 w-4 h-4 rounded-full bg-red-700 hover:bg-red-500 flex items-center justify-center border border-red-600 transition-colors"
                            title="Delete photo"
                            @click.stop="deletePhoto(photo)"
                        >
                            <i class="pi pi-times text-white" style="font-size: 7px" />
                        </button>
                    </div>
                    <!-- Upload in-progress indicator -->
                    <div
                        v-if="uploading"
                        class="w-12 h-12 rounded bg-slate-700 border border-dashed border-slate-500 flex items-center justify-center"
                    >
                        <i class="pi pi-spin pi-spinner text-slate-400 text-xs" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Photo preview overlay -->
        <Teleport to="body">
            <div
                v-if="previewPhoto"
                class="fixed inset-0 z-50 flex items-center justify-center bg-black/80"
                @click="previewPhoto = null"
            >
                <div class="relative max-w-[90vw] max-h-[90vh]" @click.stop>
                    <img
                        v-if="blobUrls[previewPhoto.id]"
                        :src="blobUrls[previewPhoto.id]"
                        :alt="previewPhoto.fileName"
                        class="max-w-full max-h-[85vh] rounded-lg shadow-2xl object-contain"
                    />
                    <p class="text-center text-xs text-slate-300 mt-2">{{ previewPhoto.fileName }}</p>
                    <button
                        class="absolute top-2 right-2 w-8 h-8 rounded-full bg-black/60 hover:bg-black/80 flex items-center justify-center text-white transition-colors"
                        @click="previewPhoto = null"
                    >
                        <i class="pi pi-times text-sm" />
                    </button>
                </div>
            </div>
        </Teleport>

        <!-- Conforming: optional note (collapsed until clicked or note already exists) -->
        <Transition name="nc-expand">
            <div v-if="response.status === 'Conforming'" class="px-4 pb-3 border-t border-slate-700/50">
                <button
                    v-if="!showConformingNote"
                    type="button"
                    class="mt-2 text-xs text-slate-500 hover:text-slate-300 transition-colors flex items-center gap-1"
                    :disabled="disabled"
                    @click="showConformingNote = true"
                >
                    <i class="pi pi-plus-circle text-[10px]" /> Add note
                </button>
                <Transition name="nc-expand">
                    <div v-if="showConformingNote" class="pt-2">
                        <label class="text-xs font-semibold block mb-1 text-slate-400">Notes</label>
                        <Textarea
                            :model-value="response.comment ?? ''"
                            rows="2"
                            class="w-full text-sm"
                            :disabled="disabled"
                            placeholder="Optional notes on this conforming item..."
                            autoResize
                            @update:model-value="store.setComment(response.questionId, $event as string || null)"
                        />
                    </div>
                </Transition>
            </div>
        </Transition>

        <!-- Expanded area: NC and Warning -->
        <Transition name="nc-expand">
            <div
                v-if="response.status === 'NonConforming' || response.status === 'Warning'"
                :class="[
                    'px-4 pb-3 space-y-2 border-t',
                    response.status === 'NonConforming' ? 'border-red-800/60' : 'border-amber-800/60',
                ]"
            >
                <!-- Comment -->
                <div class="pt-2">
                    <label
                        :class="[
                            'text-xs font-semibold block mb-1',
                            response.status === 'NonConforming' ? 'text-red-400' : 'text-amber-400',
                        ]"
                    >
                        {{ commentLabel }}
                        <span v-if="response.status === 'NonConforming' && response.requireCommentOnNC" class="ml-1">*</span>
                    </label>
                    <Textarea
                        :model-value="response.comment ?? ''"
                        rows="2"
                        class="w-full text-sm"
                        :disabled="disabled"
                        :placeholder="commentPlaceholder"
                        autoResize
                        @update:model-value="store.setComment(response.questionId, $event as string || null)"
                    />
                </div>

                <!-- Photo required badge — only shown when NC + requirePhotoOnNc + no photos -->
                <div
                    v-if="response.status === 'NonConforming' && response.requirePhotoOnNc && photos.length === 0 && !uploading"
                    class="flex items-center gap-2 px-3 py-1.5 rounded bg-amber-900/50 border border-amber-700/60 text-amber-300 text-xs"
                >
                    <i class="pi pi-camera text-xs" />
                    <span>A photo is required for this non-conformance before submitting.</span>
                </div>

                <!-- Auto-CA indicator — shown when NC + autoCreateCa flag -->
                <div
                    v-if="response.status === 'NonConforming' && response.autoCreateCa"
                    class="flex items-center gap-2 px-3 py-1.5 rounded bg-blue-900/40 border border-blue-700/50 text-blue-300 text-xs"
                >
                    <i class="pi pi-ticket text-xs" />
                    <span>A corrective action will be automatically created when this audit is submitted.</span>
                </div>

                <!-- Corrected on site — only for NonConforming -->
                <div v-if="response.status === 'NonConforming'" class="flex items-center gap-3">
                    <span class="text-xs text-slate-400">Corrected on-site?</span>
                    <div class="flex gap-1">
                        <button
                            type="button"
                            :class="[
                                'px-3 py-1 rounded text-xs font-semibold border-2 transition-colors',
                                response.correctedOnSite
                                    ? 'bg-emerald-600 border-emerald-400 text-white'
                                    : 'bg-emerald-900/20 border-emerald-600 text-emerald-400 hover:bg-emerald-600/30 hover:text-emerald-200',
                            ]"
                            :disabled="disabled"
                            @click="store.setCorrectedOnSite(response.questionId, true)"
                        >
                            Yes
                        </button>
                        <button
                            type="button"
                            :class="[
                                'px-3 py-1 rounded text-xs font-semibold border-2 transition-colors',
                                !response.correctedOnSite
                                    ? 'bg-slate-500 border-slate-400 text-white'
                                    : 'bg-slate-800/40 border-slate-500 text-slate-400 hover:bg-slate-500/30 hover:border-slate-300 hover:text-slate-200',
                            ]"
                            :disabled="disabled"
                            @click="store.setCorrectedOnSite(response.questionId, false)"
                        >
                            Not Corrected
                        </button>
                    </div>
                </div>
            </div>
        </Transition>
    </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, onBeforeUnmount } from 'vue';
import Textarea from 'primevue/textarea';
import { useToast } from 'primevue/usetoast';
import StatusButtons from './StatusButtons.vue';
import { useAuditStore, type ResponseState } from '@/modules/audit-management/stores/auditStore';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type FindingPhotoDto } from '@/apiclient/auditClient';

const props = defineProps<{
    response: ResponseState;
    displayOrder: number;
    disabled?: boolean;
    isRepeatFinding?: boolean;
}>();

const store = useAuditStore();
const apiStore = useApiStore();
const toast = useToast();

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}

// ── Photo state ────────────────────────────────────────────────────────────────

const cameraInput = ref<HTMLInputElement | null>(null);
const photos = ref<FindingPhotoDto[]>([]);
const blobUrls = ref<Record<number, string>>({});
const uploading = ref(false);
const photosLoaded = ref(false);
const previewPhoto = ref<FindingPhotoDto | null>(null);

async function loadPhotos() {
    if (photosLoaded.value || !store.auditId) return;
    photosLoaded.value = true;
    try {
        photos.value = await getClient().getFindingPhotos(store.auditId, props.response.questionId);
        for (const photo of photos.value) {
            loadBlobUrl(photo);
        }
    } catch {
        // Non-fatal — photo list is supplementary
    }
}

function loadBlobUrl(photo: FindingPhotoDto) {
    if (photo.id in blobUrls.value || !store.auditId) return;
    getClient()
        .downloadFindingPhoto(store.auditId, photo.questionId, photo.id)
        .then(blob => {
            blobUrls.value = { ...blobUrls.value, [photo.id]: URL.createObjectURL(blob) };
        })
        .catch(() => { /* non-fatal */ });
}

function onCameraClick() {
    // Open file dialog immediately so the browser doesn't block it
    cameraInput.value?.click();
    // Load any existing photos in background (for count badge + strip)
    if (!photosLoaded.value) {
        loadPhotos();
    }
}

async function onFileChange(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !store.auditId) return;
    input.value = '';

    uploading.value = true;
    try {
        // Ensure existing photos are loaded before adding the new one
        if (!photosLoaded.value) {
            photosLoaded.value = true;
            try {
                photos.value = await getClient().getFindingPhotos(store.auditId, props.response.questionId);
                for (const photo of photos.value) {
                    loadBlobUrl(photo);
                }
            } catch { /* non-fatal */ }
        }

        const dto = await getClient().uploadFindingPhoto(store.auditId, props.response.questionId, file);
        photos.value.push(dto);
        loadBlobUrl(dto);
        toast.add({ severity: 'success', summary: 'Photo attached', life: 2000 });
    } catch {
        toast.add({ severity: 'error', summary: 'Upload failed', detail: 'Could not attach photo.', life: 4000 });
    } finally {
        uploading.value = false;
    }
}

async function deletePhoto(photo: FindingPhotoDto) {
    if (!store.auditId) return;
    try {
        await getClient().deleteFindingPhoto(store.auditId, photo.questionId, photo.id);
        const url = blobUrls.value[photo.id];
        if (url) {
            URL.revokeObjectURL(url);
            const next = { ...blobUrls.value };
            delete next[photo.id];
            blobUrls.value = next;
        }
        photos.value = photos.value.filter(p => p.id !== photo.id);
        if (previewPhoto.value?.id === photo.id) previewPhoto.value = null;
        toast.add({ severity: 'info', summary: 'Photo removed', life: 2000 });
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Could not delete photo.', life: 4000 });
    }
}

function openPreview(photo: FindingPhotoDto) {
    previewPhoto.value = photo;
}

// Load photos when NC/Warning area opens
watch(() => props.response.status, (status) => {
    if ((status === 'NonConforming' || status === 'Warning') && !photosLoaded.value) {
        loadPhotos();
    }
}, { immediate: true });

onBeforeUnmount(() => {
    for (const url of Object.values(blobUrls.value)) {
        URL.revokeObjectURL(url);
    }
});

// ── Conforming note ────────────────────────────────────────────────────────────

// Show conforming note box if there's already a comment (e.g. loaded from server)
const showConformingNote = ref(!!props.response.comment);

// If a comment is cleared/restored from outside, keep the panel in sync
watch(() => props.response.comment, (val) => {
    if (val) showConformingNote.value = true;
});

// Collapse the note panel when status changes away from Conforming
watch(() => props.response.status, (val) => {
    if (val !== 'Conforming') showConformingNote.value = false;
});

// ── Computed ───────────────────────────────────────────────────────────────────

const badgeClass = computed(() => {
    switch (props.response.status) {
        case 'Conforming':    return 'bg-emerald-700 border-emerald-400 text-white';
        case 'NonConforming': return 'bg-red-700 border-red-400 text-white';
        case 'Warning':       return 'bg-amber-600 border-amber-400 text-white';
        case 'NA':            return 'bg-slate-500 border-slate-400 text-slate-100';
        default:              return 'bg-slate-700 border-slate-500 text-slate-400';
    }
});

const commentLabel = computed(() => {
    if (props.response.status === 'NonConforming') return 'Comments / Corrective Action';
    return 'Comments';
});

const commentPlaceholder = computed(() => {
    if (props.response.status === 'NonConforming') return 'Describe the non-conformance and corrective action taken...';
    return 'Describe the observed condition...';
});

function onStatusChange(newStatus: string | null) {
    store.setResponse(props.response.questionId, newStatus);
}
</script>

<style scoped>
.nc-expand-enter-active,
.nc-expand-leave-active {
    transition: all 0.2s ease;
    overflow: hidden;
}
.nc-expand-enter-from,
.nc-expand-leave-to {
    opacity: 0;
    max-height: 0;
}
.nc-expand-enter-to,
.nc-expand-leave-from {
    opacity: 1;
    max-height: 300px;
}

.question-row {
    transition: background-color 0.15s ease, box-shadow 0.15s ease;
}
.question-row:hover {
    background-color: rgba(99, 179, 237, 0.06);
    box-shadow: inset 3px 0 0 rgba(99, 179, 237, 0.55);
}
/* Keep the red/amber tint on hover for NC/Warning rows, just brighten it */
.question-row.bg-red-950\/40:hover {
    background-color: rgba(127, 29, 29, 0.35) !important;
    box-shadow: inset 3px 0 0 rgba(239, 68, 68, 0.5);
}
.question-row.bg-amber-950\/30:hover {
    background-color: rgba(120, 53, 15, 0.3) !important;
    box-shadow: inset 3px 0 0 rgba(251, 191, 36, 0.5);
}
</style>

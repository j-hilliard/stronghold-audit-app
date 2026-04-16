<template>
    <div class="mt-4">
        <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-semibold text-slate-300 uppercase tracking-wide">
                <i class="pi pi-paperclip mr-1.5 text-slate-400" />
                Attachments
                <span v-if="attachments.length" class="ml-1 text-slate-500 font-normal normal-case tracking-normal">({{ attachments.length }})</span>
            </h3>
        </div>

        <!-- Drop zone (edit mode only) -->
        <div
            v-if="!readonly"
            class="relative border-2 border-dashed rounded-lg px-4 py-5 text-center transition-colors cursor-pointer"
            :class="isDragging
                ? 'border-blue-500 bg-blue-950/20'
                : 'border-slate-600 hover:border-slate-500 bg-slate-800/40 hover:bg-slate-800/60'"
            @dragover.prevent="isDragging = true"
            @dragleave.prevent="isDragging = false"
            @drop.prevent="onDrop"
            @click="fileInput?.click()"
        >
            <input
                ref="fileInput"
                type="file"
                class="hidden"
                :accept="ACCEPTED_TYPES"
                multiple
                @change="onFileInputChange"
            />
            <i class="pi pi-upload text-2xl text-slate-400 mb-1 block" />
            <p class="text-sm text-slate-400">
                Drag &amp; drop files here, or <span class="text-blue-400 underline">click to browse</span>
            </p>
            <p class="text-xs text-slate-500 mt-1">
                PDF, DOCX, XLSX, PNG, JPG, HEIC, GIF, MP4, MOV — max 25 MB each
            </p>
        </div>

        <!-- Upload progress -->
        <div v-if="uploading.length" class="mt-2 space-y-1">
            <div
                v-for="u in uploading"
                :key="u.name"
                class="flex items-center gap-2 bg-slate-800/60 border border-slate-700 rounded px-3 py-2 text-xs"
            >
                <i class="pi pi-spin pi-spinner text-blue-400" />
                <span class="flex-1 truncate text-slate-300">{{ u.name }}</span>
                <span class="text-slate-500">Uploading…</span>
            </div>
        </div>

        <!-- Attachment list -->
        <div v-if="attachments.length" class="mt-2 space-y-1">
            <div
                v-for="a in attachments"
                :key="a.id"
                class="flex items-center gap-2 bg-slate-800/40 border border-slate-700 rounded px-3 py-2 text-xs group"
            >
                <i :class="['shrink-0 text-slate-400', fileIcon(a.fileName)]" />
                <div class="flex-1 min-w-0">
                    <p class="text-slate-200 truncate font-medium">{{ a.fileName }}</p>
                    <p class="text-slate-500 mt-0.5">
                        {{ formatBytes(a.fileSizeBytes) }}
                        &middot; {{ a.uploadedBy }}
                        &middot; {{ formatDate(a.uploadedAt) }}
                    </p>
                </div>
                <div class="flex items-center gap-1 shrink-0">
                    <Button
                        icon="pi pi-download"
                        size="small"
                        severity="secondary"
                        text
                        v-tooltip.top="'Download'"
                        :loading="downloading === a.id"
                        @click.stop="onDownload(a)"
                    />
                    <Button
                        v-if="!readonly"
                        icon="pi pi-trash"
                        size="small"
                        severity="danger"
                        text
                        v-tooltip.top="'Delete'"
                        :loading="deleting === a.id"
                        @click.stop="onDelete(a)"
                    />
                </div>
            </div>
        </div>

        <p v-else-if="!uploading.length" class="mt-2 text-xs text-slate-500 italic">No attachments yet.</p>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import Button from 'primevue/button';
import { useToast } from 'primevue/usetoast';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type AuditAttachmentDto } from '@/apiclient/auditClient';

const props = defineProps<{
    auditId: number;
    readonly?: boolean;
}>();

const ACCEPTED_TYPES = '.pdf,.docx,.doc,.xlsx,.xls,.png,.jpg,.jpeg,.heic,.gif,.mp4,.mov';
const MAX_BYTES = 25 * 1024 * 1024;

const toast = useToast();
const apiStore = useApiStore();
const fileInput = ref<HTMLInputElement | null>(null);
const attachments = ref<AuditAttachmentDto[]>([]);
const uploading = ref<{ name: string }[]>([]);
const downloading = ref<number | null>(null);
const deleting = ref<number | null>(null);
const isDragging = ref(false);

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}

async function loadAttachments() {
    try {
        attachments.value = await getClient().getAttachments(props.auditId);
    } catch {
        // Non-fatal — attachment list is supplementary
    }
}

async function uploadFiles(files: File[]) {
    const client = getClient();
    for (const file of files) {
        if (file.size > MAX_BYTES) {
            toast.add({ severity: 'warn', summary: 'File too large', detail: `${file.name} exceeds the 25 MB limit.`, life: 4000 });
            continue;
        }
        uploading.value.push({ name: file.name });
        try {
            const dto = await client.uploadAttachment(props.auditId, file);
            attachments.value.unshift(dto);
            toast.add({ severity: 'success', summary: 'Uploaded', detail: file.name, life: 2500 });
        } catch {
            toast.add({ severity: 'error', summary: 'Upload failed', detail: `Could not upload ${file.name}.`, life: 4000 });
        } finally {
            uploading.value = uploading.value.filter(u => u.name !== file.name);
        }
    }
}

function onDrop(e: DragEvent) {
    isDragging.value = false;
    const files = Array.from(e.dataTransfer?.files ?? []);
    if (files.length) uploadFiles(files);
}

function onFileInputChange(e: Event) {
    const input = e.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    if (files.length) uploadFiles(files);
    input.value = '';
}

async function onDownload(a: AuditAttachmentDto) {
    downloading.value = a.id;
    try {
        const { blob, fileName } = await getClient().downloadAttachment(props.auditId, a.id);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName || a.fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    } catch {
        toast.add({ severity: 'error', summary: 'Download failed', detail: 'Could not download the file.', life: 4000 });
    } finally {
        downloading.value = null;
    }
}

async function onDelete(a: AuditAttachmentDto) {
    deleting.value = a.id;
    try {
        await getClient().deleteAttachment(props.auditId, a.id);
        attachments.value = attachments.value.filter(x => x.id !== a.id);
        toast.add({ severity: 'info', summary: 'Deleted', detail: a.fileName, life: 2500 });
    } catch {
        toast.add({ severity: 'error', summary: 'Delete failed', detail: 'Could not delete the attachment.', life: 4000 });
    } finally {
        deleting.value = null;
    }
}

function formatBytes(bytes: number): string {
    if (!bytes || bytes <= 0) return '—';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function formatDate(iso: string): string {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}

function fileIcon(name: string): string {
    const ext = name.split('.').pop()?.toLowerCase() ?? '';
    if (['jpg', 'jpeg', 'png', 'gif', 'heic'].includes(ext)) return 'pi pi-image';
    if (['mp4', 'mov'].includes(ext)) return 'pi pi-video';
    if (ext === 'pdf') return 'pi pi-file-pdf';
    if (['xlsx', 'xls'].includes(ext)) return 'pi pi-table';
    if (['docx', 'doc'].includes(ext)) return 'pi pi-file-word';
    return 'pi pi-file';
}

onMounted(loadAttachments);
</script>

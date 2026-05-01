<template>
    <Dialog
        :visible="show"
        modal
        :header="distributionStep === 'sent'
            ? (distributionSentInfo?.type === 'api' ? 'Distribution Sent' : 'Email Client Opened')
            : 'Send Distribution'"
        :style="{ width: '820px', maxHeight: '96vh' }"
        :contentStyle="{ padding: 0 }"
        @update:visible="$emit('update:show', $event)"
    >
        <!-- ── Sent confirmation ───────────────────────────────────────────── -->
        <div v-if="distributionStep === 'sent' && distributionSentInfo" class="dist-sent-body">
            <div class="dist-sent-icon">
                <i :class="distributionSentInfo.type === 'api'
                    ? 'pi pi-check-circle text-emerald-400'
                    : 'pi pi-envelope text-blue-400'" />
            </div>
            <p v-if="distributionSentInfo.type === 'api'" class="text-sm text-slate-300 mt-1">
                Distribution email sent successfully.
            </p>
            <p v-else class="text-sm text-slate-300 mt-1">
                Your email client has been opened. Complete sending in your email application.
            </p>
            <div class="dist-sent-grid">
                <div class="dist-sent-col">
                    <p class="dist-sent-col-label">Sent To</p>
                    <div
                        v-for="r in distributionSentInfo.recipients"
                        :key="r"
                        class="flex items-center gap-2 text-sm text-slate-200 py-0.5"
                    >
                        <i class="pi pi-envelope text-blue-400 text-[10px]" />{{ r }}
                    </div>
                    <p v-if="!distributionSentInfo.recipients.length" class="text-xs text-slate-500 italic">No recipients</p>
                </div>
                <div class="dist-sent-col">
                    <p class="dist-sent-col-label">Included</p>
                    <div
                        v-for="item in distributionSentInfo.items"
                        :key="item"
                        class="flex items-center gap-2 text-sm text-slate-200 py-0.5"
                    >
                        <i class="pi pi-check text-emerald-400 text-[10px]" />{{ item }}
                    </div>
                    <p v-if="!distributionSentInfo.items.length" class="text-xs text-slate-500 italic">No items selected</p>
                </div>
            </div>
        </div>

        <!-- ── Compose ─────────────────────────────────────────────────────── -->
        <div v-else-if="review && distributionPreview" class="dist-modal-body">

            <!-- Section 1 — Include in Distribution -->
            <div class="dist-section">
                <div class="dist-section-header">
                    <i class="pi pi-list-check text-blue-400 text-sm" />
                    <span>Include in Distribution</span>
                </div>
                <div class="space-y-1">
                    <label class="include-option">
                        <input type="checkbox" :checked="includeAuditPdf" @change="$emit('update:includeAuditPdf', ($event.target as HTMLInputElement).checked)" class="accent-blue-500 w-4 h-4 shrink-0" />
                        <i class="pi pi-file-pdf text-red-400 text-sm shrink-0" />
                        <span class="text-sm text-slate-200 flex-1">Audit PDF</span>
                        <span class="text-xs text-slate-500">Full audit report</span>
                    </label>

                    <label class="include-option" :class="{ 'opacity-40 pointer-events-none': (review.nonConformingCount ?? 0) === 0 }">
                        <input type="checkbox" :checked="includeCas" :disabled="(review.nonConformingCount ?? 0) === 0" @change="$emit('update:includeCas', ($event.target as HTMLInputElement).checked)" class="accent-blue-500 w-4 h-4 shrink-0" />
                        <i class="pi pi-exclamation-triangle text-amber-400 text-sm shrink-0" />
                        <span class="text-sm text-slate-200 flex-1">Corrective Actions</span>
                        <span class="text-xs text-slate-500">{{ review.nonConformingCount }} finding{{ review.nonConformingCount !== 1 ? 's' : '' }}</span>
                    </label>
                    <div v-if="includeCas && (review.nonConformingCount ?? 0) > 0" class="ml-8 flex gap-5 py-1">
                        <label class="flex items-center gap-1.5 text-xs text-slate-300 cursor-pointer">
                            <input type="radio" :value="false" :checked="!includeOpenCasOnly" @change="$emit('update:includeOpenCasOnly', false)" class="accent-blue-500" /> All CAs
                        </label>
                        <label class="flex items-center gap-1.5 text-xs text-slate-300 cursor-pointer">
                            <input type="radio" :value="true" :checked="includeOpenCasOnly" @change="$emit('update:includeOpenCasOnly', true)" class="accent-blue-500" /> Open CAs only
                        </label>
                    </div>

                    <label class="include-option" :class="{ 'opacity-40 pointer-events-none': !(review.attachments?.length) }">
                        <input type="checkbox" :checked="includeAttachments" :disabled="!(review.attachments?.length)" @change="$emit('update:includeAttachments', ($event.target as HTMLInputElement).checked)" class="accent-blue-500 w-4 h-4 shrink-0" />
                        <i class="pi pi-paperclip text-slate-400 text-sm shrink-0" />
                        <span class="text-sm text-slate-200 flex-1">Attachments</span>
                        <span class="text-xs text-slate-500">{{ review.attachments?.length ?? 0 }} file{{ (review.attachments?.length ?? 0) !== 1 ? 's' : '' }}</span>
                    </label>
                    <div v-if="includeAttachments && review.attachments?.length" class="ml-8 space-y-0.5 bg-slate-800/50 rounded-lg p-2 mt-1">
                        <label
                            v-for="att in review.attachments"
                            :key="att.id"
                            class="flex items-center gap-2 text-xs text-slate-300 cursor-pointer hover:text-white py-1 px-1 rounded transition-colors hover:bg-slate-700/40"
                        >
                            <input
                                type="checkbox"
                                :value="att.id"
                                :checked="selectedAttachmentIds.includes(att.id)"
                                :disabled="!att.hasFile"
                                @change="toggleAttachment(att.id, ($event.target as HTMLInputElement).checked)"
                                class="accent-sky-500 w-3.5 h-3.5 shrink-0"
                            />
                            <i class="pi pi-file text-slate-500 text-[10px] shrink-0" />
                            <span class="flex-1 truncate" :class="{ 'text-slate-500': !att.hasFile }">{{ att.fileName }}</span>
                            <span class="text-slate-600 shrink-0">{{ formatBytes(att.fileSizeBytes) }}</span>
                            <span v-if="!att.hasFile" class="text-red-500 text-[10px] shrink-0">missing</span>
                        </label>
                    </div>
                </div>
            </div>

            <!-- Section 2 — Recipients -->
            <div class="dist-section">
                <div class="dist-section-header">
                    <i class="pi pi-users text-blue-400 text-sm" />
                    <span>Recipients</span>
                    <span class="dist-count">{{ review?.distributionRecipients?.length ?? 0 }}</span>
                </div>
                <div v-if="(review?.distributionRecipients?.length ?? 0) > 0" class="dist-recipients-list mb-2">
                    <div
                        v-for="r in review!.distributionRecipients"
                        :key="r.id"
                        class="flex items-center gap-2 text-xs text-slate-300 py-1"
                    >
                        <i class="pi pi-envelope text-slate-500 text-[10px] shrink-0" />
                        <span class="flex-1 truncate">{{ r.name ? `${r.name} <${r.emailAddress}>` : r.emailAddress }}</span>
                        <button
                            class="text-slate-600 hover:text-red-400 transition-colors p-0.5 flex-shrink-0"
                            :disabled="removingRecipientId === r.id"
                            title="Remove recipient"
                            @click="$emit('removeRecipient', r.id)"
                        >
                            <i :class="removingRecipientId === r.id ? 'pi pi-spin pi-spinner' : 'pi pi-times'" class="text-[10px]" />
                        </button>
                    </div>
                </div>
                <p v-else class="text-xs text-amber-400 flex items-center gap-1.5 py-1 mb-2">
                    <i class="pi pi-exclamation-triangle text-[11px]" />
                    No recipients — add one below before sending.
                </p>
                <!-- Inline add -->
                <div class="flex gap-2">
                    <InputText
                        v-model="newRecipientEmail"
                        placeholder="email@example.com"
                        size="small"
                        class="flex-1 text-xs"
                        @keyup.enter="submitInlineAdd"
                    />
                    <InputText
                        v-model="newRecipientName"
                        placeholder="Name (optional)"
                        size="small"
                        class="w-32 text-xs"
                    />
                    <Button
                        icon="pi pi-plus"
                        size="small"
                        severity="secondary"
                        :loading="addingRecipient"
                        :disabled="!newRecipientEmail.trim()"
                        title="Add recipient"
                        @click="submitInlineAdd"
                    />
                </div>
            </div>

            <!-- Section 3 — Subject -->
            <div class="dist-section">
                <div class="dist-section-header">
                    <i class="pi pi-tag text-blue-400 text-sm" />
                    <span>Subject</span>
                </div>
                <InputText :value="subject" @input="$emit('update:subject', ($event.target as HTMLInputElement).value)" class="w-full font-mono text-sm" />
            </div>

            <!-- Section 4 — Message -->
            <div class="dist-section">
                <div class="dist-section-header">
                    <i class="pi pi-align-left text-blue-400 text-sm" />
                    <span>Message</span>
                    <span class="text-xs text-slate-500 font-normal ml-1">— edit before sending</span>
                </div>
                <Textarea
                    :value="summaryEdit"
                    @input="$emit('update:summaryEdit', ($event.target as HTMLTextAreaElement).value)"
                    rows="4"
                    class="w-full text-sm"
                    placeholder="Write a findings narrative to include in the email…"
                    autoResize
                />
            </div>

            <!-- Section 5 — Email Preview -->
            <div class="dist-section">
                <div class="dist-section-header">
                    <i class="pi pi-eye text-blue-400 text-sm" />
                    <span>Email Preview</span>
                </div>
                <div class="dist-email-preview">
                    <!-- Backend-generated HTML — canonical, matches what is actually sent -->
                    <div v-html="distributionPreview.bodyHtml" class="dist-email-html" />
                </div>
            </div>

            <!-- Send summary footer bar -->
            <div class="dist-send-summary">
                <p class="text-xs font-semibold text-slate-400 mb-2 uppercase tracking-wide">Sending:</p>
                <div class="flex flex-wrap gap-2">
                    <span class="send-pill send-pill--blue">
                        <i class="pi pi-users text-[10px]" />
                        {{ distributionPreview.recipients.length }} recipient{{ distributionPreview.recipients.length !== 1 ? 's' : '' }}
                    </span>
                    <span v-if="includeAuditPdf" class="send-pill send-pill--red">
                        <i class="pi pi-file-pdf text-[10px]" />Audit PDF
                    </span>
                    <span v-if="includeCas && (review.nonConformingCount ?? 0) > 0" class="send-pill send-pill--amber">
                        <i class="pi pi-exclamation-triangle text-[10px]" />
                        CAs{{ includeOpenCasOnly ? ' (open)' : '' }}
                    </span>
                    <span v-if="includeAttachments && selectedAttachmentIds.length > 0" class="send-pill send-pill--slate">
                        <i class="pi pi-paperclip text-[10px]" />
                        {{ selectedAttachmentIds.length }} file{{ selectedAttachmentIds.length !== 1 ? 's' : '' }}
                    </span>
                </div>
                <p class="text-[11px] text-slate-500 mt-2 flex items-center gap-1">
                    <i class="pi pi-info-circle" />
                    If SMTP is not configured, your email client will open with the email pre-filled.
                </p>
            </div>
        </div>

        <template #footer>
            <template v-if="distributionStep === 'sent'">
                <Button label="Done" icon="pi pi-check" severity="success" @click="$emit('close')" />
            </template>
            <template v-else>
                <Button label="Cancel" severity="secondary" text @click="$emit('close')" />
                <Button
                    label="Open in Email Client"
                    icon="pi pi-envelope"
                    severity="secondary"
                    outlined
                    :disabled="(distributionPreview?.recipients.length ?? 0) === 0"
                    @click="$emit('openMailto')"
                />
                <Button
                    label="Send Distribution"
                    icon="pi pi-send"
                    severity="success"
                    :loading="distributionSending"
                    :disabled="(distributionPreview?.recipients.length ?? 0) === 0"
                    @click="$emit('send')"
                />
            </template>
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import type { AuditReviewDto, DistributionPreviewDto } from '@/apiclient/auditClient';

type SentInfo = { type: 'api' | 'mailto'; recipients: string[]; items: string[] } | null;

const props = defineProps<{
    show: boolean;
    review: AuditReviewDto | null;
    distributionPreview: DistributionPreviewDto | null;
    distributionStep: 'compose' | 'sent';
    distributionSentInfo: SentInfo;
    distributionSending: boolean;
    summaryEdit: string;
    subject: string;
    selectedAttachmentIds: number[];
    includeAuditPdf: boolean;
    includeCas: boolean;
    includeOpenCasOnly: boolean;
    includeAttachments: boolean;
    addingRecipient?: boolean;
    removingRecipientId?: number | null;
}>();

const emit = defineEmits<{
    'update:show': [value: boolean];
    'update:summaryEdit': [value: string];
    'update:subject': [value: string];
    'update:selectedAttachmentIds': [value: number[]];
    'update:includeAuditPdf': [value: boolean];
    'update:includeCas': [value: boolean];
    'update:includeOpenCasOnly': [value: boolean];
    'update:includeAttachments': [value: boolean];
    send: [];
    openMailto: [];
    close: [];
    addRecipient: [email: string, name: string];
    removeRecipient: [id: number];
}>();

const newRecipientEmail = ref('');
const newRecipientName  = ref('');

function submitInlineAdd() {
    const email = newRecipientEmail.value.trim();
    if (!email) return;
    emit('addRecipient', email, newRecipientName.value.trim());
    newRecipientEmail.value = '';
    newRecipientName.value  = '';
}

function formatBytes(bytes?: number | null): string {
    if (!bytes) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function toggleAttachment(id: number, checked: boolean) {
    const ids = props.selectedAttachmentIds.slice();
    if (checked) {
        if (!ids.includes(id)) ids.push(id);
    } else {
        const idx = ids.indexOf(id);
        if (idx !== -1) ids.splice(idx, 1);
    }
    emit('update:selectedAttachmentIds', ids);
}
</script>

<style scoped>
.dist-modal-body {
    display: flex;
    flex-direction: column;
    gap: 0;
    padding: 4px 0;
}

.dist-section {
    padding: 14px 20px;
    border-bottom: 1px solid rgba(51, 65, 85, 0.5);
}
.dist-section:last-child { border-bottom: none; }

.dist-section-header {
    display: flex;
    align-items: center;
    gap: 7px;
    font-size: 0.72rem;
    font-weight: 700;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: #94a3b8;
    margin-bottom: 10px;
}

.dist-count {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 18px;
    height: 18px;
    padding: 0 5px;
    border-radius: 9px;
    background: rgba(59, 130, 246, 0.2);
    color: #60a5fa;
    font-size: 0.65rem;
    font-weight: 700;
}

.dist-recipients-list {
    display: flex;
    flex-direction: column;
    gap: 4px;
    max-height: 100px;
    overflow-y: auto;
    padding: 6px 8px;
    background: rgba(15, 23, 42, 0.4);
    border-radius: 6px;
    border: 1px solid rgba(51, 65, 85, 0.4);
}

.include-option {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 7px 10px;
    border-radius: 7px;
    cursor: pointer;
    transition: background 0.1s;
}
.include-option:hover { background: rgba(51, 65, 85, 0.4); }

.dist-send-summary {
    padding: 12px 20px 4px;
    border-top: 1px solid rgba(51, 65, 85, 0.5);
}

.send-pill {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    padding: 3px 10px;
    border-radius: 20px;
    font-size: 0.72rem;
    font-weight: 600;
}
.send-pill--blue  { background: rgba(59, 130, 246, 0.15); color: #60a5fa; border: 1px solid rgba(59, 130, 246, 0.3); }
.send-pill--red   { background: rgba(239, 68, 68, 0.12);  color: #f87171; border: 1px solid rgba(239, 68, 68, 0.25); }
.send-pill--amber { background: rgba(245, 158, 11, 0.12); color: #fbbf24; border: 1px solid rgba(245, 158, 11, 0.25); }
.send-pill--slate { background: rgba(100, 116, 139, 0.15); color: #94a3b8; border: 1px solid rgba(100, 116, 139, 0.3); }

.dist-email-preview {
    border: 1px solid #334155;
    border-radius: 8px;
    overflow-y: auto;
    background: #ffffff;
    min-height: 200px;
    max-height: 520px;
    padding: 0;
}

.dist-email-html {
    font-family: sans-serif;
    color: #111827;
}

.dist-sent-body {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 32px 24px 24px;
    text-align: center;
    gap: 4px;
}

.dist-sent-icon { font-size: 3rem; line-height: 1; }

.dist-sent-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 24px;
    width: 100%;
    max-width: 520px;
    margin-top: 24px;
    text-align: left;
}

.dist-sent-col {
    background: rgba(15, 23, 42, 0.4);
    border: 1px solid rgba(51, 65, 85, 0.5);
    border-radius: 8px;
    padding: 12px 14px;
}

.dist-sent-col-label {
    font-size: 0.68rem;
    font-weight: 700;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: #64748b;
    margin-bottom: 8px;
}
</style>

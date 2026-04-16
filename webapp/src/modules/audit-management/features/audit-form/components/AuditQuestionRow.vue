<template>
    <div
        :class="[
            'question-row border-b border-slate-700 last:border-b-0',
            response.status === 'NonConforming' ? 'bg-red-950/40' : '',
            response.status === 'Warning' ? 'bg-amber-950/30' : '',
        ]"
    >
        <!-- Question: badge + text -->
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
import { computed, ref, watch } from 'vue';
import Textarea from 'primevue/textarea';
import StatusButtons from './StatusButtons.vue';
import { useAuditStore, type ResponseState } from '@/modules/audit-management/stores/auditStore';

const props = defineProps<{
    response: ResponseState;
    displayOrder: number;
    disabled?: boolean;
}>();

const store = useAuditStore();

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

<template>
    <div :data-section-id="section.id" class="border border-slate-700 rounded-lg overflow-hidden mb-2 shadow-md">
        <!-- Section header -->
        <div
            role="button"
            tabindex="0"
            :aria-expanded="isOpen"
            class="w-full flex items-center justify-between px-4 py-2 bg-[#162032] hover:bg-[#1e2d45] transition-colors duration-150 cursor-pointer select-none"
            @click="isOpen = !isOpen"
            @keydown.enter.space.prevent="isOpen = !isOpen"
        >
            <!-- Left: section name + progress badges -->
            <div class="flex items-center gap-3 flex-1 min-w-0">
                <span class="font-bold text-white tracking-wider uppercase text-sm">{{ section.name }}</span>

                <template v-if="!isNa">
                    <span
                        :class="[
                            'text-xs font-medium',
                            answeredCount === section.questions.length && section.questions.length > 0
                                ? 'text-emerald-400'
                                : ncCount > 0
                                    ? 'text-red-400'
                                    : 'text-slate-400',
                        ]"
                    >{{ answeredCount }}/{{ section.questions.length }}</span>

                    <span
                        v-if="sectionScore.scorePercent !== null"
                        :class="[
                            'text-xs font-semibold px-1.5 py-0.5 rounded',
                            sectionScore.scorePercent >= 80
                                ? 'bg-emerald-900/60 text-emerald-300'
                                : sectionScore.scorePercent >= 60
                                    ? 'bg-yellow-900/60 text-yellow-300'
                                    : 'bg-red-900/60 text-red-300',
                        ]"
                    >{{ Math.round(sectionScore.scorePercent) }}%</span>
                </template>

                <template v-else>
                    <span class="text-xs font-semibold px-2 py-0.5 rounded-full bg-amber-900/50 text-amber-300 border border-amber-700/40">
                        N/A
                    </span>
                    <span v-if="naReason" class="text-amber-400/70 text-xs truncate max-w-xs">— {{ naReason }}</span>
                </template>
            </div>

            <!-- Right: N/A controls + chevron — click.stop so chevron-area clicks don't also toggle open -->
            <div class="flex items-center gap-2 shrink-0 ml-2" @click.stop>
                <button
                    v-if="!disabled && !isNa"
                    type="button"
                    class="text-xs text-slate-400 hover:text-amber-300 border border-slate-600 hover:border-amber-600 rounded px-2 py-0.5 transition-colors"
                    @click="startNaInput"
                >
                    Mark N/A
                </button>
                <button
                    v-if="!disabled && isNa"
                    type="button"
                    class="text-xs text-amber-400 hover:text-amber-200 border border-amber-700/50 hover:border-amber-500 rounded px-2 py-0.5 transition-colors"
                    @click="clearNa"
                >
                    Remove N/A
                </button>
                <span
                    :class="[
                        'text-slate-400 text-xs transition-transform duration-200',
                        isOpen ? 'rotate-180' : '',
                    ]"
                >▼</span>
            </div>
        </div>

        <!-- Inline N/A reason input — appears between header and questions -->
        <div v-if="showNaInput" class="px-4 py-2 bg-slate-900 border-b border-slate-700" @click.stop>
            <p class="text-slate-300 text-sm font-medium mb-2">Why is this section not applicable?</p>
            <div class="flex items-center gap-2">
                <input
                    ref="reasonInputEl"
                    v-model="pendingReason"
                    type="text"
                    maxlength="500"
                    placeholder="Enter reason (required)"
                    class="flex-1 bg-slate-800 border border-slate-600 rounded px-3 py-1.5 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-amber-500"
                    @keydown.enter.prevent="confirmNa"
                    @keydown.escape.prevent="cancelNa"
                />
                <button
                    type="button"
                    :disabled="!pendingReason.trim()"
                    class="px-3 py-1.5 text-sm bg-amber-600 hover:bg-amber-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded transition-colors"
                    @click="confirmNa"
                >
                    Confirm
                </button>
                <button
                    type="button"
                    class="px-3 py-1.5 text-sm bg-slate-700 hover:bg-slate-600 text-white rounded transition-colors"
                    @click="cancelNa"
                >
                    Cancel
                </button>
            </div>
        </div>

        <!-- Question rows (hidden when section is N/A) -->
        <div v-if="isOpen && !isNa" class="bg-slate-800">
            <AuditQuestionRow
                v-for="(q, idx) in section.questions"
                :key="q.questionId"
                :response="getResponse(q.questionId)"
                :display-order="idx + 1"
                :disabled="disabled"
                :is-repeat-finding="store.repeatFindingQuestionIds.has(q.questionId)"
            />
        </div>

        <!-- N/A collapsed body (shown when section is N/A and user expands it) -->
        <div v-if="isOpen && isNa" class="bg-slate-800/60 px-4 py-2 text-sm text-amber-400/70 italic">
            This section has been marked not applicable.
            <span v-if="naReason"> Reason: <span class="not-italic text-amber-300">{{ naReason }}</span></span>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick } from 'vue';
import type { TemplateSectionDto } from '@/apiclient/auditClient';
import { useAuditStore, calculateScore, type ResponseState } from '@/modules/audit-management/stores/auditStore';
import AuditQuestionRow from './AuditQuestionRow.vue';

const props = withDefaults(defineProps<{
    section: TemplateSectionDto;
    startOpen?: boolean;
    disabled?: boolean;
}>(), {
    startOpen: true,
    disabled: false,
});

const store = useAuditStore();
const isOpen = ref(props.startOpen);

// ── N/A state ──────────────────────────────────────────────────────────────────

const isNa = computed(() => store.sectionNaOverrides.has(props.section.id));
const naReason = computed(() => store.sectionNaOverrides.get(props.section.id) ?? '');

const showNaInput = ref(false);
const pendingReason = ref('');
const reasonInputEl = ref<HTMLInputElement | null>(null);

function startNaInput() {
    pendingReason.value = '';
    showNaInput.value = true;
    isOpen.value = true;
    nextTick(() => reasonInputEl.value?.focus());
}

function confirmNa() {
    const reason = pendingReason.value.trim();
    if (!reason) return;
    store.setSectionNa(props.section.id, reason);
    showNaInput.value = false;
    pendingReason.value = '';
    isOpen.value = false;
}

function cancelNa() {
    showNaInput.value = false;
    pendingReason.value = '';
}

function clearNa() {
    store.setSectionNa(props.section.id, null);
    isOpen.value = true;
}

// ── Response helpers ───────────────────────────────────────────────────────────

const EMPTY_RESPONSE: ResponseState = {
    questionId: 0,
    questionTextSnapshot: '',
    status: null,
    comment: null,
    correctedOnSite: false,
    allowNA: true,
    requireCommentOnNC: false,
    isScoreable: true,
    requirePhotoOnNc: false,
    autoCreateCa: false,
};

function getResponse(questionId: number): ResponseState {
    return store.responses.get(questionId) ?? EMPTY_RESPONSE;
}

const answeredCount = computed(() =>
    props.section.questions.filter(q => {
        const r = store.responses.get(q.questionId);
        return r?.status != null;
    }).length,
);

const ncCount = computed(() =>
    props.section.questions.filter(q => {
        const r = store.responses.get(q.questionId);
        return r?.status === 'NonConforming';
    }).length,
);

const sectionScore = computed(() => {
    const sectionResponses = props.section.questions
        .map(q => store.responses.get(q.questionId))
        .filter((r): r is ResponseState => r != null);
    return calculateScore(sectionResponses);
});

function toggleOpen() {
    isOpen.value = !isOpen.value;
}

defineExpose({ toggleOpen, isOpen });
</script>

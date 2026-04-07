<template>
    <div class="border border-slate-700 rounded-lg overflow-hidden mb-3 shadow-md">
        <!-- Section header (click to toggle) -->
        <button
            type="button"
            class="w-full flex items-center justify-between px-4 py-3 text-left bg-[#162032] hover:bg-[#1e2d45] transition-colors duration-150"
            :aria-expanded="isOpen"
            @click="isOpen = !isOpen"
        >
            <!-- Left: section name + progress -->
            <div class="flex items-center gap-3">
                <span class="font-bold text-white tracking-wider uppercase text-sm">{{ section.name }}</span>
                <span
                    :class="[
                        'text-xs font-medium',
                        answeredCount === section.questions.length && section.questions.length > 0
                            ? 'text-emerald-400'
                            : ncCount > 0
                                ? 'text-red-400'
                                : 'text-slate-400',
                    ]"
                >
                    {{ answeredCount }}/{{ section.questions.length }}
                </span>
            </div>

            <!-- Right: filled triangle matching legacy -->
            <span
                :class="[
                    'text-slate-400 text-xs transition-transform duration-200',
                    isOpen ? 'rotate-180' : '',
                ]"
            >▼</span>
        </button>

        <!-- Question rows -->
        <div v-if="isOpen" class="bg-slate-800">
            <AuditQuestionRow
                v-for="(q, idx) in section.questions"
                :key="q.questionId"
                :response="getResponse(q.questionId)"
                :display-order="idx + 1"
                :disabled="disabled"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { TemplateSectionDto } from '@/apiclient/auditClient';
import { useAuditStore, type ResponseState } from '@/modules/audit-management/stores/auditStore';
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

const EMPTY_RESPONSE: ResponseState = {
    questionId: 0,
    questionTextSnapshot: '',
    status: null,
    comment: null,
    correctedOnSite: false,
    allowNA: true,
    requireCommentOnNC: false,
    isScoreable: true,
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

function toggleOpen() {
    isOpen.value = !isOpen.value;
}

defineExpose({ toggleOpen, isOpen });
</script>

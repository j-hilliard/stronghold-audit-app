<template>
    <div class="space-y-2">
        <textarea
            v-if="editing"
            v-model="localText"
            rows="6"
            class="w-full bg-slate-700 border border-slate-600 rounded px-3 py-2 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500 resize-y"
            placeholder="Enter narrative summary…"
            data-testid="block-narrative-text"
            @input="onInput"
        />
        <div
            v-else
            class="text-sm text-slate-300 leading-relaxed whitespace-pre-wrap cursor-text rounded p-2 hover:bg-slate-700/50 transition-colors"
            :class="{ 'text-slate-500 italic': !content.text }"
            @click="editing = true"
        >
            {{ content.text || 'Click to add narrative text, or use Generate ✨ to create an AI draft.' }}
        </div>
        <div class="flex items-center gap-2">
            <span v-if="isEdited" class="text-xs text-amber-400/70">
                <i class="pi pi-lock text-xs" /> Locked — regeneration will not overwrite this text
            </span>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { NarrativeContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: NarrativeContent;
    style: BlockStyle;
    isEdited: boolean;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: NarrativeContent): void;
    (e: 'update:isEdited', value: boolean): void;
}>();

const editing = ref(false);
const localText = ref(props.content.text);

watch(() => props.content.text, (v) => { localText.value = v; });

function onInput() {
    emit('update:content', { ...props.content, text: localText.value });
    if (!props.isEdited) emit('update:isEdited', true);
}
</script>

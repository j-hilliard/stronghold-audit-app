<template>
    <div class="space-y-1">
        <RichTextEditor
            :model-value="content.text"
            placeholder="Enter narrative summary, or use Generate ✨ to create an AI draft."
            :readonly="false"
            data-testid="block-narrative-text"
            @update:model-value="onTextUpdate"
        />
        <div v-if="isEdited" class="flex items-center gap-1.5 mt-1">
            <span class="text-xs text-amber-400/70">
                <i class="pi pi-lock text-xs" /> Locked — regeneration will not overwrite this text
            </span>
        </div>
    </div>
</template>

<script setup lang="ts">
import type { NarrativeContent, BlockStyle } from '../../types/report-block';
import RichTextEditor from '../RichTextEditor.vue';

const props = defineProps<{
    content: NarrativeContent;
    style: BlockStyle;
    isEdited: boolean;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: NarrativeContent): void;
    (e: 'update:isEdited', value: boolean): void;
}>();

function onTextUpdate(html: string) {
    emit('update:content', { ...props.content, text: html });
    if (!props.isEdited) emit('update:isEdited', true);
}
</script>

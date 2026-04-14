<template>
    <div class="fc-block">
        <!-- Section name header bar -->
        <div class="fc-header" :style="headerStyle">
            <span class="fc-section-name">{{ content.sectionName || 'Section Name' }}</span>
        </div>

        <!-- "Examples:" label -->
        <div v-if="content.showExamplesLabel" class="fc-examples-label">Examples:</div>

        <!-- Rich text findings list -->
        <RichTextEditor
            :model-value="content.findings"
            placeholder="Add findings examples — try a bullet list…"
            :readonly="!isEdited && !isFocused"
            class="fc-findings"
            @update:model-value="onFindingsUpdate"
            @focus="isFocused = true"
            @blur="isFocused = false"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { FindingsCategoryContent, BlockStyle } from '../../types/report-block';
import RichTextEditor from '../RichTextEditor.vue';

const props = defineProps<{
    content: FindingsCategoryContent;
    style: BlockStyle;
    isEdited: boolean;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: FindingsCategoryContent): void;
    (e: 'update:isEdited', value: boolean): void;
}>();

const isFocused = ref(false);

const headerStyle = computed(() => ({
    backgroundColor: props.content.accentColor || '#862633',
    borderLeftColor: props.content.accentColor || '#862633',
}));

function onFindingsUpdate(html: string) {
    emit('update:content', { ...props.content, findings: html });
}
</script>

<style scoped>
.fc-block {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
}

.fc-header {
    display: flex;
    align-items: center;
    padding: 0.35rem 0.75rem;
    border-radius: 3px;
    margin-bottom: 0.1rem;
}

.fc-section-name {
    font-size: 0.78rem;
    font-weight: 700;
    letter-spacing: 0.04em;
    text-transform: uppercase;
    color: #ffffff;
}

.fc-examples-label {
    font-size: 0.72rem;
    font-weight: 600;
    color: #94a3b8;
    letter-spacing: 0.02em;
    padding-left: 0.1rem;
}

.fc-findings {
    padding-left: 0.1rem;
    color: inherit;
}

/* Findings list items should look like newsletter bullets */
.fc-findings :deep(ul) {
    list-style: disc;
    padding-left: 1.2em;
}

.fc-findings :deep(li) {
    font-size: 0.82rem;
    line-height: 1.5;
    margin: 0.15em 0;
    color: inherit;
}

@media print {
    .fc-findings :deep(.rte-toolbar) { display: none; }
}
</style>

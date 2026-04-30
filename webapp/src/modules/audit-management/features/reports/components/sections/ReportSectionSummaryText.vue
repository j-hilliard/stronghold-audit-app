<template>
    <div class="report-section-summary-text">
        <div class="section-heading">
            <i class="pi pi-align-left text-blue-400" />
            <span>Summary</span>
            <span v-if="isEditable" class="ml-auto flex items-center gap-1 text-xs text-amber-400/80">
                <i class="pi pi-pencil text-[10px]" />editable
            </span>
        </div>

        <div v-if="isEditable">
            <textarea
                :value="text"
                @input="emit('update', { ...section, editedText: ($event.target as HTMLTextAreaElement).value })"
                rows="6"
                placeholder="Write a summary of this period's performance, key observations, and recommendations..."
                class="w-full bg-slate-800 border border-slate-600 rounded-xl p-4 text-sm text-slate-200 placeholder-slate-600 resize-none focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/50"
            />
            <div class="text-xs text-slate-600 mt-1">{{ (text ?? '').length }} characters</div>
        </div>

        <div v-else-if="text" class="text-sm text-slate-300 leading-relaxed whitespace-pre-wrap bg-slate-800/40 border border-slate-700 rounded-xl p-4">
            {{ text }}
        </div>

        <div v-else class="text-sm text-slate-600 italic py-4 text-center">
            No summary written yet. Click this section to add one.
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { StructuredSection } from '../../types/report-template';

const props = defineProps<{
    section: StructuredSection;
    isEditable: boolean;
}>();

const emit = defineEmits<{
    (e: 'update', section: StructuredSection): void;
}>();

const text = computed(() => props.section.editedText ?? '');
</script>

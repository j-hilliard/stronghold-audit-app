<template>
    <div class="report-section-findings">
        <div class="section-heading">
            <i class="pi pi-list text-blue-400" />
            <span>Findings Examples</span>
            <span v-if="isEditable" class="ml-auto flex items-center gap-1 text-xs text-amber-400/80">
                <i class="pi pi-pencil text-[10px]" />editable
            </span>
        </div>

        <div v-if="sectionNames.length === 0" class="flex flex-col items-center gap-2 py-6 text-center">
            <i class="pi pi-list text-slate-700 text-xl" />
            <p class="text-xs text-slate-600">No categories loaded — click <strong class="text-slate-500 font-semibold">Generate</strong> above to populate findings.</p>
        </div>

        <div v-else-if="isEditable" class="space-y-4">
            <div
                v-for="name in sectionNames"
                :key="name"
                class="bg-slate-800/50 border border-slate-700 rounded-xl p-4"
            >
                <div class="text-xs font-semibold text-slate-300 uppercase tracking-wide mb-2">
                    {{ name }}
                </div>
                <textarea
                    :value="getCategoryNotes(name)"
                    @input="setCategoryNotes(name, ($event.target as HTMLTextAreaElement).value)"
                    rows="3"
                    placeholder="Enter example findings for this category..."
                    class="w-full bg-slate-900 border border-slate-600 rounded-lg p-3 text-sm text-slate-200 placeholder-slate-600 resize-none focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/50"
                />
            </div>
        </div>

        <div v-else class="space-y-4">
            <div
                v-for="name in sectionNames"
                :key="name"
                class="bg-slate-800/50 border border-slate-700 rounded-xl p-4"
            >
                <div class="text-xs font-semibold text-slate-300 uppercase tracking-wide mb-2">{{ name }}</div>
                <div v-if="getCategoryNotes(name)" class="text-sm text-slate-300 whitespace-pre-wrap">
                    {{ getCategoryNotes(name) }}
                </div>
                <div v-else class="text-xs text-slate-600 italic">No examples recorded.</div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { StructuredSection } from '../../types/report-template';

const props = defineProps<{
    section: StructuredSection;
    sectionNames: string[];
    isEditable: boolean;
}>();

const emit = defineEmits<{
    (e: 'update', section: StructuredSection): void;
}>();

function parseNotes(): Record<string, string> {
    if (!props.section.editedNotes) return {};
    try { return JSON.parse(props.section.editedNotes); }
    catch { return {}; }
}

function getCategoryNotes(name: string): string {
    return parseNotes()[name] ?? '';
}

function setCategoryNotes(name: string, value: string) {
    const map = parseNotes();
    if (value.trim()) map[name] = value;
    else delete map[name];
    emit('update', { ...props.section, editedNotes: JSON.stringify(map) });
}
</script>

<template>
    <div class="report-section-highlights">
        <div class="section-heading">
            <i class="pi pi-star text-blue-400" />
            <span>Highlights</span>
            <span v-if="isEditable" class="ml-auto flex items-center gap-1 text-xs text-amber-400/80">
                <i class="pi pi-pencil text-[10px]" />editable
            </span>
        </div>

        <div v-if="isEditable" class="space-y-3">
            <div class="bg-slate-800/50 border border-slate-700 rounded-xl p-4">
                <div class="text-xs text-slate-500 mb-2">One highlight per line. Use • for bullet points.</div>
                <textarea
                    :value="raw"
                    @input="emit('update', { ...section, editedHighlights: ($event.target as HTMLTextAreaElement).value })"
                    rows="5"
                    placeholder="• Conformance improved 8% vs prior quarter&#10;• Zero life-safety violations this period&#10;• 3 corrective actions closed ahead of schedule"
                    class="w-full bg-slate-900 border border-slate-600 rounded-lg p-3 text-sm text-slate-200 placeholder-slate-600 resize-none focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/50"
                />
            </div>
        </div>

        <div v-else>
            <div v-if="highlights.length === 0" class="flex flex-col items-center gap-2 py-6 text-center">
                <i class="pi pi-star text-slate-700 text-xl" />
                <p class="text-xs text-slate-600">No highlights yet — click <strong class="text-slate-500 font-semibold">Edit</strong> (top-right) to add key takeaways.</p>
            </div>
            <ul v-else class="space-y-2">
                <li
                    v-for="(h, i) in highlights"
                    :key="i"
                    class="flex items-start gap-2.5 text-sm text-slate-200"
                >
                    <i class="pi pi-check-circle text-blue-400 text-sm mt-0.5 shrink-0" />
                    <span>{{ h }}</span>
                </li>
            </ul>
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

const raw = computed(() => props.section.editedHighlights ?? '');

const highlights = computed(() =>
    raw.value
        .split('\n')
        .map(l => l.replace(/^[•\-*]\s*/, '').trim())
        .filter(Boolean),
);
</script>

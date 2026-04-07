<template>
    <div class="w-52 shrink-0 bg-slate-800 border-r border-slate-700 flex flex-col overflow-y-auto composer-left-rail">
        <!-- Generate button -->
        <div class="p-3 border-b border-slate-700 space-y-2">
            <button
                @click="$emit('generate')"
                :disabled="generating"
                data-testid="composer-generate"
                class="w-full flex items-center justify-center gap-2 px-3 py-2 bg-blue-700 hover:bg-blue-600 disabled:opacity-50 text-white text-sm rounded font-medium transition-colors"
            >
                <i class="pi pi-sync text-xs" :class="{ 'animate-spin': generating }" />
                {{ generating ? 'Generating…' : 'Generate Report' }}
            </button>
            <button
                @click="$emit('save')"
                :disabled="saving || !isDirty"
                data-testid="composer-save"
                class="w-full flex items-center justify-center gap-2 px-3 py-1.5 bg-slate-700 hover:bg-slate-600 disabled:opacity-40 text-slate-200 text-sm rounded transition-colors"
            >
                <i class="pi pi-save text-xs" />
                {{ saving ? 'Saving…' : isDirty ? 'Save Draft' : 'Saved' }}
            </button>
            <button
                @click="$emit('print')"
                data-testid="composer-print"
                class="w-full flex items-center justify-center gap-2 px-3 py-1.5 bg-slate-700 hover:bg-slate-600 text-slate-200 text-sm rounded transition-colors"
            >
                <i class="pi pi-print text-xs" /> Print / Export PDF
            </button>
        </div>

        <!-- Block palette -->
        <div class="p-3 space-y-1">
            <div class="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">Add Block</div>

            <!-- Simple blocks — emit immediately -->
            <button
                v-for="item in simplePalette"
                :key="item.type"
                @click="$emit('add-block', item.type)"
                class="w-full text-left flex items-center gap-2 px-2 py-1.5 rounded text-sm text-slate-300 hover:bg-slate-700 transition-colors"
            >
                <i :class="item.icon" class="text-xs text-slate-500 w-4" />
                {{ item.label }}
            </button>

            <!-- Bar Chart — section picker -->
            <div>
                <button
                    @click="toggle('bar')"
                    class="w-full text-left flex items-center gap-2 px-2 py-1.5 rounded text-sm transition-colors"
                    :class="openPicker === 'bar' ? 'bg-slate-700 text-white' : 'text-slate-300 hover:bg-slate-700'"
                >
                    <i class="pi pi-chart-bar text-xs text-slate-500 w-4" />
                    <span class="flex-1">Bar Chart</span>
                    <i class="pi text-xs text-slate-500" :class="openPicker === 'bar' ? 'pi-chevron-up' : 'pi-chevron-down'" />
                </button>

                <div v-if="openPicker === 'bar'" class="ml-2 mt-1 space-y-px">
                    <!-- All-sections overview -->
                    <button
                        @click="pick('chart-bar', undefined)"
                        class="w-full text-left flex items-center gap-1.5 px-2 py-1 rounded text-xs text-blue-300 hover:bg-slate-700 transition-colors"
                    >
                        <i class="pi pi-th-large text-xs w-3" />
                        All Sections Overview
                    </button>
                    <!-- No sections available yet -->
                    <div v-if="!sections.length" class="px-2 py-1 text-xs text-slate-600 italic">
                        Generate first to load sections
                    </div>
                    <!-- Per-section bar charts -->
                    <button
                        v-for="section in sections"
                        :key="section"
                        @click="pick('chart-bar', section)"
                        class="w-full text-left flex items-center gap-1.5 px-2 py-1 rounded text-xs hover:bg-slate-700 transition-colors"
                        :class="usedBarSections.has(section) ? 'text-emerald-400' : 'text-slate-400 hover:text-slate-200'"
                    >
                        <i
                            class="text-xs w-3"
                            :class="usedBarSections.has(section) ? 'pi pi-check text-emerald-400' : 'pi pi-minus'"
                        />
                        <span class="truncate flex-1">{{ section }}</span>
                        <span v-if="usedBarSections.has(section)" class="text-xs text-emerald-500 shrink-0">✓ on canvas</span>
                    </button>
                </div>
            </div>

            <!-- Trend Chart — section picker -->
            <div>
                <button
                    @click="toggle('line')"
                    class="w-full text-left flex items-center gap-2 px-2 py-1.5 rounded text-sm transition-colors"
                    :class="openPicker === 'line' ? 'bg-slate-700 text-white' : 'text-slate-300 hover:bg-slate-700'"
                >
                    <i class="pi pi-chart-line text-xs text-slate-500 w-4" />
                    <span class="flex-1">Trend Chart</span>
                    <i class="pi text-xs text-slate-500" :class="openPicker === 'line' ? 'pi-chevron-up' : 'pi-chevron-down'" />
                </button>

                <div v-if="openPicker === 'line'" class="ml-2 mt-1 space-y-px">
                    <div v-if="!sections.length" class="px-2 py-1 text-xs text-slate-600 italic">
                        Generate first to load sections
                    </div>
                    <button
                        v-for="section in sections"
                        :key="section"
                        @click="pick('chart-line', section)"
                        class="w-full text-left flex items-center gap-1.5 px-2 py-1 rounded text-xs hover:bg-slate-700 transition-colors"
                        :class="usedLineSections.has(section) ? 'text-emerald-400' : 'text-slate-400 hover:text-slate-200'"
                    >
                        <i
                            class="text-xs w-3"
                            :class="usedLineSections.has(section) ? 'pi pi-check text-emerald-400' : 'pi pi-minus'"
                        />
                        <span class="truncate flex-1">{{ section }}</span>
                        <span v-if="usedLineSections.has(section)" class="text-xs text-emerald-500 shrink-0">✓</span>
                    </button>
                </div>
            </div>

            <!-- Remaining simple blocks -->
            <button
                v-for="item in trailingPalette"
                :key="item.type"
                @click="$emit('add-block', item.type)"
                class="w-full text-left flex items-center gap-2 px-2 py-1.5 rounded text-sm text-slate-300 hover:bg-slate-700 transition-colors"
            >
                <i :class="item.icon" class="text-xs text-slate-500 w-4" />
                {{ item.label }}
            </button>
        </div>

        <!-- Save status -->
        <div class="mt-auto p-3 border-t border-slate-700">
            <div v-if="saveError" class="text-xs text-red-400">{{ saveError }}</div>
            <div v-else-if="lastSavedAt" class="text-xs text-slate-600">
                Saved {{ relativeTime(lastSavedAt) }}
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { BlockType } from '../types/report-block';

const props = defineProps<{
    generating: boolean;
    saving: boolean;
    isDirty: boolean;
    saveError: string | null;
    lastSavedAt: Date | null;
    /** Section names from the loaded trend data — populates the chart pickers */
    sections: string[];
    /** Sections that already have a chart-line block on the canvas */
    usedLineSections: Set<string>;
    /** Sections that already have a section-specific chart-bar block on the canvas */
    usedBarSections: Set<string>;
}>();

const emit = defineEmits<{
    (e: 'generate'): void;
    (e: 'save'): void;
    (e: 'print'): void;
    (e: 'add-block', type: BlockType, sectionName?: string): void;
}>();

const openPicker = ref<'bar' | 'line' | null>(null);

function toggle(which: 'bar' | 'line') {
    openPicker.value = openPicker.value === which ? null : which;
}

function pick(type: BlockType, sectionName: string | undefined) {
    // Do NOT close picker — user should be able to click multiple sections in one pass
    emit('add-block', type, sectionName);
}

// Palette items before the chart pickers
const simplePalette: { type: BlockType; label: string; icon: string }[] = [
    { type: 'cover',    label: 'Cover Page',   icon: 'pi pi-image' },
    { type: 'heading',  label: 'Heading',       icon: 'pi pi-bars' },
    { type: 'kpi-grid', label: 'KPI Cards',     icon: 'pi pi-th-large' },
];

// Palette items after the chart pickers
const trailingPalette: { type: BlockType; label: string; icon: string }[] = [
    { type: 'narrative', label: 'Narrative Text',      icon: 'pi pi-align-left' },
    { type: 'callout',   label: 'Callout / Note',      icon: 'pi pi-info-circle' },
    { type: 'ca-table',  label: 'Corrective Actions',  icon: 'pi pi-list' },
    { type: 'image',     label: 'Image',                icon: 'pi pi-image' },
];

function relativeTime(date: Date): string {
    const secs = Math.floor((Date.now() - date.getTime()) / 1000);
    if (secs < 60) return 'just now';
    if (secs < 3600) return `${Math.floor(secs / 60)}m ago`;
    return `${Math.floor(secs / 3600)}h ago`;
}
</script>

<template>
    <div class="w-72 shrink-0 bg-slate-800 border-l border-slate-700 flex flex-col overflow-y-auto composer-right-rail">
        <div class="p-3 border-b border-slate-700 flex items-center justify-between">
            <span class="text-sm font-semibold text-slate-200">Newsletter Settings</span>
            <button @click="$emit('close')" class="text-slate-500 hover:text-slate-300 text-xs">
                <i class="pi pi-times" />
            </button>
        </div>

        <div class="p-3 space-y-4 flex-1">
            <!-- Template name -->
            <div class="space-y-1">
                <label class="text-xs text-slate-400">Template Name</label>
                <input
                    v-model="form.name"
                    type="text"
                    placeholder="e.g. Q1 2026 Newsletter"
                    class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                />
            </div>

            <!-- Primary color -->
            <div class="space-y-1">
                <label class="text-xs text-slate-400">Primary Color</label>
                <div class="flex items-center gap-2">
                    <input type="color" v-model="form.primaryColor" class="h-8 w-12 rounded cursor-pointer bg-slate-700 border border-slate-600" />
                    <input
                        v-model="form.primaryColor"
                        type="text"
                        maxlength="7"
                        class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 font-mono focus:outline-none focus:border-blue-500"
                    />
                </div>
            </div>

            <!-- Accent color -->
            <div class="space-y-1">
                <label class="text-xs text-slate-400">Accent Color</label>
                <div class="flex items-center gap-2">
                    <input type="color" v-model="form.accentColor" class="h-8 w-12 rounded cursor-pointer bg-slate-700 border border-slate-600" />
                    <input
                        v-model="form.accentColor"
                        type="text"
                        maxlength="7"
                        class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 font-mono focus:outline-none focus:border-blue-500"
                    />
                </div>
            </div>

            <!-- Cover image URL -->
            <div class="space-y-1">
                <label class="text-xs text-slate-400">Cover Image URL (optional)</label>
                <input
                    v-model="form.coverImageUrl"
                    type="text"
                    placeholder="https://…"
                    class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                />
                <div v-if="form.coverImageUrl" class="mt-1 rounded overflow-hidden border border-slate-600 h-20 bg-slate-900">
                    <img :src="form.coverImageUrl" class="w-full h-full object-cover" @error="imgError = true" />
                </div>
            </div>

            <!-- Section visibility -->
            <div v-if="availableSections.length" class="space-y-1">
                <label class="text-xs text-slate-400 flex items-center justify-between">
                    <span>Visible Sections</span>
                    <button @click="toggleAll" class="text-xs text-blue-400 hover:text-blue-300">
                        {{ allVisible ? 'Hide all' : 'Show all' }}
                    </button>
                </label>
                <div class="space-y-1 max-h-48 overflow-y-auto pr-1">
                    <label
                        v-for="section in availableSections"
                        :key="section"
                        class="flex items-center gap-2 text-xs text-slate-300 hover:text-slate-100 cursor-pointer"
                    >
                        <input
                            type="checkbox"
                            :checked="isSectionVisible(section)"
                            @change="toggleSection(section)"
                            class="accent-blue-500"
                        />
                        <span class="truncate">{{ section }}</span>
                    </label>
                </div>
            </div>

            <!-- Set as default -->
            <label class="flex items-center gap-2 text-xs text-slate-300 cursor-pointer">
                <input type="checkbox" v-model="form.isDefault" class="accent-blue-500" />
                Set as default for this division
            </label>
        </div>

        <!-- Save/status footer -->
        <div class="p-3 border-t border-slate-700 space-y-2">
            <div v-if="saveError" class="text-xs text-red-400">{{ saveError }}</div>
            <div v-else-if="savedAt" class="text-xs text-slate-500">Saved {{ relativeTime(savedAt) }}</div>
            <button
                @click="save"
                :disabled="saving || !form.name"
                class="w-full flex items-center justify-center gap-2 px-3 py-2 bg-blue-700 hover:bg-blue-600 disabled:opacity-50 text-white text-sm rounded font-medium transition-colors"
            >
                <i class="pi pi-save text-xs" :class="{ 'animate-pulse': saving }" />
                {{ saving ? 'Saving…' : 'Save Template' }}
            </button>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { NewsletterTemplateDto } from '@/apiclient/auditClient';

const props = defineProps<{
    modelValue: NewsletterTemplateDto;
    availableSections: string[];
    saving: boolean;
    saveError: string | null;
    savedAt: Date | null;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', v: NewsletterTemplateDto): void;
    (e: 'save'): void;
    (e: 'close'): void;
}>();

const imgError = ref(false);

const form = computed({
    get: () => props.modelValue,
    set: (v) => emit('update:modelValue', v),
});

watch(() => props.modelValue.coverImageUrl, () => { imgError.value = false; });

function isSectionVisible(section: string): boolean {
    if (!form.value.visibleSections) return true; // null = all visible
    return form.value.visibleSections.includes(section);
}

function toggleSection(section: string) {
    const current = form.value.visibleSections ?? [...props.availableSections];
    const idx = current.indexOf(section);
    const next = idx === -1 ? [...current, section] : current.filter(s => s !== section);
    emit('update:modelValue', { ...form.value, visibleSections: next.length === props.availableSections.length ? null : next });
}

const allVisible = computed(() => !form.value.visibleSections || form.value.visibleSections.length === props.availableSections.length);

function toggleAll() {
    if (allVisible.value) {
        emit('update:modelValue', { ...form.value, visibleSections: [] });
    } else {
        emit('update:modelValue', { ...form.value, visibleSections: null });
    }
}

function save() {
    emit('save');
}

function relativeTime(date: Date): string {
    const secs = Math.floor((Date.now() - date.getTime()) / 1000);
    if (secs < 60) return 'just now';
    if (secs < 3600) return `${Math.floor(secs / 60)}m ago`;
    return `${Math.floor(secs / 3600)}h ago`;
}
</script>

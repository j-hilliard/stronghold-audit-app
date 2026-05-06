<template>
    <div
        v-if="sectionProgress.length > 1"
        class="hidden xl:flex fixed right-3 z-20 flex-col gap-0.5
               bg-slate-900/95 backdrop-blur-sm border border-slate-700/80 rounded-xl p-2 shadow-2xl
               max-h-[75vh] overflow-y-auto w-48"
        style="top: 50%; transform: translateY(-50%);"
    >
        <!-- Quick-jump bar -->
        <div class="flex gap-1 mb-1.5 pb-1.5 border-b border-slate-700/60">
            <button
                type="button"
                class="flex-1 text-[9px] font-semibold py-1 rounded bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors leading-tight"
                title="Next unanswered section"
                @click="jumpToNextUnanswered"
            >
                Next<br>Unanswered
            </button>
            <button
                v-if="hasNcSections"
                type="button"
                class="flex-1 text-[9px] font-semibold py-1 rounded bg-red-950/60 hover:bg-red-900/60 text-red-400 hover:text-red-200 transition-colors leading-tight"
                title="Next non-conforming section"
                @click="jumpToNextNc"
            >
                Next<br>NC
            </button>
            <button
                v-if="hasWarnSections"
                type="button"
                class="flex-1 text-[9px] font-semibold py-1 rounded bg-amber-950/60 hover:bg-amber-900/60 text-amber-400 hover:text-amber-200 transition-colors leading-tight"
                title="Next warning section"
                @click="jumpToNextWarning"
            >
                Next<br>Warn
            </button>
        </div>

        <!-- Section list -->
        <button
            v-for="item in sectionProgress"
            :key="item.sectionId"
            type="button"
            :class="[
                'w-full text-left px-2 py-1.5 rounded text-xs transition-colors',
                item.sectionId === currentSectionId
                    ? 'bg-blue-900/50 border border-blue-700/50 text-blue-200'
                    : 'hover:bg-slate-800 text-slate-400 hover:text-slate-200 border border-transparent',
            ]"
            @click="jumpToSection(item.sectionId)"
        >
            <div class="font-medium truncate text-[11px] leading-snug">{{ item.name }}</div>
            <div class="flex items-center gap-1.5 mt-0.5">
                <template v-if="item.isNa">
                    <span class="text-[9px] bg-amber-900/50 text-amber-300 rounded px-1">N/A</span>
                </template>
                <template v-else>
                    <span class="text-[9px] text-slate-500">{{ item.answered }}/{{ item.totalQuestions }}</span>
                    <span v-if="item.nonConforming > 0" class="text-[9px] text-red-400 font-bold">{{ item.nonConforming }} NC</span>
                    <span v-else-if="item.warning > 0" class="text-[9px] text-amber-400 font-bold">{{ item.warning }} W</span>
                    <span v-else-if="item.answered > 0 && item.answered === item.totalQuestions && item.unanswered === 0"
                          class="text-[9px] text-emerald-400 font-bold">✓</span>
                </template>
            </div>
        </button>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { useAuditSectionProgress } from '../composables/useAuditSectionProgress';

const { sectionProgress } = useAuditSectionProgress();

const currentSectionId = ref<number | null>(null);

const hasNcSections  = computed(() => sectionProgress.value.some(s => s.nonConforming > 0));
const hasWarnSections = computed(() => sectionProgress.value.some(s => !s.nonConforming && s.warning > 0));

function jumpToSection(sectionId: number) {
    const el = document.querySelector(`[data-section-id="${sectionId}"]`);
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function jumpToNextUnanswered() {
    const next = sectionProgress.value.find(s => !s.isNa && s.unanswered > 0);
    if (next) jumpToSection(next.sectionId);
}

function jumpToNextNc() {
    const next = sectionProgress.value.find(s => s.nonConforming > 0);
    if (next) jumpToSection(next.sectionId);
}

function jumpToNextWarning() {
    const next = sectionProgress.value.find(s => !s.nonConforming && s.warning > 0);
    if (next) jumpToSection(next.sectionId);
}

function updateCurrentSection() {
    const sections = sectionProgress.value;
    if (!sections.length) return;
    const threshold = window.innerHeight * 0.35;
    let bestId: number | null = null;
    let bestDist = Infinity;
    for (const item of sections) {
        const el = document.querySelector(`[data-section-id="${item.sectionId}"]`);
        if (!el) continue;
        const rect = el.getBoundingClientRect();
        const dist = Math.abs(rect.top - threshold);
        if (dist < bestDist) {
            bestDist = dist;
            bestId = item.sectionId;
        }
    }
    currentSectionId.value = bestId;
}

onMounted(() => {
    window.addEventListener('scroll', updateCurrentSection, { passive: true });
    updateCurrentSection();
});

onBeforeUnmount(() => {
    window.removeEventListener('scroll', updateCurrentSection);
});
</script>

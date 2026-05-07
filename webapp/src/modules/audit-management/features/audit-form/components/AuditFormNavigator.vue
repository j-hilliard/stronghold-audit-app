<template>
    <!-- ── Desktop side rail (non-narrow only) ──────────────────────────────────── -->
    <div
        v-if="sectionProgress.length > 1 && !isNarrow"
        class="flex fixed right-3 z-20 flex-col gap-0.5
               bg-slate-900/95 backdrop-blur-sm border border-slate-700/80 rounded-xl p-2 shadow-2xl
               max-h-[75vh] overflow-y-auto w-48"
        style="top: 50%; transform: translateY(-50%);"
    >
        <!-- Quick-jump bar -->
        <div class="flex gap-1 mb-1.5 pb-1.5 border-b border-slate-700/60">
            <button
                type="button"
                class="flex-1 text-[9px] font-semibold py-1 rounded transition-colors leading-tight"
                :class="noUnanswered
                    ? 'bg-slate-800 text-slate-600 cursor-default'
                    : 'bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white'"
                :title="noUnanswered ? 'All sections answered' : 'Next unanswered section'"
                @click="jumpToNextUnanswered"
            >
                Next<br>{{ noUnanswered ? '✓ Done' : 'Unanswered' }}
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

        <!-- No-match feedback -->
        <div v-if="noMatchMessage" class="text-[10px] text-slate-500 italic text-center py-1 mb-1">
            {{ noMatchMessage }}
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

    <!-- ── Mobile/tablet floating button + drawer (narrow mode only) ──────────── -->
    <template v-if="sectionProgress.length > 1 && isNarrow">
        <!-- Floating trigger button — frame-aware positioning -->
        <button
            type="button"
            class="fixed z-30 flex items-center gap-2
                   bg-slate-800 border border-slate-600 rounded-full px-3 py-2 shadow-xl
                   text-xs text-slate-300 hover:bg-slate-700 hover:text-white transition-colors"
            :style="floatingBtnStyle"
            @click="drawerOpen = true"
        >
            <i class="pi pi-list text-sm" />
            <span class="font-semibold">Sections</span>
            <span v-if="totalNc > 0" class="inline-flex items-center justify-center w-4 h-4 rounded-full bg-red-500 text-white text-[9px] font-bold">{{ totalNc }}</span>
            <span v-else-if="totalWarn > 0" class="inline-flex items-center justify-center w-4 h-4 rounded-full bg-amber-500 text-white text-[9px] font-bold">{{ totalWarn }}</span>
        </button>

        <!-- Drawer overlay -->
        <Teleport to="body">
            <Transition name="nav-drawer">
                <div v-if="drawerOpen" class="fixed inset-0 z-50 flex flex-col justify-end">
                    <!-- Backdrop -->
                    <div class="absolute inset-0 bg-black/50" @click="drawerOpen = false" />

                    <!-- Drawer panel — frame-aware width/position -->
                    <div
                        class="relative bg-slate-900 border-t border-slate-700 rounded-t-2xl max-h-[70vh] flex flex-col shadow-2xl"
                        :style="drawerPanelStyle"
                    >
                        <!-- Handle + header -->
                        <div class="flex items-center justify-between px-4 py-3 border-b border-slate-700/60 shrink-0">
                            <div class="flex items-center gap-2">
                                <div class="w-8 h-1 rounded-full bg-slate-600 mx-auto" />
                            </div>
                            <span class="text-sm font-semibold text-slate-200 flex-1 text-center">Sections</span>
                            <button type="button" class="text-slate-500 hover:text-slate-300 p-1" @click="drawerOpen = false">
                                <i class="pi pi-times text-sm" />
                            </button>
                        </div>

                        <!-- Quick-jump row -->
                        <div class="flex gap-2 px-4 py-2 border-b border-slate-700/40 shrink-0">
                            <button
                                type="button"
                                class="flex-1 text-xs py-1.5 rounded transition-colors font-semibold"
                                :class="noUnanswered
                                    ? 'bg-slate-800 text-slate-600'
                                    : 'bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white'"
                                @click="jumpToNextUnansweredAndClose"
                            >
                                {{ noUnanswered ? '✓ All answered' : 'Next unanswered' }}
                            </button>
                            <button
                                v-if="hasNcSections"
                                type="button"
                                class="flex-1 text-xs py-1.5 rounded bg-red-950/60 hover:bg-red-900/60 text-red-400 hover:text-red-200 transition-colors font-semibold"
                                @click="jumpToNextNcAndClose"
                            >
                                Next NC
                            </button>
                            <button
                                v-if="hasWarnSections"
                                type="button"
                                class="flex-1 text-xs py-1.5 rounded bg-amber-950/60 hover:bg-amber-900/60 text-amber-400 hover:text-amber-200 transition-colors font-semibold"
                                @click="jumpToNextWarningAndClose"
                            >
                                Next Warn
                            </button>
                        </div>

                        <!-- No-match feedback -->
                        <div v-if="noMatchMessage" class="text-xs text-slate-500 italic text-center py-2 px-4 border-b border-slate-700/40 shrink-0">
                            {{ noMatchMessage }}
                        </div>

                        <!-- Section list (scrollable) -->
                        <div class="overflow-y-auto flex-1 py-2">
                            <button
                                v-for="item in sectionProgress"
                                :key="item.sectionId"
                                type="button"
                                class="w-full text-left px-4 py-3 flex items-center justify-between transition-colors"
                                :class="item.sectionId === currentSectionId
                                    ? 'bg-blue-900/30 border-l-2 border-blue-500 text-blue-200'
                                    : 'hover:bg-slate-800 text-slate-300 border-l-2 border-transparent'"
                                @click="jumpToSectionAndClose(item.sectionId)"
                            >
                                <div class="min-w-0 flex-1">
                                    <p class="text-sm font-medium truncate">{{ item.name }}</p>
                                    <p class="text-xs text-slate-500 mt-0.5">
                                        <template v-if="item.isNa">
                                            <span class="text-amber-400">N/A</span>
                                        </template>
                                        <template v-else>
                                            {{ item.answered }}/{{ item.totalQuestions }} answered
                                            <span v-if="item.nonConforming > 0" class="text-red-400 font-semibold ml-1">· {{ item.nonConforming }} NC</span>
                                            <span v-else-if="item.warning > 0" class="text-amber-400 font-semibold ml-1">· {{ item.warning }} W</span>
                                            <span v-else-if="item.answered === item.totalQuestions && item.unanswered === 0" class="text-emerald-400 ml-1">· ✓</span>
                                        </template>
                                    </p>
                                </div>
                                <i class="pi pi-chevron-right text-slate-600 text-xs ml-2 shrink-0" />
                            </button>
                        </div>
                    </div>
                </div>
            </Transition>
        </Teleport>
    </template>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { useAuditSectionProgress } from '../composables/useAuditSectionProgress';
import { useNarrowScreen } from '@/composables/useNarrowScreen';
import type { SectionProgressItem } from '../composables/useAuditSectionProgress';

const { sectionProgress } = useAuditSectionProgress();
const { isNarrow, previewFrameWidth } = useNarrowScreen();

const currentSectionId = ref<number | null>(null);
const drawerOpen = ref(false);
const noMatchMessage = ref('');
let noMatchTimer: ReturnType<typeof setTimeout> | null = null;

const hasNcSections   = computed(() => sectionProgress.value.some(s => s.nonConforming > 0));
const hasWarnSections = computed(() => sectionProgress.value.some(s => !s.nonConforming && s.warning > 0));
const noUnanswered    = computed(() => sectionProgress.value.every(s => s.isNa || s.unanswered === 0));
const totalNc         = computed(() => sectionProgress.value.reduce((sum, s) => sum + s.nonConforming, 0));
const totalWarn       = computed(() => sectionProgress.value.reduce((sum, s) => sum + s.warning, 0));

// ── Frame-aware positioning ────────────────────────────────────────────────────

/**
 * Positions the floating button at bottom-[134px] (above both StickyFormActions + ScoreSummaryBar)
 * and right-aligned within the preview frame when in dev preview mode.
 * bottom-[134px] = 176px total bottom clearance minus bar heights (~42px each = ~84px), leaving buffer.
 * Chosen as: ScoreSummaryBar ~52px + StickyFormActions ~52px + 16px gap + 14px safety = 134px.
 */
const floatingBtnStyle = computed(() => {
    const fw = previewFrameWidth.value;
    if (!fw || window.innerWidth <= fw) {
        return { bottom: '134px', right: '16px' };
    }
    const left = Math.max(0, Math.round((window.innerWidth - fw) / 2));
    const rightOffset = left + 16;
    return {
        bottom: '134px',
        right: `${rightOffset}px`,
    };
});

/**
 * Constrains the drawer panel to the preview frame width/position in dev preview mode.
 */
const drawerPanelStyle = computed(() => {
    const fw = previewFrameWidth.value;
    if (!fw || window.innerWidth <= fw) return {};
    const left = Math.max(0, Math.round((window.innerWidth - fw) / 2));
    return { left: `${left}px`, right: 'auto', width: `${fw}px` };
});

// ── Section jump ───────────────────────────────────────────────────────────────

function jumpToSection(sectionId: number) {
    const el = document.querySelector(`[data-section-id="${sectionId}"]`);
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    currentSectionId.value = sectionId;
}

function jumpToSectionAndClose(sectionId: number) {
    drawerOpen.value = false;
    setTimeout(() => jumpToSection(sectionId), 200);
}

// ── Relative next-match ────────────────────────────────────────────────────────
// Starts search from the section AFTER the current one and wraps around.
// Shows a brief message if nothing matches.

function findNextFrom(predicate: (s: SectionProgressItem) => boolean): number | null {
    const sections = sectionProgress.value;
    if (!sections.length) return null;
    const currentIdx = sections.findIndex(s => s.sectionId === currentSectionId.value);
    const startIdx = currentIdx === -1 ? 0 : (currentIdx + 1) % sections.length;
    for (let i = 0; i < sections.length; i++) {
        const candidate = sections[(startIdx + i) % sections.length];
        if (predicate(candidate)) return candidate.sectionId;
    }
    return null;
}

function showNoMatch(msg: string) {
    noMatchMessage.value = msg;
    if (noMatchTimer) clearTimeout(noMatchTimer);
    noMatchTimer = setTimeout(() => { noMatchMessage.value = ''; }, 2500);
}

function jumpToNextUnanswered() {
    const id = findNextFrom(s => !s.isNa && s.unanswered > 0);
    if (id !== null) jumpToSection(id);
    else showNoMatch('All sections answered.');
}

function jumpToNextNc() {
    const id = findNextFrom(s => s.nonConforming > 0);
    if (id !== null) jumpToSection(id);
    else showNoMatch('No more NC sections.');
}

function jumpToNextWarning() {
    const id = findNextFrom(s => !s.nonConforming && s.warning > 0);
    if (id !== null) jumpToSection(id);
    else showNoMatch('No more warning sections.');
}

// Close-and-jump variants for the mobile drawer
function jumpToNextUnansweredAndClose() { drawerOpen.value = false; setTimeout(jumpToNextUnanswered, 200); }
function jumpToNextNcAndClose()         { drawerOpen.value = false; setTimeout(jumpToNextNc, 200); }
function jumpToNextWarningAndClose()    { drawerOpen.value = false; setTimeout(jumpToNextWarning, 200); }

// ── Current-section scroll tracking ───────────────────────────────────────────

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
    if (noMatchTimer) clearTimeout(noMatchTimer);
});
</script>

<style scoped>
.nav-drawer-enter-active,
.nav-drawer-leave-active {
    transition: opacity 0.2s ease;
}
.nav-drawer-enter-active .relative,
.nav-drawer-leave-active .relative {
    transition: transform 0.25s ease;
}
.nav-drawer-enter-from,
.nav-drawer-leave-to {
    opacity: 0;
}
.nav-drawer-enter-from .relative,
.nav-drawer-leave-to .relative {
    transform: translateY(100%);
}
</style>

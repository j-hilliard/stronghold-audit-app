<template>
    <div
        class="fixed bottom-0 z-50 bg-slate-900 border-t border-slate-700 shadow-2xl"
        :style="barStyle"
    >
        <div :class="isNarrow ? 'px-3 py-1.5 flex items-center gap-2' : 'max-w-screen-xl mx-auto px-4 py-2 flex items-center gap-4 flex-wrap'">

            <!-- Count pills -->
            <div class="flex items-center gap-1.5 text-xs flex-wrap">
                <span class="bg-emerald-900 text-emerald-300 border border-emerald-700 rounded px-1.5 py-0.5 font-semibold">
                    C: {{ counts.conforming }}
                </span>
                <span class="bg-red-900 text-red-300 border border-red-700 rounded px-1.5 py-0.5 font-semibold">
                    NC: {{ counts.nonConforming }}
                </span>
                <span class="bg-amber-900 text-amber-300 border border-amber-700 rounded px-1.5 py-0.5 font-semibold">
                    W: {{ counts.warning }}
                </span>
                <span class="bg-slate-700 text-slate-300 border border-slate-600 rounded px-1.5 py-0.5 font-semibold">
                    N/A: {{ counts.na }}
                </span>
                <span v-if="counts.unanswered > 0 && !isNarrow" class="text-slate-500 text-xs">
                    {{ counts.unanswered }} unanswered
                </span>
            </div>

            <!-- Spacer -->
            <div class="flex-1" />

            <!-- Score -->
            <div class="flex items-center gap-2">
                <div class="text-right">
                    <div v-if="!isNarrow" class="text-xs text-slate-400 leading-none mb-0.5">Score</div>
                    <div
                        :class="[
                            'font-bold leading-none',
                            isNarrow ? 'text-lg' : 'text-2xl',
                            scoreColor,
                        ]"
                    >
                        {{ scoreDisplay }}
                    </div>
                </div>

                <!-- Visual bar — hidden on narrow to save space -->
                <div
                    v-if="scorePercent !== null && !isNarrow"
                    class="w-24 h-3 bg-slate-700 rounded-full overflow-hidden"
                    :title="`${scoreDisplay}`"
                >
                    <div
                        class="h-full rounded-full transition-all duration-500"
                        :class="barColor"
                        :style="{ width: `${scorePercent}%` }"
                    />
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { ScoreCounts } from '@/modules/audit-management/stores/auditStore';
import { useNarrowScreen } from '@/composables/useNarrowScreen';

const props = defineProps<{
    counts: ScoreCounts;
    scorePercent: number | null;
}>();

const { isNarrow, previewFrameWidth } = useNarrowScreen();

/** Constrain bar to the preview frame when dev viewport preview is active. */
const barStyle = computed(() => {
    const fw = previewFrameWidth.value;
    if (!fw || window.innerWidth <= fw) {
        return { left: '0', right: '0' };
    }
    const left = Math.max(0, Math.round((window.innerWidth - fw) / 2));
    return { left: `${left}px`, right: 'auto', width: `${fw}px` };
});

const scoreDisplay = computed(() => {
    if (props.scorePercent === null) return '—';
    return `${Math.round(props.scorePercent)}%`;
});

const scoreColor = computed(() => {
    if (props.scorePercent === null) return 'text-slate-400';
    if (props.scorePercent >= 90) return 'text-emerald-400';
    if (props.scorePercent >= 75) return 'text-amber-400';
    return 'text-red-400';
});

const barColor = computed(() => {
    if (props.scorePercent === null) return 'bg-slate-500';
    if (props.scorePercent >= 90) return 'bg-emerald-500';
    if (props.scorePercent >= 75) return 'bg-amber-500';
    return 'bg-red-500';
});
</script>

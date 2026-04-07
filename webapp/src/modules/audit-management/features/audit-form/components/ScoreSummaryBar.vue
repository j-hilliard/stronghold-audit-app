<template>
    <div class="fixed bottom-0 left-0 right-0 z-50 bg-slate-900 border-t border-slate-700 shadow-2xl">
        <div class="max-w-screen-xl mx-auto px-4 py-2 flex items-center gap-4 flex-wrap">

            <!-- Count pills -->
            <div class="flex items-center gap-2 text-xs flex-wrap">
                <span class="bg-emerald-900 text-emerald-300 border border-emerald-700 rounded px-2 py-0.5 font-semibold">
                    C: {{ counts.conforming }}
                </span>
                <span class="bg-red-900 text-red-300 border border-red-700 rounded px-2 py-0.5 font-semibold">
                    NC: {{ counts.nonConforming }}
                </span>
                <span class="bg-amber-900 text-amber-300 border border-amber-700 rounded px-2 py-0.5 font-semibold">
                    W: {{ counts.warning }}
                </span>
                <span class="bg-slate-700 text-slate-300 border border-slate-600 rounded px-2 py-0.5 font-semibold">
                    N/A: {{ counts.na }}
                </span>
                <span v-if="counts.unanswered > 0" class="text-slate-500 text-xs">
                    {{ counts.unanswered }} unanswered
                </span>
            </div>

            <!-- Spacer -->
            <div class="flex-1" />

            <!-- Score -->
            <div class="flex items-center gap-3">
                <div class="text-right">
                    <div class="text-xs text-slate-400 leading-none mb-0.5">Score</div>
                    <div
                        :class="[
                            'text-2xl font-bold leading-none',
                            scoreColor,
                        ]"
                    >
                        {{ scoreDisplay }}
                    </div>
                </div>

                <!-- Visual bar -->
                <div
                    v-if="scorePercent !== null"
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

const props = defineProps<{
    counts: ScoreCounts;
    scorePercent: number | null;
}>();

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

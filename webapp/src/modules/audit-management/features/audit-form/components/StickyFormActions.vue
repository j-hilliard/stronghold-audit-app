<template>
    <!-- Sits just above ScoreSummaryBar (fixed bottom-0) -->
    <Teleport to="body">
        <Transition name="slide-up">
            <div
                v-if="visible"
                class="fixed bottom-14 z-40 pointer-events-none"
                :style="barStyle"
            >
                <div :class="isNarrow ? '' : 'max-w-screen-xl mx-auto px-4'">
                    <div class="pointer-events-auto flex items-center justify-between gap-2 bg-slate-900/95 backdrop-blur-sm border border-slate-700/70 rounded-t-xl px-3 py-2 shadow-elevation-4">
                        <!-- Left: status (hidden on narrow) -->
                        <div v-if="!isNarrow" class="flex items-center gap-2 text-xs text-slate-500 hidden sm:flex">
                            <i class="pi pi-pencil text-slate-600" />
                            <span>{{ statusLabel }}</span>
                        </div>

                        <!-- Right: actions -->
                        <div class="flex items-center gap-2 ml-auto">
                            <Button
                                :label="isReviewerMode ? 'Save Changes' : (isNarrow ? 'Save' : 'Save Draft')"
                                icon="pi pi-save"
                                severity="secondary"
                                size="small"
                                outlined
                                :loading="saving"
                                @click="$emit('save')"
                            />
                            <Button
                                v-if="canSubmit && !isReviewerMode"
                                :label="isNarrow ? 'Submit' : 'Submit for Review'"
                                icon="pi pi-send"
                                size="small"
                                :loading="saving"
                                @click="$emit('submit')"
                            />
                            <!-- Delete hidden on narrow — user can delete from the header -->
                            <Button
                                v-if="!isReviewerMode && !isNarrow"
                                label="Delete"
                                icon="pi pi-trash"
                                severity="danger"
                                size="small"
                                text
                                :loading="saving"
                                @click="$emit('delete')"
                            />
                        </div>
                    </div>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import Button from 'primevue/button';
import { useNarrowScreen } from '@/composables/useNarrowScreen';

withDefaults(defineProps<{
    visible?: boolean;
    saving?: boolean;
    canSubmit?: boolean;
    statusLabel?: string;
    isReviewerMode?: boolean;
}>(), {
    visible: true,
    saving: false,
    canSubmit: false,
    statusLabel: 'Editing draft',
    isReviewerMode: false,
});

defineEmits<{ save: []; submit: []; delete: [] }>();

const { isNarrow, previewFrameWidth } = useNarrowScreen();

/** Constrain the bar to the preview frame when dev viewport preview is active. */
const barStyle = computed(() => {
    const fw = previewFrameWidth.value;
    if (!fw || window.innerWidth <= fw) {
        return { left: '0', right: '0' };
    }
    const left = Math.max(0, Math.round((window.innerWidth - fw) / 2));
    return { left: `${left}px`, right: 'auto', width: `${fw}px` };
});
</script>

<style scoped>
.slide-up-enter-active, .slide-up-leave-active {
    transition: transform 0.2s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.2s ease;
}
.slide-up-enter-from, .slide-up-leave-to {
    transform: translateY(100%);
    opacity: 0;
}
</style>

<template>
    <!-- Sits just above ScoreSummaryBar (fixed bottom-0) -->
    <Teleport to="body">
        <Transition name="slide-up">
            <div
                v-if="visible"
                class="fixed bottom-14 left-0 right-0 z-40 pointer-events-none"
            >
                <div class="max-w-screen-xl mx-auto px-4">
                    <div class="pointer-events-auto flex items-center justify-between gap-3 bg-slate-900/95 backdrop-blur-sm border border-slate-700/70 rounded-t-xl px-4 py-2.5 shadow-elevation-4">
                        <!-- Left: status -->
                        <div class="flex items-center gap-2 text-xs text-slate-500 hidden sm:flex">
                            <i class="pi pi-pencil text-slate-600" />
                            <span>{{ statusLabel }}</span>
                        </div>

                        <!-- Right: actions -->
                        <div class="flex items-center gap-2 ml-auto">
                            <Button
                                label="Save Draft"
                                icon="pi pi-save"
                                severity="secondary"
                                size="small"
                                outlined
                                :loading="saving"
                                @click="$emit('save')"
                            />
                            <Button
                                v-if="canSubmit"
                                label="Submit for Review"
                                icon="pi pi-send"
                                size="small"
                                :loading="saving"
                                @click="$emit('submit')"
                            />
                            <Button
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
import Button from 'primevue/button';

withDefaults(defineProps<{
    visible?: boolean;
    saving?: boolean;
    canSubmit?: boolean;
    statusLabel?: string;
}>(), {
    visible: true,
    saving: false,
    canSubmit: false,
    statusLabel: 'Editing draft',
});

defineEmits<{ save: []; submit: []; delete: [] }>();
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

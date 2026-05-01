<template>
    <div class="reviewer-context-banner">
        <div class="reviewer-banner-inner">
            <i class="pi pi-pencil text-blue-400 text-sm shrink-0" />
            <div class="flex-1 min-w-0">
                <span class="font-semibold text-blue-300 text-sm">Review Mode</span>
                <span class="text-blue-400/80 text-sm"> — Editing Responses</span>
                <span v-if="auditRef" class="text-blue-500/60 text-xs ml-2">· {{ auditRef }}</span>
            </div>
            <Button
                label="Back to Review"
                icon="pi pi-arrow-left"
                severity="info"
                size="small"
                outlined
                @click="$emit('back')"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button';

defineProps<{
    auditRef?: string | null;
}>();

defineEmits<{
    (e: 'back'): void;
}>();
</script>

<style scoped>
.reviewer-context-banner {
    position: sticky;
    top: 0;
    z-index: 40;
    border-bottom: 1px solid rgba(59, 130, 246, 0.25);
    background: rgba(15, 23, 42, 0.96);
    backdrop-filter: blur(8px);
    padding: 8px 16px;
}

.reviewer-banner-inner {
    display: flex;
    align-items: center;
    gap: 10px;
    border-radius: 8px;
    border: 1px solid rgba(59, 130, 246, 0.35);
    background: rgba(59, 130, 246, 0.07);
    padding: 8px 12px;
}
</style>

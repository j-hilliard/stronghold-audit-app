<template>
    <BasePageHeader
        title="Email Routing"
        subtitle="Configure which recipients receive audit review notifications per division"
        icon="pi pi-envelope"
    />
    <div class="p-4">
        <div class="bg-slate-800 border border-slate-700 rounded-lg p-6 text-slate-400 text-center">
            <i class="pi pi-envelope text-4xl mb-3 block text-slate-500" />
            <p class="text-lg font-medium text-slate-300 mb-1">Email Routing Settings</p>
            <p class="text-sm">Per-division email recipient configuration — coming in Phase 5</p>
            <p class="text-xs mt-2 text-amber-500/70">Admin only</p>
        </div>
    </div>
</template>

<script setup lang="ts">
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
</script>

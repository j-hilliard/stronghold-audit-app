<template>
    <BasePageHeader
        title="New Audit"
        subtitle="Select a division to begin a new compliance audit"
        icon="pi pi-plus"
    />
    <div class="p-4">
        <div class="bg-slate-800 border border-slate-700 rounded-lg p-6 text-slate-400 text-center">
            <i class="pi pi-plus-circle text-4xl mb-3 block text-slate-500" />
            <p class="text-lg font-medium text-slate-300 mb-1">New Audit</p>
            <p class="text-sm">Division selector and audit creation form — coming in Phase 4</p>
        </div>
    </div>
</template>

<script setup lang="ts">
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
</script>

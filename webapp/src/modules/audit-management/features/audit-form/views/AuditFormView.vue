<template>
    <BasePageHeader
        title="Audit Form"
        subtitle="Complete the compliance checklist"
        icon="pi pi-clipboard"
    />
    <div class="p-4">
        <div class="bg-slate-800 border border-slate-700 rounded-lg p-6 text-slate-400 text-center">
            <i class="pi pi-list-check text-4xl mb-3 block text-slate-500" />
            <p class="text-lg font-medium text-slate-300 mb-1">Audit Form Engine</p>
            <p class="text-sm">Dynamic checklist, status buttons, scoring, save/submit — coming in Phase 4</p>
            <p class="text-xs mt-2 text-slate-500">Audit ID: {{ route.params.id }}</p>
        </div>
    </div>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';

const route = useRoute();
</script>

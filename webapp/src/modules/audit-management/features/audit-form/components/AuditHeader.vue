<template>
    <Card class="mb-4">
        <template #title>
            <div class="flex items-center justify-between gap-4 w-full">
                <span class="text-base font-semibold text-white">Audit Information</span>
                <div v-if="displayAuditNumber" class="flex items-center gap-2">
                    <span class="text-xs text-slate-400 uppercase tracking-wide">Audit #</span>
                    <span class="font-mono text-sm font-bold text-blue-300 bg-blue-950/50 border border-blue-800/40 px-2 py-0.5 rounded">
                        {{ displayAuditNumber }}
                    </span>
                </div>
            </div>
        </template>
        <template #content>

            <!-- ── Job Site fields ─────────────────────────────────────────── -->
            <template v-if="auditType === 'JobSite'">
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                    <BaseFormField label="Job Number *">
                        <InputText v-model="header.jobNumber" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                    <BaseFormField label="Date *">
                        <InputText v-model="header.auditDate" type="date" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                    <BaseFormField label="Client *">
                        <InputText v-model="header.client" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                    <BaseFormField label="Location *">
                        <InputText v-model="header.location" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                    <BaseFormField label="Unit">
                        <InputText v-model="header.unit" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                    <BaseFormField label="Time">
                        <InputText v-model="header.time" type="time" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                    <BaseFormField label="Shift">
                        <Dropdown
                            v-model="header.shift"
                            :options="SHIFTS"
                            placeholder="Select shift"
                            class="w-full"
                            :disabled="disabled"
                        />
                    </BaseFormField>
                    <BaseFormField label="Project Manager *">
                        <InputText v-model="header.pm" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                    <BaseFormField label="Auditor *">
                        <InputText v-model="header.auditor" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                    <BaseFormField label="Work Description" class="md:col-span-2">
                        <Textarea
                            v-model="header.workDescription"
                            rows="2"
                            class="w-full"
                            :disabled="disabled"
                            autoResize
                        />
                    </BaseFormField>
                    <BaseFormField label="Customer Code">
                        <InputText
                            v-model="header.siteCode"
                            class="w-full font-mono uppercase"
                            :disabled="disabled"
                            maxlength="10"
                            placeholder="e.g. IPT"
                            @input="header.siteCode = (($event.target as HTMLInputElement).value ?? '').toUpperCase()"
                        />
                        <small class="text-slate-500">3-char site code appended to Audit #</small>
                    </BaseFormField>
                </div>
            </template>

            <!-- ── Facility fields ─────────────────────────────────────────── -->
            <template v-else>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                    <BaseFormField label="Company *">
                        <InputText v-model="header.company1" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                    <BaseFormField label="Location *">
                        <InputText v-model="header.location" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                    <BaseFormField label="Date *">
                        <InputText v-model="header.auditDate" type="date" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <BaseFormField label="Responsible Party *">
                        <InputText v-model="header.responsibleParty" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                    <BaseFormField label="Auditor *">
                        <InputText v-model="header.auditor" class="w-full" :disabled="disabled" />
                    </BaseFormField>
                </div>
            </template>

        </template>
    </Card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import InputText from 'primevue/inputtext';
import Dropdown from 'primevue/dropdown';
import Textarea from 'primevue/textarea';
import Card from 'primevue/card';
import BaseFormField from '@/components/forms/BaseFormField.vue';
import type { AuditHeaderDto } from '@/apiclient/auditClient';

const props = defineProps<{
    header: AuditHeaderDto;
    /** "JobSite" | "Facility" */
    auditType: string;
    disabled?: boolean;
    trackingNumber?: string | null;
}>();

const SHIFTS = ['Day', 'Evening', 'Night'];

// Live-computed display: base (auto-generated part) + current customer code
const displayAuditNumber = computed(() => {
    if (!props.trackingNumber) return null;
    // Extract base = first two dash-segments, e.g. "H26" + "003" → "H26-003"
    const parts = props.trackingNumber.split('-');
    const base = parts.slice(0, 2).join('-');
    const site = props.header.siteCode?.trim().toUpperCase();
    return site ? `${base}-${site}` : base;
});
</script>

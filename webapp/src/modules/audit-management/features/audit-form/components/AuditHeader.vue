<template>
    <Card class="mb-4">
        <template #title>
            <span class="text-base font-semibold text-white">Audit Information</span>
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
                <div class="mt-4">
                    <BaseFormField label="Work Description">
                        <Textarea
                            v-model="header.workDescription"
                            rows="2"
                            class="w-full"
                            :disabled="disabled"
                            autoResize
                        />
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
import InputText from 'primevue/inputtext';
import Dropdown from 'primevue/dropdown';
import Textarea from 'primevue/textarea';
import Card from 'primevue/card';
import BaseFormField from '@/components/forms/BaseFormField.vue';
import type { AuditHeaderDto } from '@/apiclient/auditClient';

defineProps<{
    header: AuditHeaderDto;
    /** "JobSite" | "Facility" */
    auditType: string;
    disabled?: boolean;
}>();

const SHIFTS = ['Day', 'Evening', 'Night'];
</script>

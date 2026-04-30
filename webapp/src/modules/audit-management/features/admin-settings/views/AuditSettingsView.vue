<template>
    <div>
        <BasePageHeader
            title="Admin Settings"
            subtitle="Email routing, user role assignments, and division score targets"
            icon="pi pi-cog"
        />

        <div class="p-4 max-w-6xl">
            <!-- Tab bar -->
            <div class="flex gap-0.5 mb-5 border-b border-slate-700">
                <button
                    v-for="tab in TABS"
                    :key="tab.key"
                    @click="activeTab = tab.key"
                    class="px-5 py-2.5 text-sm font-medium transition-colors rounded-t -mb-px"
                    :class="activeTab === tab.key
                        ? 'border border-slate-700 border-b-transparent bg-slate-800 text-blue-400'
                        : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50'"
                >
                    {{ tab.label }}
                </button>
            </div>

            <div v-if="adminStore.loading" class="text-slate-400 text-sm">Loading…</div>

            <!-- ── EMAIL ROUTING TAB ── -->
            <template v-else-if="activeTab === 'email'">
                <!-- Toolbar -->
                <div class="flex flex-wrap items-center gap-2 mb-4">
                    <p class="text-slate-400 text-sm flex-1 min-w-0">
                        Active recipients receive a review email when an audit is submitted for their division.
                    </p>
                    <div class="flex items-center gap-2 shrink-0">
                        <select
                            v-model="selectedDivisionCode"
                            class="bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 focus:outline-none focus:border-blue-500"
                        >
                            <option v-for="g in divisionGroups" :key="g.divisionCode" :value="g.divisionCode">
                                {{ g.divisionName }}
                            </option>
                        </select>
                        <button
                            @click="addRow(activeDivisionGroup!)"
                            :disabled="!activeDivisionGroup"
                            class="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-slate-700 hover:bg-slate-600 text-slate-200 rounded disabled:opacity-40"
                        >
                            <i class="pi pi-user-plus text-xs" /> Add Person
                        </button>
                        <button
                            @click="deactivateAll"
                            :disabled="adminStore.saving"
                            class="px-3 py-1.5 text-sm bg-amber-700 hover:bg-amber-600 text-white rounded disabled:opacity-40"
                            title="Uncheck all recipients — prevents real emails during testing"
                        >
                            Deactivate All
                        </button>
                        <button
                            @click="onSave"
                            :disabled="adminStore.saving || !isDirty"
                            class="px-4 py-1.5 text-sm bg-blue-700 hover:bg-blue-600 text-white rounded disabled:opacity-40"
                        >
                            {{ adminStore.saving ? 'Saving…' : 'Save Changes' }}
                        </button>
                    </div>
                </div>

                <!-- Recipient table for selected division -->
                <div v-if="activeDivisionGroup" class="bg-slate-800 border border-slate-700 rounded-lg overflow-x-auto">
                    <table class="w-full text-sm" style="min-width: 420px;">
                        <thead>
                            <tr class="border-b border-slate-700 text-slate-400 text-xs uppercase tracking-wide">
                                <th class="px-4 py-2 text-left">Name</th>
                                <th class="px-4 py-2 text-left">Email</th>
                                <th class="px-4 py-2 text-center w-24">Active</th>
                                <th class="px-4 py-2 w-10" />
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-slate-700/50">
                            <tr
                                v-for="(rule, idx) in activeDivisionGroup.rules"
                                :key="idx"
                                class="hover:bg-slate-700/30 transition-colors"
                            >
                                <td class="px-4 py-2 text-slate-200">
                                    {{ rule.displayName || '—' }}
                                </td>
                                <td class="px-4 py-2 text-slate-400">{{ rule.emailAddress }}</td>
                                <td class="px-4 py-2 text-center">
                                    <input
                                        v-model="rule.isActive"
                                        @change="isDirty = true"
                                        type="checkbox"
                                        class="accent-blue-500 w-4 h-4"
                                    />
                                </td>
                                <td class="px-4 py-2 text-center">
                                    <button
                                        @click="removeRow(activeDivisionGroup!, idx)"
                                        class="text-red-400 hover:text-red-300 pi pi-trash text-xs"
                                        title="Remove"
                                    />
                                </td>
                            </tr>
                            <tr v-if="!activeDivisionGroup.rules.length">
                                <td colspan="4" class="px-4 py-6 text-center text-slate-500 italic text-sm">
                                    No recipients configured for {{ activeDivisionGroup.divisionName }}.
                                    <button @click="addRow(activeDivisionGroup!)" class="ml-2 text-blue-400 hover:text-blue-300 underline">Add one</button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div v-if="!divisionGroups.length" class="text-slate-500 text-sm">
                    No divisions found.
                </div>
            </template>

            <!-- ── ADD RECIPIENT DIALOG ── -->
            <div
                v-if="addDialog.open"
                class="fixed inset-0 bg-black/60 flex items-center justify-center z-50"
                @click.self="closeAddDialog"
            >
                <div class="bg-slate-800 border border-slate-700 rounded-lg shadow-xl w-full max-w-md mx-4 p-6">
                    <h3 class="text-base font-semibold text-slate-100 mb-4">Add Recipient — {{ addDialog.divisionName }}</h3>
                    <div class="space-y-3">
                        <div class="grid grid-cols-2 gap-3">
                            <div>
                                <label class="block text-xs text-slate-400 mb-1">First Name</label>
                                <input
                                    v-model="addDialog.firstName"
                                    @input="autoFillEmail"
                                    type="text"
                                    placeholder="Joseph"
                                    class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                                />
                            </div>
                            <div>
                                <label class="block text-xs text-slate-400 mb-1">Last Name</label>
                                <input
                                    v-model="addDialog.lastName"
                                    @input="autoFillEmail"
                                    type="text"
                                    placeholder="Hilliard"
                                    class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                                />
                            </div>
                        </div>
                        <div>
                            <label class="block text-xs text-slate-400 mb-1">Company</label>
                            <select
                                v-model="addDialog.company"
                                @change="autoFillEmail"
                                class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 focus:outline-none focus:border-blue-500"
                            >
                                <option value="">— Select Company —</option>
                                <option v-for="c in COMPANIES" :key="c.name" :value="c.name">{{ c.name }}</option>
                            </select>
                        </div>
                        <div>
                            <label class="block text-xs text-slate-400 mb-1">Email Address</label>
                            <input
                                v-model="addDialog.email"
                                type="email"
                                placeholder="firstname.lastname@company.com"
                                class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1.5 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                            />
                        </div>
                    </div>
                    <div class="flex justify-end gap-2 mt-5">
                        <button
                            @click="closeAddDialog"
                            class="px-4 py-1.5 text-sm text-slate-400 hover:text-slate-200"
                        >Cancel</button>
                        <button
                            @click="confirmAddRow"
                            :disabled="!addDialog.email.trim()"
                            class="px-4 py-1.5 text-sm bg-blue-700 hover:bg-blue-600 text-white rounded disabled:opacity-40"
                        >Add Recipient</button>
                    </div>
                </div>
            </div>

            <!-- ── SCORE TARGETS TAB ── -->
            <template v-else-if="activeTab === 'targets'">
                <p class="text-slate-400 text-sm mb-4">
                    Configure compliance targets, audit schedules, and escalation rules per division.
                </p>

                <div class="bg-slate-800 border border-slate-700 rounded-lg overflow-hidden">
                    <table class="w-full text-sm table-fixed">
                        <thead>
                            <!-- Section group headers -->
                            <tr class="bg-slate-900/60 border-b border-slate-700/50">
                                <th class="px-3 py-2 text-left w-40" />
                                <th colspan="2" class="px-3 py-2 text-center text-[10px] font-semibold text-blue-400 uppercase tracking-widest border-l border-slate-700/40">
                                    <i class="pi pi-chart-line mr-1 text-[9px]" />Score Targets
                                </th>
                                <th colspan="1" class="px-3 py-2 text-center text-[10px] font-semibold text-emerald-400 uppercase tracking-widest border-l border-slate-700/40">
                                    <i class="pi pi-check-circle mr-1 text-[9px]" />Compliance
                                </th>
                                <th colspan="1" class="px-3 py-2 text-center text-[10px] font-semibold text-amber-400 uppercase tracking-widest border-l border-slate-700/40">
                                    <i class="pi pi-exclamation-triangle mr-1 text-[9px]" />Escalation
                                </th>
                            </tr>
                            <!-- Column headers -->
                            <tr class="border-b border-slate-700 text-slate-400 text-xs uppercase tracking-wide">
                                <th class="px-3 py-3 text-left w-40">Division</th>
                                <th class="px-3 py-3 text-center w-44 border-l border-slate-700/40">Score Target (%)</th>
                                <th class="px-3 py-3 text-center w-44">Audit Frequency (days)</th>
                                <th class="px-3 py-3 text-center w-36 border-l border-slate-700/40" title="Require at least one photo when closing a corrective action">Closure Photo</th>
                                <th class="px-3 py-3 text-center border-l border-slate-700/40" title="SLA due-date window per priority level">SLA (Normal / Urgent / Escalate days + Email)</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-slate-700/50">
                            <tr
                                v-for="row in targetRows"
                                :key="row.divisionId"
                                class="hover:bg-slate-700/30 transition-colors"
                            >
                                <td class="px-3 py-3 text-slate-200">
                                    <div>{{ row.divisionName }}</div>
                                    <div class="text-slate-500 font-mono text-[11px] mt-0.5">{{ row.divisionCode }}</div>
                                </td>
                                <td class="px-3 py-3">
                                    <div class="flex items-center justify-center gap-2">
                                        <input
                                            v-model.number="row.pendingTarget"
                                            type="number"
                                            min="0"
                                            max="100"
                                            step="1"
                                            placeholder="—"
                                            class="w-16 text-center bg-slate-700 border border-slate-600 rounded px-2 py-1 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                                        />
                                        <span class="text-slate-400 text-xs">%</span>
                                        <button
                                            @click="saveTarget(row)"
                                            :disabled="row.saving || row.pendingTarget === (row.scoreTarget ?? null)"
                                            class="px-2.5 py-1 text-xs bg-blue-700 hover:bg-blue-600 text-white rounded disabled:opacity-40 transition-colors whitespace-nowrap"
                                        >
                                            {{ row.saving ? 'Saving…' : 'Save' }}
                                        </button>
                                    </div>
                                </td>
                                <td class="px-3 py-3">
                                    <div class="flex items-center justify-center gap-2">
                                        <input
                                            v-model.number="row.pendingFrequency"
                                            type="number"
                                            min="1"
                                            step="1"
                                            placeholder="—"
                                            class="w-16 text-center bg-slate-700 border border-slate-600 rounded px-2 py-1 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                                        />
                                        <span class="text-slate-400 text-xs">days</span>
                                        <button
                                            @click="saveFrequency(row)"
                                            :disabled="row.savingFrequency || (row.pendingFrequency ?? null) === (row.auditFrequencyDays ?? null)"
                                            class="px-2.5 py-1 text-xs bg-blue-700 hover:bg-blue-600 text-white rounded disabled:opacity-40 transition-colors whitespace-nowrap"
                                        >
                                            {{ row.savingFrequency ? 'Saving…' : 'Save' }}
                                        </button>
                                    </div>
                                </td>
                                <td class="px-3 py-3 text-center">
                                    <label class="inline-flex items-center gap-2 cursor-pointer">
                                        <input
                                            type="checkbox"
                                            :checked="row.requireClosurePhoto"
                                            :disabled="row.savingClosurePhoto"
                                            class="w-4 h-4 accent-blue-500 cursor-pointer"
                                            @change="toggleClosurePhoto(row, ($event.target as HTMLInputElement).checked)"
                                        />
                                        <span v-if="row.savingClosurePhoto" class="text-xs text-slate-400">Saving…</span>
                                    </label>
                                </td>
                                <!-- SLA configuration -->
                                <td class="px-3 py-2">
                                    <div class="flex flex-col gap-1.5">
                                        <div class="flex items-center gap-1.5">
                                            <span class="text-[10px] text-slate-500 w-14 shrink-0">Normal</span>
                                            <input
                                                v-model.number="row.pendingSlaNormal"
                                                type="number"
                                                min="1"
                                                step="1"
                                                placeholder="14"
                                                class="w-14 text-center bg-slate-700 border border-slate-600 rounded px-1.5 py-0.5 text-xs text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                                            />
                                            <span class="text-[10px] text-slate-500">d</span>
                                            <span class="text-[10px] text-slate-500 w-10 shrink-0 ml-1">Urgent</span>
                                            <input
                                                v-model.number="row.pendingSlaUrgent"
                                                type="number"
                                                min="1"
                                                step="1"
                                                placeholder="7"
                                                class="w-14 text-center bg-slate-700 border border-slate-600 rounded px-1.5 py-0.5 text-xs text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                                            />
                                            <span class="text-[10px] text-slate-500">d</span>
                                        </div>
                                        <div class="flex items-center gap-1.5">
                                            <span class="text-[10px] text-slate-500 w-14 shrink-0">Escalate</span>
                                            <input
                                                v-model.number="row.pendingSlaEscalate"
                                                type="number"
                                                min="1"
                                                step="1"
                                                placeholder="—"
                                                class="w-14 text-center bg-slate-700 border border-slate-600 rounded px-1.5 py-0.5 text-xs text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                                            />
                                            <span class="text-[10px] text-slate-500 shrink-0">d via</span>
                                            <input
                                                v-model="row.pendingEscalationEmail"
                                                type="email"
                                                placeholder="email"
                                                class="flex-1 bg-slate-700 border border-slate-600 rounded px-1.5 py-0.5 text-xs text-slate-200 placeholder-slate-500 focus:outline-none focus:border-blue-500"
                                            />
                                        </div>
                                        <button
                                            @click="saveSla(row)"
                                            :disabled="row.savingSla"
                                            class="self-end px-2.5 py-0.5 text-xs bg-blue-700 hover:bg-blue-600 text-white rounded disabled:opacity-40 transition-colors whitespace-nowrap"
                                        >
                                            {{ row.savingSla ? 'Saving…' : 'Save SLA' }}
                                        </button>
                                    </div>
                                </td>
                            </tr>
                            <tr v-if="!targetRows.length">
                                <td colspan="5" class="px-4 py-6 text-center text-slate-500 italic">Loading divisions…</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </template>

            <!-- ── USER ROLES TAB ── -->
            <template v-else-if="activeTab === 'roles'">
                <div class="flex items-center justify-between mb-4">
                    <p class="text-slate-400 text-sm">
                        Assign one audit role per user. Changes save immediately on selection.
                    </p>
                    <button
                        @click="adminStore.loadUserAuditRoles()"
                        :disabled="adminStore.loading"
                        class="px-3 py-1.5 text-sm bg-slate-700 hover:bg-slate-600 text-white rounded disabled:opacity-40"
                    >
                        <i class="pi pi-refresh mr-1" /> Refresh
                    </button>
                </div>

                <!-- Role legend -->
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 mb-4">
                    <div v-for="role in AUDIT_ROLES" :key="role.value"
                        class="flex items-start gap-2 px-3 py-2 rounded-lg bg-slate-800/50 border border-slate-700/50">
                        <i class="pi pi-info-circle text-slate-500 text-xs mt-0.5 flex-shrink-0" />
                        <div>
                            <div class="text-xs font-semibold text-slate-300">{{ role.label }}</div>
                            <div class="text-[11px] text-slate-500 mt-0.5 leading-tight">{{ role.desc }}</div>
                        </div>
                    </div>
                </div>

                <div class="bg-slate-800 border border-slate-700 rounded-lg overflow-x-auto">
                    <table class="w-full text-sm" style="min-width: 500px;">
                        <thead>
                            <tr class="border-b border-slate-700 text-slate-400 text-xs uppercase tracking-wide">
                                <th class="px-4 py-3 text-left">Name</th>
                                <th class="px-4 py-3 text-left">Email</th>
                                <th class="px-4 py-3 text-left w-56">Audit Role</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-slate-700/50">
                            <tr v-for="user in adminStore.userAuditRoles" :key="user.userId"
                                class="hover:bg-slate-700/30 transition-colors">
                                <td class="px-4 py-2 text-slate-200">
                                    {{ [user.firstName, user.lastName].filter(Boolean).join(' ') || '—' }}
                                </td>
                                <td class="px-4 py-2 text-slate-400">{{ user.email || '—' }}</td>
                                <td class="px-4 py-2">
                                    <select
                                        :value="user.auditRole ?? ''"
                                        @change="onRoleChange(user.userId, ($event.target as HTMLSelectElement).value)"
                                        :disabled="adminStore.saving"
                                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-sm text-slate-200 focus:outline-none focus:border-blue-500 disabled:opacity-50"
                                    >
                                        <option value="">— None —</option>
                                        <option v-for="role in AUDIT_ROLES" :key="role.value" :value="role.value" :title="role.desc">
                                            {{ role.label }}
                                        </option>
                                    </select>
                                </td>
                            </tr>
                            <tr v-if="!adminStore.userAuditRoles.length">
                                <td colspan="3" class="px-4 py-6 text-center text-slate-500 italic">No users found.</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </template>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAdminStore } from '../../../stores/adminStore';
import type { EmailRoutingRuleUpsertDto } from '@/apiclient/auditClient';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import { useToast } from 'primevue/usetoast';

const adminStore = useAdminStore();
const toast = useToast();
const service = useAuditService();

const TABS = [
    { key: 'email',   label: 'Email Routing' },
    { key: 'targets', label: 'Score Targets' },
    { key: 'roles',   label: 'User Roles' },
] as const;
type TabKey = typeof TABS[number]['key'];
const activeTab = ref<TabKey>('email');

const AUDIT_ROLES = [
    { value: 'TemplateAdmin',          label: 'Template Admin',             desc: 'Full admin — creates and edits audit templates, manages all settings' },
    { value: 'AuditManager',           label: 'Audit Manager',              desc: 'Creates, saves, and submits audits; manages corrective actions' },
    { value: 'AuditReviewer',          label: 'Audit Reviewer',             desc: 'Reviews submitted audits, closes/reopens, sends distribution emails' },
    { value: 'CorrectiveActionOwner',  label: 'Corrective Action Owner',    desc: 'Views and resolves assigned corrective actions only' },
    { value: 'ReadOnlyViewer',         label: 'Read-Only Viewer',           desc: 'Can view audits and reports but cannot make any changes' },
    { value: 'ExecutiveViewer',        label: 'Executive Viewer',           desc: 'Dashboard and reports access only; executive summary view' },
    { value: 'Auditor',                label: 'Auditor',                    desc: 'Field auditor — creates and submits audits; no review access' },
    { value: 'ITAdmin',                label: 'IT Admin',                   desc: 'User management and system configuration access' },
    { value: 'AuditAdmin',             label: 'Audit Admin',                desc: 'Full audit module admin — all capabilities except system config' },
];

// ── Email routing ──────────────────────────────────────────────────────────────

interface EditableRule {
    id?: number | null;
    divisionId: number;
    emailAddress: string;
    isActive: boolean;
    displayName?: string;
}

const COMPANIES: { name: string; domain: string }[] = [
    { name: 'Stronghold Companies',          domain: 'thestrongholdcompanies.com' },
    { name: 'Quanta Services',               domain: 'quantaservices.com' },
    { name: 'Kinder Morgan',                 domain: 'kindermorgan.com' },
    { name: 'Enterprise Products Partners',  domain: 'enterpriseproducts.com' },
    { name: 'ExxonMobil',                    domain: 'exxonmobil.com' },
    { name: 'Motiva Enterprises',            domain: 'motivaenterprises.com' },
    { name: 'Marathon Petroleum',            domain: 'marathonpetroleum.com' },
    { name: 'Valero Energy',                 domain: 'valero.com' },
    { name: 'LyondellBasell',               domain: 'lyondellbasell.com' },
    { name: 'Chevron Phillips Chemical',     domain: 'cpchem.com' },
    { name: 'Plains All American Pipeline',  domain: 'plains.com' },
    { name: 'INEOS',                         domain: 'ineos.com' },
    { name: 'Huntsman Corporation',          domain: 'huntsman.com' },
];

interface DivisionGroup {
    divisionId: number;
    divisionCode: string;
    divisionName: string;
    rules: EditableRule[];
}

const divisionGroups = ref<DivisionGroup[]>([]);
const isDirty = ref(false);
const selectedDivisionCode = ref<string>('');

const activeDivisionGroup = computed(() =>
    divisionGroups.value.find(g => g.divisionCode === selectedDivisionCode.value) ?? null
);

// ── Score Targets ──────────────────────────────────────────────────────────────

interface TargetRow {
    divisionId: number;
    divisionCode: string;
    divisionName: string;
    scoreTarget: number | null;
    /** Input-bound value — starts equal to scoreTarget */
    pendingTarget: number | null | undefined;
    auditFrequencyDays: number | null;
    pendingFrequency: number | null | undefined;
    savingFrequency: boolean;
    saving: boolean;
    requireClosurePhoto: boolean;
    savingClosurePhoto: boolean;
    slaNormalDays: number | null;
    pendingSlaNormal: number | null | undefined;
    slaUrgentDays: number | null;
    pendingSlaUrgent: number | null | undefined;
    slaEscalateAfterDays: number | null;
    pendingSlaEscalate: number | null | undefined;
    escalationEmail: string | null;
    pendingEscalationEmail: string | null | undefined;
    savingSla: boolean;
}

const targetRows = ref<TargetRow[]>([]);

async function loadScoreTargets() {
    try {
        const dtos = await service.getDivisionScoreTargets();
        targetRows.value = dtos.map(d => ({
            divisionId:            d.divisionId,
            divisionCode:          d.divisionCode,
            divisionName:          d.divisionName,
            scoreTarget:           d.scoreTarget ?? null,
            pendingTarget:         d.scoreTarget ?? null,
            auditFrequencyDays:    d.auditFrequencyDays ?? null,
            pendingFrequency:      d.auditFrequencyDays ?? null,
            savingFrequency:       false,
            saving:                false,
            requireClosurePhoto:   d.requireClosurePhoto ?? false,
            savingClosurePhoto:    false,
            slaNormalDays:         d.slaNormalDays ?? null,
            pendingSlaNormal:      d.slaNormalDays ?? null,
            slaUrgentDays:         d.slaUrgentDays ?? null,
            pendingSlaUrgent:      d.slaUrgentDays ?? null,
            slaEscalateAfterDays:  d.slaEscalateAfterDays ?? null,
            pendingSlaEscalate:    d.slaEscalateAfterDays ?? null,
            escalationEmail:       d.escalationEmail ?? null,
            pendingEscalationEmail: d.escalationEmail ?? null,
            savingSla:             false,
        }));
    } catch {
        // Non-fatal — tab will show "Loading divisions…"
    }
}

async function saveTarget(row: TargetRow) {
    row.saving = true;
    try {
        // Convert empty string / undefined from number input to null
        const val = row.pendingTarget != null && !Number.isNaN(Number(row.pendingTarget))
            ? Number(row.pendingTarget)
            : null;
        const dto = await service.setDivisionScoreTarget(row.divisionId, val);
        row.scoreTarget = dto.scoreTarget ?? null;
        row.pendingTarget = dto.scoreTarget ?? null;
        toast.add({ severity: 'success', summary: 'Saved', detail: `${row.divisionCode} target updated.`, life: 2500 });
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Could not save score target.', life: 4000 });
    } finally {
        row.saving = false;
    }
}

async function saveFrequency(row: TargetRow) {
    row.savingFrequency = true;
    try {
        const val = row.pendingFrequency != null && !Number.isNaN(Number(row.pendingFrequency)) && Number(row.pendingFrequency) > 0
            ? Number(row.pendingFrequency)
            : null;
        const dto = await service.setDivisionAuditFrequency(row.divisionId, val);
        row.auditFrequencyDays = dto.auditFrequencyDays ?? null;
        row.pendingFrequency = dto.auditFrequencyDays ?? null;
        toast.add({ severity: 'success', summary: 'Saved', detail: `${row.divisionCode} audit frequency updated.`, life: 2500 });
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Could not save audit frequency.', life: 4000 });
    } finally {
        row.savingFrequency = false;
    }
}

async function toggleClosurePhoto(row: TargetRow, value: boolean) {
    row.savingClosurePhoto = true;
    try {
        const dto = await service.setDivisionRequireClosurePhoto(row.divisionId, value);
        row.requireClosurePhoto = dto.requireClosurePhoto ?? false;
        toast.add({ severity: 'success', summary: 'Saved', detail: `${row.divisionCode} closure photo requirement ${value ? 'enabled' : 'disabled'}.`, life: 2500 });
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Could not save closure photo setting.', life: 4000 });
    } finally {
        row.savingClosurePhoto = false;
    }
}

async function saveSla(row: TargetRow) {
    row.savingSla = true;
    try {
        const toInt = (v: number | null | undefined) =>
            v != null && !Number.isNaN(Number(v)) && Number(v) > 0 ? Number(v) : null;
        const dto = await service.setDivisionSla(row.divisionId, {
            slaNormalDays:        toInt(row.pendingSlaNormal),
            slaUrgentDays:        toInt(row.pendingSlaUrgent),
            slaEscalateAfterDays: toInt(row.pendingSlaEscalate),
            escalationEmail:      row.pendingEscalationEmail?.trim() || null,
        });
        row.slaNormalDays        = dto.slaNormalDays        ?? null;
        row.slaUrgentDays        = dto.slaUrgentDays        ?? null;
        row.slaEscalateAfterDays = dto.slaEscalateAfterDays ?? null;
        row.escalationEmail      = dto.escalationEmail      ?? null;
        row.pendingSlaNormal     = row.slaNormalDays;
        row.pendingSlaUrgent     = row.slaUrgentDays;
        row.pendingSlaEscalate   = row.slaEscalateAfterDays;
        row.pendingEscalationEmail = row.escalationEmail;
        toast.add({ severity: 'success', summary: 'Saved', detail: `${row.divisionCode} SLA updated.`, life: 2500 });
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Could not save SLA settings.', life: 4000 });
    } finally {
        row.savingSla = false;
    }
}

// Load score targets the first time the tab is clicked
watch(activeTab, (tab) => {
    if (tab === 'targets' && targetRows.value.length === 0) {
        loadScoreTargets();
    }
});

onMounted(async () => {
    await Promise.all([
        adminStore.loadEmailRouting(),
        adminStore.loadUserAuditRoles(),
    ]);
    buildGroups();
});

function buildGroups() {
    const map = new Map<number, DivisionGroup>();
    for (const r of adminStore.emailRules) {
        if (!map.has(r.divisionId)) {
            map.set(r.divisionId, {
                divisionId: r.divisionId,
                divisionCode: r.divisionCode,
                divisionName: r.divisionName,
                rules: [],
            });
        }
        map.get(r.divisionId)!.rules.push({
            id: r.id,
            divisionId: r.divisionId,
            emailAddress: r.emailAddress,
            isActive: r.isActive,
            displayName: undefined,
        });
    }
    divisionGroups.value = Array.from(map.values()).sort((a, b) => a.divisionName.localeCompare(b.divisionName));
    // Auto-select first division (or keep current selection if still valid)
    if (!selectedDivisionCode.value || !divisionGroups.value.find(g => g.divisionCode === selectedDivisionCode.value)) {
        selectedDivisionCode.value = divisionGroups.value[0]?.divisionCode ?? '';
    }
    isDirty.value = false;
}

function deactivateAll() {
    for (const group of divisionGroups.value) {
        for (const rule of group.rules) {
            rule.isActive = false;
        }
    }
    isDirty.value = true;
}

// ── Add Recipient Dialog ───────────────────────────────────────────────────────

const addDialog = ref({
    open: false,
    targetGroup: null as DivisionGroup | null,
    divisionName: '',
    firstName: '',
    lastName: '',
    company: '',
    email: '',
});

function addRow(group: DivisionGroup) {
    addDialog.value = {
        open: true,
        targetGroup: group,
        divisionName: group.divisionName,
        firstName: '',
        lastName: '',
        company: '',
        email: '',
    };
}

function autoFillEmail() {
    const { firstName, lastName, company } = addDialog.value;
    if (!firstName && !lastName) return;
    const companyData = COMPANIES.find(c => c.name === company);
    const domain = companyData?.domain ?? '';
    const local = [firstName.toLowerCase(), lastName.toLowerCase()].filter(Boolean).join('.');
    if (local && domain) {
        addDialog.value.email = `${local}@${domain}`;
    } else if (local) {
        addDialog.value.email = `${local}@`;
    }
}

function confirmAddRow() {
    const { targetGroup, firstName, lastName, email } = addDialog.value;
    if (!targetGroup || !email.trim()) return;
    const displayName = [firstName.trim(), lastName.trim()].filter(Boolean).join(' ') || undefined;
    targetGroup.rules.push({
        divisionId: targetGroup.divisionId,
        emailAddress: email.trim(),
        isActive: true,
        displayName,
    });
    isDirty.value = true;
    closeAddDialog();
}

function closeAddDialog() {
    addDialog.value.open = false;
}

function removeRow(group: DivisionGroup, idx: number) {
    group.rules.splice(idx, 1);
    isDirty.value = true;
}

async function onSave() {
    const allRules: EmailRoutingRuleUpsertDto[] = [];
    for (const group of divisionGroups.value) {
        for (const r of group.rules) {
            if (r.emailAddress.trim()) {
                allRules.push({
                    id: r.id,
                    divisionId: r.divisionId,
                    emailAddress: r.emailAddress.trim(),
                    isActive: r.isActive,
                });
            }
        }
    }
    const ok = await adminStore.saveEmailRouting(allRules);
    if (ok) {
        buildGroups();
    }
}

// ── User roles ─────────────────────────────────────────────────────────────────

async function onRoleChange(userId: number, roleName: string) {
    await adminStore.setUserAuditRole(userId, roleName || null);
}
</script>

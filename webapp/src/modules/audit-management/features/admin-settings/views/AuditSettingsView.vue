<template>
    <div>
        <BasePageHeader
            title="Admin Settings"
            subtitle="Email routing and user role assignments for the audit module"
            icon="pi pi-cog"
        />

        <div class="p-4 max-w-5xl">
            <!-- Tab bar -->
            <div class="flex gap-1 mb-5 border-b border-slate-700">
                <button
                    v-for="tab in TABS"
                    :key="tab.key"
                    @click="activeTab = tab.key"
                    class="px-4 py-2 text-sm font-medium transition-colors"
                    :class="activeTab === tab.key
                        ? 'border-b-2 border-blue-500 text-blue-400'
                        : 'text-slate-400 hover:text-slate-200'"
                >
                    {{ tab.label }}
                </button>
            </div>

            <div v-if="adminStore.loading" class="text-slate-400 text-sm">Loading…</div>

            <!-- ── EMAIL ROUTING TAB ── -->
            <template v-else-if="activeTab === 'email'">
                <div class="flex items-center justify-between mb-4">
                    <p class="text-slate-400 text-sm">
                        When an audit is submitted, all active recipients for that division receive a review email.
                    </p>
                    <div class="flex gap-2">
                        <button
                            @click="deactivateAll"
                            :disabled="adminStore.saving"
                            class="px-3 py-1.5 text-sm bg-amber-700 hover:bg-amber-600 text-white rounded disabled:opacity-40"
                            title="Uncheck all recipients — useful during testing to prevent real emails from going out"
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

                <div
                    v-for="group in divisionGroups"
                    :key="group.divisionCode"
                    class="mb-5 bg-slate-800 border border-slate-700 rounded-lg overflow-hidden"
                >
                    <div class="px-4 py-2 bg-slate-750 border-b border-slate-700 flex items-center justify-between">
                        <span class="font-medium text-slate-200 text-sm">{{ group.divisionName }}</span>
                        <button
                            @click="addRow(group)"
                            class="text-xs text-blue-400 hover:text-blue-300 pi pi-plus"
                            title="Add recipient"
                        />
                    </div>

                    <div class="divide-y divide-slate-700">
                        <div
                            v-for="(rule, idx) in group.rules"
                            :key="idx"
                            class="px-4 py-2 flex items-center gap-3"
                        >
                            <input
                                v-model="rule.emailAddress"
                                @input="isDirty = true"
                                type="email"
                                placeholder="email@example.com"
                                class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-sm text-slate-200 placeholder-slate-500 focus:outline-none focus:border-slate-400"
                            />
                            <label class="flex items-center gap-1.5 text-sm text-slate-400 cursor-pointer shrink-0">
                                <input
                                    v-model="rule.isActive"
                                    @change="isDirty = true"
                                    type="checkbox"
                                    class="accent-blue-500"
                                />
                                Active
                            </label>
                            <button
                                @click="removeRow(group, idx)"
                                class="text-red-400 hover:text-red-300 pi pi-trash text-xs shrink-0"
                                title="Remove"
                            />
                        </div>

                        <div v-if="!group.rules.length" class="px-4 py-2 text-sm text-slate-500 italic">
                            No recipients configured
                        </div>
                    </div>
                </div>

                <div v-if="!divisionGroups.length" class="text-slate-500 text-sm">
                    No divisions found.
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

                <div class="bg-slate-800 border border-slate-700 rounded-lg overflow-hidden">
                    <table class="w-full text-sm">
                        <thead>
                            <tr class="border-b border-slate-700 text-slate-400 text-xs uppercase tracking-wide">
                                <th class="px-4 py-2 text-left">Name</th>
                                <th class="px-4 py-2 text-left">Email</th>
                                <th class="px-4 py-2 text-left w-56">Audit Role</th>
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
                                        <option v-for="role in AUDIT_ROLES" :key="role.value" :value="role.value">
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
import { ref, onMounted } from 'vue';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAdminStore } from '../../../stores/adminStore';
import type { EmailRoutingRuleUpsertDto } from '@/apiclient/auditClient';

const adminStore = useAdminStore();

const TABS = [
    { key: 'email', label: 'Email Routing' },
    { key: 'roles', label: 'User Roles' },
] as const;
type TabKey = typeof TABS[number]['key'];
const activeTab = ref<TabKey>('email');

const AUDIT_ROLES = [
    { value: 'TemplateAdmin',          label: 'Template Admin' },
    { value: 'AuditManager',           label: 'Audit Manager' },
    { value: 'AuditReviewer',          label: 'Audit Reviewer' },
    { value: 'CorrectiveActionOwner',  label: 'Corrective Action Owner' },
    { value: 'ReadOnlyViewer',         label: 'Read-Only Viewer' },
    { value: 'ExecutiveViewer',        label: 'Executive Viewer' },
];

// ── Email routing ──────────────────────────────────────────────────────────────

interface EditableRule {
    id?: number | null;
    divisionId: number;
    emailAddress: string;
    isActive: boolean;
}

interface DivisionGroup {
    divisionId: number;
    divisionCode: string;
    divisionName: string;
    rules: EditableRule[];
}

const divisionGroups = ref<DivisionGroup[]>([]);
const isDirty = ref(false);

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
        });
    }
    divisionGroups.value = Array.from(map.values()).sort((a, b) => a.divisionName.localeCompare(b.divisionName));
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

function addRow(group: DivisionGroup) {
    group.rules.push({ divisionId: group.divisionId, emailAddress: '', isActive: true });
    isDirty.value = true;
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

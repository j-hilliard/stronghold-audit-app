<template>
    <div class="flex flex-col min-h-0">

        <BasePageHeader
            icon="pi pi-users"
            title="User Management"
            subtitle="Manage users, roles, and access permissions"
        >
            <Button label="Add User" icon="pi pi-user-plus" size="small" @click="openAddUserDialog" />
            <Button icon="pi pi-refresh" outlined size="small" :loading="loading" @click="loadData" />
        </BasePageHeader>

        <div class="flex flex-col gap-4 p-4 min-h-0 flex-1">

        <!-- ── Stats Row ──────────────────────────────────────────────────────── -->
        <div class="grid grid-cols-3 gap-3">
            <div class="bg-slate-800 border border-slate-700 rounded-xl p-4 flex items-center gap-3">
                <div class="w-10 h-10 rounded-lg bg-blue-500/15 flex items-center justify-center shrink-0">
                    <i class="pi pi-users text-blue-400" />
                </div>
                <div>
                    <div class="text-2xl font-bold text-white">{{ users.length }}</div>
                    <div class="text-xs text-slate-400">Total Users</div>
                </div>
            </div>
            <div class="bg-slate-800 border border-slate-700 rounded-xl p-4 flex items-center gap-3">
                <div class="w-10 h-10 rounded-lg bg-emerald-500/15 flex items-center justify-center shrink-0">
                    <i class="pi pi-check-circle text-emerald-400" />
                </div>
                <div>
                    <div class="text-2xl font-bold text-emerald-400">{{ activeCount }}</div>
                    <div class="text-xs text-slate-400">Active</div>
                </div>
            </div>
            <div class="bg-slate-800 border border-slate-700 rounded-xl p-4 flex items-center gap-3">
                <div class="w-10 h-10 rounded-lg bg-slate-600/30 flex items-center justify-center shrink-0">
                    <i class="pi pi-ban text-slate-400" />
                </div>
                <div>
                    <div class="text-2xl font-bold text-slate-400">{{ users.length - activeCount }}</div>
                    <div class="text-xs text-slate-400">Disabled</div>
                </div>
            </div>
        </div>

        <!-- ── Filter Bar ─────────────────────────────────────────────────────── -->
        <div class="flex flex-wrap gap-2 items-end">
            <div class="relative flex-1 min-w-[200px]">
                <i class="pi pi-search absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm pointer-events-none z-10" />
                <InputText
                    v-model="searchText"
                    placeholder="Search by name, email, or role…"
                    class="w-full pl-9 pr-8"
                />
                <button
                    v-if="searchText"
                    class="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white transition-colors"
                    @click="searchText = ''"
                >
                    <i class="pi pi-times text-xs" />
                </button>
            </div>
            <Dropdown
                v-model="filterStatus"
                :options="statusFilterOptions"
                option-label="label"
                option-value="value"
                class="w-40"
                @change="() => {}"
            />
            <Dropdown
                v-model="filterRole"
                :options="roleFilterOptions"
                option-label="label"
                option-value="value"
                placeholder="All Roles"
                show-clear
                class="w-48"
            />
        </div>

        <!-- ── Users Table ────────────────────────────────────────────────────── -->
        <DataTable
            :value="filteredUsers"
            :loading="loading"
            sort-field="lastName"
            :sort-order="1"
            class="stronghold-table text-sm"
            data-key="userId"
            scrollable
            scroll-height="flex"
        >
            <template #empty>
                <div class="flex flex-col items-center py-12 gap-2">
                    <i class="pi pi-users text-4xl text-slate-600" />
                    <p class="text-slate-400">No users found</p>
                </div>
            </template>

            <!-- Avatar + Name -->
            <Column header="User" sortable sort-field="lastName" style="min-width:200px">
                <template #body="{ data }">
                    <div class="flex items-center gap-3">
                        <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold shrink-0"
                            :class="data.active ? 'bg-blue-600/30 text-blue-300' : 'bg-slate-700 text-slate-400'">
                            {{ initials(data) }}
                        </div>
                        <div>
                            <div class="font-medium text-white">{{ fullName(data) }}</div>
                            <div class="text-xs text-slate-400">{{ data.email }}</div>
                        </div>
                    </div>
                </template>
            </Column>

            <!-- Department -->
            <Column header="Department" sortable sort-field="department" style="min-width:150px">
                <template #body="{ data }">
                    <div class="text-slate-300 text-sm">{{ data.department || '—' }}</div>
                </template>
            </Column>

            <!-- Roles -->
            <Column header="Roles" style="min-width:220px">
                <template #body="{ data }">
                    <div class="flex flex-wrap gap-1">
                        <span
                            v-for="ur in data.roles ?? []"
                            :key="ur.roleId"
                            class="inline-flex items-center gap-1 text-[11px] px-2 py-0.5 rounded-full border"
                            :class="roleChipClass(ur.role?.name)"
                        >
                            {{ ur.role?.name ?? '?' }}
                            <button
                                class="hover:text-red-400 transition-colors ml-0.5"
                                title="Remove role"
                                @click="confirmRemoveRole(data, ur)"
                            >
                                <i class="pi pi-times text-[9px]" />
                            </button>
                        </span>
                        <button
                            class="add-role-btn inline-flex items-center gap-1 text-[11px] px-2 py-0.5 rounded-full border border-dashed border-slate-600 text-slate-400 hover:border-blue-500 hover:text-blue-400 transition-colors"
                            @click="openAddRoleDialog(data)"
                        >
                            <i class="pi pi-plus text-[10px]" /> Add
                        </button>
                    </div>
                </template>
            </Column>

            <!-- Status -->
            <Column header="Status" sortable sort-field="active" style="min-width:100px">
                <template #body="{ data }">
                    <Tag
                        :value="data.active ? 'Active' : 'Disabled'"
                        :severity="data.active ? 'success' : 'secondary'"
                    />
                </template>
            </Column>

            <!-- Last Login -->
            <Column header="Last Login" sortable sort-field="lastLogin" style="min-width:120px">
                <template #body="{ data }">
                    <span class="text-xs text-slate-400">
                        {{ data.lastLogin ? new Date(data.lastLogin).toLocaleDateString() : 'Never' }}
                    </span>
                </template>
            </Column>

            <!-- Actions -->
            <Column header="" style="min-width:110px; text-align:right" frozen align-frozen="right">
                <template #body="{ data }">
                    <div class="user-row-actions">
                        <Button
                            icon="pi pi-pencil"
                            size="small"
                            severity="secondary"
                            text
                            v-tooltip.top="'Edit user'"
                            @click="openEditUserDialog(data)"
                        />
                        <Button
                            v-if="data.active"
                            icon="pi pi-ban"
                            size="small"
                            severity="danger"
                            text
                            title="Disable user"
                            aria-label="Disable user"
                            v-tooltip.top="'Disable user'"
                            @click="toggleUserStatus(data, false)"
                        />
                        <Button
                            v-else
                            icon="pi pi-check"
                            size="small"
                            severity="success"
                            text
                            title="Enable user"
                            aria-label="Enable user"
                            v-tooltip.top="'Enable user'"
                            @click="toggleUserStatus(data, true)"
                        />
                        <Button
                            icon="pi pi-trash"
                            size="small"
                            severity="danger"
                            text
                            v-tooltip.top="'Delete user'"
                            @click="confirmDeleteUser(data)"
                        />
                    </div>
                </template>
            </Column>
        </DataTable>

        </div><!-- /.inner -->

        <!-- ── Add Role Dialog ────────────────────────────────────────────────── -->
        <Dialog v-model:visible="showAddRoleDialog" modal header="Assign Role" :style="{ width: '400px' }">
            <div v-if="addRoleTarget" class="flex flex-col gap-4">
                <div class="flex items-center gap-3 bg-slate-800 rounded-lg p-3">
                    <div class="w-8 h-8 rounded-full bg-blue-600/30 flex items-center justify-center text-sm font-bold text-blue-300">
                        {{ initials(addRoleTarget) }}
                    </div>
                    <div>
                        <div class="font-medium text-white">{{ fullName(addRoleTarget) }}</div>
                        <div class="text-xs text-slate-400">{{ addRoleTarget.email }}</div>
                    </div>
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-medium">Select Role</label>
                    <Dropdown
                        v-model="selectedRoleId"
                        :options="availableRolesForUser"
                        option-label="name"
                        option-value="roleId"
                        placeholder="Choose a role…"
                        class="w-full"
                    />
                </div>
                <div class="flex justify-end gap-2">
                    <Button label="Cancel" text @click="showAddRoleDialog = false" />
                    <Button
                        label="Assign Role"
                        icon="pi pi-plus"
                        :loading="addRoleSaving"
                        :disabled="!selectedRoleId"
                        @click="submitAddRole"
                    />
                </div>
            </div>
        </Dialog>

        <!-- ── Add User Dialog ───────────────────────────────────────────────── -->
        <Dialog v-model:visible="showAddUserDialog" modal header="Add New User" :style="{ width: '460px' }">
            <div class="flex flex-col gap-4 pt-2">
                <div class="grid grid-cols-2 gap-3">
                    <div class="flex flex-col gap-1">
                        <label class="text-xs text-slate-400">First Name *</label>
                        <InputText v-model="newUser.firstName" placeholder="First name" class="w-full" />
                    </div>
                    <div class="flex flex-col gap-1">
                        <label class="text-xs text-slate-400">Last Name *</label>
                        <InputText v-model="newUser.lastName" placeholder="Last name" class="w-full" />
                    </div>
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-xs text-slate-400">Email *</label>
                    <InputText v-model="newUser.email" placeholder="name@company.com" class="w-full" />
                </div>
                <div class="grid grid-cols-2 gap-3">
                    <div class="flex flex-col gap-1">
                        <label class="text-xs text-slate-400">Department</label>
                        <InputText v-model="newUser.department" placeholder="e.g. Safety" class="w-full" />
                    </div>
                    <div class="flex flex-col gap-1">
                        <label class="text-xs text-slate-400">Title</label>
                        <InputText v-model="newUser.title" placeholder="e.g. Field Auditor" class="w-full" />
                    </div>
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-xs text-slate-400">Initial Role</label>
                    <Dropdown
                        v-model="newUserRoleId"
                        :options="userFacingRoles"
                        option-label="name"
                        option-value="roleId"
                        placeholder="Assign a role (optional)"
                        show-clear
                        class="w-full"
                    />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" severity="secondary" text @click="showAddUserDialog = false" />
                <Button
                    label="Add User"
                    icon="pi pi-user-plus"
                    :loading="addUserSaving"
                    :disabled="!newUser.firstName?.trim() || !newUser.lastName?.trim() || !newUser.email?.trim()"
                    @click="submitAddUser"
                />
            </template>
        </Dialog>

        <!-- ── Edit User Dialog ──────────────────────────────────────────────── -->
        <Dialog v-model:visible="showEditUserDialog" modal header="Edit User" :style="{ width: '460px' }">
            <div class="flex flex-col gap-4 pt-2">
                <div class="grid grid-cols-2 gap-3">
                    <div class="flex flex-col gap-1">
                        <label class="text-xs text-slate-400">First Name *</label>
                        <InputText v-model="editUser.firstName" class="w-full text-sm" placeholder="First name" />
                    </div>
                    <div class="flex flex-col gap-1">
                        <label class="text-xs text-slate-400">Last Name *</label>
                        <InputText v-model="editUser.lastName" class="w-full text-sm" placeholder="Last name" />
                    </div>
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-xs text-slate-400">Email *</label>
                    <InputText v-model="editUser.email" type="email" class="w-full text-sm" placeholder="email@example.com" />
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-xs text-slate-400">Department</label>
                    <InputText v-model="editUser.department" class="w-full text-sm" placeholder="Department" />
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-xs text-slate-400">Title</label>
                    <InputText v-model="editUser.title" class="w-full text-sm" placeholder="Job title" />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" severity="secondary" text @click="showEditUserDialog = false" />
                <Button
                    label="Save Changes"
                    icon="pi pi-check"
                    :loading="editUserSaving"
                    :disabled="!editUser.firstName?.trim() || !editUser.lastName?.trim() || !editUser.email?.trim()"
                    @click="submitEditUser"
                />
            </template>
        </Dialog>



        <!-- ── Remove Role Confirm ────────────────────────────────────────────── -->
        <Dialog v-model:visible="showRemoveRoleDialog" modal header="Remove Role" :style="{ width: '380px' }">
            <div v-if="removeRoleTarget && removeRoleItem" class="flex flex-col gap-4">
                <p class="text-sm text-slate-300">
                    Remove <span class="text-white font-medium">{{ removeRoleItem.role?.name }}</span>
                    from <span class="text-white font-medium">{{ fullName(removeRoleTarget) }}</span>?
                </p>
                <div class="flex justify-end gap-2">
                    <Button label="Cancel" text @click="showRemoveRoleDialog = false" />
                    <Button label="Remove" severity="danger" :loading="removeRoleSaving" @click="submitRemoveRole" />
                </div>
            </div>
        </Dialog>

    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'primevue/usetoast';
import { useConfirm } from 'primevue/useconfirm';
import { type User, type Role, type UserRole } from '@/apiclient/client';
import { useUserService } from '@/modules/audit-management/services/useUserService';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';

const toast    = useToast();
const confirm  = useConfirm();
const { getUserClient, getRoleClient, getUserRoleClient } = useUserService();

// ── State ─────────────────────────────────────────────────────────────────────
const loading    = ref(false);
const users      = ref<User[]>([]);
const allRoles   = ref<Role[]>([]);
const searchText = ref('');
const filterStatus = ref<'all' | 'active' | 'disabled'>('all');
const filterRole   = ref<number | null>(null);

// ── Derived ───────────────────────────────────────────────────────────────────
const activeCount = computed(() => users.value.filter(u => u.active).length);

const statusFilterOptions = [
    { label: 'All Users', value: 'all' },
    { label: 'Active Only', value: 'active' },
    { label: 'Disabled Only', value: 'disabled' },
];

const roleFilterOptions = computed(() => [
    { label: 'All Roles', value: null },
    ...allRoles.value.map(r => ({ label: r.name, value: r.roleId })),
]);

const filteredUsers = computed(() => {
    let result = users.value;

    if (filterStatus.value === 'active')   result = result.filter(u => u.active);
    if (filterStatus.value === 'disabled') result = result.filter(u => !u.active);

    if (filterRole.value != null) {
        result = result.filter(u => u.roles?.some(ur => ur.roleId === filterRole.value));
    }

    if (searchText.value.trim()) {
        const q = searchText.value.trim().toLowerCase();
        result = result.filter(u =>
            fullName(u).toLowerCase().includes(q) ||
            (u.email?.toLowerCase().includes(q)) ||
            u.roles?.some(ur => ur.role?.name?.toLowerCase().includes(q))
        );
    }

    return result;
});

// ── Add Role Dialog ───────────────────────────────────────────────────────────
const showAddRoleDialog    = ref(false);
const addRoleTarget        = ref<User | null>(null);
const selectedRoleId       = ref<number | null>(null);
const addRoleSaving        = ref(false);

const USER_FACING_ROLES = ['ITAdmin', 'Auditor', 'AuditAdmin', 'Executive', 'NormalUser'];

const userFacingRoles = computed(() =>
    allRoles.value.filter(r => USER_FACING_ROLES.includes(r.name ?? ''))
);

const availableRolesForUser = computed(() => {
    if (!addRoleTarget.value) return userFacingRoles.value;
    const existing = new Set(addRoleTarget.value.roles?.map(ur => ur.roleId) ?? []);
    return userFacingRoles.value.filter(r => r.roleId != null && !existing.has(r.roleId));
});

function openAddRoleDialog(user: User) {
    addRoleTarget.value  = user;
    selectedRoleId.value = null;
    showAddRoleDialog.value = true;
}

async function submitAddRole() {
    if (!addRoleTarget.value || !selectedRoleId.value) return;
    addRoleSaving.value = true;
    try {
        await getUserRoleClient().addUserToRole({
            userId: addRoleTarget.value.userId!,
            roleId: selectedRoleId.value,
        } as any);
        toast.add({ severity: 'success', summary: 'Role Assigned', detail: 'Role has been assigned.', life: 3000 });
        showAddRoleDialog.value = false;
        await loadData();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign role.', life: 4000 });
    } finally {
        addRoleSaving.value = false;
    }
}

// ── Remove Role Dialog ────────────────────────────────────────────────────────
const showRemoveRoleDialog = ref(false);
const removeRoleTarget     = ref<User | null>(null);
const removeRoleItem       = ref<UserRole | null>(null);
const removeRoleSaving     = ref(false);

function confirmRemoveRole(user: User, ur: UserRole) {
    removeRoleTarget.value = user;
    removeRoleItem.value   = ur;
    showRemoveRoleDialog.value = true;
}

async function submitRemoveRole() {
    if (!removeRoleTarget.value || !removeRoleItem.value) return;
    removeRoleSaving.value = true;
    try {
        await getUserRoleClient().deleteRole(
            removeRoleTarget.value.userId!,
            removeRoleItem.value.roleId,
        );
        toast.add({ severity: 'success', summary: 'Role Removed', detail: 'Role has been removed.', life: 3000 });
        showRemoveRoleDialog.value = false;
        await loadData();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove role.', life: 4000 });
    } finally {
        removeRoleSaving.value = false;
    }
}

// ── Add User Dialog ───────────────────────────────────────────────────────────
const showAddUserDialog = ref(false);
const newUser           = ref({ firstName: '', lastName: '', email: '', department: '', title: '' });
const newUserRoleId     = ref<number | null>(null);
const addUserSaving     = ref(false);

function openAddUserDialog() {
    newUser.value = { firstName: '', lastName: '', email: '', department: '', title: '' };
    newUserRoleId.value = null;
    showAddUserDialog.value = true;
}

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

async function submitAddUser() {
    const firstName = newUser.value.firstName.trim();
    const lastName  = newUser.value.lastName.trim();
    const email     = newUser.value.email.trim();

    if (!firstName) {
        toast.add({ severity: 'warn', summary: 'Validation', detail: 'First name is required.', life: 4000 });
        return;
    }
    if (!lastName) {
        toast.add({ severity: 'warn', summary: 'Validation', detail: 'Last name is required.', life: 4000 });
        return;
    }
    if (!email || !EMAIL_RE.test(email)) {
        toast.add({ severity: 'warn', summary: 'Validation', detail: 'A valid email address is required (e.g. john@company.com).', life: 5000 });
        return;
    }

    addUserSaving.value = true;
    try {
        const created = await getUserClient().createUser({
            firstName:        newUser.value.firstName.trim(),
            lastName:         newUser.value.lastName.trim(),
            email:            newUser.value.email.trim(),
            department:       newUser.value.department.trim() || undefined,
            title:            newUser.value.title.trim() || undefined,
            azureAdObjectId:  crypto.randomUUID(),
            active:           true,
        } as any);
        if (newUserRoleId.value && created.userId) {
            await getUserRoleClient().addUserToRole({
                userId: created.userId,
                roleId: newUserRoleId.value,
            } as any);
        }
        toast.add({ severity: 'success', summary: 'User Added', detail: `${newUser.value.firstName} ${newUser.value.lastName} has been added.`, life: 3000 });
        showAddUserDialog.value = false;
        await loadData();
    } catch (err: any) {
        const msg = err?.response?.data?.detail
            || err?.response?.data?.message
            || err?.response?.data?.[0]?.errorMessage
            || err?.message
            || 'Failed to add user.';
        toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 6000 });
    } finally {
        addUserSaving.value = false;
    }
}

// ── Edit User Dialog ──────────────────────────────────────────────────────────
const showEditUserDialog = ref(false);
const editUserTarget     = ref<User | null>(null);
const editUser           = ref({ firstName: '', lastName: '', email: '', department: '', title: '' });
const editUserSaving     = ref(false);

function openEditUserDialog(user: User) {
    editUserTarget.value = user;
    editUser.value = {
        firstName:  user.firstName  ?? '',
        lastName:   user.lastName   ?? '',
        email:      user.email      ?? '',
        department: user.department ?? '',
        title:      user.title      ?? '',
    };
    showEditUserDialog.value = true;
}

async function submitEditUser() {
    if (!editUserTarget.value?.userId) return;
    editUserSaving.value = true;
    try {
        await getUserClient().updateUser(editUserTarget.value.userId, {
            ...editUserTarget.value,
            firstName:  editUser.value.firstName.trim(),
            lastName:   editUser.value.lastName.trim(),
            email:      editUser.value.email.trim(),
            department: editUser.value.department.trim() || undefined,
            title:      editUser.value.title.trim() || undefined,
        } as any);
        toast.add({ severity: 'success', summary: 'User Updated', detail: `${editUser.value.firstName} ${editUser.value.lastName} has been updated.`, life: 3000 });
        showEditUserDialog.value = false;
        await loadData();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to update user.', life: 4000 });
    } finally {
        editUserSaving.value = false;
    }
}

// ── Enable / Disable ──────────────────────────────────────────────────────────
async function toggleUserStatus(user: User, enable: boolean) {
    try {
        if (enable) {
            await getUserClient().activateUser(user.userId!);
            toast.add({ severity: 'success', summary: 'User Enabled', detail: `${fullName(user)} is now active.`, life: 3000 });
        } else {
            await getUserClient().disableUser(user.userId!);
            toast.add({ severity: 'warn', summary: 'User Disabled', detail: `${fullName(user)} has been disabled.`, life: 3000 });
        }
        await loadData();
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to update user status.', life: 4000 });
    }
}

// ── Delete User ───────────────────────────────────────────────────────────────
function confirmDeleteUser(user: User) {
    confirm.require({
        message: `Permanently delete ${fullName(user)} (${user.email})? This cannot be undone.`,
        header: 'Delete User',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Delete',
        rejectLabel: 'Cancel',
        acceptClass: 'p-button-danger',
        accept: () => {
            getUserClient().deleteUser(user.userId!)
                .then(() => {
                    toast.add({ severity: 'success', summary: 'User Deleted', detail: `${fullName(user)} has been deleted.`, life: 3000 });
                    return loadData();
                })
                .catch((err: any) => {
                    const msg = err?.response?.data?.detail || err?.message || 'Failed to delete user.';
                    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 5000 });
                });
        },
    });
}

// ── Load ──────────────────────────────────────────────────────────────────────
async function loadData() {
    loading.value = true;
    try {
        const [usersResult, rolesResult] = await Promise.all([
            getUserClient().getAllUsers(),
            getRoleClient().getAllRoles(),
        ]);
        users.value    = usersResult;
        allRoles.value = rolesResult;
    } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load users.', life: 4000 });
    } finally {
        loading.value = false;
    }
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function fullName(u: User) {
    return `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || (u.email ?? 'Unknown');
}
function initials(u: User) {
    const first = (u.firstName ?? '').charAt(0).toUpperCase();
    const last  = (u.lastName  ?? '').charAt(0).toUpperCase();
    return (first + last) || (u.email ?? '?').charAt(0).toUpperCase();
}

const ROLE_CHIP_CLASSES: Record<string, string> = {
    Administrator:          'bg-purple-900/40 border-purple-700/50 text-purple-300',
    AuditManager:           'bg-blue-900/40 border-blue-700/50 text-blue-300',
    TemplateAdmin:          'bg-indigo-900/40 border-indigo-700/50 text-indigo-300',
    FieldAuditor:           'bg-teal-900/40 border-teal-700/50 text-teal-300',
    AuditAdmin:             'bg-blue-900/40 border-blue-700/50 text-blue-300',
    AuditReviewer:          'bg-cyan-900/40 border-cyan-700/50 text-cyan-300',
    ExecutiveViewer:        'bg-amber-900/40 border-amber-700/50 text-amber-300',
    CorrectiveActionOwner:  'bg-orange-900/40 border-orange-700/50 text-orange-300',
    ReadOnlyViewer:         'bg-slate-700/60 border-slate-600/50 text-slate-300',
};
function roleChipClass(roleName?: string | null) {
    return ROLE_CHIP_CLASSES[roleName ?? ''] ?? 'bg-slate-700/60 border-slate-600/50 text-slate-300';
}

onMounted(loadData);
</script>

<style scoped>
.user-row-actions {
    display: flex;
    gap: 2px;
    justify-content: flex-end;
    opacity: 0;
    transition: opacity 0.15s ease;
}
:deep(tr:hover) .user-row-actions { opacity: 1; }

.add-role-btn {
    opacity: 0;
    transition: opacity 0.15s ease;
}
:deep(tr:hover) .add-role-btn { opacity: 1; }
</style>

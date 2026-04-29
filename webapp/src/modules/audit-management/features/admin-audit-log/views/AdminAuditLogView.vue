<template>
    <div class="flex flex-col min-h-0">

        <BasePageHeader
            icon="pi pi-shield"
            title="Audit Log"
            subtitle="Non-repudiation log — every action and every database change"
        >
            <Button icon="pi pi-refresh" outlined size="small" :loading="loading" @click="loadData" />
        </BasePageHeader>

        <div class="flex flex-col gap-4 p-4 min-h-0 flex-1">

            <!-- ── Stats ───────────────────────────────────────────────────────── -->
            <div class="grid grid-cols-2 gap-3">
                <div class="bg-slate-800 border border-slate-700 rounded-xl p-4 flex items-center gap-3">
                    <div class="w-10 h-10 rounded-lg bg-blue-500/15 flex items-center justify-center shrink-0">
                        <i class="pi pi-list text-blue-400" />
                    </div>
                    <div>
                        <div class="text-2xl font-bold text-white">{{ result?.totalActionLogs ?? 0 }}</div>
                        <div class="text-xs text-slate-400">Action Log Entries</div>
                    </div>
                </div>
                <div class="bg-slate-800 border border-slate-700 rounded-xl p-4 flex items-center gap-3">
                    <div class="w-10 h-10 rounded-lg bg-violet-500/15 flex items-center justify-center shrink-0">
                        <i class="pi pi-database text-violet-400" />
                    </div>
                    <div>
                        <div class="text-2xl font-bold text-white">{{ result?.totalTrailLogs ?? 0 }}</div>
                        <div class="text-xs text-slate-400">Change Trail Entries</div>
                    </div>
                </div>
            </div>

            <!-- ── Filter Bar ──────────────────────────────────────────────────── -->
            <div class="bg-slate-800 border border-slate-700 rounded-xl p-4 flex flex-wrap gap-3 items-end">
                <div class="relative flex-1 min-w-[200px]">
                    <i class="pi pi-search absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm pointer-events-none z-10" />
                    <InputText
                        v-model="filters.search"
                        placeholder="Search descriptions, IDs..."
                        size="small"
                        class="w-full pl-8"
                    />
                </div>
                <InputText
                    v-model="filters.userEmail"
                    placeholder="User email"
                    size="small"
                    class="min-w-[180px]"
                />
                <Select
                    v-model="filters.entityType"
                    :options="entityTypeOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="Entity type"
                    size="small"
                    class="min-w-[160px]"
                    show-clear
                />
                <Select
                    v-model="filters.action"
                    :options="actionOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="Action"
                    size="small"
                    class="min-w-[140px]"
                    show-clear
                />
                <DatePicker
                    v-model="filters.dateFrom"
                    placeholder="From date"
                    size="small"
                    show-icon
                    class="min-w-[160px]"
                />
                <DatePicker
                    v-model="filters.dateTo"
                    placeholder="To date"
                    size="small"
                    show-icon
                    class="min-w-[160px]"
                />
                <Button label="Search" icon="pi pi-search" size="small" @click="applyFilters" />
                <Button label="Clear" icon="pi pi-times" outlined size="small" @click="clearFilters" />
            </div>

            <!-- ── Tabs ────────────────────────────────────────────────────────── -->
            <Tabs v-model:value="activeTab">
                <TabList>
                    <Tab value="actions">
                        <i class="pi pi-list mr-2" />
                        Action Log
                        <Badge v-if="result" :value="result.totalActionLogs" class="ml-2" severity="info" />
                    </Tab>
                    <Tab value="trail">
                        <i class="pi pi-database mr-2" />
                        Change Trail
                        <Badge v-if="result" :value="result.totalTrailLogs" class="ml-2" severity="secondary" />
                    </Tab>
                </TabList>

                <TabPanels>

                    <!-- Action Log tab -->
                    <TabPanel value="actions">
                        <DataTable
                            v-if="activeTab === 'actions'"
                            :value="result?.actionLogs ?? []"
                            :loading="loading"
                            row-hover
                            striped-rows
                            size="small"
                            class="text-sm"
                            dataKey="id"
                            v-model:expandedRows="expandedActionRows"
                        >
                            <Column expander style="width: 3rem" />
                            <Column field="timestamp" header="Time" style="width: 170px">
                                <template #body="{ data }">
                                    <span class="text-slate-300 text-xs font-mono">{{ formatTs(data.timestamp) }}</span>
                                </template>
                            </Column>
                            <Column field="severity" header="Sev" style="width: 80px">
                                <template #body="{ data }">
                                    <Tag :value="data.severity" :severity="severityTag(data.severity)" size="small" />
                                </template>
                            </Column>
                            <Column field="performedBy" header="User">
                                <template #body="{ data }">
                                    <span class="text-slate-200 text-xs">{{ data.performedBy }}</span>
                                </template>
                            </Column>
                            <Column field="action" header="Action">
                                <template #body="{ data }">
                                    <span class="font-mono text-xs text-cyan-400">{{ data.action }}</span>
                                </template>
                            </Column>
                            <Column field="entityType" header="Entity">
                                <template #body="{ data }">
                                    <span class="text-slate-300 text-xs">{{ data.entityType }}</span>
                                    <span v-if="data.entityId" class="text-slate-500 text-xs ml-1">#{{ data.entityId }}</span>
                                </template>
                            </Column>
                            <Column field="description" header="Description">
                                <template #body="{ data }">
                                    <span class="text-slate-300 text-xs">{{ data.description }}</span>
                                </template>
                            </Column>
                            <Column field="ipAddress" header="IP" style="width: 130px">
                                <template #body="{ data }">
                                    <span class="font-mono text-xs text-slate-500">{{ data.ipAddress ?? '—' }}</span>
                                </template>
                            </Column>
                            <template #expansion="{ data }">
                                <div class="p-3 bg-slate-900 rounded text-xs font-mono text-slate-300 whitespace-pre-wrap">
                                    {{ data.description }}
                                </div>
                            </template>
                            <template #empty>
                                <div class="text-center py-8 text-slate-500">No action log entries match the current filters.</div>
                            </template>
                        </DataTable>
                    </TabPanel>

                    <!-- Change Trail tab -->
                    <TabPanel value="trail">
                        <DataTable
                            v-if="activeTab === 'trail'"
                            :value="result?.trailLogs ?? []"
                            :loading="loading"
                            row-hover
                            striped-rows
                            size="small"
                            class="text-sm"
                            dataKey="id"
                            v-model:expandedRows="expandedTrailRows"
                        >
                            <Column expander style="width: 3rem" />
                            <Column field="timestamp" header="Time" style="width: 170px">
                                <template #body="{ data }">
                                    <span class="text-slate-300 text-xs font-mono">{{ formatTs(data.timestamp) }}</span>
                                </template>
                            </Column>
                            <Column field="action" header="Op" style="width: 80px">
                                <template #body="{ data }">
                                    <Tag :value="data.action" :severity="opTag(data.action)" size="small" />
                                </template>
                            </Column>
                            <Column field="userEmail" header="User">
                                <template #body="{ data }">
                                    <span class="text-slate-200 text-xs">{{ data.userEmail }}</span>
                                </template>
                            </Column>
                            <Column field="entityType" header="Entity">
                                <template #body="{ data }">
                                    <span class="text-slate-300 text-xs">{{ data.entityType }}</span>
                                    <span class="text-slate-500 text-xs ml-1">#{{ data.entityId }}</span>
                                </template>
                            </Column>
                            <Column field="changedColumns" header="Changed Fields">
                                <template #body="{ data }">
                                    <span class="text-xs text-amber-400 font-mono">{{ data.changedColumns ?? '—' }}</span>
                                </template>
                            </Column>
                            <Column field="ipAddress" header="IP" style="width: 130px">
                                <template #body="{ data }">
                                    <span class="font-mono text-xs text-slate-500">{{ data.ipAddress ?? '—' }}</span>
                                </template>
                            </Column>
                            <template #expansion="{ data }">
                                <div class="grid grid-cols-2 gap-3 p-3">
                                    <div v-if="data.oldValues">
                                        <div class="text-xs text-slate-400 mb-1 uppercase tracking-wide">Before</div>
                                        <pre class="bg-red-950/40 border border-red-900/40 rounded p-2 text-xs text-red-300 overflow-auto max-h-48">{{ prettyJson(data.oldValues) }}</pre>
                                    </div>
                                    <div v-if="data.newValues">
                                        <div class="text-xs text-slate-400 mb-1 uppercase tracking-wide">After</div>
                                        <pre class="bg-emerald-950/40 border border-emerald-900/40 rounded p-2 text-xs text-emerald-300 overflow-auto max-h-48">{{ prettyJson(data.newValues) }}</pre>
                                    </div>
                                    <div v-if="!data.oldValues && !data.newValues" class="col-span-2 text-slate-500 text-xs">
                                        No field-level diff available for this entry.
                                    </div>
                                </div>
                            </template>
                            <template #empty>
                                <div class="text-center py-8 text-slate-500">No change trail entries match the current filters.</div>
                            </template>
                        </DataTable>
                    </TabPanel>

                </TabPanels>
            </Tabs>

            <!-- ── Pagination ──────────────────────────────────────────────────── -->
            <div class="flex items-center justify-between mt-2">
                <span class="text-xs text-slate-500">
                    Page {{ page }} of {{ totalPages }} ({{ activeTab === 'actions' ? (result?.totalActionLogs ?? 0) : (result?.totalTrailLogs ?? 0) }} entries)
                </span>
                <Paginator
                    :rows="pageSize"
                    :total-records="activeTab === 'actions' ? (result?.totalActionLogs ?? 0) : (result?.totalTrailLogs ?? 0)"
                    :first="(page - 1) * pageSize"
                    @page="onPage"
                    template="PrevPageLink PageLinks NextPageLink"
                />
            </div>

        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { AuditClient } from '@/apiclient/auditClient';
import type { AuditLogsResult } from '@/apiclient/auditClient';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useApiStore } from '@/stores/apiStore';

const apiStore  = useApiStore();
const loading   = ref(false);
const activeTab = ref<'actions' | 'trail'>('actions');

function getClient() {
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}
const page      = ref(1);
const pageSize  = 50;

const result = ref<AuditLogsResult | null>(null);

const expandedActionRows = ref<any[]>([]);
const expandedTrailRows  = ref<any[]>([]);

const filters = ref({
    search:     '',
    userEmail:  '',
    entityType: null as string | null,
    action:     null as string | null,
    dateFrom:   null as Date | null,
    dateTo:     null as Date | null,
});

const entityTypeOptions = [
    { label: 'Audit',                  value: 'Audit' },
    { label: 'CorrectiveAction',       value: 'CorrectiveAction' },
    { label: 'User',                   value: 'User' },
    { label: 'UserRole',               value: 'UserRole' },
    { label: 'AuditTemplateVersion',   value: 'AuditTemplateVersion' },
    { label: 'Division',               value: 'Division' },
    { label: 'AuditFinding',           value: 'AuditFinding' },
];

const actionOptions = [
    { label: 'CreateAudit',            value: 'CreateAudit' },
    { label: 'DeleteAudit',            value: 'DeleteAudit' },
    { label: 'SubmitAudit',            value: 'SubmitAudit' },
    { label: 'CloseAudit',             value: 'CloseAudit' },
    { label: 'ReopenAudit',            value: 'ReopenAudit' },
    { label: 'AssignCorrectiveAction', value: 'AssignCorrectiveAction' },
    { label: 'CloseCorrectiveAction',  value: 'CloseCorrectiveAction' },
    { label: 'Insert',                 value: 'Insert' },
    { label: 'Update',                 value: 'Update' },
    { label: 'Delete',                 value: 'Delete' },
];

const totalPages = computed(() => {
    const total = activeTab.value === 'actions'
        ? (result.value?.totalActionLogs ?? 0)
        : (result.value?.totalTrailLogs ?? 0);
    return Math.max(1, Math.ceil(total / pageSize));
});

async function loadData() {
    loading.value = true;
    try {
        result.value = await getClient().getAuditLogs({
            search:     filters.value.search     || null,
            userEmail:  filters.value.userEmail  || null,
            entityType: filters.value.entityType || null,
            action:     filters.value.action     || null,
            dateFrom:   filters.value.dateFrom   ? filters.value.dateFrom.toISOString() : null,
            dateTo:     filters.value.dateTo     ? filters.value.dateTo.toISOString()   : null,
            page:       page.value,
            pageSize,
        });
    } catch (e) {
        console.error('Failed to load audit logs', e);
    } finally {
        loading.value = false;
    }
}

function applyFilters() {
    page.value = 1;
    loadData();
}

function clearFilters() {
    filters.value = { search: '', userEmail: '', entityType: null, action: null, dateFrom: null, dateTo: null };
    page.value = 1;
    loadData();
}

function onPage(event: { page: number }) {
    page.value = event.page + 1;
    loadData();
}


function formatTs(ts: string): string {
    const d = new Date(ts);
    return d.toLocaleDateString('en-US', { month: '2-digit', day: '2-digit', year: '2-digit' })
        + ' ' + d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

function prettyJson(raw: string | null): string {
    if (!raw) return '';
    try { return JSON.stringify(JSON.parse(raw), null, 2); }
    catch { return raw; }
}

function severityTag(sev: string): 'info' | 'warn' | 'danger' | 'secondary' {
    if (sev === 'Error')   return 'danger';
    if (sev === 'Warning') return 'warn';
    return 'info';
}

function opTag(op: string): 'success' | 'danger' | 'warn' | 'secondary' {
    if (op === 'Insert') return 'success';
    if (op === 'Delete') return 'danger';
    if (op === 'Update') return 'warn';
    return 'secondary';
}

onMounted(loadData);
</script>

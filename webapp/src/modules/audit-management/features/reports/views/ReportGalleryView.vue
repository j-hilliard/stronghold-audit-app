<template>
    <div>
        <BasePageHeader
            title="Generate Report"
            subtitle="Choose a template, configure options, and download a PDF"
            icon="pi pi-file-pdf"
        >
            <Button
                label="Report Composer"
                icon="pi pi-file-edit"
                severity="secondary"
                outlined
                size="small"
                @click="router.push('/audit-management/reports/composer')"
            />
            <Button
                label="Scheduled Deliveries"
                icon="pi pi-calendar"
                severity="secondary"
                outlined
                size="small"
                @click="router.push('/audit-management/reports/scheduled')"
            />
        </BasePageHeader>

        <!-- Template gallery grid -->
        <div class="p-6">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
                <button
                    v-for="tpl in templates"
                    :key="tpl.id"
                    class="template-card text-left"
                    @click="selectTemplate(tpl)"
                >
                    <!-- Color accent bar -->
                    <div class="h-1.5 rounded-t-xl" :style="`background:${tpl.accentColor}`" />
                    <div class="p-5">
                        <div class="flex items-start justify-between gap-3 mb-3">
                            <div class="template-icon" :style="`background:${tpl.accentColor}22; color:${tpl.accentColor};`">
                                <i :class="tpl.icon" class="text-xl" />
                            </div>
                            <span class="template-tag">{{ tpl.tag }}</span>
                        </div>
                        <h3 class="text-white font-semibold text-base mb-1">{{ tpl.name }}</h3>
                        <p class="text-slate-400 text-xs leading-relaxed">{{ tpl.description }}</p>
                        <div class="mt-4 flex items-center gap-2 text-xs text-slate-500">
                            <i class="pi pi-file-pdf" />
                            <span>{{ tpl.landscape ? 'A4 Landscape' : 'A4 Portrait' }}</span>
                            <span class="mx-1">·</span>
                            <i class="pi pi-chart-bar" />
                            <span>{{ tpl.pages }}</span>
                        </div>
                    </div>
                </button>
            </div>
        </div>

        <!-- Config dialog -->
        <Dialog
            v-model:visible="showConfig"
            :header="selectedTemplate ? `Generate — ${selectedTemplate.name}` : 'Generate Report'"
            modal
            class="report-config-dialog"
            :style="{ width: '480px' }"
            :pt="{ mask: { class: 'backdrop-blur-sm' } }"
        >
            <div v-if="selectedTemplate" class="space-y-5 py-2">
                <!-- Division -->
                <div>
                    <label class="field-label">Division</label>
                    <Dropdown
                        v-model="config.divisionId"
                        :options="[{ id: null, code: 'All Divisions' }, ...store.divisions]"
                        option-label="code"
                        option-value="id"
                        class="w-full"
                        placeholder="All Divisions"
                    />
                </div>

                <!-- Date range -->
                <div>
                    <label class="field-label">Date Range</label>
                    <div class="flex gap-2 mb-2">
                        <button
                            v-for="preset in datePresets"
                            :key="preset.id"
                            :class="['preset-btn', activePreset === preset.id ? 'preset-btn--active' : '']"
                            @click="applyPreset(preset)"
                        >{{ preset.label }}</button>
                    </div>
                    <div class="flex gap-2">
                        <Calendar
                            v-model="config.dateFrom"
                            placeholder="From"
                            dateFormat="yy-mm-dd"
                            class="flex-1"
                            :show-clear="!!config.dateFrom"
                            @date-select="activePreset = 'custom'"
                            @clear-click="activePreset = 'custom'"
                        />
                        <Calendar
                            v-model="config.dateTo"
                            placeholder="To"
                            dateFormat="yy-mm-dd"
                            class="flex-1"
                            :show-clear="!!config.dateTo"
                            @date-select="activePreset = 'custom'"
                            @clear-click="activePreset = 'custom'"
                        />
                    </div>
                </div>

                <!-- Title override -->
                <div>
                    <label class="field-label">Report Title <span class="text-slate-500">(optional)</span></label>
                    <InputText v-model="config.title" class="w-full" :placeholder="defaultTitle" />
                </div>

                <!-- Primary color -->
                <div>
                    <label class="field-label">Accent Color</label>
                    <div class="flex items-center gap-3">
                        <div
                            class="w-8 h-8 rounded-md border border-slate-600 cursor-pointer"
                            :style="`background:${config.primaryColor}`"
                            title="Click to change"
                        />
                        <InputText v-model="config.primaryColor" class="flex-1 font-mono text-sm" placeholder="#1e3a5f" />
                        <div class="flex gap-1.5">
                            <button
                                v-for="c in colorSwatches"
                                :key="c"
                                class="w-6 h-6 rounded-md border-2 transition-all"
                                :style="`background:${c}; border-color: ${config.primaryColor === c ? '#fff' : 'transparent'}`"
                                @click="config.primaryColor = c"
                            />
                        </div>
                    </div>
                </div>
            </div>

            <template #footer>
                <div class="flex justify-end gap-2">
                    <Button label="Cancel" severity="secondary" outlined @click="showConfig = false" />
                    <Button
                        label="Generate PDF"
                        icon="pi pi-download"
                        :loading="generating"
                        :disabled="generating"
                        @click="generate"
                    />
                </div>
            </template>
        </Dialog>

        <Toast />
    </div>
</template>

<script setup lang="ts">
import { computed, ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import Button from 'primevue/button';
import Dialog from 'primevue/dialog';
import Dropdown from 'primevue/dropdown';
import Calendar from 'primevue/calendar';
import InputText from 'primevue/inputtext';
import Toast from 'primevue/toast';
import { useToast } from 'primevue/usetoast';
import BasePageHeader from '@/components/layout/BasePageHeader.vue';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useReportGenerate } from '../composables/useReportGenerate';

const router = useRouter();
const store  = useAuditStore();
const toast  = useToast();
const { generateReport, generating } = useReportGenerate();

// ── Template definitions ───────────────────────────────────────────────────────

const templates = [
    {
        id: 'annual-review',
        name: 'Annual Review',
        description: 'Multi-page year-in-review with KPI overview, trend charts, section breakdown, and audit log. Ideal for client and executive presentations.',
        icon: 'pi pi-star',
        tag: 'Comprehensive',
        accentColor: '#2563eb',
        landscape: true,
        pages: 'Multi-page',
    },
    {
        id: 'quarterly-summary',
        name: 'Quarterly Summary',
        description: 'Concise quarterly performance brief with 4 KPI cards, weekly trend line, and section breakdown. Perfect for management review.',
        icon: 'pi pi-chart-line',
        tag: 'Management',
        accentColor: '#22c55e',
        landscape: false,
        pages: '2–3 pages',
    },
    {
        id: 'post-audit-summary',
        name: 'Post-Audit Summary',
        description: 'Single-division audit summary with score banner, section findings, and full audit log. Great for quick division-level reporting.',
        icon: 'pi pi-clipboard',
        tag: 'Operations',
        accentColor: '#8b5cf6',
        landscape: false,
        pages: '1–2 pages',
    },
    {
        id: 'ncr-report',
        name: 'NCR Report',
        description: 'Table-focused non-conformance report listing findings by section with corrective action status. Designed for safety team review.',
        icon: 'pi pi-exclamation-triangle',
        tag: 'Safety',
        accentColor: '#ef4444',
        landscape: false,
        pages: 'Variable',
    },
    {
        id: 'executive-dashboard',
        name: 'Executive Dashboard',
        description: 'One-page company-wide KPI snapshot with division comparison chart and top risk categories. Ideal for C-suite and client-facing reporting.',
        icon: 'pi pi-th-large',
        tag: 'Executive',
        accentColor: '#f59e0b',
        landscape: true,
        pages: '1–2 pages',
    },
    {
        id: 'ca-aging',
        name: 'CA Aging Report',
        description: 'Open corrective actions sorted by age with overdue flagging. Includes aging KPIs and full CA detail table for operations review.',
        icon: 'pi pi-clock',
        tag: 'Compliance',
        accentColor: '#06b6d4',
        landscape: false,
        pages: 'Variable',
    },
] as const;

type Template = typeof templates[number];

// ── Config state ───────────────────────────────────────────────────────────────

const showConfig       = ref(false);
const selectedTemplate = ref<Template | null>(null);
const activePreset     = ref<string>('last30');

const config = reactive({
    divisionId:   null as number | null,
    dateFrom:     null as Date | null,
    dateTo:       null as Date | null,
    title:        '',
    primaryColor: '#1e3a5f',
});

const colorSwatches = ['#1e3a5f', '#2563eb', '#22c55e', '#ef4444', '#8b5cf6', '#f59e0b', '#0f172a'];

const datePresets = [
    { id: 'last30',    label: 'Last 30d',   fromDays: 30 },
    { id: 'thisqtr',   label: 'This Qtr',   fromFn: thisQuarterStart },
    { id: 'lastqtr',   label: 'Last Qtr',   fromFn: lastQuarterStart, toFn: lastQuarterEnd },
    { id: 'thisyear',  label: 'This Year',  fromFn: thisYearStart },
    { id: 'lastyear',  label: 'Last Year',  fromFn: lastYearStart, toFn: lastYearEnd },
];

const defaultTitle = computed(() => {
    if (!selectedTemplate.value) return '';
    const div = config.divisionId
        ? store.divisions.find(d => d.id === config.divisionId)?.code ?? 'Division'
        : 'All Divisions';
    const names: Record<string, string> = {
        'annual-review':       `${div} — Annual Review`,
        'quarterly-summary':   `${div} — Quarterly Summary`,
        'post-audit-summary':  `${div} — Audit Summary`,
        'ncr-report':          `${div} — Non-Conformance Report`,
        'executive-dashboard': `Executive Dashboard`,
        'ca-aging':            `Corrective Action Aging Report`,
    };
    return names[selectedTemplate.value.id] ?? selectedTemplate.value.name;
});

function selectTemplate(tpl: Template) {
    selectedTemplate.value = tpl;
    config.primaryColor    = tpl.accentColor;
    applyPreset(datePresets[0]);
    showConfig.value = true;
}

function applyPreset(preset: typeof datePresets[0]) {
    activePreset.value = preset.id;
    const today = new Date();
    today.setHours(23, 59, 59, 0);

    config.dateTo = preset.toFn ? preset.toFn() : today;

    if ('fromDays' in preset) {
        const from = new Date();
        from.setDate(from.getDate() - (preset.fromDays as number));
        from.setHours(0, 0, 0, 0);
        config.dateFrom = from;
    } else if (preset.fromFn) {
        config.dateFrom = preset.fromFn();
    }
}

async function generate() {
    if (!selectedTemplate.value) return;
    try {
        await generateReport({
            templateId:   selectedTemplate.value.id,
            divisionId:   config.divisionId,
            dateFrom:     config.dateFrom ? fmt(config.dateFrom) : null,
            dateTo:       config.dateTo   ? fmt(config.dateTo)   : null,
            title:        config.title    || undefined,
            primaryColor: config.primaryColor,
        });
        showConfig.value = false;
        toast.add({ severity: 'success', summary: 'PDF generated', detail: 'Your download should start automatically.', life: 4000 });
    } catch {
        toast.add({ severity: 'error', summary: 'Generation failed', detail: 'Check your filters and try again.', life: 6000 });
    }
}

// ── Date helpers ───────────────────────────────────────────────────────────────

function fmt(d: Date) {
    return d.toISOString().split('T')[0];
}

function thisQuarterStart() {
    const now = new Date();
    const m = Math.floor(now.getMonth() / 3) * 3;
    const d = new Date(now.getFullYear(), m, 1, 0, 0, 0);
    return d;
}

function lastQuarterStart() {
    const now = new Date();
    const m = Math.floor(now.getMonth() / 3) * 3 - 3;
    const year = m < 0 ? now.getFullYear() - 1 : now.getFullYear();
    return new Date(year, (m + 12) % 12, 1, 0, 0, 0);
}

function lastQuarterEnd() {
    const start = lastQuarterStart();
    const end = new Date(start.getFullYear(), start.getMonth() + 3, 0, 23, 59, 59);
    return end;
}

function thisYearStart() {
    return new Date(new Date().getFullYear(), 0, 1, 0, 0, 0);
}

function lastYearStart() {
    return new Date(new Date().getFullYear() - 1, 0, 1, 0, 0, 0);
}

function lastYearEnd() {
    return new Date(new Date().getFullYear() - 1, 11, 31, 23, 59, 59);
}

// Load divisions
store.loadDivisions();
</script>

<style scoped>
.template-card {
    background: rgb(30, 41, 59);
    border: 1px solid rgb(51, 65, 85);
    border-radius: 12px;
    overflow: hidden;
    transition: box-shadow 0.2s ease, transform 0.2s ease, border-color 0.2s ease;
    cursor: pointer;
    width: 100%;
}
.template-card:hover {
    box-shadow: 0 0 0 1px rgba(99, 179, 237, 0.4), 0 12px 32px rgba(0, 0, 0, 0.5);
    transform: translateY(-3px);
    border-color: rgba(99, 179, 237, 0.4);
}
.template-icon {
    width: 44px;
    height: 44px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
}
.template-tag {
    font-size: 10px;
    font-weight: 600;
    color: #94a3b8;
    background: rgb(51, 65, 85);
    padding: 2px 8px;
    border-radius: 999px;
    letter-spacing: 0.5px;
    text-transform: uppercase;
    white-space: nowrap;
}
.field-label {
    display: block;
    font-size: 11px;
    font-weight: 600;
    color: #94a3b8;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    margin-bottom: 6px;
}
.preset-btn {
    padding: 4px 10px;
    border-radius: 6px;
    border: 1px solid rgb(71, 85, 105);
    background: transparent;
    color: #94a3b8;
    font-size: 11px;
    cursor: pointer;
    transition: background 0.15s, color 0.15s, border-color 0.15s;
    white-space: nowrap;
}
.preset-btn:hover { background: rgb(51, 65, 85); color: #e2e8f0; }
.preset-btn--active {
    background: rgb(37, 99, 235);
    border-color: rgb(37, 99, 235);
    color: #fff;
}
</style>

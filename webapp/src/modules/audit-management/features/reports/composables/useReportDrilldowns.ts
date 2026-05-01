import { computed, nextTick, reactive, ref } from 'vue';
import type { Ref } from 'vue';
import { useRouter } from 'vue-router';
import type { AuditReportDto } from '@/apiclient/auditClient';

interface DrilldownOptions {
    report:       Ref<AuditReportDto | null>;
    openQSummary: () => void;
}

export function useReportDrilldowns({ report, openQSummary }: DrilldownOptions) {
    const router = useRouter();

    // ── Tab state ─────────────────────────────────────────────────────────────
    const TABS = [
        { key: 'overview',      label: 'Overview'      },
        { key: 'action-items',  label: 'Action Items'  },
        { key: 'history',       label: 'Audit History' },
        { key: 'analysis',      label: 'Analysis'      },
        { key: 'performance',   label: 'Performance'   },
    ] as const;

    const activeTab = ref<'overview' | 'action-items' | 'history' | 'analysis' | 'performance'>('overview');
    const tabBarEl  = ref<HTMLElement | null>(null);

    function scrollToTabs() {
        nextTick(() => tabBarEl.value?.scrollIntoView({ behavior: 'smooth', block: 'start' }));
    }

    // ── Section collapse ──────────────────────────────────────────────────────
    const collapsed = reactive<Record<string, boolean>>({});
    function toggleSection(key: string) { collapsed[key] = !collapsed[key]; }
    const divisionHealthCollapsed = ref(true);

    // ── Drill-down state ──────────────────────────────────────────────────────
    const drillAuditor  = ref<string | null>(null);
    const drillLocation = ref<string | null>(null);
    const drillNcOnly   = ref(false);
    const drillWarnOnly = ref(false);
    const auditDetailCard = ref<any>(null);

    function scrollToAuditDetail() {
        nextTick(() => {
            const el = auditDetailCard.value?.$el ?? auditDetailCard.value;
            el?.scrollIntoView({ behavior: 'smooth', block: 'start' });
        });
    }

    function drillByAuditor(auditor: string) {
        drillAuditor.value  = auditor;
        drillLocation.value = null;
        collapsed.auditDetail = false;
        activeTab.value       = 'history';
        scrollToTabs();
    }

    function drillByLocation(location: string) {
        drillLocation.value = location;
        drillAuditor.value  = null;
        collapsed.auditDetail = false;
        activeTab.value       = 'history';
        scrollToTabs();
    }

    function drillByNcOnly() {
        drillNcOnly.value   = true;
        drillWarnOnly.value = false;
        drillAuditor.value  = null;
        drillLocation.value = null;
        collapsed.auditDetail = false;
        activeTab.value       = 'history';
        scrollToTabs();
    }

    function drillByWarnOnly() {
        drillWarnOnly.value = true;
        drillNcOnly.value   = false;
        drillAuditor.value  = null;
        drillLocation.value = null;
        collapsed.auditDetail = false;
        activeTab.value       = 'history';
        scrollToTabs();
    }

    function drillAllAudits() {
        drillNcOnly.value   = false;
        drillWarnOnly.value = false;
        drillAuditor.value  = null;
        drillLocation.value = null;
        collapsed.auditDetail = false;
        activeTab.value       = 'history';
        scrollToTabs();
    }

    const filteredAuditRows = computed(() => {
        let rows = report.value?.rows ?? [];
        if (drillAuditor.value)  rows = rows.filter(r => r.auditor  === drillAuditor.value);
        if (drillLocation.value) rows = rows.filter(r => r.location === drillLocation.value);
        if (drillNcOnly.value)   rows = rows.filter(r => r.nonConformingCount > 0);
        if (drillWarnOnly.value) rows = rows.filter(r => r.warningCount       > 0);
        return rows;
    });

    // ── Reports nav menu ──────────────────────────────────────────────────────
    const reportsMenuRef = ref<any>(null);

    const reportsMenuItems = [
        { label: 'Newsletter',        icon: 'pi pi-envelope',  command: () => router.push('/audit-management/newsletter')          },
        { label: 'Quarterly Summary', icon: 'pi pi-print',     command: openQSummary },
        { separator: true },
        { label: 'Report Composer',   icon: 'pi pi-file-edit', command: () => router.push('/audit-management/reports/composer')    },
        { label: 'Generate PDF',      icon: 'pi pi-file-pdf',  command: () => router.push('/audit-management/reports/gallery')     },
        { label: 'By Employee',       icon: 'pi pi-users',     command: () => router.push('/audit-management/reports/by-employee') },
    ];

    function toggleReportsMenu(event: Event) { reportsMenuRef.value?.toggle(event); }

    return {
        TABS, activeTab, tabBarEl, scrollToTabs,
        collapsed, toggleSection, divisionHealthCollapsed,
        drillAuditor, drillLocation, drillNcOnly, drillWarnOnly, auditDetailCard,
        scrollToAuditDetail,
        drillByAuditor, drillByLocation, drillByNcOnly, drillByWarnOnly, drillAllAudits,
        filteredAuditRows,
        reportsMenuRef, reportsMenuItems, toggleReportsMenu,
    };
}

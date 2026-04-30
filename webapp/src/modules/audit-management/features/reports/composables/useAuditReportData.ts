import { computed, onMounted, ref, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import type { AuditReportDto, ComplianceStatusDto } from '@/apiclient/auditClient';

export function useAuditReportData() {
    const router  = useRouter();
    const route   = useRoute();
    const store   = useAuditStore();
    const service = useAuditService();

    // ── State ─────────────────────────────────────────────────────────────────
    const loading            = ref(false);
    const report             = ref<AuditReportDto | null>(null);
    const complianceStatus   = ref<ComplianceStatusDto[]>([]);
    const exportingQs        = ref(false);
    const exportingNcr       = ref(false);
    const filterDivisionId   = ref<number | null>(null);
    const filterStatus       = ref<string | null>(null);
    const filterDateFrom     = ref<Date | null>(null);
    const filterDateTo       = ref<Date | null>(null);
    const filterSection      = ref<string | null>((route.query.section as string) || null);

    const STATUS_OPTIONS = [
        { label: 'Submitted', value: 'Submitted' },
        { label: 'Closed',    value: 'Closed'    },
        { label: 'Draft',     value: 'Draft'      },
    ];

    // ── API calls ─────────────────────────────────────────────────────────────
    async function loadReport() {
        loading.value = true;
        try {
            const from = filterDateFrom.value ? filterDateFrom.value.toISOString().split('T')[0] : null;
            const to   = filterDateTo.value   ? filterDateTo.value.toISOString().split('T')[0]   : null;
            report.value = await service.getAuditReport(
                filterDivisionId.value,
                filterStatus.value,
                from,
                to,
                filterSection.value,
            );
        } finally {
            loading.value = false;
        }
    }

    async function loadComplianceStatus() {
        try {
            complianceStatus.value = await service.getComplianceStatus();
        } catch { /* non-blocking */ }
    }

    onMounted(async () => {
        await Promise.all([store.loadDivisions(), loadReport(), loadComplianceStatus()]);
    });

    // ── Section filter ────────────────────────────────────────────────────────
    function toggleSectionFilter(sectionName: string) {
        filterSection.value = filterSection.value === sectionName ? null : sectionName;
        const q = { ...route.query };
        if (filterSection.value) q.section = filterSection.value;
        else delete q.section;
        router.replace({ query: q });
        loadReport();
    }

    function clearSectionFilter() {
        filterSection.value = null;
        const q = { ...route.query };
        delete q.section;
        router.replace({ query: q });
        loadReport();
    }

    function filterByDivisionCode(code: string) {
        const div = store.divisions.find(d => d.code === code);
        if (div) { filterDivisionId.value = div.id; loadReport(); }
    }

    function complianceStatusLabel(div: ComplianceStatusDto): string {
        if (div.status === 'NeverAudited') return 'Never audited';
        if (div.daysUntilDue == null || div.daysSinceLastAudit == null) return '';
        if (div.status === 'Overdue') return `${Math.abs(div.daysUntilDue)}d overdue`;
        if (div.status === 'DueSoon') return `Due in ${div.daysUntilDue}d`;
        return `Due in ${div.daysUntilDue}d`;
    }

    // ── Exports ───────────────────────────────────────────────────────────────
    function exportCsv() {
        if (!report.value?.rows.length) return;
        const headers = ['Audit #', 'Division', 'Date', 'Auditor', 'Job #', 'Location', 'Status', 'Score %', 'NCs', 'Warnings'];
        const rows = report.value.rows.map(r => [
            r.id, r.divisionCode, r.auditDate ?? '', r.auditor ?? '', r.jobNumber ?? '',
            r.location ?? '', r.status, r.scorePercent ?? '', r.nonConformingCount, r.warningCount,
        ]);
        const csv = [headers, ...rows].map(row => row.map(v => `"${String(v).replace(/"/g, '""')}"`).join(',')).join('\n');
        const divCode = store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'all';
        const from = filterDateFrom.value?.toISOString().split('T')[0] ?? '';
        const to   = filterDateTo.value?.toISOString().split('T')[0]   ?? '';
        const fileName = `audit-report-${divCode}${from ? `-${from}` : ''}${to ? `-${to}` : ''}.csv`;
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const url  = URL.createObjectURL(blob);
        const a    = document.createElement('a');
        a.href = url; a.download = fileName; a.click();
        URL.revokeObjectURL(url);
    }

    async function downloadExcel(endpoint: string, fileName: string, loadingRef: { value: boolean }) {
        loadingRef.value = true;
        try {
            const params: Record<string, string> = {};
            if (filterDivisionId.value) params.divisionId = String(filterDivisionId.value);
            if (filterDateFrom.value)   params.dateFrom   = filterDateFrom.value.toISOString();
            if (filterDateTo.value)     params.dateTo     = filterDateTo.value.toISOString();
            const blob    = await service.downloadBlob(endpoint, params);
            const blobUrl = URL.createObjectURL(new Blob([blob], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            }));
            const link    = document.createElement('a');
            link.href = blobUrl; link.download = fileName; link.click();
            URL.revokeObjectURL(blobUrl);
        } finally {
            loadingRef.value = false;
        }
    }

    function exportQuarterlySummary() {
        const divCode = store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'all';
        downloadExcel('/v1/audits/export/quarterly-summary', `quarterly-summary-${divCode}.xlsx`, exportingQs);
    }

    function exportNcrReport() {
        const divCode = store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? 'all';
        downloadExcel('/v1/audits/export/ncr-report', `ncr-report-${divCode}.xlsx`, exportingNcr);
    }

    const exportMenuItems = computed(() => [
        { label: 'Audit CSV',         icon: 'pi pi-file',       command: exportCsv              },
        { label: 'Quarterly Excel',   icon: 'pi pi-file-excel', command: exportQuarterlySummary },
        { label: 'NCR Excel',         icon: 'pi pi-file-excel', command: exportNcrReport        },
    ]);

    function openQSummary() {
        const divId   = filterDivisionId.value ?? '';
        const now     = new Date();
        const year    = now.getFullYear();
        const quarter = Math.ceil((now.getMonth() + 1) / 3);
        window.open(`/audit-management/reports/quarterly-summary?divisionId=${divId}&year=${year}&quarter=${quarter}`, '_blank');
    }

    // ── Derived computeds ─────────────────────────────────────────────────────
    const activeFilterChips = computed(() => {
        const chips: { label: string; key: string }[] = [];
        if (filterDivisionId.value) {
            const div = store.divisions.find(d => d.id === filterDivisionId.value);
            if (div) chips.push({ label: `${div.code} division`, key: 'division' });
        }
        if (filterStatus.value)                              chips.push({ label: filterStatus.value, key: 'status' });
        if (filterDateFrom.value || filterDateTo.value) {
            const fmt  = (d: Date) => d.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
            const from = filterDateFrom.value ? fmt(filterDateFrom.value) : '';
            const to   = filterDateTo.value   ? fmt(filterDateTo.value)   : '';
            const label = from && to ? `${from} – ${to}` : from ? `From ${from}` : `To ${to}`;
            chips.push({ label, key: 'dates' });
        }
        if (filterSection.value) chips.push({ label: filterSection.value, key: 'section' });
        return chips;
    });

    const activeFilterDesc = computed(() => {
        const parts: string[] = [];
        if (filterDivisionId.value)
            parts.push(store.divisions.find(d => d.id === filterDivisionId.value)?.code ?? '');
        if (filterSection.value) parts.push(`Section: ${filterSection.value}`);
        if (filterDateFrom.value || filterDateTo.value) {
            const fmt  = (d: Date) => d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
            const from = filterDateFrom.value ? fmt(filterDateFrom.value) : '';
            const to   = filterDateTo.value   ? fmt(filterDateTo.value)   : '';
            parts.push(from && to ? `${from} – ${to}` : from ? `From ${from}` : `To ${to}`);
        }
        return parts.join(' · ');
    });

    const divisionHealthCards = computed(() => {
        const allDivisions   = store.divisions;
        const openCaByDiv    = new Map<string, number>();
        const lastAuditByDiv = new Map<string, string>();
        for (const row of report.value?.rows ?? []) {
            const current = lastAuditByDiv.get(row.divisionCode);
            if (!current || (row.auditDate && row.auditDate > current)) {
                if (row.auditDate) lastAuditByDiv.set(row.divisionCode, row.auditDate);
            }
        }
        const auditDivMap = new Map<number, string>();
        for (const row of report.value?.rows ?? []) auditDivMap.set(row.id, row.divisionCode);
        for (const ca of report.value?.openCorrectiveActions ?? []) {
            const divCode = auditDivMap.get(ca.auditId);
            if (divCode) openCaByDiv.set(divCode, (openCaByDiv.get(divCode) ?? 0) + 1);
        }
        const compStatusByCode = new Map<string, string>();
        for (const cs of complianceStatus.value) compStatusByCode.set(cs.divisionCode, cs.status);
        const divisionStats = buildDivisionStats();
        return divisionStats.map(ds => ({
            divisionCode:    ds.division,
            divisionName:    allDivisions.find(d => d.code === ds.division)?.name ?? '',
            auditCount:      ds.auditCount,
            avgScore:        ds.avgScore,
            totalNcs:        ds.totalNcs,
            openCaCount:     openCaByDiv.get(ds.division)  ?? 0,
            lastAuditDate:   lastAuditByDiv.get(ds.division) ?? null,
            complianceStatus: compStatusByCode.get(ds.division) ?? 'NoSchedule',
        }));
    });

    const recentAuditFeed = computed(() =>
        [...(report.value?.rows ?? [])].sort((a, b) => (b.auditDate ?? '').localeCompare(a.auditDate ?? '')).slice(0, 8)
    );

    const overdueAlertCas = computed(() =>
        (report.value?.openCorrectiveActions ?? []).filter(ca => ca.isOverdue)
    );

    function buildDivisionStats() {
        const map = new Map<string, { scores: number[]; ncs: number; count: number }>();
        for (const row of report.value?.rows ?? []) {
            const key = row.divisionCode;
            if (!map.has(key)) map.set(key, { scores: [], ncs: 0, count: 0 });
            const entry = map.get(key)!;
            if (row.scorePercent != null) entry.scores.push(row.scorePercent);
            entry.ncs += row.nonConformingCount;
            entry.count++;
        }
        return Array.from(map.entries()).map(([division, s]) => ({
            division, auditCount: s.count,
            avgScore: s.scores.length > 0 ? Math.round(s.scores.reduce((a, b) => a + b, 0) / s.scores.length * 10) / 10 : null as number | null,
            totalNcs: s.ncs,
        })).filter(r => r.avgScore != null).sort((a, b) => (b.avgScore ?? 0) - (a.avgScore ?? 0));
    }

    return {
        // State
        loading, report, complianceStatus, exportingQs, exportingNcr,
        filterDivisionId, filterStatus, filterDateFrom, filterDateTo, filterSection,
        STATUS_OPTIONS,
        // Computed
        activeFilterChips, activeFilterDesc, divisionHealthCards, recentAuditFeed, overdueAlertCas, exportMenuItems,
        // Functions
        loadReport, loadComplianceStatus, toggleSectionFilter, clearSectionFilter,
        filterByDivisionCode, complianceStatusLabel,
        exportCsv, exportQuarterlySummary, exportNcrReport, openQSummary,
    };
}

import { ref } from 'vue';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';

export interface ReportGenerateOptions {
    templateId: string;
    divisionId?: number | null;
    dateFrom?: string | null;
    dateTo?: string | null;
    title?: string;
    primaryColor?: string;
}

export function useReportGenerate() {
    const generating = ref(false);
    const error = ref<string | null>(null);

    async function generateReport(opts: ReportGenerateOptions): Promise<void> {
        generating.value = true;
        error.value = null;

        try {
            const blob = await useAuditService().postBlob('/v1/reports/generate', {
                templateId:   opts.templateId,
                divisionId:   opts.divisionId ?? null,
                dateFrom:     opts.dateFrom ?? null,
                dateTo:       opts.dateTo ?? null,
                title:        opts.title ?? null,
                primaryColor: opts.primaryColor ?? null,
            });
            const url  = URL.createObjectURL(blob);
            const a    = document.createElement('a');

            const divPart = opts.divisionId ? `-div${opts.divisionId}` : '';
            const datePart = opts.dateFrom ? `-${opts.dateFrom.slice(0, 7)}` : '';
            a.download = `${opts.templateId}${divPart}${datePart}.pdf`;
            a.href = url;
            a.click();
            URL.revokeObjectURL(url);
        } catch (e: any) {
            error.value = e?.response?.data?.message ?? e?.message ?? 'Failed to generate report.';
            throw e;
        } finally {
            generating.value = false;
        }
    }

    return { generateReport, generating, error };
}

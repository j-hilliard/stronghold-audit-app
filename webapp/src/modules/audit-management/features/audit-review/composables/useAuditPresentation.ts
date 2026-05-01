import { computed, toValue } from 'vue';
import type { MaybeRefOrGetter } from 'vue';
import type { AuditReviewDto } from '@/apiclient/auditClient';

export const LOGO_MAP: Record<string, string> = {
    ETS:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/ETS-Logo-color-full.png',
    STS:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/STS-Logo-Full-Clr@2x-2.png',
    STG:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/STG-Logo-Full-Clr@2x-3.png',
    SHI:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/SHI-Logo-Full-color.png',
    TKIE: 'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/TKIE-Logo-Full-Clr@2x-2.png',
    CSL:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/CSL-Logo-Full-Clr@2x-3.png',
    CC:   'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/CC-Logo-Full-Clr@2x-1.png',
};

export function useAuditPresentation(review: MaybeRefOrGetter<AuditReviewDto | null | undefined>) {
    const logoUrl = computed(() => {
        const code = toValue(review)?.divisionCode?.toUpperCase() ?? '';
        return LOGO_MAP[code] ?? null;
    });

    const scoreDisplay = computed(() => {
        const s = toValue(review)?.scorePercent;
        return s != null ? `${Math.round(s)}%` : '—';
    });

    const scoreColorClass = computed(() => {
        const s = toValue(review)?.scorePercent;
        if (s == null) return '';
        if (s >= 90) return 'score-green';
        if (s >= 75) return 'score-amber';
        return 'score-red';
    });

    function statusCellClass(status: string | null | undefined): string {
        switch (status) {
            case 'Conforming':    return 'status-conforming';
            case 'NonConforming': return 'status-nc';
            case 'Warning':       return 'status-warning';
            case 'NA':            return 'status-na';
            default:              return 'status-unanswered';
        }
    }

    function formatStatus(status: string | null | undefined): string {
        switch (status) {
            case 'Conforming':    return 'Conforming';
            case 'NonConforming': return 'Non-Conforming';
            case 'Warning':       return 'Warning';
            case 'NA':            return 'N/A';
            default:              return status ?? '';
        }
    }

    return { logoUrl, scoreDisplay, scoreColorClass, statusCellClass, formatStatus };
}

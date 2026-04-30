import { computed } from 'vue';
import type { Ref } from 'vue';
import type { AuditReviewDto } from '@/apiclient/auditClient';

export function useAuditReviewFormatting(review: Ref<AuditReviewDto | null>) {
    // ── Score display ─────────────────────────────────────────────────────────
    const scoreDisplay = computed(() => {
        const pct = review.value?.scorePercent;
        if (pct == null) return '—';
        return `${Math.round(pct)}%`;
    });

    const scoreColor = computed(() => {
        const pct = review.value?.scorePercent;
        if (pct == null) return 'text-slate-400';
        if (pct >= 90)   return 'text-emerald-400';
        if (pct >= 75)   return 'text-amber-400';
        return 'text-red-400';
    });

    const barColor = computed(() => {
        const pct = review.value?.scorePercent;
        if (pct == null) return 'bg-slate-500';
        if (pct >= 90)   return 'bg-emerald-500';
        if (pct >= 75)   return 'bg-amber-500';
        return 'bg-red-500';
    });

    // ── Score ring ────────────────────────────────────────────────────────────
    const ringCircumference = 2 * Math.PI * 42; // r=42

    const ringDashoffset = computed(() => {
        const pct = review.value?.scorePercent ?? 0;
        return ringCircumference - (pct / 100) * ringCircumference;
    });

    const ringColor = computed(() => {
        const pct = review.value?.scorePercent;
        if (pct == null) return '#475569';
        if (pct >= 90)   return '#34d399';
        if (pct >= 75)   return '#fbbf24';
        return '#f87171';
    });

    // ── Benchmark status ──────────────────────────────────────────────────────
    const benchmarkStatusClass = computed(() => {
        const pct    = review.value?.scorePercent;
        const target = review.value?.divisionScoreTarget;
        if (pct    == null) return 'bg-slate-700/60 border-slate-600 text-slate-300';
        if (target == null) return 'bg-slate-700/60 border-slate-600 text-slate-300';
        return pct >= Number(target)
            ? 'bg-emerald-900/50 border-emerald-700 text-emerald-300'
            : 'bg-red-900/50 border-red-700 text-red-300';
    });

    // ── Status helpers ────────────────────────────────────────────────────────
    function caSeverity(status: string): string {
        const map: Record<string, string> = { Open: 'danger', InProgress: 'warning', Closed: 'success' };
        return map[status] ?? 'secondary';
    }

    function statusDotClass(status: string | null | undefined): string {
        switch (status) {
            case 'Conforming':    return 'bg-emerald-500';
            case 'NonConforming': return 'bg-red-500';
            case 'Warning':       return 'bg-amber-500';
            case 'NA':            return 'bg-slate-500';
            default:              return 'bg-slate-700 border border-slate-500';
        }
    }

    function statusTextClass(status: string | null | undefined): string {
        switch (status) {
            case 'Conforming':    return 'text-emerald-400';
            case 'NonConforming': return 'text-red-400';
            case 'Warning':       return 'text-amber-400';
            case 'NA':            return 'text-slate-500';
            default:              return 'text-slate-600';
        }
    }

    function formatBytes(bytes: number): string {
        if (bytes < 1024)           return `${bytes} B`;
        if (bytes < 1024 * 1024)    return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    }

    return {
        scoreDisplay, scoreColor, barColor,
        ringCircumference, ringDashoffset, ringColor,
        benchmarkStatusClass,
        caSeverity, statusDotClass, statusTextClass, formatBytes,
    };
}

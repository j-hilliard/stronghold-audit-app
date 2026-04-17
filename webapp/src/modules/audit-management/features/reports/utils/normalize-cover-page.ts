// ─────────────────────────────────────────────────────────────────────────────
// normalizeCoverPageContent
//
// Applies safe defaults to partial or untrusted CoverPageContent.
// Called in:
//   - useReportEngine.ts → buildSingleBlock ('cover-page' case)
//   - CoverPageBlock.vue → onMounted guard
//
// This ensures:
//   - All boolean toggles are always defined
//   - All arrays always exist (never undefined)
//   - templateId resolves to a known template
//   - Strings are trimmed
//   - Missing values fall back to sensible defaults
// ─────────────────────────────────────────────────────────────────────────────

import { getCoverPageTemplate } from '../types/cover-template';
import type { CoverPageContent } from '../types/report-block';

const DEFAULT_STATS: CoverPageContent['stats'] = [
    { label: 'Audits Completed', value: '—' },
    { label: 'Avg Score', value: '—' },
    { label: 'Open CAs', value: '—' },
];

export function normalizeCoverPageContent(raw?: Partial<CoverPageContent>): CoverPageContent {
    // Validate templateId — getCoverPageTemplate falls back to templates[0] if not found
    const tpl = getCoverPageTemplate(raw?.templateId ?? '');

    return {
        schemaVersion: raw?.schemaVersion ?? 1,
        templateId: tpl.id,

        divisionName: raw?.divisionName?.trim() || 'Division Name',
        year: raw?.year?.trim() || String(new Date().getFullYear()),

        reportSubtitle: raw?.reportSubtitle?.trim() || undefined,
        preparedBy: raw?.preparedBy?.trim() || undefined,
        period: raw?.period?.trim() || undefined,
        tagline: raw?.tagline?.trim() || undefined,
        summaryText: raw?.summaryText?.trim() || undefined,
        locationsText: raw?.locationsText?.trim() || undefined,

        heroImageUrl: raw?.heroImageUrl?.trim() || undefined,
        chartImageUrl: raw?.chartImageUrl?.trim() || undefined,
        mapImageUrl: raw?.mapImageUrl?.trim() || undefined,

        // Module toggles — always booleans
        showStats: raw?.showStats ?? true,
        showCallout: raw?.showCallout ?? true,
        showMap: raw?.showMap ?? false,
        showLocations: raw?.showLocations ?? true,
        showAward: raw?.showAward ?? false,
        showHighlights: raw?.showHighlights ?? false,

        // Arrays — always defined
        stats: Array.isArray(raw?.stats) && raw.stats.length > 0
            ? raw.stats
            : DEFAULT_STATS,
        highlights: Array.isArray(raw?.highlights) ? raw.highlights : [],

        calloutTitle: raw?.calloutTitle?.trim() || undefined,
        calloutBody: raw?.calloutBody?.trim() || undefined,
        awardTitle: raw?.awardTitle?.trim() || undefined,
        awardDescription: raw?.awardDescription?.trim() || undefined,

        primaryColor: raw?.primaryColor || undefined,
        accentColor: raw?.accentColor || undefined,
    };
}

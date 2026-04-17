// ─────────────────────────────────────────────────────────────────────────────
// Cover Page Template Registry
//
// Provides the template definitions for the full-page `cover-page` block type.
// Designed for future migration into a page-first architecture:
//   ReportDocument → ReportPage → PageTemplateDefinition → PageRegion
//
// Do NOT import this from CoverBlock.vue or CoverTemplateGallery.vue.
// Those files use the lightweight `cover` block system — entirely separate.
// ─────────────────────────────────────────────────────────────────────────────

export interface CoverTemplateTheme {
    /** Page background color */
    primaryBg: string;
    /** Top band / header strip color */
    bandBg: string;
    /** Left sidebar background. Empty string means no sidebar in this template. */
    sidebarBg: string;
    /** Footer strip background */
    footerBg: string;
    /** Accent color for decorative elements (stripes, rules, stat highlights) */
    accent: string;
    textHeading: string;
    textBody: string;
    textMuted: string;
}

export interface CoverTemplateLayout {
    /** Height of the top color band in px */
    bandHeight: number;
    /** Width of the left sidebar in px. 0 = no sidebar. */
    sidebarWidth: number;
    /** Height of the footer strip in px */
    footerHeight: number;
    /** Decorative accent stripe placement */
    accentStripe: 'left-band' | 'top-page' | 'none';
    /** Width (left-band) or height (top-page) of the accent stripe in px */
    accentStripeSize: number;
    /** Alignment of the band/title content */
    bandAlign: 'left' | 'center';
}

export interface CoverTemplateDefinition {
    id: string;
    name: string;
    description: string;
    theme: CoverTemplateTheme;
    layout: CoverTemplateLayout;
}

// ── Template Registry ─────────────────────────────────────────────────────────

export const COVER_PAGE_TEMPLATES: CoverTemplateDefinition[] = [
    {
        id: 'stronghold-dark',
        name: 'Stronghold Dark',
        description: 'Navy · amber accent · left sidebar',
        theme: {
            primaryBg: '#0f1e36',
            bandBg: '#1e3a5f',
            sidebarBg: '#152c4a',
            footerBg: '#1e3a5f',
            accent: '#f59e0b',
            textHeading: '#ffffff',
            textBody: '#cbd5e1',
            textMuted: 'rgba(255,255,255,0.45)',
        },
        layout: {
            bandHeight: 292,
            sidebarWidth: 200,
            footerHeight: 48,
            accentStripe: 'left-band',
            accentStripeSize: 6,
            bandAlign: 'left',
        },
    },
    {
        id: 'safety-red',
        name: 'Safety Red',
        description: 'Dark red · bold top band · no sidebar',
        theme: {
            primaryBg: '#1c0a0a',
            bandBg: '#450a0a',
            sidebarBg: '',
            footerBg: '#450a0a',
            accent: '#fca5a5',
            textHeading: '#ffffff',
            textBody: '#fecaca',
            textMuted: 'rgba(255,255,255,0.45)',
        },
        layout: {
            bandHeight: 264,
            sidebarWidth: 0,
            footerHeight: 48,
            accentStripe: 'none',
            accentStripeSize: 0,
            bandAlign: 'left',
        },
    },
    {
        id: 'executive-minimal',
        name: 'Executive Minimal',
        description: 'Light page · navy accent · centered',
        theme: {
            primaryBg: '#f8fafc',
            bandBg: '#1e3a5f',
            sidebarBg: '',
            footerBg: '#e2e8f0',
            accent: '#1e3a5f',
            textHeading: '#0f172a',
            textBody: '#475569',
            textMuted: '#94a3b8',
        },
        layout: {
            bandHeight: 316,
            sidebarWidth: 0,
            footerHeight: 56,
            accentStripe: 'top-page',
            accentStripeSize: 6,
            bandAlign: 'center',
        },
    },
];

/**
 * Look up a template by id, falling back to the first template if not found.
 * Safe to call with any string — never throws.
 */
export function getCoverPageTemplate(id: string): CoverTemplateDefinition {
    return COVER_PAGE_TEMPLATES.find(t => t.id === id) ?? COVER_PAGE_TEMPLATES[0];
}

// ── Migration path: future page-first architecture ────────────────────────────
//
// TODO(page-first): When the composer migrates to a page-first model, replace
//   this file with a PageTemplateRegistry that maps CoverTemplateDefinition
//   to a full PageTemplateDefinition (document page → named regions → blocks).
//
// Planned migration steps (do not delete until all steps are complete):
//   1. Rename COVER_PAGE_TEMPLATES → PAGE_TEMPLATE_REGISTRY (or move to a
//      shared registry module) — keep getCoverPageTemplate() as a shim.
//   2. Introduce PageTemplateDefinition = { id, regions: PageRegion[] } where
//      PageRegion = { name, role, defaultStyle } so regions are declared, not
//      hardwired in component templates.
//   3. CoverPageContent → PageInstanceData stored per-page in pagesJson[], not
//      embedded inside a block in blocksJson[].
//   4. CoverPageBlock.vue → PageRenderer.vue that reads from regions[] and
//      renders each region via a RegionBlock component.
//   5. normalizeCoverPageContent → normalizePage(snapshot, templateId) housed
//      in utils/normalize-page.ts — same safe-defaults pattern.
//   6. CoverPageTemplatePicker → PageTemplatePicker with the same 3-column
//      thumbnail grid but keyed off PageTemplateDefinition[].
//   7. Keep `cover` (original lightweight block) entirely separate throughout —
//      it is NOT part of the page-first model.
//
// getCoverPageTemplate() can forward to the new registry until all callers are
// updated; delete it only after CoverPageBlock.vue is retired.
// ─────────────────────────────────────────────────────────────────────────────

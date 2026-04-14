// ─────────────────────────────────────────────────────────────────────────────
// Report Composer — typed block model
//
// Rules:
//   • ReportBlock[] is the frontend type. Backend stores it as opaque BlocksJson.
//   • Only useReportDraft.ts may call JSON.parse / JSON.stringify on blocks.
//   • style is ALWAYS preserved on regeneration — never overwritten.
//   • caption fields are ALWAYS preserved on regeneration.
//   • isEdited = true locks authored text (heading.text, narrative.text) from
//     being overwritten during regeneration. Applies only to those two block types.
//   • CoverBlock: BlockStyle.backgroundColor is intentionally unused.
//     Visual identity comes from content.primaryColor and content.backgroundImageUrl.
// ─────────────────────────────────────────────────────────────────────────────

export type BlockType =
    | 'cover'
    | 'heading'
    | 'kpi-grid'
    | 'chart-bar'
    | 'chart-line'
    | 'narrative'
    | 'callout'
    | 'ca-table'
    | 'image'
    | 'column-row'
    | 'divider'
    | 'spacer'
    | 'toc-sidebar'
    | 'oval-callout'
    | 'findings-category';

// ── Shared style model ────────────────────────────────────────────────────────
// Editable via the property panel. Never written by the regeneration engine.

export interface BlockStyle {
    /** Hex color or empty string = page default. Intentionally unused for 'cover' blocks. */
    backgroundColor?: string;
    textColor?: string;
    accentColor?: string;
    borderColor?: string;
    padding?: 'none' | 'sm' | 'md' | 'lg';
}

// ── Free-form layout model ────────────────────────────────────────────────────
// Controls position and size on the absolute-positioned canvas.
// Persisted in BlocksJson — never touched by the regeneration engine.

export interface BlockLayout {
    /** px from left edge of the page (794px wide). */
    x: number;
    /** px from top edge of the page. */
    y: number;
    /** px width. Minimum: 80. */
    width: number;
    /**
     * px height. 0 = auto (content-driven).
     * Becomes fixed when user drags the resize handle.
     */
    height: number;
    /** Stacking order. Higher = on top. */
    zIndex: number;
    /** When true, drag-to-move is disabled for this block. */
    locked?: boolean;
}

// ── Shared base ───────────────────────────────────────────────────────────────

export interface ReportBlockBase {
    /** client-side crypto.randomUUID() */
    id: string;
    type: BlockType;
    /**
     * true = authored text is locked against regeneration.
     * Only meaningful for 'heading' and 'narrative' blocks.
     * Always false for all other block types.
     */
    isEdited: boolean;
    /** Always persisted, never touched by the regeneration engine. */
    style: BlockStyle;
    /**
     * Free-form position and size on the canvas.
     * Required on all blocks. Missing layout is back-filled by ensureLayout()
     * in useReportDraft.ts when deserializing old drafts.
     */
    layout: BlockLayout;
    /**
     * true = block is part of a locked newsletter layout.
     * Newsletter blocks can be styled but not reordered or deleted.
     */
    isNewsletterBlock?: boolean;
}

// ── Cover ─────────────────────────────────────────────────────────────────────

export interface CoverContent {
    /** regenerated */
    divisionName: string;
    /** regenerated */
    divisionCode: string;
    /** regenerated from draft.period */
    period: string;
    /** regenerated from current user display name */
    preparedBy: string;
    /**
     * preserved — sole source of truth for the cover gradient color.
     * BlockStyle.backgroundColor is intentionally NOT used for cover blocks.
     */
    primaryColor: string;
    /** preserved — sole source of truth for cover background image */
    backgroundImageUrl: string;
    // ── Layout & sizing ────────────────────────────────────────────────────────
    /** Block min-height preset. Default: 'md' (220px). */
    coverHeight?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
    /** Division name font size. Default: 'xl'. */
    nameSize?: 'sm' | 'md' | 'lg' | 'xl' | '2xl';
    /** Text transform for the division name. Default: 'uppercase'. */
    nameTransform?: 'uppercase' | 'none';
    /** Opacity of the black overlay (0–80). Default: 40. */
    overlayOpacity?: number;
    // ── Newsletter template fields (all preserved) ────────────────────────────
    /** Small text rendered above the division name. e.g. "2022 Compliance" */
    subtitle?: string;
    /** Right-aligned text below the decorative rules. e.g. "A Year in Review" */
    tagline?: string;
    /**
     * Color for the large division name display.
     * Defaults to nameAccentColor → accentColor → amber (#f59e0b).
     */
    nameAccentColor?: string;
    /**
     * When true (default), renders horizontal rule lines above and below the division name.
     * Set to false to remove the lines entirely.
     */
    showDecorativeRules?: boolean;
}

export interface CoverBlock extends ReportBlockBase {
    type: 'cover';
    content: CoverContent;
}

// ── Heading ───────────────────────────────────────────────────────────────────

export interface HeadingContent {
    /** regenerated if isEdited === false; preserved if isEdited === true */
    text: string;
    level: 1 | 2 | 3;
    /** preserved — URL for a section-header banner background photo */
    backgroundImageUrl?: string;
    /** preserved — 0–80 opacity of the dark overlay on the banner; default 50 */
    overlayOpacity?: number;
    /** preserved — explicit font size override (px). When set, overrides the level-based size. */
    fontSize?: number;
    /** preserved — font weight override. Default: 600 (semibold). */
    fontWeight?: 400 | 500 | 600 | 700 | 800 | 900;
}

export interface HeadingBlock extends ReportBlockBase {
    type: 'heading';
    content: HeadingContent;
}

// ── KPI Grid ──────────────────────────────────────────────────────────────────

export interface KpiCard {
    label: string;
    /** Division-specific value */
    value: string | number;
    /** Company-wide parallel value — shown when showComparison === true */
    companyValue?: string | number;
    unit?: string;
    /** e.g. "+2% vs last quarter" — empty string if unavailable */
    delta?: string;
    variant: 'neutral' | 'good' | 'warn' | 'bad';
    /**
     * Set for per-section cards. Drives chart-line block sync:
     * enabled section cards get a corresponding chart-line block on Generate.
     */
    sectionName?: string;
    /**
     * Whether this card is visible in the rendered grid.
     * Toggled by the user in the property panel.
     * Drives line chart block inclusion on Generate.
     */
    enabled: boolean;
}

export interface KpiGridContent {
    /**
     * All available cards (summary + per-section), enabled or not.
     * Always regenerated from API data; enabled state is preserved from existing block.
     */
    cards: KpiCard[];
    /** When true, show company-wide value alongside division value on each card. */
    showComparison: boolean;
}

export interface KpiGridBlock extends ReportBlockBase {
    type: 'kpi-grid';
    content: KpiGridContent;
}

// ── Bar Chart ─────────────────────────────────────────────────────────────────

export interface ChartDataset {
    label: string;
    data: number[];
    backgroundColor?: string;
    borderColor?: string;
    borderDash?: number[];
}

export interface BarChartContent {
    /** preserved — initialized on block creation, user can rename */
    title: string;
    /** regenerated — section names from report.sectionBreakdown */
    labels: string[];
    /** regenerated — NC counts per dataset */
    datasets: ChartDataset[];
    /** preserved — user-authored annotation displayed below the chart */
    caption: string;
}

export interface BarChartBlock extends ReportBlockBase {
    type: 'chart-bar';
    content: BarChartContent;
}

// ── Line Chart ────────────────────────────────────────────────────────────────

export interface LineChartContent {
    /** preserved — key used to select the right section from SectionTrendsReportDto */
    sectionName: string;
    /** preserved — initialized on creation, user can rename */
    title: string;
    /** regenerated — quarter labels from sectionTrends.quarters */
    labels: string[];
    /** regenerated — division + company trend series */
    datasets: ChartDataset[];
    /** preserved — user-authored annotation */
    caption: string;
    /**
     * preserved — editable bullet list of real findings from this section.
     * Shown below the chart in the printed newsletter.
     * Maps to the "Examples:" boxes in the legacy Word newsletters.
     */
    examples: string;
}

export interface LineChartBlock extends ReportBlockBase {
    type: 'chart-line';
    content: LineChartContent;
}

// ── Narrative ─────────────────────────────────────────────────────────────────

export interface NarrativeContent {
    /**
     * HTML string (Tiptap output). Regenerated if isEdited === false;
     * preserved if isEdited === true. Plain text is valid (renders as-is).
     */
    text: string;
    /**
     * preserved — stored so "Regenerate ✨" can replay the same prompt context.
     * Cleared when user manually types over generated text (isEdited → true).
     */
    aiPromptContext?: string;
}

export interface NarrativeBlock extends ReportBlockBase {
    type: 'narrative';
    content: NarrativeContent;
}

// ── Callout ───────────────────────────────────────────────────────────────────

export interface CalloutContent {
    /** always preserved — no generated version */
    title: string;
    /** always preserved */
    body: string;
    variant: 'info' | 'warning' | 'success' | 'danger';
}

export interface CalloutBlock extends ReportBlockBase {
    type: 'callout';
    content: CalloutContent;
}

// ── CA Table ──────────────────────────────────────────────────────────────────

export interface CaTableRow {
    id: number;
    auditId: number;
    description: string;
    assignedTo: string;
    dueDate: string;
    status: string;
    isOverdue: boolean;
    daysOpen: number;
}

export interface CaTableContent {
    /** always regenerated from report.openCorrectiveActions */
    rows: CaTableRow[];
}

export interface CaTableBlock extends ReportBlockBase {
    type: 'ca-table';
    content: CaTableContent;
}

// ── Image ─────────────────────────────────────────────────────────────────────

export interface ImageContent {
    /** URL of the image — empty string shows placeholder */
    url: string;
    alt: string;
    /** optional caption displayed below the image */
    caption: string;
    /** full = w-full, half = w-1/2 centered */
    width: 'full' | 'half';
}

export interface ImageBlock extends ReportBlockBase {
    type: 'image';
    content: ImageContent;
}

// ── Column Row ────────────────────────────────────────────────────────────────

export type ColumnRatio = '30/70' | '50/50' | '60/40' | '40/60' | '70/30';
export type ColumnGap   = 'none' | 'sm' | 'md' | 'lg';

export interface ColumnRowContent {
    ratio: ColumnRatio;
    gap: ColumnGap;
    /** Blocks rendered in the left column. Cannot be column-row (no nesting). */
    leftBlocks: ReportBlock[];
    /** Blocks rendered in the right column. Cannot be column-row (no nesting). */
    rightBlocks: ReportBlock[];
}

export interface ColumnRowBlock extends ReportBlockBase {
    type: 'column-row';
    content: ColumnRowContent;
}

// ── Divider ───────────────────────────────────────────────────────────────────

export interface DividerContent {
    thickness: 1 | 2 | 4;
    variant: 'solid' | 'dashed' | 'dotted';
    /** Hex color. Defaults to slate-600 (#475569) if empty. */
    color?: string;
    marginY: 'sm' | 'md' | 'lg';
}

export interface DividerBlock extends ReportBlockBase {
    type: 'divider';
    content: DividerContent;
}

// ── Spacer ────────────────────────────────────────────────────────────────────

export interface SpacerContent {
    /** xs=8px  sm=16px  md=32px  lg=48px  xl=64px */
    height: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
}

export interface SpacerBlock extends ReportBlockBase {
    type: 'spacer';
    content: SpacerContent;
}

// ── TOC Sidebar ───────────────────────────────────────────────────────────────

export interface TocItem {
    heading: string;
    description?: string;
}

export interface TocSidebarContent {
    /** e.g. "INSIDE" */
    title: string;
    items: TocItem[];
    /** true = dark sidebar background (matches the physical newsletter template) */
    darkBackground: boolean;
}

export interface TocSidebarBlock extends ReportBlockBase {
    type: 'toc-sidebar';
    content: TocSidebarContent;
}

// ── Oval Callout ──────────────────────────────────────────────────────────────

export interface OvalCalloutContent {
    /** Bold title e.g. "strong-hold" */
    title: string;
    /** Italic phonetic line e.g. "/'strôNG.hōld/ noun." */
    phonetic?: string;
    /** Body definition / quote text */
    body: string;
    /** Hex background. Defaults to dark navy (#1e3a5f) if empty. */
    backgroundColor?: string;
    /** Hex text color. Defaults to white if empty. */
    textColor?: string;
    /** Oval width in px. Default: 300. */
    ovalWidth?: number;
    /** Oval height in px. Default: 200. */
    ovalHeight?: number;
}

export interface OvalCalloutBlock extends ReportBlockBase {
    type: 'oval-callout';
    content: OvalCalloutContent;
}

// ── Findings Category ─────────────────────────────────────────────────────────
// The dominant block type in the physical newsletters:
//   [Section Name header]
//   "Examples:"
//   • Bulleted findings list (rich text)

export interface FindingsCategoryContent {
    /**
     * Compliance section name. e.g. "Confined Space Procedures"
     * Regenerated from section breakdown; preserved if isEdited === true.
     */
    sectionName: string;
    /**
     * Rich text HTML (Tiptap). Typically a bullet list of finding examples.
     * Preserved across regeneration — never overwritten.
     */
    findings: string;
    /** When true, renders the "Examples:" label above the list. Default: true. */
    showExamplesLabel: boolean;
    /**
     * Accent color for the section header bar.
     * Defaults to brandBurgundy (#862633) to match newsletter templates.
     */
    accentColor?: string;
}

export interface FindingsCategoryBlock extends ReportBlockBase {
    type: 'findings-category';
    content: FindingsCategoryContent;
}

// ── Discriminated union ───────────────────────────────────────────────────────

export type ReportBlock =
    | CoverBlock
    | HeadingBlock
    | KpiGridBlock
    | BarChartBlock
    | LineChartBlock
    | NarrativeBlock
    | CalloutBlock
    | CaTableBlock
    | ImageBlock
    | ColumnRowBlock
    | DividerBlock
    | SpacerBlock
    | TocSidebarBlock
    | OvalCalloutBlock
    | FindingsCategoryBlock;

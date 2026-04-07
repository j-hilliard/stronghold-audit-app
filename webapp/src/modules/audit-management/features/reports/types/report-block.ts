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
    | 'image';

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
     * Initialized from localStorage newsletter template on block creation.
     * BlockStyle.backgroundColor is intentionally NOT used for cover blocks.
     */
    primaryColor: string;
    /** preserved — sole source of truth for cover background image */
    backgroundImageUrl: string;
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
    /** regenerated if isEdited === false; preserved if isEdited === true */
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
    | ImageBlock;

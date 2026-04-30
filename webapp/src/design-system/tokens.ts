/**
 * Design token constants — single source of truth for all semantic colors,
 * spacing, shadows, radius, and transitions. Values mirror tailwind.config.js
 * exactly so TypeScript code (Chart.js options, dynamic styles) stays in sync.
 *
 * Usage in Vue:  import { tokens } from '@/design-system'
 * Usage in CSS:  via Tailwind utility classes (bg-surface-2, text-danger, etc.)
 */

// ── Color tokens ──────────────────────────────────────────────────────────────

export const surface = {
    1: '#0f172a',   // navy-900  — main background
    2: '#1e293b',   // slate-800 — cards / panels
    3: '#273549',   // elevated  — dialogs, modals
} as const;

export const border = {
    subtle:  'rgba(148,163,184,0.12)',
    DEFAULT: 'rgba(148,163,184,0.20)',
    strong:  'rgba(148,163,184,0.30)',
} as const;

export const text = {
    primary:   '#f1f5f9',
    secondary: '#94a3b8',
    muted:     '#475569',
} as const;

export const status = {
    success: {
        DEFAULT: '#10b981',
        light:   '#d1fae5',
        dark:    '#065f46',
        ring:    'rgba(16,185,129,0.25)',
        chart:   '#10b981',
        chartFill: 'rgba(16,185,129,0.12)',
    },
    warning: {
        DEFAULT: '#f59e0b',
        light:   '#fef3c7',
        dark:    '#78350f',
        ring:    'rgba(245,158,11,0.25)',
        chart:   '#f59e0b',
        chartFill: 'rgba(245,158,11,0.12)',
    },
    danger: {
        DEFAULT: '#ef4444',
        light:   '#fee2e2',
        dark:    '#7f1d1d',
        ring:    'rgba(239,68,68,0.25)',
        chart:   '#ef4444',
        chartFill: 'rgba(239,68,68,0.10)',
    },
    info: {
        DEFAULT: '#3b82f6',
        light:   '#dbeafe',
        dark:    '#1e3a8a',
        ring:    'rgba(59,130,246,0.25)',
        chart:   '#3b82f6',
        chartFill: 'rgba(59,130,246,0.12)',
    },
} as const;

// ── Chart-specific palette (Chart.js / SVG) ───────────────────────────────────

export const chart = {
    grid:   'rgba(148,163,184,0.10)',
    axis:   text.secondary,
    axisTitle: text.muted,
    division: status.info.chart,
    divisionFill: status.info.chartFill,
    company: text.muted,
    companyDash: [5, 5] as [number, number],
    danger:  status.danger.chart,
    warning: status.warning.chart,
    success: status.success.chart,
} as const;

// ── Typography ────────────────────────────────────────────────────────────────

export const font = {
    family: '"Montserrat", sans-serif',
    size: {
        xs:  '0.6875rem',   // 11px
        sm:  '0.8125rem',   // 13px
        base: '0.875rem',   // 14px
        md:  '1rem',        // 16px
        lg:  '1.125rem',    // 18px
        xl:  '1.25rem',     // 20px
        '2xl': '1.5rem',    // 24px
        '3xl': '1.875rem',  // 30px
    },
} as const;

// ── Spacing ───────────────────────────────────────────────────────────────────

export const spacing = {
    xs:   '0.25rem',   // 4px
    sm:   '0.5rem',    // 8px
    md:   '1rem',      // 16px
    lg:   '1.5rem',    // 24px
    xl:   '2rem',      // 32px
    '2xl': '3rem',     // 48px
} as const;

// ── Radius ────────────────────────────────────────────────────────────────────

export const radius = {
    sm:   '0.375rem',  // 6px   — inputs, small elements
    md:   '0.5rem',    // 8px   — buttons
    lg:   '0.625rem',  // 10px  — cards (rounded-card)
    xl:   '0.75rem',   // 12px  — panels
    full: '9999px',    // pills, badges
} as const;

// ── Shadows (elevation system) ────────────────────────────────────────────────

export const shadow = {
    1: '0 1px 3px rgba(0,0,0,0.30), 0 1px 2px rgba(0,0,0,0.20)',
    2: '0 4px 8px rgba(0,0,0,0.35), 0 2px 4px rgba(0,0,0,0.25)',
    3: '0 10px 20px rgba(0,0,0,0.40), 0 4px 8px rgba(0,0,0,0.30)',
    4: '0 20px 40px rgba(0,0,0,0.50), 0 8px 16px rgba(0,0,0,0.35)',
    glow: {
        focus:   '0 0 0 3px rgba(59,130,246,0.35)',
        success: '0 0 0 3px rgba(16,185,129,0.30)',
        warning: '0 0 0 3px rgba(245,158,11,0.30)',
        danger:  '0 0 0 3px rgba(239,68,68,0.30)',
    },
} as const;

// ── Transitions ───────────────────────────────────────────────────────────────

export const transition = {
    fast:   '100ms cubic-bezier(0.4,0,0.2,1)',
    base:   '150ms cubic-bezier(0.4,0,0.2,1)',
    slow:   '200ms cubic-bezier(0.4,0,0.2,1)',
} as const;

// ── Aggregated export ─────────────────────────────────────────────────────────

export const tokens = {
    surface,
    border,
    text,
    status,
    chart,
    font,
    spacing,
    radius,
    shadow,
    transition,
} as const;

// ── Variant helpers ───────────────────────────────────────────────────────────
// Maps domain-specific variant names to design system variants

export type SemanticVariant = 'default' | 'success' | 'warning' | 'danger' | 'info';
export type KpiVariant      = 'good' | 'warn' | 'bad' | undefined;

export function kpiVariantToSemantic(v: KpiVariant): SemanticVariant {
    if (v === 'good') return 'success';
    if (v === 'warn') return 'warning';
    if (v === 'bad')  return 'danger';
    return 'default';
}

export type CategoryVariant = 'good' | 'warn' | 'bad';

export function categoryVariantToSemantic(v: CategoryVariant): SemanticVariant {
    if (v === 'good') return 'success';
    if (v === 'warn') return 'warning';
    return 'danger';
}

// ── Chart.js default options factory ─────────────────────────────────────────

export function makeChartDefaults() {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { display: false },
            tooltip: { mode: 'index' as const, intersect: false },
        },
        scales: {
            x: {
                grid: { color: chart.grid },
                ticks: { color: chart.axis, font: { size: 11, family: font.family } },
            },
            y: {
                grid: { color: chart.grid },
                ticks: { color: chart.axis, font: { size: 11, family: font.family } },
            },
        },
    };
}

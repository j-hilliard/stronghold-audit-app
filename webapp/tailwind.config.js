/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ['./src/**/*.{html,js,ts,vue}'],
    theme: {
        extend: {
            colors: {
                // Surface hierarchy — dark theme
                surface: {
                    1: '#0f172a',  // navy-900 — main background
                    2: '#1e293b',  // slate-800 — cards / panels
                    3: '#273549',  // slightly lifted — dialogs, elevated panels
                },
                // Semantic borders
                border: {
                    subtle: 'rgba(148,163,184,0.12)',
                    DEFAULT: 'rgba(148,163,184,0.20)',
                    strong: 'rgba(148,163,184,0.30)',
                },
                // Semantic text
                text: {
                    primary: '#f1f5f9',
                    secondary: '#94a3b8',
                    muted: '#475569',
                },
                // Semantic status
                success: { DEFAULT: '#10b981', light: '#d1fae5', dark: '#065f46', ring: 'rgba(16,185,129,0.25)' },
                warning: { DEFAULT: '#f59e0b', light: '#fef3c7', dark: '#78350f', ring: 'rgba(245,158,11,0.25)' },
                danger:  { DEFAULT: '#ef4444', light: '#fee2e2', dark: '#7f1d1d', ring: 'rgba(239,68,68,0.25)' },
                info:    { DEFAULT: '#3b82f6', light: '#dbeafe', dark: '#1e3a8a', ring: 'rgba(59,130,246,0.25)' },
            },
            boxShadow: {
                // Elevation system
                'elevation-1': '0 1px 3px rgba(0,0,0,0.30), 0 1px 2px rgba(0,0,0,0.20)',
                'elevation-2': '0 4px 8px rgba(0,0,0,0.35), 0 2px 4px rgba(0,0,0,0.25)',
                'elevation-3': '0 10px 20px rgba(0,0,0,0.40), 0 4px 8px rgba(0,0,0,0.30)',
                'elevation-4': '0 20px 40px rgba(0,0,0,0.50), 0 8px 16px rgba(0,0,0,0.35)',
                // Focus / glow rings
                'glow-focus':   '0 0 0 3px rgba(59,130,246,0.35)',
                'glow-success': '0 0 0 3px rgba(16,185,129,0.30)',
                'glow-warning': '0 0 0 3px rgba(245,158,11,0.30)',
                'glow-danger':  '0 0 0 3px rgba(239,68,68,0.30)',
                // Card hover
                'card-hover':   '0 0 0 1px rgba(99,179,237,0.35), 0 8px 24px rgba(0,0,0,0.50)',
                'card-hover-danger': '0 0 0 1px rgba(239,68,68,0.40), 0 8px 24px rgba(0,0,0,0.50)',
                'card-hover-warn':   '0 0 0 1px rgba(245,158,11,0.40), 0 8px 24px rgba(0,0,0,0.50)',
            },
            transitionDuration: {
                DEFAULT: '150ms',
                fast: '100ms',
                base: '150ms',
                slow: '200ms',
            },
            transitionTimingFunction: {
                smooth: 'cubic-bezier(0.4,0,0.2,1)',
            },
            borderRadius: {
                card: '0.625rem',
            },
        },
    },
    plugins: [],
};

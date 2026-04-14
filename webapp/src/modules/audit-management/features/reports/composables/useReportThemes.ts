import type { BlockStyle } from '../types/report-block';

export interface ReportTheme {
    id: string;
    name: string;
    /** Hex color used for the swatch circle in the toolbar */
    swatch: string;
    coverPrimaryColor: string;
    blockStyle: Partial<BlockStyle>;
}

export const REPORT_THEMES: ReportTheme[] = [
    {
        id: 'dark-pro',
        name: 'Dark Pro',
        swatch: '#1e40af',
        coverPrimaryColor: '#1e40af',
        blockStyle: { backgroundColor: '', textColor: '' },
    },
    {
        id: 'light-classic',
        name: 'Light Classic',
        swatch: '#1e3a5f',
        coverPrimaryColor: '#1e3a5f',
        blockStyle: { backgroundColor: '#ffffff', textColor: '#1e293b' },
    },
    {
        id: 'corp-blue',
        name: 'Corporate Blue',
        swatch: '#0369a1',
        coverPrimaryColor: '#0369a1',
        blockStyle: { backgroundColor: '#f0f9ff', textColor: '#0c4a6e', accentColor: '#0369a1' },
    },
    {
        id: 'slate',
        name: 'Slate',
        swatch: '#475569',
        coverPrimaryColor: '#334155',
        blockStyle: { backgroundColor: '#1e293b', textColor: '#f1f5f9' },
    },
    {
        id: 'forest',
        name: 'Forest',
        swatch: '#15803d',
        coverPrimaryColor: '#14532d',
        blockStyle: { backgroundColor: '#f0fdf4', textColor: '#14532d', accentColor: '#16a34a' },
    },
    {
        id: 'stronghold-crimson',
        name: 'Stronghold Crimson',
        swatch: '#7f1d1d',
        coverPrimaryColor: '#1c1c2e',
        blockStyle: { backgroundColor: '#1c1c2e', textColor: '#f1f5f9', accentColor: '#dc2626' },
    },
];

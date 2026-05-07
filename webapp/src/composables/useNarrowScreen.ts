import { ref } from 'vue';

const NARROW_BREAKPOINT = 768;
const DEV_VP_KEY = 'stronghold-dev-viewport';

const PRESET_WIDTHS: Record<string, number | null> = {
    desktop: null,
    tablet: 768,
    phone: 390,
};

// ── Module-level shared state ──────────────────────────────────────────────────
// Single source of truth for narrow mode. All consumers share these refs.
// Listeners are attached once here — NOT per component instance — so there is
// no risk of multiple handlers fighting each other during dev-viewport switches.

const previewPreset      = ref<string>('desktop');
const previewFrameWidth  = ref<number | null>(null);
const isNarrow           = ref<boolean>(false);

function computeNarrow(): boolean {
    if (previewPreset.value === 'tablet' || previewPreset.value === 'phone') return true;
    return window.innerWidth <= NARROW_BREAKPOINT;
}

function onResize(): void {
    // Only real viewport drives narrow when NOT in a dev-preview narrow preset.
    // Dev-preview narrow state is owned by the preset, not the browser width.
    if (previewPreset.value !== 'tablet' && previewPreset.value !== 'phone') {
        isNarrow.value = window.innerWidth <= NARROW_BREAKPOINT;
    }
}

function onDevViewportChange(e: Event): void {
    const preset = (e as CustomEvent<{ preset: string }>).detail?.preset ?? 'desktop';
    previewPreset.value     = preset;
    previewFrameWidth.value = PRESET_WIDTHS[preset] ?? null;
    isNarrow.value          = computeNarrow();
}

// ── Init once at module load ───────────────────────────────────────────────────
if (typeof window !== 'undefined') {
    if (import.meta.env.DEV) {
        const saved = localStorage.getItem(DEV_VP_KEY) ?? 'desktop';
        previewPreset.value     = saved;
        previewFrameWidth.value = PRESET_WIDTHS[saved] ?? null;
    }
    isNarrow.value = computeNarrow();
    window.addEventListener('resize', onResize);
    window.addEventListener('dev-viewport-change', onDevViewportChange);
}

export function useNarrowScreen() {
    return { isNarrow, previewPreset, previewFrameWidth };
}

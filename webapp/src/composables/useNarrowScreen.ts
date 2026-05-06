/**
 * useNarrowScreen
 *
 * Returns a reactive boolean that is true when the effective visible width is
 * narrow enough to warrant card-style layouts instead of data tables.
 *
 * Works in two contexts:
 *  1. Real mobile device: window.innerWidth <= 768
 *  2. Dev viewport switcher: listens for the 'dev-viewport-change' custom event
 *     that AppLayout/DevViewportSwitcher emits when phone/tablet is selected
 *
 * Usage:
 *   const { isNarrow } = useNarrowScreen()
 */
import { ref, onMounted, onBeforeUnmount } from 'vue';

const NARROW_BREAKPOINT = 768;

export function useNarrowScreen() {
    const isNarrow = ref(window.innerWidth <= NARROW_BREAKPOINT);

    function update() {
        isNarrow.value = window.innerWidth <= NARROW_BREAKPOINT;
    }

    function onDevViewportChange(e: Event) {
        const preset = (e as CustomEvent<{ preset: string }>).detail?.preset;
        // 'phone' and 'tablet' presets are narrow; 'desktop' is not
        if (preset === 'phone' || preset === 'tablet') {
            isNarrow.value = true;
        } else if (preset === 'desktop') {
            // Fall back to actual window width on desktop preset
            isNarrow.value = window.innerWidth <= NARROW_BREAKPOINT;
        }
    }

    onMounted(() => {
        update();
        window.addEventListener('resize', update);
        window.addEventListener('dev-viewport-change', onDevViewportChange);
    });

    onBeforeUnmount(() => {
        window.removeEventListener('resize', update);
        window.removeEventListener('dev-viewport-change', onDevViewportChange);
    });

    return { isNarrow };
}

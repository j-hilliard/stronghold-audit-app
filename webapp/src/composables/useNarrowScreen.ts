import { ref, onMounted, onBeforeUnmount } from 'vue';

const NARROW_BREAKPOINT = 768;
const DEV_VP_KEY = 'stronghold-dev-viewport';

// Module-level shared ref — survives navigation because the module is evaluated once.
// All component instances that call useNarrowScreen() share this single reactive ref.
// This means switching to phone mode stays active when the user navigates to a new page.
const isNarrow = ref(
    typeof window !== 'undefined' ? window.innerWidth <= NARROW_BREAKPOINT : false,
);

export function useNarrowScreen() {
    function update() {
        isNarrow.value = window.innerWidth <= NARROW_BREAKPOINT;
    }

    function onDevViewportChange(e: Event) {
        const preset = (e as CustomEvent<{ preset: string }>).detail?.preset;
        if (preset === 'phone' || preset === 'tablet') {
            isNarrow.value = true;
        } else if (preset === 'desktop') {
            isNarrow.value = window.innerWidth <= NARROW_BREAKPOINT;
        }
    }

    onMounted(() => {
        // On every mount (including after navigation) sync with the dev viewport
        // switcher's persisted preference so the correct state is applied immediately
        // without needing to re-receive the already-fired dev-viewport-change event.
        if (import.meta.env.DEV) {
            const saved = localStorage.getItem(DEV_VP_KEY);
            isNarrow.value = (saved && saved !== 'desktop')
                ? true
                : window.innerWidth <= NARROW_BREAKPOINT;
        } else {
            update();
        }
        window.addEventListener('resize', update);
        window.addEventListener('dev-viewport-change', onDevViewportChange);
    });

    onBeforeUnmount(() => {
        window.removeEventListener('resize', update);
        window.removeEventListener('dev-viewport-change', onDevViewportChange);
    });

    return { isNarrow };
}

import { ref, watch } from 'vue';

const isDark = ref(localStorage.getItem('theme') !== 'light');

function applyTheme(dark: boolean) {
    const html = document.documentElement;
    if (dark) {
        html.classList.remove('theme-light');
        html.classList.add('theme-dark');
    } else {
        html.classList.remove('theme-dark');
        html.classList.add('theme-light');
    }
    localStorage.setItem('theme', dark ? 'dark' : 'light');
}

// Apply on load immediately
applyTheme(isDark.value);

watch(isDark, applyTheme);

export function useTheme() {
    function toggle() {
        isDark.value = !isDark.value;
    }
    return { isDark, toggle };
}

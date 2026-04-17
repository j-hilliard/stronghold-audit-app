<template>
    <div
        class="relative overflow-hidden flex flex-col print:rounded-none"
        :class="variant !== 'minimal' ? 'rounded-xl' : 'rounded-xl'"
        :style="[bgStyle, heightStyle]"
        @click="editing = true"
    >
        <!-- Overlay (skip for minimal — light bg) -->
        <div v-if="variant !== 'minimal'" class="absolute inset-0 pointer-events-none" :style="overlayStyle" />

        <!-- ── CLASSIC: bottom-left, rules ──────────────────────────────── -->
        <div v-if="variant === 'classic'" class="relative z-10 flex flex-col justify-end h-full p-8 print:p-10">
            <div v-if="content.subtitle !== undefined || editing"
                ref="subtitleEl" contenteditable="true" spellcheck="false"
                class="cover-text cover-subtitle outline-none cursor-text"
                data-placeholder="Subtitle (optional)"
                @focus="editing = true" @blur="onBlur('subtitle', $event)"
                @keydown.enter.prevent="blurEl($refs.subtitleEl)" />
            <div v-if="showRules" class="cover-rule" />
            <h1 ref="nameEl" contenteditable="true" spellcheck="false"
                class="cover-name outline-none cursor-text" :style="nameStyle"
                @focus="editing = true" @blur="onBlur('divisionName', $event)"
                @keydown.enter.prevent="blurEl($refs.nameEl)" />
            <div v-if="showRules" class="cover-rule-row">
                <div class="cover-rule cover-rule--wide" />
                <span v-if="content.tagline !== undefined || editing"
                    ref="taglineEl" contenteditable="true" spellcheck="false"
                    class="cover-tagline outline-none cursor-text"
                    data-placeholder="Tagline"
                    @focus="editing = true" @blur="onBlur('tagline', $event)"
                    @keydown.enter.prevent="blurEl($refs.taglineEl)" />
            </div>
            <div class="text-white/70 text-sm mt-3 select-none">{{ content.period }}</div>
            <div class="text-white/40 text-xs mt-1 select-none">Prepared by {{ content.preparedBy }}</div>
        </div>

        <!-- ── CENTERED: everything centered ───────────────────────────── -->
        <div v-else-if="variant === 'centered'" class="relative z-10 flex flex-col items-center justify-center h-full p-8 text-center">
            <div v-if="content.subtitle !== undefined || editing"
                ref="subtitleEl" contenteditable="true" spellcheck="false"
                class="cover-text cover-subtitle outline-none cursor-text mb-2"
                data-placeholder="Subtitle (optional)"
                @focus="editing = true" @blur="onBlur('subtitle', $event)"
                @keydown.enter.prevent="blurEl($refs.subtitleEl)" />
            <h1 ref="nameEl" contenteditable="true" spellcheck="false"
                class="cover-name outline-none cursor-text" :style="nameStyle"
                @focus="editing = true" @blur="onBlur('divisionName', $event)"
                @keydown.enter.prevent="blurEl($refs.nameEl)" />
            <div class="w-24 h-0.5 my-3 mx-auto" :style="{ background: accentColor }" />
            <span v-if="content.tagline !== undefined || editing"
                ref="taglineEl" contenteditable="true" spellcheck="false"
                class="cover-tagline outline-none cursor-text block"
                data-placeholder="Tagline"
                @focus="editing = true" @blur="onBlur('tagline', $event)"
                @keydown.enter.prevent="blurEl($refs.taglineEl)" />
            <div class="text-white/60 text-sm mt-4 select-none">{{ content.period }}</div>
            <div class="text-white/35 text-xs mt-1 select-none">Prepared by {{ content.preparedBy }}</div>
        </div>

        <!-- ── TOP-LEFT: text anchored at top ───────────────────────────── -->
        <div v-else-if="variant === 'top-left'" class="relative z-10 flex flex-col justify-start h-full p-8 print:p-10">
            <div v-if="content.subtitle !== undefined || editing"
                ref="subtitleEl" contenteditable="true" spellcheck="false"
                class="cover-text cover-subtitle outline-none cursor-text"
                data-placeholder="Subtitle (optional)"
                @focus="editing = true" @blur="onBlur('subtitle', $event)"
                @keydown.enter.prevent="blurEl($refs.subtitleEl)" />
            <h1 ref="nameEl" contenteditable="true" spellcheck="false"
                class="cover-name outline-none cursor-text mt-1" :style="nameStyle"
                @focus="editing = true" @blur="onBlur('divisionName', $event)"
                @keydown.enter.prevent="blurEl($refs.nameEl)" />
            <div class="cover-rule mt-3" />
            <span v-if="content.tagline !== undefined || editing"
                ref="taglineEl" contenteditable="true" spellcheck="false"
                class="cover-tagline outline-none cursor-text mt-2"
                data-placeholder="Tagline (e.g. A Year in Review)"
                @focus="editing = true" @blur="onBlur('tagline', $event)"
                @keydown.enter.prevent="blurEl($refs.taglineEl)" />
            <div class="text-white/60 text-sm mt-4 select-none">{{ content.period }}</div>
            <div class="text-white/35 text-xs mt-1 select-none">Prepared by {{ content.preparedBy }}</div>
        </div>

        <!-- ── STRIPE-SIDE: left accent bar, bottom-left text ───────────── -->
        <div v-else-if="variant === 'stripe-side'" class="relative z-10 flex h-full">
            <!-- Left accent stripe -->
            <div class="shrink-0 w-5 h-full rounded-l-xl" :style="{ background: accentColor }" />
            <!-- Content: bottom-left -->
            <div class="flex flex-col justify-end flex-1 p-8 print:p-10">
                <div v-if="content.subtitle !== undefined || editing"
                    ref="subtitleEl" contenteditable="true" spellcheck="false"
                    class="cover-text cover-subtitle outline-none cursor-text"
                    data-placeholder="Subtitle (optional)"
                    @focus="editing = true" @blur="onBlur('subtitle', $event)"
                    @keydown.enter.prevent="blurEl($refs.subtitleEl)" />
                <h1 ref="nameEl" contenteditable="true" spellcheck="false"
                    class="cover-name outline-none cursor-text" :style="nameStyle"
                    @focus="editing = true" @blur="onBlur('divisionName', $event)"
                    @keydown.enter.prevent="blurEl($refs.nameEl)" />
                <div class="cover-rule mt-3" />
                <span v-if="content.tagline !== undefined || editing"
                    ref="taglineEl" contenteditable="true" spellcheck="false"
                    class="cover-tagline outline-none cursor-text mt-2"
                    data-placeholder="Tagline"
                    @focus="editing = true" @blur="onBlur('tagline', $event)"
                    @keydown.enter.prevent="blurEl($refs.taglineEl)" />
                <div class="text-white/60 text-sm mt-3 select-none">{{ content.period }}</div>
                <div class="text-white/35 text-xs mt-1 select-none">Prepared by {{ content.preparedBy }}</div>
            </div>
        </div>

        <!-- ── MINIMAL: light background, dark text ──────────────────────── -->
        <div v-else-if="variant === 'minimal'" class="relative z-10 flex flex-col justify-center h-full px-10 py-8">
            <div v-if="content.subtitle !== undefined || editing"
                ref="subtitleEl" contenteditable="true" spellcheck="false"
                class="outline-none cursor-text text-xs font-semibold tracking-widest uppercase mb-2"
                :style="{ color: accentColor }"
                data-placeholder="Subtitle (optional)"
                @focus="editing = true" @blur="onBlur('subtitle', $event)"
                @keydown.enter.prevent="blurEl($refs.subtitleEl)" />
            <h1 ref="nameEl" contenteditable="true" spellcheck="false"
                class="outline-none cursor-text font-black leading-tight" :style="minimalNameStyle"
                @focus="editing = true" @blur="onBlur('divisionName', $event)"
                @keydown.enter.prevent="blurEl($refs.nameEl)" />
            <div class="h-1 mt-3 w-16 rounded" :style="{ background: accentColor }" />
            <span v-if="content.tagline !== undefined || editing"
                ref="taglineEl" contenteditable="true" spellcheck="false"
                class="outline-none cursor-text italic text-slate-500 mt-3 text-sm"
                data-placeholder="Tagline (e.g. A Year in Review)"
                @focus="editing = true" @blur="onBlur('tagline', $event)"
                @keydown.enter.prevent="blurEl($refs.taglineEl)" />
            <div class="text-slate-400 text-xs mt-4 select-none">{{ content.period }}</div>
            <div class="text-slate-300 text-xs mt-1 select-none">Prepared by {{ content.preparedBy }}</div>
        </div>

        <!-- ── BOLD-FULL: oversized title, centered, no clutter ─────────── -->
        <div v-else-if="variant === 'bold-full'" class="relative z-10 flex flex-col items-center justify-center h-full p-8 text-center">
            <div v-if="content.subtitle !== undefined || editing"
                ref="subtitleEl" contenteditable="true" spellcheck="false"
                class="cover-text outline-none cursor-text text-xs font-bold tracking-[0.25em] uppercase mb-3"
                style="color: rgba(255,255,255,0.6)"
                data-placeholder="Subtitle (optional)"
                @focus="editing = true" @blur="onBlur('subtitle', $event)"
                @keydown.enter.prevent="blurEl($refs.subtitleEl)" />
            <h1 ref="nameEl" contenteditable="true" spellcheck="false"
                class="cover-name outline-none cursor-text" :style="boldNameStyle"
                @focus="editing = true" @blur="onBlur('divisionName', $event)"
                @keydown.enter.prevent="blurEl($refs.nameEl)" />
            <span v-if="content.tagline !== undefined || editing"
                ref="taglineEl" contenteditable="true" spellcheck="false"
                class="cover-tagline outline-none cursor-text block mt-4"
                data-placeholder="Tagline"
                @focus="editing = true" @blur="onBlur('tagline', $event)"
                @keydown.enter.prevent="blurEl($refs.taglineEl)" />
            <div class="text-white/40 text-xs mt-5 select-none tracking-wider uppercase">{{ content.period }}</div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch, nextTick } from 'vue';
import type { CoverContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: CoverContent;
    style: BlockStyle;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: CoverContent): void;
}>();

const editing = ref(false);
const nameEl     = ref<HTMLElement | null>(null);
const subtitleEl = ref<HTMLElement | null>(null);
const taglineEl  = ref<HTMLElement | null>(null);

const variant = computed(() => props.content.layoutVariant ?? 'classic');

// ── DOM sync ──────────────────────────────────────────────────────────────────

function blurEl(el: unknown) { (el as HTMLElement | null)?.blur(); }

function syncEl(el: HTMLElement | null, text: string | undefined) {
    if (!el || document.activeElement === el) return;
    const val = text ?? '';
    if (el.innerText !== val) el.innerText = val;
}

function syncAll() {
    syncEl(nameEl.value,     props.content.divisionName);
    syncEl(subtitleEl.value, props.content.subtitle);
    syncEl(taglineEl.value,  props.content.tagline);
}

onMounted(() => nextTick(syncAll));
watch(() => [props.content.divisionName, props.content.subtitle, props.content.tagline, props.content.layoutVariant],
    () => nextTick(syncAll));

function onBlur(field: 'divisionName' | 'subtitle' | 'tagline', e: FocusEvent) {
    editing.value = false;
    const text = (e.target as HTMLElement).innerText.trim();
    if (text !== props.content[field]) {
        emit('update:content', { ...props.content, [field]: text || undefined });
    }
}

// ── Style computeds ───────────────────────────────────────────────────────────

const HEIGHT_MAP: Record<string, string> = {
    xs: '120px', sm: '180px', md: '220px', lg: '320px', xl: '420px',
};

const NAME_SIZE_MAP: Record<string, string> = {
    sm: '1.2rem', md: '1.8rem', lg: '2.4rem', xl: '3rem', '2xl': '4rem',
};

const BOLD_SIZE_MAP: Record<string, string> = {
    sm: '2rem', md: '2.8rem', lg: '3.6rem', xl: '4.5rem', '2xl': '6rem',
};

const showRules = computed(() => props.content.showDecorativeRules !== false);
const accentColor = computed(() => props.content.nameAccentColor || props.style.accentColor || '#f59e0b');

const bgStyle = computed(() => {
    const image = props.content.backgroundImageUrl
        ? `url('${props.content.backgroundImageUrl}') center/cover no-repeat, `
        : '';
    const color = variant.value === 'minimal'
        ? (props.content.primaryColor || '#f1f5f9')
        : (props.content.primaryColor || '#1e3a5f');
    return `background: ${image}${color};`;
});

const heightStyle = computed(() => ({
    minHeight: HEIGHT_MAP[props.content.coverHeight ?? 'lg'] ?? '320px',
    height: '100%',
}));

const overlayStyle = computed(() => {
    const opacity = (props.content.overlayOpacity ?? 30) / 100;
    return { background: `rgba(0,0,0,${opacity})` };
});

const nameStyle = computed(() => ({
    color: accentColor.value,
    fontSize: NAME_SIZE_MAP[props.content.nameSize ?? 'xl'] ?? '3rem',
    textTransform: (props.content.nameTransform ?? 'uppercase') as string,
    fontWeight: '900',
    lineHeight: '1.1',
    letterSpacing: '-0.01em',
}));

const minimalNameStyle = computed(() => {
    const isLight = isLightColor(props.content.primaryColor ?? '#f1f5f9');
    return {
        color: isLight ? '#0f172a' : '#f8fafc',
        fontSize: NAME_SIZE_MAP[props.content.nameSize ?? 'xl'] ?? '3rem',
        textTransform: (props.content.nameTransform ?? 'uppercase') as string,
    };
});

const boldNameStyle = computed(() => ({
    color: accentColor.value,
    fontSize: BOLD_SIZE_MAP[props.content.nameSize ?? 'xl'] ?? '4.5rem',
    textTransform: (props.content.nameTransform ?? 'uppercase') as string,
    fontWeight: '900',
    lineHeight: '1',
    letterSpacing: '-0.03em',
}));

/** Simple luma check to determine if a hex color is light */
function isLightColor(hex: string): boolean {
    const h = hex.replace('#', '');
    if (h.length < 6) return true;
    const r = parseInt(h.slice(0, 2), 16);
    const g = parseInt(h.slice(2, 4), 16);
    const b = parseInt(h.slice(4, 6), 16);
    return (r * 299 + g * 587 + b * 114) / 1000 > 160;
}
</script>

<style scoped>
.cover-text, .cover-subtitle {
    font-size: 0.85rem;
    font-weight: 500;
    color: rgba(255, 255, 255, 0.75);
    letter-spacing: 0.04em;
    margin-bottom: 0.35rem;
    min-height: 1.2em;
}
.cover-subtitle:empty::before {
    content: attr(data-placeholder);
    color: rgba(255,255,255,0.25);
    font-style: italic;
}

.cover-name {
    font-weight: 900;
    line-height: 1.1;
    letter-spacing: -0.01em;
    margin: 0;
    min-height: 1em;
}

.cover-rule {
    height: 2px;
    background: rgba(255, 255, 255, 0.45);
    margin: 0.5rem 0;
    width: 60%;
}

.cover-rule-row {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.cover-rule--wide {
    flex: 1;
    margin: 0;
    width: auto;
}

.cover-tagline {
    font-size: 1rem;
    font-weight: 400;
    font-style: italic;
    color: rgba(255, 255, 255, 0.85);
    white-space: nowrap;
    min-width: 60px;
    min-height: 1.2em;
}
.cover-tagline:empty::before {
    content: attr(data-placeholder);
    color: rgba(255,255,255,0.25);
}
</style>

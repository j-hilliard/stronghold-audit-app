<template>
    <div
        class="relative rounded-xl overflow-hidden flex flex-col justify-end print:rounded-none"
        :style="[bgStyle, heightStyle]"
    >
        <!-- Configurable dark overlay -->
        <div class="absolute inset-0" :style="overlayStyle" />

        <div class="relative z-10 p-8 print:p-10">
            <!-- Optional subtitle above name -->
            <div
                v-if="content.subtitle !== undefined || editing"
                ref="subtitleEl"
                contenteditable="true"
                spellcheck="false"
                class="cover-subtitle outline-none cursor-text"
                data-placeholder="Subtitle (optional)"
                @focus="editing = true"
                @blur="onBlur('subtitle', $event)"
                @keydown.enter.prevent="($refs.subtitleEl as HTMLElement)?.blur()"
            />

            <!-- Rule above name -->
            <div v-if="showRules" class="cover-rule" />

            <!-- Main division name — click to override -->
            <h1
                ref="nameEl"
                contenteditable="true"
                spellcheck="false"
                class="cover-name outline-none cursor-text"
                :style="nameStyle"
                @focus="editing = true"
                @blur="onBlur('divisionName', $event)"
                @keydown.enter.prevent="($refs.nameEl as HTMLElement)?.blur()"
            />

            <!-- Rule below name + optional tagline -->
            <div v-if="showRules" class="cover-rule-row">
                <div class="cover-rule cover-rule--wide" />
                <span
                    v-if="content.tagline !== undefined || editing"
                    ref="taglineEl"
                    contenteditable="true"
                    spellcheck="false"
                    class="cover-tagline outline-none cursor-text"
                    data-placeholder="Tagline"
                    @focus="editing = true"
                    @blur="onBlur('tagline', $event)"
                    @keydown.enter.prevent="($refs.taglineEl as HTMLElement)?.blur()"
                />
            </div>

            <!-- Period / prepared-by -->
            <div class="text-white/70 text-sm mt-3">{{ content.period }}</div>
            <div class="text-white/40 text-xs mt-1">Prepared by {{ content.preparedBy }}</div>
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

// ── Sync DOM text from props without disturbing cursor ────────────────────────
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
watch(() => [props.content.divisionName, props.content.subtitle, props.content.tagline], () => nextTick(syncAll));

function onBlur(field: 'divisionName' | 'subtitle' | 'tagline', e: FocusEvent) {
    editing.value = false;
    const text = (e.target as HTMLElement).innerText.trim();
    const current = props.content[field];
    if (text !== current) {
        emit('update:content', { ...props.content, [field]: text || undefined });
    }
}

// ── Styles ────────────────────────────────────────────────────────────────────
const HEIGHT_MAP: Record<string, string> = {
    xs: '120px', sm: '180px', md: '220px', lg: '320px', xl: '420px',
};

const NAME_SIZE_MAP: Record<string, string> = {
    sm: '1.2rem', md: '1.8rem', lg: '2.4rem', xl: '3rem', '2xl': '4rem',
};

const showRules = computed(() => props.content.showDecorativeRules !== false);

const bgStyle = computed(() => {
    const image = props.content.backgroundImageUrl
        ? `url('${props.content.backgroundImageUrl}') center/cover no-repeat, `
        : '';
    return `background: ${image}${props.content.primaryColor};`;
});

const heightStyle = computed(() => ({
    minHeight: HEIGHT_MAP[props.content.coverHeight ?? 'md'] ?? '220px',
    height: '100%',   // fill the block-wrapper's allocated height when resized
}));

const overlayStyle = computed(() => {
    const opacity = (props.content.overlayOpacity ?? 40) / 100;
    return { background: `rgba(0,0,0,${opacity})` };
});

const nameStyle = computed(() => {
    const color = props.content.nameAccentColor || props.style.accentColor || '#f59e0b';
    const size  = NAME_SIZE_MAP[props.content.nameSize ?? 'xl'] ?? '3rem';
    const transform = props.content.nameTransform ?? 'uppercase';
    return { color, fontSize: size, textTransform: transform };
});
</script>

<style scoped>
.cover-subtitle {
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
    background: rgba(255, 255, 255, 0.55);
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

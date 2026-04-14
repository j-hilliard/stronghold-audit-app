<template>
    <!-- Banner mode — when a background image URL is set -->
    <div
        v-if="content.backgroundImageUrl"
        class="relative overflow-hidden rounded-lg"
        :style="`background: url('${content.backgroundImageUrl}') center/cover no-repeat; min-height: 80px;`"
    >
        <div
            class="absolute inset-0"
            :style="`background: rgba(0,0,0,${(content.overlayOpacity ?? 50) / 100})`"
        />
        <div class="relative z-10 p-6">
            <component
                :is="`h${content.level}`"
                ref="bannerEl"
                contenteditable="true"
                spellcheck="false"
                class="leading-tight text-white outline-none cursor-text"
                :class="!content.fontSize ? headingSizeClass : ''"
                :style="headingOverrideStyle"
                @blur="onBlur"
                @keydown.enter.prevent="(bannerEl as HTMLElement)?.blur()"
            />
        </div>
    </div>

    <!-- Plain heading -->
    <div v-else :style="containerStyle">
        <component
            :is="`h${content.level}`"
            ref="plainEl"
            contenteditable="true"
            spellcheck="false"
            class="leading-tight outline-none cursor-text"
            :class="!content.fontSize ? plainHeadingClass : ''"
            :style="headingOverrideStyle"
            @blur="onBlur"
            @keydown.enter.prevent="(plainEl as HTMLElement)?.blur()"
        />
    </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch, nextTick } from 'vue';
import type { HeadingContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: HeadingContent;
    style: BlockStyle;
    isEdited: boolean;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: HeadingContent): void;
    (e: 'update:isEdited', value: boolean): void;
}>();

const bannerEl = ref<HTMLElement | null>(null);
const plainEl  = ref<HTMLElement | null>(null);

/** The active contenteditable element, whichever is rendered. */
const activeEl = computed(() => props.content.backgroundImageUrl ? bannerEl.value : plainEl.value);

/** Sync DOM text from props without disturbing cursor (only when not focused). */
function syncText() {
    const el = activeEl.value;
    if (!el || document.activeElement === el) return;
    if (el.innerText !== props.content.text) el.innerText = props.content.text;
}

onMounted(() => {
    nextTick(syncText);
});

watch(() => props.content.text, () => nextTick(syncText));
watch(() => props.content.backgroundImageUrl, () => nextTick(syncText));

function onBlur(e: FocusEvent) {
    const text = (e.target as HTMLElement).innerText.trim();
    if (text !== props.content.text) {
        emit('update:content', { ...props.content, text });
        if (!props.isEdited) emit('update:isEdited', true);
    }
}

const headingSizeClass = computed(() => ({
    'text-2xl': props.content.level === 1,
    'text-xl':  props.content.level === 2,
    'text-lg':  props.content.level === 3,
}));

const plainHeadingClass = computed(() => {
    if (props.style.textColor) return headingSizeClass.value;
    return {
        'text-2xl text-gray-900': props.content.level === 1,
        'text-xl  text-gray-800': props.content.level === 2,
        'text-lg  text-gray-700': props.content.level === 3,
    };
});

const containerStyle = computed(() => {
    const parts: string[] = [];
    if (props.style.textColor)       parts.push(`color: ${props.style.textColor}`);
    if (props.style.backgroundColor) parts.push(`background: ${props.style.backgroundColor}`);
    return parts.join('; ');
});

const headingOverrideStyle = computed(() => {
    const style: Record<string, string> = {};
    if (props.content.fontSize) style.fontSize = `${props.content.fontSize}px`;
    if (props.content.fontWeight) style.fontWeight = String(props.content.fontWeight);
    return style;
});
</script>

<template>
    <div class="rounded-lg border-l-4 p-4" :class="containerClass">
        <div class="flex items-start gap-2">
            <i :class="iconClass" class="mt-0.5 shrink-0" />
            <div class="flex-1 space-y-1">
                <div
                    ref="titleEl"
                    contenteditable="true"
                    spellcheck="false"
                    class="text-sm font-semibold outline-none cursor-text"
                    :class="titleClass"
                    @blur="onTitleBlur"
                    @keydown.enter.prevent="(titleEl as HTMLElement)?.blur()"
                />
                <RichTextEditor
                    :model-value="content.body"
                    placeholder="Add callout body text…"
                    :class="['text-sm', bodyClass]"
                    @update:model-value="(v) => emit('update:content', { ...props.content, body: v })"
                />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch, nextTick } from 'vue';
import type { CalloutContent, BlockStyle } from '../../types/report-block';
import RichTextEditor from '../RichTextEditor.vue';

const props = defineProps<{
    content: CalloutContent;
    style: BlockStyle;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: CalloutContent): void;
}>();

const titleEl = ref<HTMLElement | null>(null);

function syncTitle() {
    if (!titleEl.value || document.activeElement === titleEl.value) return;
    if (titleEl.value.innerText !== props.content.title) titleEl.value.innerText = props.content.title;
}
onMounted(() => nextTick(syncTitle));
watch(() => props.content.title, () => nextTick(syncTitle));

function onTitleBlur(e: FocusEvent) {
    const text = (e.target as HTMLElement).innerText.trim();
    if (text !== props.content.title) emit('update:content', { ...props.content, title: text });
}

const containerClass = computed(() => ({
    'bg-blue-900/20 border-blue-500': props.content.variant === 'info',
    'bg-amber-900/20 border-amber-500': props.content.variant === 'warning',
    'bg-emerald-900/20 border-emerald-500': props.content.variant === 'success',
    'bg-red-900/20 border-red-500': props.content.variant === 'danger',
}));

const iconClass = computed(() => ({
    'pi pi-info-circle text-blue-400': props.content.variant === 'info',
    'pi pi-exclamation-triangle text-amber-400': props.content.variant === 'warning',
    'pi pi-check-circle text-emerald-400': props.content.variant === 'success',
    'pi pi-times-circle text-red-400': props.content.variant === 'danger',
}));

const titleClass = computed(() => ({
    'text-blue-300': props.content.variant === 'info',
    'text-amber-300': props.content.variant === 'warning',
    'text-emerald-300': props.content.variant === 'success',
    'text-red-300': props.content.variant === 'danger',
}));

const bodyClass = computed(() => ({
    'text-blue-200/80': props.content.variant === 'info',
    'text-amber-200/80': props.content.variant === 'warning',
    'text-emerald-200/80': props.content.variant === 'success',
    'text-red-200/80': props.content.variant === 'danger',
}));
</script>

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
                class="font-semibold leading-tight text-white"
                :class="headingSizeClass"
            >{{ content.text }}</component>
        </div>
    </div>

    <!-- Plain heading — on white document page use dark text by default -->
    <div v-else :style="containerStyle">
        <component
            :is="`h${content.level}`"
            class="font-semibold leading-tight"
            :class="plainHeadingClass"
        >{{ content.text }}</component>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { HeadingContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: HeadingContent;
    style: BlockStyle;
}>();

const headingSizeClass = computed(() => ({
    'text-2xl': props.content.level === 1,
    'text-xl':  props.content.level === 2,
    'text-lg':  props.content.level === 3,
}));

// On the white document page, default to dark text unless overridden by style.textColor
const plainHeadingClass = computed(() => {
    if (props.style.textColor) return headingSizeClass.value; // color applied via containerStyle
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
</script>

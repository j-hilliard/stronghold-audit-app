<template>
    <!-- BlockStyle.backgroundColor is intentionally unused here.
         Visual identity comes from content.primaryColor and content.backgroundImageUrl. -->
    <div
        class="relative rounded-xl overflow-hidden flex flex-col justify-end print:rounded-none"
        :style="bgStyle"
        style="min-height: 220px;"
    >
        <!-- dark overlay for readability over photo backgrounds -->
        <div class="absolute inset-0 bg-black/40" />

        <div class="relative z-10 p-8 print:p-10">
            <!-- Optional subtitle line above the name -->
            <div v-if="content.subtitle" class="cover-subtitle">
                {{ content.subtitle }}
            </div>

            <!-- Decorative rule above name -->
            <div v-if="showRules" class="cover-rule" />

            <!-- Main division name in accent color -->
            <h1 class="cover-name" :style="nameStyle">{{ content.divisionName }}</h1>

            <!-- Decorative rule below name + optional tagline on the right -->
            <div v-if="showRules" class="cover-rule-row">
                <div class="cover-rule cover-rule--wide" />
                <span v-if="content.tagline" class="cover-tagline">{{ content.tagline }}</span>
            </div>
            <div v-else-if="content.tagline" class="cover-tagline-standalone">{{ content.tagline }}</div>

            <!-- Period / prepared-by -->
            <div class="text-white/70 text-sm mt-3">{{ content.period }}</div>
            <div class="text-white/40 text-xs mt-1">Prepared by {{ content.preparedBy }}</div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { CoverContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: CoverContent;
    style: BlockStyle;
}>();

const showRules = computed(() =>
    props.content.showDecorativeRules !== false
);

const bgStyle = computed(() => {
    const image = props.content.backgroundImageUrl
        ? `url('${props.content.backgroundImageUrl}') center/cover no-repeat, `
        : '';
    return `background: ${image}${props.content.primaryColor};`;
});

const nameStyle = computed(() => {
    const color = props.content.nameAccentColor || props.style.accentColor || '#f59e0b';
    return `color: ${color};`;
});
</script>

<style scoped>
.cover-subtitle {
    font-size: 0.85rem;
    font-weight: 500;
    color: rgba(255, 255, 255, 0.75);
    letter-spacing: 0.04em;
    margin-bottom: 0.35rem;
}

.cover-name {
    font-size: clamp(1.8rem, 5vw, 3rem);
    font-weight: 900;
    line-height: 1.1;
    letter-spacing: -0.01em;
    margin: 0;
    text-transform: uppercase;
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
}

.cover-tagline-standalone {
    font-size: 1rem;
    font-weight: 400;
    font-style: italic;
    color: rgba(255, 255, 255, 0.85);
    margin-top: 0.5rem;
}
</style>

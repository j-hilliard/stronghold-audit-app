<template>
    <div class="oval-wrapper">
        <div class="oval-callout" :style="ovalStyle">
            <p class="oval-title">{{ content.title }}</p>
            <p v-if="content.phonetic" class="oval-phonetic">{{ content.phonetic }}</p>
            <hr class="oval-rule" />
            <p class="oval-body">{{ content.body }}</p>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { OvalCalloutContent, BlockStyle } from '../../types/report-block';

const props = defineProps<{
    content: OvalCalloutContent;
    style: BlockStyle;
    isEdited: boolean;
}>();

const ovalStyle = computed(() => ({
    background: props.content.backgroundColor || '#1e3a5f',
    color: props.content.textColor || '#ffffff',
}));
</script>

<style scoped>
.oval-wrapper {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 0.75rem;
}

.oval-callout {
    /* Fixed dimensions so border-radius: 50% always produces an ellipse */
    width: 300px;
    height: 200px;
    border-radius: 50%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    padding: 1.5rem 2.5rem;
    /* Clip overflowing text rather than expanding the box */
    overflow: hidden;
}

.oval-title {
    font-size: 1rem;
    font-weight: 700;
    text-decoration: underline;
    margin: 0 0 0.15rem;
    letter-spacing: -0.01em;
    flex-shrink: 0;
}

.oval-phonetic {
    font-size: 0.7rem;
    font-style: italic;
    opacity: 0.85;
    margin: 0 0 0.35rem;
    flex-shrink: 0;
}

.oval-rule {
    width: 40%;
    border: none;
    border-top: 1px solid rgba(255, 255, 255, 0.35);
    margin: 0 auto 0.4rem;
    flex-shrink: 0;
}

.oval-body {
    font-size: 0.72rem;
    line-height: 1.4;
    margin: 0;
    opacity: 0.92;
    /* allow body text to shrink if space is tight */
    overflow: hidden;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 4;
}
</style>

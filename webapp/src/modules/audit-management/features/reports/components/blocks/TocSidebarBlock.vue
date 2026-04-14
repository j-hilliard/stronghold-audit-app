<template>
    <div class="toc-sidebar" :class="{ 'toc-dark': content.darkBackground }">
        <p class="toc-label">{{ content.title }}</p>
        <template v-for="(item, i) in content.items" :key="i">
            <div class="toc-rule" />
            <div class="toc-item">
                <p class="toc-heading">{{ item.heading }}</p>
                <p v-if="item.description" class="toc-description">{{ item.description }}</p>
            </div>
        </template>
        <div v-if="!content.items.length" class="toc-empty">
            Add items in the properties panel
        </div>
    </div>
</template>

<script setup lang="ts">
import type { TocSidebarContent, BlockStyle } from '../../types/report-block';

defineProps<{
    content: TocSidebarContent;
    style: BlockStyle;
    isEdited: boolean;
}>();
</script>

<style scoped>
.toc-sidebar {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    padding: 1.5rem 1rem;
    height: 100%;
    min-height: 200px;
    background: transparent;
}

.toc-dark {
    background: #374151; /* slate-700 */
    color: #f1f5f9;
}

.toc-label {
    font-size: 0.7rem;
    font-weight: 700;
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: inherit;
    opacity: 0.7;
    margin: 0 0 0.5rem 0;
    text-align: right;
}

.toc-rule {
    width: 2rem;
    height: 1px;
    background: currentColor;
    opacity: 0.3;
    margin: 0.6rem 0;
    align-self: flex-end;
}

.toc-item {
    text-align: right;
    margin-bottom: 0.25rem;
}

.toc-heading {
    font-size: 0.78rem;
    font-weight: 600;
    color: inherit;
    margin: 0;
    line-height: 1.3;
}

.toc-description {
    font-size: 0.7rem;
    color: inherit;
    opacity: 0.6;
    margin: 0.1rem 0 0 0;
    line-height: 1.3;
}

.toc-empty {
    font-size: 0.7rem;
    opacity: 0.4;
    font-style: italic;
    text-align: right;
    margin-top: 1rem;
}
</style>

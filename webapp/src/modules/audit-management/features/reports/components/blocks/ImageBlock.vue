<template>
    <div class="space-y-2">
        <!-- Placeholder when no URL is set -->
        <div
            v-if="!content.url"
            class="flex flex-col items-center justify-center border-2 border-dashed border-gray-300 rounded-lg py-12 text-gray-400 text-sm space-y-2"
        >
            <i class="pi pi-image text-3xl" />
            <span>Paste a URL or upload from PC in the panel on the right</span>
        </div>

        <!-- Actual image -->
        <div v-else :class="content.width === 'half' ? 'flex justify-center' : ''">
            <img
                :src="content.url"
                :alt="content.alt || ''"
                :class="content.width === 'half' ? 'w-1/2' : 'w-full'"
                class="rounded object-cover"
            />
        </div>

        <!-- Caption -->
        <p v-if="content.caption" class="text-sm text-gray-500 text-center italic">
            {{ content.caption }}
        </p>
    </div>
</template>

<script setup lang="ts">
import type { ImageContent, BlockStyle } from '../../types/report-block';

defineProps<{
    content: ImageContent;
    style: BlockStyle;
    isEdited: boolean;
}>();
</script>

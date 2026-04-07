<template>
    <div class="w-64 shrink-0 bg-slate-800 border-l border-slate-700 overflow-y-auto composer-right-rail">
        <div v-if="!block" class="p-4 text-slate-500 text-sm">
            Select a block to edit its properties.
        </div>

        <div v-else class="p-4 space-y-5">
            <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider">
                {{ blockLabel(block.type) }}
            </div>

            <!-- Cover-specific -->
            <template v-if="block.type === 'cover'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Primary Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.content.primaryColor"
                            @input="updateCoverColor('primaryColor', ($event.target as HTMLInputElement).value)"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <input type="text" :value="block.content.primaryColor"
                            @input="updateCoverColor('primaryColor', ($event.target as HTMLInputElement).value)"
                            class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 font-mono focus:outline-none focus:border-blue-500" />
                    </div>
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Background Image</label>
                    <input type="text" :value="block.content.backgroundImageUrl"
                        @input="updateContent({ ...block.content, backgroundImageUrl: ($event.target as HTMLInputElement).value })"
                        placeholder="https://… or upload below"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    <label class="flex items-center gap-2 cursor-pointer mt-1 px-2 py-1.5 bg-slate-700 hover:bg-slate-600 rounded text-xs text-slate-300 border border-slate-600 transition-colors">
                        <i class="pi pi-upload text-xs" /> Upload from PC
                        <input type="file" accept="image/*" class="hidden"
                            @change="onImageUpload($event, (url) => updateContent({ ...block.content, backgroundImageUrl: url }))" />
                    </label>
                    <p v-if="block.content.backgroundImageUrl" class="text-xs text-slate-500 truncate">{{ block.content.backgroundImageUrl.slice(0, 40) }}…</p>
                </div>
            </template>

            <!-- Heading-specific -->
            <template v-if="block.type === 'heading'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Text</label>
                    <input type="text" :value="block.content.text"
                        @input="updateContent({ ...block.content, text: ($event.target as HTMLInputElement).value }); setIsEdited(true)"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Level</label>
                    <select :value="block.content.level" @change="updateContent({ ...block.content, level: Number(($event.target as HTMLSelectElement).value) })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none">
                        <option value="1">H1 — Large</option>
                        <option value="2">H2 — Medium</option>
                        <option value="3">H3 — Small</option>
                    </select>
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Background Image</label>
                    <input type="text" :value="block.content.backgroundImageUrl || ''"
                        @input="updateContent({ ...block.content, backgroundImageUrl: ($event.target as HTMLInputElement).value || undefined })"
                        placeholder="https://… or upload below"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    <label class="flex items-center gap-2 cursor-pointer mt-1 px-2 py-1.5 bg-slate-700 hover:bg-slate-600 rounded text-xs text-slate-300 border border-slate-600 transition-colors">
                        <i class="pi pi-upload text-xs" /> Upload from PC
                        <input type="file" accept="image/*" class="hidden"
                            @change="onImageUpload($event, (url) => updateContent({ ...block.content, backgroundImageUrl: url }))" />
                    </label>
                </div>
                <div v-if="block.content.backgroundImageUrl" class="space-y-1">
                    <label class="block text-xs text-slate-400">Overlay Opacity (0–80)</label>
                    <input type="range" min="0" max="80"
                        :value="block.content.overlayOpacity ?? 50"
                        @input="updateContent({ ...block.content, overlayOpacity: Number(($event.target as HTMLInputElement).value) })"
                        class="w-full accent-blue-500" />
                    <span class="text-xs text-slate-500">{{ block.content.overlayOpacity ?? 50 }}%</span>
                </div>
            </template>

            <!-- Chart title + caption -->
            <template v-if="block.type === 'chart-bar' || block.type === 'chart-line'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Chart Title</label>
                    <input type="text" :value="block.content.title"
                        @input="updateContent({ ...block.content, title: ($event.target as HTMLInputElement).value })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>
                <div v-if="block.type === 'chart-line'" class="space-y-1">
                    <label class="block text-xs text-slate-400">Section</label>
                    <input type="text" :value="block.content.sectionName" readonly
                        class="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-xs text-slate-400 cursor-default" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Caption / Commentary</label>
                    <textarea :value="block.content.caption" rows="3"
                        @input="updateContent({ ...block.content, caption: ($event.target as HTMLTextAreaElement).value })"
                        placeholder="Add a note below this chart…"
                        data-testid="block-caption-input"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500 resize-none" />
                </div>
                <div v-if="block.type === 'chart-line'" class="space-y-1">
                    <label class="block text-xs text-slate-400">Examples / Findings</label>
                    <textarea :value="block.content.examples" rows="5"
                        @input="updateContent({ ...block.content, examples: ($event.target as HTMLTextAreaElement).value })"
                        placeholder="• Finding example 1&#10;• Finding example 2&#10;• Finding example 3"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500 resize-y" />
                    <p class="text-xs text-slate-500">Shown below the chart in print. Also editable inline.</p>
                </div>
            </template>

            <!-- Callout -->
            <template v-if="block.type === 'callout'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Title</label>
                    <input type="text" :value="block.content.title"
                        @input="updateContent({ ...block.content, title: ($event.target as HTMLInputElement).value })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Body</label>
                    <textarea :value="block.content.body" rows="3"
                        @input="updateContent({ ...block.content, body: ($event.target as HTMLTextAreaElement).value })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500 resize-none" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Variant</label>
                    <select :value="block.content.variant"
                        @change="updateContent({ ...block.content, variant: ($event.target as HTMLSelectElement).value })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none">
                        <option value="info">Info</option>
                        <option value="warning">Warning</option>
                        <option value="success">Success</option>
                        <option value="danger">Danger</option>
                    </select>
                </div>
            </template>

            <!-- KPI Grid card toggles -->
            <template v-if="block.type === 'kpi-grid'">
                <div class="space-y-2">
                    <div class="flex items-center justify-between">
                        <label class="text-xs text-slate-400">Show Company Comparison</label>
                        <button
                            @click="updateContent({ ...block.content, showComparison: !block.content.showComparison })"
                            class="relative inline-flex h-5 w-9 items-center rounded-full transition-colors focus:outline-none"
                            :class="block.content.showComparison ? 'bg-blue-600' : 'bg-slate-600'"
                        >
                            <span
                                class="inline-block h-3.5 w-3.5 transform rounded-full bg-white transition-transform"
                                :class="block.content.showComparison ? 'translate-x-4' : 'translate-x-1'"
                            />
                        </button>
                    </div>
                    <p class="text-xs text-slate-500">Division value vs company-wide on each card.</p>
                </div>

                <div class="space-y-1">
                    <div class="flex items-center justify-between">
                        <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Summary Cards</label>
                        <button @click="toggleAllCards('summary', block)" class="text-xs text-blue-400 hover:text-blue-300">Toggle all</button>
                    </div>
                    <div v-for="card in summaryCards(block.content.cards)" :key="card.label" class="flex items-center justify-between py-0.5">
                        <span class="text-xs text-slate-300">{{ card.label }}</span>
                        <button
                            @click="toggleCard(card.label, undefined, block)"
                            class="relative inline-flex h-4 w-7 items-center rounded-full transition-colors focus:outline-none"
                            :class="card.enabled ? 'bg-blue-600' : 'bg-slate-600'"
                        >
                            <span
                                class="inline-block h-2.5 w-2.5 transform rounded-full bg-white transition-transform"
                                :class="card.enabled ? 'translate-x-3.5' : 'translate-x-0.5'"
                            />
                        </button>
                    </div>
                </div>

                <div class="space-y-1">
                    <div class="flex items-center justify-between">
                        <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Section Cards</label>
                        <button @click="toggleAllCards('section', block)" class="text-xs text-blue-400 hover:text-blue-300">Toggle all</button>
                    </div>
                    <p class="text-xs text-slate-500 mb-1">Enabled sections get a trend chart on Generate.</p>
                    <div v-for="card in sectionCards(block.content.cards)" :key="card.sectionName" class="flex items-center justify-between py-0.5">
                        <div class="flex-1 min-w-0">
                            <span class="text-xs text-slate-300 truncate block">{{ card.label }}</span>
                            <span class="text-xs text-slate-500">{{ card.value }} NC/audit</span>
                        </div>
                        <button
                            @click="toggleCard(card.label, card.sectionName!, block)"
                            class="relative inline-flex h-4 w-7 shrink-0 items-center rounded-full transition-colors focus:outline-none ml-2"
                            :class="card.enabled ? 'bg-blue-600' : 'bg-slate-600'"
                        >
                            <span
                                class="inline-block h-2.5 w-2.5 transform rounded-full bg-white transition-transform"
                                :class="card.enabled ? 'translate-x-3.5' : 'translate-x-0.5'"
                            />
                        </button>
                    </div>
                    <div v-if="!sectionCards(block.content.cards).length" class="text-xs text-slate-500 italic">
                        Generate the report to load section cards.
                    </div>
                </div>
            </template>

            <!-- Image block -->
            <template v-if="block.type === 'image'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Image URL</label>
                    <input type="text" :value="block.content.url"
                        @input="updateContent({ ...block.content, url: ($event.target as HTMLInputElement).value })"
                        placeholder="https://… or upload below"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    <label class="flex items-center gap-2 cursor-pointer mt-1 px-2 py-1.5 bg-slate-700 hover:bg-slate-600 rounded text-xs text-slate-300 border border-slate-600 transition-colors">
                        <i class="pi pi-upload text-xs" /> Upload from PC
                        <input type="file" accept="image/*" class="hidden"
                            @change="onImageUpload($event, (url) => updateContent({ ...block.content, url }))" />
                    </label>
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Alt Text</label>
                    <input type="text" :value="block.content.alt"
                        @input="updateContent({ ...block.content, alt: ($event.target as HTMLInputElement).value })"
                        placeholder="Describe the image…"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Caption</label>
                    <textarea :value="block.content.caption" rows="2"
                        @input="updateContent({ ...block.content, caption: ($event.target as HTMLTextAreaElement).value })"
                        placeholder="Optional caption…"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500 resize-none" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Width</label>
                    <div class="flex gap-2">
                        <button
                            @click="updateContent({ ...block.content, width: 'full' })"
                            :class="['flex-1 px-2 py-1 rounded text-xs transition-colors', block.content.width === 'full' ? 'bg-blue-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600']"
                        >Full width</button>
                        <button
                            @click="updateContent({ ...block.content, width: 'half' })"
                            :class="['flex-1 px-2 py-1 rounded text-xs transition-colors', block.content.width === 'half' ? 'bg-blue-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600']"
                        >Half width</button>
                    </div>
                </div>
            </template>

            <!-- Narrative lock indicator -->
            <template v-if="block.type === 'narrative'">
                <div class="text-xs text-slate-400 space-y-1">
                    <div>Edit the narrative directly on the canvas. Click "Regenerate ✨" in the block to refresh.</div>
                    <button v-if="block.isEdited" @click="setIsEdited(false)"
                        class="text-xs text-amber-400 hover:text-amber-300">
                        Unlock for regeneration
                    </button>
                </div>
            </template>

            <!-- ── Shared style panel ────────────────────────────────────────── -->
            <div class="pt-3 border-t border-slate-700 space-y-3">
                <div class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Style</div>

                <div v-if="block.type !== 'cover'" class="space-y-1">
                    <label class="block text-xs text-slate-400">Background Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.style.backgroundColor || '#1e293b'"
                            @input="updateStyle({ ...block.style, backgroundColor: ($event.target as HTMLInputElement).value })"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <input type="text" :value="block.style.backgroundColor || ''"
                            @input="updateStyle({ ...block.style, backgroundColor: ($event.target as HTMLInputElement).value })"
                            placeholder="#1e293b"
                            maxlength="7"
                            data-testid="block-style-backgroundColor"
                            class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 font-mono focus:outline-none focus:border-blue-500" />
                        <button @click="updateStyle({ ...block.style, backgroundColor: '' })"
                            class="text-xs text-slate-500 hover:text-slate-300">Clear</button>
                    </div>
                </div>

                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Text Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.style.textColor || '#e2e8f0'"
                            @input="updateStyle({ ...block.style, textColor: ($event.target as HTMLInputElement).value })"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <button @click="updateStyle({ ...block.style, textColor: '' })"
                            class="text-xs text-slate-500 hover:text-slate-300">Clear</button>
                    </div>
                </div>

                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Accent Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.style.accentColor || '#f59e0b'"
                            @input="updateStyle({ ...block.style, accentColor: ($event.target as HTMLInputElement).value })"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <button @click="updateStyle({ ...block.style, accentColor: '' })"
                            class="text-xs text-slate-500 hover:text-slate-300">Clear</button>
                    </div>
                </div>

                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Padding</label>
                    <select :value="block.style.padding || 'md'"
                        @change="updateStyle({ ...block.style, padding: ($event.target as HTMLSelectElement).value as 'none' | 'sm' | 'md' | 'lg' })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none">
                        <option value="none">None</option>
                        <option value="sm">Small</option>
                        <option value="md">Medium</option>
                        <option value="lg">Large</option>
                    </select>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import type { ReportBlock, BlockStyle, BlockType, KpiCard, KpiGridBlock, ImageContent } from '../types/report-block';

/** Convert a local file to a base64 data-URL so it can be stored in the draft. */
function onImageUpload(event: Event, callback: (url: string) => void) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => { if (reader.result) callback(reader.result as string); };
    reader.readAsDataURL(file);
    // Reset the input so the same file can be re-selected
    (event.target as HTMLInputElement).value = '';
}

const props = defineProps<{
    block: ReportBlock | null;
}>();

const emit = defineEmits<{
    (e: 'update', block: ReportBlock): void;
}>();

function updateContent(content: unknown) {
    if (!props.block) return;
    emit('update', { ...props.block, content } as ReportBlock);
}

function updateStyle(style: BlockStyle) {
    if (!props.block) return;
    emit('update', { ...props.block, style });
}

function setIsEdited(value: boolean) {
    if (!props.block) return;
    emit('update', { ...props.block, isEdited: value });
}

function updateCoverColor(field: 'primaryColor', value: string) {
    if (!props.block || props.block.type !== 'cover') return;
    emit('update', { ...props.block, content: { ...props.block.content, [field]: value } });
}

function summaryCards(cards: KpiCard[]): KpiCard[] {
    return cards.filter(c => !c.sectionName);
}

function sectionCards(cards: KpiCard[]): KpiCard[] {
    return cards.filter(c => !!c.sectionName);
}

function toggleCard(label: string, sectionName: string | undefined, block: ReportBlock) {
    if (block.type !== 'kpi-grid') return;
    const updated = {
        ...block.content,
        cards: block.content.cards.map(c => {
            const match = sectionName
                ? c.sectionName === sectionName
                : c.label === label && !c.sectionName;
            return match ? { ...c, enabled: !c.enabled } : c;
        }),
    };
    emit('update', { ...block, content: updated } as ReportBlock);
}

function toggleAllCards(group: 'summary' | 'section', block: ReportBlock) {
    if (block.type !== 'kpi-grid') return;
    const relevant = group === 'summary'
        ? block.content.cards.filter(c => !c.sectionName)
        : block.content.cards.filter(c => !!c.sectionName);
    const allEnabled = relevant.every(c => c.enabled);
    const updated = {
        ...block.content,
        cards: block.content.cards.map(c => {
            const inGroup = group === 'summary' ? !c.sectionName : !!c.sectionName;
            return inGroup ? { ...c, enabled: !allEnabled } : c;
        }),
    };
    emit('update', { ...block, content: updated } as ReportBlock);
}

function blockLabel(type: BlockType): string {
    const labels: Record<BlockType, string> = {
        'cover': 'Cover Page',
        'heading': 'Heading',
        'kpi-grid': 'KPI Cards',
        'chart-bar': 'Bar Chart',
        'chart-line': 'Trend Chart',
        'narrative': 'Narrative',
        'callout': 'Callout',
        'ca-table': 'Corrective Actions',
        'image': 'Image',
    };
    return labels[type] ?? type;
}
</script>

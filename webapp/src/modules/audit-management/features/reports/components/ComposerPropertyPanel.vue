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

                <!-- ── Template picker ───────────────────────────────────── -->
                <button @click="showTemplateGallery = true"
                    class="w-full flex items-center justify-center gap-2 px-3 py-2 bg-blue-700 hover:bg-blue-600 text-white text-xs font-semibold rounded transition-colors">
                    <i class="pi pi-th-large" /> Choose Template
                </button>

                <!-- ── Size & spacing ─────────────────────────────────────── -->
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Block Height</label>
                    <div class="grid grid-cols-5 gap-1">
                        <button v-for="h in ['xs','sm','md','lg','xl']" :key="h"
                            @click="updateContent({ ...block.content, coverHeight: h as any })"
                            :class="['text-xs py-1 rounded border transition-colors',
                                (block.content.coverHeight ?? 'md') === h
                                    ? 'bg-blue-700 border-blue-500 text-white'
                                    : 'bg-slate-700 border-slate-600 text-slate-300 hover:bg-slate-600']">
                            {{ h.toUpperCase() }}
                        </button>
                    </div>
                </div>

                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Name Size</label>
                    <div class="grid grid-cols-5 gap-1">
                        <button v-for="s in ['sm','md','lg','xl','2xl']" :key="s"
                            @click="updateContent({ ...block.content, nameSize: s as any })"
                            :class="['text-xs py-1 rounded border transition-colors',
                                (block.content.nameSize ?? 'xl') === s
                                    ? 'bg-blue-700 border-blue-500 text-white'
                                    : 'bg-slate-700 border-slate-600 text-slate-300 hover:bg-slate-600']">
                            {{ s }}
                        </button>
                    </div>
                    <div class="text-xs text-slate-600">sm=19px · md=29px · lg=38px · xl=48px · 2xl=64px</div>
                </div>

                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Name Transform</label>
                    <div class="flex gap-1">
                        <button
                            @click="updateContent({ ...block.content, nameTransform: 'uppercase' })"
                            :class="['flex-1 text-xs py-1 rounded border transition-colors',
                                (block.content.nameTransform ?? 'uppercase') === 'uppercase'
                                    ? 'bg-blue-700 border-blue-500 text-white'
                                    : 'bg-slate-700 border-slate-600 text-slate-300 hover:bg-slate-600']">
                            UPPERCASE
                        </button>
                        <button
                            @click="updateContent({ ...block.content, nameTransform: 'none' })"
                            :class="['flex-1 text-xs py-1 rounded border transition-colors',
                                block.content.nameTransform === 'none'
                                    ? 'bg-blue-700 border-blue-500 text-white'
                                    : 'bg-slate-700 border-slate-600 text-slate-300 hover:bg-slate-600']">
                            As-Is
                        </button>
                    </div>
                </div>

                <!-- ── Color & image ───────────────────────────────────────── -->
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Background Color</label>
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
                    <div class="flex items-center justify-between">
                        <label class="text-xs text-slate-400">Overlay Darkness</label>
                        <span class="text-xs text-slate-500">{{ block.content.overlayOpacity ?? 40 }}%</span>
                    </div>
                    <input type="range" min="0" max="80" step="5"
                        :value="block.content.overlayOpacity ?? 40"
                        @input="updateContent({ ...block.content, overlayOpacity: Number(($event.target as HTMLInputElement).value) })"
                        class="w-full accent-blue-500" />
                    <div class="flex justify-between text-xs text-slate-600">
                        <span>None</span><span>Dark</span>
                    </div>
                </div>

                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Name Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.content.nameAccentColor || '#f59e0b'"
                            @input="updateContent({ ...block.content, nameAccentColor: ($event.target as HTMLInputElement).value })"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <input type="text" :value="block.content.nameAccentColor || ''"
                            @input="updateContent({ ...block.content, nameAccentColor: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="#f59e0b"
                            class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 font-mono focus:outline-none focus:border-blue-500" />
                        <button @click="updateContent({ ...block.content, nameAccentColor: undefined })"
                            class="text-xs text-slate-500 hover:text-slate-300">Clear</button>
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
                            @change="onImageUpload($event, (url) => updateContent({ ...block!.content, backgroundImageUrl: url }))" />
                    </label>
                    <p v-if="block.content.backgroundImageUrl" class="text-xs text-slate-500 truncate">{{ block.content.backgroundImageUrl.slice(0, 40) }}…</p>
                </div>

                <!-- ── Text content ────────────────────────────────────────── -->
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Subtitle (above name)</label>
                    <input type="text" :value="block.content.subtitle || ''"
                        @input="updateContent({ ...block.content, subtitle: ($event.target as HTMLInputElement).value || undefined })"
                        placeholder="e.g. 2024 Compliance"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>

                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Tagline (below name)</label>
                    <input type="text" :value="block.content.tagline || ''"
                        @input="updateContent({ ...block.content, tagline: ($event.target as HTMLInputElement).value || undefined })"
                        placeholder="e.g. A Year in Review"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>

                <!-- ── Decorative lines (prominent toggle) ────────────────── -->
                <div class="flex items-center justify-between py-2 px-3 rounded border"
                    :class="block.content.showDecorativeRules !== false ? 'border-blue-700/50 bg-blue-900/20' : 'border-slate-700 bg-slate-700/30'">
                    <div>
                        <div class="text-xs font-medium text-slate-300">Decorative Lines</div>
                        <div class="text-xs text-slate-500 mt-0.5">Horizontal rules above &amp; below name</div>
                    </div>
                    <button
                        @click="updateContent({ ...block.content, showDecorativeRules: !(block.content.showDecorativeRules !== false) })"
                        class="relative inline-flex h-5 w-9 items-center rounded-full transition-colors focus:outline-none shrink-0 ml-3"
                        :class="block.content.showDecorativeRules !== false ? 'bg-blue-600' : 'bg-slate-600'"
                    >
                        <span class="inline-block h-3.5 w-3.5 transform rounded-full bg-white transition-transform"
                            :class="block.content.showDecorativeRules !== false ? 'translate-x-4' : 'translate-x-1'" />
                    </button>
                </div>

            </template>

            <!-- ── Full Cover Page (cover-page block) ───────────────────── -->
            <template v-if="block.type === 'cover-page'">

                <!-- Template -->
                <div class="space-y-2">
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider border-b border-slate-700 pb-1">Template</div>
                    <button
                        @click="showCoverPagePicker = true"
                        class="w-full flex items-center justify-center gap-2 px-3 py-2 bg-blue-700 hover:bg-blue-600 text-white text-xs font-semibold rounded transition-colors focus-visible:ring-2 focus-visible:ring-blue-400"
                    >
                        <i class="pi pi-th-large" /> Change Template
                    </button>
                    <div class="text-[10px] text-slate-500 text-center">{{ block.content.templateId }}</div>
                </div>

                <!-- Theme overrides -->
                <div class="space-y-2">
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider border-b border-slate-700 pb-1">Theme Overrides</div>
                    <div class="grid grid-cols-2 gap-2">
                        <div class="space-y-1">
                            <label class="block text-[10px] text-slate-400">Background</label>
                            <div class="flex items-center gap-1.5">
                                <input type="color"
                                    :value="block.content.primaryColor || '#0f1e36'"
                                    @input="updateContent({ ...block.content, primaryColor: ($event.target as HTMLInputElement).value })"
                                    class="w-7 h-7 rounded border border-slate-600 cursor-pointer bg-transparent p-0.5"
                                    aria-label="Background color override"
                                />
                                <button v-if="block.content.primaryColor"
                                    @click="updateContent({ ...block.content, primaryColor: undefined })"
                                    class="text-slate-500 hover:text-slate-300 text-[10px]"
                                    aria-label="Reset background color">Reset</button>
                            </div>
                        </div>
                        <div class="space-y-1">
                            <label class="block text-[10px] text-slate-400">Accent</label>
                            <div class="flex items-center gap-1.5">
                                <input type="color"
                                    :value="block.content.accentColor || '#f59e0b'"
                                    @input="updateContent({ ...block.content, accentColor: ($event.target as HTMLInputElement).value })"
                                    class="w-7 h-7 rounded border border-slate-600 cursor-pointer bg-transparent p-0.5"
                                    aria-label="Accent color override"
                                />
                                <button v-if="block.content.accentColor"
                                    @click="updateContent({ ...block.content, accentColor: undefined })"
                                    class="text-slate-500 hover:text-slate-300 text-[10px]"
                                    aria-label="Reset accent color">Reset</button>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Content -->
                <div class="space-y-2">
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider border-b border-slate-700 pb-1">Content</div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Subtitle</label>
                        <input type="text" :value="block.content.reportSubtitle ?? ''"
                            @input="updateContent({ ...block.content, reportSubtitle: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="Annual Compliance Review"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Tagline</label>
                        <input type="text" :value="block.content.tagline ?? ''"
                            @input="updateContent({ ...block.content, tagline: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="A Year in Review"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Summary Text</label>
                        <textarea :value="block.content.summaryText ?? ''"
                            @input="updateContent({ ...block.content, summaryText: ($event.target as HTMLTextAreaElement).value || undefined })"
                            rows="3" placeholder="Sidebar intro text…"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500 resize-none" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Locations</label>
                        <input type="text" :value="block.content.locationsText ?? ''"
                            @input="updateContent({ ...block.content, locationsText: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="TX · OK · AR"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    </div>
                </div>

                <!-- Modules -->
                <div class="space-y-2">
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider border-b border-slate-700 pb-1">Modules</div>
                    <div v-for="(toggle, i) in ([
                        { key: 'showStats',      label: 'Stats Row' },
                        { key: 'showCallout',    label: 'Callout Box' },
                        { key: 'showLocations',  label: 'Locations' },
                        { key: 'showMap',        label: 'Map' },
                        { key: 'showHighlights', label: 'Highlights' },
                        { key: 'showAward',      label: 'Award' },
                    ] as const)" :key="i"
                        class="flex items-center justify-between">
                        <span class="text-xs text-slate-300">{{ toggle.label }}</span>
                        <button
                            @click="updateContent({ ...block.content, [toggle.key]: !(block.content as any)[toggle.key] })"
                            :aria-label="`Toggle ${toggle.label}`"
                            class="relative inline-flex h-5 w-9 items-center rounded-full transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-400 shrink-0"
                            :class="(block.content as any)[toggle.key] ? 'bg-blue-600' : 'bg-slate-600'"
                        >
                            <span class="inline-block h-3.5 w-3.5 transform rounded-full bg-white transition-transform"
                                :class="(block.content as any)[toggle.key] ? 'translate-x-4' : 'translate-x-1'" />
                        </button>
                    </div>
                </div>

                <!-- Images -->
                <div class="space-y-2">
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider border-b border-slate-700 pb-1">Images</div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Hero Image URL</label>
                        <input type="text" :value="block.content.heroImageUrl ?? ''"
                            @input="updateContent({ ...block.content, heroImageUrl: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="https://…"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Chart Image URL</label>
                        <input type="text" :value="block.content.chartImageUrl ?? ''"
                            @input="updateContent({ ...block.content, chartImageUrl: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="https://…"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Map Image URL</label>
                        <input type="text" :value="block.content.mapImageUrl ?? ''"
                            @input="updateContent({ ...block.content, mapImageUrl: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="https://…"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    </div>
                </div>

                <!-- Stats editor -->
                <div class="space-y-2">
                    <div class="flex items-center justify-between border-b border-slate-700 pb-1">
                        <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider">Stats</div>
                        <button
                            v-if="block.content.stats.length < 6"
                            @click="updateContent({ ...block.content, stats: [...block.content.stats, { label: 'Label', value: '—' }] })"
                            class="text-blue-400 hover:text-blue-300 text-xs focus-visible:ring-1 focus-visible:ring-blue-400 rounded px-1"
                            aria-label="Add stat"
                        >+ Add</button>
                    </div>
                    <div v-for="(stat, si) in block.content.stats" :key="si" class="space-y-1 p-2 bg-slate-700/40 rounded">
                        <div class="flex gap-1">
                            <input type="text" :value="stat.value"
                                @input="updateContent({ ...block.content, stats: block.content.stats.map((s: CoverStat, j: number) => j === si ? { ...s, value: ($event.target as HTMLInputElement).value } : s) })"
                                placeholder="Value"
                                class="w-1/3 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500"
                                :aria-label="`Stat ${si + 1} value`"
                            />
                            <input type="text" :value="stat.label"
                                @input="updateContent({ ...block.content, stats: block.content.stats.map((s: CoverStat, j: number) => j === si ? { ...s, label: ($event.target as HTMLInputElement).value } : s) })"
                                placeholder="Label"
                                class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500"
                                :aria-label="`Stat ${si + 1} label`"
                            />
                            <button
                                @click="updateContent({ ...block.content, stats: block.content.stats.filter((_: CoverStat, j: number) => j !== si) })"
                                class="text-slate-500 hover:text-red-400 px-1 text-xs focus-visible:ring-1 focus-visible:ring-red-400 rounded"
                                :aria-label="`Remove stat ${si + 1}`"
                            ><i class="pi pi-times" /></button>
                        </div>
                        <label class="flex items-center gap-2 text-[10px] text-slate-400 cursor-pointer">
                            <input type="checkbox" :checked="!!stat.accent"
                                @change="updateContent({ ...block.content, stats: block.content.stats.map((s: CoverStat, j: number) => j === si ? { ...s, accent: ($event.target as HTMLInputElement).checked } : s) })"
                                class="rounded" :aria-label="`Stat ${si + 1} accent color`"
                            /> Accent color
                        </label>
                    </div>
                </div>

                <!-- Callout editor (shown when callout module is on) -->
                <div v-if="block.content.showCallout" class="space-y-2">
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider border-b border-slate-700 pb-1">Callout</div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Headline</label>
                        <input type="text" :value="block.content.calloutTitle ?? ''"
                            @input="updateContent({ ...block.content, calloutTitle: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="Callout headline"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Body</label>
                        <textarea :value="block.content.calloutBody ?? ''"
                            @input="updateContent({ ...block.content, calloutBody: ($event.target as HTMLTextAreaElement).value || undefined })"
                            rows="3" placeholder="Supporting detail…"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500 resize-none" />
                    </div>
                </div>

                <!-- Award editor (shown when award module is on) -->
                <div v-if="block.content.showAward" class="space-y-2">
                    <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider border-b border-slate-700 pb-1">Award</div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Title</label>
                        <input type="text" :value="block.content.awardTitle ?? ''"
                            @input="updateContent({ ...block.content, awardTitle: ($event.target as HTMLInputElement).value || undefined })"
                            placeholder="Award title"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-[10px] text-slate-400">Description</label>
                        <textarea :value="block.content.awardDescription ?? ''"
                            @input="updateContent({ ...block.content, awardDescription: ($event.target as HTMLTextAreaElement).value || undefined })"
                            rows="2" placeholder="Award description"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500 resize-none" />
                    </div>
                </div>

                <!-- Highlights editor (shown when highlights module is on) -->
                <div v-if="block.content.showHighlights" class="space-y-2">
                    <div class="flex items-center justify-between border-b border-slate-700 pb-1">
                        <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider">Highlights</div>
                        <button
                            v-if="block.content.highlights.length < 5"
                            @click="updateContent({ ...block.content, highlights: [...block.content.highlights, { icon: 'pi pi-check-circle', text: 'Highlight' }] })"
                            class="text-blue-400 hover:text-blue-300 text-xs focus-visible:ring-1 focus-visible:ring-blue-400 rounded px-1"
                            aria-label="Add highlight"
                        >+ Add</button>
                    </div>
                    <div v-for="(h, hi) in block.content.highlights" :key="hi" class="space-y-1 p-2 bg-slate-700/40 rounded">
                        <div class="flex gap-1">
                            <input type="text" :value="h.icon"
                                @input="updateContent({ ...block.content, highlights: block.content.highlights.map((x: CoverHighlight, j: number) => j === hi ? { ...x, icon: ($event.target as HTMLInputElement).value } : x) })"
                                placeholder="pi pi-check-circle"
                                class="w-2/5 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500"
                                :aria-label="`Highlight ${hi + 1} icon`"
                            />
                            <input type="text" :value="h.text"
                                @input="updateContent({ ...block.content, highlights: block.content.highlights.map((x: CoverHighlight, j: number) => j === hi ? { ...x, text: ($event.target as HTMLInputElement).value } : x) })"
                                placeholder="Highlight text"
                                class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500"
                                :aria-label="`Highlight ${hi + 1} text`"
                            />
                            <button
                                @click="updateContent({ ...block.content, highlights: block.content.highlights.filter((_: CoverHighlight, j: number) => j !== hi) })"
                                class="text-slate-500 hover:text-red-400 px-1 text-xs focus-visible:ring-1 focus-visible:ring-red-400 rounded"
                                :aria-label="`Remove highlight ${hi + 1}`"
                            ><i class="pi pi-times" /></button>
                        </div>
                    </div>
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
                <div class="grid grid-cols-2 gap-2">
                    <div class="space-y-1">
                        <label class="block text-xs text-slate-400">Font Size (px)</label>
                        <input type="number" min="8" max="120" step="1"
                            :value="block.content.fontSize || ''"
                            placeholder="auto"
                            @change="updateContent({ ...block.content, fontSize: ($event.target as HTMLInputElement).value ? Number(($event.target as HTMLInputElement).value) : undefined })"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-xs text-slate-400">Weight</label>
                        <select :value="block.content.fontWeight || 600"
                            @change="updateContent({ ...block.content, fontWeight: Number(($event.target as HTMLSelectElement).value) as any })"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none">
                            <option value="400">Regular</option>
                            <option value="500">Medium</option>
                            <option value="600">Semibold</option>
                            <option value="700">Bold</option>
                            <option value="800">Extra Bold</option>
                            <option value="900">Black</option>
                        </select>
                    </div>
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
                            @change="onImageUpload($event, (url) => updateContent({ ...block!.content, backgroundImageUrl: url }))" />
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
                            @change="onImageUpload($event, (url) => updateContent({ ...block!.content, url }))" />
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

            <!-- Column Row -->
            <template v-if="block.type === 'column-row'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Column Ratio</label>
                    <div class="grid grid-cols-3 gap-1">
                        <button v-for="r in ['50/50','60/40','40/60','70/30','30/70']" :key="r"
                            @click="updateContent({ ...block.content, ratio: r })"
                            :class="['px-1.5 py-1 rounded text-xs transition-colors', block.content.ratio === r ? 'bg-blue-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600']">
                            {{ r }}
                        </button>
                    </div>
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Column Gap</label>
                    <div class="flex gap-1">
                        <button v-for="g in ['none','sm','md','lg']" :key="g"
                            @click="updateContent({ ...block.content, gap: g })"
                            :class="['flex-1 px-2 py-1 rounded text-xs transition-colors capitalize', block.content.gap === g ? 'bg-blue-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600']">
                            {{ g }}
                        </button>
                    </div>
                </div>
                <p class="text-xs text-slate-500 italic">Use the + Add Block buttons inside each column to add content.</p>
            </template>

            <!-- Divider -->
            <template v-if="block.type === 'divider'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Thickness</label>
                    <div class="flex gap-1">
                        <button v-for="t in [1, 2, 4]" :key="t"
                            @click="updateContent({ ...block.content, thickness: t })"
                            :class="['flex-1 px-2 py-1 rounded text-xs transition-colors', block.content.thickness === t ? 'bg-blue-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600']">
                            {{ t }}px
                        </button>
                    </div>
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Style</label>
                    <div class="flex gap-1">
                        <button v-for="v in ['solid','dashed','dotted']" :key="v"
                            @click="updateContent({ ...block.content, variant: v })"
                            :class="['flex-1 px-2 py-1 rounded text-xs transition-colors capitalize', block.content.variant === v ? 'bg-blue-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600']">
                            {{ v }}
                        </button>
                    </div>
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.content.color || '#475569'"
                            @input="updateContent({ ...block.content, color: ($event.target as HTMLInputElement).value })"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <input type="text" :value="block.content.color || '#475569'"
                            @input="updateContent({ ...block.content, color: ($event.target as HTMLInputElement).value })"
                            class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 font-mono focus:outline-none focus:border-blue-500" />
                    </div>
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Margin</label>
                    <div class="flex gap-1">
                        <button v-for="m in ['sm','md','lg']" :key="m"
                            @click="updateContent({ ...block.content, marginY: m })"
                            :class="['flex-1 px-2 py-1 rounded text-xs transition-colors capitalize', block.content.marginY === m ? 'bg-blue-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600']">
                            {{ m }}
                        </button>
                    </div>
                </div>
            </template>

            <!-- Spacer -->
            <template v-if="block.type === 'spacer'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Height</label>
                    <div class="grid grid-cols-5 gap-1">
                        <button v-for="[k, px] in [['xs','8px'],['sm','16px'],['md','32px'],['lg','48px'],['xl','64px']]" :key="k"
                            @click="updateContent({ ...block.content, height: k })"
                            :class="['flex flex-col items-center px-1 py-1.5 rounded text-xs transition-colors', block.content.height === k ? 'bg-blue-600 text-white' : 'bg-slate-700 text-slate-300 hover:bg-slate-600']">
                            <span class="font-semibold">{{ k }}</span>
                            <span class="text-[10px] opacity-70">{{ px }}</span>
                        </button>
                    </div>
                </div>
            </template>

            <!-- TOC Sidebar -->
            <template v-if="block.type === 'toc-sidebar'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Title</label>
                    <input type="text" :value="block.content.title"
                        @input="updateContent({ ...block.content, title: ($event.target as HTMLInputElement).value })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>
                <div class="flex items-center justify-between">
                    <label class="text-xs text-slate-400">Dark Background</label>
                    <button
                        @click="updateContent({ ...block.content, darkBackground: !block.content.darkBackground })"
                        class="relative inline-flex h-5 w-9 items-center rounded-full transition-colors focus:outline-none"
                        :class="block.content.darkBackground ? 'bg-blue-600' : 'bg-slate-600'"
                    >
                        <span class="inline-block h-3.5 w-3.5 transform rounded-full bg-white transition-transform"
                            :class="block.content.darkBackground ? 'translate-x-4' : 'translate-x-1'" />
                    </button>
                </div>
                <div class="space-y-2">
                    <div class="flex items-center justify-between">
                        <label class="text-xs text-slate-400 font-medium uppercase tracking-wide">Items</label>
                        <button @click="addTocItem(block)"
                            class="text-xs text-blue-400 hover:text-blue-300 flex items-center gap-1">
                            <i class="pi pi-plus text-xs" /> Add
                        </button>
                    </div>
                    <div v-for="(item, i) in block.content.items" :key="i" class="bg-slate-900 rounded p-2 space-y-1">
                        <input type="text" :value="item.heading"
                            @input="updateTocItem(block, i, 'heading', ($event.target as HTMLInputElement).value)"
                            placeholder="Section heading"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                        <input type="text" :value="item.description || ''"
                            @input="updateTocItem(block, i, 'description', ($event.target as HTMLInputElement).value)"
                            placeholder="Short description (optional)"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                        <button @click="removeTocItem(block, i)"
                            class="text-xs text-red-400 hover:text-red-300">Remove</button>
                    </div>
                    <p v-if="!block.content.items.length" class="text-xs text-slate-500 italic">No items yet. Click Add.</p>
                </div>
            </template>

            <!-- Oval Callout -->
            <template v-if="block.type === 'oval-callout'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Title</label>
                    <input type="text" :value="block.content.title"
                        @input="updateContent({ ...block.content, title: ($event.target as HTMLInputElement).value })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Phonetic (optional)</label>
                    <input type="text" :value="block.content.phonetic || ''"
                        @input="updateContent({ ...block.content, phonetic: ($event.target as HTMLInputElement).value || undefined })"
                        placeholder="/'strôNG.hōld/ noun."
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Body</label>
                    <textarea :value="block.content.body" rows="3"
                        @input="updateContent({ ...block.content, body: ($event.target as HTMLTextAreaElement).value })"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500 resize-none" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Background Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.content.backgroundColor || '#1e3a5f'"
                            @input="updateContent({ ...block.content, backgroundColor: ($event.target as HTMLInputElement).value })"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <input type="text" :value="block.content.backgroundColor || '#1e3a5f'"
                            @input="updateContent({ ...block.content, backgroundColor: ($event.target as HTMLInputElement).value })"
                            class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 font-mono focus:outline-none focus:border-blue-500" />
                    </div>
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Text Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.content.textColor || '#ffffff'"
                            @input="updateContent({ ...block.content, textColor: ($event.target as HTMLInputElement).value })"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <button @click="updateContent({ ...block.content, textColor: undefined })"
                            class="text-xs text-slate-500 hover:text-slate-300">Clear</button>
                    </div>
                </div>
            </template>

            <!-- Findings Category -->
            <template v-if="block.type === 'findings-category'">
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Section Name</label>
                    <input type="text" :value="block.content.sectionName"
                        @input="updateContent({ ...block.content, sectionName: ($event.target as HTMLInputElement).value })"
                        placeholder="e.g. Confined Space Procedures"
                        class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none focus:border-blue-500" />
                </div>
                <div class="space-y-1">
                    <label class="block text-xs text-slate-400">Header Accent Color</label>
                    <div class="flex items-center gap-2">
                        <input type="color" :value="block.content.accentColor || '#862633'"
                            @input="updateContent({ ...block.content, accentColor: ($event.target as HTMLInputElement).value })"
                            class="w-8 h-8 rounded cursor-pointer bg-transparent border-0 p-0" />
                        <input type="text" :value="block.content.accentColor || '#862633'"
                            @input="updateContent({ ...block.content, accentColor: ($event.target as HTMLInputElement).value })"
                            class="flex-1 bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 font-mono focus:outline-none focus:border-blue-500" />
                    </div>
                </div>
                <div class="flex items-center gap-2">
                    <input type="checkbox" :checked="block.content.showExamplesLabel"
                        @change="updateContent({ ...block.content, showExamplesLabel: ($event.target as HTMLInputElement).checked })"
                        class="rounded" id="fc-show-examples" />
                    <label for="fc-show-examples" class="text-xs text-slate-400 cursor-pointer">Show "Examples:" label</label>
                </div>
                <div class="text-xs text-slate-500">Edit findings directly on the canvas using the rich text toolbar.</div>
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

            <!-- ── Position & Size (all blocks) ─────────────────────────── -->
            <div class="pt-3 border-t border-slate-700 space-y-3">
                <div class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Position &amp; Size</div>
                <div class="grid grid-cols-2 gap-2">
                    <div class="space-y-1">
                        <label class="block text-xs text-slate-400">X</label>
                        <input type="number" :value="block.layout?.x ?? 40"
                            @change="updateLayout({ x: +($event.target as HTMLInputElement).value })"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-xs text-slate-400">Y</label>
                        <input type="number" :value="block.layout?.y ?? 0"
                            @change="updateLayout({ y: +($event.target as HTMLInputElement).value })"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-xs text-slate-400">Width</label>
                        <input type="number" :value="block.layout?.width ?? 714"
                            @change="updateLayout({ width: +($event.target as HTMLInputElement).value })"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none" />
                    </div>
                    <div class="space-y-1">
                        <label class="block text-xs text-slate-400">Height</label>
                        <input type="number" :value="block.layout?.height ?? 0" placeholder="auto"
                            @change="updateLayout({ height: +($event.target as HTMLInputElement).value })"
                            class="w-full bg-slate-700 border border-slate-600 rounded px-2 py-1 text-xs text-slate-200 focus:outline-none" />
                    </div>
                </div>
                <div class="flex gap-1">
                    <button @click="$emit('bring-forward', block.id)" class="flex-1 text-xs py-1 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 border border-slate-600">↑ Forward</button>
                    <button @click="$emit('send-backward', block.id)" class="flex-1 text-xs py-1 bg-slate-700 hover:bg-slate-600 rounded text-slate-300 border border-slate-600">↓ Back</button>
                </div>
                <button
                    @click="updateLayout({ width: 714, height: 0, x: 40 })"
                    class="w-full text-xs py-1 bg-slate-700 hover:bg-slate-600 rounded text-slate-400 hover:text-slate-200 border border-slate-600 transition-colors"
                    title="Reset to full-width auto-height"
                >
                    Reset Size &amp; Position
                </button>
                <label class="flex items-center gap-2 cursor-pointer">
                    <input type="checkbox" :checked="block.layout?.locked ?? false"
                        @change="updateLayout({ locked: ($event.target as HTMLInputElement).checked })"
                        class="rounded" />
                    <span class="text-xs text-slate-400">Lock position</span>
                </label>
            </div>
        </div>
    </div>

    <!-- Cover template gallery modal (existing cover block) -->
    <CoverTemplateGallery
        v-if="block?.type === 'cover'"
        v-model="showTemplateGallery"
        :current-content="(block as any).content"
        @apply="applyPreset"
    />

    <!-- Cover-page template picker modal (full-page cover-page block) -->
    <CoverPageTemplatePicker
        v-if="block?.type === 'cover-page'"
        v-model="showCoverPagePicker"
        :current-template-id="(block as any).content.templateId"
        @apply="(id: string) => updateContent({ ...(block as any).content, templateId: id })"
    />
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { ReportBlock, BlockStyle, BlockLayout, BlockType, KpiCard, CoverContent, CoverPageContent, CoverStat, CoverHighlight } from '../types/report-block';
import CoverTemplateGallery from './CoverTemplateGallery.vue';
import CoverPageTemplatePicker from './CoverPageTemplatePicker.vue';

// ── Cover template gallery (existing cover block) ─────────────────────────────
const showTemplateGallery = ref(false);

// ── Cover-page template picker (full-page cover-page block) ──────────────────
const showCoverPagePicker = ref(false);

function applyPreset(preset: Partial<CoverContent>) {
    if (!props.block || props.block.type !== 'cover') return;
    const content = props.block.content;
    // Merge preset visual properties; always preserve the text/identity fields
    updateContent({
        ...content,
        ...preset,
        divisionName: content.divisionName,
        divisionCode: content.divisionCode,
        period: content.period,
        preparedBy: content.preparedBy,
        subtitle: content.subtitle,
        tagline: content.tagline,
    });
}

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
    (e: 'bring-forward', id: string): void;
    (e: 'send-backward', id: string): void;
}>();

function updateLayout(layout: Partial<BlockLayout>) {
    if (!props.block) return;
    emit('update', { ...props.block, layout: { ...props.block.layout, ...layout } as BlockLayout });
}

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

function addTocItem(block: ReportBlock) {
    if (block.type !== 'toc-sidebar') return;
    updateContent({ ...block.content, items: [...block.content.items, { heading: 'New Section', description: '' }] });
}

function removeTocItem(block: ReportBlock, index: number) {
    if (block.type !== 'toc-sidebar') return;
    const items = block.content.items.filter((_, i) => i !== index);
    updateContent({ ...block.content, items });
}

function updateTocItem(block: ReportBlock, index: number, field: 'heading' | 'description', value: string) {
    if (block.type !== 'toc-sidebar') return;
    const items = block.content.items.map((item, i) =>
        i === index ? { ...item, [field]: value || undefined } : item
    );
    updateContent({ ...block.content, items });
}

function blockLabel(type: BlockType): string {
    const labels: Record<BlockType, string> = {
        'cover':        'Cover Page',
        'cover-page':   'Full Cover Page',
        'heading':      'Heading',
        'kpi-grid':     'KPI Cards',
        'chart-bar':    'Bar Chart',
        'chart-line':   'Trend Chart',
        'narrative':    'Narrative',
        'callout':      'Callout',
        'ca-table':     'Corrective Actions',
        'image':        'Image',
        'column-row':   'Two Column Row',
        'divider':      'Divider',
        'spacer':       'Spacer',
        'toc-sidebar':        'TOC / Inside Panel',
        'oval-callout':       'Oval Callout',
        'findings-category':  'Findings Category',
    };
    return labels[type] ?? type;
}
</script>

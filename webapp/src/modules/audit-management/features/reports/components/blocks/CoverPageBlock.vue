<template>
    <!-- Root fills the canvas slot exactly — position:absolute;inset:0 via .cpb-root -->
    <div class="cpb-root" :style="rootStyle">

        <!-- ── STRONGHOLD-DARK: left accent stripe + sidebar + main ──────── -->
        <template v-if="tpl.id === 'stronghold-dark'">
            <!-- Accent stripe (left edge, full height) -->
            <div class="cpb-abs cpb-stripe-left" :style="{ width: `${tpl.layout.accentStripeSize}px`, background: ac }" />

            <!-- Body row: sidebar + main content -->
            <div class="cpb-abs" :style="{ left: `${tpl.layout.accentStripeSize}px`, right: 0, top: 0, bottom: `${tpl.layout.footerHeight}px`, display: 'flex' }">

                <!-- Sidebar -->
                <div class="cpb-sidebar" :style="{ width: `${tpl.layout.sidebarWidth}px`, background: tpl.theme.sidebarBg }">
                    <!-- Logo placeholder -->
                    <div class="cpb-logo-ph">
                        <i class="pi pi-shield" style="font-size:1.4rem" />
                    </div>
                    <!-- Division name -->
                    <h1
                        ref="nameEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Division name"
                        class="cpb-sidebar-name outline-none"
                        :style="{ color: ac }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('divisionName', $event)"
                        @keydown.enter.prevent="($refs.nameEl as HTMLElement)?.blur()"
                    />
                    <div class="cpb-sidebar-year" :style="{ color: tpl.theme.textMuted }">{{ content.year }}</div>
                    <div class="cpb-sidebar-rule" :style="{ background: ac }" />
                    <!-- Summary text -->
                    <div
                        v-if="content.showLocations || editing"
                        ref="summaryEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Summary text"
                        class="cpb-sidebar-summary outline-none"
                        data-placeholder="Summary text…"
                        :style="{ color: tpl.theme.textBody }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('summaryText', $event)"
                        @keydown.enter.prevent="($refs.summaryEl as HTMLElement)?.blur()"
                    />
                    <!-- Locations -->
                    <div v-if="content.showLocations" class="cpb-sidebar-section">
                        <div class="cpb-sidebar-section-label" :style="{ color: tpl.theme.textMuted }">Operations</div>
                        <div
                            ref="locationsEl"
                            contenteditable="true"
                            spellcheck="false"
                            aria-label="Locations"
                            class="cpb-sidebar-locations outline-none"
                            data-placeholder="TX · OK · AR…"
                            :style="{ color: tpl.theme.textBody }"
                            @paste.prevent="onPaste"
                            @focus="editing = true"
                            @blur="onBlur('locationsText', $event)"
                            @keydown.enter.prevent="($refs.locationsEl as HTMLElement)?.blur()"
                        />
                    </div>
                    <!-- Highlights -->
                    <div v-if="content.showHighlights && visibleHighlights.length" class="cpb-sidebar-highlights">
                        <div v-for="(h, i) in visibleHighlights" :key="i" class="cpb-highlight-row" :style="{ color: tpl.theme.textBody }">
                            <i :class="h.icon || 'pi pi-check-circle'" :style="{ color: ac, flexShrink: 0 }" />
                            <span>{{ h.text }}</span>
                        </div>
                    </div>
                </div>

                <!-- Main content area -->
                <div class="cpb-main" :style="{ flex: 1, background: pc }">
                    <!-- Top band -->
                    <div class="cpb-band" :style="{ height: `${tpl.layout.bandHeight}px`, background: tpl.theme.bandBg, position: 'relative' }">
                        <!-- Hero image if set -->
                        <template v-if="content.heroImageUrl">
                            <div class="cpb-abs" :style="heroImgStyle" />
                            <div class="cpb-abs" style="background:rgba(0,0,0,0.45)" />
                        </template>
                        <div v-else class="cpb-img-placeholder cpb-hero-ph">
                            <i class="pi pi-image" />
                            <span>Hero image URL in panel</span>
                        </div>
                        <!-- Band text overlay -->
                        <div class="cpb-band-content" :style="{ padding: '28px 32px' }">
                            <div
                                ref="subtitleEl"
                                contenteditable="true"
                                spellcheck="false"
                                aria-label="Report subtitle"
                                class="cpb-band-subtitle outline-none"
                                data-placeholder="Subtitle…"
                                :style="{ color: tpl.theme.textMuted }"
                                @paste.prevent="onPaste"
                                @focus="editing = true"
                                @blur="onBlur('reportSubtitle', $event)"
                                @keydown.enter.prevent="($refs.subtitleEl as HTMLElement)?.blur()"
                            />
                            <div
                                ref="taglineEl"
                                contenteditable="true"
                                spellcheck="false"
                                aria-label="Tagline"
                                class="cpb-band-tagline outline-none"
                                data-placeholder="A Year in Review"
                                :style="{ color: 'rgba(255,255,255,0.85)' }"
                                @paste.prevent="onPaste"
                                @focus="editing = true"
                                @blur="onBlur('tagline', $event)"
                                @keydown.enter.prevent="($refs.taglineEl as HTMLElement)?.blur()"
                            />
                        </div>
                    </div>

                    <!-- Below-band area: stats + callout + award + chart -->
                    <div class="cpb-below-band" :style="{ padding: '20px 24px', display: 'flex', flexDirection: 'column', gap: '16px' }">
                        <!-- Stats row -->
                        <div v-if="content.showStats && visibleStats.length" class="cpb-stats-row">
                            <div v-for="(stat, i) in visibleStats" :key="i" class="cpb-stat-pill" :style="{ background: 'rgba(255,255,255,0.06)', borderColor: 'rgba(255,255,255,0.1)' }">
                                <div class="cpb-stat-value" :style="{ color: stat.accent ? ac : tpl.theme.textHeading }">{{ stat.value }}</div>
                                <div class="cpb-stat-label" :style="{ color: tpl.theme.textMuted }">{{ stat.label }}</div>
                            </div>
                        </div>
                        <!-- Callout box -->
                        <div v-if="content.showCallout" class="cpb-callout-box" :style="{ borderColor: ac, background: 'rgba(255,255,255,0.04)' }">
                            <div
                                ref="calloutTitleEl"
                                contenteditable="true"
                                spellcheck="false"
                                aria-label="Callout title"
                                class="cpb-callout-title outline-none"
                                data-placeholder="Callout headline…"
                                :style="{ color: ac }"
                                @paste.prevent="onPaste"
                                @focus="editing = true"
                                @blur="onBlur('calloutTitle', $event)"
                                @keydown.enter.prevent="($refs.calloutTitleEl as HTMLElement)?.blur()"
                            />
                            <div
                                ref="calloutBodyEl"
                                contenteditable="true"
                                spellcheck="false"
                                aria-label="Callout body"
                                class="cpb-callout-body outline-none"
                                data-placeholder="Supporting detail…"
                                :style="{ color: tpl.theme.textBody }"
                                @paste.prevent="onPaste"
                                @focus="editing = true"
                                @blur="onBlur('calloutBody', $event)"
                            />
                        </div>
                        <!-- Chart placeholder -->
                        <div class="cpb-chart-area">
                            <template v-if="content.chartImageUrl">
                                <img :src="content.chartImageUrl" class="cpb-chart-img" alt="Chart" />
                            </template>
                            <div v-else class="cpb-img-placeholder">
                                <i class="pi pi-chart-bar" />
                                <span>Chart image URL in panel</span>
                            </div>
                        </div>
                        <!-- Award -->
                        <div v-if="content.showAward" class="cpb-award-box" :style="{ borderColor: ac }">
                            <i class="pi pi-star-fill cpb-award-icon" :style="{ color: ac }" />
                            <div>
                                <div
                                    ref="awardTitleEl"
                                    contenteditable="true"
                                    spellcheck="false"
                                    aria-label="Award title"
                                    class="cpb-award-title outline-none"
                                    data-placeholder="Award title…"
                                    :style="{ color: tpl.theme.textHeading }"
                                    @paste.prevent="onPaste"
                                    @focus="editing = true"
                                    @blur="onBlur('awardTitle', $event)"
                                    @keydown.enter.prevent="($refs.awardTitleEl as HTMLElement)?.blur()"
                                />
                                <div
                                    ref="awardDescEl"
                                    contenteditable="true"
                                    spellcheck="false"
                                    aria-label="Award description"
                                    class="cpb-award-desc outline-none"
                                    data-placeholder="Award description…"
                                    :style="{ color: tpl.theme.textBody }"
                                    @paste.prevent="onPaste"
                                    @focus="editing = true"
                                    @blur="onBlur('awardDescription', $event)"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Footer -->
            <div class="cpb-footer cpb-abs" :style="{ height: `${tpl.layout.footerHeight}px`, bottom: 0, left: `${tpl.layout.accentStripeSize}px`, right: 0, background: tpl.theme.footerBg }">
                <span :style="{ color: tpl.theme.textMuted }">{{ content.preparedBy }}</span>
                <span :style="{ color: tpl.theme.textMuted }">{{ content.period }}</span>
            </div>
        </template>

        <!-- ── SAFETY-RED: full-width top band, no sidebar ───────────────── -->
        <template v-else-if="tpl.id === 'safety-red'">
            <!-- Top band -->
            <div class="cpb-abs" :style="{ top: 0, left: 0, right: 0, height: `${tpl.layout.bandHeight}px`, background: tpl.theme.bandBg, position: 'relative', overflow: 'hidden' }">
                <template v-if="content.heroImageUrl">
                    <div class="cpb-abs" :style="heroImgStyle" />
                    <div class="cpb-abs" style="background:rgba(0,0,0,0.55)" />
                </template>
                <div v-else class="cpb-img-placeholder cpb-hero-ph">
                    <i class="pi pi-image" />
                    <span>Hero image URL in panel</span>
                </div>
                <!-- Band text -->
                <div class="cpb-band-content cpb-abs" style="bottom:0;left:0;right:0;padding:32px 40px">
                    <div
                        ref="subtitleEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Report subtitle"
                        class="cpb-sr-subtitle outline-none"
                        data-placeholder="Subtitle…"
                        :style="{ color: tpl.theme.textMuted }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('reportSubtitle', $event)"
                        @keydown.enter.prevent="($refs.subtitleEl as HTMLElement)?.blur()"
                    />
                    <h1
                        ref="nameEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Division name"
                        class="cpb-sr-name outline-none"
                        :style="{ color: ac }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('divisionName', $event)"
                        @keydown.enter.prevent="($refs.nameEl as HTMLElement)?.blur()"
                    />
                    <div
                        ref="taglineEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Tagline"
                        class="cpb-sr-tagline outline-none"
                        data-placeholder="A Year in Review"
                        :style="{ color: 'rgba(255,255,255,0.75)' }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('tagline', $event)"
                        @keydown.enter.prevent="($refs.taglineEl as HTMLElement)?.blur()"
                    />
                </div>
            </div>

            <!-- Main body below band -->
            <div class="cpb-abs" :style="{ top: `${tpl.layout.bandHeight}px`, left: 0, right: 0, bottom: `${tpl.layout.footerHeight}px`, background: pc, padding: '24px 40px', display: 'flex', flexDirection: 'column', gap: '16px' }">
                <!-- Stats -->
                <div v-if="content.showStats && visibleStats.length" class="cpb-stats-row">
                    <div v-for="(stat, i) in visibleStats" :key="i" class="cpb-stat-pill" :style="{ background: 'rgba(255,255,255,0.05)', borderColor: 'rgba(255,255,255,0.1)' }">
                        <div class="cpb-stat-value" :style="{ color: stat.accent ? ac : tpl.theme.textHeading }">{{ stat.value }}</div>
                        <div class="cpb-stat-label" :style="{ color: tpl.theme.textMuted }">{{ stat.label }}</div>
                    </div>
                </div>
                <!-- Callout -->
                <div v-if="content.showCallout" class="cpb-callout-box" :style="{ borderColor: ac, background: 'rgba(255,255,255,0.04)' }">
                    <div
                        ref="calloutTitleEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Callout title"
                        class="cpb-callout-title outline-none"
                        data-placeholder="Callout headline…"
                        :style="{ color: ac }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('calloutTitle', $event)"
                        @keydown.enter.prevent="($refs.calloutTitleEl as HTMLElement)?.blur()"
                    />
                    <div
                        ref="calloutBodyEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Callout body"
                        class="cpb-callout-body outline-none"
                        data-placeholder="Supporting detail…"
                        :style="{ color: tpl.theme.textBody }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('calloutBody', $event)"
                    />
                </div>
                <!-- Chart -->
                <div class="cpb-chart-area" style="flex:1">
                    <template v-if="content.chartImageUrl">
                        <img :src="content.chartImageUrl" class="cpb-chart-img" alt="Chart" />
                    </template>
                    <div v-else class="cpb-img-placeholder">
                        <i class="pi pi-chart-bar" />
                        <span>Chart image URL in panel</span>
                    </div>
                </div>
                <!-- Locations -->
                <div v-if="content.showLocations" class="cpb-locations-row">
                    <i class="pi pi-map-marker" :style="{ color: ac, flexShrink: 0 }" />
                    <div
                        ref="locationsEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Locations"
                        class="cpb-locations-text outline-none"
                        data-placeholder="Operations in TX · OK · AR…"
                        :style="{ color: tpl.theme.textBody }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('locationsText', $event)"
                        @keydown.enter.prevent="($refs.locationsEl as HTMLElement)?.blur()"
                    />
                </div>
                <!-- Award -->
                <div v-if="content.showAward" class="cpb-award-box" :style="{ borderColor: ac }">
                    <i class="pi pi-star-fill cpb-award-icon" :style="{ color: ac }" />
                    <div>
                        <div
                            ref="awardTitleEl"
                            contenteditable="true"
                            spellcheck="false"
                            aria-label="Award title"
                            class="cpb-award-title outline-none"
                            data-placeholder="Award title…"
                            :style="{ color: tpl.theme.textHeading }"
                            @paste.prevent="onPaste"
                            @focus="editing = true"
                            @blur="onBlur('awardTitle', $event)"
                            @keydown.enter.prevent="($refs.awardTitleEl as HTMLElement)?.blur()"
                        />
                        <div
                            ref="awardDescEl"
                            contenteditable="true"
                            spellcheck="false"
                            aria-label="Award description"
                            class="cpb-award-desc outline-none"
                            data-placeholder="Award description…"
                            :style="{ color: tpl.theme.textBody }"
                            @paste.prevent="onPaste"
                            @focus="editing = true"
                            @blur="onBlur('awardDescription', $event)"
                        />
                    </div>
                </div>
            </div>

            <!-- Footer -->
            <div class="cpb-footer cpb-abs" :style="{ height: `${tpl.layout.footerHeight}px`, bottom: 0, left: 0, right: 0, background: tpl.theme.footerBg }">
                <span :style="{ color: tpl.theme.textMuted }">{{ content.preparedBy }}</span>
                <span :style="{ color: tpl.theme.textMuted }">{{ content.period }}</span>
            </div>
        </template>

        <!-- ── EXECUTIVE-MINIMAL: light page, centered band ──────────────── -->
        <template v-else-if="tpl.id === 'executive-minimal'">
            <!-- Top accent stripe -->
            <div v-if="tpl.layout.accentStripe === 'top-page'" class="cpb-abs" :style="{ top: 0, left: 0, right: 0, height: `${tpl.layout.accentStripeSize}px`, background: ac }" />

            <!-- Band -->
            <div class="cpb-abs" :style="{ top: `${tpl.layout.accentStripeSize}px`, left: 0, right: 0, height: `${tpl.layout.bandHeight}px`, background: tpl.theme.bandBg, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', padding: '40px' }">
                <div
                    ref="subtitleEl"
                    contenteditable="true"
                    spellcheck="false"
                    aria-label="Report subtitle"
                    class="cpb-em-subtitle outline-none"
                    data-placeholder="2025 Compliance Review"
                    style="color:rgba(255,255,255,0.6)"
                    @paste.prevent="onPaste"
                    @focus="editing = true"
                    @blur="onBlur('reportSubtitle', $event)"
                    @keydown.enter.prevent="($refs.subtitleEl as HTMLElement)?.blur()"
                />
                <h1
                    ref="nameEl"
                    contenteditable="true"
                    spellcheck="false"
                    aria-label="Division name"
                    class="cpb-em-name outline-none"
                    style="color:#ffffff"
                    @paste.prevent="onPaste"
                    @focus="editing = true"
                    @blur="onBlur('divisionName', $event)"
                    @keydown.enter.prevent="($refs.nameEl as HTMLElement)?.blur()"
                />
                <div class="cpb-em-rule" :style="{ background: ac }" />
                <div
                    ref="taglineEl"
                    contenteditable="true"
                    spellcheck="false"
                    aria-label="Tagline"
                    class="cpb-em-tagline outline-none"
                    data-placeholder="A Year in Review"
                    style="color:rgba(255,255,255,0.7)"
                    @paste.prevent="onPaste"
                    @focus="editing = true"
                    @blur="onBlur('tagline', $event)"
                    @keydown.enter.prevent="($refs.taglineEl as HTMLElement)?.blur()"
                />
            </div>

            <!-- Content body below band -->
            <div class="cpb-abs" :style="{ top: `${tpl.layout.accentStripeSize + tpl.layout.bandHeight}px`, left: 0, right: 0, bottom: `${tpl.layout.footerHeight}px`, background: pc, padding: '24px 40px', display: 'flex', flexDirection: 'column', gap: '16px' }">
                <!-- Stats row -->
                <div v-if="content.showStats && visibleStats.length" class="cpb-stats-row cpb-stats-row--light">
                    <div v-for="(stat, i) in visibleStats" :key="i" class="cpb-stat-pill cpb-stat-pill--light" :style="{ borderColor: '#e2e8f0', background: '#f1f5f9' }">
                        <div class="cpb-stat-value" :style="{ color: stat.accent ? ac : tpl.theme.textHeading }">{{ stat.value }}</div>
                        <div class="cpb-stat-label" :style="{ color: tpl.theme.textMuted }">{{ stat.label }}</div>
                    </div>
                </div>

                <!-- Chart + callout row -->
                <div class="cpb-em-split-row">
                    <!-- Chart -->
                    <div class="cpb-chart-area" style="flex:1.6">
                        <template v-if="content.chartImageUrl">
                            <img :src="content.chartImageUrl" class="cpb-chart-img" alt="Chart" />
                        </template>
                        <div v-else class="cpb-img-placeholder cpb-img-placeholder--light">
                            <i class="pi pi-chart-bar" />
                            <span>Chart image URL in panel</span>
                        </div>
                    </div>
                    <!-- Callout -->
                    <div v-if="content.showCallout" class="cpb-callout-box cpb-callout-box--light" :style="{ borderColor: ac, background: '#f8fafc', flex: 1 }">
                        <div
                            ref="calloutTitleEl"
                            contenteditable="true"
                            spellcheck="false"
                            aria-label="Callout title"
                            class="cpb-callout-title outline-none"
                            data-placeholder="Callout headline…"
                            :style="{ color: ac }"
                            @paste.prevent="onPaste"
                            @focus="editing = true"
                            @blur="onBlur('calloutTitle', $event)"
                            @keydown.enter.prevent="($refs.calloutTitleEl as HTMLElement)?.blur()"
                        />
                        <div
                            ref="calloutBodyEl"
                            contenteditable="true"
                            spellcheck="false"
                            aria-label="Callout body"
                            class="cpb-callout-body outline-none"
                            data-placeholder="Supporting detail…"
                            :style="{ color: tpl.theme.textBody }"
                            @paste.prevent="onPaste"
                            @focus="editing = true"
                            @blur="onBlur('calloutBody', $event)"
                        />
                    </div>
                </div>

                <!-- Map placeholder -->
                <div v-if="content.showMap" class="cpb-map-area">
                    <template v-if="content.mapImageUrl">
                        <img :src="content.mapImageUrl" class="cpb-chart-img" alt="Map" />
                    </template>
                    <div v-else class="cpb-img-placeholder cpb-img-placeholder--light">
                        <i class="pi pi-map" />
                        <span>Map image URL in panel</span>
                    </div>
                </div>

                <!-- Locations -->
                <div v-if="content.showLocations" class="cpb-locations-row">
                    <i class="pi pi-map-marker" :style="{ color: ac, flexShrink: 0 }" />
                    <div
                        ref="locationsEl"
                        contenteditable="true"
                        spellcheck="false"
                        aria-label="Locations"
                        class="cpb-locations-text outline-none"
                        data-placeholder="Operations in TX · OK · AR…"
                        :style="{ color: tpl.theme.textBody }"
                        @paste.prevent="onPaste"
                        @focus="editing = true"
                        @blur="onBlur('locationsText', $event)"
                        @keydown.enter.prevent="($refs.locationsEl as HTMLElement)?.blur()"
                    />
                </div>

                <!-- Award -->
                <div v-if="content.showAward" class="cpb-award-box cpb-award-box--light" :style="{ borderColor: ac, background: '#fff' }">
                    <i class="pi pi-star-fill cpb-award-icon" :style="{ color: ac }" />
                    <div>
                        <div
                            ref="awardTitleEl"
                            contenteditable="true"
                            spellcheck="false"
                            aria-label="Award title"
                            class="cpb-award-title outline-none"
                            data-placeholder="Award title…"
                            :style="{ color: tpl.theme.textHeading }"
                            @paste.prevent="onPaste"
                            @focus="editing = true"
                            @blur="onBlur('awardTitle', $event)"
                            @keydown.enter.prevent="($refs.awardTitleEl as HTMLElement)?.blur()"
                        />
                        <div
                            ref="awardDescEl"
                            contenteditable="true"
                            spellcheck="false"
                            aria-label="Award description"
                            class="cpb-award-desc outline-none"
                            data-placeholder="Award description…"
                            :style="{ color: tpl.theme.textBody }"
                            @paste.prevent="onPaste"
                            @focus="editing = true"
                            @blur="onBlur('awardDescription', $event)"
                        />
                    </div>
                </div>
            </div>

            <!-- Footer -->
            <div class="cpb-footer cpb-abs" :style="{ height: `${tpl.layout.footerHeight}px`, bottom: 0, left: 0, right: 0, background: tpl.theme.footerBg }">
                <span :style="{ color: tpl.theme.textMuted }">{{ content.preparedBy }}</span>
                <span :style="{ color: tpl.theme.textMuted }">{{ content.period }}</span>
            </div>
        </template>

        <!-- ── FALLBACK: unknown template id ─────────────────────────────── -->
        <template v-else>
            <div class="cpb-fallback">
                <i class="pi pi-file" style="font-size:2rem;opacity:0.4" />
                <span>Unknown template: {{ content.templateId }}</span>
            </div>
        </template>

    </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch, nextTick } from 'vue';
import { getCoverPageTemplate } from '../../types/cover-template';
import { normalizeCoverPageContent } from '../../utils/normalize-cover-page';
import type { CoverPageContent } from '../../types/report-block';

const props = defineProps<{
    content: CoverPageContent;
}>();

const emit = defineEmits<{
    (e: 'update:content', content: CoverPageContent): void;
}>();

// ── Template lookup ───────────────────────────────────────────────────────────

const tpl = computed(() => getCoverPageTemplate(props.content.templateId));

/** Effective primary background (content override → template default) */
const pc = computed(() => props.content.primaryColor || tpl.value.theme.primaryBg);

/** Effective accent color (content override → template default) */
const ac = computed(() => props.content.accentColor || tpl.value.theme.accent);

// ── Computed caps ─────────────────────────────────────────────────────────────

const MAX_STATS = 6;
const MAX_HIGHLIGHTS = 5;

const visibleStats = computed(() => props.content.stats.slice(0, MAX_STATS));
const visibleHighlights = computed(() => props.content.highlights.slice(0, MAX_HIGHLIGHTS));

// ── Root style ────────────────────────────────────────────────────────────────

const rootStyle = computed(() => ({
    background: pc.value,
    fontFamily: 'inherit',
}));

const heroImgStyle = computed(() => ({
    backgroundImage: `url('${props.content.heroImageUrl}')`,
    backgroundSize: 'cover',
    backgroundPosition: 'center',
}));

// ── Inline editing ────────────────────────────────────────────────────────────

const editing = ref(false);

const nameEl        = ref<HTMLElement | null>(null);
const subtitleEl    = ref<HTMLElement | null>(null);
const taglineEl     = ref<HTMLElement | null>(null);
const summaryEl     = ref<HTMLElement | null>(null);
const locationsEl   = ref<HTMLElement | null>(null);
const calloutTitleEl = ref<HTMLElement | null>(null);
const calloutBodyEl  = ref<HTMLElement | null>(null);
const awardTitleEl   = ref<HTMLElement | null>(null);
const awardDescEl    = ref<HTMLElement | null>(null);

type TextField = 'divisionName' | 'reportSubtitle' | 'tagline' | 'summaryText'
    | 'locationsText' | 'calloutTitle' | 'calloutBody' | 'awardTitle' | 'awardDescription';

const FIELD_REFS: Record<TextField, () => HTMLElement | null> = {
    divisionName:    () => nameEl.value,
    reportSubtitle:  () => subtitleEl.value,
    tagline:         () => taglineEl.value,
    summaryText:     () => summaryEl.value,
    locationsText:   () => locationsEl.value,
    calloutTitle:    () => calloutTitleEl.value,
    calloutBody:     () => calloutBodyEl.value,
    awardTitle:      () => awardTitleEl.value,
    awardDescription: () => awardDescEl.value,
};

function syncEl(el: HTMLElement | null, text: string | undefined) {
    if (!el || document.activeElement === el) return;
    const val = text ?? '';
    if (el.innerText !== val) el.innerText = val;
}

function syncAll() {
    const c = props.content;
    syncEl(nameEl.value,         c.divisionName);
    syncEl(subtitleEl.value,     c.reportSubtitle);
    syncEl(taglineEl.value,      c.tagline);
    syncEl(summaryEl.value,      c.summaryText);
    syncEl(locationsEl.value,    c.locationsText);
    syncEl(calloutTitleEl.value, c.calloutTitle);
    syncEl(calloutBodyEl.value,  c.calloutBody);
    syncEl(awardTitleEl.value,   c.awardTitle);
    syncEl(awardDescEl.value,    c.awardDescription);
}

onMounted(() => {
    // Guard against invalid content arriving from old drafts
    const safe = normalizeCoverPageContent(props.content);
    if (safe.templateId !== props.content.templateId || !props.content.stats) {
        emit('update:content', safe);
    }
    nextTick(syncAll);
    // TODO(telemetry): Emit 'cover_page_block_rendered' with { templateId: props.content.templateId }
    //   to track which templates are actively used in production drafts.
});

watch(
    () => [
        props.content.divisionName, props.content.reportSubtitle, props.content.tagline,
        props.content.summaryText, props.content.locationsText,
        props.content.calloutTitle, props.content.calloutBody,
        props.content.awardTitle, props.content.awardDescription,
        props.content.templateId,
    ],
    () => nextTick(syncAll),
);

// TODO(telemetry): Watch props.content.templateId specifically and emit
//   'cover_page_template_switched' with { from: previousId, to: newId }
//   so we can measure which template users gravitate toward after seeing the picker.

function onBlur(field: TextField, e: FocusEvent) {
    editing.value = false;
    const raw = (e.target as HTMLElement).innerText;
    // Normalize line breaks: collapse to single line for single-line fields, trim
    const isSingleLine = field !== 'summaryText' && field !== 'calloutBody' && field !== 'awardDescription';
    const text = isSingleLine
        ? raw.replace(/[\r\n]+/g, ' ').trim()
        : raw.trim();

    if (text !== (props.content[field] ?? '')) {
        emit('update:content', { ...props.content, [field]: text || undefined });
        // TODO(telemetry): Emit 'cover_page_field_edited' with { field, templateId: props.content.templateId }
        //   to track inline editing engagement. Fire only when value actually changed (already guarded above).
    }
}

/** Sanitize paste to plain text only */
function onPaste(e: ClipboardEvent) {
    e.preventDefault();
    const text = e.clipboardData?.getData('text/plain') ?? '';
    // Use execCommand for broad browser compat within contenteditable
    document.execCommand('insertText', false, text);
}
</script>

<style scoped>
/* ── Root ──────────────────────────────────────────────────────────────── */
.cpb-root {
    position: absolute;
    inset: 0;
    overflow: hidden;
    font-family: inherit;
    font-size: 14px;
    line-height: 1.4;
    user-select: none;
}
.cpb-root [contenteditable] {
    user-select: text;
    cursor: text;
}

/* ── Layout primitives ────────────────────────────────────────────────── */
.cpb-abs {
    position: absolute;
}
.cpb-fill {
    position: absolute;
    inset: 0;
}
.cpb-stripe-left {
    top: 0;
    left: 0;
    bottom: 0;
}

/* ── Sidebar (stronghold-dark) ────────────────────────────────────────── */
.cpb-sidebar {
    display: flex;
    flex-direction: column;
    padding: 28px 20px;
    overflow: hidden;
    flex-shrink: 0;
}
.cpb-logo-ph {
    width: 44px;
    height: 44px;
    border-radius: 8px;
    background: rgba(255,255,255,0.1);
    display: flex;
    align-items: center;
    justify-content: center;
    color: rgba(255,255,255,0.4);
    margin-bottom: 20px;
}
.cpb-sidebar-name {
    font-size: 1.25rem;
    font-weight: 900;
    line-height: 1.15;
    letter-spacing: -0.01em;
    text-transform: uppercase;
    margin: 0 0 4px;
    min-height: 1em;
    overflow: hidden;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
}
.cpb-sidebar-year {
    font-size: 0.75rem;
    font-weight: 600;
    letter-spacing: 0.1em;
    margin-bottom: 12px;
}
.cpb-sidebar-rule {
    height: 2px;
    border-radius: 1px;
    margin-bottom: 16px;
    opacity: 0.6;
}
.cpb-sidebar-summary {
    font-size: 0.72rem;
    line-height: 1.5;
    margin-bottom: 12px;
    min-height: 1.5em;
    max-height: 5.5em;
    overflow: hidden;
}
.cpb-sidebar-summary:empty::before,
.cpb-sidebar-locations:empty::before {
    content: attr(data-placeholder);
    opacity: 0.3;
    font-style: italic;
}
.cpb-sidebar-section {
    margin-bottom: 12px;
}
.cpb-sidebar-section-label {
    font-size: 0.6rem;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    font-weight: 600;
    margin-bottom: 4px;
}
.cpb-sidebar-locations {
    font-size: 0.72rem;
    line-height: 1.5;
    min-height: 1em;
}
.cpb-sidebar-highlights {
    display: flex;
    flex-direction: column;
    gap: 6px;
    margin-top: auto;
}
.cpb-highlight-row {
    display: flex;
    align-items: flex-start;
    gap: 6px;
    font-size: 0.72rem;
    line-height: 1.4;
}

/* ── Main content ─────────────────────────────────────────────────────── */
.cpb-main {
    overflow: hidden;
}
.cpb-band {
    overflow: hidden;
    flex-shrink: 0;
}
.cpb-band-content {
    position: relative;
    z-index: 2;
}
.cpb-band-subtitle {
    font-size: 0.72rem;
    font-weight: 600;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    margin-bottom: 8px;
    min-height: 1em;
}
.cpb-band-subtitle:empty::before,
.cpb-band-tagline:empty::before,
.cpb-em-subtitle:empty::before,
.cpb-em-tagline:empty::before,
.cpb-sr-subtitle:empty::before,
.cpb-sr-tagline:empty::before {
    content: attr(data-placeholder);
    opacity: 0.3;
    font-style: italic;
}
.cpb-band-tagline {
    font-size: 1.1rem;
    font-style: italic;
    font-weight: 300;
    min-height: 1em;
}
.cpb-below-band {
    overflow: hidden;
}

/* ── Safety Red variants ──────────────────────────────────────────────── */
.cpb-sr-subtitle {
    font-size: 0.72rem;
    font-weight: 600;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    margin-bottom: 8px;
    min-height: 1em;
}
.cpb-sr-name {
    font-size: 2.4rem;
    font-weight: 900;
    line-height: 1.05;
    letter-spacing: -0.02em;
    text-transform: uppercase;
    margin: 0 0 12px;
    min-height: 1em;
    overflow: hidden;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
}
.cpb-sr-tagline {
    font-size: 1rem;
    font-style: italic;
    min-height: 1em;
}

/* ── Executive Minimal variants ───────────────────────────────────────── */
.cpb-em-subtitle {
    font-size: 0.72rem;
    font-weight: 600;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    margin-bottom: 12px;
    min-height: 1em;
    text-align: center;
}
.cpb-em-name {
    font-size: 2.2rem;
    font-weight: 900;
    line-height: 1.05;
    letter-spacing: -0.02em;
    text-transform: uppercase;
    text-align: center;
    margin: 0 0 16px;
    min-height: 1em;
    overflow: hidden;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
}
.cpb-em-rule {
    width: 64px;
    height: 3px;
    border-radius: 2px;
    margin: 0 auto 16px;
}
.cpb-em-tagline {
    font-size: 0.95rem;
    font-style: italic;
    min-height: 1em;
    text-align: center;
}
.cpb-em-split-row {
    display: flex;
    gap: 16px;
    min-height: 140px;
}

/* ── Stats ────────────────────────────────────────────────────────────── */
.cpb-stats-row {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
}
.cpb-stat-pill {
    flex: 1;
    min-width: 80px;
    max-width: 140px;
    border: 1px solid;
    border-radius: 8px;
    padding: 10px 12px;
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
}
.cpb-stat-value {
    font-size: 1.4rem;
    font-weight: 800;
    line-height: 1;
    letter-spacing: -0.02em;
    margin-bottom: 4px;
}
.cpb-stat-label {
    font-size: 0.65rem;
    font-weight: 500;
    text-transform: uppercase;
    letter-spacing: 0.06em;
}

/* ── Callout ──────────────────────────────────────────────────────────── */
.cpb-callout-box {
    border-left: 3px solid;
    padding: 12px 16px;
    border-radius: 0 6px 6px 0;
}
.cpb-callout-title {
    font-size: 0.8rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    margin-bottom: 6px;
    min-height: 1em;
}
.cpb-callout-title:empty::before,
.cpb-callout-body:empty::before {
    content: attr(data-placeholder);
    opacity: 0.3;
    font-style: italic;
    font-weight: 400;
    text-transform: none;
    letter-spacing: 0;
}
.cpb-callout-body {
    font-size: 0.78rem;
    line-height: 1.5;
    min-height: 1.5em;
}

/* ── Chart area ───────────────────────────────────────────────────────── */
.cpb-chart-area {
    min-height: 80px;
    border-radius: 6px;
    overflow: hidden;
    display: flex;
    align-items: stretch;
}
.cpb-chart-img {
    width: 100%;
    height: 100%;
    object-fit: contain;
}
.cpb-map-area {
    height: 100px;
    border-radius: 6px;
    overflow: hidden;
    display: flex;
    align-items: stretch;
}

/* ── Image placeholders ───────────────────────────────────────────────── */
.cpb-img-placeholder {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 6px;
    background: rgba(255,255,255,0.05);
    border: 1px dashed rgba(255,255,255,0.18);
    border-radius: 6px;
    padding: 16px;
    color: rgba(255,255,255,0.3);
    font-size: 0.7rem;
    min-height: 60px;
}
.cpb-img-placeholder--light {
    background: #f1f5f9;
    border-color: #cbd5e1;
    color: #94a3b8;
}
.cpb-hero-ph {
    position: absolute;
    inset: 0;
    border-radius: 0;
}

/* ── Award ────────────────────────────────────────────────────────────── */
.cpb-award-box {
    display: flex;
    align-items: flex-start;
    gap: 12px;
    border: 1px solid;
    border-radius: 8px;
    padding: 12px 16px;
}
.cpb-award-icon {
    font-size: 1.1rem;
    flex-shrink: 0;
    margin-top: 2px;
}
.cpb-award-title {
    font-size: 0.82rem;
    font-weight: 700;
    margin-bottom: 4px;
    min-height: 1em;
}
.cpb-award-title:empty::before,
.cpb-award-desc:empty::before {
    content: attr(data-placeholder);
    opacity: 0.3;
    font-style: italic;
    font-weight: 400;
}
.cpb-award-desc {
    font-size: 0.75rem;
    line-height: 1.5;
    min-height: 1em;
}

/* ── Locations row ────────────────────────────────────────────────────── */
.cpb-locations-row {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    font-size: 0.78rem;
}
.cpb-locations-text {
    min-height: 1em;
    flex: 1;
}
.cpb-locations-text:empty::before {
    content: attr(data-placeholder);
    opacity: 0.3;
    font-style: italic;
}

/* ── Footer ───────────────────────────────────────────────────────────── */
.cpb-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 24px;
    font-size: 0.7rem;
    letter-spacing: 0.04em;
}

/* ── Fallback ─────────────────────────────────────────────────────────── */
.cpb-fallback {
    position: absolute;
    inset: 0;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 12px;
    color: rgba(255,255,255,0.3);
    font-size: 0.8rem;
}
</style>

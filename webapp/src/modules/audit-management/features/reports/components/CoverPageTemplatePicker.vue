<template>
    <Teleport to="body">
        <div v-if="modelValue" class="fixed inset-0 z-[9999] flex items-center justify-center p-6" @keydown.escape="close">
            <!-- Backdrop -->
            <div class="absolute inset-0 bg-black/70 backdrop-blur-sm" @click="close" />

            <!-- Modal -->
            <div
                class="relative bg-[#0f1623] border border-slate-700 rounded-xl shadow-2xl w-full max-w-3xl max-h-[88vh] flex flex-col"
                role="dialog"
                aria-modal="true"
                aria-label="Choose a cover page template"
            >
                <!-- Header -->
                <div class="flex items-center justify-between px-6 py-4 border-b border-slate-700 shrink-0">
                    <div>
                        <h2 class="text-white font-semibold text-lg">Full Cover Page Templates</h2>
                        <p class="text-slate-400 text-xs mt-0.5">Choose a template — your text and stats are preserved.</p>
                    </div>
                    <button
                        @click="close"
                        class="text-slate-400 hover:text-white transition-colors p-1 rounded focus-visible:ring-2 focus-visible:ring-blue-400"
                        aria-label="Close template picker"
                    >
                        <i class="pi pi-times text-lg" />
                    </button>
                </div>

                <!-- Grid -->
                <div class="overflow-y-auto p-6 grid grid-cols-3 gap-5">
                    <button
                        v-for="t in COVER_PAGE_TEMPLATES"
                        :key="t.id"
                        @click="applyTemplate(t.id)"
                        @keydown.enter.prevent="applyTemplate(t.id)"
                        @keydown.space.prevent="applyTemplate(t.id)"
                        class="group flex flex-col rounded-lg overflow-hidden border-2 transition-all text-left focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-400"
                        :class="isActive(t.id)
                            ? 'border-blue-500 ring-1 ring-blue-500/40'
                            : 'border-slate-700 hover:border-slate-400'"
                        :aria-pressed="isActive(t.id)"
                        :aria-label="`${t.name}: ${t.description}`"
                    >
                        <!-- Thumbnail viewport — clips the scaled-down full-page render -->
                        <div class="relative shrink-0 overflow-hidden" style="height:206px;width:100%">
                            <!-- Scaled canvas — inner 794×1123 scaled to ~0.26× -->
                            <div class="cptp-thumb-inner" :style="thumbInnerStyle(t)">

                                <!-- Stronghold Dark thumbnail -->
                                <template v-if="t.id === 'stronghold-dark'">
                                    <div class="cptp-abs" :style="{ left: 0, top: 0, bottom: 0, width: '6px', background: t.theme.accent }" />
                                    <div class="cptp-abs" :style="{ left: '6px', top: 0, bottom: 0, width: '200px', background: t.theme.sidebarBg, padding: '28px 20px' }">
                                        <div style="width:36px;height:36px;border-radius:6px;background:rgba(255,255,255,0.08);margin-bottom:16px" />
                                        <div :style="{ color: t.theme.accent, fontWeight: 900, fontSize: '18px', textTransform: 'uppercase', lineHeight: 1.2, marginBottom: '6px' }">DIVISION<br>NAME</div>
                                        <div :style="{ color: t.theme.textMuted, fontSize: '10px', letterSpacing: '0.1em', marginBottom: '12px' }">2025</div>
                                        <div :style="{ height: '2px', background: t.theme.accent, opacity: 0.5, marginBottom: '14px' }" />
                                        <div :style="{ color: t.theme.textBody, fontSize: '10px', lineHeight: 1.6 }">Compliance report summary text here.</div>
                                    </div>
                                    <div class="cptp-abs" :style="{ left: '206px', top: 0, right: 0, height: '292px', background: t.theme.bandBg, padding: '28px 24px' }">
                                        <div :style="{ color: t.theme.textMuted, fontSize: '10px', textTransform: 'uppercase', letterSpacing: '0.12em', marginBottom: '8px' }">Annual Review</div>
                                        <div :style="{ color: 'rgba(255,255,255,0.85)', fontSize: '14px', fontStyle: 'italic' }">A Year in Review</div>
                                    </div>
                                    <div class="cptp-abs" :style="{ left: '206px', top: '292px', right: 0, bottom: '48px', background: t.theme.primaryBg, padding: '16px 20px' }">
                                        <div style="display:flex;gap:8px;margin-bottom:10px">
                                            <div v-for="n in 3" :key="n" :style="{ flex: 1, height: '44px', borderRadius: '6px', background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.1)' }" />
                                        </div>
                                        <div :style="{ height: '56px', borderRadius: '6px', borderLeft: `3px solid ${t.theme.accent}`, background: 'rgba(255,255,255,0.04)', padding: '8px 12px' }">
                                            <div :style="{ height: '8px', background: t.theme.accent, borderRadius: '4px', width: '60%', marginBottom: '6px' }" />
                                            <div :style="{ height: '6px', background: 'rgba(255,255,255,0.15)', borderRadius: '3px', width: '90%' }" />
                                        </div>
                                    </div>
                                    <div class="cptp-abs" :style="{ left: '206px', bottom: 0, right: 0, height: '48px', background: t.theme.footerBg, padding: '0 20px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }">
                                        <div :style="{ color: t.theme.textMuted, fontSize: '9px' }">Prepared by Safety Team</div>
                                        <div :style="{ color: t.theme.textMuted, fontSize: '9px' }">January 2025</div>
                                    </div>
                                </template>

                                <!-- Safety Red thumbnail -->
                                <template v-else-if="t.id === 'safety-red'">
                                    <div class="cptp-abs" :style="{ top: 0, left: 0, right: 0, height: '264px', background: t.theme.bandBg, padding: '32px 40px', display: 'flex', flexDirection: 'column', justifyContent: 'flex-end' }">
                                        <div :style="{ color: t.theme.textMuted, fontSize: '10px', textTransform: 'uppercase', letterSpacing: '0.12em', marginBottom: '8px' }">Safety Compliance</div>
                                        <div :style="{ color: t.theme.accent, fontWeight: 900, fontSize: '28px', textTransform: 'uppercase', lineHeight: 1.05, marginBottom: '10px' }">DIVISION</div>
                                        <div :style="{ color: 'rgba(255,255,255,0.75)', fontSize: '12px', fontStyle: 'italic' }">A Year in Review</div>
                                    </div>
                                    <div class="cptp-abs" :style="{ top: '264px', left: 0, right: 0, bottom: '48px', background: t.theme.primaryBg, padding: '20px 32px', display: 'flex', flexDirection: 'column', gap: '10px' }">
                                        <div style="display:flex;gap:8px">
                                            <div v-for="n in 3" :key="n" :style="{ flex: 1, height: '44px', borderRadius: '6px', background: 'rgba(255,255,255,0.05)', border: '1px solid rgba(255,255,255,0.08)' }" />
                                        </div>
                                        <div :style="{ flex: 1, borderRadius: '6px', background: 'rgba(255,255,255,0.04)', minHeight: '40px' }" />
                                    </div>
                                    <div class="cptp-abs" :style="{ bottom: 0, left: 0, right: 0, height: '48px', background: t.theme.footerBg, padding: '0 24px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }">
                                        <div :style="{ color: t.theme.textMuted, fontSize: '9px' }">Prepared by Safety Team</div>
                                        <div :style="{ color: t.theme.textMuted, fontSize: '9px' }">January 2025</div>
                                    </div>
                                </template>

                                <!-- Executive Minimal thumbnail -->
                                <template v-else-if="t.id === 'executive-minimal'">
                                    <div class="cptp-abs" :style="{ top: 0, left: 0, right: 0, height: '6px', background: t.theme.accent }" />
                                    <div class="cptp-abs" :style="{ top: '6px', left: 0, right: 0, height: '316px', background: t.theme.bandBg, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', padding: '32px 40px' }">
                                        <div style="color:rgba(255,255,255,0.6);font-size:10px;text-transform:uppercase;letter-spacing:0.15em;margin-bottom:10px">2025 Compliance</div>
                                        <div :style="{ color: '#fff', fontWeight: 900, fontSize: '26px', textTransform: 'uppercase', textAlign: 'center', marginBottom: '12px', lineHeight: 1.1 }">DIVISION NAME</div>
                                        <div :style="{ width: '48px', height: '3px', background: t.theme.accent, borderRadius: '2px', marginBottom: '10px' }" />
                                        <div style="color:rgba(255,255,255,0.7);font-size:11px;font-style:italic">A Year in Review</div>
                                    </div>
                                    <div class="cptp-abs" :style="{ top: `${6 + 316}px`, left: 0, right: 0, bottom: '56px', background: t.theme.primaryBg, padding: '20px 32px', display: 'flex', flexDirection: 'column', gap: '10px' }">
                                        <div style="display:flex;gap:8px">
                                            <div v-for="n in 3" :key="n" :style="{ flex: 1, height: '44px', borderRadius: '6px', background: '#f1f5f9', border: '1px solid #e2e8f0' }" />
                                        </div>
                                        <div style="display:flex;gap:8px;flex:1;min-height:40px">
                                            <div :style="{ flex: 1.6, borderRadius: '6px', background: '#f1f5f9', border: '1px solid #e2e8f0' }" />
                                            <div :style="{ flex: 1, borderRadius: '6px', borderLeft: `3px solid ${t.theme.accent}`, background: '#f8fafc', padding: '8px' }">
                                                <div :style="{ height: '7px', background: t.theme.accent, borderRadius: '3px', width: '70%', marginBottom: '5px' }" />
                                                <div style="height:5px;background:#e2e8f0;border-radius:3px;width:90%" />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="cptp-abs" :style="{ bottom: 0, left: 0, right: 0, height: '56px', background: t.theme.footerBg, padding: '0 28px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }">
                                        <div :style="{ color: t.theme.textMuted, fontSize: '9px' }">Prepared by Safety Team</div>
                                        <div :style="{ color: t.theme.textMuted, fontSize: '9px' }">January 2025</div>
                                    </div>
                                </template>

                            </div>

                            <!-- Active checkmark overlay -->
                            <div v-if="isActive(t.id)"
                                class="absolute top-2 right-2 w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center shadow z-10">
                                <i class="pi pi-check text-white text-[11px]" />
                            </div>
                        </div>

                        <!-- Card label -->
                        <div class="bg-slate-800 px-4 py-3 border-t border-slate-700 group-hover:bg-slate-750 transition-colors">
                            <div class="text-white text-sm font-semibold leading-tight">{{ t.name }}</div>
                            <div class="text-slate-400 text-xs mt-0.5 leading-tight">{{ t.description }}</div>
                        </div>
                    </button>
                </div>
            </div>
        </div>
    </Teleport>
</template>

<script setup lang="ts">
import { COVER_PAGE_TEMPLATES } from '../types/cover-template';
import type { CoverTemplateDefinition } from '../types/cover-template';

const props = defineProps<{
    modelValue: boolean;
    currentTemplateId: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', v: boolean): void;
    (e: 'apply', templateId: string): void;
}>();

/** Scale factor: thumbnail width ÷ full page width.
 *  The card is ~206px wide at max-w-3xl/3-col; full page is 794px.
 *  scale ≈ 206/794 ≈ 0.259
 */
const THUMB_SCALE = 206 / 794;

function thumbInnerStyle(t: CoverTemplateDefinition) {
    return {
        position: 'absolute' as const,
        top: 0,
        left: 0,
        width: '794px',
        height: '1123px',
        transform: `scale(${THUMB_SCALE})`,
        transformOrigin: 'top left',
        background: t.theme.primaryBg,
        overflow: 'hidden',
    };
}

function isActive(id: string): boolean {
    return props.currentTemplateId === id;
}

function applyTemplate(id: string) {
    emit('apply', id);
    emit('update:modelValue', false);
}

function close() {
    emit('update:modelValue', false);
}
</script>

<style scoped>
.cptp-abs {
    position: absolute;
}
/* Prevent text selection in thumbnails */
.cptp-thumb-inner * {
    user-select: none;
    pointer-events: none;
}
</style>

<template>
    <Teleport to="body">
        <div v-if="modelValue" class="fixed inset-0 z-[9999] flex items-center justify-center p-6">
            <!-- Backdrop -->
            <div class="absolute inset-0 bg-black/70 backdrop-blur-sm" @click="$emit('update:modelValue', false)" />

            <!-- Modal -->
            <div class="relative bg-[#0f1623] border border-slate-700 rounded-xl shadow-2xl w-full max-w-4xl max-h-[88vh] flex flex-col">

                <!-- Header -->
                <div class="flex items-center justify-between px-6 py-4 border-b border-slate-700 shrink-0">
                    <div>
                        <h2 class="text-white font-semibold text-lg">Cover Templates</h2>
                        <p class="text-slate-400 text-xs mt-0.5">Click a template to apply it. Your title, tagline, and text are preserved.</p>
                    </div>
                    <button @click="$emit('update:modelValue', false)"
                        class="text-slate-400 hover:text-white transition-colors p-1">
                        <i class="pi pi-times text-lg" />
                    </button>
                </div>

                <!-- Grid -->
                <div class="overflow-y-auto p-6 grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
                    <button
                        v-for="t in TEMPLATES"
                        :key="t.id"
                        @click="applyTemplate(t)"
                        class="group flex flex-col rounded-lg overflow-hidden border-2 transition-all text-left focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-400"
                        :class="isActive(t) ? 'border-blue-500 ring-1 ring-blue-500/40' : 'border-slate-700 hover:border-slate-400'"
                    >
                        <!-- Thumbnail -->
                        <div class="relative h-28 w-full overflow-hidden shrink-0" :style="thumbBg(t)">
                            <!-- Overlay -->
                            <div v-if="t.preset.layoutVariant !== 'minimal'" class="absolute inset-0"
                                :style="{ background: `rgba(0,0,0,${(t.preset.overlayOpacity ?? 30) / 100})` }" />

                            <!-- classic / top-left layouts -->
                            <div v-if="t.preset.layoutVariant === 'classic'" class="absolute bottom-0 left-0 right-0 p-3">
                                <div class="h-px w-8 mb-1.5" :style="{ background: 'rgba(255,255,255,0.4)' }" />
                                <div class="font-black text-sm leading-tight tracking-tight truncate"
                                    :style="{ color: t.preset.nameAccentColor || '#f59e0b', textTransform: t.preset.nameTransform || 'uppercase' }">
                                    DIVISION NAME
                                </div>
                                <div class="h-px w-full mt-1.5" :style="{ background: 'rgba(255,255,255,0.4)' }" />
                                <div class="text-white/40 text-[8px] mt-1">2025 · Annual Review</div>
                            </div>

                            <div v-else-if="t.preset.layoutVariant === 'top-left'" class="absolute top-0 left-0 right-0 p-3">
                                <div class="text-white/50 text-[7px] tracking-widest uppercase mb-1">2025 COMPLIANCE</div>
                                <div class="font-black text-sm leading-tight tracking-tight truncate"
                                    :style="{ color: t.preset.nameAccentColor || '#f59e0b', textTransform: t.preset.nameTransform || 'uppercase' }">
                                    DIVISION NAME
                                </div>
                                <div class="h-px w-10 mt-2" :style="{ background: 'rgba(255,255,255,0.4)' }" />
                                <div class="text-white/50 text-[8px] italic mt-1.5">A Year in Review</div>
                            </div>

                            <div v-else-if="t.preset.layoutVariant === 'centered'"
                                class="absolute inset-0 flex flex-col items-center justify-center text-center p-3">
                                <div class="text-white/50 text-[7px] tracking-widest uppercase mb-1">2025 COMPLIANCE</div>
                                <div class="font-black text-sm leading-tight"
                                    :style="{ color: t.preset.nameAccentColor || '#f59e0b', textTransform: t.preset.nameTransform || 'uppercase' }">
                                    DIVISION
                                </div>
                                <div class="h-px w-8 mt-1.5 mx-auto" :style="{ background: t.preset.nameAccentColor || '#f59e0b' }" />
                                <div class="text-white/50 text-[7px] italic mt-1">A Year in Review</div>
                            </div>

                            <div v-else-if="t.preset.layoutVariant === 'stripe-side'" class="absolute inset-0 flex">
                                <div class="w-3 h-full shrink-0" :style="{ background: t.preset.nameAccentColor || '#f59e0b' }" />
                                <div class="flex flex-col justify-end p-3 flex-1">
                                    <div class="font-black text-sm leading-tight truncate"
                                        :style="{ color: t.preset.nameAccentColor || '#f59e0b', textTransform: t.preset.nameTransform || 'uppercase' }">
                                        DIVISION
                                    </div>
                                    <div class="h-px w-full mt-1" :style="{ background: 'rgba(255,255,255,0.35)' }" />
                                    <div class="text-white/40 text-[8px] mt-1">A Year in Review</div>
                                </div>
                            </div>

                            <div v-else-if="t.preset.layoutVariant === 'minimal'"
                                class="absolute inset-0 flex flex-col justify-center p-3">
                                <div class="text-[7px] tracking-widest uppercase font-semibold mb-1.5"
                                    :style="{ color: t.preset.nameAccentColor || '#1e3a5f' }">2025 COMPLIANCE</div>
                                <div class="font-black text-sm leading-tight"
                                    :style="{ color: isLightHex(t.preset.primaryColor || '#f1f5f9') ? '#0f172a' : '#f8fafc', textTransform: t.preset.nameTransform || 'uppercase' }">
                                    DIVISION NAME
                                </div>
                                <div class="h-1 w-6 rounded mt-1.5" :style="{ background: t.preset.nameAccentColor || '#1e3a5f' }" />
                                <div class="text-slate-400 text-[7px] italic mt-1">A Year in Review</div>
                            </div>

                            <div v-else-if="t.preset.layoutVariant === 'bold-full'"
                                class="absolute inset-0 flex flex-col items-center justify-center text-center p-3">
                                <div class="text-white/40 text-[6px] tracking-[0.2em] uppercase mb-2">2025 COMPLIANCE</div>
                                <div class="font-black leading-none"
                                    :style="{ color: t.preset.nameAccentColor || '#f59e0b', fontSize: '1.35rem', textTransform: t.preset.nameTransform || 'uppercase', letterSpacing: '-0.02em' }">
                                    DIV
                                </div>
                                <div class="text-white/40 text-[7px] italic mt-2">A Year in Review</div>
                            </div>

                            <!-- Active check -->
                            <div v-if="isActive(t)"
                                class="absolute top-2 right-2 w-5 h-5 rounded-full bg-blue-500 flex items-center justify-center">
                                <i class="pi pi-check text-white text-[10px]" />
                            </div>
                        </div>

                        <!-- Label -->
                        <div class="bg-slate-800 group-hover:bg-slate-750 px-3 py-2 border-t border-slate-700 transition-colors">
                            <div class="text-white text-xs font-semibold leading-tight">{{ t.name }}</div>
                            <div class="text-slate-400 text-[10px] mt-0.5 leading-tight">{{ t.description }}</div>
                        </div>
                    </button>
                </div>

            </div>
        </div>
    </Teleport>
</template>

<script setup lang="ts">
import type { CoverContent } from '../types/report-block';

type CoverPreset = Partial<CoverContent> & { layoutVariant: CoverContent['layoutVariant'] };

interface CoverTemplate {
    id: string;
    name: string;
    description: string;
    preset: CoverPreset;
}

const props = defineProps<{
    modelValue: boolean;
    currentContent: CoverContent;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', v: boolean): void;
    (e: 'apply', preset: CoverPreset): void;
}>();

const TEMPLATES: CoverTemplate[] = [
    {
        id: 'stronghold-dark',
        name: 'Stronghold Dark',
        description: 'Navy bg · amber title · decorative rules',
        preset: {
            layoutVariant: 'classic',
            primaryColor: '#1e3a5f',
            nameAccentColor: '#f59e0b',
            overlayOpacity: 20,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: true,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'corporate-charcoal',
        name: 'Corporate Charcoal',
        description: 'Dark charcoal · gold accent · rules',
        preset: {
            layoutVariant: 'classic',
            primaryColor: '#18181b',
            nameAccentColor: '#d4af37',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: true,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'annual-review-top',
        name: 'Annual Review',
        description: 'Top-left layout · white title · clean',
        preset: {
            layoutVariant: 'top-left',
            primaryColor: '#0f172a',
            nameAccentColor: '#ffffff',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: true,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'safety-red',
        name: 'Safety Red',
        description: 'Top-left · bold red · high visibility',
        preset: {
            layoutVariant: 'top-left',
            primaryColor: '#450a0a',
            nameAccentColor: '#fca5a5',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: true,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'centered-executive',
        name: 'Executive Center',
        description: 'Centered layout · dark slate · amber',
        preset: {
            layoutVariant: 'centered',
            primaryColor: '#1e293b',
            nameAccentColor: '#fbbf24',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: false,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'centered-forest',
        name: 'Forest',
        description: 'Centered · deep green · cream title',
        preset: {
            layoutVariant: 'centered',
            primaryColor: '#052e16',
            nameAccentColor: '#d1fae5',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: false,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'stripe-navy',
        name: 'Corporate Stripe',
        description: 'Left amber stripe · navy bg',
        preset: {
            layoutVariant: 'stripe-side',
            primaryColor: '#1a365d',
            nameAccentColor: '#f59e0b',
            overlayOpacity: 10,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: false,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'stripe-crimson',
        name: 'Crimson Stripe',
        description: 'Left red stripe · dark bg · white title',
        preset: {
            layoutVariant: 'stripe-side',
            primaryColor: '#1c1c2e',
            nameAccentColor: '#ef4444',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: false,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'minimal-white',
        name: 'Minimal White',
        description: 'Light background · clean lines',
        preset: {
            layoutVariant: 'minimal',
            primaryColor: '#f8fafc',
            nameAccentColor: '#1e3a5f',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: false,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'minimal-cream',
        name: 'Warm Cream',
        description: 'Cream background · dark navy title',
        preset: {
            layoutVariant: 'minimal',
            primaryColor: '#fef9f0',
            nameAccentColor: '#92400e',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: false,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'bold-dark',
        name: 'Bold Dark',
        description: 'Maximum impact · oversized title',
        preset: {
            layoutVariant: 'bold-full',
            primaryColor: '#09090b',
            nameAccentColor: '#f59e0b',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: false,
            nameTransform: 'uppercase',
        },
    },
    {
        id: 'bold-blue',
        name: 'Bold Blue',
        description: 'Royal blue · white title · bold',
        preset: {
            layoutVariant: 'bold-full',
            primaryColor: '#1d4ed8',
            nameAccentColor: '#ffffff',
            overlayOpacity: 0,
            nameSize: 'xl',
            coverHeight: 'lg',
            showDecorativeRules: false,
            nameTransform: 'uppercase',
        },
    },
];

function thumbBg(t: CoverTemplate) {
    const color = t.preset.primaryColor ?? '#1e3a5f';
    return { background: color };
}

function isActive(t: CoverTemplate): boolean {
    return props.currentContent.layoutVariant === t.preset.layoutVariant
        && props.currentContent.primaryColor === t.preset.primaryColor
        && props.currentContent.nameAccentColor === t.preset.nameAccentColor;
}

function isLightHex(hex: string): boolean {
    const h = (hex || '#f1f5f9').replace('#', '');
    if (h.length < 6) return true;
    const r = parseInt(h.slice(0, 2), 16);
    const g = parseInt(h.slice(2, 4), 16);
    const b = parseInt(h.slice(4, 6), 16);
    return (r * 299 + g * 587 + b * 114) / 1000 > 160;
}

function applyTemplate(t: CoverTemplate) {
    emit('apply', t.preset);
    emit('update:modelValue', false);
}
</script>

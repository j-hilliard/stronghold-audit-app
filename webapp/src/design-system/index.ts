// Design system public API
// Import from here, not from individual files

export {
    tokens,
    surface, border, text, status, chart, font, spacing, radius, shadow, transition,
    kpiVariantToSemantic, categoryVariantToSemantic, makeChartDefaults,
    type SemanticVariant, type KpiVariant, type CategoryVariant,
} from './tokens';

// Re-export shared UI components so feature code imports from one place
export { default as MetricCard }    from '@/components/ui/MetricCard.vue';
export { default as StatusBadge }   from '@/components/ui/StatusBadge.vue';
export { default as SectionPanel }  from '@/components/ui/SectionPanel.vue';
export { default as AlertBanner }   from '@/components/ui/AlertBanner.vue';
export { default as ActionBar }     from '@/components/ui/ActionBar.vue';
export { default as EmptyState }    from '@/components/ui/EmptyState.vue';
export { default as FilterBar }     from '@/components/ui/FilterBar.vue';

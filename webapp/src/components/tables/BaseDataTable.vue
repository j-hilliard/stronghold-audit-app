<template>
  <div class="mt-4 rounded-card border border-slate-700/60 bg-surface-2 shadow-elevation-1 overflow-hidden">
    <!-- Loading bar -->
    <div class="base-table-loadbar" :class="{ 'base-table-loadbar--active': loading }">
      <div class="base-table-loadbar-fill" />
    </div>

    <!-- Filters slot -->
    <div v-if="$slots.filters" class="flex flex-wrap items-end gap-2 px-4 py-3 border-b border-slate-700/40">
      <slot name="filters" />
    </div>

    <DataTable
      v-bind="$attrs"
      :value="value"
      :loading="loading"
      :emptyMessage="emptyMessage"
      class="stronghold-table"
    >
      <slot />
    </DataTable>
  </div>
</template>

<script setup lang="ts">
defineOptions({ inheritAttrs: false });

defineProps<{
  value: any[];
  emptyMessage?: string;
  loading?: boolean;
}>();
</script>

<style scoped>
/* ── Loading bar ─────────────────────────────────────────────────────────────── */
.base-table-loadbar {
  height: 2px;
  overflow: hidden;
  opacity: 0;
  transition: opacity 0.2s ease;
}
.base-table-loadbar--active {
  opacity: 1;
}
.base-table-loadbar-fill {
  height: 100%;
  width: 40%;
  background: linear-gradient(90deg, transparent, #63b3ed, #3b82f6, transparent);
  animation: loadbar-slide 1.2s ease-in-out infinite;
}
@keyframes loadbar-slide {
  0%   { transform: translateX(-150%); }
  100% { transform: translateX(350%); }
}
</style>


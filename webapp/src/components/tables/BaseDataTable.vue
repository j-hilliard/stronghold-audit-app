<template>
  <div class="mt-4 rounded-card border border-border bg-surface-2 shadow-elevation-1 overflow-hidden">
    <!-- Loading bar -->
    <div class="base-table-loadbar" :class="{ 'base-table-loadbar--active': loading }">
      <div class="base-table-loadbar-fill" />
    </div>

    <!-- Filters slot -->
    <div v-if="$slots.filters" class="flex flex-wrap items-end gap-2 px-4 py-3 border-b border-border-subtle">
      <slot name="filters" />
    </div>

    <DataTable
      v-bind="$attrs"
      :value="value"
      :loading="loading"
      :emptyMessage="$slots.empty ? undefined : emptyMessage"
      class="stronghold-table"
    >
      <!-- Named empty state slot — override default empty message with rich content -->
      <template v-if="$slots.empty" #empty>
        <slot name="empty" />
      </template>

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
  transition: opacity var(--transition-base);
}
.base-table-loadbar--active {
  opacity: 1;
}
.base-table-loadbar-fill {
  height: 100%;
  width: 40%;
  background: linear-gradient(90deg, transparent, var(--color-info), transparent);
  animation: loadbar-slide 1.2s ease-in-out infinite;
}
@keyframes loadbar-slide {
  0%   { transform: translateX(-150%); }
  100% { transform: translateX(350%); }
}
</style>

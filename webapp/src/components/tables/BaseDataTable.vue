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

<style>
/* ── PrimeVue DataTable overrides — not scoped so they reach the shadow DOM ─── */
.stronghold-table .p-datatable-thead > tr > th {
  background: rgb(15, 23, 42) !important;
  border-color: rgba(51, 65, 85, 0.6) !important;
  color: #94a3b8 !important;
  font-size: 0.72rem !important;
  font-weight: 600 !important;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  padding: 0.6rem 1rem !important;
}

.stronghold-table .p-datatable-tbody > tr {
  background: transparent !important;
  transition: background 0.12s ease;
  border-color: rgba(51, 65, 85, 0.4) !important;
}

.stronghold-table .p-datatable-tbody > tr:nth-child(even) {
  background: rgba(15, 23, 42, 0.35) !important;
}

.stronghold-table .p-datatable-tbody > tr:hover {
  background: rgba(59, 130, 246, 0.07) !important;
  cursor: pointer;
}

.stronghold-table .p-datatable-tbody > tr > td {
  padding: 0.65rem 1rem !important;
  border-color: rgba(51, 65, 85, 0.35) !important;
  font-size: 0.85rem;
}

.stronghold-table .p-datatable-emptymessage td {
  text-align: center !important;
  color: #475569 !important;
  padding: 3rem 1rem !important;
}
</style>

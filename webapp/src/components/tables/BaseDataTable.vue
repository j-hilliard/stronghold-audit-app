<template>
  <Card class="mt-6">
    <template #content>
      <!-- Top loading bar -->
      <div class="base-table-loadbar" :class="{ 'base-table-loadbar--active': loading }">
        <div class="base-table-loadbar-fill" />
      </div>

      <div v-if="$slots.filters" class="flex flex-col md:flex-row md:items-center gap-4 mb-4">
        <slot name="filters" />
      </div>
      <DataTable
        v-bind="$attrs"
        :value="value"
        :loading="loading"
        :emptyMessage="emptyMessage"
        class="bg-navy-800/50 border border-slate-700 border-round-3xl"
      >
        <slot />
      </DataTable>
    </template>
  </Card>
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
.base-table-loadbar {
  height: 3px;
  margin: -1rem -1rem 0.75rem -1rem;
  border-radius: 2px 2px 0 0;
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

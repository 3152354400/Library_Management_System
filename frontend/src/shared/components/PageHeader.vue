<template>
  <div class="page-header">
    <div class="header-left">
      <h1 v-if="title" class="title">{{ title }}</h1>
      <p v-if="subtitle" class="subtitle">{{ subtitle }}</p>
    </div>
    <div class="header-right">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' }
})
</script>

<style scoped>
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: var(--space-6);
  gap: var(--space-4);
}
.title {
  font-size: var(--fs-2xl);
  font-weight: 600;
  color: var(--color-text);
  margin: 0;
}
.subtitle {
  margin: 6px 0 0;
  color: var(--color-text-soft);
  font-size: var(--fs-base);
}
.header-right {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  flex-shrink: 0;
}
@media (max-width: 768px) {
  .page-header { flex-direction: column; }
  .header-right { width: 100%; flex-wrap: wrap; }
}
</style>
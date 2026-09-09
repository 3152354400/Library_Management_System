<template>
  <div class="empty-state">
    <div class="empty-art">
      <svg viewBox="0 0 200 200" width="160" height="160" aria-hidden="true">
        <defs>
          <linearGradient id="emp1" x1="0" y1="0" x2="1" y2="1">
            <stop offset="0" stop-color="#EEF1FE"/>
            <stop offset="1" stop-color="#DCE3FE"/>
          </linearGradient>
        </defs>
        <circle cx="100" cy="100" r="80" fill="url(#emp1)"/>
        <rect x="55" y="70" width="90" height="80" rx="6" fill="#fff" stroke="#BAC8FD" stroke-width="2"/>
        <line x1="70" y1="90" x2="130" y2="90" stroke="#BAC8FD" stroke-width="3" stroke-linecap="round"/>
        <line x1="70" y1="105" x2="115" y2="105" stroke="#DCE3FE" stroke-width="3" stroke-linecap="round"/>
        <line x1="70" y1="120" x2="125" y2="120" stroke="#DCE3FE" stroke-width="3" stroke-linecap="round"/>
        <circle cx="155" cy="65" r="14" fill="#3B5BDB"/>
        <line x1="155" y1="59" x2="155" y2="71" stroke="#fff" stroke-width="2.5" stroke-linecap="round"/>
        <line x1="149" y1="65" x2="161" y2="65" stroke="#fff" stroke-width="2.5" stroke-linecap="round"/>
      </svg>
    </div>
    <h3 class="empty-title">{{ title }}</h3>
    <p v-if="description" class="empty-desc">{{ description }}</p>
    <div class="empty-actions">
      <slot />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, default: '暂无数据' },
  description: { type: String, default: '' }
})
</script>

<style scoped>
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 16px;
  text-align: center;
}
.empty-art { margin-bottom: 12px; opacity: 0.95; }
.empty-title {
  font-size: var(--fs-lg);
  color: var(--color-text);
  margin: 0 0 8px;
}
.empty-desc {
  color: var(--color-text-muted);
  margin: 0 0 20px;
  max-width: 360px;
  font-size: var(--fs-base);
}
.empty-actions :deep(.el-button) { margin: 0 6px; }
</style>
<template>
  <div class="stat-card" :class="`accent-${accent}`">
    <div class="stat-icon">
      <el-icon :size="22"><component :is="iconComp" /></el-icon>
    </div>
    <div class="stat-body">
      <div class="stat-label">{{ label }}</div>
      <div class="stat-value">
        <span class="value-num">{{ displayValue }}</span>
        <span v-if="suffix" class="value-suffix">{{ suffix }}</span>
      </div>
      <div v-if="hint" class="stat-hint">
        <el-icon v-if="trend === 'up'" class="trend-up"><CaretTop /></el-icon>
        <el-icon v-else-if="trend === 'down'" class="trend-down"><CaretBottom /></el-icon>
        {{ hint }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  label: { type: String, required: true },
  value: { type: [Number, String], default: 0 },
  suffix: { type: String, default: '' },
  hint: { type: String, default: '' },
  trend: { type: String, default: '' }, // up/down
  icon: { type: [String, Object], default: 'DataAnalysis' },
  accent: { type: String, default: 'primary' } // primary/cyan/orange/green/purple/red
})

const iconComp = computed(() => props.icon)
const displayValue = computed(() => {
  if (typeof props.value === 'number') return props.value.toLocaleString()
  return props.value
})
</script>

<style scoped>
.stat-card {
  background: var(--color-bg-elevated);
  border-radius: var(--radius-lg);
  padding: var(--space-5) var(--space-6);
  border: 1px solid var(--color-border-soft);
  box-shadow: var(--shadow-sm);
  display: flex;
  align-items: center;
  gap: var(--space-4);
  transition: transform .25s, box-shadow .25s;
}
.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}
.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.accent-primary .stat-icon { background: var(--color-primary-50); color: var(--color-primary-600); }
.accent-cyan    .stat-icon { background: #E3F8FA; color: var(--color-accent-cyan); }
.accent-orange  .stat-icon { background: #FFF4E6; color: var(--color-accent-orange); }
.accent-green   .stat-icon { background: #EBFBEE; color: var(--color-accent-green); }
.accent-purple  .stat-icon { background: #F3F0FF; color: var(--color-accent-purple); }
.accent-red     .stat-icon { background: #FFE3E3; color: var(--color-accent-red); }

.stat-body { flex: 1; min-width: 0; }
.stat-label {
  color: var(--color-text-soft);
  font-size: var(--fs-sm);
  margin-bottom: 4px;
}
.stat-value { display: flex; align-items: baseline; gap: 4px; }
.value-num {
  font-size: var(--fs-3xl);
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.1;
  font-feature-settings: 'tnum';
}
.value-suffix { font-size: var(--fs-sm); color: var(--color-text-muted); }
.stat-hint {
  margin-top: 6px;
  font-size: var(--fs-xs);
  color: var(--color-text-soft);
  display: flex;
  align-items: center;
  gap: 2px;
}
.trend-up { color: var(--color-accent-green); }
.trend-down { color: var(--color-accent-red); }
</style>
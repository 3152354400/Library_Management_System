<template>
  <span class="status-tag" :class="`type-${type}`">
    <span class="dot" />
    {{ label || defaultLabel }}
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: { type: String, required: true },
  label: { type: String, default: '' },
  // 自定义映射
  map: { type: Object, default: () => ({}) }
})

const type = computed(() => {
  if (props.map[props.status]) return props.map[props.status]
  const s = props.status
  if (['正常', '已完成', '已缴纳', '已采纳', '已确认', '已赔偿', '已读', 'Y', '生效中'].includes(s)) return 'success'
  if (['借出', '未归还', '未完成', '未读', 'N'].includes(s)) return 'warning'
  if (['待处理', '待审核', '待缴纳'].includes(s)) return 'danger'
  if (['已取消', '驳回', '下架', '已撤回', '已删除', '冻结'].includes(s)) return 'muted'
  return 'info'
})

const defaultLabel = computed(() => props.status)
</script>

<style scoped>
.status-tag {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--fs-xs);
  padding: 3px 10px;
  border-radius: 999px;
  font-weight: 500;
  line-height: 1.5;
}
.dot { width: 6px; height: 6px; border-radius: 999px; display: inline-block; }
.type-success { background: #EBFBEE; color: var(--color-accent-green); }
.type-success .dot { background: var(--color-accent-green); }
.type-warning { background: #FFF9DB; color: #B08900; }
.type-warning .dot { background: var(--color-accent-orange); }
.type-danger  { background: #FFF5F5; color: var(--color-accent-red); }
.type-danger .dot  { background: var(--color-accent-red); }
.type-muted   { background: #F1F3F5; color: var(--color-text-muted); }
.type-muted .dot   { background: var(--color-text-muted); }
.type-info    { background: var(--color-primary-50); color: var(--color-primary-600); }
.type-info .dot    { background: var(--color-primary-600); }
</style>
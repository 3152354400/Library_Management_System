<template>
  <div :class="['card', className]">
    <div class="title">{{ title }}</div>
    <div class="count">{{ count }}</div>
  </div>
</template>

<script setup>
defineProps({
  title: String,
  count: [String, Number],
  className: String
})
</script>

<style scoped>
.card {
  padding: 16px;
  border-radius: 12px;
  color: white;
  font-weight: bold;
  display: flex;
  flex-direction: column;
  align-items: center;
}
.card-red {
  background: #ef4444;
}
.card-orange {
  background: #f97316;
}
.card-green {
  background: #10b981;
}
.card-blue {
  background: #3b82f6;
}
.title {
  font-size: 14px;
  margin-bottom: 8px;
}
.count {
  font-size: 24px;
}
</style>

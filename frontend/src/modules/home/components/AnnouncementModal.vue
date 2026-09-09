<template>
  <div class="modal-overlay" @click.self="close">
    <div class="modal-content">
      <button @click="close" class="close-button">&times;</button>
      <h2 class="title">{{ announcement.Title }}</h2>
      <p class="date">{{ new Date(announcement.CreateTime).toLocaleString() }}</p>
      <div class="content-body" v-html="formattedContent"></div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  announcement: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['close'])

const close = () => {
  emit('close')
}

// 将换行符 \n 替换为 <br> 以便在 HTML 中正确显示
const formattedContent = computed(() => {
  return props.announcement.Content.replace(/\n/g, '<br />');
});
</script>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0, 0, 0, 0.6); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-content { background-color: white; padding: 2rem; border-radius: 8px; width: 60%; max-width: 800px; height: 70%; max-height: 600px; position: relative; display: flex; flex-direction: column; }
.close-button { position: absolute; top: 1rem; right: 1rem; font-size: 2rem; line-height: 1; border: none; background: none; cursor: pointer; }
.title { font-size: 1.75rem; font-weight: bold; margin-bottom: 0.5rem; }
.date { font-size: 0.875rem; color: #6b7280; margin-bottom: 1.5rem; }
.content-body { flex-grow: 1; overflow-y: auto; line-height: 1.8; color: #374151; }
</style>
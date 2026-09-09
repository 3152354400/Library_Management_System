<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      <button @click="$emit('close')" class="close-button">&times;</button>
      <h2 class="title">📖 续借确认</h2>

      <div class="book-info">
        <div class="info-row">
          <span class="info-label">书名：</span>
          <span class="info-value">{{ record.BookTitle || '未知' }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">作者：</span>
          <span class="info-value">{{ record.BookAuthor || '-' }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">ISBN：</span>
          <span class="info-value">{{ record.ISBN }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">原应还时间：</span>
          <span class="info-value">{{ formatDate(record.DueTime) }}</span>
        </div>
        <div class="info-row highlight">
          <span class="info-label">新应还时间：</span>
          <span class="info-value text-blue-600">{{ newDueTime ? formatDate(newDueTime) : '计算中...' }}</span>
        </div>
      </div>

      <div class="tips">
        <p>📌 续借规则：</p>
        <ul>
          <li>每本书最多续借 <strong>2次</strong></li>
          <li>每次续借延长 <strong>30天</strong></li>
          <li>逾期图书需先归还</li>
          <li>信用分需 &ge; 60分</li>
        </ul>
      </div>

      <button @click="submitRenew" :disabled="isSubmitting" class="renew-btn">
        {{ isSubmitting ? '正在续借...' : '确认续借' }}
      </button>

      <p v-if="error" class="error-message">{{ error }}</p>
      <p v-if="successMsg" class="success-message">{{ successMsg }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { renewBook } from '@/modules/reader/api.js';

const props = defineProps({
  record: { type: Object, required: true }
});
const emit = defineEmits(['close', 'renew-success']);

const isSubmitting = ref(false);
const error = ref('');
const successMsg = ref('');
const newDueTime = ref(null);

function formatDate(date) {
  if (!date) return '-';
  return new Date(date).toLocaleString('zh-CN');
}

async function submitRenew() {
  try {
    isSubmitting.value = true;
    error.value = '';
    successMsg.value = '';

    const res = await renewBook({ BookId: props.record.BookID });
    const data = res.data;

    if (data.success) {
      successMsg.value = data.message;
      newDueTime.value = data.newDueTime;
      emit('renew-success', data);
      // 2秒后关闭
      setTimeout(() => emit('close'), 2000);
    } else {
      error.value = data.message || '续借失败';
    }
  } catch (err) {
    console.error('Renew failed:', err);
    error.value = err.response?.data?.message || '续借失败，请稍后再试。';
  } finally {
    isSubmitting.value = false;
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
}
.modal-content {
  background-color: white;
  padding: 2rem;
  border-radius: 8px;
  width: 90%;
  max-width: 480px;
  position: relative;
}
.close-button {
  position: absolute;
  top: 1rem;
  right: 1rem;
  font-size: 2rem;
  line-height: 1;
  border: none;
  background: none;
  cursor: pointer;
  color: #9ca3af;
}
.title {
  font-size: 1.5rem;
  font-weight: bold;
  margin-bottom: 1.25rem;
  color: #1f2937;
}
.book-info {
  background-color: #f9fafb;
  padding: 1rem;
  border-radius: 6px;
  margin-bottom: 1.25rem;
}
.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 0;
  font-size: 14px;
}
.info-row.highlight {
  border-top: 1px dashed #d1d5db;
  padding-top: 8px;
  margin-top: 4px;
}
.info-label {
  color: #6b7280;
  font-weight: 500;
}
.info-value {
  color: #1f2937;
  font-weight: 600;
}
.tips {
  background-color: #fef3c7;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1.25rem;
  font-size: 13px;
}
.tips p {
  margin: 0 0 6px;
  font-weight: 600;
  color: #92400e;
}
.tips ul {
  margin: 0;
  padding-left: 20px;
  color: #78350f;
}
.tips li {
  margin: 2px 0;
}
.renew-btn {
  width: 100%;
  background-color: #2563eb;
  color: white;
  padding: 0.75rem;
  border-radius: 6px;
  font-weight: bold;
  border: none;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 15px;
}
.renew-btn:disabled {
  background-color: #9ca3af;
  cursor: not-allowed;
}
.renew-btn:hover:not(:disabled) {
  background-color: #1d4ed8;
}
.error-message {
  color: #dc2626;
  text-align: center;
  margin-top: 0.75rem;
  font-size: 0.875rem;
}
.success-message {
  color: #059669;
  text-align: center;
  margin-top: 0.75rem;
  font-size: 0.875rem;
  font-weight: 600;
}
</style>

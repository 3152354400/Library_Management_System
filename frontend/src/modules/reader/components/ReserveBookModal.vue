<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      <button @click="$emit('close')" class="close-button">&times;</button>
      <h2 class="title">📌 预约图书</h2>

      <div class="book-info" v-if="book">
        <div class="info-row">
          <span class="info-label">书名：</span>
          <span class="info-value">{{ book.Title || '未知' }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">作者：</span>
          <span class="info-value">{{ book.Author || '-' }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">ISBN：</span>
          <span class="info-value">{{ book.ISBN }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">可借副本：</span>
          <span :class="['info-value', book.AvailableStock > 0 ? 'text-green-600' : 'text-orange-600']">
            {{ book.AvailableStock > 0 ? `${book.AvailableStock} 本可借` : '当前无可借' }}
          </span>
        </div>
      </div>

      <div v-if="book && book.AvailableStock > 0" class="hint">
        ⚠️ 当前图书有可借副本，建议直接到馆借阅
      </div>

      <div class="form-group">
        <label>期望借书天数：<span class="font-bold text-blue-600">{{ expectedDays }} 天</span></label>
        <input type="range" min="1" max="30" v-model="expectedDays" class="slider" />
        <p class="hint-text">预约成功后，系统将为您保留 3 天</p>
      </div>

      <div class="tips">
        <p>📌 预约规则：</p>
        <ul>
          <li>仅当所有副本均借出时可预约</li>
          <li>每位读者最多同时预约 <strong>3本</strong></li>
          <li>预约成功后将排队等待</li>
          <li>有书归还时会优先通知排在前面的读者</li>
        </ul>
      </div>

      <button @click="submitReserve" :disabled="isSubmitting" class="reserve-btn">
        {{ isSubmitting ? '正在预约...' : '确认预约' }}
      </button>

      <p v-if="error" class="error-message">{{ error }}</p>
      <p v-if="successMsg" class="success-message">{{ successMsg }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { reserveBook } from '@/modules/reader/api.js';

const props = defineProps({
  book: { type: Object, required: true }
});
const emit = defineEmits(['close', 'reserve-success']);

const expectedDays = ref(7);
const isSubmitting = ref(false);
const error = ref('');
const successMsg = ref('');

async function submitReserve() {
  try {
    isSubmitting.value = true;
    error.value = '';
    successMsg.value = '';

    const res = await reserveBook({
      ISBN: props.book.ISBN,
      ExpectedDays: expectedDays.value
    });
    const data = res.data;

    if (data.success) {
      successMsg.value = data.message;
      emit('reserve-success', data);
      setTimeout(() => emit('close'), 1800);
    } else {
      error.value = data.message || '预约失败';
    }
  } catch (err) {
    console.error('Reserve failed:', err);
    error.value = err.response?.data?.message || '预约失败，请稍后再试。';
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
  margin-bottom: 1rem;
}
.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 0;
  font-size: 14px;
}
.info-label {
  color: #6b7280;
  font-weight: 500;
}
.info-value {
  color: #1f2937;
  font-weight: 600;
}
.hint {
  background-color: #fef3c7;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1rem;
  font-size: 13px;
  color: #92400e;
}
.form-group {
  margin-bottom: 1rem;
}
.form-group label {
  font-weight: 500;
  color: #374151;
  font-size: 14px;
}
.slider {
  width: 100%;
  margin-top: 0.5rem;
}
.hint-text {
  font-size: 12px;
  color: #9ca3af;
  margin-top: 4px;
}
.tips {
  background-color: #ecfeff;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1.25rem;
  font-size: 13px;
}
.tips p {
  margin: 0 0 6px;
  font-weight: 600;
  color: #0e7490;
}
.tips ul {
  margin: 0;
  padding-left: 20px;
  color: #155e75;
}
.tips li {
  margin: 2px 0;
}
.reserve-btn {
  width: 100%;
  background-color: #f97316;
  color: white;
  padding: 0.75rem;
  border-radius: 6px;
  font-weight: bold;
  border: none;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 15px;
}
.reserve-btn:disabled {
  background-color: #9ca3af;
  cursor: not-allowed;
}
.reserve-btn:hover:not(:disabled) {
  background-color: #ea580c;
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

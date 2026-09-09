<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      <button @click="$emit('close')" class="close-button">&times;</button>
      <h2 class="title">💡 荐购图书</h2>

      <form @submit.prevent="submitRecommend" class="form">
        <div class="form-group">
          <label class="form-label">书名 <span class="required">*</span></label>
          <input
            v-model="form.Title"
            type="text"
            placeholder="请输入书名"
            class="form-input"
            required
          />
        </div>

        <div class="form-group">
          <label class="form-label">ISBN（可选）</label>
          <input
            v-model="form.ISBN"
            type="text"
            placeholder="如：9787532750845（若有则填）"
            class="form-input"
          />
        </div>

        <div class="form-group">
          <label class="form-label">作者</label>
          <input
            v-model="form.Author"
            type="text"
            placeholder="请输入作者"
            class="form-input"
          />
        </div>

        <div class="form-group">
          <label class="form-label">出版社</label>
          <input
            v-model="form.Publisher"
            type="text"
            placeholder="请输入出版社"
            class="form-input"
          />
        </div>

        <div class="form-group">
          <label class="form-label">出版年份</label>
          <input
            v-model.number="form.PublishYear"
            type="number"
            min="1900"
            :max="currentYear"
            placeholder="如：2024"
            class="form-input"
          />
        </div>

        <div class="form-group">
          <label class="form-label">推荐理由</label>
          <textarea
            v-model="form.Reason"
            rows="3"
            placeholder="请简要说明推荐理由，例如：该书为某领域经典著作..."
            class="form-textarea"
            maxlength="500"
          />
          <span class="char-count">{{ form.Reason?.length || 0 }} / 500</span>
        </div>

        <div class="tips">
          <p>📌 荐购规则：</p>
          <ul>
            <li>每月最多荐购 <strong>5 次</strong></li>
            <li>同一本书不能重复荐购</li>
            <li>图书馆已馆藏的图书无需荐购</li>
          </ul>
        </div>

        <button type="submit" :disabled="isSubmitting" class="submit-btn">
          {{ isSubmitting ? '提交中...' : '提交荐购' }}
        </button>
      </form>

      <p v-if="error" class="error-message">{{ error }}</p>
      <p v-if="successMsg" class="success-message">{{ successMsg }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import { recommendPurchase } from '@/modules/reader/api.js';

const emit = defineEmits(['close', 'recommend-success']);

const currentYear = new Date().getFullYear();
const isSubmitting = ref(false);
const error = ref('');
const successMsg = ref('');

const form = reactive({
  Title: '',
  ISBN: '',
  Author: '',
  Publisher: '',
  PublishYear: null,
  Reason: ''
});

async function submitRecommend() {
  try {
    isSubmitting.value = true;
    error.value = '';
    successMsg.value = '';

    if (!form.Title?.trim()) {
      error.value = '书名不能为空';
      return;
    }

    const payload = {
      Title: form.Title.trim(),
      Author: form.Author?.trim() || null,
      Publisher: form.Publisher?.trim() || null,
      Reason: form.Reason?.trim() || null
    };
    if (form.ISBN?.trim()) payload.ISBN = form.ISBN.trim();
    if (form.PublishYear) payload.PublishYear = form.PublishYear;

    const res = await recommendPurchase(payload);
    const data = res.data;

    if (data.success) {
      successMsg.value = data.message;
      emit('recommend-success', data);
      setTimeout(() => emit('close'), 1800);
    } else {
      error.value = data.message || '荐购失败';
    }
  } catch (err) {
    console.error('Recommend failed:', err);
    error.value = err.response?.data?.message || '荐购失败，请稍后再试。';
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
  max-width: 520px;
  position: relative;
  max-height: 90vh;
  overflow-y: auto;
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
.form-group {
  margin-bottom: 1rem;
}
.form-label {
  display: block;
  font-weight: 500;
  color: #374151;
  margin-bottom: 0.4rem;
  font-size: 14px;
}
.required {
  color: #dc2626;
}
.form-input,
.form-textarea {
  width: 100%;
  padding: 0.55rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s;
  box-sizing: border-box;
  font-family: inherit;
}
.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #2563eb;
}
.form-textarea {
  resize: vertical;
  min-height: 70px;
}
.char-count {
  display: block;
  text-align: right;
  font-size: 12px;
  color: #9ca3af;
  margin-top: 2px;
}
.tips {
  background-color: #f0f9ff;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1.25rem;
  font-size: 13px;
}
.tips p {
  margin: 0 0 6px;
  font-weight: 600;
  color: #075985;
}
.tips ul {
  margin: 0;
  padding-left: 20px;
  color: #0c4a6e;
}
.tips li {
  margin: 2px 0;
}
.submit-btn {
  width: 100%;
  background-color: #7c3aed;
  color: white;
  padding: 0.75rem;
  border-radius: 6px;
  font-weight: bold;
  border: none;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 15px;
}
.submit-btn:disabled {
  background-color: #9ca3af;
  cursor: not-allowed;
}
.submit-btn:hover:not(:disabled) {
  background-color: #6d28d9;
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

<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      <button @click="$emit('close')" class="close-button">&times;</button>
      <h2 class="title">预约座位：{{ seat.SeatNumber }}</h2>
      <div class="info">
        位置：{{ formatBuilding(seat.BuildingID) }} {{ seat.Floor }}层 {{ seat.Zone }}区
      </div>

      <div class="time-selector">
        <label>选择时长：<span class="font-bold text-blue-600">{{ duration }} 小时</span></label>
        <input type="range" min="1" max="4" step="0.5" v-model="duration" class="slider">
      </div>

      <div class="time-display">
        <p><strong>开始时间：</strong> {{ formatTime(startTime) }}</p>
        <p><strong>结束时间：</strong> {{ formatTime(endTime) }}</p>
      </div>

      <button @click="submitReservation" :disabled="isSubmitting" class="reserve-btn">
        {{ isSubmitting ? '正在预约...' : '确认预约' }}
      </button>
      <p v-if="error" class="error-message">{{ error }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { createSeatReservation } from '@/modules/reader/api.js';

const props = defineProps({
  seat: { type: Object, required: true }
});
const emit = defineEmits(['close', 'reserve-success']);

const duration = ref(1); // 默认时长1小时
const isSubmitting = ref(false);
const error = ref('');

const startTime = computed(() => new Date());
const endTime = computed(() => {
  const end = new Date(startTime.value);
  end.setMinutes(end.getMinutes() + duration.value * 60);
  return end;
});

function formatTime(date) {
  return date.toLocaleString('zh-CN', { hour: '2-digit', minute: '2-digit' });
}

function formatBuilding(buildingId) {
  return buildingId === 21 ? '总图书馆' : '德文图书馆';
}

async function submitReservation() {
  try {
    isSubmitting.value = true;
    error.value = '';
    
    await createSeatReservation({
      SeatID: props.seat.SeatID,
      StartTime: startTime.value,
      EndTime: endTime.value,
    });
    
    alert('预约成功！');
    emit('reserve-success');
  } catch (err) {
    console.error('Reservation failed:', err);
    error.value = err.response?.data?.message || '预约失败，请稍后再试。';
  } finally {
    isSubmitting.value = false;
  }
}
</script>

<style scoped>
.modal-overlay { position: fixed; inset: 0; background-color: rgba(0, 0, 0, 0.6); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-content { background-color: white; padding: 2rem; border-radius: 8px; width: 90%; max-width: 450px; position: relative; }
.close-button { position: absolute; top: 1rem; right: 1rem; font-size: 2rem; line-height: 1; border: none; background: none; cursor: pointer; color: #9ca3af; }
.title { font-size: 1.5rem; font-weight: bold; margin-bottom: 0.5rem; }
.info { color: #6b7280; margin-bottom: 1.5rem; }
.time-selector { margin-bottom: 1.5rem; }
.time-selector label { font-weight: 500; }
.slider { width: 100%; margin-top: 0.5rem; }
.time-display { background-color: #f3f4f6; padding: 1rem; border-radius: 6px; margin-bottom: 1.5rem; }
.time-display p { margin: 0.25rem 0; }
.reserve-btn { width: 100%; background-color: #2563eb; color: white; padding: 0.75rem; border-radius: 6px; font-weight: bold; transition: background-color 0.2s; }
.reserve-btn:disabled { background-color: #9ca3af; cursor: not-allowed; }
.reserve-btn:hover:not(:disabled) { background-color: #1d4ed8; }
.error-message { color: #dc2626; text-align: center; margin-top: 1rem; font-size: 0.875rem; }
</style>
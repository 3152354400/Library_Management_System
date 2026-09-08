<template>
    <Layout>
        <div class="my-reservations-page">
            <h1 class="page-title">我的预约记录</h1>
            
            <div v-if="loading" class="status-message">
            正在加载您的预约记录...
            </div>
            <div v-else-if="error" class="status-message error">
            {{ error }}
            </div>
            <div v-else-if="reservations.length === 0" class="status-message">
            您还没有任何预约记录。
            </div>
            
            <div v-else class="reservations-list">
            <div v-for="r in reservations" :key="r.ReservationID" class="reservation-card">
                <div class="card-header">
                <span class="building-name">{{ r.BuildingName }}</span>
                <span class="status" :class="getStatusClass(r.Status)">{{ r.Status }}</span>
                </div>
                <div class="card-body">
                <p><strong>座位号：</strong> {{ r.Floor }}楼 - {{ r.SeatNumber }}</p>
                <p><strong>开始时间：</strong> {{ formatTime(r.StartTime) }}</p>
                <p><strong>结束时间：</strong> {{ formatTime(r.EndTime) }}</p>
                </div>
                <div class="card-footer" v-if="r.Status === '未完成' && isFuture(r.StartTime)">
                <button @click="handleCancel(r.ReservationID)" class="cancel-btn">取消预约</button>
                </div>
            </div>
            </div>
        </div>
  </Layout>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getMyReservations, cancelReservation } from '@/modules/reader/api.js';
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';
const loading = ref(true);
const error = ref('');
const reservations = ref([]);

async function fetchMyReservations() {
  try {
    loading.value = true;
    error.value = '';
    const response = await getMyReservations();
    reservations.value = response.data;
  } catch (err) {
    console.error('Failed to fetch my reservations:', err);
    error.value = '加载预约记录失败，请稍后重试。';
  } finally {
    loading.value = false;
  }
}

async function handleCancel(reservationId) {
    if (!confirm('您确定要取消本次预约吗？')) return;
    try {
        await cancelReservation(reservationId);
        alert('预约已成功取消！');
        // 刷新列表以显示最新状态
        await fetchMyReservations();
    } catch (err) {
        console.error('Failed to cancel reservation:', err);
        alert(err.response?.data?.message || '取消预约失败。');
    }
}

// 辅助函数：格式化时间
function formatTime(dateStr) {
    return new Date(dateStr).toLocaleString('zh-CN', { 
        year: 'numeric', 
        month: '2-digit', 
        day: '2-digit', 
        hour: '2-digit', 
        minute: '2-digit' 
    });
}

// 辅助函数：根据状态返回不同的CSS class
function getStatusClass(status) {
    switch(status) {
        case '未完成': return 'status-pending';
        case '已完成': return 'status-completed';
        case '取消': return 'status-cancelled';
        default: return '';
    }
}

// 辅助函数：判断预约是否还未开始
function isFuture(dateStr) {
    return new Date(dateStr) > new Date();
}

onMounted(fetchMyReservations);
</script>

<style scoped>
.my-reservations-page { 
  max-width: 900px; 
  margin: 2rem auto; 
  padding: 1rem; 
  font-family: sans-serif;
}

.page-title { 
  font-size: 2rem; 
  font-weight: bold; 
  margin-bottom: 2.5rem; 
  text-align: center;
  color: #1e293b;
}

.status-message { 
  text-align: center; 
  padding: 3rem; 
  color: #64748b; 
  background-color: #f8fafc;
  border-radius: 8px;
}
.status-message.error {
    color: #dc2626;
    background-color: #fee2e2;
}

.reservations-list { 
  display: grid; 
  gap: 1.5rem; 
}

.reservation-card { 
  background-color: #fff; 
  border: 1px solid #e2e8f0; 
  border-radius: 8px; 
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
  transition: box-shadow 0.3s ease, transform 0.3s ease;
}
.reservation-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
}

.card-header { 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  padding: 1rem 1.5rem; 
  border-bottom: 1px solid #e2e8f0; 
}

.building-name { 
  font-weight: bold;
  font-size: 1.1rem;
  color: #0f172a;
}

.status { 
  padding: 0.25rem 0.75rem; 
  border-radius: 9999px; 
  font-size: 0.75rem; 
  font-weight: 600; 
  text-transform: uppercase;
}

.status.status-pending { background-color: #dbeafe; color: #1e40af; }
.status.status-completed { background-color: #d1fae5; color: #065f46; }
.status.status-cancelled { background-color: #fee2e2; color: #991b1b; }

.card-body { 
  padding: 1.5rem; 
}

.card-body p { 
  margin: 0.5rem 0; 
  color: #334155;
}

.card-footer { 
  padding: 1rem 1.5rem; 
  background-color: #f8fafc;
  border-top: 1px solid #e2e8f0; 
  text-align: right; 
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
}

.cancel-btn { 
  background-color: #ef4444; 
  color: white; 
  padding: 0.5rem 1.25rem; 
  border: none; 
  border-radius: 6px; 
  cursor: pointer;
  font-weight: 500;
  transition: background-color 0.2s ease;
}
.cancel-btn:hover {
  background-color: #dc2626;
}
</style>
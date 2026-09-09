<template>
  <Layout>
    <h1 class="title">📋 我的图书预约</h1>

    <div class="tabs">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        :class="['tab-btn', { active: activeTab === tab.value }]"
        @click="activeTab = tab.value"
      >
        {{ tab.label }}
        <span v-if="tab.count > 0" class="tab-badge">{{ tab.count }}</span>
      </button>
    </div>

    <div class="actions">
      <button class="btn-refresh" @click="loadReserves">🔄 刷新</button>
    </div>

    <div class="card" v-if="filteredReserves.length">
      <table class="styled-table">
        <thead>
          <tr>
            <th>书名</th>
            <th>作者</th>
            <th>ISBN</th>
            <th>预约时间</th>
            <th>期望借书日</th>
            <th>过期时间</th>
            <th>状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in filteredReserves" :key="r.reserveId">
            <td class="text-left">{{ r.bookTitle }}</td>
            <td>{{ r.author || '-' }}</td>
            <td>{{ r.ISBN }}</td>
            <td>{{ formatDate(r.reserveTime) }}</td>
            <td>{{ formatDate(r.expectedBorrowTime) }}</td>
            <td>{{ formatDate(r.expireTime) }}</td>
            <td>
              <span :class="getStatusClass(r.status)">{{ r.status }}</span>
            </td>
            <td>
              <button
                v-if="canCancel(r)"
                @click="handleCancel(r)"
                class="action-btn cancel-btn"
                :disabled="cancelingId === r.reserveId"
              >
                {{ cancelingId === r.reserveId ? '取消中...' : '取消预约' }}
              </button>
              <span v-else class="text-gray-400">-</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="empty-state" v-else>
      <p>暂无预约记录 📚</p>
    </div>

    <div class="notice">
      <p>📌 <strong>预约说明：</strong></p>
      <ul>
        <li>当图书所有副本均被借出时，您可以预约排队</li>
        <li>每位读者最多同时预约 <strong>3本</strong></li>
        <li>预约成功后系统会保留 <strong>3天</strong></li>
        <li>有读者归还该书时，排在前面的预约者会收到通知</li>
      </ul>
    </div>
  </Layout>

  <teleport to="body">
    <div v-if="showToast" class="toast">{{ toastMsg }}</div>
  </teleport>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { getMyBookReserves, cancelReserve } from '@/modules/reader/api.js';
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';

const reserves = ref([]);
const activeTab = ref('all');
const cancelingId = ref(null);
const showToast = ref(false);
const toastMsg = ref('');

const tabs = computed(() => [
  { label: '全部', value: 'all', count: reserves.value.length },
  { label: '等待中', value: '等待中', count: reserves.value.filter(r => r.status === '等待中').length },
  { label: '已通知', value: '已通知', count: reserves.value.filter(r => r.status === '已通知').length },
  { label: '已借出', value: '已借出', count: reserves.value.filter(r => r.status === '已借出').length },
  { label: '已取消', value: '已取消', count: reserves.value.filter(r => r.status === '已取消').length }
]);

const filteredReserves = computed(() => {
  if (activeTab.value === 'all') return reserves.value;
  return reserves.value.filter(r => r.status === activeTab.value);
});

const loadReserves = async () => {
  try {
    const res = await getMyBookReserves();
    reserves.value = res.data || [];
  } catch (error) {
    console.error('获取预约记录失败', error);
  }
};

function formatDate(date) {
  if (!date) return '-';
  return new Date(date).toLocaleString('zh-CN');
}

function canCancel(r) {
  return r.status === '等待中' || r.status === '已通知';
}

function getStatusClass(status) {
  return {
    'badge-waiting': status === '等待中',
    'badge-notified': status === '已通知',
    'badge-borrowed': status === '已借出',
    'badge-canceled': status === '已取消',
    'badge-expired': status === '已过期'
  }[status] || '';
}

async function handleCancel(r) {
  if (!confirm(`确定要取消预约《${r.bookTitle}》吗？`)) return;
  try {
    cancelingId.value = r.reserveId;
    await cancelReserve(r.reserveId);
    showToastMsg('取消成功');
    await loadReserves();
  } catch (error) {
    console.error('取消预约失败', error);
    alert('取消预约失败，请稍后再试');
  } finally {
    cancelingId.value = null;
  }
}

function showToastMsg(msg) {
  toastMsg.value = msg;
  showToast.value = true;
  setTimeout(() => { showToast.value = false; }, 1500);
}

onMounted(loadReserves);
</script>

<style scoped>
.title {
  text-align: center;
  margin-bottom: 20px;
  color: #2c3e50;
  font-size: 30px;
  font-weight: bold;
}
.tabs {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}
.tab-btn {
  padding: 8px 18px;
  border: 1px solid #d1d5db;
  background: white;
  border-radius: 20px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}
.tab-btn:hover {
  background: #f3f4f6;
}
.tab-btn.active {
  background: #2563eb;
  color: white;
  border-color: #2563eb;
}
.tab-badge {
  background: rgba(255,255,255,0.3);
  padding: 2px 7px;
  border-radius: 999px;
  font-size: 12px;
}
.tab-btn.active .tab-badge {
  background: rgba(255,255,255,0.3);
}
.actions {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 15px;
}
.btn-refresh {
  background: #3498db;
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 15px;
  transition: 0.3s;
}
.btn-refresh:hover {
  background: #2980b9;
}
.card {
  background: #fff;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0px 4px 12px rgba(0,0,0,0.08);
}
.styled-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 15px;
}
.styled-table th {
  background-color: #4da6ff;
  color: white;
  text-align: center;
  padding: 12px 8px;
}
.styled-table td {
  padding: 12px 8px;
  border-bottom: 1px solid #e5e7eb;
  text-align: center;
}
.styled-table tbody tr:nth-child(even) {
  background-color: #f9fbfd;
}
.styled-table tbody tr:hover {
  background-color: #e6f3ff;
}
.text-left {
  text-align: left;
}
.badge-waiting { background: #fef3c7; color: #92400e; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.badge-notified { background: #dbeafe; color: #1e40af; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.badge-borrowed { background: #d1fae5; color: #047857; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.badge-canceled { background: #f3f4f6; color: #6b7280; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.badge-expired { background: #fee2e2; color: #b91c1c; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.action-btn {
  padding: 4px 12px;
  border-radius: 4px;
  border: none;
  cursor: pointer;
  font-size: 13px;
  transition: all 0.2s;
}
.cancel-btn {
  background: #ef4444;
  color: white;
}
.cancel-btn:hover:not(:disabled) {
  background: #dc2626;
}
.cancel-btn:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}
.text-gray-400 { color: #9ca3af; }
.empty-state {
  text-align: center;
  padding: 60px;
  background: #fff;
  border-radius: 10px;
  color: #999;
  font-size: 18px;
  box-shadow: 0px 4px 12px rgba(0,0,0,0.08);
}
.notice {
  margin-top: 20px;
  padding: 16px 20px;
  background: #ecfeff;
  border-left: 4px solid #0ea5e9;
  border-radius: 6px;
  font-size: 14px;
  color: #0c4a6e;
}
.notice p { margin: 0 0 6px; font-weight: 600; }
.notice ul { margin: 0; padding-left: 20px; }
.notice li { margin: 2px 0; }
.toast {
  position: fixed;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  background: #2ecc71;
  color: #fff;
  padding: 10px 20px;
  border-radius: 6px;
  animation: fadeInOut 1.5s ease forwards;
  z-index: 9999;
}
@keyframes fadeInOut {
  0% { opacity: 0; transform: translate(-50%, -20px); }
  20% { opacity: 1; transform: translate(-50%, 0); }
  80% { opacity: 1; }
  100% { opacity: 0; transform: translate(-50%, -20px); }
}
</style>

<template>
  <Layout>
    <h1 class="title">🔔 我的消息</h1>

    <!-- 汇总 -->
    <div class="summary-bar">
      <span class="summary-text">
        共 {{ totalCount }} 条消息，
        <span class="text-red-500 font-bold">{{ unreadCount }}</span> 条未读
      </span>
      <div class="summary-actions">
        <button v-if="unreadCount > 0" class="btn-mark-all" @click="markAllRead">
          📖 全部标为已读
        </button>
        <button class="btn-refresh" @click="loadNotifications">🔄 刷新</button>
      </div>
    </div>

    <!-- 筛选 -->
    <div class="filter-bar">
      <button
        v-for="f in filters"
        :key="f.value"
        :class="['filter-btn', { active: activeFilter === f.value }]"
        @click="activeFilter = f.value"
      >
        {{ f.label }}
      </button>
    </div>

    <!-- 通知列表 -->
    <div class="card" v-if="notifications.length">
      <div
        v-for="n in notifications"
        :key="n.notificationId"
        :class="['notif-item', { unread: n.isRead === 'N' }]"
        @click="handleRead(n)"
      >
        <div class="notif-header">
          <span class="notif-icon">{{ getTypeIcon(n.type) }}</span>
          <span class="notif-title">{{ n.title }}</span>
          <span :class="['notif-priority', getPriorityClass(n.priority)]">
            {{ n.priority }}
          </span>
          <span class="notif-time">{{ formatDate(n.createTime) }}</span>
          <span v-if="n.isRead === 'N'" class="unread-dot">●</span>
        </div>
        <div class="notif-content">{{ n.content }}</div>
        <div class="notif-footer">
          <span class="notif-type">{{ n.type }}</span>
        </div>
      </div>

      <!-- 分页 -->
      <div class="pagination" v-if="totalPages > 1">
        <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">上一页</button>
        <span>第 {{ currentPage }} / {{ totalPages }} 页</span>
        <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages">下一页</button>
      </div>
    </div>

    <div class="empty-state" v-else>
      <p>📭 暂无消息通知</p>
    </div>
  </Layout>

  <teleport to="body">
    <div v-if="showToast" class="toast">{{ toastMsg }}</div>
  </teleport>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { getNotifications, getUnreadCount, markNotificationRead, markAllNotificationsRead } from '@/modules/reader/api.js';
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';

const notifications = ref([]);
const totalCount = ref(0);
const unreadCount = ref(0);
const currentPage = ref(1);
const pageSize = 20;
const activeFilter = ref('all');
const showToast = ref(false);
const toastMsg = ref('');

const filters = [
  { label: '全部', value: 'all' },
  { label: '未读', value: 'N' },
  { label: '逾期提醒', value: '逾期提醒' },
  { label: '预约到书', value: '预约到书' },
  { label: '罚款通知', value: '罚款通知' },
  { label: '系统公告', value: '系统公告' }
];

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize));

const loadNotifications = async () => {
  try {
    const params = { pageSize, pageNum: currentPage.value };
    if (activeFilter.value !== 'all') {
      if (activeFilter.value === 'N') {
        params.isRead = 'N';
      } else {
        params.type = activeFilter.value;
      }
    }
    const res = await getNotifications(params);
    notifications.value = res.data?.notifications || [];
    totalCount.value = res.data?.totalCount || 0;
    unreadCount.value = res.data?.unreadCount || 0;
  } catch (error) {
    console.error('获取通知失败', error);
  }
};

const loadUnreadCount = async () => {
  try {
    const res = await getUnreadCount();
    unreadCount.value = res.data?.unreadCount || 0;
  } catch (error) {
    console.error('获取未读数失败', error);
  }
};

function formatDate(date) {
  if (!date) return '-';
  return new Date(date).toLocaleString('zh-CN');
}

function getTypeIcon(type) {
  const icons = {
    '逾期提醒': '⚠️',
    '预约到书': '📖',
    '罚款通知': '💰',
    '系统公告': '📢',
    '评论回复': '💬',
    '借阅成功': '✅',
    '还书提醒': '📚',
    '其他': '📋'
  };
  return icons[type] || '📋';
}

function getPriorityClass(priority) {
  return {
    '普通': 'priority-normal',
    '重要': 'priority-important',
    '紧急': 'priority-urgent'
  }[priority] || 'priority-normal';
}

async function handleRead(n) {
  if (n.isRead === 'Y') return;
  try {
    await markNotificationRead(n.notificationId);
    n.isRead = 'Y';
    unreadCount.value = Math.max(0, unreadCount.value - 1);
  } catch (error) {
    console.error('标记已读失败', error);
  }
}

async function markAllRead() {
  try {
    await markAllNotificationsRead();
    showToastMsg('已全部标记为已读');
    await loadNotifications();
  } catch (error) {
    console.error('全部已读失败', error);
  }
}

function changePage(page) {
  if (page < 1 || page > totalPages.value) return;
  currentPage.value = page;
  loadNotifications();
}

function showToastMsg(msg) {
  toastMsg.value = msg;
  showToast.value = true;
  setTimeout(() => { showToast.value = false; }, 1500);
}

onMounted(() => {
  loadNotifications();
  loadUnreadCount();
});
</script>

<style scoped>
.title {
  text-align: center;
  margin-bottom: 20px;
  color: #2c3e50;
  font-size: 30px;
  font-weight: bold;
}
.summary-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding: 12px 16px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.06);
}
.summary-text { font-size: 15px; color: #6b7280; }
.summary-actions { display: flex; gap: 10px; }
.btn-refresh, .btn-mark-all {
  padding: 6px 14px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 14px;
  transition: 0.2s;
}
.btn-refresh { background: #3498db; color: white; }
.btn-refresh:hover { background: #2980b9; }
.btn-mark-all { background: #059669; color: white; }
.btn-mark-all:hover { background: #047857; }
.filter-bar {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}
.filter-btn {
  padding: 6px 14px;
  border: 1px solid #d1d5db;
  background: white;
  border-radius: 20px;
  cursor: pointer;
  font-size: 13px;
  transition: all 0.2s;
}
.filter-btn:hover { background: #f3f4f6; }
.filter-btn.active { background: #2563eb; color: white; border-color: #2563eb; }
.card {
  background: #fff;
  border-radius: 10px;
  box-shadow: 0px 4px 12px rgba(0,0,0,0.08);
  overflow: hidden;
}
.notif-item {
  padding: 16px 20px;
  border-bottom: 1px solid #f3f4f6;
  cursor: pointer;
  transition: background 0.2s;
}
.notif-item:last-child { border-bottom: none; }
.notif-item:hover { background: #f9fbfd; }
.notif-item.unread { background: #eff6ff; }
.notif-item.unread:hover { background: #dbeafe; }
.notif-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
  flex-wrap: wrap;
}
.notif-icon { font-size: 18px; }
.notif-title { font-weight: 600; font-size: 15px; color: #1f2937; flex: 1; }
.notif-priority {
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 999px;
  font-weight: 600;
}
.priority-normal { background: #f3f4f6; color: #6b7280; }
.priority-important { background: #fef3c7; color: #92400e; }
.priority-urgent { background: #fee2e2; color: #b91c1c; }
.notif-time { font-size: 12px; color: #9ca3af; margin-left: auto; }
.unread-dot { color: #ef4444; font-size: 18px; }
.notif-content { font-size: 14px; color: #4b5563; margin-bottom: 6px; line-height: 1.5; }
.notif-footer { display: flex; gap: 8px; }
.notif-type {
  font-size: 12px;
  color: #6b7280;
  background: #f3f4f6;
  padding: 2px 8px;
  border-radius: 4px;
}
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 14px;
  padding: 16px;
  border-top: 1px solid #f3f4f6;
}
.pagination button {
  padding: 6px 14px;
  background: #4da6ff;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: 0.2s;
}
.pagination button:hover:not(:disabled) { background: #3399ff; }
.pagination button:disabled { background: #d1d5db; cursor: not-allowed; }
.empty-state {
  text-align: center;
  padding: 60px;
  background: #fff;
  border-radius: 10px;
  color: #999;
  font-size: 18px;
  box-shadow: 0px 4px 12px rgba(0,0,0,0.08);
}
.text-red-500 { color: #ef4444; }
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

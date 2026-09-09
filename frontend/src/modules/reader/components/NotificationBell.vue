<template>
  <div class="notification-bell">
    <button class="bell-button" @click="toggle">
      🔔
      <span v-if="unreadCount > 0" class="badge">{{ unreadCount > 99 ? '99+' : unreadCount }}</span>
    </button>

    <transition name="fade">
      <div v-if="showPanel" class="notif-panel">
        <div class="panel-header">
          <span>消息通知 ({{ unreadCount }}未读)</span>
          <button v-if="unreadCount > 0" class="btn-mark" @click="markAllRead">全部已读</button>
        </div>
        <div v-if="loading" class="loading">加载中...</div>
        <div v-else-if="notifications.length === 0" class="empty">暂无消息</div>
        <div v-else class="notif-list">
          <div
            v-for="n in notifications"
            :key="n.NotificationID || n.id"
            :class="['notif-item', { unread: n.isRead === 'N' || !n.isRead }]"
            @click="handleClick(n)"
          >
            <div class="notif-title">{{ n.title }}</div>
            <div class="notif-content">{{ n.content }}</div>
            <div class="notif-time">{{ formatDate(n.createTime) }}</div>
          </div>
        </div>
        <div class="panel-footer">
          <router-link to="/my/notifications" @click="showPanel = false">查看全部</router-link>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user.js'
import { getUnreadCount, markAllNotificationsRead, markNotificationRead } from '@/modules/reader/api.js'
import wsClient from '@/services/websocket.js'

const router = useRouter()
const userStore = useUserStore()

const showPanel = ref(false)
const unreadCount = ref(0)
const notifications = ref([])
const loading = ref(false)

let unsubscribe = null

async function loadUnreadCount() {
  if (!userStore.isLoggedIn) return
  try {
    const res = await getUnreadCount()
    unreadCount.value = res.data?.unreadCount || 0
  } catch (e) {
    console.error(e)
  }
}

async function loadRecentNotifications() {
  loading.value = true
  try {
    const res = await getUnreadCount()
    notifications.value = res.data?.recentNotifications || []
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

function toggle() {
  showPanel.value = !showPanel.value
  if (showPanel.value) {
    loadRecentNotifications()
  }
}

async function markAllRead() {
  try {
    await markAllNotificationsRead()
    unreadCount.value = 0
    notifications.value = notifications.value.map(n => ({ ...n, isRead: 'Y' }))
  } catch (e) {
    console.error(e)
  }
}

async function handleClick(n) {
  if (n.isRead === 'N' || !n.isRead) {
    try {
      await markNotificationRead(n.NotificationID || n.id)
      unreadCount.value = Math.max(0, unreadCount.value - 1)
      n.isRead = 'Y'
    } catch (e) {
      console.error(e)
    }
  }
}

function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleString('zh-CN')
}

// 处理WebSocket消息
function handleWSMessage(data) {
  if (data.type === 'notification' || data.unreadCount !== undefined) {
    if (data.unreadCount !== undefined) {
      unreadCount.value = data.unreadCount
    } else {
      unreadCount.value++
      notifications.value.unshift(data)
    }
  }
}

// 监听登录状态变化
watch(() => userStore.isLoggedIn, async (loggedIn) => {
  if (loggedIn) {
    await loadUnreadCount()
    wsClient.connect()
    unsubscribe = wsClient.onMessage(handleWSMessage)
  } else {
    wsClient.disconnect()
    unreadCount.value = 0
    notifications.value = []
  }
}, { immediate: false })

onMounted(async () => {
  if (userStore.isLoggedIn) {
    await loadUnreadCount()
    wsClient.connect()
    unsubscribe = wsClient.onMessage(handleWSMessage)

    // 轮询兜底（每30秒刷新一次，WebSocket失败时仍可工作）
    setInterval(loadUnreadCount, 30000)
  }
})

onBeforeUnmount(() => {
  if (unsubscribe) unsubscribe()
})
</script>

<style scoped>
.notification-bell { position: relative; display: inline-block; }
.bell-button { position: relative; background: none; border: none; font-size: 22px; cursor: pointer; padding: 4px 10px; }
.bell-button:hover { background: #f3f4f6; border-radius: 6px; }
.badge { position: absolute; top: -4px; right: 0; background: #ef4444; color: white; font-size: 11px; padding: 2px 6px; border-radius: 999px; min-width: 18px; text-align: center; font-weight: 600; }
.notif-panel { position: absolute; top: 40px; right: 0; width: 320px; max-height: 400px; background: white; border: 1px solid #e5e7eb; border-radius: 8px; box-shadow: 0 4px 16px rgba(0,0,0,0.1); z-index: 1000; overflow: hidden; display: flex; flex-direction: column; }
.panel-header { display: flex; justify-content: space-between; align-items: center; padding: 12px 16px; border-bottom: 1px solid #f3f4f6; font-size: 14px; font-weight: 600; }
.btn-mark { background: none; border: none; color: #4da6ff; font-size: 12px; cursor: pointer; }
.btn-mark:hover { text-decoration: underline; }
.notif-list { flex: 1; overflow-y: auto; max-height: 280px; }
.notif-item { padding: 12px 16px; border-bottom: 1px solid #f9fafb; cursor: pointer; transition: background 0.2s; }
.notif-item:hover { background: #f9fafb; }
.notif-item.unread { background: #eff6ff; }
.notif-item.unread:hover { background: #dbeafe; }
.notif-title { font-size: 14px; font-weight: 600; color: #1f2937; margin-bottom: 4px; }
.notif-content { font-size: 13px; color: #6b7280; line-height: 1.5; }
.notif-time { font-size: 12px; color: #9ca3af; margin-top: 4px; }
.loading, .empty { text-align: center; padding: 40px; color: #9ca3af; font-size: 14px; }
.panel-footer { padding: 10px; text-align: center; border-top: 1px solid #f3f4f6; }
.panel-footer a { color: #4da6ff; font-size: 13px; text-decoration: none; }
.panel-footer a:hover { text-decoration: underline; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>

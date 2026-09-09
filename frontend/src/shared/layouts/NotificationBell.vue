<template>
  <div class="bell-wrap" @click.stop>
    <el-badge :value="notificationStore.unreadCount" :hidden="!notificationStore.unreadCount" :max="99">
      <el-button circle text @click="open = !open">
        <el-icon :size="20"><Bell /></el-icon>
      </el-button>
    </el-badge>

    <transition name="fade">
      <div v-if="open" class="bell-dropdown" @click.stop>
        <div class="bell-header">
          <strong>消息中心</strong>
          <el-link v-if="notificationStore.unreadCount" type="primary" :underline="false" @click="markAllRead">全部已读</el-link>
        </div>
        <div class="bell-body">
          <el-empty v-if="!notificationStore.list.length" description="暂无消息" :image-size="80" />
          <div v-for="n in notificationStore.list.slice(0, 8)" :key="n.NotificationId" class="bell-item" :class="{ unread: n.IsRead === 'N' }" @click="onClick(n)">
            <div class="bell-dot" />
            <div class="bell-content">
              <div class="bell-title">{{ n.Title }}</div>
              <div class="bell-text">{{ n.Content }}</div>
              <div class="bell-time">{{ formatTime(n.CreateTime) }}</div>
            </div>
          </div>
        </div>
        <div class="bell-footer">
          <router-link to="/reader/notifications" @click="open = false">查看全部</router-link>
        </div>
      </div>
    </transition>

    <div v-if="open" class="bell-mask" @click="open = false" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useNotificationStore } from '@/stores/notification'

const router = useRouter()
const notificationStore = useNotificationStore()
const open = ref(false)

onMounted(() => {
  notificationStore.fetchList().catch(() => {})
})

async function markAllRead() {
  await notificationStore.markAllRead()
}

function onClick(n) {
  if (n.IsRead === 'N') notificationStore.markRead(n.NotificationId)
  open.value = false
  if (n.RelatedType === 'announcement') router.push('/announcements')
}

function formatTime(t) {
  if (!t) return ''
  const date = new Date(t)
  return date.toLocaleString('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}
</script>

<style scoped>
.bell-wrap { position: relative; }
.bell-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  width: 340px;
  background: #fff;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  z-index: 60;
  overflow: hidden;
}
.bell-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 16px;
  border-bottom: 1px solid var(--color-border-soft);
  font-size: var(--fs-sm);
}
.bell-body {
  max-height: 380px;
  overflow-y: auto;
}
.bell-item {
  display: flex;
  gap: 10px;
  padding: 12px 16px;
  cursor: pointer;
  border-bottom: 1px solid var(--color-border-soft);
  position: relative;
}
.bell-item:hover { background: var(--color-bg); }
.bell-dot {
  width: 8px; height: 8px;
  border-radius: 999px;
  background: transparent;
  margin-top: 6px;
  flex-shrink: 0;
}
.bell-item.unread .bell-dot { background: var(--color-accent-red); }
.bell-content { flex: 1; min-width: 0; }
.bell-title { font-size: var(--fs-sm); font-weight: 600; color: var(--color-text); }
.bell-text {
  font-size: var(--fs-xs);
  color: var(--color-text-soft);
  margin-top: 2px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.bell-time {
  font-size: var(--fs-xs);
  color: var(--color-text-muted);
  margin-top: 4px;
}
.bell-footer {
  padding: 10px 16px;
  text-align: center;
  border-top: 1px solid var(--color-border-soft);
}
.bell-footer a { font-size: var(--fs-sm); }
.bell-mask {
  position: fixed;
  inset: 0;
  z-index: 55;
}
</style>
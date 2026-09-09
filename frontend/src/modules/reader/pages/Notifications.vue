<template>
  <div class="rd-page">
    <PageHeader title="我的消息" subtitle="系统通知、借阅提醒、预约确认实时送达">
      <template #actions>
        <el-button :disabled="!notificationStore.unreadCount" @click="markAllRead">
          <el-icon><Check /></el-icon> 全部已读
        </el-button>
      </template>
    </PageHeader>

    <div class="card">
      <el-tabs v-model="tab">
        <el-tab-pane :label="`全部 (${notificationStore.list.length})`" name="all" />
        <el-tab-pane :label="`未读 (${notificationStore.unreadCount})`" name="unread" />
      </el-tabs>

      <div v-if="loading"><el-skeleton :rows="5" animated /></div>
      <EmptyState v-else-if="!filtered.length" title="没有消息" />
      <div v-else class="notif-list">
        <div v-for="n in filtered" :key="n.NotificationId" class="notif-card" :class="{ unread: n.IsRead === 'N' }" @click="onRead(n)">
          <div class="type-icon">
            <el-icon :size="20">
              <component :is="iconForType(n.Type)" />
            </el-icon>
          </div>
          <div class="notif-body">
            <div class="head">
              <span class="title">{{ n.Title }}</span>
              <el-tag v-if="n.Priority === 'high'" type="danger" size="small" effect="light">紧急</el-tag>
              <span class="time">{{ formatTime(n.CreateTime) }}</span>
            </div>
            <div class="content">{{ n.Content }}</div>
          </div>
          <div v-if="n.IsRead === 'N'" class="unread-dot" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useNotificationStore } from '@/stores/notification'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const notificationStore = useNotificationStore()
const tab = ref('all')
const loading = ref(false)

const filtered = computed(() => {
  if (tab.value === 'unread') return notificationStore.list.filter((n) => n.IsRead === 'N')
  return notificationStore.list
})

const iconMap = {
  borrow: 'Reading',
  seat: 'OfficeBuilding',
  reservation: 'Calendar',
  fine: 'Money',
  recommend: 'Promotion',
  system: 'Bell',
  announcement: 'BellFilled'
}
function iconForType(t) { return iconMap[t] || 'InfoFilled' }

async function load() {
  loading.value = true
  try { await notificationStore.fetchList() }
  finally { loading.value = false }
}

async function onRead(n) {
  if (n.IsRead === 'N') {
    await notificationStore.markRead(n.NotificationId)
  }
}

async function markAllRead() {
  await notificationStore.markAllRead()
  ElMessage.success('已全部标记为已读')
}

function formatTime(t) {
  if (!t) return ''
  return new Date(t).toLocaleString('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}

onMounted(load)
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
.notif-list { display: flex; flex-direction: column; gap: 10px; }
.notif-card {
  display: flex;
  gap: 14px;
  padding: 16px;
  background: var(--color-bg);
  border-radius: var(--radius-md);
  cursor: pointer;
  align-items: flex-start;
  position: relative;
  transition: background .2s;
}
.notif-card:hover { background: var(--color-primary-50); }
.notif-card.unread { background: #fff; border: 1px solid var(--color-primary-200); }
.type-icon {
  width: 40px;
  height: 40px;
  border-radius: 999px;
  background: var(--color-primary-100);
  color: var(--color-primary-600);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.notif-body { flex: 1; min-width: 0; }
.notif-body .head { display: flex; align-items: center; gap: 8px; margin-bottom: 4px; }
.notif-body .title { font-weight: 600; }
.notif-body .time { margin-left: auto; font-size: var(--fs-xs); color: var(--color-text-muted); }
.notif-body .content { font-size: var(--fs-sm); color: var(--color-text-soft); line-height: 1.6; }
.unread-dot {
  position: absolute;
  top: 18px;
  right: 18px;
  width: 8px;
  height: 8px;
  border-radius: 999px;
  background: var(--color-accent-red);
}
</style>
<template>
  <div class="rd-page">
    <PageHeader :title="`你好,${userStore.nickName || userStore.userName}`" :subtitle="`信用分 ${userStore.creditScore} · ${userStore.accountStatus}`">
      <template #actions>
        <el-button @click="$router.push('/')"><el-icon><HomeFilled /></el-icon> 返回首页</el-button>
      </template>
    </PageHeader>

    <!-- 概览统计 -->
    <div class="stat-row">
      <StatCard label="未归还图书" :value="stats.unreturnedCount" suffix="本" icon="Reading" accent="primary" hint="及时归还避免逾期" />
      <StatCard label="逾期图书" :value="stats.overdueCount" suffix="本" icon="WarningFilled" accent="red" hint="请尽快归还" />
      <StatCard label="待缴罚款" :value="stats.pendingFineAmount" suffix="元" icon="Money" accent="orange" hint="逾期产生" />
      <StatCard label="未读消息" :value="stats.unreadNotification" suffix="条" icon="Bell" accent="cyan" hint="实时通知推送" />
    </div>

    <!-- 快捷操作 -->
    <div class="quick-row card">
      <h3>快捷操作</h3>
      <div class="quick-grid">
        <button class="qb" @click="$router.push('/reader/borrowing')">
          <el-icon :size="24"><Tickets /></el-icon>
          <span>我的借阅</span>
        </button>
        <button class="qb" @click="$router.push('/reader/reserves')">
          <el-icon :size="24"><Calendar /></el-icon>
          <span>图书预约</span>
        </button>
        <button class="qb" @click="$router.push('/reader/seat')">
          <el-icon :size="24"><OfficeBuilding /></el-icon>
          <span>座位预约</span>
        </button>
        <button class="qb" @click="$router.push('/reader/recommends')">
          <el-icon :size="24"><Promotion /></el-icon>
          <span>图书荐购</span>
        </button>
        <button class="qb" @click="$router.push('/reader/favorites')">
          <el-icon :size="24"><Star /></el-icon>
          <span>我的收藏</span>
        </button>
        <button class="qb" @click="$router.push('/reader/fines')">
          <el-icon :size="24"><Money /></el-icon>
          <span>缴纳罚款</span>
        </button>
      </div>
    </div>

    <!-- 双栏:推荐图书 + 通知 -->
    <div class="dual-col">
      <div class="card">
        <div class="card-head">
          <h3>为你推荐</h3>
          <span class="hint">基于你的阅读偏好</span>
        </div>
        <div v-if="loading.rec" class="loading-block"><el-skeleton :rows="3" animated /></div>
        <EmptyState v-else-if="!recommendations.length" title="暂无推荐" description="借阅几本书后我们会为你生成个性化推荐" />
        <div v-else class="rec-list">
          <div v-for="b in recommendations.slice(0, 5)" :key="b.ISBN" class="rec-item" @click="$router.push(`/book/${b.ISBN}`)">
            <BookCover :title="b.Title" :author="b.Author" :isbn="b.ISBN" />
            <div class="rec-info">
              <div class="rec-title">{{ b.Title }}</div>
              <div class="rec-author">{{ b.Author }}</div>
              <div class="rec-pub">{{ b.Publisher }} · {{ b.PublishYear }}</div>
            </div>
          </div>
        </div>
      </div>

      <div class="card">
        <div class="card-head">
          <h3>最新通知</h3>
          <router-link to="/reader/notifications" class="more">查看全部 →</router-link>
        </div>
        <div v-if="loading.notif" class="loading-block"><el-skeleton :rows="3" animated /></div>
        <EmptyState v-else-if="!notifications.length" title="暂无新通知" />
        <div v-else class="notif-list">
          <div v-for="n in notifications.slice(0, 5)" :key="n.NotificationId" class="notif-item" :class="{ unread: n.IsRead === 'N' }">
            <div class="dot" />
            <div class="body">
              <div class="title">{{ n.Title }}</div>
              <div class="text">{{ n.Content }}</div>
              <div class="time">{{ formatTime(n.CreateTime) }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 我的书单 -->
    <div class="card">
      <div class="card-head">
        <h3>我的书单</h3>
        <router-link to="/reader/booklist" class="more">管理 →</router-link>
      </div>
      <EmptyState v-if="!booklists.length" title="还没有书单">
        <el-button type="primary" @click="$router.push('/reader/booklist')">创建书单</el-button>
      </EmptyState>
      <div v-else class="booklist-grid">
        <div v-for="bl in booklists.slice(0, 4)" :key="bl.BooklistID" class="bl-card">
          <div class="bl-cover">
            <el-icon :size="32"><Notebook /></el-icon>
          </div>
          <div class="bl-info">
            <div class="bl-name">{{ bl.BooklistName }}</div>
            <div class="bl-desc">{{ bl.BooklistIntroduction }}</div>
            <div class="bl-meta">{{ bl.BookCount }} 本 · {{ bl.CollectorCount }} 人收藏</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useUserStore } from '@/stores/user'
import { authApi } from '@/api/auth'
import { bookApi } from '@/api/book'
import { borrowApi } from '@/api/borrow'
import { fineApi } from '@/api/fine'
import { useNotificationStore } from '@/stores/notification'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatCard from '@/shared/components/StatCard.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import BookCover from '@/shared/components/BookCover.vue'

const userStore = useUserStore()
const notificationStore = useNotificationStore()

const stats = reactive({
  unreturnedCount: 0,
  overdueCount: 0,
  pendingFineAmount: 0,
  unreadNotification: 0,
  activeReserves: 0,
  favoriteCount: 0,
  creditScore: 100
})
const recommendations = ref([])
const notifications = ref([])
const booklists = ref([])
const loading = reactive({ rec: true, notif: true })

function formatTime(t) {
  if (!t) return ''
  return new Date(t).toLocaleString('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}

onMounted(async () => {
  // 并行加载
  const tasks = [
    borrowApi.unreturnedCount(userStore.user?.ReaderID || 1).then((v) => { stats.unreturnedCount = v || 0 }).catch(() => {}),
    borrowApi.overdueCount(userStore.user?.ReaderID || 1).then((v) => { stats.overdueCount = v || 0 }).catch(() => {}),
    fineApi.mySummary().then((d) => { stats.pendingFineAmount = d?.TotalAmount || 0 }).catch(() => {}),
    bookApi.favorites().then((list) => { stats.favoriteCount = list?.length || 0 }).catch(() => {}),
    notificationStore.fetchList().then(() => {
      notifications.value = notificationStore.list
      stats.unreadNotification = notificationStore.unreadCount
    }).catch(() => {}),
    authApi.myRecommendations().then((r) => { recommendations.value = Array.isArray(r) ? r : [] }).catch(() => {}).finally(() => { loading.rec = false }),
    bookApi.myBooklists().then((d) => { const list = d?.data ?? d; booklists.value = Array.isArray(list) ? list : (list?.Created || list?.Items || []) }).catch(() => {}).finally(() => { /* noop */ })
  ]
  loading.notif = false
  await Promise.allSettled(tasks)
})
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 20px; }
.stat-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
}
.quick-row h3 { margin: 0 0 16px; font-size: var(--fs-md); }
.quick-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 12px;
}
.qb {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 20px 8px;
  border-radius: var(--radius-md);
  background: var(--color-bg);
  border: none;
  cursor: pointer;
  font-size: var(--fs-sm);
  color: var(--color-text-soft);
  transition: all .2s;
}
.qb:hover { background: var(--color-primary-50); color: var(--color-primary-600); transform: translateY(-2px); }

.dual-col {
  display: grid;
  grid-template-columns: 1.4fr 1fr;
  gap: 16px;
}
.card-head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-bottom: 16px;
}
.card-head h3 { margin: 0; font-size: var(--fs-md); }
.hint { font-size: var(--fs-xs); color: var(--color-text-muted); }
.more { font-size: var(--fs-sm); color: var(--color-primary-600); }

.rec-list, .notif-list { display: flex; flex-direction: column; gap: 10px; }
.rec-item {
  display: flex;
  gap: 12px;
  padding: 10px;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: background .2s;
}
.rec-item:hover { background: var(--color-bg); }
.rec-info { flex: 1; min-width: 0; }
.rec-title { font-size: var(--fs-sm); font-weight: 600; margin-bottom: 2px; }
.rec-author { font-size: var(--fs-xs); color: var(--color-text-soft); }
.rec-pub { font-size: var(--fs-xs); color: var(--color-text-muted); margin-top: 2px; }
.rec-info > div { white-space: nowrap; text-overflow: ellipsis; overflow: hidden; }

.notif-item { display: flex; gap: 10px; padding: 12px; border-radius: var(--radius-md); }
.notif-item.unread { background: var(--color-primary-50); }
.notif-item .dot { width: 8px; height: 8px; border-radius: 999px; background: transparent; margin-top: 6px; flex-shrink: 0; }
.notif-item.unread .dot { background: var(--color-accent-red); }
.notif-item .body { flex: 1; min-width: 0; }
.notif-item .title { font-size: var(--fs-sm); font-weight: 600; }
.notif-item .text { font-size: var(--fs-xs); color: var(--color-text-soft); margin-top: 2px; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.notif-item .time { font-size: var(--fs-xs); color: var(--color-text-muted); margin-top: 4px; }

.booklist-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 12px; }
.bl-card { display: flex; gap: 12px; padding: 12px; background: var(--color-bg); border-radius: var(--radius-md); }
.bl-cover { width: 56px; height: 72px; background: linear-gradient(135deg, #3B5BDB, #748FFC); color: #fff; border-radius: var(--radius-sm); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.bl-name { font-size: var(--fs-sm); font-weight: 600; }
.bl-desc { font-size: var(--fs-xs); color: var(--color-text-soft); margin: 4px 0; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.bl-meta { font-size: var(--fs-xs); color: var(--color-text-muted); }

.loading-block { padding: 16px 0; }

@media (max-width: 1024px) {
  .stat-row { grid-template-columns: repeat(2, 1fr); }
  .quick-grid { grid-template-columns: repeat(3, 1fr); }
  .dual-col { grid-template-columns: 1fr; }
}
</style>
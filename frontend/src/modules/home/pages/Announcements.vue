<template>
  <div class="announce-page">
    <PageHeader title="系统公告" subtitle="开馆通知、活动公告、维护通知" />
    <el-tabs v-model="activeTab" class="announce-tabs">
      <el-tab-pane label="全部" name="all" />
      <el-tab-pane label="紧急" name="urgent" />
      <el-tab-pane label="读者" name="reader" />
      <el-tab-pane label="管理员" name="admin" />
    </el-tabs>

    <div v-if="loading"><el-skeleton :rows="5" animated /></div>
    <EmptyState v-else-if="!filtered.length" title="暂无公告" />
    <div v-else class="announce-grid">
      <div v-for="a in filtered" :key="a.AnnouncementID" class="announce-card" @click="open(a)">
        <div class="ac-head">
          <span v-if="a.Title.includes('紧急')" class="badge urgent">紧急</span>
          <span class="badge">{{ a.TargetGroup }}</span>
          <span class="ac-time">{{ formatTime(a.CreateTime) }}</span>
        </div>
        <h3 class="ac-title">{{ a.Title }}</h3>
        <p class="ac-text">{{ a.Content }}</p>
      </div>
    </div>

    <el-dialog v-model="dialogVisible" :title="current?.Title" width="640px">
      <div v-if="current">
        <div class="meta-bar">
          <span>{{ formatTime(current.CreateTime) }}</span>
          <span>面向: {{ current.TargetGroup }}</span>
        </div>
        <div class="dialog-content">{{ current.Content }}</div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { announcementApi } from '@/api/announcement'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const activeTab = ref('all')
const loading = ref(true)
const urgent = ref([])
const regular = ref([])
const dialogVisible = ref(false)
const current = ref(null)

const filtered = computed(() => {
  let list = []
  if (activeTab.value === 'urgent') list = urgent.value
  else if (activeTab.value === 'reader') list = [...urgent.value, ...regular.value].filter((a) => a.TargetGroup === '读者' || a.TargetGroup === '所有人')
  else if (activeTab.value === 'admin') list = [...urgent.value, ...regular.value].filter((a) => a.TargetGroup === '管理员' || a.TargetGroup === '所有人')
  else list = [...urgent.value, ...regular.value]
  return list
})

function open(a) { current.value = a; dialogVisible.value = true }
function formatTime(t) { return t ? new Date(t).toLocaleString('zh-CN') : '' }

onMounted(async () => {
  try {
    const data = await announcementApi.public()
    urgent.value = data?.Urgent || []
    regular.value = data?.Regular || []
  } finally { loading.value = false }
})
</script>

<style scoped>
.announce-page { padding: 32px 24px; max-width: 1100px; margin: 0 auto; }
.announce-tabs { margin-bottom: 24px; background: #fff; padding: 8px 16px; border-radius: var(--radius-md); }
.announce-grid { display: grid; gap: 16px; }
.announce-card {
  background: #fff;
  border-radius: var(--radius-lg);
  padding: 20px 24px;
  cursor: pointer;
  transition: transform .2s, box-shadow .2s;
  border: 1px solid var(--color-border-soft);
}
.announce-card:hover { transform: translateY(-2px); box-shadow: var(--shadow-md); }
.ac-head { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
.badge {
  background: var(--color-primary-50);
  color: var(--color-primary-600);
  font-size: var(--fs-xs);
  padding: 2px 10px;
  border-radius: 999px;
  font-weight: 500;
}
.badge.urgent { background: #FFE3E3; color: var(--color-accent-red); }
.ac-time { font-size: var(--fs-xs); color: var(--color-text-muted); margin-left: auto; }
.ac-title { margin: 4px 0; font-size: var(--fs-lg); }
.ac-text { color: var(--color-text-soft); margin: 0; font-size: var(--fs-sm); display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.meta-bar { display: flex; gap: 16px; color: var(--color-text-muted); font-size: var(--fs-sm); margin-bottom: 16px; padding-bottom: 12px; border-bottom: 1px solid var(--color-border-soft); }
.dialog-content { line-height: 1.8; color: var(--color-text); white-space: pre-wrap; }
</style>
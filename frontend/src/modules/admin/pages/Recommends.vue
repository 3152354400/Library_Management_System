<template>
  <div class="ad-page">
    <PageHeader title="荐购处理" subtitle="审核读者推荐的图书,决定是否纳入采购计划" />

    <div class="card">
      <el-tabs v-model="tab">
        <el-tab-pane label="待审核" name="待审核" />
        <el-tab-pane label="已采纳" name="已采纳" />
        <el-tab-pane label="全部" name="all" />
      </el-tabs>

      <el-table :data="filtered" v-loading="loading" stripe>
        <el-table-column prop="Title" label="书名" min-width="180" />
        <el-table-column prop="Author" label="作者" width="120" />
        <el-table-column prop="Publisher" label="出版社" width="160" />
        <el-table-column prop="PublishYear" label="年份" width="80" />
        <el-table-column prop="Reason" label="推荐理由" min-width="240" show-overflow-tooltip />
        <el-table-column label="推荐时间" width="160">
          <template #default="{ row }">{{ formatDate(row.RecommendTime) }}</template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }"><StatusTag :status="row.Status" /></template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.Status === '待审核'" text type="success" @click="onHandle(row, '采纳')"><el-icon><Check /></el-icon> 采纳</el-button>
            <el-button v-if="row.Status === '待审核'" text type="danger" @click="onHandle(row, '拒绝')"><el-icon><Close /></el-icon> 拒绝</el-button>
          </template>
        </el-table-column>
      </el-table>
      <EmptyState v-if="!loading && !filtered.length" title="没有荐购记录" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { recommendApi } from '@/api/recommend'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const tab = ref('待审核')
const list = ref([])
const loading = ref(false)

const filtered = computed(() => {
  if (tab.value === 'all') return list.value
  return list.value.filter((r) => r.Status === tab.value)
})

async function load() {
  loading.value = true
  try { list.value = await recommendApi.all() }
  finally { loading.value = false }
}

async function onHandle(row, action) {
  const { value } = await ElMessageBox.prompt(`处理意见 / 备注`, `${action}《${row.Title}》`, {
    confirmButtonText: '确认',
    cancelButtonText: '取消'
  }).catch(() => ({ value: '' }))
  if (value === undefined) return
  try {
    const r = await recommendApi.handle({ RecommendId: row.RecommendId, Action: action, HandleResult: value })
    if (r?.message && typeof r.message === 'string' && r.message.startsWith('ERROR')) {
      ElMessage.error(r.message)
      return
    }
    ElMessage.success('已处理')
    load()
  } catch (err) {
    ElMessage.error(err?.response?.data?.message || '处理失败，请稍后再试')
  }
}

function formatDate(t) { return t ? new Date(t).toLocaleDateString('zh-CN') : '' }
onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
</style>
<template>
  <div class="ad-page">
    <PageHeader title="评论举报" subtitle="审核违规评论,处理读者举报" />

    <div class="card">
      <el-table :data="reports" v-loading="loading" stripe>
        <el-table-column label="举报人" prop="ReporterUsername" width="140" />
        <el-table-column label="被举报评论" min-width="280">
          <template #default="{ row }">
            <div>
              <div class="t">{{ row.ReviewContent }}</div>
              <div class="sub">来自《{{ row.BookTitle }}》</div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="评分" width="100">
          <template #default="{ row }">
            <el-rate :model-value="row.Rating" disabled :max="5" />
          </template>
        </el-table-column>
        <el-table-column prop="ReportReason" label="举报理由" min-width="200" show-overflow-tooltip />
        <el-table-column label="评论人" width="140">
          <template #default="{ row }">
            <div>
              <div class="t">{{ row.CommenterNickname }}</div>
              <StatusTag v-if="row.CommenterStatus" :status="row.CommenterStatus" />
            </div>
          </template>
        </el-table-column>
        <el-table-column label="举报时间" width="160">
          <template #default="{ row }">{{ formatDate(row.ReportTime) }}</template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }"><StatusTag :status="row.ReportStatus" /></template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button text type="success" @click="onHandle(row, '处理完成')">通过</el-button>
            <el-button text type="danger" @click="onHandle(row, '驳回', true)">驳回并封禁</el-button>
          </template>
        </el-table-column>
      </el-table>
      <EmptyState v-if="!loading && !reports.length" title="没有待处理举报" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { adminApi } from '@/api/admin'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const reports = ref([])
const loading = ref(false)

async function load() {
  loading.value = true
  try { reports.value = await adminApi.pendingReports() }
  finally { loading.value = false }
}

async function onHandle(row, action, banUser = false) {
  await adminApi.handleReport(row.ReportID, { Action: action, BanUser: banUser })
  ElMessage.success(`已${action}`)
  load()
}

function formatDate(t) { return t ? new Date(t).toLocaleDateString('zh-CN') : '' }
onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.t { font-weight: 600; font-size: var(--fs-sm); }
.sub { font-size: var(--fs-xs); color: var(--color-text-muted); margin-top: 2px; }
</style>
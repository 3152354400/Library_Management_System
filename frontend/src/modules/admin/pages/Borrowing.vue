<template>
  <div class="ad-page">
    <PageHeader title="借阅记录" subtitle="查看全部借阅记录,支持借出 / 归还操作" />

    <div class="card">
      <div class="filter-bar">
        <el-input v-model="keyword" placeholder="读者 / 书名 / 条码" clearable style="width:280px" :prefix-icon="Search" />
        <el-select v-model="status" placeholder="状态" clearable style="width:140px">
          <el-option value="未归还" label="未归还" />
          <el-option value="已归还" label="已归还" />
        </el-select>
        <el-button type="primary" :icon="Search" @click="load">查询</el-button>
      </div>

      <el-table :data="records" v-loading="loading" stripe>
        <el-table-column label="读者" width="140">
          <template #default="{ row }">{{ row.Fullname || row.Username }}</template>
        </el-table-column>
        <el-table-column label="书名" min-width="200" prop="Title" />
        <el-table-column label="条码" prop="Barcode" width="120" />
        <el-table-column label="借出时间" width="160">
          <template #default="{ row }">{{ formatDate(row.BorrowTime) }}</template>
        </el-table-column>
        <el-table-column label="应还时间" width="160">
          <template #default="{ row }">{{ formatDate(row.DueTime) }}</template>
        </el-table-column>
        <el-table-column label="归还时间" width="160">
          <template #default="{ row }">{{ row.ReturnTime ? formatDate(row.ReturnTime) : '—' }}</template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }"><StatusTag :status="row.BorrowStatus" /></template>
        </el-table-column>
        <el-table-column label="罚款" width="100">
          <template #default="{ row }">
            <span v-if="row.OverdueFine > 0" class="text-danger">¥{{ row.OverdueFine }}</span>
            <span v-else>—</span>
          </template>
        </el-table-column>
      </el-table>

      <EmptyState v-if="!loading && !records.length" title="暂无借阅记录" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Search } from '@element-plus/icons-vue'
import { borrowApi } from '@/api/borrow'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const keyword = ref('')
const status = ref('')
const records = ref([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    let data = await borrowApi.all()
    records.value = data || []
    if (status.value) records.value = records.value.filter((r) => r.BorrowStatus === status.value)
    if (keyword.value) {
      const kw = keyword.value.toLowerCase()
      records.value = records.value.filter((r) => (r.Title || '').toLowerCase().includes(kw) || (r.Username || '').toLowerCase().includes(kw) || (r.Fullname || '').toLowerCase().includes(kw))
    }
  } finally { loading.value = false }
}

function formatDate(t) { return t ? new Date(t).toLocaleDateString('zh-CN') : '—' }
onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.filter-bar { display: flex; gap: 12px; margin-bottom: 16px; }
</style>
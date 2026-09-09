<template>
  <div class="rd-page">
    <PageHeader title="我的罚款" subtitle="查看与缴纳因逾期或损坏产生的罚款" />

    <div class="stat-row">
      <StatCard label="待缴罚款" :value="summary.UnpaidAmount || summary.unpaidAmount || 0" suffix="元" icon="Money" accent="orange" />
      <StatCard label="待缴笔数" :value="summary.UnpaidFines || summary.unpaidFines || 0" suffix="笔" icon="Tickets" accent="red" />
      <StatCard label="已缴笔数" :value="(summary.TotalFines || 0) - (summary.UnpaidFines || 0)" suffix="笔" icon="Check" accent="green" />
    </div>

    <div class="card">
      <el-tabs v-model="tab">
        <el-tab-pane label="待缴纳" name="pending" />
        <el-tab-pane label="已缴纳" name="paid" />
      </el-tabs>

      <el-table :data="filtered" v-loading="loading" stripe>
        <el-table-column prop="FineID" label="编号" width="80" />
        <el-table-column prop="Remark" label="罚款原因" min-width="280" />
        <el-table-column label="金额" width="120">
          <template #default="{ row }">¥{{ row.Amount }}</template>
        </el-table-column>
        <el-table-column label="状态" width="120">
          <template #default="{ row }">
            <StatusTag :status="row.PayStatus" />
          </template>
        </el-table-column>
        <el-table-column label="生成时间" width="180">
          <template #default="{ row }">{{ formatDate(row.FineDate) }}</template>
        </el-table-column>
        <el-table-column label="缴费时间" width="180">
          <template #default="{ row }">{{ row.PayTime ? formatDate(row.PayTime) : '—' }}</template>
        </el-table-column>
      </el-table>

      <EmptyState v-if="!loading && !filtered.length" title="没有罚款记录" description="保持良好借阅习惯,远离罚款" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { fineApi } from '@/api/fine'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatCard from '@/shared/components/StatCard.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const tab = ref('pending')
const loading = ref(false)
const list = ref([])
const summary = ref({})

const filtered = computed(() => {
  if (tab.value === 'pending') return list.value.filter((f) => f.PayStatus === '未缴纳')
  return list.value.filter((f) => f.PayStatus === '已缴纳')
})

async function load() {
  loading.value = true
  try {
    const [listData, summaryData] = await Promise.all([fineApi.myList(), fineApi.mySummary()])
    list.value = Array.isArray(listData) ? listData : []
    summary.value = summaryData || {}
  } finally { loading.value = false }
}

function formatDate(t) { return t ? new Date(t).toLocaleString('zh-CN') : '' }
onMounted(load)
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
.stat-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; }
@media (max-width: 768px) { .stat-row { grid-template-columns: 1fr; } }
</style>
<template>
  <div class="ad-page">
    <PageHeader title="罚款管理" subtitle="查看读者待缴罚款、办理缴纳与减免申请" />

    <div class="stat-row">
      <StatCard label="待缴笔数" :value="pending.length" suffix="笔" icon="Tickets" accent="orange" />
      <StatCard label="待缴总额" :value="totalAmount" suffix="元" icon="Money" accent="red" />
    </div>

    <div class="card">
      <el-table :data="pending" v-loading="loading" stripe>
        <el-table-column label="读者" prop="ReaderName" width="140" />
        <el-table-column label="原因" prop="Reason" min-width="280" />
        <el-table-column label="金额" width="120">
          <template #default="{ row }">¥{{ row.Amount }}</template>
        </el-table-column>
        <el-table-column label="状态" width="120">
          <template #default="{ row }"><StatusTag :status="row.Status" /></template>
        </el-table-column>
        <el-table-column label="生成时间" width="180">
          <template #default="{ row }">{{ formatDate(row.CreateTime) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button text type="primary" @click="onPay(row)"><el-icon><Money /></el-icon> 缴纳</el-button>
            <el-button text type="warning" @click="onWaive(row)"><el-icon><Discount /></el-icon> 减免</el-button>
          </template>
        </el-table-column>
      </el-table>
      <EmptyState v-if="!loading && !pending.length" title="没有待缴罚款" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { fineApi } from '@/api/fine'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatCard from '@/shared/components/StatCard.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const pending = ref([])
const loading = ref(false)

const totalAmount = computed(() => pending.value.reduce((s, p) => s + Number(p.Amount || 0), 0).toFixed(2))

async function load() {
  loading.value = true
  try { pending.value = await fineApi.pending() }
  finally { loading.value = false }
}

async function onPay(row) {
  await ElMessageBox.prompt(`读者 ${row.ReaderName} 的罚款 ¥${row.Amount}`, '确认缴纳', {
    confirmButtonText: '确认已收',
    cancelButtonText: '取消',
    inputPlaceholder: '支付方式(现金/微信/支付宝)'
  }).catch(() => {})
  await fineApi.pay({ FineId: row.FineID, PayAmount: row.Amount, PayMethod: '现金' })
  ElMessage.success('已登记缴纳')
  load()
}

async function onWaive(row) {
  const { value: reason } = await ElMessageBox.prompt('请输入减免理由', '减免罚款', { confirmButtonText: '确认减免', cancelButtonText: '取消', inputPlaceholder: '理由' }).catch(() => ({ value: '' }))
  if (!reason) return
  await fineApi.waive({ FineId: row.FineID, Reason: reason })
  ElMessage.success('已减免')
  load()
}

function formatDate(t) { return t ? new Date(t).toLocaleString('zh-CN') : '' }
onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.stat-row { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
</style>
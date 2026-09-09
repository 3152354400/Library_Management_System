<template>
  <div class="ad-page">
    <PageHeader title="丢书上报" subtitle="处理读者上报的图书遗失/损坏报告" />

    <div class="card">
      <el-tabs v-model="tab">
        <el-tab-pane label="待处理" name="待处理" />
        <el-tab-pane label="已确认" name="已确认" />
        <el-tab-pane label="已赔偿" name="已赔偿" />
        <el-tab-pane label="全部" name="all" />
      </el-tabs>

      <el-table :data="filtered" v-loading="loading" stripe>
        <el-table-column label="读者" prop="ReaderName" width="120" />
        <el-table-column label="图书" min-width="200">
          <template #default="{ row }">
            <div>
              <div class="t">{{ row.BookTitle }}</div>
              <div class="i">ISBN: {{ row.ISBN }}</div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="类型" prop="ReportType" width="100" />
        <el-table-column label="描述" prop="Description" min-width="220" show-overflow-tooltip />
        <el-table-column label="估值" width="100">
          <template #default="{ row }">¥{{ row.EstimatedValue || 0 }}</template>
        </el-table-column>
        <el-table-column label="赔偿" width="120">
          <template #default="{ row }">
            <span v-if="row.CompensationAmount" class="text-danger">¥{{ row.CompensationAmount }}</span>
            <span v-else>—</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }"><StatusTag :status="row.Status" /></template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.Status === '待处理'" text type="primary" @click="onConfirm(row)">确认赔偿</el-button>
            <el-button v-if="row.Status === '已确认'" text type="success" @click="onCompensate(row)">标记已赔偿</el-button>
          </template>
        </el-table-column>
      </el-table>
      <EmptyState v-if="!loading && !filtered.length" title="没有丢书记录" />
    </div>

    <el-dialog v-model="confirmDialog" title="确认赔偿" width="460px">
      <el-form :model="confirmForm" label-width="100px">
        <el-form-item label="赔偿金额"><el-input-number v-model="confirmForm.Amount" :min="0" :precision="2" /></el-form-item>
        <el-form-item label="处理意见"><el-input v-model="confirmForm.Result" type="textarea" :rows="3" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="confirmDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmSubmit">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { adminApi } from '@/api/admin'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const tab = ref('待处理')
const list = ref([])
const loading = ref(false)
const confirmDialog = ref(false)
const confirmTarget = ref(null)
const confirmForm = reactive({ Amount: 0, Result: '' })

const filtered = computed(() => {
  if (tab.value === 'all') return list.value
  return list.value.filter((r) => r.Status === tab.value)
})

async function load() {
  loading.value = true
  try { list.value = await adminApi.bookLossList() }
  finally { loading.value = false }
}

function onConfirm(row) {
  confirmTarget.value = row
  confirmForm.Amount = row.EstimatedValue || 0
  confirmForm.Result = ''
  confirmDialog.value = true
}

async function confirmSubmit() {
  await adminApi.confirmBookLoss(confirmTarget.value.ReportID, { Amount: confirmForm.Amount, Result: confirmForm.Result })
  ElMessage.success('已确认')
  confirmDialog.value = false
  load()
}

async function onCompensate(row) {
  await adminApi.compensateBookLoss(row.ReportID)
  ElMessage.success('已标记为已赔偿')
  load()
}

onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.t { font-weight: 600; font-size: var(--fs-sm); }
.i { font-size: var(--fs-xs); color: var(--color-text-muted); }
</style>
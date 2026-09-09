<template>
  <div class="ad-page">
    <PageHeader title="采购分析" subtitle="基于借阅数据,辅助采购决策" />

    <div class="layout">
      <div class="card rank-card">
        <div class="card-head">
          <h3>借阅频次 TOP10</h3>
          <span class="hint">最近 30 天</span>
        </div>
        <EmptyState v-if="!analysis.TopByBorrowCount?.length" title="暂无数据" />
        <ul v-else class="rank-list">
          <li v-for="(b, i) in analysis.TopByBorrowCount" :key="b.ISBN || i">
            <span class="rk" :class="`rk-${i + 1}`">{{ i + 1 }}</span>
            <div class="info">
              <div class="t">{{ b.Title }}</div>
              <div class="a">{{ b.Author }}</div>
            </div>
            <span class="val">{{ b.MetricValue }} 次</span>
          </li>
        </ul>
      </div>

      <div class="card rank-card">
        <div class="card-head">
          <h3>借阅时长 TOP10</h3>
          <span class="hint">累计借阅天数</span>
        </div>
        <EmptyState v-if="!analysis.TopByBorrowDuration?.length" title="暂无数据" />
        <ul v-else class="rank-list">
          <li v-for="(b, i) in analysis.TopByBorrowDuration" :key="b.ISBN || i">
            <span class="rk" :class="`rk-${i + 1}`">{{ i + 1 }}</span>
            <div class="info">
              <div class="t">{{ b.Title }}</div>
              <div class="a">{{ b.Author }}</div>
            </div>
            <span class="val">{{ b.MetricValue }} 天</span>
          </li>
        </ul>
      </div>

      <div class="card rank-card">
        <div class="card-head">
          <h3>副本被借 TOP10</h3>
          <span class="hint">副本被借次数</span>
        </div>
        <EmptyState v-if="!analysis.TopByInstanceBorrow?.length" title="暂无数据" />
        <ul v-else class="rank-list">
          <li v-for="(b, i) in analysis.TopByInstanceBorrow" :key="b.ISBN || i">
            <span class="rk" :class="`rk-${i + 1}`">{{ i + 1 }}</span>
            <div class="info">
              <div class="t">{{ b.Title }}</div>
              <div class="a">{{ b.Author }}</div>
              <div class="bar">
                <div class="bar-fill" :style="{ width: Math.round((b.MetricValue || 0) / maxInstanceMetric * 100) + '%' }" />
              </div>
            </div>
            <span class="val">{{ b.MetricValue }} 次</span>
          </li>
        </ul>
      </div>
    </div>

    <div class="card">
      <div class="card-head">
        <h3>采购日志</h3>
        <el-button type="primary" :icon="Plus" @click="addDialog = true">新增</el-button>
      </div>
      <el-table :data="logs" v-loading="loadingLogs" stripe>
        <el-table-column prop="LogText" label="日志内容" min-width="400" />
        <el-table-column label="时间" width="180">
          <template #default="{ row }">{{ formatDate(row.LogDate || row.CreateTime) }}</template>
        </el-table-column>
      </el-table>
    </div>

    <el-dialog v-model="addDialog" title="新增采购日志" width="500px">
      <el-input v-model="newLog" type="textarea" :rows="4" placeholder="例如:本月采购XX类图书 100 册..." />
      <template #footer>
        <el-button @click="addDialog = false">取消</el-button>
        <el-button type="primary" @click="onAddLog">提交</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { adminApi } from '@/api/admin'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const analysis = ref({})
const logs = ref([])
const loadingLogs = ref(false)
const addDialog = ref(false)
const newLog = ref('')

// 副本被借排行中最大次数，用于进度条相对宽度
const maxInstanceMetric = computed(() =>
  Math.max(1, ...(analysis.value.TopByInstanceBorrow || []).map((b) => b.MetricValue || 0))
)

async function load() {
  analysis.value = await adminApi.purchaseAnalysis()
  loadingLogs.value = true
  try { logs.value = await adminApi.purchaseLogs() }
  finally { loadingLogs.value = false }
}

async function onAddLog() {
  if (!newLog.value) return ElMessage.warning('请输入内容')
  await adminApi.addPurchaseLog({ LogText: newLog.value })
  ElMessage.success('已添加')
  newLog.value = ''
  addDialog.value = false
  load()
}

function formatDate(t) { return t ? new Date(t).toLocaleDateString('zh-CN') : '' }
onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.layout { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; }
.card-head { display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 12px; }
.card-head h3 { margin: 0; font-size: var(--fs-md); }
.hint { font-size: var(--fs-xs); color: var(--color-text-muted); }
.rank-list { display: flex; flex-direction: column; gap: 8px; }
.rank-list li { display: flex; align-items: center; gap: 12px; padding: 8px 10px; border-radius: var(--radius-sm); }
.rank-list li:hover { background: var(--color-bg); }
.rk { width: 24px; height: 24px; border-radius: 999px; display: flex; align-items: center; justify-content: center; background: var(--color-bg); color: var(--color-text-soft); font-weight: 700; font-size: var(--fs-xs); }
.rk-1 { background: linear-gradient(135deg, #FFD43B, #F59F00); color: #fff; }
.rk-2 { background: linear-gradient(135deg, #CED4DA, #868E96); color: #fff; }
.rk-3 { background: linear-gradient(135deg, #DAA66D, #B06838); color: #fff; }
.info { flex: 1; min-width: 0; }
.info .t { font-size: var(--fs-sm); font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.info .a { font-size: var(--fs-xs); color: var(--color-text-muted); }
.info .bar { height: 4px; background: var(--color-bg); border-radius: 999px; margin-top: 4px; overflow: hidden; }
.bar-fill { height: 100%; background: linear-gradient(90deg, #3B5BDB, #748FFC); border-radius: 999px; }
.val { font-weight: 600; color: var(--color-primary-600); font-size: var(--fs-sm); font-feature-settings: 'tnum'; }
@media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }
</style>
<template>
  <Layout>
    <h1 class="title">📋 图书遗失/损坏赔偿管理</h1>

    <!-- 统计卡片 -->
    <div class="summary-cards">
      <div class="summary-card card-orange">
        <div>待处理</div>
        <div class="count">{{ stats.pending }}</div>
      </div>
      <div class="summary-card card-blue">
        <div>已确认</div>
        <div class="count">{{ stats.confirmed }}</div>
      </div>
      <div class="summary-card card-green">
        <div>已赔偿</div>
        <div class="count">{{ stats.compensated }}</div>
      </div>
    </div>

    <!-- 筛选 -->
    <div class="filter-bar">
      <button
        v-for="f in filters"
        :key="f.value"
        :class="['filter-btn', { active: activeFilter === f.value }]"
        @click="activeFilter = f.value"
      >
        {{ f.label }}
      </button>
    </div>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else-if="filteredReports.length === 0" class="empty">
      <p>暂无相关记录 🎉</p>
    </div>
    <div v-else class="report-list">
      <div v-for="r in filteredReports" :key="r.ReportID" class="report-item">
        <div class="report-header">
          <span :class="['badge', r.ReportType === '遗失' ? 'badge-red' : 'badge-orange']">
            {{ r.ReportType }}
          </span>
          <span class="report-time">{{ formatDate(r.ReportTime) }}</span>
          <span :class="['status-badge', getStatusClass(r.Status)]">{{ r.Status }}</span>
        </div>
        <div class="report-body">
          <div class="report-info">
            <div class="info-row">
              <span class="label">读者：</span>
              <span class="value">{{ r.ReaderName }} ({{ r.ReaderID }})</span>
            </div>
            <div class="info-row">
              <span class="label">图书：</span>
              <span class="value">{{ r.BookTitle }} ({{ r.ISBN }})</span>
            </div>
            <div class="info-row" v-if="r.Description">
              <span class="label">描述：</span>
              <span class="value">{{ r.Description }}</span>
            </div>
          </div>
          <div class="report-amount">
            <div class="amount-label">估值</div>
            <div class="amount-value">¥{{ r.EstimatedValue || '-' }}</div>
            <div class="amount-label">赔偿额</div>
            <div class="amount-value text-red">¥{{ r.CompensationAmount || '-' }}</div>
          </div>
        </div>
        <div class="report-actions" v-if="r.Status === '待处理'">
          <button class="btn-confirm" @click="openConfirm(r)">确认赔偿金额</button>
        </div>
        <div class="report-actions" v-if="r.Status === '已确认'">
          <button class="btn-compensate" @click="markCompensated(r)">确认已赔偿</button>
        </div>
      </div>
    </div>

    <!-- 确认弹窗 -->
    <div v-if="confirmDialog.visible" class="modal-overlay" @click.self="confirmDialog.visible = false">
      <div class="modal-content">
        <button @click="confirmDialog.visible = false" class="close">&times;</button>
        <h3>确认赔偿金额</h3>
        <div class="book-info">
          <div>图书：{{ confirmDialog.report?.BookTitle }}</div>
          <div>报告类型：{{ confirmDialog.report?.ReportType }}</div>
          <div>读者估值：¥{{ confirmDialog.report?.EstimatedValue || '-' }}</div>
        </div>
        <div class="form-group">
          <label>最终赔偿金额</label>
          <input v-model.number="confirmDialog.amount" type="number" step="0.01" min="0" class="form-input" />
        </div>
        <div class="form-group">
          <label>处理说明</label>
          <textarea v-model="confirmDialog.result" rows="3" class="form-input" placeholder="说明处理结果..."></textarea>
        </div>
        <div class="modal-actions">
          <button @click="confirmDialog.visible = false" class="btn-cancel">取消</button>
          <button @click="submitConfirm" :disabled="submitting" class="btn-submit">
            {{ submitting ? '处理中...' : '确认' }}
          </button>
        </div>
      </div>
    </div>
  </Layout>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import http from '@/services/http.js'
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue'

const reports = ref([])
const loading = ref(true)
const activeFilter = ref('all')
const submitting = ref(false)

const stats = reactive({ pending: 0, confirmed: 0, compensated: 0 })

const filters = [
  { label: '全部', value: 'all' },
  { label: '待处理', value: '待处理' },
  { label: '已确认', value: '已确认' },
  { label: '已赔偿', value: '已赔偿' }
]

const filteredReports = computed(() => {
  if (activeFilter.value === 'all') return reports.value
  return reports.value.filter(r => r.Status === activeFilter.value)
})

const confirmDialog = reactive({
  visible: false,
  report: null,
  amount: 0,
  result: ''
})

async function fetchData() {
  loading.value = true
  try {
    const res = await http.get('/admin/book-loss/list', { withToken: true })
    reports.value = res.data || []
    stats.pending = reports.value.filter(r => r.Status === '待处理').length
    stats.confirmed = reports.value.filter(r => r.Status === '已确认').length
    stats.compensated = reports.value.filter(r => r.Status === '已赔偿').length
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

function formatDate(d) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function getStatusClass(status) {
  return {
    '待处理': 'status-pending',
    '已确认': 'status-confirmed',
    '已赔偿': 'status-compensated'
  }[status] || ''
}

function openConfirm(report) {
  confirmDialog.report = report
  confirmDialog.amount = report.EstimatedValue || 0
  confirmDialog.result = ''
  confirmDialog.visible = true
}

async function submitConfirm() {
  try {
    submitting.value = true
    await http.put(`/admin/book-loss/${confirmDialog.report.ReportID}/confirm`,
      { amount: confirmDialog.amount, result: confirmDialog.result },
      { withToken: true }
    )
    confirmDialog.visible = false
    alert('确认成功')
    await fetchData()
  } catch (e) {
    alert(e.response?.data?.message || '操作失败')
  } finally {
    submitting.value = false
  }
}

async function markCompensated(report) {
  if (!confirm(`确定读者已赔偿《${report.BookTitle}》？`)) return
  try {
    await http.put(`/admin/book-loss/${report.ReportID}/compensate`, {},
      { withToken: true }
    )
    alert('已标记为已赔偿')
    await fetchData()
  } catch (e) {
    alert(e.response?.data?.message || '操作失败')
  }
}

onMounted(fetchData)
</script>

<style scoped>
.title { text-align: center; margin-bottom: 24px; color: #2c3e50; font-size: 30px; font-weight: bold; }
.summary-cards { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; margin-bottom: 24px; }
.summary-card { padding: 20px; border-radius: 12px; color: white; text-align: center; }
.summary-card div:first-child { font-size: 14px; opacity: 0.9; }
.summary-card .count { font-size: 32px; font-weight: bold; margin-top: 6px; }
.card-orange { background: linear-gradient(135deg, #fb923c, #ea580c); }
.card-blue { background: linear-gradient(135deg, #4da6ff, #2563eb); }
.card-green { background: linear-gradient(135deg, #34d399, #059669); }
.filter-bar { display: flex; gap: 8px; margin-bottom: 16px; }
.filter-btn { padding: 6px 14px; border: 1px solid #d1d5db; background: white; border-radius: 20px; cursor: pointer; font-size: 13px; }
.filter-btn.active { background: #2563eb; color: white; border-color: #2563eb; }
.loading, .empty { text-align: center; padding: 40px; color: #999; font-size: 16px; }
.report-list { display: flex; flex-direction: column; gap: 16px; }
.report-item { background: white; border-radius: 12px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
.report-header { display: flex; align-items: center; gap: 10px; margin-bottom: 12px; }
.badge { padding: 2px 10px; border-radius: 999px; font-size: 12px; font-weight: 600; }
.badge-red { background: #fee2e2; color: #b91c1c; }
.badge-orange { background: #fed7aa; color: #c2410c; }
.report-time { color: #9ca3af; font-size: 13px; margin-left: auto; }
.status-badge { padding: 2px 10px; border-radius: 999px; font-size: 12px; font-weight: 600; }
.status-pending { background: #fef3c7; color: #92400e; }
.status-confirmed { background: #dbeafe; color: #1e40af; }
.status-compensated { background: #d1fae5; color: #047857; }
.report-body { display: flex; justify-content: space-between; align-items: flex-start; }
.report-info { flex: 1; }
.info-row { display: flex; gap: 8px; font-size: 14px; margin-bottom: 4px; }
.label { color: #6b7280; }
.value { color: #1f2937; }
.report-amount { text-align: right; min-width: 100px; }
.amount-label { font-size: 12px; color: #9ca3af; }
.amount-value { font-size: 20px; font-weight: bold; color: #1f2937; }
.amount-value.text-red { color: #dc2626; }
.report-actions { margin-top: 12px; padding-top: 12px; border-top: 1px solid #f3f4f6; }
.btn-confirm, .btn-compensate { background: #2563eb; color: white; padding: 6px 16px; border: none; border-radius: 6px; cursor: pointer; font-size: 14px; }
.btn-confirm:hover { background: #1d4ed8; }
.btn-compensate { background: #059669; }
.btn-compensate:hover { background: #047857; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-content { background: white; padding: 24px; border-radius: 12px; width: 90%; max-width: 480px; position: relative; }
.close { position: absolute; top: 12px; right: 16px; font-size: 24px; background: none; border: none; cursor: pointer; color: #9ca3af; }
.modal-content h3 { margin: 0 0 16px; font-size: 18px; }
.book-info { background: #f9fafb; padding: 12px; border-radius: 8px; font-size: 14px; margin-bottom: 16px; }
.book-info div { margin: 4px 0; }
.form-group { margin-bottom: 12px; }
.form-group label { display: block; font-size: 14px; font-weight: 500; margin-bottom: 4px; }
.form-input { width: 100%; padding: 8px 12px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; box-sizing: border-box; font-family: inherit; }
.modal-actions { display: flex; gap: 10px; margin-top: 16px; }
.btn-cancel, .btn-submit { flex: 1; padding: 10px; border-radius: 6px; font-size: 15px; font-weight: 600; border: none; cursor: pointer; }
.btn-cancel { background: #6b7280; color: white; }
.btn-submit { background: #2563eb; color: white; }
.btn-submit:disabled { background: #9ca3af; cursor: not-allowed; }
</style>

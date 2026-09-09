<template>
  <Layout>
    <h1 class="title">📖 借阅管理</h1>

    <!-- 标签页切换 -->
    <div class="tabs">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        :class="['tab-btn', { active: activeTab === tab.value }]"
        @click="activeTab = tab.value"
      >
        {{ tab.label }}
      </button>
    </div>

    <!-- 借阅操作区 -->
    <div v-if="activeTab === 'borrow'" class="section">
      <div class="section-header">
        <h2>📖 借阅图书</h2>
        <p class="section-desc">输入读者ID和图书条码进行借阅</p>
      </div>
      <div class="action-form">
        <div class="form-row">
          <div class="form-group">
            <label>读者ID</label>
            <input v-model="borrowForm.readerId" placeholder="输入读者ID" class="form-input" />
          </div>
          <div class="form-group">
            <label>图书条码</label>
            <input v-model="borrowForm.bookId" placeholder="输入图书条码/BookID" class="form-input" />
          </div>
          <button class="btn-action btn-borrow" @click="handleBorrow" :disabled="borrowLoading">
            {{ borrowLoading ? '借阅中...' : '确认借阅' }}
          </button>
        </div>
        <p v-if="borrowError" class="error-text">{{ borrowError }}</p>
        <p v-if="borrowSuccess" class="success-text">{{ borrowSuccess }}</p>
      </div>
    </div>

    <!-- 归还操作区 -->
    <div v-if="activeTab === 'return'" class="section">
      <div class="section-header">
        <h2>📚 归还图书</h2>
        <p class="section-desc">输入读者ID和图书条码进行归还</p>
      </div>
      <div class="action-form">
        <div class="form-row">
          <div class="form-group">
            <label>读者ID</label>
            <input v-model="returnForm.readerId" placeholder="输入读者ID" class="form-input" />
          </div>
          <div class="form-group">
            <label>图书条码</label>
            <input v-model="returnForm.bookId" placeholder="输入图书条码/BookID" class="form-input" />
          </div>
          <button class="btn-action btn-return" @click="handleReturn" :disabled="returnLoading">
            {{ returnLoading ? '归还中...' : '确认归还' }}
          </button>
        </div>
        <p v-if="returnError" class="error-text">{{ returnError }}</p>
        <p v-if="returnSuccess" class="success-text">{{ returnSuccess }}</p>
      </div>
    </div>

    <!-- 批量借阅区 -->
    <div v-if="activeTab === 'batch'" class="section">
      <div class="section-header">
        <h2>📦 批量操作</h2>
        <p class="section-desc">支持批量借阅/批量归还，多个条码用逗号或换行分隔</p>
      </div>

      <div class="batch-form">
        <div class="form-group">
          <label>读者ID</label>
          <input v-model="batchForm.readerId" placeholder="输入读者ID" class="form-input" />
        </div>
        <div class="form-group">
          <label>图书条码（批量）</label>
          <textarea v-model="batchForm.bookIds" rows="5" placeholder="输入多个图书条码，每行一个或用逗号分隔" class="form-textarea"></textarea>
        </div>
        <div class="form-group">
          <label>操作类型</label>
          <div class="radio-group">
            <label class="radio-label">
              <input type="radio" v-model="batchForm.action" value="borrow" />
              批量借阅
            </label>
            <label class="radio-label">
              <input type="radio" v-model="batchForm.action" value="return" />
              批量归还
            </label>
          </div>
        </div>
        <button class="btn-action btn-batch" @click="handleBatch" :disabled="batchLoading">
          {{ batchLoading ? '处理中...' : '执行批量操作' }}
        </button>
        <p v-if="batchError" class="error-text">{{ batchError }}</p>
        <p v-if="batchResult.length" class="result-text">
          批量操作结果：
          <span v-for="(r, i) in batchResult" :key="i" :class="r.success ? 'text-green' : 'text-red'">
            {{ r.message }} {{ i < batchResult.length - 1 ? '；' : '' }}
          </span>
        </p>
      </div>
    </div>

    <!-- 借阅记录查询 -->
    <div v-if="activeTab === 'records'" class="section">
      <div class="section-header">
        <h2>📋 借阅记录查询</h2>
        <p class="section-desc">查询当前读者的借阅记录或图书的借阅历史</p>
      </div>
      <div class="search-form">
        <div class="form-row">
          <div class="form-group">
            <label>读者ID</label>
            <input v-model="searchForm.readerId" placeholder="输入读者ID" class="form-input" />
          </div>
          <div class="form-group">
            <label>图书ID/条码</label>
            <input v-model="searchForm.bookId" placeholder="可选" class="form-input" />
          </div>
          <button class="btn-action btn-search" @click="searchRecords">查询</button>
        </div>
      </div>

      <div v-if="recordsLoading" class="loading">加载中...</div>
      <div v-else-if="records.length" class="records-table">
        <table>
          <thead>
            <tr>
              <th>记录ID</th>
              <th>读者ID</th>
              <th>图书ID</th>
              <th>借出时间</th>
              <th>应还时间</th>
              <th>归还时间</th>
              <th>状态</th>
              <th>逾期罚金</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in records" :key="r.BorrowRecordID">
              <td>{{ r.BorrowRecordID }}</td>
              <td>{{ r.ReaderID }}</td>
              <td>{{ r.BookID }}</td>
              <td>{{ formatDate(r.BorrowTime) }}</td>
              <td>{{ formatDate(r.DueTime) }}</td>
              <td>{{ formatDate(r.ReturnTime) || '-' }}</td>
              <td>
                <span :class="getStatusClass(r)">{{ r.Status }}</span>
              </td>
              <td>{{ r.OverdueFine ? '¥' + r.OverdueFine.toFixed(2) : '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else class="empty">暂无记录</div>
    </div>
  </Layout>
</template>

<script setup>
import { ref, reactive } from 'vue'
import http from '@/services/http.js'
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue'

const activeTab = ref('borrow')
const tabs = [
  { label: '📖 借阅', value: 'borrow' },
  { label: '📚 归还', value: 'return' },
  { label: '📦 批量操作', value: 'batch' },
  { label: '📋 记录查询', value: 'records' }
]

// 借阅
const borrowForm = reactive({ readerId: '', bookId: '' })
const borrowLoading = ref(false)
const borrowError = ref('')
const borrowSuccess = ref('')

// 归还
const returnForm = reactive({ readerId: '', bookId: '' })
const returnLoading = ref(false)
const returnError = ref('')
const returnSuccess = ref('')

// 批量
const batchForm = reactive({ readerId: '', bookIds: '', action: 'borrow' })
const batchLoading = ref(false)
const batchError = ref('')
const batchResult = ref([])

// 查询
const searchForm = reactive({ readerId: '', bookId: '' })
const records = ref([])
const recordsLoading = ref(false)

async function handleBorrow() {
  if (!borrowForm.readerId || !borrowForm.bookId) {
    borrowError.value = '请填写完整信息'
    borrowSuccess.value = ''
    return
  }
  borrowLoading.value = true
  borrowError.value = ''
  borrowSuccess.value = ''
  try {
    const res = await http.post('/borrowing/borrow', null, {
      params: { readerId: borrowForm.readerId, bookId: borrowForm.bookId },
      withToken: true
    })
    borrowSuccess.value = res.data?.message || res.data?.Message || '借阅成功'
  } catch (e) {
    borrowError.value = e.response?.data?.message || e.response?.data?.Message || '借阅失败'
  } finally {
    borrowLoading.value = false
  }
}

async function handleReturn() {
  if (!returnForm.readerId || !returnForm.bookId) {
    returnError.value = '请填写完整信息'
    returnSuccess.value = ''
    return
  }
  returnLoading.value = true
  returnError.value = ''
  returnSuccess.value = ''
  try {
    const res = await http.post('/borrowing/return', null, {
      params: { readerId: returnForm.readerId, bookId: returnForm.bookId },
      withToken: true
    })
    returnSuccess.value = res.data?.message || res.data?.Message || '归还成功'
  } catch (e) {
    returnError.value = e.response?.data?.message || e.response?.data?.Message || '归还失败'
  } finally {
    returnLoading.value = false
  }
}

async function handleBatch() {
  if (!batchForm.readerId || !batchForm.bookIds) {
    batchError.value = '请填写完整信息'
    batchResult.value = []
    return
  }
  batchLoading.value = true
  batchError.value = ''
  batchResult.value = []

  const ids = batchForm.bookIds.split(/[,\n]/).map(s => s.trim()).filter(Boolean)
  const action = batchForm.action

  for (const id of ids) {
    try {
      const endpoint = action === 'borrow' ? '/borrowing/borrow' : '/borrowing/return'
      const res = await http.post(endpoint, null, {
        params: { readerId: batchForm.readerId, bookId: id },
        withToken: true
      })
      batchResult.value.push({ success: true, message: `${id}: 成功` })
    } catch (e) {
      batchResult.value.push({
        success: false,
        message: `${id}: ${e.response?.data?.message || '失败'}`
      })
    }
  }
  batchLoading.value = false
}

async function searchRecords() {
  if (!searchForm.readerId && !searchForm.bookId) {
    alert('请至少输入读者ID或图书ID')
    return
  }
  recordsLoading.value = true
  records.value = []
  try {
    let res
    if (searchForm.readerId) {
      res = await http.get('/borrowing/reader', { withToken: true })
      let data = res.data || []
      if (searchForm.bookId) {
        data = data.filter(r => r.BookID == searchForm.bookId)
      }
      records.value = data
    } else if (searchForm.bookId) {
      res = await http.get(`/borrowing/book/${searchForm.bookId}`, { withToken: true })
      records.value = res.data || []
    }
  } catch (e) {
    console.error(e)
    alert('查询失败')
  } finally {
    recordsLoading.value = false
  }
}

function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleString('zh-CN')
}

function getStatusClass(r) {
  if (r.ReturnTime) return 'status-returned'
  if (r.DueTime && new Date(r.DueTime) < Date.now()) return 'status-overdue'
  return 'status-borrowing'
}
</script>

<style scoped>
.title { text-align: center; margin-bottom: 24px; color: #2c3e50; font-size: 30px; font-weight: bold; }
.tabs { display: flex; gap: 8px; margin-bottom: 24px; flex-wrap: wrap; }
.tab-btn { padding: 10px 20px; border: 1px solid #d1d5db; background: white; border-radius: 8px; cursor: pointer; font-size: 15px; transition: all 0.2s; }
.tab-btn.active { background: #2563eb; color: white; border-color: #2563eb; }
.section { background: white; border-radius: 12px; padding: 24px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); margin-bottom: 24px; }
.section-header { margin-bottom: 20px; }
.section-header h2 { margin: 0 0 6px; font-size: 20px; color: #1f2937; }
.section-desc { margin: 0; color: #6b7280; font-size: 14px; }
.action-form { background: #f9fafb; padding: 20px; border-radius: 10px; }
.form-row { display: flex; gap: 16px; align-items: flex-end; flex-wrap: wrap; }
.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group label { font-weight: 500; font-size: 14px; color: #374151; }
.form-input, .form-textarea { padding: 8px 12px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; min-width: 200px; box-sizing: border-box; font-family: inherit; }
.form-textarea { width: 100%; min-height: 100px; resize: vertical; }
.btn-action { padding: 10px 24px; border: none; border-radius: 6px; cursor: pointer; font-size: 15px; font-weight: 600; transition: 0.2s; white-space: nowrap; }
.btn-action:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-borrow { background: #2563eb; color: white; }
.btn-borrow:hover:not(:disabled) { background: #1d4ed8; }
.btn-return { background: #059669; color: white; }
.btn-return:hover:not(:disabled) { background: #047857; }
.btn-batch { background: #7c3aed; color: white; }
.btn-batch:hover:not(:disabled) { background: #6d28d9; }
.btn-search { background: #4da6ff; color: white; }
.btn-search:hover { background: #3399ff; }
.error-text { color: #dc2626; font-size: 14px; margin-top: 10px; }
.success-text { color: #059669; font-size: 14px; margin-top: 10px; font-weight: 600; }
.batch-form { background: #f9fafb; padding: 20px; border-radius: 10px; }
.radio-group { display: flex; gap: 20px; }
.radio-label { display: flex; align-items: center; gap: 6px; font-size: 14px; cursor: pointer; }
.result-text { margin-top: 12px; font-size: 14px; line-height: 1.8; }
.text-green { color: #059669; }
.text-red { color: #dc2626; }
.loading { text-align: center; padding: 30px; color: #999; }
.records-table { overflow-x: auto; margin-top: 16px; }
.records-table table { width: 100%; border-collapse: collapse; font-size: 14px; }
.records-table th { background: #4da6ff; color: white; padding: 10px 8px; text-align: center; }
.records-table td { padding: 10px 8px; border-bottom: 1px solid #e5e7eb; text-align: center; }
.records-table tbody tr:nth-child(even) { background: #f9fbfd; }
.status-borrowing { color: #2563eb; font-weight: 600; }
.status-returned { color: #059669; }
.status-overdue { color: #dc2626; font-weight: 600; }
.empty { text-align: center; padding: 40px; color: #9ca3af; font-size: 15px; }
</style>

<template>
  <div class="rd-borrowing">
    <PageHeader title="我的借阅" subtitle="查看在借图书、借阅历史与续借管理">
      <template #actions>
        <el-radio-group v-model="statusTab" size="default">
          <el-radio-button value="all">全部</el-radio-button>
          <el-radio-button value="unreturned">在借</el-radio-button>
          <el-radio-button value="returned">已还</el-radio-button>
        </el-radio-group>
      </template>
    </PageHeader>

    <div class="card">
      <div class="filter-bar">
        <el-input v-model="keyword" placeholder="搜索书名 / 作者 / 条码" clearable style="width:300px" :prefix-icon="Search" />
        <el-date-picker v-model="dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" />
        <el-button type="primary" :icon="Search" @click="load">查询</el-button>
      </div>

      <el-table :data="filtered" v-loading="loading" stripe>
        <el-table-column label="书名" min-width="200">
          <template #default="{ row }">
            <div class="book-cell">
              <BookCover :title="row.BookTitle || row.Title" :author="row.BookAuthor || row.Author" :isbn="row.ISBN" class="cover-mini" />
              <div>
                <div class="btitle" @click="$router.push(`/book/${row.ISBN}`)">{{ row.BookTitle || row.Title }}</div>
                <div class="bauthor">{{ row.BookAuthor || row.Author }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="Barcode" label="条码" width="120" />
        <el-table-column label="借阅时间" width="160">
          <template #default="{ row }">{{ formatDate(row.BorrowTime) }}</template>
        </el-table-column>
        <el-table-column label="应还时间" width="160">
          <template #default="{ row }">
            <span :class="{ overdue: isOverdue(row) }">{{ formatDate(row.DueTime) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="归还时间" width="160">
          <template #default="{ row }">{{ row.ReturnTime ? formatDate(row.ReturnTime) : '—' }}</template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <StatusTag :status="row.BorrowStatus" />
          </template>
        </el-table-column>
        <el-table-column label="罚款" width="100">
          <template #default="{ row }">
            <span v-if="row.OverdueFine > 0" class="text-danger">¥{{ row.OverdueFine }}</span>
            <span v-else class="text-muted">—</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.BorrowStatus !== '已归还'" text type="primary" @click="onRenew(row)">
              <el-icon><Refresh /></el-icon> 续借
            </el-button>
            <el-button v-if="row.BorrowStatus !== '已归还'" text type="danger" @click="onReturn(row)">
              <el-icon><RefreshRight /></el-icon> 归还
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pager">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next"
          @current-change="load"
          @size-change="load"
        />
      </div>
    </div>

    <el-dialog v-model="renewDialog" title="续借图书" width="480px">
      <div v-if="renewTarget">
        <p>确认续借《{{ renewTarget.BookTitle || renewTarget.Title }}》?</p>
        <p class="text-muted">续借后将延长 30 天借期,最多可续借 1 次。</p>
      </div>
      <template #footer>
        <el-button @click="renewDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmRenew" :loading="renewing">确认续借</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import { borrowApi } from '@/api/borrow'
import { useUserStore } from '@/stores/user'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import BookCover from '@/shared/components/BookCover.vue'

const userStore = useUserStore()
const statusTab = ref('all')
const keyword = ref('')
const dateRange = ref(null)
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const loading = ref(false)
const records = ref([])
const renewDialog = ref(false)
const renewTarget = ref(null)
const renewing = ref(false)

const filtered = computed(() => {
  let list = records.value
  if (statusTab.value === 'unreturned') list = list.filter((r) => r.BorrowStatus !== '已归还')
  else if (statusTab.value === 'returned') list = list.filter((r) => r.BorrowStatus === '已归还')
  if (keyword.value) {
    const kw = keyword.value.toLowerCase()
    list = list.filter((r) => (r.BookTitle || r.Title || '').toLowerCase().includes(kw) || (r.BookAuthor || r.Author || '').toLowerCase().includes(kw) || (r.Barcode || '').toLowerCase().includes(kw))
  }
  return list
})

async function load() {
  loading.value = true
  try {
    const params = {
      pageNum: page.value,
      pageSize: pageSize.value,
      status: statusTab.value === 'all' ? null : (statusTab.value === 'unreturned' ? '未归还' : '已归还')
    }
    const resp = await borrowApi.myPaged(params)
    const data = resp?.data ?? resp
    records.value = data?.Data || data?.Items || data?.items || data?.Records || []
    total.value = data?.TotalCount || data?.totalCount || records.value.length
  } finally {
    loading.value = false
  }
}

function onRenew(row) {
  renewTarget.value = row
  renewDialog.value = true
}

async function confirmRenew() {
  renewing.value = true
  try {
    await borrowApi.renew(renewTarget.value.BookID)
    ElMessage.success('续借成功')
    renewDialog.value = false
    load()
  } finally { renewing.value = false }
}

function onReturn(row) {
  ElMessageBox.confirm(`确认归还《${row.BookTitle || row.Title}》?`, '归还图书', { type: 'warning' })
    .then(async () => {
      await borrowApi.returnBook(row.BookID)
      ElMessage.success('归还成功')
      load()
    })
    .catch(() => {})
}

function isOverdue(row) {
  return row.BorrowStatus !== '已归还' && row.DueTime && new Date(row.DueTime) < new Date()
}

function formatDate(t) {
  if (!t) return '—'
  return new Date(t).toLocaleDateString('zh-CN')
}

onMounted(load)
</script>

<style scoped>
.rd-borrowing { display: flex; flex-direction: column; gap: 16px; }
.filter-bar { display: flex; gap: 12px; margin-bottom: 20px; flex-wrap: wrap; }
.book-cell { display: flex; gap: 12px; align-items: center; }
.cover-mini { width: 36px; height: 54px; border-radius: 4px; }
.btitle { font-weight: 600; font-size: var(--fs-sm); cursor: pointer; }
.btitle:hover { color: var(--color-primary-600); }
.bauthor { font-size: var(--fs-xs); color: var(--color-text-soft); }
.overdue { color: var(--color-accent-red); font-weight: 500; }
.pager { margin-top: 16px; display: flex; justify-content: flex-end; }
</style>
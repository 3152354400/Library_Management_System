<template>
  <div class="rd-page">
    <PageHeader title="我的预约" subtitle="预约感兴趣的图书,到馆后第一时间取书">
      <template #actions>
        <el-button type="primary" :icon="Plus" @click="$router.push('/books')">去选书预约</el-button>
      </template>
    </PageHeader>

    <div class="card">
      <el-tabs v-model="tab">
        <el-tab-pane label="生效中" name="active" />
        <el-tab-pane label="已取消" name="cancelled" />
        <el-tab-pane label="全部" name="all" />
      </el-tabs>

      <el-table :data="filteredList" v-loading="loading" stripe>
        <el-table-column label="图书" min-width="240">
          <template #default="{ row }">
            <div class="book-cell">
              <BookCover :title="row.BookTitle || row.Title" :author="row.Author" :isbn="row.ISBN" class="cover-mini" />
              <div>
                <div class="btitle" @click="$router.push(`/book/${row.ISBN}`)">{{ row.BookTitle || row.Title }}</div>
                <div class="bauthor">{{ row.Author }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="ReserveTime" label="预约时间" width="180">
          <template #default="{ row }">{{ formatDate(row.ReserveTime) }}</template>
        </el-table-column>
        <el-table-column label="状态" width="120">
          <template #default="{ row }">
            <StatusTag :status="row.Status" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.Status === '等待中' || row.Status === '生效中' || row.Status === '未完成'" text type="danger" @click="onCancel(row)">取消预约</el-button>
          </template>
        </el-table-column>
      </el-table>

      <EmptyState v-if="!loading && !filteredList.length" title="暂无预约记录" description="去图书分类找找感兴趣的书吧">
        <el-button type="primary" @click="$router.push('/books')">浏览图书</el-button>
      </EmptyState>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { borrowApi } from '@/api/borrow'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import BookCover from '@/shared/components/BookCover.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const tab = ref('active')
const loading = ref(false)
const list = ref([])

const filteredList = computed(() => {
  if (tab.value === 'all') return list.value
  if (tab.value === 'active') return list.value.filter((r) => r.Status === '等待中' || r.Status === '生效中' || r.Status === '未完成' || r.Status === '待取书')
  return list.value.filter((r) => r.Status === '已取消' || r.Status === '已归还')
})

async function load() {
  loading.value = true
  try {
    const data = await borrowApi.myReserves()
    list.value = Array.isArray(data) ? data : []
  } finally { loading.value = false }
}

function onCancel(row) {
  ElMessageBox.confirm(`确认取消《${row.BookTitle || row.Title}》的预约?`, '提示', { type: 'warning' })
    .then(async () => {
      await borrowApi.cancelReserve(row.ReserveID || row.ReserveId)
      ElMessage.success('已取消')
      load()
    })
    .catch(() => {})
}

function formatDate(t) { return t ? new Date(t).toLocaleString('zh-CN') : '' }
onMounted(load)
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
.book-cell { display: flex; gap: 12px; align-items: center; }
.cover-mini { width: 36px; height: 54px; border-radius: 4px; }
.btitle { font-weight: 600; font-size: var(--fs-sm); cursor: pointer; }
.btitle:hover { color: var(--color-primary-600); }
.bauthor { font-size: var(--fs-xs); color: var(--color-text-soft); }
</style>
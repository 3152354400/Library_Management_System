<template>
  <div class="ad-page">
    <PageHeader title="书架与副本" subtitle="管理书架布局,定位馆藏副本" />

    <div class="layout">
      <div class="card">
        <h4>搜索书架</h4>
        <el-input v-model="shelfKw" placeholder="输入书架编码 / 区域" :prefix-icon="Search" clearable @keyup.enter="searchShelves" />
        <el-button type="primary" style="margin-top:12px;width:100%" @click="searchShelves">查询书架</el-button>

        <el-divider />

        <h4>新建书架</h4>
        <el-form :model="newShelf" label-width="80px" size="small">
          <el-form-item label="楼号"><el-input-number v-model="newShelf.BUILDINGID" :min="1" /></el-form-item>
          <el-form-item label="编码"><el-input v-model="newShelf.SHELFCODE" /></el-form-item>
          <el-form-item label="楼层"><el-input-number v-model="newShelf.FLOOR" :min="1" /></el-form-item>
          <el-form-item label="区域"><el-input v-model="newShelf.ZONE" /></el-form-item>
          <el-button type="primary" size="small" @click="onAddShelf">添加</el-button>
        </el-form>
      </div>

      <div class="card">
        <h4>书架列表</h4>
        <el-table :data="shelves" stripe size="small">
          <el-table-column prop="BUILDINGID" label="楼号" width="80" />
          <el-table-column prop="SHELFCODE" label="编码" width="100" />
          <el-table-column prop="FLOOR" label="楼层" width="80" />
          <el-table-column prop="ZONE" label="区域" />
          <el-table-column label="操作" width="120">
            <template #default="{ row }">
              <el-button text type="danger" @click="onDelete(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="card">
        <h4>按书名定位</h4>
        <el-input v-model="bookKw" placeholder="输入书名 / 关键词" :prefix-icon="Search" clearable @keyup.enter="searchBooks" />
        <el-button type="primary" style="margin-top:12px;width:100%" @click="searchBooks">查询</el-button>

        <el-divider />

        <h4 v-if="books.length">查询结果</h4>
        <EmptyState v-if="!books.length" title="输入关键词查询" />
        <ul v-else class="loc-list">
          <li v-for="b in books" :key="b.BOOKID">
            <div class="lt">{{ b.TITLE }}</div>
            <div class="lm">{{ b.BUILDINGID }} 楼 · {{ b.SHELFCODE }} · {{ b.FLOOR }}F · {{ b.ZONE }}</div>
            <el-tag size="small" :type="b.STATUS === '正常' ? 'success' : 'info'">{{ b.STATUS }}</el-tag>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import { bookApi } from '@/api/book'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const shelfKw = ref('')
const bookKw = ref('')
const shelves = ref([])
const books = ref([])
const newShelf = reactive({ BUILDINGID: 1, SHELFCODE: '', FLOOR: 1, ZONE: '' })

async function searchShelves() {
  shelves.value = await bookApi.searchShelves(shelfKw.value)
}

async function searchBooks() {
  books.value = await bookApi.searchBooksOnShelf(bookKw.value)
}

async function onAddShelf() {
  if (!newShelf.SHELFCODE) return ElMessage.warning('请输入书架编码')
  await bookApi.addShelf(newShelf.BUILDINGID, newShelf.SHELFCODE, newShelf.FLOOR, newShelf.ZONE)
  ElMessage.success('书架已添加')
  Object.assign(newShelf, { SHELFCODE: '', ZONE: '' })
  searchShelves()
}

async function onDelete(row) {
  await ElMessageBox.confirm('确认删除该书架?', '提示', { type: 'warning' }).catch(() => {})
  await bookApi.deleteShelf(row.SHELFID)
  ElMessage.success('已删除')
  searchShelves()
}
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.layout { display: grid; grid-template-columns: 280px 1fr 1fr; gap: 16px; align-items: flex-start; }
.layout h4 { margin: 0 0 12px; font-size: var(--fs-md); }
.loc-list { display: flex; flex-direction: column; gap: 8px; }
.loc-list li { padding: 10px 12px; background: var(--color-bg); border-radius: var(--radius-sm); }
.lt { font-weight: 600; font-size: var(--fs-sm); }
.lm { font-size: var(--fs-xs); color: var(--color-text-muted); margin: 2px 0 6px; }
@media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }
</style>
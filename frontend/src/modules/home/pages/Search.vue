<template>
  <div class="search-page">
    <PageHeader title="图书搜索" :subtitle="`${total} 条结果 · 关键词「${keyword}」`">
      <template #actions>
        <el-input v-model="keyword" placeholder="重新输入关键词..." clearable @keyup.enter="load" style="width:280px">
          <template #prefix><el-icon><Search /></el-icon></template>
        </el-input>
      </template>
    </PageHeader>

    <div class="search-layout">
      <aside class="filter-pane card">
        <h4>筛选</h4>
        <div class="filter-group">
          <div class="filter-label">排序方式</div>
          <el-radio-group v-model="filters.sort" @change="load" size="small">
            <el-radio-button value="default">默认</el-radio-button>
            <el-radio-button value="title">按书名</el-radio-button>
            <el-radio-button value="year">按年份</el-radio-button>
          </el-radio-group>
        </div>
        <div class="filter-group">
          <div class="filter-label">每页显示</div>
          <el-select v-model="filters.pageSize" @change="load" size="small">
            <el-option :value="10" label="10 条" />
            <el-option :value="20" label="20 条" />
            <el-option :value="50" label="50 条" />
          </el-select>
        </div>
      </aside>

      <div class="results">
        <el-skeleton v-if="loading" :rows="6" animated />
        <EmptyState v-else-if="!results.length" title="没有找到相关图书" :description="`换个关键词试试? 当前关键词: 「${keyword}」`">
          <el-button type="primary" @click="$router.push('/books')">浏览全部分类</el-button>
        </EmptyState>
        <div v-else class="result-grid">
          <div v-for="b in results" :key="b.ISBN" class="result-card" @click="$router.push(`/book/${b.ISBN}`)">
            <BookCover :title="b.Title" :author="b.Author" :isbn="b.ISBN" />
            <div class="rc-info">
              <div class="rc-title">{{ b.Title }}</div>
              <div class="rc-author">{{ b.Author }}</div>
              <div class="rc-meta">
                <span>{{ b.Publisher }}</span>
                <span class="stock">库存 {{ b.Stock }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { bookApi } from '@/api/book'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import BookCover from '@/shared/components/BookCover.vue'

const route = useRoute()
const keyword = ref(route.query.keyword || '')
const results = ref([])
const total = ref(0)
const loading = ref(false)
const filters = reactive({ sort: 'default', pageSize: 20 })

async function load() {
  loading.value = true
  try {
    const data = await bookApi.search(keyword.value)
    let list = data || []
    if (filters.sort === 'title') list.sort((a, b) => a.Title.localeCompare(b.Title))
    else if (filters.sort === 'year') list.sort((a, b) => (b.PublishYear || 0) - (a.PublishYear || 0))
    results.value = list.slice(0, filters.pageSize)
    total.value = list.length
  } finally {
    loading.value = false
  }
}

watch(() => route.query.keyword, (v) => { keyword.value = v || ''; load() })
onMounted(load)
</script>

<style scoped>
.search-page { padding: 32px 24px; max-width: 1200px; margin: 0 auto; }
.search-layout {
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 24px;
  align-items: flex-start;
}
.filter-pane { position: sticky; top: 80px; }
.filter-pane h4 { margin-bottom: 16px; font-size: var(--fs-md); }
.filter-group { margin-bottom: 18px; }
.filter-label { font-size: var(--fs-sm); color: var(--color-text-soft); margin-bottom: 8px; }
.result-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 20px;
}
.result-card {
  cursor: pointer;
  transition: transform .2s;
}
.result-card:hover { transform: translateY(-3px); }
.rc-info { padding: 10px 4px 0; }
.rc-title {
  font-size: var(--fs-sm);
  font-weight: 600;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  margin-bottom: 4px;
}
.rc-author { font-size: var(--fs-xs); color: var(--color-text-soft); }
.rc-meta { display: flex; justify-content: space-between; margin-top: 4px; font-size: var(--fs-xs); color: var(--color-text-muted); }
.stock { color: var(--color-accent-green); font-weight: 500; }
@media (max-width: 768px) {
  .search-layout { grid-template-columns: 1fr; }
  .filter-pane { position: static; }
}
</style>
<template>
  <div class="category-page">
    <PageHeader title="图书分类" subtitle="按分类浏览馆藏" />

    <div class="cat-layout">
      <aside class="cat-tree card">
        <h4>分类树</h4>
        <el-input v-model="search" placeholder="搜索分类..." size="small" clearable>
          <template #prefix><el-icon><Search /></el-icon></template>
        </el-input>
        <div class="tree-wrap">
          <div v-for="node in filteredTree" :key="node.CategoryID" class="tree-node">
            <div class="node-line" :class="{ active: currentCategory?.CategoryID === node.CategoryID }" @click="selectNode(node)">
              <span class="node-name">{{ node.CategoryName }}</span>
              <span v-if="node.Children?.length" class="node-count">{{ node.Children.length }}</span>
            </div>
            <div v-if="node.Children?.length" class="node-children">
              <div
                v-for="child in node.Children"
                :key="child.CategoryID"
                class="node-line small"
                :class="{ active: currentCategory?.CategoryID === child.CategoryID }"
                @click="selectNode(child)"
              >
                {{ child.CategoryName }}
              </div>
            </div>
          </div>
        </div>
      </aside>

      <div class="cat-content">
        <div v-if="!currentCategory" class="cat-banner">
          <h2>选择一个分类开始浏览</h2>
          <p>点击左侧任意分类查看图书列表</p>
        </div>
        <template v-else>
          <div class="cat-bread">
            <span class="current">{{ currentCategory.CategoryName }}</span>
          </div>
          <div v-if="loading" class="loading">
            <el-skeleton :rows="4" animated />
          </div>
          <EmptyState v-else-if="!currentBooks.length" title="该分类下暂无图书" />
          <div v-else class="books-grid">
            <div v-for="b in currentBooks" :key="b.ISBN" class="book-cell" @click="$router.push(`/book/${b.ISBN}`)">
              <BookCover :title="b.Title" :author="b.Author" :isbn="b.ISBN" />
              <div class="info">
                <div class="title">{{ b.Title }}</div>
                <div class="author">{{ b.Author }}</div>
                <div class="stock">库存 {{ b.Stock }}</div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { bookApi } from '@/api/book'
import PageHeader from '@/shared/components/PageHeader.vue'
import BookCover from '@/shared/components/BookCover.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const route = useRoute()
const tree = ref([])
const search = ref('')
const currentCategory = ref(null)
const currentBooks = ref([])
const allBooks = ref([])
const loading = ref(false)

const filteredTree = computed(() => {
  if (!search.value) return tree.value
  const kw = search.value.toLowerCase()
  return tree.value.filter((n) =>
    n.CategoryName.toLowerCase().includes(kw) ||
    n.Children?.some((c) => c.CategoryName.toLowerCase().includes(kw))
  ).map((n) => ({
    ...n,
    Children: n.Children?.filter((c) => c.CategoryName.toLowerCase().includes(kw) || n.CategoryName.toLowerCase().includes(kw))
  }))
})

async function load() {
  const [treeData, all] = await Promise.all([
    bookApi.getCategoryTree(),
    bookApi.search('')
  ])
  tree.value = treeData || []
  allBooks.value = all || []
  // 从路由恢复
  if (route.params.categoryId) {
    const target = findCategoryById(route.params.categoryId)
    if (target) selectNode(target)
  } else if (tree.value.length) {
    selectNode(tree.value[0])
  }
}

function findCategoryById(id) {
  for (const n of tree.value) {
    if (n.CategoryID === id) return n
    if (n.Children) {
      const c = n.Children.find((x) => x.CategoryID === id)
      if (c) return c
    }
  }
  return null
}

function selectNode(node) {
  currentCategory.value = node
  currentBooks.value = allBooks.value.filter((b) =>
    b.Categories?.toLowerCase().includes(node.CategoryName.toLowerCase())
  )
}

onMounted(load)
</script>

<style scoped>
.category-page { padding: 32px 24px; max-width: 1280px; margin: 0 auto; }
.cat-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 24px;
  align-items: flex-start;
}
.cat-tree { position: sticky; top: 80px; }
.cat-tree h4 { margin: 0 0 12px; }
.tree-wrap {
  margin-top: 16px;
  max-height: 540px;
  overflow-y: auto;
}
.node-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 12px;
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--fs-base);
  color: var(--color-text);
  margin-bottom: 2px;
}
.node-line:hover { background: var(--color-bg); }
.node-line.small { padding: 8px 12px 8px 28px; font-size: var(--fs-sm); color: var(--color-text-soft); }
.node-line.active { background: var(--color-primary-50); color: var(--color-primary-600); font-weight: 600; }
.node-count {
  background: var(--color-bg);
  font-size: var(--fs-xs);
  padding: 2px 8px;
  border-radius: 999px;
  color: var(--color-text-muted);
}
.cat-banner {
  background: linear-gradient(135deg, #EEF1FE, #fff);
  border-radius: var(--radius-lg);
  padding: 80px 24px;
  text-align: center;
  border: 1px dashed var(--color-primary-200);
}
.cat-banner h2 { color: var(--color-text); margin: 0 0 8px; }
.cat-banner p { color: var(--color-text-soft); }
.cat-bread {
  margin-bottom: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.cat-bread .current {
  background: var(--color-primary-50);
  color: var(--color-primary-600);
  padding: 4px 14px;
  border-radius: 999px;
  font-size: var(--fs-sm);
  font-weight: 500;
}
.books-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 20px;
}
.book-cell {
  cursor: pointer;
  transition: transform .2s;
}
.book-cell:hover { transform: translateY(-3px); }
.info { padding: 10px 4px 0; }
.title {
  font-size: var(--fs-sm);
  font-weight: 600;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  margin-bottom: 4px;
}
.author { font-size: var(--fs-xs); color: var(--color-text-soft); }
.stock { font-size: var(--fs-xs); color: var(--color-accent-green); margin-top: 4px; font-weight: 500; }
@media (max-width: 768px) {
  .cat-layout { grid-template-columns: 1fr; }
  .cat-tree { position: static; }
}
</style>
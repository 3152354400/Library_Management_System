<template>
  <section class="search-result-wrapper bg-gray-100 " >
    <div class="search-bar-container">
      <BookSearchBar />
    </div>

    <div v-if="loading" class="loading">加载中...</div>

    <div v-else-if="error" class="error">{{ error }}</div>

    <div v-else>
      <div v-if="categories.length" class="category-filter">
        <div class="category-filter-row">
          <label class="mr-2 text-sm text-gray-600">按分类筛选：</label>
          <button class="cat-chip" :class="{ active: selectedCategory==='全部' }"
                  @click="selectCategory('全部')">全部</button>
          <button v-for="cat in categories" :key="cat"
                  class="cat-chip" :class="{ active: selectedCategory===cat }"
                  @click="selectCategory(cat)">{{ cat }}</button>
          <select v-model="selectedCategory" class="cat-select">
            <option value="全部">全部</option>
            <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
          </select>
        </div>
      </div>
      <p v-if="filteredBooks.length === 0" class="empty">未找到相关图书</p>
      <ul v-else class="flex flex-wrap gap-6 justify-center">
        <div v-for="book in filteredBooks" :key="book.ISBN" class="wr_suggestion_card_wrapper relative" >
          <div class="wr_suggestion_card_content pb-16">
            <img
              :src="`/covers/${book.ISBN}.jpg`"
              alt="封面"
              class="fixed-cover-size"
              @error="e => (e.target.src = defaultCover)"
            />
            <div class="mt-4 text-center ">
              <div class="text-base font-semibold leading-tight ">
                {{ book.Title }}
              </div>
              <div class="text-sm text-gray-600 mt-1 ">
                作者：{{ book.Author }}
              </div>
              <div class="text-sm text-gray-600 mt-1">
                ISBN：{{ book.ISBN }}
              </div>
              <div class="text-sm text-gray-600 mt-1">
                分类：{{ displayCategories(book) || '暂无分类' }}
              </div>
              <div class="mt-2">

              </div>
            </div>
          </div>

          <div class="absolute bottom-4   flex flex-col gap-2 items-center ">
            <button 
              @click="viewComments(book.ISBN)"
              class="comments-button "
            >
              查看评论
            </button>
            <button 
              @click="viewPhysicalBooks(book.Title)"
              class="physical-books-button "
            >
              查看实体书
            </button>
          </div>
        </div>
      </ul>
    </div>
  </section>
</template>

<script setup>
import { ref, onMounted, watch, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getBooks } from '@/modules/book/api.js'
import BookSearchBar from '@/modules/home/components/BookSearchBar.vue'


const route = useRoute()
const router = useRouter()
const keyword = ref(route.query.q || '')
const books = ref([])
const loading = ref(false)
const error = ref('')

// 默认封面
const defaultCover = new URL('@/assets/book_cover_default.jpg', import.meta.url).href


// ===== 分类筛选逻辑 =====
const selectedCategory = ref('全部')

// 规范化：把一本书的分类转换为字符串数组
function normalizeCategories(book) {
  // 兼容: 数组 / 字符串（英文逗号, 中文逗号、顿号） / null
  let cats = []
  const raw = book?.Categories ?? book?.categories ?? null
  if (Array.isArray(raw)) {
    cats = raw
  } else if (typeof raw === 'string') {
    cats = raw
      .split(/[,，、;；\s]+/)
      .map(s => s.trim())
      .filter(Boolean)
  }
  // 去重 + 去空
  return [...new Set(cats)]
}

// 所有可选分类（从当前 books 计算）
const categories = computed(() => {
  const set = new Set()
  for (const b of books.value) {
    for (const c of normalizeCategories(b)) set.add(c)
  }
  return Array.from(set).sort()
})

// 过滤后的图书
const filteredBooks = computed(() => {
  if (selectedCategory.value === '全部') return books.value
  return books.value.filter(b => normalizeCategories(b).includes(selectedCategory.value))
})

function selectCategory(cat) {
  selectedCategory.value = cat
}

function displayCategories(book) {
  return normalizeCategories(book).join('，')
}
// =======================

async function fetchBooks() {
  if (!keyword.value) return
  loading.value = true
  error.value = ''
  try {
    console.log('开始搜索，关键词:', keyword.value)
    const res = await getBooks(keyword.value)
    console.log('API响应:', res)
    console.log('响应数据:', res.data)
    books.value = res.data || []
    selectedCategory.value = '全部'
    console.log('处理后的图书列表:', books.value)
    console.log('图书数量:', books.value.length)
    
    // 详细检查每本书的数据结构
    if (books.value.length > 0) {
      console.log('第一本书的详细信息:')
      console.log('原始数据:', books.value[0])
      console.log('字段列表:', Object.keys(books.value[0]))
      console.log('Title字段:', books.value[0].Title)
      console.log('Author字段:', books.value[0].Author)
      console.log('ISBN字段:', books.value[0].ISBN)
      console.log('Categories字段:', books.value[0].Categories)
      
      // 检查所有可能的字段名变体
      const book = books.value[0]
      console.log('所有字段值:')
      console.log('  title:', book.title)
      console.log('  Title:', book.Title)
      console.log('  author:', book.author)
      console.log('  Author:', book.Author)
      console.log('  isbn:', book.isbn)
      console.log('  ISBN:', book.ISBN)
      console.log('  categories:', book.categories)
      console.log('  Categories:', book.Categories)
    }
  } catch (e) {
    console.error('搜索错误:', e)
    console.error('错误详情:', {
      message: e.message,
      status: e.response?.status,
      data: e.response?.data
    })
    error.value = '搜索失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

// 查看评论功能
function viewComments(isbn) {
  router.push({
    path: '/comments',
    query: { isbn: isbn }
  })
}

//实体书
// 查看实体书功能
function viewPhysicalBooks(title) {
  router.push({
    path: '/booklocation',
    query: { q: title }
  })
}
// 初次加载
onMounted(fetchBooks)

// 监听路由 query 变化，支持在结果页继续搜索
watch(
  () => route.query.q,
  newQ => {
    keyword.value = newQ || ''
    fetchBooks()
  }
)

</script>

<style scoped>

.search-result-wrapper {
  position: relative;
  margin: 0 auto;        /* 不再用 margin-top */
  padding: 10px 12rem 0; /* 顶部留 60px 内边距 */
  color: #666;
}
.title {
  font-size: 1.25rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
}
.loading,
.error,
.empty {
  text-align: center;
  font-size: 1rem;
  padding: 2rem 0;
  color: #666;
}


.wr_suggestion_card_wrapper {
  background: #ffffff; 
  border: 1px solid #e5e7eb; /* 添加浅灰色边框 */
  border-radius: 12px;
  box-sizing: border-box;
  cursor: pointer;
  padding: 40px 20px;
  transition: transform 0.3s;
  width: 282px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.comments-button {
  background-color: #409eff;
  color: white;
  border: none;
  border-radius: 4px;
  padding: 6px 8px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s;
}

.comments-button:hover {
  background-color: #337ecc;
}



.physical-books-button {
  background-color: #67c23a;
  color: white;
  border: none;
  border-radius: 4px;
  padding: 6px 8px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s;
}

.physical-books-button:hover {
  background-color: #85ce61;
}

.comments-button, .physical-books-button {
  width: 120px;  /* 或者具体的像素值，如 width: 80px; */
  
}

.fixed-cover-size {
  display: block;
  margin: 0 auto; /* 居中关键 */
  max-width: 100%; /* 避免溢出 */
}

.search-bar-container {
  margin: 60px 0 30px 0; /* 上 40px，下 30px，左右 0 */
}

/* 分类筛选 */
.category-filter { margin: 6px 0 18px; }
.category-filter-row { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; }
.cat-chip {
  border: 1px solid #e5e7eb; background: #fff; color: #374151;
  padding: 4px 10px; border-radius: 999px; font-size: 12px; cursor: pointer;
  transition: all .2s;
}
.cat-chip:hover { background: #f3f4f6; }
.cat-chip.active { background: #2563eb; border-color: #2563eb; color: #fff; }
.cat-select { margin-left: auto; padding: 4px 8px; font-size: 12px; border: 1px solid #e5e7eb; border-radius: 6px; }

</style>
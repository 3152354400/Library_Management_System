<template>
  <section class="category-books-wrapper bg-gray-100">
    <div class="category-header">
      <h1 class="text-2xl font-bold text-gray-900 mb-2 mt-[60px]">分类: {{ categoryName }}</h1>
      <p class="text-gray-600 mt-[60px]">该分类下的所有图书</p>
    </div>

    <!-- <div class="search-bar-container">
      <BookSearchBar />
    </div> -->

    <div v-if="loading" class="loading">加载中...</div>

    <div v-else-if="error" class="error">{{ error }}</div>

    <div v-else>
      <p v-if="books.length === 0" class="empty">该分类下暂无图书</p>
      <ul v-else class="flex flex-wrap gap-6 justify-center">
        <div v-for="book in books" :key="book.ISBN" class="wr_suggestion_card_wrapper relative">
          <div class="wr_suggestion_card_content pb-16">
            <img
              :src="`/covers/${book.ISBN}.jpg`"
              alt="封面"
              class="fixed-cover-size"
              @error="e => (e.target.src = defaultCover)"
            />
            <div class="mt-4 text-center">
              <div class="text-base font-semibold leading-tight">
                {{ book.Title }}
              </div>
              <div class="text-sm text-gray-600 mt-1">
                作者：{{ book.Author }}
              </div>
              <div class="text-sm text-gray-600 mt-1">
                ISBN：{{ book.ISBN }}
              </div>
              <div class="text-sm text-gray-600 mt-1">
                分类：{{ book.CategoryName || '暂无分类' }}
              </div>
            </div>
          </div>

          <div class="absolute bottom-4 flex flex-col gap-2 items-center">
            <button 
              @click="viewComments(book.ISBN)"
              class="comments-button"
            >
              查看评论
            </button>
            <button 
              @click="viewPhysicalBooks(book.Title)"
              class="physical-books-button"
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
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getCategoryBooks } from '@/modules/book/api.js'
import BookSearchBar from '@/modules/home/components/BookSearchBar.vue'

const route = useRoute()
const router = useRouter()
const categoryId = ref(route.params.categoryId || '')
const categoryName = ref(route.query.categoryName || '未知分类')
const books = ref([])
const loading = ref(false)
const error = ref('')

// 默认封面
const defaultCover = new URL('@/assets/book_cover_default.jpg', import.meta.url).href

async function fetchBooks() {
  if (!categoryId.value) {
    error.value = '分类ID不能为空'
    return
  }
  
  loading.value = true
  error.value = ''
  
  try {
    console.log('开始获取分类下的图书，分类ID:', categoryId.value)
    const res = await getCategoryBooks(categoryId.value)
    console.log('API响应:', res)
    books.value = res.data || []
    console.log('图书数量:', books.value.length)
  } catch (e) {
    console.error('获取图书错误:', e)
    console.error('错误详情:', {
      message: e.message,
      status: e.response?.status,
      data: e.response?.data
    })
    error.value = '获取图书失败，请稍后重试'
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

// 查看实体书功能
function viewPhysicalBooks(title) {
  router.push({
    path: '/booklocation',
    query: { q: title }
  })
}

// 初次加载
onMounted(fetchBooks)
</script>

<style scoped>
.category-books-wrapper {
  position: relative;
  margin: 0 auto;
  padding: 10px 12rem 0;
  color: #666;
}

.category-header {
  text-align: center;
  margin: 20px 0 30px 0;
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
  border: 1px solid #e5e7eb;
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
  width: 120px;
}

.fixed-cover-size {
  display: block;
  margin: 0 auto;
  max-width: 100%;
}

.search-bar-container {
  margin: 30px 0;
}
</style>
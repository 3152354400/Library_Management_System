<template>
  <div class="physical-books-page">
    <div class="header-section">
      <h1>《{{ bookTitle }}》实体书信息</h1>
      <p>共找到 {{ physicalBooks.length }} 个实体书</p>
    </div>

    <div class="back-button">
      <button @click="goBack">返回搜索结果</button>
    </div>

    <div v-if="loading" class="loading">加载中...</div>
    
    <div v-else-if="error" class="error">{{ error }}</div>
    
    <div v-else class="books-container">
      <div v-if="physicalBooks.length === 0" class="no-books">
        未找到该书的实体书信息
      </div>
      
      <table v-else class="books-table">
        <thead>
          <tr>
            
            <th>馆藏条码</th>
            <th>楼宇</th>
            <th>书架编码</th>
            <th>楼层</th>
            <th>区域</th>
            <th>状态</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="book in physicalBooks" :key="book.BOOKID">
            
            <td>{{ book.BARCODE || '无信息' }}</td>
            <td>
              {{ book.BUILDINGID == 21 ? '总图书馆' : 
                 book.BUILDINGID == 22 ? '德文图书馆' : '无信息' }}
            </td>
            <td>{{ book.SHELFCODE || '无信息' }}</td>
            <td>{{ book.FLOOR ? book.FLOOR + '层' : '无信息' }}</td>
            <td>{{ book.ZONE ? book.ZONE  : '无信息' }}</td>
            <td>{{ book.STATUS || '下架' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getBooksBookShelf } from '@/modules/book/api.js'

const route = useRoute()
const router = useRouter()
const bookTitle = ref(route.query.q || '')
const physicalBooks = ref([])
const loading = ref(false)
const error = ref('')

// 获取实体书信息
const fetchPhysicalBooks = async () => {
  if (!bookTitle.value) return
  
  loading.value = true
  error.value = ''
  
  try {
    const response = await getBooksBookShelf(bookTitle.value.toLowerCase())
    // 过滤出该书名对应的所有实体书
    physicalBooks.value = response.data?.filter(item => item.TITLE === bookTitle.value) || []
  } catch (err) {
    console.error('获取实体书信息失败:', err)
    error.value = '获取实体书信息失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

// 返回上一页
const goBack = () => {
  router.go(-1)
}

onMounted(() => {
  fetchPhysicalBooks()
})
</script>

<style scoped>
.physical-books-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  min-height: 100vh;
  background-color: #f8fafc;
}

.header-section {
  background: #ffffff;
  color: #333;
  padding: 24px;
  border-radius: 8px;
  margin-bottom: 25px;
  text-align: center;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
}

.header-section h1 {
  margin: 0 0 8px 0;
  font-size: 24px;
  font-weight: 600;
  color: #1f2937;
}

.header-section p {
  margin: 0;
  color: #6b7280;
  font-size: 14px;
}

.back-button {
  margin-bottom: 30px;
}

.back-button button {
  padding: 10px 20px;
  background-color: #f1f5f9;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  color: #475569;
  transition: all 0.2s ease;
}

.back-button button:hover {
  background-color: #e2e8f0;
  border-color: #94a3b8;
}

.loading, .error, .no-books {
  text-align: center;
  padding: 40px;
  font-size: 18px;
  color: #64748b;
}

.error {
  color: #ef4444;
}

.books-container {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.books-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 16px;
}

.books-table th {
  background-color: #f8fafc;
  padding: 16px;
  text-align: left;
  font-weight: 600;
  color: #374151;
  border-bottom: 2px solid #e5e7eb;
}

.books-table td {
  padding: 16px;
  border-bottom: 1px solid #e5e7eb;
  color: #4b5563;
}

.books-table tr:hover {
  background-color: #f9fafb;
}

.books-table tr:last-child td {
  border-bottom: none;
}
</style>
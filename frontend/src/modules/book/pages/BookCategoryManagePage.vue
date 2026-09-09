<template>
    <div class="book-category-manage">
      <div class="page-header">
        <h1 class="page-title">图书分类管理</h1>
        <p class="page-description">管理图书与分类的关联关系</p>
      </div>

      <!-- 搜索区域 -->
      <div class="search-section">
        <div class="search-form">
          <div class="form-group">
            <label for="searchType">搜索类型：</label>
            <select id="searchType" v-model="searchType" class="form-select">
              <option value="book">按图书搜索</option>
              <option value="category">按分类搜索</option>
            </select>
          </div>
          <div class="form-group">
            <label for="searchKeyword">关键词：</label>
            <input 
              id="searchKeyword"
              type="text" 
              v-model="searchKeyword" 
              placeholder="请输入ISBN或分类ID"
              class="form-input"
            />
          </div>
          <button @click="handleSearch" class="btn btn-primary">搜索</button>
        </div>
      </div>

      <!-- 绑定操作区域 -->
      <div class="bind-section">
        <h2>图书分类绑定</h2>
        <div class="bind-form">
          <div class="form-group">
            <label for="bindIsbn">图书ISBN：</label>
            <input 
              id="bindIsbn"
              type="text" 
              v-model="bindForm.isbn" 
              placeholder="请输入图书ISBN"
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label for="bindCategories">选择分类：</label>
            <div class="category-selector">
              <div v-if="availableCategories.length === 0" class="no-categories">
                暂无可用分类
              </div>
              <div 
                v-for="category in availableCategories" 
                :key="category.CategoryID"
                class="category-option"
              >
                <label class="checkbox-label">
                  <input 
                    type="checkbox" 
                    :value="category.CategoryID"
                    v-model="bindForm.categoryIds"
                  />
                  <span class="category-name">{{ category.CategoryPath || category.CategoryName || '未知分类' }}</span>
                  <small class="debug-info" style="color: #666; font-size: 10px;">
                    (ID: {{ category.CategoryID }}, 路径: {{ category.CategoryPath || '无' }})
                  </small>
                </label>
              </div>
            </div>
          </div>
          <div class="form-group">
            <label for="bindNote">关联备注：</label>
            <textarea 
              id="bindNote"
              v-model="bindForm.relationNote" 
              placeholder="请输入关联备注（可选）"
              class="form-textarea"
              rows="3"
            ></textarea>
          </div>
          <button @click="handleBind" class="btn btn-success" :disabled="!canBind">
            绑定分类
          </button>
        </div>
      </div>

      <!-- 结果展示区域 -->
      <div class="results-section">
        <h2>搜索结果</h2>
        <div v-if="loading" class="loading">加载中...</div>
        <div v-else-if="searchResults.length === 0" class="no-results">
          暂无数据
        </div>
        <div v-else class="results-list">
          <div 
            v-for="item in searchResults" 
            :key="`${item.ISBN}-${item.CategoryID}`"
            class="result-item"
          >
            <div class="item-info">
              <h3 class="book-title">{{ item.Title }}</h3>
              <p class="book-author">作者：{{ item.Author }}</p>
              <p class="book-isbn">ISBN：{{ item.ISBN }}</p>
              <p class="category-path">分类：{{ item.CategoryPath || item.CategoryName || '暂无分类' }}</p>
              <p v-if="item.RelationNote" class="relation-note">备注：{{ item.RelationNote }}</p>
            </div>
            <div class="item-actions">
              <button 
                @click="handleRemove(item.ISBN, item.CategoryID)"
                class="btn btn-danger btn-sm"
              >
                移除关联
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 统计信息 -->
      <div class="stats-section">
        <h2>分类统计</h2>
        <div v-if="statsLoading" class="loading">加载中...</div>
        <div v-else-if="Object.keys(categoryStats).length === 0" class="no-stats">
          暂无分类数据
        </div>
        <div v-else class="stats-grid">
          <div 
            v-for="(count, categoryName) in categoryStats" 
            :key="categoryName"
            class="stat-item"
          >
            <div class="stat-name">{{ categoryName }}</div>
            <div class="stat-count">{{ count }} 本图书</div>
          </div>
        </div>
      </div>
    </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { 
  bindBookToCategories, 
  getBookCategories, 
  getCategoryBooks, 
  getLeafCategories, 
  getBookCategoryStats,
  removeBookCategory 
} from '../api.js'

// 响应式数据
const searchType = ref('book')
const searchKeyword = ref('')
const loading = ref(false)
const statsLoading = ref(false)
const searchResults = ref([])
const availableCategories = ref([])
const categoryStats = ref({})

// 绑定表单
const bindForm = reactive({
  isbn: '',
  categoryIds: [],
  relationNote: ''
})

// 监听选择变化
watch(() => bindForm.categoryIds, (newIds) => {
  console.log('选中的分类ID:', newIds)
}, { deep: true })

// 计算属性
const canBind = computed(() => {
  return bindForm.isbn.trim() && bindForm.categoryIds.length > 0
})

// 方法
const loadAvailableCategories = async () => {
  try {
    const response = await getLeafCategories()
    availableCategories.value = response.data
    console.log('加载的分类数据:', response.data)
    console.log('第一个分类的详细信息:', response.data[0])
    if (response.data.length > 0) {
      console.log('第一个分类的字段:', Object.keys(response.data[0]))
    }
  } catch (error) {
    console.error('加载分类失败:', error)
    alert('加载分类失败')
  }
}

const loadCategoryStats = async () => {
  try {
    statsLoading.value = true
    const response = await getBookCategoryStats()
    categoryStats.value = response.data || {}
  } catch (error) {
    console.error('加载统计失败:', error)
    categoryStats.value = {}
    alert('加载分类统计失败，请检查数据库连接')
  } finally {
    statsLoading.value = false
  }
}

const handleSearch = async () => {
  if (!searchKeyword.value.trim()) {
    alert('请输入搜索关键词')
    return
  }

  try {
    loading.value = true
    if (searchType.value === 'book') {
      // 按图书搜索
      const response = await getBookCategories(searchKeyword.value)
      searchResults.value = response.data
    } else {
      // 按分类搜索
      const response = await getCategoryBooks(searchKeyword.value)
      searchResults.value = response.data
    }
  } catch (error) {
    console.error('搜索失败:', error)
    alert('搜索失败')
    searchResults.value = []
  } finally {
    loading.value = false
  }
}

const handleBind = async () => {
  if (!canBind.value) {
    alert('请填写完整信息')
    return
  }

  try {
    const bindData = {
      isbn: bindForm.isbn.trim(),
      categoryIds: bindForm.categoryIds,
      relationNote: bindForm.relationNote.trim() || null
    }

    await bindBookToCategories(bindData)
    alert('绑定成功')
    
    // 清空表单
    bindForm.isbn = ''
    bindForm.categoryIds = []
    bindForm.relationNote = ''
    
    // 刷新统计
    loadCategoryStats()
  } catch (error) {
    console.error('绑定失败:', error)
    alert('绑定失败: ' + (error.response?.data?.message || error.message))
  }
}

const handleRemove = async (isbn, categoryId) => {
  if (!confirm('确定要移除这个分类关联吗？')) {
    return
  }

  try {
    await removeBookCategory(isbn, categoryId)
    alert('移除成功')
    
    // 刷新搜索结果
    if (searchKeyword.value.trim()) {
      handleSearch()
    }
    
    // 刷新统计
    loadCategoryStats()
  } catch (error) {
    console.error('移除失败:', error)
    alert('移除失败: ' + (error.response?.data?.message || error.message))
  }
}

// 生命周期
onMounted(() => {
  loadAvailableCategories()
  loadCategoryStats()
})
</script>

<style scoped>
.book-category-manage {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 32px;
  text-align: center;
}

.page-title {
  font-size: 28px;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 8px;
}

.page-description {
  color: #6b7280;
  font-size: 16px;
}

.search-section,
.bind-section,
.results-section,
.stats-section {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.search-section h2,
.bind-section h2,
.results-section h2,
.stats-section h2 {
  font-size: 20px;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 16px;
}

.search-form {
  display: flex;
  gap: 16px;
  align-items: end;
  flex-wrap: wrap;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.form-group label {
  font-weight: 500;
  color: #374151;
  font-size: 14px;
}

.form-input,
.form-select,
.form-textarea {
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.form-input:focus,
.form-select:focus,
.form-textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-textarea {
  resize: vertical;
  min-height: 80px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover {
  background: #2563eb;
}

.btn-success {
  background: #10b981;
  color: white;
}

.btn-success:hover {
  background: #059669;
}

.btn-success:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

.btn-danger {
  background: #ef4444;
  color: white;
}

.btn-danger:hover {
  background: #dc2626;
}

.btn-sm {
  padding: 4px 8px;
  font-size: 12px;
}



.bind-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.category-selector {
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 8px;
}

.category-option {
  margin-bottom: 8px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 14px;
}

.category-name {
  color: #374151;
}

.no-categories {
  text-align: center;
  color: #6b7280;
  padding: 20px;
  font-style: italic;
}

.debug-info {
  display: block;
  margin-top: 2px;
  color: #9ca3af;
  font-size: 10px;
}

.results-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.result-item {
  display: flex;
  justify-content: space-between;
  align-items: start;
  padding: 16px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  background: #f9fafb;
}

.item-info {
  flex: 1;
}

.book-title {
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 4px;
}

.book-author,
.book-isbn,
.category-path,
.relation-note {
  font-size: 14px;
  color: #6b7280;
  margin-bottom: 2px;
}

.item-actions {
  margin-left: 16px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
}

.stat-item {
  padding: 16px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  text-align: center;
  background: #f9fafb;
}

.stat-name {
  font-size: 14px;
  color: #6b7280;
  margin-bottom: 4px;
}

.stat-count {
  font-size: 18px;
  font-weight: 600;
  color: #1f2937;
}

.loading {
  text-align: center;
  color: #6b7280;
  padding: 32px;
}

.no-results {
  text-align: center;
  color: #6b7280;
  padding: 32px;
}

.no-stats {
  text-align: center;
  color: #6b7280;
  padding: 32px;
  font-style: italic;
}
</style>

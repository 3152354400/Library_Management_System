<template>
  <div>
    <h2 class="text-2xl font-bold mb-4 text-gray-700">书籍分类绑定</h2>
    <p class="text-sm text-gray-500 mb-6">在此页面搜索图书，并为其绑定或更新所属的分类。</p>

    <!-- 1. 图书搜索区域 -->
    <div class="search-section">
      <input 
        v-model="searchTerm" 
        @keyup.enter="searchBooks"
        placeholder="输入书名或作者进行搜索..." 
        class="search-input"
      >
      <button @click="searchBooks" class="btn-primary">搜索图书</button>
    </div>

    <!-- 2. 主内容区：左侧图书列表，右侧分类选择 -->
    <div class="main-content-grid">
      <!-- 左侧：搜索结果 -->
      <div class="book-list-panel">
        <h3 class="panel-title">搜索结果</h3>
        <div v-if="loadingBooks" class="text-center p-4">正在搜索...</div>
        <div v-else-if="books.length === 0" class="text-center p-4 text-gray-500">
          {{ searchPerformed ? '未找到相关图书' : '请输入关键词进行搜索' }}
        </div>
        <ul v-else class="book-list">
          <li 
            v-for="book in books" 
            :key="book.ISBN"
            @click="selectBook(book)"
            :class="{ 'selected': selectedBook && selectedBook.ISBN === book.ISBN }"
          >
            <div class="font-semibold">{{ book.Title }}</div>
            <div class="text-xs text-gray-500">{{ book.Author }} - {{ book.ISBN }}</div>
          </li>
        </ul>
      </div>

      <!-- 右侧：分类绑定区 -->
      <div class="category-panel" v-if="selectedBook">
        <h3 class="panel-title">为《{{ selectedBook.Title }}》绑定分类</h3>
        <div v-if="loadingCategories" class="text-center p-4">正在加载分类...</div>
        <div v-else>
          <div class="category-select-box">
            <label v-for="cat in allLeafCategories" :key="cat.CategoryID" class="category-checkbox">
              <input type="checkbox" :value="cat.CategoryID" v-model="selectedCategories">
              <span>{{ cat.CategoryPath }}</span>
            </label>
          </div>
          <div class="text-right mt-4">
            <button @click="handleBindCategories" :disabled="isBinding" class="btn-primary">
              {{ isBinding ? '更新中...' : '更新绑定' }}
            </button>
          </div>
        </div>
      </div>
      <div v-else class="category-panel placeholder">
        请先从左侧选择一本书以管理其分类
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
// 确保从正确的 api 文件引入
import { 
  getBooks, 
  getBookCategories, 
  getLeafCategories, 
  bindBookToCategories 
} from '@/modules/book/api.js';

const searchTerm = ref('');
const searchPerformed = ref(false);
const loadingBooks = ref(false);
const books = ref([]);

const selectedBook = ref(null);
const loadingCategories = ref(false);
const allLeafCategories = ref([]);
const selectedCategories = ref([]);
const isBinding = ref(false);

async function searchBooks() {
  if (!searchTerm.value.trim()) return;
  try {
    loadingBooks.value = true;
    searchPerformed.value = true;
    selectedBook.value = null; // 清空选择
    const res = await getBooks(searchTerm.value);
    books.value = res.data || [];
  } catch (error) {
    console.error("Failed to search books:", error);
    alert("搜索图书失败！");
  } finally {
    loadingBooks.value = false;
  }
}

async function selectBook(book) {
  selectedBook.value = book;
  try {
    loadingCategories.value = true;
    const [boundRes, leafRes] = await Promise.all([
      getBookCategories(book.ISBN),
      getLeafCategories()
    ]);
    
    allLeafCategories.value = leafRes.data || [];
    // 初始化勾选状态
    const boundCategoryIds = (boundRes.data || []).map(c => c.CategoryID);
    selectedCategories.value = boundCategoryIds;

  } catch (error) {
    console.error("Failed to fetch categories for book:", error);
    alert("加载分类信息失败！");
  } finally {
    loadingCategories.value = false;
  }
}

async function handleBindCategories() {
  if (!selectedBook.value) return;
  try {
    isBinding.value = true;
    await bindBookToCategories({
      isbn: selectedBook.value.ISBN,
      categoryIds: selectedCategories.value
    });
    alert(`已成功更新《${selectedBook.value.Title}》的分类绑定！`);
  } catch (error) {
    console.error("Failed to bind categories:", error);
    alert("绑定失败：" + (error.response?.data?.message || "请稍后再试"));
  } finally {
    isBinding.value = false;
  }
}
</script>

<style scoped>
.search-section { display: flex; gap: 1rem; margin-bottom: 1.5rem; }
.search-input { flex-grow: 1; padding: 0.5rem 1rem; border: 1px solid #d1d5db; border-radius: 6px; }
.btn-primary { background-color: #2563eb; color: white; padding: 0.5rem 1.5rem; border: none; border-radius: 6px; cursor: pointer; }
.main-content-grid { display: grid; grid-template-columns: 1fr 2fr; gap: 1.5rem; height: 65vh; }
.book-list-panel, .category-panel { background-color: white; border-radius: 8px; padding: 1rem; box-shadow: 0 1px 3px rgba(0,0,0,0.05); display: flex; flex-direction: column; }
.panel-title { font-weight: 600; border-bottom: 1px solid #e5e7eb; padding-bottom: 0.75rem; margin-bottom: 0.75rem; }
.book-list { list-style: none; padding: 0; margin: 0; overflow-y: auto; }
.book-list li { padding: 0.75rem; border-radius: 4px; cursor: pointer; }
.book-list li:hover { background-color: #f3f4f6; }
.book-list li.selected { background-color: #dbeafe; color: #1e40af; }
.category-panel.placeholder { display: flex; align-items: center; justify-content: center; color: #9ca3af; }
.category-select-box { flex-grow: 1; max-height: 100%; overflow-y: auto; border: 1px solid #d1d5db; border-radius: 6px; padding: 0.5rem; }
.category-checkbox { display: flex; align-items: center; padding: 0.5rem; cursor: pointer; border-radius: 4px; }
.category-checkbox:hover { background-color: #f3f4f6; }
.category-checkbox input { margin-right: 0.75rem; }
</style>
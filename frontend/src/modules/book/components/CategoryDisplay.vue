<template>
  <div class="category-display mt-[60px]">
    <div class="tree-header">
      <h3 class="text-lg font-semibold mb-4">图书分类</h3>
    </div>

    <!-- 分类列表 -->
    <div class="tree-container">
      <div v-if="loading" class="text-center py-8">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
        <p class="mt-2 text-gray-600">加载中...</p>
      </div>
      
      <div v-else-if="leafCategories.length === 0" class="text-center py-8">
        <p class="text-gray-500">暂无分类数据</p>
      </div>
      
      <div v-else class="category-list">
        <div 
          v-for="category in leafCategories" 
          :key="category.CategoryID"
          class="category-item"
          @click="goToCategoryBooks(category)"
        >
          {{ category.CategoryName }}
          <span class="category-id text-gray-500 text-sm">({{ category.CategoryID }})</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { getCategoryTree } from '../api.js'

export default {
  name: 'CategoryDisplay',
  setup() {
    const router = useRouter()
    const categories = ref([])
    const loading = ref(false)

    // 计算属性：只显示叶子节点（没有子节点的分类）
    const leafCategories = computed(() => {
      const leaves = [];
      
      // 递归函数查找所有叶子节点
      function findLeaves(nodes) {
        nodes.forEach(node => {
          // 如果没有子节点或者子节点为空数组，则为叶子节点
          if (!node.Children || node.Children.length === 0) {
            leaves.push(node);
          } else {
            // 否则继续递归查找子节点
            findLeaves(node.Children);
          }
        });
      }
      
      findLeaves(categories.value);
      return leaves;
    });

    // 加载分类树
    const loadCategories = async () => {
      loading.value = true
      try {
        const response = await getCategoryTree()
        categories.value = response.data || []
      } catch (error) {
        console.error('加载分类失败:', error)
        const errorMessage = error.response?.data?.message || error.message || '加载分类失败'
        console.error('加载分类失败:', errorMessage)
        categories.value = []
      } finally {
        loading.value = false
      }
    }

    // 跳转到分类图书页面
    const goToCategoryBooks = (category) => {
      router.push({
        name: 'CategoryBooks',
        params: { categoryId: category.CategoryID },
        query: { categoryName: category.CategoryName }
      })
    }

    onMounted(() => {
      loadCategories()
    })

    return {
      categories,
      leafCategories,
      loading,
      loadCategories,
      goToCategoryBooks
    }
  }
}
</script>

<style scoped>
.category-display {
  padding: 20px;
}

.tree-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.tree-container {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.category-list {
  padding: 16px;
}

.category-item {
  padding: 12px 16px;
  border: 1px solid #e5e7eb;
  border-radius: 6px;
  margin-bottom: 8px;
  background: white;
  font-weight: 500;
  color: #374151;
  cursor: pointer;
  transition: background-color 0.2s;
}

.category-item:hover {
  background-color: #f3f4f6;
}
</style>
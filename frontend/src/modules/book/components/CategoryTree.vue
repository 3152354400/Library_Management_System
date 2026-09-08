<template>
  <div class="category-tree">
    <div class="tree-header">
      <h3 class="text-lg font-semibold mb-4">分类管理</h3>
      <button 
        @click="addTopLevelCategory" 
        class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
      >
        添加顶级分类
      </button>
    </div>

    <!-- 分类树 -->
    <div class="tree-container">
      <div v-if="loading" class="text-center py-8">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
        <p class="mt-2 text-gray-600">加载中...</p>
      </div>
      
      <div v-else-if="categories.length === 0" class="text-center py-8">
        <p class="text-gray-500">暂无分类数据</p>
      </div>
      
      <div v-else class="tree-list">
        <div 
          v-for="category in categories" 
          :key="category.CategoryID"
          class="tree-item"
        >
          <CategoryTreeNode 
            :category="category" 
            @edit="editCategory"
            @delete="deleteCategory"
            @add-child="addChildCategory"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { getCategoryTree } from '../api.js'
import CategoryTreeNode from './CategoryTreeNode.vue'

export default {
  name: 'CategoryTree',
  components: {
    CategoryTreeNode
  },
  emits: ['add-child', 'edit', 'delete'],
  setup(props, { emit }) {
    const categories = ref([])
    const loading = ref(false)

    // 加载分类树
    const loadCategories = async () => {
      loading.value = true
      try {
        console.log('开始加载分类树...')
        const response = await getCategoryTree()
        console.log('API响应:', response)
        categories.value = response.data || []
        console.log('分类树加载成功，共', categories.value.length, '个顶级分类')
      } catch (error) {
        console.error('加载分类失败:', error)
        console.error('错误详情:', {
          message: error.message,
          status: error.response?.status,
          statusText: error.response?.statusText,
          data: error.response?.data,
          config: error.config
        })
        const errorMessage = error.response?.data?.message || error.message || '加载分类失败'
        console.error('加载分类失败:', errorMessage)
        categories.value = []
      } finally {
        loading.value = false
      }
    }

    // 添加顶级分类
    const addTopLevelCategory = () => {
      emit('add-child', null) // 传递null表示顶级分类
    }

    // 添加子分类
    const addChildCategory = (parentCategory) => {
      emit('add-child', parentCategory)
    }

    // 编辑分类
    const editCategory = (category) => {
      emit('edit', category)
    }

    // 删除分类
    const deleteCategory = (category) => {
      emit('delete', category)
    }

    onMounted(() => {
      loadCategories()
    })

    return {
      categories,
      loading,
      loadCategories,
      addTopLevelCategory,
      addChildCategory,
      editCategory,
      deleteCategory
    }
  }
}
</script>

<style scoped>
.category-tree {
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

.tree-list {
  padding: 16px;
}

.tree-item {
  margin-bottom: 8px;
}
</style>

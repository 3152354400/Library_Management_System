<template>
  <div class="category-manage-page">
    <div class="page-header">
      <h1 class="text-2xl font-bold text-gray-900">分类管理</h1>
      <p class="text-gray-600 mt-2">管理图书分类的层级结构，支持添加、编辑、删除分类</p>
    </div>

    <div class="page-content">
      <CategoryTree 
        ref="categoryTreeRef"
        @add-child="showAddForm"
        @edit="showEditForm"
        @delete="showDeleteConfirm"
      />
    </div>

    <!-- 添加/编辑分类模态框 -->
    <div v-if="showFormModal" class="modal-overlay" @click="closeFormModal">
      <div class="modal-content" @click.stop>
        <CategoryForm 
          :category="editingCategory"
          :all-categories="allCategories"
          :submitting="submitting"
          @close="closeFormModal"
          @success="onFormSuccess"
        />
      </div>
    </div>

    <!-- 确认删除模态框 -->
    <div v-if="showDeleteModal" class="modal-overlay" @click="showDeleteModal = false">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3 class="text-lg font-semibold text-red-600">确认删除</h3>
        </div>
        
        <div class="modal-body">
          <p class="text-gray-700 mb-4">
            确定要删除分类 "{{ categoryToDelete?.CategoryName }}" 吗？
          </p>
          <p class="text-sm text-red-600 mb-4">
            注意：删除后无法恢复，且该分类下的所有子分类和关联图书将受到影响。
          </p>
          
          <div class="form-actions">
            <button 
              @click="showDeleteModal = false"
              class="bg-gray-300 hover:bg-gray-400 text-gray-800 font-bold py-2 px-4 rounded mr-2"
            >
              取消
            </button>
            <button 
              @click="confirmDelete"
              :disabled="submitting"
              class="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded disabled:opacity-50"
            >
              {{ submitting ? '删除中...' : '确认删除' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed } from 'vue'
import CategoryTree from '../components/CategoryTree.vue'
import CategoryForm from '../components/CategoryForm.vue'
import { deleteCategory } from '../api.js'

export default {
  name: 'CategoryManagePage',
  components: {
    CategoryTree,
    CategoryForm
  },
  setup() {
    const categoryTreeRef = ref(null)
    const showFormModal = ref(false)
    const showDeleteModal = ref(false)
    const submitting = ref(false)
    const editingCategory = ref(null)
    const categoryToDelete = ref(null)

    // 获取所有分类（扁平化）
    const allCategories = computed(() => {
      if (!categoryTreeRef.value?.categories) return []
      
      const flatten = (cats) => {
        let result = []
        cats.forEach(cat => {
          result.push(cat)
          if (cat.Children && cat.Children.length > 0) {
            result = result.concat(flatten(cat.Children))
          }
        })
        return result
      }
      return flatten(categoryTreeRef.value.categories)
    })

    // 显示添加表单
    const showAddForm = (parentCategory = null) => {
      editingCategory.value = null
      if (parentCategory) {
        // 设置父分类
        editingCategory.value = {
          CategoryID: '',
          CategoryName: '',
          ParentCategoryID: parentCategory.CategoryID
        }
      }
      showFormModal.value = true
    }

    // 显示编辑表单
    const showEditForm = (category) => {
      editingCategory.value = category
      showFormModal.value = true
    }

    // 关闭表单模态框
    const closeFormModal = () => {
      showFormModal.value = false
      editingCategory.value = null
    }

    // 表单提交成功
    const onFormSuccess = () => {
      closeFormModal()
      // 重新加载分类树
      if (categoryTreeRef.value) {
        categoryTreeRef.value.loadCategories()
      }
    }

    // 显示删除确认
    const showDeleteConfirm = (category) => {
      categoryToDelete.value = category
      showDeleteModal.value = true
    }

    // 确认删除
    const confirmDelete = async () => {
      if (!categoryToDelete.value) return
      
      submitting.value = true
      try {
        const operatorId = 'admin001' // 这里应该从用户登录状态获取
        await deleteCategory(categoryToDelete.value.CategoryID, operatorId)
        // 使用更友好的提示
        const message = `分类 "${categoryToDelete.value.CategoryName}" 删除成功`
        console.log(message)
        showDeleteModal.value = false
        categoryToDelete.value = null
        // 重新加载分类树
        if (categoryTreeRef.value) {
          categoryTreeRef.value.loadCategories()
        }
      } catch (error) {
        console.error('删除失败:', error)
        const errorMessage = error.response?.data?.message || error.message || '删除失败'
        console.error('删除失败:', errorMessage)
      } finally {
        submitting.value = false
      }
    }

    return {
      categoryTreeRef,
      showFormModal,
      showDeleteModal,
      submitting,
      editingCategory,
      categoryToDelete,
      allCategories,
      showAddForm,
      showEditForm,
      closeFormModal,
      onFormSuccess,
      showDeleteConfirm,
      confirmDelete
    }
  }
}
</script>

<style scoped>
.category-manage-page {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 32px;
}

.page-content {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-body {
  padding: 20px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 20px;
}
</style> 
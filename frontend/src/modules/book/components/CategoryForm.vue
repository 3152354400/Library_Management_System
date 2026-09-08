<template>
  <div class="category-form">
    <div class="form-header">
      <h3 class="text-lg font-semibold">
        {{ isEdit ? '编辑分类' : '添加分类' }}
      </h3>
      <button @click="$emit('close')" class="text-gray-500 hover:text-gray-700">
        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
        </svg>
      </button>
    </div>
    
    <form @submit.prevent="submitForm" class="form-body">
      <div class="form-group">
        <label class="block text-sm font-medium text-gray-700 mb-2">
          分类ID *
        </label>
        <input 
          v-model="formData.CategoryID"
          type="text"
          required
          :disabled="isEdit"
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
          placeholder="请输入分类ID（如：LIT001）"
        />
        <p class="text-xs text-gray-500 mt-1">
          {{ isEdit ? '编辑模式下分类ID不可修改' : '请输入唯一的分类ID，建议使用字母数字组合' }}
        </p>
      </div>
      
      <div class="form-group">
        <label class="block text-sm font-medium text-gray-700 mb-2">
          分类名称 *
        </label>
        <input 
          v-model="formData.CategoryName"
          type="text"
          required
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="请输入分类名称"
        />
      </div>
      
      <div class="form-group">
        <label class="block text-sm font-medium text-gray-700 mb-2">
          父分类
        </label>
        <select 
          v-model="formData.ParentCategoryID"
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option :value="null">顶级分类</option>
          <option 
            v-for="cat in availableParents" 
            :key="cat.CategoryID"
            :value="cat.CategoryID"
          >
            {{ getCategoryPath(cat) }}
          </option>
        </select>
      </div>
      
      <div class="form-actions">
        <button 
          type="button"
          @click="$emit('close')"
          class="bg-gray-300 hover:bg-gray-400 text-gray-800 font-bold py-2 px-4 rounded mr-2"
        >
          取消
        </button>
        <button 
          type="submit"
          :disabled="submitting"
          class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded disabled:opacity-50"
        >
          {{ submitting ? '提交中...' : (isEdit ? '更新' : '添加') }}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { ref, computed, watch } from 'vue'
import { addCategory, updateCategory } from '../api.js'

export default {
  name: 'CategoryForm',
  props: {
    category: {
      type: Object,
      default: null
    },
    allCategories: {
      type: Array,
      default: () => []
    },
    submitting: {
      type: Boolean,
      default: false
    }
  },
  emits: ['close', 'success'],
  setup(props, { emit }) {
    const formData = ref({
      CategoryID: '',
      CategoryName: '',
      ParentCategoryID: null
    })

    const isEdit = computed(() => !!props.category)

    // 可选的父分类（排除自己和自己的子分类）
    const availableParents = computed(() => {
      if (!isEdit.value) return props.allCategories
      
      const excludeIds = [props.category.CategoryID]
      const addChildrenIds = (catId) => {
        const cat = props.allCategories.find(c => c.CategoryID === catId)
        if (cat && cat.Children) {
          cat.Children.forEach(child => {
            excludeIds.push(child.CategoryID)
            addChildrenIds(child.CategoryID)
          })
        }
      }
      addChildrenIds(props.category.CategoryID)
      
      return props.allCategories.filter(cat => !excludeIds.includes(cat.CategoryID))
    })

    // 获取分类路径
    const getCategoryPath = (category) => {
      const findPath = (catId) => {
        const cat = props.allCategories.find(c => c.CategoryID === catId)
        if (!cat) return []
        if (!cat.ParentCategoryID) return [cat.CategoryName]
        return [...findPath(cat.ParentCategoryID), cat.CategoryName]
      }
      return findPath(category.CategoryID).join(' / ')
    }

    // 监听编辑模式变化
    watch(() => props.category, (newCategory) => {
      if (newCategory) {
        // 编辑模式：包含CategoryID
        formData.value = {
          CategoryID: newCategory.CategoryID,
          CategoryName: newCategory.CategoryName,
          ParentCategoryID: newCategory.ParentCategoryID || null
        }
      } else {
        // 添加模式：包含CategoryID字段，但为空
        formData.value = {
          CategoryID: '',
          CategoryName: '',
          ParentCategoryID: null // 顶级分类设为null
        }
      }
    }, { immediate: true })

    // 提交表单
    const submitForm = async () => {
      try {
        // 验证CategoryID不为空
        if (!formData.value.CategoryID || formData.value.CategoryID.trim() === '') {
          alert('请输入分类ID')
          return
        }

        // 验证CategoryName不为空
        if (!formData.value.CategoryName || formData.value.CategoryName.trim() === '') {
          alert('请输入分类名称')
          return
        }   
        const operatorId = 'admin001' // 这里应该从用户登录状态获取
        
        if (isEdit.value) {
          // 编辑模式：发送完整数据包括CategoryID
          await updateCategory({
            Category: formData.value,
            OperatorId: operatorId
          })
        } else {
          // 添加模式：发送完整数据包括CategoryID
          await addCategory({
            Category: formData.value,
            OperatorId: operatorId
          })
        }
        
        emit('success')
      } catch (error) {
        console.error('操作失败:', error)
        alert('操作失败: ' + (error.response?.data?.message || error.message))
      }
    }

    return {
      formData,
      isEdit,
      availableParents,
      getCategoryPath,
      submitForm
    }
  }
}
</script>

<style scoped>
.category-form {
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.form-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e5e7eb;
}

.form-body {
  padding: 20px;
}

.form-group {
  margin-bottom: 16px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 20px;
}
</style>
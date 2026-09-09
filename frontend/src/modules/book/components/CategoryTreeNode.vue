<template>
  <div class="category-node">
    <div class="node-content">
      <div class="node-info">
        <span class="category-name">{{ category.CategoryName }}</span>
        <span class="category-id text-gray-500 text-sm">({{ category.CategoryID }})</span>
      </div>
      
      <div class="node-actions">
        <button 
          @click="$emit('add-child', category)"
          class="action-btn add-btn"
          title="添加子分类"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
          </svg>
        </button>
        
        <button 
          @click="$emit('edit', category)"
          class="action-btn edit-btn"
          title="编辑分类"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
          </svg>
        </button>
        
        <button 
          @click="$emit('delete', category)"
          class="action-btn delete-btn"
          title="删除分类"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
          </svg>
        </button>
      </div>
    </div>
    
    <!-- 子分类 -->
    <div v-if="category.Children && category.Children.length > 0" class="children-container">
      <div 
        v-for="child in category.Children" 
        :key="child.CategoryID"
        class="child-node"
      >
        <CategoryTreeNode 
          :category="child"
          @edit="$emit('edit', $event)"
          @delete="$emit('delete', $event)"
          @add-child="$emit('add-child', $event)"
        />
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CategoryTreeNode',
  props: {
    category: {
      type: Object,
      required: true
    }
  },
  emits: ['edit', 'delete', 'add-child']
}
</script>

<style scoped>
.category-node {
  border: 1px solid #e5e7eb;
  border-radius: 6px;
  margin-bottom: 8px;
  background: white;
}

.node-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #f3f4f6;
}

.node-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.category-name {
  font-weight: 500;
  color: #374151;
}

.node-actions {
  display: flex;
  gap: 4px;
}

.action-btn {
  padding: 4px;
  border-radius: 4px;
  border: none;
  cursor: pointer;
  transition: all 0.2s;
}

.add-btn {
  color: #059669;
  background: #d1fae5;
}

.add-btn:hover {
  background: #a7f3d0;
}

.edit-btn {
  color: #2563eb;
  background: #dbeafe;
}

.edit-btn:hover {
  background: #bfdbfe;
}

.delete-btn {
  color: #dc2626;
  background: #fee2e2;
}

.delete-btn:hover {
  background: #fecaca;
}

.children-container {
  padding: 8px 16px;
  background: #f9fafb;
}

.child-node {
  margin-left: 16px;
  border-left: 2px solid #e5e7eb;
  padding-left: 8px;
}
</style> 
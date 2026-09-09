<template>
  <div v-if="isLeaf" class="category-node">
    <div class="node-content">
      <div class="node-info">
        <span class="category-name">{{ category.CategoryName }}</span>
        <span class="category-id text-gray-500 text-sm">({{ category.CategoryID }})</span>
      </div>
    </div>
  </div>
  
  <!-- 递归渲染子节点 -->
  <div v-if="category.Children && category.Children.length > 0">
    <div 
      v-for="child in category.Children" 
      :key="child.CategoryID"
    >
      <CategoryDisplayNode 
        :category="child"
      />
    </div>
  </div>
</template>

<script>
import { computed } from 'vue';

export default {
  name: 'CategoryDisplayNode',
  props: {
    category: {
      type: Object,
      required: true
    }
  },
  setup(props) {
    // 判断是否为叶子节点（没有子节点）
    const isLeaf = computed(() => {
      return !props.category.Children || props.category.Children.length === 0;
    });

    return {
      isLeaf
    };
  }
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
</style>
<template>
  <div class="category-display-page">
    <div class="page-header">
      <h1 class="text-2xl font-bold text-gray-900 mt-[60px]">图书分类</h1>
      <!-- <p class="text-gray-600 mt-2">查看图书馆所有分类</p> -->
    </div>

    <div class="page-content">
      <CategoryDisplay />
    </div>
  </div>
</template>

<script>
import CategoryDisplay from '../components/CategoryDisplay.vue'

export default {
  name: 'CategoryDisplayPage',
  components: {
    CategoryDisplay
  }
}
</script>

<style scoped>
.category-display-page {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 32px;
}

.page-content {
  background: white;
}
</style>
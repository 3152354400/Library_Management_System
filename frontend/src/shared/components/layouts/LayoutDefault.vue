<template>
  <!-- 1. 根元素保持不变 -->
  <div class="layout">
    
    <!-- 2. 顶部导航条保持不变 -->
    <NavbarTop />

    <!-- 3. 新增：紧急公告栏 -->
    <!-- 这个组件会从 Pinia store 中获取数据，只有在存在紧急公告时才会显示 -->
    <UrgentAnnouncements />
    
    <!-- 4. 主内容区域 (之前叫 .slot) -->
    <!-- 我们将它包裹在一个 <main> 标签中，语义更清晰 -->
    <main class="main-content">
      <slot />
    </main>
    
    <!-- 5. 页脚信息栏保持不变 -->
    <Footer />
  </div>
</template>

<script setup>
import NavbarTop from '@/shared/components/layouts/NavbarTop.vue'
import Footer from '@/shared/components/layouts/Footer.vue'
// 引入新的紧急公告组件
import UrgentAnnouncements from '@/shared/components/layouts/UrgentAnnouncements.vue' 
</script>

<style scoped>
/* ① 根容器样式保持不变 */
.layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh; 
}

/* ② 主内容区域样式调整 */
.main-content {
  flex: 1 0 auto;
  /* 移除了固定的 padding-top，因为现在可能有紧急公告栏，高度是动态的 */
}
</style>
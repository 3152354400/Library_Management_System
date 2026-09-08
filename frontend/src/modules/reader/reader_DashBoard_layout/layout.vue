<template>
  <div class="rdl-wrap">
    <!-- 左侧侧边栏 -->
    <DashBoardSidebar class="rdl-sidebar" />

    <!-- 右侧主区域 -->
    <div class="rdl-main">
      <DashBoardNavbar class="rdl-navbar" />
      <main class="rdl-content">
        <!-- 页面主体插槽 -->
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup>
import DashBoardSidebar from '@/modules/reader/reader_DashBoard_layout/DashBoardSidebar.vue'
import DashBoardNavbar from '@/modules/reader/reader_DashBoard_layout/DashBoardNavbar.vue'
</script>

<style scoped>
/* 两列布局：左侧固定侧边栏，右侧为内容区 */
.rdl-wrap {
  display: grid;
  grid-template-columns: 260px 1fr; /* 侧边栏宽度按需调整 */
  min-height: 100vh;
  background: #f7f9fc;
}

/* 侧边栏：整列滚动 */
.rdl-sidebar {
  position: sticky;
  top: 0;
  align-self: start;
  height: 100vh;
  overflow-y: auto;
  /* 可保持你组件内部原有样式，这里只做容器定位 */
}

/* 主区域：上方二级导航 + 内容 */
.rdl-main {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

/* 二级导航吸顶 */
.rdl-navbar {
  position: sticky;
  top: 0;
  z-index: 10;
}

/* 内容区内边距 */
.rdl-content {
  padding: 20px 24px;
  min-height: 0;
}
</style>

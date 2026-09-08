<template>
  <!-- 1. 绑定 style 属性来应用背景图片 -->
  <div 
    class="admin-layout" 
    :style="{ backgroundImage: `url(${adminBgImage})` }"
  >
    <!-- 2. 添加一个半透明的遮罩层，以确保文字在任何背景下都清晰 -->
    <div class="overlay"></div>

    <!-- 3. 将内容放在一个相对定位的容器中，使其浮于遮罩层之上 -->
    <div class="relative z-10 min-h-screen flex flex-col">
      <header class="bg-white/70 backdrop-blur-sm shadow-md sticky top-0">
        <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
          <h1 class="text-xl font-bold text-gray-800">管理员控制台</h1>
        </div>
      </header>
      <main class="p-4 sm:p-6 lg:p-8 flex-grow">
        <slot></slot>
      </main>
    </div>
  </div>
</template>

<script setup>
// 4. 导入您的背景图片，Vite会自动处理路径
import adminBgImage from '@/assets/admin.png';
</script>

<style scoped>
.admin-layout {
  /* 5. 设置背景图片的核心样式 */
  background-size: cover;
  background-position: center;
  background-attachment: fixed; /* 固定背景，内容滚动时背景不动 */
  position: relative;
}

.overlay {
  /* 6. 遮罩层样式，轻微调暗背景，让白色文字和卡片更突出 */
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.2); /* 轻微的白色蒙版 */
  z-index: 0;
}
</style>
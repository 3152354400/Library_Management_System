<template>
  <div class="dashboard-container">
    <h2 class="text-3xl font-extrabold mb-8 text-gray-800" style="text-shadow: 1px 1px 3px rgba(0,0,0,0.1);">
      功能导航
    </h2>
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">

      <!-- 1. 为每张卡片添加了独立的 border-l-4 和 边框颜色/悬停背景色 -->
      <router-link to="/admin/books" class="dashboard-card group border-l-4 border-blue-500 hover:bg-blue-50/60">
        <h3 class="card-title">图书管理</h3>
        <p class="card-description">新增、查询、修改和删除图书信息。</p>
      </router-link>

      <router-link to="/admin/category" class="dashboard-card group border-l-4 border-green-500 hover:bg-green-50/60">
        <h3 class="card-title">分类管理</h3>
        <p class="card-description">管理图书的分类层级结构。</p>
      </router-link>

      <router-link to="/admin/shelves" class="dashboard-card group bg-yellow-100 hover:bg-yellow-200">
        <h3 class="card-title text-yellow-800">书架管理</h3>
        <p class="card-description text-yellow-600">管理图书馆的物理书架位置。</p>
      </router-link>


      <router-link to="/admin/announcements" class="dashboard-card group border-l-4 border-rose-500 hover:bg-rose-50/60">
        <h3 class="card-title">公告发布</h3>
        <p class="card-description">发布图书馆最新通知与公告。</p>
      </router-link>

      <router-link to="/admin/reports" class="dashboard-card group border-l-4 border-red-500 hover:bg-red-50/60">
        <h3 class="card-title">举报处理</h3>
        <p class="card-description">处理用户提交的各类举报信息。</p>
      </router-link>

      <router-link to="/admin/readers" class="dashboard-card group border-l-4 border-red-500 hover:bg-red-50/60">
        <h3 class="card-title">读者管理</h3>
        <p class="card-description">查看并管理图书馆读者信息</p>
      </router-link>

      <router-link to="/admin/book-category" class="dashboard-card group border-l-4 border-teal-500 hover:bg-teal-50/60">
        <h3 class="card-title text-teal-800">书籍分类</h3>
        <p class="card-description text-teal-600">为指定的图书绑定或更新其所属分类。</p>
      </router-link>



    </div>
  </div>
</template>

<style scoped>
/* 2. 修改了基础卡片样式，移除了统一的 border，为单独设置颜色做准备 */
.dashboard-card {
  @apply block p-8 rounded-lg shadow-lg cursor-pointer transition-all duration-300 transform hover:-translate-y-2 hover:shadow-2xl bg-white/70 backdrop-blur-md;
  /* 移除了 border 和 border-white/30 */
}

.card-title {
  @apply text-xl font-bold mb-3 text-gray-800;
}

.card-description {
  @apply text-sm text-gray-600 font-medium;
}
</style>

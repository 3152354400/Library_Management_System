<template>
  <div class="about-page">
    <!-- 1. 英雄区 (Hero Section) -->
    <header class="hero-section" :style="{ backgroundImage: `url('${heroImageUrl}')` }">
      <div class="hero-overlay"></div>
      <div class="hero-content">
        <h1 class="hero-title">智能 · 便捷 · 开放</h1>
        <p class="hero-subtitle">欢迎来到新一代图书馆管理系统</p>
      </div>
    </header>

    <main class="content-wrapper">
      <!-- 2. 项目简介区 (已替换为纯文字) -->
      <section class="intro-section">
        <div class="intro-text-full">
          <h2 class="section-title">关于我们的系统</h2>
          <p class="section-paragraph">
            本图书馆管理系统是一个全面、现代化的解决方案，旨在通过先进的技术手段，为读者、图书管理员乃至整个图书馆的运营管理带来革命性的体验。我们深信，知识的获取与分享应当是轻松、愉悦且无障碍的。因此，我们倾力打造了一个集资源检索、空间预约、个性化服务与后台智能管理于一体的综合性平台。
          </p>
          <p class="section-paragraph">
            我们的系统核心理念是**“以用户为中心”**。对于读者而言，这意味着一个更加直观的图书查询界面、一个公平且易于使用的座位预约系统，以及一个能够记录个人阅读足迹的“我的图书馆”空间。对于管理员，我们提供了一套功能强大且操作清晰的后台，涵盖了从图书入库、分类编目到数据驱动的采购分析，极大地提升了管理效率和决策的科学性。
          </p>
          <p class="section-paragraph">
            我们致力于打破传统图书馆的信息壁垒和操作复杂性，通过技术创新，让图书馆不仅是一个藏书之地，更是一个充满活力、智慧互联的知识社区。
          </p>
        </div>
      </section>

      <!-- 3. 核心功能区 (保持不变) -->
      <section class="features-section">
        <h2 class="section-title">核心功能</h2>
        <div class="features-grid">
          <div class="feature-card">
            <h3 class="feature-title">可视化空间预约</h3>
            <p>通过直观的座位地图，实时查看和预约您的学习空间，告别占座烦恼。</p>
          </div>
          <div class="feature-card">
            <h3 class="feature-title">全方位图书管理</h3>
            <p>为管理员提供强大的图书入库、信息修改、副本管理和分类绑定工具。</p>
          </div>
          <div class="feature-card">
            <h3 class="feature-title">数据驱动决策</h3>
            <p>通过多维度的数据分析排行榜，为采购和馆藏优化提供科学依据。</p>
          </div>
        </div>
      </section>
      
    </main>
  </div>
</template>

<script setup>
// 1. 将图片路径改为您提供的 URL
const heroImageUrl = 'https://i.ibb.co/4R6rN8hv/banner1.jpg';

// 2. 不再需要本地的架构图
// import archImage from '@/assets/image.png'; 
</script>

<style scoped>
.about-page {
  background-color: #f8fafc;
}

/* 英雄区 */
.hero-section {
  position: relative;
  height: 60vh;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: white;
  background-size: cover;
  background-position: center;
}
.hero-overlay {
  position: absolute;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.5);
}
.hero-content {
  position: relative;
  z-index: 1;
}
.hero-title {
  font-size: 3rem;
  font-weight: 800;
  text-shadow: 0 2px 4px rgba(0,0,0,0.5);
}
.hero-subtitle {
  font-size: 1.25rem;
  margin-top: 1rem;
  opacity: 0.9;
}

/* 内容区 */
.content-wrapper {
  max-width: 900px; /* 稍微缩小宽度以适应纯文本 */
  margin: 0 auto;
  padding: 4rem 2rem;
}
.section-title {
  font-size: 2rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 2rem;
  text-align: center;
}
.section-paragraph {
  color: #475569;
  line-height: 1.8;
  margin-bottom: 1.5rem; /* 加大段间距 */
  font-size: 1.1rem; /* 稍微增大字号 */
}

/* 3. 移除两栏布局，使用新的单栏全宽文本布局 */
.intro-section {
  margin-bottom: 5rem;
}
.intro-text-full {
  max-width: 800px; /* 限制文本宽度以提高可读性 */
  margin: 0 auto;
}

/* 功能区 (样式保持不变) */
.features-section {
  padding-top: 3rem;
  border-top: 1px solid #e2e8f0;
}
.features-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
}
.feature-card {
  background-color: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
  text-align: center;
}
.feature-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #004b8d;
  margin-bottom: 1rem;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .features-grid {
    grid-template-columns: 1fr;
  }
  .hero-title { font-size: 2.5rem; }
  .section-title { font-size: 1.8rem; }
}
</style>
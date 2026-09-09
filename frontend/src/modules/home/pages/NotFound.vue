<template>
  <div class="not-found">
    <div class="nf-art">
      <svg viewBox="0 0 200 200" width="200" height="200" aria-hidden="true">
        <circle cx="100" cy="100" r="80" fill="#EEF1FE"/>
        <text x="100" y="115" text-anchor="middle" font-size="72" font-weight="700" fill="#3B5BDB">404</text>
      </svg>
    </div>
    <h1>页面走丢了</h1>
    <p>您访问的页面不存在或已被移除。</p>
    <el-button type="primary" @click="$router.push('/')">返回首页</el-button>
  </div>
</template>

<style scoped>
.not-found {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: var(--color-bg);
  padding: 40px;
}
h1 { font-size: var(--fs-3xl); margin: 24px 0 8px; }
p { color: var(--color-text-soft); margin-bottom: 24px; }
</style>
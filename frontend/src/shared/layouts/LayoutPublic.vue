<template>
  <div class="public-layout">
    <header class="topbar">
      <div class="topbar-inner">
        <router-link to="/" class="brand">
          <svg width="32" height="32" viewBox="0 0 64 64" aria-hidden="true">
            <defs>
              <linearGradient id="bgLogo" x1="0" y1="0" x2="1" y2="1">
                <stop offset="0" stop-color="#3B5BDB"/>
                <stop offset="1" stop-color="#5C7CFA"/>
              </linearGradient>
            </defs>
            <rect x="6" y="10" width="6" height="44" rx="2" fill="url(#bgLogo)"/>
            <rect x="14" y="14" width="6" height="40" rx="2" fill="#748FFC"/>
            <rect x="22" y="18" width="6" height="36" rx="2" fill="#91A7FF"/>
            <rect x="30" y="14" width="6" height="40" rx="2" fill="#748FFC"/>
            <rect x="38" y="10" width="6" height="44" rx="2" fill="url(#bgLogo)"/>
            <rect x="46" y="16" width="6" height="38" rx="2" fill="#91A7FF"/>
          </svg>
          <span class="brand-name">智书云</span>
          <span class="brand-tag">图书管理系统</span>
        </router-link>

        <nav class="topnav">
          <router-link to="/" exact-active-class="active">首页</router-link>
          <router-link to="/books" active-class="active">图书分类</router-link>
          <router-link to="/announcements" active-class="active">公告</router-link>
          <router-link to="/about" active-class="active">关于</router-link>
        </nav>

        <div class="topbar-actions">
          <template v-if="!userStore.isLoggedIn">
            <el-button text @click="$router.push('/auth')">登录</el-button>
            <el-button type="primary" @click="$router.push('/auth?mode=register')">注册</el-button>
          </template>
          <template v-else>
            <el-dropdown @command="onCommand">
              <div class="user-chip">
                <img :src="avatarUrl" class="avatar" />
                <span class="name">{{ userStore.nickName || userStore.userName }}</span>
                <el-icon><ArrowDown /></el-icon>
              </div>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="reader" v-if="userStore.isReader">
                    <el-icon><UserFilled /></el-icon> 读者中心
                  </el-dropdown-item>
                  <el-dropdown-item command="admin" v-if="userStore.isAdmin">
                    <el-icon><Setting /></el-icon> 管理后台
                  </el-dropdown-item>
                  <el-dropdown-item divided command="logout">
                    <el-icon><SwitchButton /></el-icon> 退出登录
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </template>
        </div>
      </div>
    </header>

    <main class="main-content">
      <router-view />
    </main>

    <footer class="footer">
      <div class="footer-inner">
        <div class="footer-brand">
          <strong>智书云 · 图书管理系统</strong>
          <span>基于 Oracle 19c + ASP.NET Core 8 + Vue 3 课程设计</span>
        </div>
        <div class="footer-links">
          <a href="#">使用帮助</a>
          <a href="#">联系管理员</a>
          <span>© 2026 智书云团队</span>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const userStore = useUserStore()
const baseAvatarUrl = import.meta.env.VITE_BASE_AVATAR_URL || '/avatars/'
const avatarUrl = computed(() => baseAvatarUrl + (userStore.avatar || 'system_0.png'))

function onCommand(cmd) {
  if (cmd === 'reader') router.push('/reader/dashboard')
  if (cmd === 'admin') router.push('/admin/dashboard')
  if (cmd === 'logout') {
    userStore.logout()
    router.push('/')
  }
}
</script>

<style scoped>
.public-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--color-bg);
}
.topbar {
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid var(--color-border-soft);
  position: sticky;
  top: 0;
  z-index: 50;
}
.topbar-inner {
  max-width: 1280px;
  margin: 0 auto;
  padding: 0 24px;
  height: var(--topbar-height);
  display: flex;
  align-items: center;
  gap: 32px;
}
.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  text-decoration: none;
  color: var(--color-text);
}
.brand-name {
  font-size: var(--fs-xl);
  font-weight: 700;
  letter-spacing: 0.5px;
}
.brand-tag {
  font-size: var(--fs-xs);
  color: var(--color-text-muted);
  padding: 2px 8px;
  background: var(--color-primary-50);
  color: var(--color-primary-600);
  border-radius: 999px;
  margin-left: 4px;
}
.topnav {
  display: flex;
  gap: 28px;
  margin-left: 16px;
  flex: 1;
}
.topnav a {
  color: var(--color-text-soft);
  text-decoration: none;
  font-size: var(--fs-base);
  font-weight: 500;
  padding: 6px 0;
  position: relative;
}
.topnav a:hover { color: var(--color-primary-600); }
.topnav a.active {
  color: var(--color-primary-600);
}
.topnav a.active::after {
  content: '';
  position: absolute;
  bottom: -22px;
  left: 50%;
  width: 24px;
  height: 3px;
  background: var(--color-primary-600);
  border-radius: 999px;
  transform: translateX(-50%);
}
.topbar-actions { display: flex; align-items: center; gap: 12px; }
.user-chip {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 10px 4px 4px;
  border-radius: 999px;
  background: var(--color-bg);
  cursor: pointer;
  font-size: var(--fs-sm);
}
.user-chip:hover { background: var(--color-primary-50); }
.avatar {
  width: 32px; height: 32px; border-radius: 999px;
  background: var(--color-primary-100);
  object-fit: cover;
}
.main-content { flex: 1; }
.footer {
  background: #1f2a44;
  color: rgba(255, 255, 255, 0.7);
  margin-top: 64px;
}
.footer-inner {
  max-width: 1280px;
  margin: 0 auto;
  padding: 36px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}
.footer-brand { display: flex; flex-direction: column; gap: 4px; }
.footer-brand strong { color: #fff; font-size: var(--fs-base); }
.footer-brand span { font-size: var(--fs-xs); }
.footer-links { display: flex; gap: 20px; font-size: var(--fs-sm); }
.footer-links a { color: rgba(255, 255, 255, 0.7); }
.footer-links a:hover { color: #fff; }
@media (max-width: 768px) {
  .topnav { display: none; }
  .topbar-inner { gap: 12px; }
  .brand-tag { display: none; }
  .footer-inner { flex-direction: column; align-items: flex-start; }
}
</style>
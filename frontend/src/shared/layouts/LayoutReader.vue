<template>
  <div class="reader-layout" :class="{ 'sidebar-collapsed': appStore.sidebarCollapsed }">
    <aside class="sidebar" :class="{ 'mobile-open': appStore.mobileDrawerVisible }">
      <div class="sidebar-header">
        <router-link to="/" class="brand">
          <svg width="28" height="28" viewBox="0 0 64 64" aria-hidden="true">
            <rect x="6" y="10" width="6" height="44" rx="2" fill="#fff" opacity=".9"/>
            <rect x="14" y="14" width="6" height="40" rx="2" fill="#fff" opacity=".7"/>
            <rect x="22" y="18" width="6" height="36" rx="2" fill="#fff" opacity=".5"/>
            <rect x="30" y="14" width="6" height="40" rx="2" fill="#fff" opacity=".7"/>
            <rect x="38" y="10" width="6" height="44" rx="2" fill="#fff" opacity=".9"/>
            <rect x="46" y="16" width="6" height="38" rx="2" fill="#fff" opacity=".6"/>
          </svg>
          <transition name="fade">
            <div v-if="!appStore.sidebarCollapsed" class="brand-text">
              <div class="brand-name">智书云</div>
              <div class="brand-sub">读者中心</div>
            </div>
          </transition>
        </router-link>
      </div>

      <nav class="nav">
        <div v-for="group in navGroups" :key="group.label" class="nav-group">
          <div v-if="!appStore.sidebarCollapsed" class="nav-group-title">{{ group.label }}</div>
          <router-link
            v-for="item in group.items"
            :key="item.path"
            :to="item.path"
            class="nav-item"
            active-class="nav-item-active"
            @click="appStore.closeMobileDrawer"
          >
            <el-icon class="nav-icon"><component :is="item.icon" /></el-icon>
            <span v-if="!appStore.sidebarCollapsed" class="nav-label">{{ item.label }}</span>
            <el-badge v-if="item.badge && !appStore.sidebarCollapsed" :value="item.badge" class="nav-badge" />
          </router-link>
        </div>
      </nav>

      <div class="sidebar-footer">
        <button class="collapse-btn" @click="appStore.toggleSidebar">
          <el-icon><component :is="appStore.sidebarCollapsed ? 'Expand' : 'Fold'" /></el-icon>
          <span v-if="!appStore.sidebarCollapsed">收起</span>
        </button>
      </div>
    </aside>

    <div class="sidebar-mask" v-if="appStore.mobileDrawerVisible" @click="appStore.closeMobileDrawer" />

    <div class="main">
      <header class="topbar">
        <div class="topbar-left">
          <el-button class="hide-on-pc" text @click="appStore.openMobileDrawer">
            <el-icon :size="20"><Menu /></el-icon>
          </el-button>
          <div class="bread">
            <span class="bread-tag">读者中心</span>
            <span class="bread-current">{{ $route.meta?.title || '我的主页' }}</span>
          </div>
        </div>
        <div class="topbar-right">
          <NotificationBell />
          <el-dropdown @command="onCommand">
            <div class="user-chip">
              <img :src="avatarUrl" class="avatar" />
              <div class="user-info">
                <div class="name">{{ userStore.nickName || userStore.userName }}</div>
                <div class="role">信用 {{ userStore.creditScore }}</div>
              </div>
              <el-icon><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="home"><el-icon><HomeFilled /></el-icon> 返回首页</el-dropdown-item>
                <el-dropdown-item command="profile"><el-icon><User /></el-icon> 个人资料</el-dropdown-item>
                <el-dropdown-item divided command="logout"><el-icon><SwitchButton /></el-icon> 退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </header>

      <div class="page">
        <router-view />
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useAppStore } from '@/stores/app'
import { useNotificationStore } from '@/stores/notification'
import NotificationBell from './NotificationBell.vue'

const router = useRouter()
const userStore = useUserStore()
const appStore = useAppStore()
const notificationStore = useNotificationStore()

const baseAvatarUrl = import.meta.env.VITE_BASE_AVATAR_URL || '/avatars/'
const avatarUrl = computed(() => baseAvatarUrl + (userStore.avatar || 'system_0.png'))

const navGroups = computed(() => [
  {
    label: '概览',
    items: [
      { path: '/reader/dashboard', label: '我的主页', icon: 'Odometer' }
    ]
  },
  {
    label: '借阅流通',
    items: [
      { path: '/reader/borrowing', label: '我的借阅', icon: 'Reading' },
      { path: '/reader/reserves', label: '我的预约', icon: 'Calendar' },
      { path: '/reader/fines', label: '我的罚款', icon: 'Money' },
      { path: '/reader/seat', label: '座位预约', icon: 'OfficeBuilding' }
    ]
  },
  {
    label: '互动推荐',
    items: [
      { path: '/reader/favorites', label: '我的收藏', icon: 'Star' },
      { path: '/reader/booklist', label: '我的书单', icon: 'Notebook' },
      { path: '/reader/recommends', label: '我的荐购', icon: 'Promotion' },
      { path: '/reader/notifications', label: '我的消息', icon: 'Bell', badge: notificationStore.unreadCount }
    ]
  },
  {
    label: '账号',
    items: [
      { path: '/reader/profile', label: '个人资料', icon: 'User' },
      { path: '/reader/security', label: '账号安全', icon: 'Lock' }
    ]
  }
])

function onCommand(cmd) {
  if (cmd === 'home') router.push('/')
  if (cmd === 'profile') router.push('/reader/profile')
  if (cmd === 'logout') {
    userStore.logout()
    router.push('/auth')
  }
}
</script>

<style scoped>
.reader-layout {
  display: grid;
  grid-template-columns: var(--sidebar-width) 1fr;
  min-height: 100vh;
  background: var(--color-bg);
}
.reader-layout.sidebar-collapsed {
  grid-template-columns: var(--sidebar-collapsed-width) 1fr;
}
.sidebar {
  background: linear-gradient(180deg, #2B3D9E 0%, #3B5BDB 100%);
  color: #fff;
  display: flex;
  flex-direction: column;
  position: sticky;
  top: 0;
  height: 100vh;
  overflow: hidden;
}
.sidebar-header {
  padding: 18px 18px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}
.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #fff;
  text-decoration: none;
}
.brand-text .brand-name { font-size: var(--fs-lg); font-weight: 700; line-height: 1.2; }
.brand-text .brand-sub { font-size: var(--fs-xs); opacity: 0.7; }
.nav { flex: 1; overflow-y: auto; padding: 16px 12px; }
.nav-group { margin-bottom: 16px; }
.nav-group-title {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 1px;
  opacity: 0.5;
  padding: 0 12px 8px;
}
.nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  border-radius: var(--radius-md);
  color: rgba(255, 255, 255, 0.85);
  text-decoration: none;
  font-size: var(--fs-base);
  font-weight: 500;
  margin-bottom: 2px;
  position: relative;
  transition: background .2s;
}
.nav-item:hover { background: rgba(255, 255, 255, 0.08); color: #fff; }
.nav-item-active {
  background: rgba(255, 255, 255, 0.18);
  color: #fff;
}
.nav-item-active::before {
  content: '';
  position: absolute;
  left: 0;
  top: 8px;
  bottom: 8px;
  width: 3px;
  background: #fff;
  border-radius: 0 3px 3px 0;
}
.nav-icon { font-size: 18px; flex-shrink: 0; }
.nav-label { flex: 1; }
.nav-badge { margin-left: auto; }
.sidebar-footer {
  padding: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}
.collapse-btn {
  width: 100%;
  background: rgba(255, 255, 255, 0.06);
  border: none;
  color: #fff;
  padding: 8px;
  border-radius: var(--radius-md);
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: var(--fs-sm);
}
.collapse-btn:hover { background: rgba(255, 255, 255, 0.15); }
.main {
  display: flex;
  flex-direction: column;
  min-width: 0;
}
.topbar {
  background: var(--color-bg-elevated);
  border-bottom: 1px solid var(--color-border-soft);
  padding: 12px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  position: sticky;
  top: 0;
  z-index: 20;
}
.topbar-left { display: flex; align-items: center; gap: 16px; }
.bread {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: var(--fs-sm);
}
.bread-tag {
  background: var(--color-primary-50);
  color: var(--color-primary-600);
  padding: 2px 10px;
  border-radius: 999px;
  font-weight: 500;
}
.bread-current { color: var(--color-text); font-weight: 600; }
.topbar-right { display: flex; align-items: center; gap: 16px; }
.user-chip {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  padding: 4px 12px 4px 4px;
  border-radius: 999px;
}
.user-chip:hover { background: var(--color-bg); }
.avatar {
  width: 36px; height: 36px;
  border-radius: 999px;
  object-fit: cover;
  background: var(--color-primary-100);
}
.user-info { line-height: 1.2; }
.user-info .name { font-size: var(--fs-sm); font-weight: 600; }
.user-info .role { font-size: var(--fs-xs); color: var(--color-text-muted); }
.page { padding: 24px; flex: 1; }

.sidebar-mask {
  display: none;
}
@media (max-width: 1024px) {
  .reader-layout { grid-template-columns: var(--sidebar-collapsed-width) 1fr; }
  .sidebar {
    position: fixed;
    left: 0; top: 0;
    width: var(--sidebar-width);
    z-index: 100;
    transform: translateX(-100%);
    transition: transform .25s;
  }
  .sidebar.mobile-open { transform: translateX(0); }
  .sidebar-mask {
    display: block;
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.4);
    z-index: 99;
  }
  .collapse-btn { display: none; }
  .hide-on-pc { display: inline-flex !important; }
}
@media (min-width: 1025px) {
  .hide-on-pc { display: none !important; }
}
</style>
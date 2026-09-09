<template>
  <div class="admin-layout" :class="{ 'sidebar-collapsed': appStore.sidebarCollapsed }">
    <aside class="sidebar">
      <div class="sidebar-header">
        <router-link to="/admin/dashboard" class="brand">
          <svg width="28" height="28" viewBox="0 0 64 64" aria-hidden="true">
            <rect x="6" y="10" width="6" height="44" rx="2" fill="#fff" opacity=".9"/>
            <rect x="14" y="14" width="6" height="40" rx="2" fill="#fff" opacity=".7"/>
            <rect x="22" y="18" width="6" height="36" rx="2" fill="#fff" opacity=".5"/>
            <rect x="30" y="14" width="6" height="40" rx="2" fill="#fff" opacity=".7"/>
            <rect x="38" y="10" width="6" height="44" rx="2" fill="#fff" opacity=".9"/>
          </svg>
          <transition name="fade">
            <div v-if="!appStore.sidebarCollapsed" class="brand-text">
              <div class="brand-name">智书云</div>
              <div class="brand-sub">管理后台</div>
            </div>
          </transition>
        </router-link>
      </div>

      <nav class="nav">
        <router-link
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          class="nav-item"
          active-class="nav-item-active"
        >
          <el-icon class="nav-icon"><component :is="item.icon" /></el-icon>
          <span v-if="!appStore.sidebarCollapsed" class="nav-label">{{ item.label }}</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <button class="collapse-btn" @click="appStore.toggleSidebar">
          <el-icon><component :is="appStore.sidebarCollapsed ? 'Expand' : 'Fold'" /></el-icon>
          <span v-if="!appStore.sidebarCollapsed">收起</span>
        </button>
      </div>
    </aside>

    <div class="main">
      <header class="topbar">
        <div class="topbar-left">
          <el-breadcrumb separator="/">
            <el-breadcrumb-item :to="{ path: '/admin/dashboard' }">管理后台</el-breadcrumb-item>
            <el-breadcrumb-item>{{ $route.meta?.title || '' }}</el-breadcrumb-item>
          </el-breadcrumb>
        </div>
        <div class="topbar-right">
          <el-button text @click="$router.push('/')">
            <el-icon><HomeFilled /></el-icon>
            <span style="margin-left: 6px">前台首页</span>
          </el-button>
          <el-dropdown @command="onCommand">
            <div class="user-chip">
              <div class="avatar">{{ userStore.userName?.slice(0, 2) || '管' }}</div>
              <div class="user-info">
                <div class="name">{{ userStore.fullName || userStore.userName || '管理员' }}</div>
                <div class="role">高级管理员</div>
              </div>
              <el-icon><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="logout"><el-icon><SwitchButton /></el-icon> 退出登录</el-dropdown-item>
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
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useAppStore } from '@/stores/app'

const router = useRouter()
const userStore = useUserStore()
const appStore = useAppStore()

const navItems = [
  { path: '/admin/dashboard', label: '仪表盘', icon: 'Odometer' },
  { path: '/admin/books', label: '图书管理', icon: 'Reading' },
  { path: '/admin/categories', label: '分类管理', icon: 'Files' },
  { path: '/admin/shelves', label: '书架与副本', icon: 'OfficeBuilding' },
  { path: '/admin/borrowing', label: '借阅记录', icon: 'Tickets' },
  { path: '/admin/fines', label: '罚款管理', icon: 'Money' },
  { path: '/admin/book-loss', label: '丢书上报', icon: 'WarningFilled' },
  { path: '/admin/readers', label: '读者管理', icon: 'UserFilled' },
  { path: '/admin/recommends', label: '荐购处理', icon: 'Promotion' },
  { path: '/admin/reports', label: '评论举报', icon: 'ChatLineRound' },
  { path: '/admin/announcements', label: '公告管理', icon: 'BellFilled' },
  { path: '/admin/purchase-analysis', label: '采购分析', icon: 'DataAnalysis' }
]

function onCommand(cmd) {
  if (cmd === 'logout') {
    userStore.logout()
    router.push('/auth')
  }
}
</script>

<style scoped>
.admin-layout {
  display: grid;
  grid-template-columns: var(--sidebar-width) 1fr;
  min-height: 100vh;
  background: var(--color-bg);
}
.admin-layout.sidebar-collapsed { grid-template-columns: var(--sidebar-collapsed-width) 1fr; }
.sidebar {
  background: #fff;
  border-right: 1px solid var(--color-border-soft);
  display: flex;
  flex-direction: column;
  position: sticky;
  top: 0;
  height: 100vh;
  overflow: hidden;
}
.sidebar-header { padding: 16px 18px; border-bottom: 1px solid var(--color-border-soft); }
.brand { display: flex; align-items: center; gap: 10px; text-decoration: none; color: var(--color-text); }
.brand-name { font-size: var(--fs-lg); font-weight: 700; line-height: 1.2; }
.brand-sub { font-size: var(--fs-xs); color: var(--color-text-muted); }
.nav { flex: 1; overflow-y: auto; padding: 12px; }
.nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 12px;
  border-radius: var(--radius-md);
  color: var(--color-text-soft);
  text-decoration: none;
  font-size: var(--fs-base);
  margin-bottom: 2px;
  font-weight: 500;
}
.nav-item:hover { background: var(--color-bg); color: var(--color-primary-600); }
.nav-item-active {
  background: var(--color-primary-50);
  color: var(--color-primary-600);
  font-weight: 600;
}
.nav-icon { font-size: 18px; }
.nav-label { flex: 1; }
.sidebar-footer { padding: 12px; border-top: 1px solid var(--color-border-soft); }
.collapse-btn {
  width: 100%;
  background: var(--color-bg);
  border: none;
  color: var(--color-text-soft);
  padding: 8px;
  border-radius: var(--radius-md);
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: var(--fs-sm);
}
.collapse-btn:hover { background: var(--color-primary-50); color: var(--color-primary-600); }
.main { display: flex; flex-direction: column; min-width: 0; }
.topbar {
  background: #fff;
  border-bottom: 1px solid var(--color-border-soft);
  padding: 12px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  position: sticky; top: 0; z-index: 20;
}
.topbar-right { display: flex; align-items: center; gap: 16px; }
.user-chip {
  display: flex; align-items: center; gap: 10px;
  cursor: pointer;
  padding: 4px 12px 4px 4px;
  border-radius: 999px;
}
.user-chip:hover { background: var(--color-bg); }
.avatar {
  width: 36px; height: 36px;
  border-radius: 999px;
  background: linear-gradient(135deg, #3B5BDB, #748FFC);
  color: #fff;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
}
.user-info { line-height: 1.2; }
.user-info .name { font-size: var(--fs-sm); font-weight: 600; }
.user-info .role { font-size: var(--fs-xs); color: var(--color-text-muted); }
.page { padding: 24px; flex: 1; }
@media (max-width: 768px) {
  .admin-layout { grid-template-columns: 1fr; }
  .sidebar { display: none; }
}
</style>
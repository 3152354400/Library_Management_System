/**
 * stores/app.js —— 侧边栏折叠、暗色模式等
 */
import { defineStore } from 'pinia'

export const useAppStore = defineStore('app', {
  state: () => ({
    sidebarCollapsed: false,
    mobileDrawerVisible: false,
    theme: 'light'
  }),
  actions: {
    hydrate() {
      this.sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true'
      this.theme = localStorage.getItem('theme') || 'light'
    },
    toggleSidebar() {
      this.sidebarCollapsed = !this.sidebarCollapsed
      localStorage.setItem('sidebarCollapsed', this.sidebarCollapsed)
    },
    openMobileDrawer() { this.mobileDrawerVisible = true },
    closeMobileDrawer() { this.mobileDrawerVisible = false },
    setTheme(t) {
      this.theme = t
      localStorage.setItem('theme', t)
      document.documentElement.dataset.theme = t
    }
  }
})
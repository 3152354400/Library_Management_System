/**
 * stores/notification.js —— 通知状态 + ws 绑定
 */
import { defineStore } from 'pinia'
import { ElNotification } from 'element-plus'
import http from '@/services/http'
import ws from '@/services/websocket'

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    list: [],
    unreadCount: 0,
    totalCount: 0
  }),
  actions: {
    bindWsListeners() {
      ws.onMessage((data) => {
        if (data.type === 'notification' && data.payload) {
          this.unreadCount += 1
          this.list.unshift(data.payload)
          ElNotification({
            title: data.payload.Title || '新通知',
            message: data.payload.Content || '',
            type: data.payload.Priority === 'high' ? 'warning' : 'info',
            duration: 4500
          })
        }
      })
    },
    async fetchList(params = {}) {
      const data = await http.get('/reader/extension/notifications', { params, mock: 'notification.list' })
      this.list = data?.Notifications || []
      this.totalCount = data?.TotalCount || 0
      this.unreadCount = data?.UnreadCount || 0
      return data
    },
    async fetchUnreadCount() {
      const data = await http.get('/reader/extension/notifications/unread/count', { mock: 'notification.unread' })
      this.unreadCount = data?.unreadCount || 0
    },
    async markRead(id) {
      await http.put(`/reader/extension/notifications/${id}/read`, null, { mock: 'notification.markRead' })
      const item = this.list.find((n) => n.NotificationId === id)
      if (item) item.IsRead = 'Y'
      this.unreadCount = Math.max(0, this.unreadCount - 1)
    },
    async markAllRead() {
      await http.put('/reader/extension/notifications/read/all', null, { mock: 'notification.markAllRead' })
      this.list.forEach((n) => (n.IsRead = 'Y'))
      this.unreadCount = 0
    }
  }
})
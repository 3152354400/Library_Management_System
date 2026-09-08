import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/services/http.js' // 确认 http.js 的路径


export const useAnnouncementStore = defineStore('announcements', () => {
  const urgentAnnouncements = ref([])
  const regularAnnouncements = ref([])
  const hasFetched = ref(false)

  async function fetchPublicAnnouncements(force = false) {
    // 如果不是强制刷新，并且已经获取过，则直接返回
    if (hasFetched.value && !force) {
      return;
    }
    
    try {
      // 标记为正在获取，避免重复请求
      hasFetched.value = true;
      const response = await http.get('/announcements/public');
      urgentAnnouncements.value = response.data.Urgent || [];
      regularAnnouncements.value = response.data.Regular || [];
    } catch (error) {
      console.error('Failed to fetch public announcements:', error);
      // 如果获取失败，重置 hasFetched 状态，以便下次可以重试
      hasFetched.value = false; 
    }
  }

  return { 
    urgentAnnouncements, 
    regularAnnouncements, 
    // 不再需要暴露 hasFetched
    fetchPublicAnnouncements 
  }
})
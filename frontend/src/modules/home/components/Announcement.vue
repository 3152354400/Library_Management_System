<template>
  <div class="announcement-section">
    <h2 class="section-title">最新公告</h2>
    <div class="announcement-list">
      <div v-if="regularAnnouncements.length === 0" class="no-announcements">
        暂无最新公告
      </div>
      <div v-for="ann in regularAnnouncements" :key="ann.AnnouncementID" class="announcement-item" @click="openModal(ann)">
        <span class="date">{{ formatDate(ann.CreateTime) }}</span>
        <span class="title">{{ ann.Title }}</span>
      </div>
    </div>
  </div>

  <!-- 公告详情弹窗 -->
  <AnnouncementModal v-if="selectedAnnouncement" :announcement="selectedAnnouncement" @close="closeModal" />
</template>

<script setup>
import { ref } from 'vue'
import { useAnnouncementStore } from '@/stores/announcementStore'
import { storeToRefs } from 'pinia'
import AnnouncementModal from './AnnouncementModal.vue' // 引入弹窗组件

const announcementStore = useAnnouncementStore()
const { regularAnnouncements } = storeToRefs(announcementStore)

const selectedAnnouncement = ref(null)

function openModal(announcement) {
  selectedAnnouncement.value = announcement
}

function closeModal() {
  selectedAnnouncement.value = null
}

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString()
}
</script>

<style scoped>
/* 样式可以根据您的喜好调整 */
.announcement-section { padding: 2rem 1rem; max-width: 1200px; margin: auto; }
.section-title { font-size: 1.8rem; font-weight: bold; text-align: center; margin-bottom: 2rem; }
.announcement-list { border-top: 1px solid #e5e7eb; }
.announcement-item { display: flex; justify-content: space-between; padding: 1rem; border-bottom: 1px solid #e5e7eb; cursor: pointer; transition: background-color 0.2s ease; }
.announcement-item:hover { background-color: #f9fafb; }
.date { color: #6b7280; }
.title { font-weight: 500; color: #111827; }
.no-announcements { text-align: center; padding: 2rem; color: #6b7280; }
</style>
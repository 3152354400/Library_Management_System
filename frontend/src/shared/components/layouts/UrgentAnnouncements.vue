<template>
  <div v-if="urgentAnnouncements.length > 0" class="urgent-announcements-banner">
    <div class="container">
      <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 20 20" fill="currentColor">
        <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
      </svg>
      <div class="content">
        <p v-for="ann in urgentAnnouncements" :key="ann.AnnouncementID">
          <span class="font-bold">[紧急]</span> {{ ann.Title }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAnnouncementStore } from '@/stores/announcementStore'
import { storeToRefs } from 'pinia'

const announcementStore = useAnnouncementStore()
const { urgentAnnouncements } = storeToRefs(announcementStore)
</script>

<style scoped>
.urgent-announcements-banner {
  background-color: #fffbe6; /* 淡黄色 */
  border-bottom: 1px solid #fde68a;
  color: #92400e;
  padding: 0.75rem 1rem;
  width: 100%;
  position: sticky; /* 将其固定在导航栏下方 */
  top: 3.5rem; /* 假设滚动后导航栏高度为 3.5rem */
  z-index: 999;
  transition: top 0.3s ease;
}
.navbar.scrolled + .urgent-announcements-banner {
  top: 3.5rem;
}
.container {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  align-items: center;
}
.icon {
  width: 1.25rem;
  height: 1.25rem;
  margin-right: 0.75rem;
  flex-shrink: 0;
}
.content {
  font-size: 0.875rem;
}
</style>
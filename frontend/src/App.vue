<template>
  <component :is="layoutComponent">
    <router-view />
  </component>
</template>

<script setup>
import { computed, onMounted } from 'vue' // 1. 引入 onMounted
import { useRoute } from 'vue-router'
// 导入您已有的默认布局和我们新增的管理员布局
import LayoutDefault from '@/shared/components/layouts/LayoutDefault.vue'
import LayoutAdmin from '@/shared/components/layouts/LayoutAdmin.vue'
import { useAnnouncementStore } from '@/stores/announcementStore' // 2. 引入 store

const route = useRoute()

// 根据路由的 meta.layout 字段，动态决定使用哪个布局组件
const layoutComponent = computed(() => {
  if (route.meta.layout === 'Admin') {
    return LayoutAdmin
  }
  // 如果没有指定，则默认使用您的 LayoutDefault
  return LayoutDefault
})

// 3. 在应用根组件挂载时，立即获取一次公开公告数据
const announcementStore = useAnnouncementStore()
onMounted(() => {
  announcementStore.fetchPublicAnnouncements()
})

</script>

<style>
  html, body, #app {
    margin: 0;
    padding: 0;
    width: 100%;
    height: 100%;
    overflow-x: hidden;
  }
</style>
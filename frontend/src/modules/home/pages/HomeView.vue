<template>

  <!-- 首页轮播图和图书搜索栏 -->
  <Banner />

  <!-- 快捷入口模块 -->
  <QuickEntry />

  <!-- 推荐图书模块 -->
  <BookRecommend v-if="isLoggedIn"/>

  <!-- 公告栏模块 -->
  <Announcement />

</template>

<script setup>
import Banner from '@/modules/home/components/Banner.vue'
import QuickEntry from '@/modules/home/components/QuickEntry.vue'
import BookRecommend from '@/modules/home/components/BookRecommend.vue'
import Announcement from '@/modules/home/components/Announcement.vue'
import { ref } from 'vue'

const isLoggedIn = ref(!!localStorage.getItem('token'))
</script>

<style scoped>
</style>

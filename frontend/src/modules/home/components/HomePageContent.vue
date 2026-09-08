<template>
  <!-- 首页轮播图和图书搜索栏 -->
  <Banner />

  <!-- 快捷入口模块 -->
  <QuickEntry />

  <!-- 推荐图书模块 -->
  <BookRecommend />

  <!-- 公告栏模块 (我们将在下一步修复它) -->
  <Announcement />
</template>

<script setup>
// 导入这些组件
import Banner from '@/modules/home/components/Banner.vue';
import QuickEntry from '@/modules/home/components/QuickEntry.vue';
import BookRecommend from '@/modules/home/components/BookRecommend.vue';
import Announcement from '@/modules/home/components/Announcement.vue';
</script>
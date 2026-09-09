<template>
  <section class="banner">
    <swiper
      :modules="[Autoplay, Pagination, EffectFade]"
      :autoplay="{ delay: 4000 }"
      :loop="true"
      :pagination="{ clickable: true }"
      effect="fade"
      class="banner-swiper"
    >
      <swiper-slide v-for="(item, index) in images" :key="index">
        <div
          class="banner-image"
          :style="{ backgroundImage: `url(${item})` }"
        />
      </swiper-slide>
    </swiper>

    <div class="banner-center">
      <BookSearchBar />
    </div>
  </section>
</template>

<script setup>
import { Swiper, SwiperSlide } from 'swiper/vue'
import { Autoplay, Pagination, EffectFade } from 'swiper/modules'
import 'swiper/css'
import 'swiper/css/pagination'
import 'swiper/css/effect-fade'
import BookSearchBar from '@/modules/home/components/BookSearchBar.vue'

const images = [
  'https://i.ibb.co/4R6rN8hv/banner1.jpg',
  'https://i.ibb.co/S4tMtN9F/banner2.jpg',
  // 'https://i.ibb.co/qMZqsB6S/banner3.jpg',
  // 'https://i.ibb.co/8g7JW9SX/banner4.jpg',
  // 'https://i.ibb.co/tpqyX5dV/banner5.jpg',
]
</script>

<style scoped>
.banner {
 position: relative;
  width: 100%;
  height: 80vh;
  background-color: #eef5fb;
  overflow: hidden;
}

.banner-swiper {
  width: 100%;
  height: 100%;
}

.banner-image {
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
}

.banner-center {
  position: absolute;
  top: 60%;
  left: 50%;
  transform: translate(-50%, -50%); /* 把自身中心点对齐父容器中心 */
  width: 80%;                        /* 宽度占 Banner 80%，可调 */
  max-width: 700px;                  /* 最大宽度限制 */
  display: flex;
  justify-content: center;
  z-index: 10;                       /* 确保在背景图之上 */
}
</style>

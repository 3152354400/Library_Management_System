<template>
  <section class="book-recommend">
    <h2 class="section-title">推荐图书</h2>
    <div class="book-list">
      <div v-for="(book, index) in books" :key="index" class="book-card" @click="goToComments(book.ISBN)" style="cursor: pointer;">
        <img :src="`/covers/${book.ISBN}.jpg`" class="book-cover" alt="book cover" @error="e => (e.target.src = defaultCover)"/>
        <div class="book-info">
          <h3 class="book-title">{{ book.Title }}</h3>
          <p class="book-author">{{ book.Author }}</p>
          <p class="book-count">被书单引用: {{ book.BooklistCount }}</p>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { getRecommendations } from '@/modules/reader/api.js'
import { onMounted, ref } from 'vue'
import router from '@/router/index.js'

const defaultCover = new URL('@/assets/book_cover_default.jpg', import.meta.url).href
const books = ref([])

onMounted(async () => {
  const res = await getRecommendations()
  books.value = res.data || []
})

// 点击跳转评论页
function goToComments(isbn) {
  router.push({ path: '/comments', query: { isbn } })
}
</script>

<style scoped>
.book-recommend {
  padding: 2rem 1rem;
  background-color: #ffffff;
}

.section-title {
  text-align: center;
  font-size: 1.5rem;
  font-weight: bold;
  color: #004b8d;
  margin-bottom: 1.5rem;
}

.book-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 1.5rem;
  max-width: 1000px;
  margin: auto;
}

.book-card {
  background-color: #f5faff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
  transition: transform 0.2s;
}

.book-card:hover {
  transform: translateY(-4px);
}

.book-cover {
  width: 100%;
  height: 200px;
  object-fit: cover;
}

.book-info {
  padding: 0.75rem;
}

.book-title {
  font-size: 1rem;
  font-weight: 600;
  color: #004b8d;
}

.book-author {
  font-size: 0.9rem;
  color: #666;
}

.book-count {
  font-size: 0.85rem;
  color: #999;
  margin-top: 0.25rem;
}
</style>

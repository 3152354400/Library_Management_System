<template>
  <section class="quick-entry">
    <div class="entry-grid">
      <div
        v-for="(item, index) in entries"
        :key="index"
        class="entry-item"
        @click="goTo(item.route)"
      >
        <img :src="item.icon" class="entry-icon" alt="icon" />
        <div class="entry-label">{{ item.label }}</div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()

import searchIcon  from '@/assets/icons/search.svg'
import bookIcon    from '@/assets/icons/book.svg'
import renewIcon   from '@/assets/icons/renew.svg'


const entries = [
  { label: '图书检索', icon: searchIcon,  route: '/books' },
  { label: '我的借阅', icon: bookIcon,    route: '/my/borrowingRecords' },
  { label: '借还服务', icon: renewIcon,   route: '/books/circulation' },
]

function goTo(route) {
  router.push(route)
}
</script>

<style scoped>
.quick-entry {
  padding: 2rem 1rem;
  background-color: #ffffff;
}

.entry-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1.5rem;
  max-width: 1000px;
  margin: auto;
}

.entry-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background-color: #f1f6fc;
  border-radius: 12px;
  padding: 1rem;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.entry-item:hover {
  background-color: #e3efff;
}

.entry-icon {
  width: 40px;
  height: 40px;
  margin-bottom: 0.5rem;
}

.entry-label {
  font-size: 1rem;
  color: #004b8d;
  font-weight: 500;
}
</style>

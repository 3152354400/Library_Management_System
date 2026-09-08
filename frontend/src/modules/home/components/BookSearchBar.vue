<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const keyword = ref('')
const router = useRouter()

function submitSearch() {
  const trimmed = keyword.value.trim()
  if (trimmed) {
    router.push({ path: '/search', query: { q: trimmed } })
  }
}
</script>

<template>
  <div class="search-bar">
    <input
      v-model="keyword"
      type="text"
      placeholder="请输入书名或作者"
      @keyup.enter="submitSearch"
    />
    <button @click="submitSearch">
      <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 24 24">
        <path
          fill="purple"
          d="M9.5 3A6.5 6.5 0 0 1 16 9.5a6.5 6.5 0 0 1-1.25 3.75l5.5 5.5-1.5 1.5-5.5-5.5A6.5 6.5 0 1 1 9.5 3zm0 2a4.5 4.5 0 1 0 0 9a4.5 4.5 0 0 0 0-9z"
        />
      </svg>
    </button>
  </div>
</template>

<style scoped>
.search-bar {
  display: flex;
  align-items: center;
  width: 80%;
  max-width: 600px;
  margin: 0 auto;
  padding: 0.5rem 1rem;
  background-color: white;
  border-radius: 9999px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
}

.search-bar input {
  flex: 1;
  border: none;
  outline: none;
  font-size: 1rem;
  padding: 0.5rem 1rem;
  border-radius: 9999px;
  font-weight: 600;
  color: #333;
}

.search-bar input::placeholder {
  color: #aaa;
  font-weight: 600;
}

.search-bar button {
  background: transparent;
  border: none;
  padding: 0.5rem;
  cursor: pointer;
}

.search-bar .icon {
  width: 1.5rem;
  height: 1.5rem;
}
</style>
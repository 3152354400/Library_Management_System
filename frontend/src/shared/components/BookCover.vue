<template>
  <div class="book-cover">
    <svg viewBox="0 0 80 110" width="100%" height="100%" preserveAspectRatio="xMidYMid slice" aria-hidden="true">
      <defs>
        <linearGradient :id="`grad-${uid}`" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" :stop-color="color1"/>
          <stop offset="1" :stop-color="color2"/>
        </linearGradient>
      </defs>
      <rect width="80" height="110" :fill="`url(#grad-${uid})`"/>
      <rect x="6" y="8" width="68" height="2" fill="rgba(255,255,255,.4)"/>
      <rect x="6" y="100" width="68" height="2" fill="rgba(0,0,0,.15)"/>
      <text x="40" y="52" text-anchor="middle" :font-size="fontSize" font-weight="600" fill="rgba(255,255,255,.92)" font-family="serif">{{ displayText }}</text>
      <text x="40" y="66" text-anchor="middle" font-size="6" fill="rgba(255,255,255,.75)" font-family="serif">图书馆</text>
    </svg>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  title: { type: String, default: '' },
  author: { type: String, default: '' },
  isbn: { type: String, default: '' }
})

const uid = Math.random().toString(36).slice(2, 8)

// 用 ISBN 派生稳定颜色
const palettes = [
  ['#3B5BDB', '#748FFC'],
  ['#15AABF', '#4DABF7'],
  ['#7950F2', '#B197FC'],
  ['#F59F00', '#FFD43B'],
  ['#37B24D', '#8CE99A'],
  ['#E03131', '#FF8787'],
  ['#0C8599', '#66D9E8'],
  ['#5C9407', '#94D82D']
]

const color1 = computed(() => palettes[(props.isbn?.charCodeAt(2) || 0) % palettes.length][0])
const color2 = computed(() => palettes[(props.isbn?.charCodeAt(4) || 1) % palettes.length][1])

// 封面显示书名本身（去空白），按长度自适应字号
const displayText = computed(() => {
  const t = (props.title || '').replace(/\s+/g, '') || '未命名'
  return t.length > 14 ? t.slice(0, 13) + '…' : t
})

const fontSize = computed(() => {
  const n = displayText.value.length
  if (n <= 4) return 10
  if (n <= 8) return 7
  if (n <= 12) return 5.5
  return 4.5
})
</script>

<style scoped>
.book-cover {
  display: block;
  width: 100%;
  aspect-ratio: 8 / 11;
  border-radius: var(--radius-md);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
  transition: transform .3s;
  background: #eee;
}
.book-cover:hover { transform: translateY(-3px); box-shadow: var(--shadow-md); }
</style>

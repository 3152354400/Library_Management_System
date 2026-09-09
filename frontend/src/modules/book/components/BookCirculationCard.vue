<template>
  <div class="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm">
    <div class="mb-3">
      <div class="text-base font-semibold leading-tight">馆藏信息</div>
      <div class="text-sm text-gray-500">仅按条码进行借出 / 还回</div>
    </div>

    <div v-if="book" class="grid grid-cols-2 gap-y-2 gap-x-6 text-sm">
      <div><span class="text-gray-500">BookID：</span><span class="font-medium">{{ book.BookID }}</span></div>
      <div><span class="text-gray-500">条码：</span><span class="font-medium">{{ book.Barcode }}</span></div>
      <div><span class="text-gray-500">ISBN：</span><span class="font-medium">{{ book.ISBN }}</span></div>
      <div><span class="text-gray-500">书架：</span><span class="font-medium">{{ book.ShelfID ?? '—' }}</span></div>
      <div class="col-span-2 flex items-center gap-2">
        <span class="text-gray-500">状态：</span>
        <span :class="['px-2 py-0.5 rounded-full text-xs font-semibold', badgeClass(book.Status)]">
          {{ book.Status }}
        </span>
      </div>
    </div>

    <div v-else class="text-sm text-gray-500">暂无数据，请先查询条码。</div>

    <div v-if="book" class="mt-4 flex flex-wrap gap-2">
      <!-- 正常 -> 借出 / 下架 -->
      <button
        class="px-3 py-2 rounded-lg text-white bg-blue-600 disabled:opacity-60"
        :disabled="loading || book.Status !== normal"
        @click="$emit('borrow')"
      >借出</button>

      <!-- 借出 -> 还回 -->
      <button
        class="px-3 py-2 rounded-lg bg-gray-100 border border-gray-300 disabled:opacity-60"
        :disabled="loading || book.Status !== borrowed"
        @click="$emit('return')"
      >还回</button>

    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  book: { type: Object, default: null }, // { BookID, Barcode, Status, ShelfID, ISBN }
  loading: { type: Boolean, default: false }
})
defineEmits(['borrow', 'return', 'offShelf', 'onShelf'])

const normal   = '正常'
const borrowed = '借出'
const off      = '下架'

function badgeClass(status) {
  if (status === normal)   return 'bg-blue-50 text-blue-700'
  if (status === borrowed) return 'bg-orange-50 text-orange-700'
  if (status === off)      return 'bg-emerald-50 text-emerald-700'
  return 'bg-gray-100 text-gray-600'
}
</script>

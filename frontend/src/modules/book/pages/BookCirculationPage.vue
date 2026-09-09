<template>
  <div class="max-w-3xl mx-auto px-4 py-6 mt-[60px]">
    <div class="mb-4">
      <div class="text-xl font-semibold leading-tight">借还服务（按条码）</div>
      <p class="text-sm text-gray-500">输入条码 → 查询 → 执行对应操作</p>
    </div>

    <div class="bg-white rounded-2xl border border-gray-200 p-4 shadow-sm mb-4">
      <label class="block text-sm text-gray-600 mb-1">图书条码</label>
      <div class="flex gap-2">
        <input
          v-model="barcode"
          class="flex-1 px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500"
          type="text"
          placeholder="例如：BC2025090300021"
          @keyup.enter="query"
          autofocus
        />
        <button
          type="button"
          class="px-2 py-1 rounded bg-gray-100 text-gray-600 border border-gray-300 hover:bg-gray-200"
          @click="copyExample"
          title="点击复制示例条码"
        >
          BC2025082400001
        </button>
        <button
          class="px-3 py-2 rounded-lg bg-blue-600 text-white disabled:opacity-60"
          :disabled="!barcode || loading"
          @click="query"
        >查询</button>
        <button
          class="px-3 py-2 rounded-lg border border-gray-300 bg-white"
          :disabled="loading && !book"
          @click="clearForm"
        >清空</button>
      </div>

      <p v-if="error" class="mt-2 text-sm text-red-600">{{ error }}</p>
      <p v-if="tip" class="mt-2 text-sm text-emerald-600">{{ tip }}</p>
    </div>

    <BookCirculationCard
      :book="book"
      :loading="loading"
      @borrow="doAction('borrow')"
      @return="doAction('return')"
      @offShelf="doAction('off-shelf')"
      @onShelf="doAction('on-shelf')"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import BookCirculationCard from '@/modules/book/components/BookCirculationCard.vue'
import {
  getBookByBarcode,
  borrowBookByBarcode,
  returnBookByBarcode,
  offShelfBookByBarcode,
  onShelfBookByBarcode
} from '@/modules/book/api.js'

const barcode = ref('')
const book    = ref(null)
const loading = ref(false)
const error   = ref('')
const tip     = ref('')

async function query() {
  error.value = ''
  tip.value = ''
  book.value = null
  if (!barcode.value) return

  loading.value = true
  try {
    const { data } = await getBookByBarcode(barcode.value.trim())
    book.value = data
  } catch (e) {
    error.value = e?.response?.status === 404
      ? '未找到该条码对应的图书'
      : (e?.response?.data?.message || e?.message || '查询失败')
  } finally {
    loading.value = false
  }
}

async function doAction(act) {
  if (!barcode.value) return
  error.value = ''
  tip.value = ''
  loading.value = true

  try {
    const code = barcode.value.trim()
    if (act === 'borrow')      await borrowBookByBarcode(code)
    else if (act === 'return') await returnBookByBarcode(code)
    else if (act === 'off-shelf') await offShelfBookByBarcode(code)
    else if (act === 'on-shelf')  await onShelfBookByBarcode(code)

    tip.value = '操作成功'
    await query() // 回查最新状态
  } catch (e) {
    error.value = e?.response?.data?.message || e?.message || '操作失败'
  } finally {
    loading.value = false
  }
}

function clearForm() {
  barcode.value = ''
  book.value = null
  error.value = ''
  tip.value = ''
}

function copyExample() {
  const example = 'BC2025090300021'
  navigator.clipboard.writeText(example)
  barcode.value = example
}
</script>

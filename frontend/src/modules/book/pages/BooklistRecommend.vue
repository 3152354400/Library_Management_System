<template>
    <Layout>
    <div class="p-4 mt-[60px]">
      <button class="text-blue-500 mb-4" @click="$router.push({ name: 'Booklist' })">
        ⬅ 返回
      </button>
      <h2 class="text-xl font-bold mb-4">猜你喜欢</h2>

      <div
        v-for="b in store.recommended"
        :key="b.BooklistId"
        class="flex justify-between items-center border p-3 mb-2 rounded"
      >
        <div>
          <h3 class="font-semibold">{{ b.BooklistName }}</h3>
          <p class="text-gray-500">{{ b.BooklistIntroduction }}</p>
          <p class="text-sm text-gray-400">创建者: {{ b.CreatorName }}</p>
        </div>
        <button
          class="px-3 py-1 bg-blue-500 text-white rounded"
          @click="collect(b.BooklistId)"
        >
          收藏
        </button>
      </div>
    </div>
    </Layout>
</template>

<script setup>
import { onMounted } from 'vue'
import { useBooklistStore } from '@/stores/bookliststore'
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';

const store = useBooklistStore()

// 默认随机推荐一个书单ID
onMounted(async () => {
  try {
    if (store.created.length === 0) {
      await store.fetchBooklistsByReader(1, { withToken: true }) // 使用实际用户ID
    }

    let randomId = 1
    if (store.created.length > 0) {
      const randomIndex = Math.floor(Math.random() * store.created.length)
      randomId = store.created[randomIndex].BooklistId
    }

    await store.fetchRecommended(randomId, 10, { withToken: true })
  } catch (error) {
    console.error('获取推荐书单失败:', error)
    store.recommended = []
  }
})

// ✅ 收藏书单（带备注输入）
async function collect(booklistId) {
  const notes = prompt('请输入收藏备注（可选）') || ''
  try {
    await store.collect(booklistId, { Notes: notes }, { withToken: true })
    alert('收藏成功！')
  } catch (err) {
    console.error(err)
    alert('收藏失败')
  }
}
</script>

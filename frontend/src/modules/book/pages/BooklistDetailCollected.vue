<template>
  <Layout>
    <div class="p-4 mt-[60px]">
      <button class="text-blue-500 mb-4" @click="$router.push({ name: 'Booklist' })">
        ⬅ 返回
      </button>

      <BooklistHeader
        :name="store.currentBooklist?.BooklistInfo.BooklistName"
        :intro="store.currentBooklist?.BooklistInfo.BooklistIntroduction"
        :notes="store.currentBooklist?.Notes"
        show-notes
        @edit-notes="editNotes"
      />

      <BookItem
        v-for="b in store.currentBooklist?.Books"
        :key="b.ISBN"
        :book="b"
      />

      <div class="mt-4">
        <button class="px-3 py-1 bg-red-500 text-white rounded" @click="cancelCollect">
          取消收藏
        </button>
      </div>
    </div>
  </Layout>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useBooklistStore } from '@/stores/bookliststore'
import BooklistHeader from '../components/BooklistHeader.vue'
import BookItem from '../components/BookItem.vue'
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';

const store = useBooklistStore()
const route = useRoute()
const router = useRouter()
const booklistId = route.params.id

onMounted(() => {
  store.fetchBooklistDetails(booklistId, {withToken:true})
})

// 修改收藏备注
async function editNotes() {
  const newNotes = prompt('请输入新的收藏备注')
  if (newNotes) {
    await store.updateCollectNotes(booklistId, { NewNotes: newNotes }, {withToken:true})
    store.fetchBooklistDetails(booklistId, {withToken:true})
  }
}

// 取消收藏
async function cancelCollect() {
  if (!confirm('确定要取消收藏吗？')) return
  await store.cancelCollect(booklistId)
  router.push({ name: 'Booklist' })
}
</script>

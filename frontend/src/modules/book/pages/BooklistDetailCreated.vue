<template>
  <Layout>
    <div class="p-4 mt-[60px]">
      <button class="text-blue-500 mb-4" @click="$router.push({ name: 'Booklist' })">
        ⬅ 返回
      </button>

      <!-- ✅ 正确从 BooklistInfo 取字段 -->
      <BooklistHeader
        :name="store.currentBooklist?.BooklistInfo.BooklistName"
        :intro="store.currentBooklist?.BooklistInfo.BooklistIntroduction"
        editable-name
        editable-intro
        @edit-name="editName"
        @edit-intro="editIntro"
      />

      <BookItem
        v-for="b in store.currentBooklist?.Books"
        :key="b.ISBN"
        :book="b"
        show-remove
        @remove="removeBook"
      />

      <!-- ✅ ISBN 输入框点击后显示 -->
      <div class="mt-4">
        <div v-if="showInput">
          <input v-model="isbn" placeholder="输入 ISBN" class="border px-2 py-1 mr-2"/>
          <button class="px-3 py-1 bg-green-500 text-white rounded" @click="addBook">确定</button>
          <button class="px-3 py-1 bg-gray-300 rounded ml-2" @click="cancelAdd">取消</button>
        </div>
        <div v-else>
          <button class="px-3 py-1 bg-green-500 text-white rounded" @click="showInput = true">
            ➕ 添加图书
          </button>
        </div>
      </div>
    </div>
  </Layout>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useBooklistStore } from '@/stores/bookliststore'
import BooklistHeader from '../components/BooklistHeader.vue'
import BookItem from '../components/BookItem.vue'
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';

const store = useBooklistStore()
const route = useRoute()
const booklistId = route.params.id
const isbn = ref('')
const showInput = ref(false)

// 获取书单详情
onMounted(() => {
  store.fetchBooklistDetails(booklistId, { withToken: true })
})

// 添加图书
async function addBook() {
  if (!isbn.value) return
  await store.addBook(booklistId, { ISBN: isbn.value, Notes: '' }, { withToken: true })
  isbn.value = ''
  showInput.value = false // ✅ 添加完成后隐藏
  store.fetchBooklistDetails(booklistId, { withToken: true })
}

// 取消添加
function cancelAdd() {
  isbn.value = ''
  showInput.value = false
}

// 移除图书（带确认）
async function removeBook(isbnValue) {
  const confirmed = confirm('确定从书单中移除此图书吗？')
  if (!confirmed) return
  await store.removeBook(booklistId, isbnValue)
  store.fetchBooklistDetails(booklistId, { withToken: true })
}

// 修改书单名称
async function editName() {
  const newName = prompt('请输入新书单名称')
  if (newName) {
    await store.updateName(booklistId, { NewName: newName }, { withToken: true })
    store.fetchBooklistDetails(booklistId, { withToken: true })
  }
}

// 修改书单简介
async function editIntro() {
  const newIntro = prompt('请输入新简介')
  if (newIntro) {
    await store.updateIntro(booklistId, { NewIntro: newIntro }, { withToken: true })
    store.fetchBooklistDetails(booklistId, { withToken: true })
  }
}
</script>

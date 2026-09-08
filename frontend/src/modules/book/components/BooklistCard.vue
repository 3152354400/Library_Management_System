<template>
  <div class="flex justify-between items-center border p-3 rounded-lg shadow-sm hover:bg-gray-50">
    <div>
      <h3 class="font-semibold text-lg">{{ booklist.BooklistName }}</h3>
      <p class="text-gray-500 text-sm">{{ booklist.BooklistIntroduction || booklist.CollectNotes }}</p>
    </div>
    <div>
      <button
        v-if="booklist._source === 'created'"
        class="text-red-500 hover:underline"
        @click.stop="confirmDelete"
      >
        删除
      </button>
      <button
        v-else
        class="text-red-500 hover:underline"
        @click.stop="confirmCancelCollect"
      >
        取消收藏
      </button>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  booklist: { type: Object, required: true }
})
const emit = defineEmits(['delete', 'cancel-collect'])

// ✅ 删除前确认
function confirmDelete() {
  if (confirm('确定要删除该书单吗？')) {
    emit('delete', props.booklist.BooklistId)
  }
}

// ✅ 取消收藏前确认
function confirmCancelCollect() {
  if (confirm('确定要取消收藏该书单吗？')) {
    emit('cancel-collect', props.booklist.BooklistId)
  }
}
</script>

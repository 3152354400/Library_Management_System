<template>
  <div class="flex justify-between items-center border-b py-2">
    <div>
      <p class="font-medium">{{ book.Title }} (ISBN: {{ book.ISBN }})</p>
      <p class="text-gray-500 text-sm">作者: {{ book.Author }} | 添加时间: {{ book.AddTime }}</p>
    </div>
    <div v-if="showRemove">
      <button class="text-red-500 hover:underline" @click="$emit('remove', book.ISBN)">删除</button>
    </div>
  </div>
</template>

<script setup>
defineProps({
  book: { type: Object, required: true },
  showRemove: { type: Boolean, default: false }
})
defineEmits(['remove'])
</script>

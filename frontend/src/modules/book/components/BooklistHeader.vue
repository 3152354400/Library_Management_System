<template>
  <div class="mb-4">
    <div class="flex items-center">
      <h2 class="text-xl font-bold">{{ name }}</h2>
      <button v-if="editableName" class="ml-2 text-blue-500" @click="$emit('edit-name')">✏️</button>
    </div>
    <div class="flex items-center mt-2">
      <p class="text-gray-600">{{ intro }}</p>
      <button v-if="editableIntro" class="ml-2 text-blue-500" @click="$emit('edit-intro')">✏️</button>
    </div>
    <div v-if="showNotes" class="flex items-center mt-2">
      <p class="text-gray-600">收藏备注: {{ notes }}</p>
      <button class="ml-2 text-blue-500" @click="$emit('edit-notes')">✏️</button>
    </div>
  </div>
</template>

<script setup>
defineProps({
  name: String,
  intro: String,
  notes: String,
  editableName: Boolean,
  editableIntro: Boolean,
  showNotes: Boolean
})
defineEmits(['edit-name', 'edit-intro', 'edit-notes'])
</script>

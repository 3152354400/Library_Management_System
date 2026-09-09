<!-- frontend/src/modules/book/pages/BookCommentsPage.vue -->
<template>

    <div class="book-comments-page mt-[60px]" >
      <BookComments />
    </div>

</template>

<script setup>
import LayoutDefault from '@/shared/components/layouts/LayoutDefault.vue'
import BookComments from '@/modules/book/components/BookComments.vue'
</script>

<style scoped>
.book-comments-page {
  min-height: calc(100vh - 120px);
  background-color: #fff;
}
</style>
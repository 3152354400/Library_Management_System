<!-- frontend/src/modules/book/pages/ReportPage.vue -->
<template>

    <div class="report-page">
      <Report />
    </div>

</template>
<script setup>
import Report from '../components/Report.vue'
import LayoutDefault from '@/shared/components/layouts/LayoutDefault.vue'
</script>

<style scoped>
.report-page {
  padding: 20px;
}
</style>
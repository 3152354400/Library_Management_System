<template>
  <div class="bg-white/80 backdrop-blur-md p-5 rounded-lg shadow-md flex flex-col min-h-[300px]">
    <h3 class="font-bold text-lg mb-4 flex items-center flex-none" :class="iconColor">
      <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" viewBox="0 0 20 20" fill="currentColor">
        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-8.707l-3-3a1 1 0 00-1.414 0l-3 3a1 1 0 001.414 1.414L9 9.414V13a1 1 0 102 0V9.414l1.293 1.293a1 1 0 001.414-1.414z" clip-rule="evenodd" />
      </svg>
      {{ title }}
    </h3>
    <div class="flex-grow relative">
      <ol v-if="items && items.length > 0" class="list-decimal list-inside space-y-2 text-sm text-gray-700">
        <li v-for="item in items" :key="item.ISBN" class="truncate">
          <span class="font-semibold">{{ item.Title }}</span>
          <span class="text-xs ml-2 text-gray-500">({{ item.MetricValue }} {{ metricLabel }})</span>
        </li>
      </ol>
      <div v-else class="absolute inset-0 flex items-center justify-center text-center text-gray-400">
        <div>
          <svg xmlns="http://www.w3.org/2000/svg" class="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V7a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <p class="mt-2 text-sm">暂无统计数据</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: String,
  items: Array,
  metricLabel: String,
  iconColor: String,
});
</script>
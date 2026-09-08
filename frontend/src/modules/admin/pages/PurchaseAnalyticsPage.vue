<template>
  <div class="flex flex-col h-full gap-8">
    <!-- 上半部分：排名数据展示区 -->
    <div class="flex-none">
      <h2 class="text-2xl font-bold mb-6 text-gray-700">图书数据洞察</h2>
      <div v-if="loading" class="text-center text-gray-500 py-10">
        <p>正在加载排名数据...</p>
      </div>
      <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <RankingCard 
          title="图书种类借阅 Top 10" 
          :items="analysisData.TopByBorrowCount" 
          metricLabel="次"
          iconColor="text-sky-500"
        ></RankingCard>
        <RankingCard 
          title="借阅总时长 Top 10" 
          :items="analysisData.TopByBorrowDuration" 
          metricLabel="天"
          iconColor="text-amber-500"
        ></RankingCard>
        <RankingCard 
          title="单本图书借阅 Top 10" 
          :items="analysisData.TopByInstanceBorrow" 
          metricLabel="次"
          iconColor="text-violet-500"
        ></RankingCard>
      </div>
    </div>

    <!-- 下半部分：采购日志记录区 -->
    <div class="flex-grow grid grid-cols-1 lg:grid-cols-2 gap-8">
      <div class="bg-white/80 backdrop-blur-md p-6 rounded-lg shadow-md flex flex-col">
        <h3 class="font-bold text-lg mb-4 text-gray-700 flex-none">采购决策记录</h3>
        <textarea v-model="newLogText" class="w-full p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition flex-grow" placeholder="例如：采购《三体》20本，ISBN: 9787536692930..."></textarea>
        <button @click="submitLog" :disabled="isSubmitting" class="mt-4 w-full bg-blue-600 text-white font-bold py-2 px-4 rounded-md hover:bg-blue-700 transition disabled:bg-gray-400 flex-none">{{ isSubmitting ? '提交中...' : '提交记录' }}</button>
      </div>
      <div class="bg-white/80 backdrop-blur-md p-6 rounded-lg shadow-md flex flex-col">
        <h3 class="font-bold text-lg mb-4 text-gray-700 flex-none">历史记录</h3>
        <div class="flex-grow overflow-y-auto space-y-4 pr-2 border-t pt-4">
          <div v-if="logs.length === 0" class="flex items-center justify-center h-full text-center text-gray-500"><p>暂无记录</p></div>
          <div v-for="log in logs" :key="log.LogID" class="text-sm p-3 bg-gray-50/80 rounded-md border">
            <p class="text-gray-800 whitespace-pre-wrap">{{ log.LogText }}</p>
            <p class="text-right text-xs text-gray-400 mt-2">{{ new Date(log.LogDate).toLocaleString() }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getPurchaseAnalysis, getPurchaseLogs, addPurchaseLog } from '../api.js';
// 1. 引入新的 RankingCard 组件
import RankingCard from '../components/RankingCard.vue';

// 2. 删除旧的内联组件定义
// const RankingCard = { ... }; // <--- 这一整块都被删除了

const loading = ref(true);
const analysisData = ref({});
const logs = ref([]);
const newLogText = ref('');
const isSubmitting = ref(false);

async function fetchData() {
  try {
    loading.value = true;
    const [analysisRes, logsRes] = await Promise.all([
      getPurchaseAnalysis(),
      getPurchaseLogs()
    ]);
    analysisData.value = analysisRes.data;
    logs.value = logsRes.data;
  } catch (error) {
    console.error("Failed to fetch purchase analysis data:", error);
  } finally {
    loading.value = false;
  }
}

async function submitLog() {
  if (!newLogText.value.trim()) {
    alert('记录内容不能为空！');
    return;
  }
  try {
    isSubmitting.value = true;
    await addPurchaseLog(newLogText.value);
    newLogText.value = '';
    const logsRes = await getPurchaseLogs();
    logs.value = logsRes.data;
  } catch (error) {
    console.error("Failed to submit log:", error);
    alert('提交失败，请稍后再试。');
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(fetchData);
</script>

<style scoped>
/* 样式无需改动 */
</style>
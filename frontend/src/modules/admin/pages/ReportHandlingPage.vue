<template>
  <div>
    <h2 class="text-2xl font-bold mb-6 text-gray-700">待处理的举报</h2>

    <div v-if="loading" class="text-center text-gray-500 py-10">
      正在加载举报列表...
    </div>

    <div v-else-if="reports.length === 0" class="text-center bg-white/80 backdrop-blur-md p-10 rounded-lg shadow-md">
      <svg xmlns="http://www.w3.org/2000/svg" class="mx-auto h-16 w-16 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1">
        <path stroke-linecap="round" stroke-linejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      <p class="mt-4 text-lg font-semibold text-gray-700">太棒了！</p>
      <p class="text-gray-500">当前没有待处理的举报。</p>
    </div>

    <div v-else class="space-y-6">
      <div v-for="(report, index) in reports" :key="report.ReportID" class="bg-white/80 backdrop-blur-md p-5 rounded-lg shadow-md border-l-4" :class="processingId === report.ReportID ? 'border-gray-400' : 'border-red-500'">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <!-- 左侧：被举报的评论 -->
          <div class="md:col-span-2 space-y-4">
            <div>
              <h4 class="font-semibold text-gray-800">被举报的评论 (ID: {{ report.CommentID }})</h4>
              <p class="mt-1 p-3 bg-gray-50 rounded text-gray-700 italic border">"{{ report.ReviewContent }}"</p>
              <div class="text-xs text-gray-500 mt-2">
                <span>评论人: {{ report.CommenterNickname }}</span> 
                <span class="font-bold ml-2" :class="report.CommenterAccountStatus === '正常' ? 'text-green-600' : 'text-red-600'">
                  (状态: {{ report.CommenterAccountStatus }})
                </span>
              </div>
            </div>
             <div>
              <h4 class="font-semibold text-gray-800">所属图书</h4>
              <p class="text-sm text-gray-600">{{ report.BookTitle }} ({{ report.ISBN }})</p>
            </div>
          </div>

          <!-- 右侧：举报信息和操作 -->
          <div class="space-y-4 flex flex-col justify-between">
            <div>
              <h4 class="font-semibold text-gray-800">举报理由</h4>
              <p class="text-sm text-red-700">{{ report.ReportReason }}</p>
              <div class="text-xs text-gray-500 mt-2">
                <span>举报人: {{ report.ReporterNickname }}</span> | 
                <span>举报于: {{ new Date(report.ReportTime).toLocaleString() }}</span>
              </div>
            </div>
            <div class="pt-4 border-t">
              <div class="flex space-x-3">
                <button @click="processReport(report.ReportID, 'reject', index)" :disabled="!!processingId" class="btn-secondary flex-1">
                  驳回举报
                </button>
                <button @click="processReport(report.ReportID, 'approve', index)" :disabled="!!processingId" class="btn-danger flex-1">
                  同意并删除
                </button>
              </div>
              <div class="mt-3 text-center" v-if="report.CommenterAccountStatus === '正常'">
                <label class="text-sm flex items-center justify-center">
                  <input type="checkbox" v-model="banFlags[report.ReportID]" class="mr-2 h-4 w-4 rounded border-gray-300 text-red-600 focus:ring-red-500">
                  同时禁言该用户
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue';
import { getPendingReports, handleReport } from '@/modules/admin/api.js';

const loading = ref(true);
const reports = ref([]);
const processingId = ref(null);
const banFlags = reactive({});

async function fetchData() {
  try {
    loading.value = true;
    const res = await getPendingReports();
    reports.value = res.data;
    reports.value.forEach(r => {
      banFlags[r.ReportID] = false;
    });
  } catch (error) {
    console.error("Failed to fetch reports:", error);
    alert("加载举报列表失败！");
  } finally {
    loading.value = false;
  }
}

async function processReport(reportId, action, index) {
  const banUser = banFlags[reportId] || false;
  
  let confirmMessage = action === 'approve' 
    ? `您确定要删除这条评论吗？`
    : "您确定要驳回这条举报吗？";

  if (action === 'approve' && banUser) {
    confirmMessage += "\n\n警告：您已选择同时禁言该用户！";
  }

  if (!confirm(confirmMessage)) return;

  try {
    processingId.value = reportId;
    await handleReport(reportId, action, banUser);
    reports.value.splice(index, 1);
  } catch (error) {
    console.error(`Failed to ${action} report:`, error);
    alert(error.response?.data?.message || "操作失败，请稍后再试。");
  } finally {
    processingId.value = null;
  }
}

onMounted(fetchData);
</script>

<style scoped>
.btn-danger { @apply bg-red-600 text-white font-bold py-2 px-4 rounded-md hover:bg-red-700 transition disabled:bg-gray-400; }
.btn-secondary { @apply bg-gray-200 text-gray-700 font-bold py-2 px-4 rounded-md hover:bg-gray-300 transition disabled:bg-gray-400; }
</style>
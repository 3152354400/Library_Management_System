<template>
  <Layout>
    <h1 class="title">📚 我的借阅记录</h1>

    <div class="actions">
      <button class="btn-fines" @click="goFines">
        💰 我的罚款
      </button>
      <button class="btn-export" @click="exportExcel">
        📊 导出Excel
      </button>
      <button class="btn-refresh" @click="loadRecords">
        🔄 刷新记录
      </button>
    </div>

    <div class="card">
      <BorrowingRecordTable
        :records="records"
        :pageSize="7"
        :renewingId="renewingId"
        @renew="handleRenew"
      />
    </div>

    <!-- 续借弹窗 -->
    <RenewBookModal
      v-if="renewTarget"
      :record="renewTarget"
      @close="renewTarget = null"
      @renew-success="onRenewSuccess"
    />
  </Layout>

  <!-- 顶部提示 -->
  <teleport to="body">
    <div v-if="showToast" class="toast">{{ toastMsg }}</div>
  </teleport>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import BorrowingRecordTable from '@/modules/reader/components/BorrowingRecordTable.vue';
import RenewBookModal from '@/modules/reader/components/RenewBookModal.vue';
import { getBorrowingRecordPaged } from '@/modules/reader/api.js';
import { exportToExcel } from '@/utils/export.js';
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';

const router = useRouter();
const records = ref([]);
const totalCount = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);
const showToast = ref(false);
const toastMsg = ref('');
const renewTarget = ref(null);
const renewingId = ref(null);

const loadRecords = async () => {
  try {
    const res = await getBorrowingRecordPaged({
      pageNum: currentPage.value,
      pageSize: pageSize.value
    });
    const result = res.data;
    records.value = result?.data || [];
    totalCount.value = result?.totalCount || 0;
    showToast('刷新成功 ✅');
  } catch (error) {
    console.error('获取借阅记录失败', error);
    alert('获取失败，请检查登录状态或接口地址');
  }
};

function showToastFn(msg) {
  toastMsg.value = msg;
  showToast.value = true;
  setTimeout(() => {
    showToast.value = false;
  }, 1000);
}

function handleRenew(record) {
  renewTarget.value = record;
}

async function onRenewSuccess() {
  await loadRecords();
}

function goFines() {
  router.push('/my/fines');
}

function exportExcel() {
  if (records.value.length === 0) {
    alert('暂无数据可导出')
    return
  }
  const headers = [
    { key: 'ISBN', label: 'ISBN' },
    { key: 'BookTitle', label: '书名' },
    { key: 'BookAuthor', label: '作者' },
    { key: 'BorrowTime', label: '借出时间' },
    { key: 'DueTime', label: '应还时间' },
    { key: 'ReturnTime', label: '归还时间' },
    { key: 'BorrowStatus', label: '状态' }
  ]
  const data = records.value.map(r => ({
    ...r,
    BorrowTime: formatDateTime(r.BorrowTime),
    DueTime: formatDateTime(r.DueTime),
    ReturnTime: formatDateTime(r.ReturnTime)
  }))
  exportToExcel(data, headers, '我的借阅记录', '借阅记录列表')
}

function formatDateTime(d) {
  if (!d) return ''
  return new Date(d).toLocaleString('zh-CN')
}

onMounted(loadRecords);
</script>

<style scoped>
.title {
  text-align: center;
  margin-bottom: 20px;
  color: #2c3e50;
  font-size: 30px;
  font-weight: bold;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-bottom: 15px;
}

.btn-refresh,
.btn-fines {
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 16px;
  transition: 0.3s;
}

.btn-refresh {
  background: #3498db;
}
.btn-refresh:hover {
  background: #2980b9;
}

.btn-fines {
  background: #f97316;
}
.btn-fines:hover {
  background: #ea580c;
}

.btn-export {
  background: #059669;
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 16px;
  transition: 0.3s;
}
.btn-export:hover {
  background: #047857;
}

.card {
  background: #fff;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0px 4px 12px rgba(0,0,0,0.08);
  transition: 0.3s;
}
.card:hover {
  box-shadow: 0px 6px 18px rgba(0,0,0,0.12);
}

/* 顶部提示框样式 */
.toast {
  position: fixed;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  background: #2ecc71;
  color: #fff;
  padding: 10px 20px;
  border-radius: 6px;
  box-shadow: 0px 4px 10px rgba(0,0,0,0.15);
  animation: fadeInOut 1s ease forwards;
  z-index: 9999;
}

@keyframes fadeInOut {
  0% { opacity: 0; transform: translate(-50%, -20px); }
  20% { opacity: 1; transform: translate(-50%, 0); }
  80% { opacity: 1; }
  100% { opacity: 0; transform: translate(-50%, -20px); }
}
</style>

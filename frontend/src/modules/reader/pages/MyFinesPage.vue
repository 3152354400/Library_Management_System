<template>
  <Layout>
    <h1 class="title">💰 我的罚款</h1>

    <!-- 汇总卡片 -->
    <div class="summary-cards" v-if="summary">
      <div class="summary-card card-total">
        <div class="summary-label">累计罚款</div>
        <div class="summary-value">{{ summary.totalFines || 0 }}笔</div>
        <div class="summary-amount">¥{{ summary.totalAmount?.toFixed(2) || '0.00' }}</div>
      </div>
      <div class="summary-card card-unpaid">
        <div class="summary-label">待缴笔数</div>
        <div class="summary-value">{{ summary.unpaidFines || 0 }}笔</div>
        <div class="summary-amount">¥{{ summary.unpaidAmount?.toFixed(2) || '0.00' }}</div>
      </div>
      <div class="summary-card card-paid">
        <div class="summary-label">已缴金额</div>
        <div class="summary-value">¥{{ ((summary.totalAmount || 0) - (summary.unpaidAmount || 0)).toFixed(2) }}</div>
        <div class="summary-amount">已缴纳总额</div>
      </div>
    </div>

    <div class="actions">
      <button class="btn-export" @click="exportExcel">📊 导出Excel</button>
      <button class="btn-refresh" @click="loadData">🔄 刷新</button>
    </div>

    <div class="card">
      <table v-if="fines.length" class="styled-table">
        <thead>
          <tr>
            <th>编号</th>
            <th>罚款类型</th>
            <th>关联图书</th>
            <th>罚款金额</th>
            <th>已缴金额</th>
            <th>待缴金额</th>
            <th>生成时间</th>
            <th>状态</th>
            <th>备注</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="fine in fines" :key="fine.fineId">
            <td>{{ fine.fineId }}</td>
            <td>{{ fine.fineType }}</td>
            <td>{{ fine.bookTitle || '-' }}</td>
            <td>¥{{ fine.amount?.toFixed(2) }}</td>
            <td>¥{{ fine.paidAmount?.toFixed(2) }}</td>
            <td :class="{ 'text-red-600': fine.unpaidAmount > 0 }">
              <strong>¥{{ fine.unpaidAmount?.toFixed(2) }}</strong>
            </td>
            <td>{{ formatDate(fine.fineDate) }}</td>
            <td>
              <span :class="getStatusClass(fine.payStatus)">
                {{ getStatusText(fine.payStatus) }}
              </span>
            </td>
            <td class="text-sm">{{ fine.remark || '-' }}</td>
          </tr>
        </tbody>
      </table>
      <p v-else class="no-data">暂无罚款记录 🎉</p>

      <!-- 分页 -->
      <div class="pagination" v-if="totalPages >= 1">
        <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">上一页</button>
        <span>第 {{ currentPage }} 页 / 共 {{ totalPages }} 页 (共 {{ totalCount }} 条)</span>
        <button @click="changePage(currentPage + 1)" :disabled="currentPage >= totalPages">下一页</button>
      </div>
    </div>

    <div class="notice">
      <p>📌 <strong>说明：</strong></p>
      <ul>
        <li>逾期罚款按 0.5元/天 计算</li>
        <li>请在生成后30天内缴清，否则将影响信用分</li>
        <li>支持部分缴纳，未缴清部分将持续显示在待缴列表中</li>
        <li>如对罚款有疑问，请联系图书馆管理员</li>
      </ul>
    </div>
  </Layout>

  <teleport to="body">
    <div v-if="showToast" class="toast">{{ toastMsg }}</div>
  </teleport>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getMyFines, getMyFineSummary, getMyFinesPaged } from '@/modules/reader/api.js';
import { exportToExcel } from '@/utils/export.js';
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';

const fines = ref([]);
const summary = ref(null);
const showToast = ref(false);
const toastMsg = ref('');
const totalCount = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);

const loadData = async () => {
  try {
    const [finesRes, summaryRes] = await Promise.all([
      getMyFinesPaged({ pageNum: currentPage.value, pageSize: pageSize.value }),
      getMyFineSummary()
    ]);
    const finesResult = finesRes.data;
    fines.value = finesResult?.data || [];
    totalCount.value = finesResult?.totalCount || 0;
    totalPages.value = finesResult?.totalPages || 1;
    summary.value = summaryRes.data || {};
  } catch (error) {
    console.error('获取罚款记录失败', error);
    alert('获取罚款记录失败，请稍后再试');
  }
};

const changePage = (page) => {
  if (page < 1 || page > totalPages.value) return;
  currentPage.value = page;
  loadData();
};

function formatDate(date) {
  if (!date) return '-';
  return new Date(date).toLocaleString('zh-CN');
}

function getStatusText(status) {
  return {
    '未缴纳': '未缴纳',
    '部分缴纳': '部分缴纳',
    '已缴纳': '已缴纳',
    '已减免': '已减免'
  }[status] || status;
}

function getStatusClass(status) {
  return {
    'badge-unpaid': status === '未缴纳',
    'badge-partial': status === '部分缴纳',
    'badge-paid': status === '已缴纳',
    'badge-waived': status === '已减免'
  }[status] || '';
}

onMounted(loadData);

function exportExcel() {
  if (fines.value.length === 0) {
    alert('暂无数据可导出')
    return
  }
  const headers = [
    { key: 'fineId', label: '罚款编号' },
    { key: 'fineType', label: '罚款类型' },
    { key: 'bookTitle', label: '关联图书' },
    { key: 'ISBN', label: 'ISBN' },
    { key: 'amount', label: '罚款金额(元)' },
    { key: 'paidAmount', label: '已缴金额(元)' },
    { key: 'unpaidAmount', label: '待缴金额(元)' },
    { key: 'fineDate', label: '生成时间' },
    { key: 'payStatus', label: '状态' },
    { key: 'remark', label: '备注' }
  ]
  const data = fines.value.map(f => ({
    ...f,
    amount: f.amount?.toFixed(2) || '0.00',
    paidAmount: f.paidAmount?.toFixed(2) || '0.00',
    unpaidAmount: f.unpaidAmount?.toFixed(2) || '0.00',
    fineDate: formatDate(f.fineDate)
  }))
  exportToExcel(data, headers, '我的罚款记录', '罚款记录列表')
}
</script>

<style scoped>
.title {
  text-align: center;
  margin-bottom: 24px;
  color: #2c3e50;
  font-size: 30px;
  font-weight: bold;
}

/* 汇总卡片 */
.summary-cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}
.summary-card {
  padding: 20px;
  border-radius: 12px;
  color: white;
  text-align: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  transition: transform 0.3s, box-shadow 0.3s;
}
.summary-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.12);
}
.summary-label {
  font-size: 14px;
  opacity: 0.95;
}
.summary-value {
  font-size: 26px;
  font-weight: bold;
  margin: 6px 0;
}
.summary-amount {
  font-size: 14px;
  opacity: 0.9;
}
.card-total {
  background: linear-gradient(135deg, #4da6ff, #2563eb);
}
.card-unpaid {
  background: linear-gradient(135deg, #fb923c, #ea580c);
}
.card-paid {
  background: linear-gradient(135deg, #34d399, #059669);
}

.actions {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 15px;
}
.btn-refresh {
  background: #3498db;
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 16px;
  transition: 0.3s;
}
.btn-refresh:hover {
  background: #2980b9;
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
  margin-right: 12px;
}
.btn-export:hover {
  background: #047857;
}

.card {
  background: #fff;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0px 4px 12px rgba(0,0,0,0.08);
}

.styled-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 15px;
}
.styled-table th {
  background-color: #4da6ff;
  color: white;
  text-align: center;
  padding: 12px 8px;
  font-size: 15px;
}
.styled-table td {
  padding: 12px 8px;
  border-bottom: 1px solid #e5e7eb;
  text-align: center;
  font-size: 14px;
}
.styled-table tbody tr:nth-child(even) {
  background-color: #f9fbfd;
}
.styled-table tbody tr:hover {
  background-color: #e6f3ff;
}

.text-red-600 {
  color: #dc2626;
}
.text-sm {
  font-size: 13px;
  color: #6b7280;
}

.badge-unpaid {
  display: inline-block;
  padding: 4px 12px;
  background-color: #fee2e2;
  color: #dc2626;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 600;
}
.badge-partial {
  display: inline-block;
  padding: 4px 12px;
  background-color: #fed7aa;
  color: #c2410c;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 600;
}
.badge-paid {
  display: inline-block;
  padding: 4px 12px;
  background-color: #d1fae5;
  color: #047857;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 600;
}
.badge-waived {
  display: inline-block;
  padding: 4px 12px;
  background-color: #e0e7ff;
  color: #4338ca;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 600;
}

.no-data {
  text-align: center;
  color: #999;
  padding: 40px;
  font-size: 18px;
}

.notice {
  margin-top: 20px;
  padding: 16px 20px;
  background: #fef3c7;
  border-left: 4px solid #f59e0b;
  border-radius: 6px;
  font-size: 14px;
  color: #78350f;
}
.notice p {
  margin: 0 0 6px;
  font-weight: 600;
}
.notice ul {
  margin: 0;
  padding-left: 20px;
}
.notice li {
  margin: 2px 0;
}

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

.pagination {
  margin-top: 20px;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 14px;
  font-size: 16px;
  padding: 16px 0 4px;
  border-top: 1px solid #f0f0f0;
}

.pagination button {
  padding: 8px 14px;
  background-color: #4da6ff;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  font-size: 16px;
}

.pagination button:hover:not(:disabled) {
  background-color: #3399ff;
}

.pagination button:disabled {
  background-color: #b3d9ff;
  cursor: not-allowed;
}
</style>

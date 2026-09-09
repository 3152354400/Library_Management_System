<template>
  <div>
    <h2 class="text-2xl font-bold mb-6 text-gray-700">💰 罚款管理</h2>

    <div class="flex gap-3 mb-4">
      <button @click="exportExcel" class="btn-export">📊 导出Excel</button>
      <button @click="fetchData" class="btn-refresh">🔄 刷新</button>
    </div>

    <!-- 汇总 -->
    <div class="grid grid-cols-3 gap-4 mb-6">
      <div class="bg-white rounded-lg shadow p-4">
        <div class="text-sm text-gray-500">待缴纳</div>
        <div class="text-2xl font-bold text-orange-600">{{ pendingCount }} 笔</div>
      </div>
      <div class="bg-white rounded-lg shadow p-4">
        <div class="text-sm text-gray-500">待缴金额</div>
        <div class="text-2xl font-bold text-red-600">¥{{ totalUnpaid?.toFixed(2) || '0.00' }}</div>
      </div>
      <div class="bg-white rounded-lg shadow p-4">
        <div class="text-sm text-gray-500">已处理</div>
        <div class="text-2xl font-bold text-green-600">{{ handledCount }} 笔</div>
      </div>
    </div>

    <div v-if="loading" class="text-center text-gray-500 py-10">正在加载...</div>
    <div v-else-if="fines.length === 0" class="text-center bg-white p-10 rounded-lg shadow">
      <p class="text-lg text-gray-500">太棒了！当前没有待处理的罚款 🎉</p>
    </div>
    <div v-else class="space-y-4">
      <div v-for="fine in fines" :key="fine.fineId" class="bg-white rounded-lg shadow p-5 border-l-4 border-orange-400">
        <div class="flex justify-between items-start">
          <div class="flex-1">
            <div class="flex items-center gap-3 mb-2">
              <h4 class="font-bold text-gray-800">{{ fine.fineType }}</h4>
              <span :class="getStatusBadge(fine.payStatus)">{{ fine.payStatus }}</span>
            </div>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-2 text-sm text-gray-600">
              <div>读者ID: <span class="text-gray-800">{{ fine.readerId }}</span></div>
              <div>图书: <span class="text-gray-800">{{ fine.bookTitle || '-' }}</span></div>
              <div>ISBN: <span class="text-gray-800">{{ fine.ISBN || '-' }}</span></div>
              <div>生成时间: <span class="text-gray-800">{{ formatDate(fine.fineDate) }}</span></div>
            </div>
            <div v-if="fine.remark" class="text-sm text-gray-500 mt-1">备注: {{ fine.remark }}</div>
          </div>
          <div class="text-right ml-4">
            <div class="text-sm text-gray-500">罚款金额</div>
            <div class="text-2xl font-bold text-red-600">¥{{ fine.amount?.toFixed(2) }}</div>
            <div class="text-sm text-gray-500">已缴: ¥{{ fine.paidAmount?.toFixed(2) }}</div>
            <div v-if="fine.unpaidAmount > 0" class="text-sm font-bold text-orange-600">待缴: ¥{{ fine.unpaidAmount?.toFixed(2) }}</div>
          </div>
        </div>

        <!-- 操作区 -->
        <div v-if="fine.payStatus !== '已缴纳' && fine.payStatus !== '已减免'" class="mt-4 pt-3 border-t flex gap-3 flex-wrap">
          <button @click="showPayDialog(fine)" class="btn-primary">💰 办理缴纳</button>
          <button @click="showWaiveDialog(fine)" class="btn-secondary">📋 减免罚款</button>
        </div>
      </div>
    </div>

    <!-- 缴纳对话框 -->
    <div v-if="payDialog.visible" class="modal-overlay" @click.self="payDialog.visible = false">
      <div class="modal-content">
        <button @click="payDialog.visible = false" class="close">&times;</button>
        <h3 class="text-xl font-bold mb-4">💰 办理罚款缴纳</h3>
        <div class="space-y-3">
          <div class="info-row"><span>罚款类型：</span><span>{{ payDialog.fine?.fineType }}</span></div>
          <div class="info-row"><span>待缴金额：</span><span class="text-red-600 font-bold">¥{{ payDialog.fine?.unpaidAmount?.toFixed(2) }}</span></div>
          <div class="form-group">
            <label>缴纳金额</label>
            <input v-model.number="payDialog.amount" type="number" min="0.01" :max="payDialog.fine?.unpaidAmount" step="0.01" class="form-input" />
          </div>
          <div class="form-group">
            <label>缴纳方式</label>
            <select v-model="payDialog.method" class="form-input">
              <option value="现金">现金</option>
              <option value="微信">微信</option>
              <option value="支付宝">支付宝</option>
              <option value="银行卡">银行卡</option>
              <option value="其他">其他</option>
            </select>
          </div>
          <div class="flex gap-3 mt-4">
            <button @click="submitPay" :disabled="paying" class="btn-primary flex-1">
              {{ paying ? '处理中...' : '确认缴纳' }}
            </button>
            <button @click="payDialog.visible = false" class="btn-secondary flex-1">取消</button>
          </div>
          <p v-if="payDialog.error" class="text-red-600 text-sm mt-2">{{ payDialog.error }}</p>
        </div>
      </div>
    </div>

    <!-- 减免对话框 -->
    <div v-if="waiveDialog.visible" class="modal-overlay" @click.self="waiveDialog.visible = false">
      <div class="modal-content">
        <button @click="waiveDialog.visible = false" class="close">&times;</button>
        <h3 class="text-xl font-bold mb-4">📋 减免罚款</h3>
        <div class="info-row mb-4"><span>待缴金额：</span><span class="text-red-600 font-bold">¥{{ waiveDialog.fine?.unpaidAmount?.toFixed(2) }}</span></div>
        <div class="form-group">
          <label>减免说明（必填）</label>
          <textarea v-model="waiveDialog.reason" rows="3" class="form-input" placeholder="请输入减免原因..." maxlength="200"></textarea>
        </div>
        <div class="flex gap-3 mt-4">
          <button @click="submitWaive" :disabled="waiving" class="btn-primary flex-1">
            {{ waiving ? '处理中...' : '确认减免' }}
          </button>
          <button @click="waiveDialog.visible = false" class="btn-secondary flex-1">取消</button>
        </div>
        <p v-if="waiveDialog.error" class="text-red-600 text-sm mt-2">{{ waiveDialog.error }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { getPendingFines, payFine, waiveFine } from '@/modules/admin/api.js';
import { exportToExcel } from '@/utils/export.js';

const fines = ref([]);
const loading = ref(true);
const paying = ref(false);
const waiving = ref(false);

const pendingCount = computed(() => fines.value.filter(f => f.payStatus !== '已缴纳' && f.payStatus !== '已减免').length);
const handledCount = computed(() => fines.value.filter(f => f.payStatus === '已缴纳' || f.payStatus === '已减免').length);
const totalUnpaid = computed(() => fines.value.reduce((sum, f) => sum + (f.unpaidAmount || 0), 0));

const payDialog = reactive({ visible: false, fine: null, amount: 0, method: '现金', error: '' });
const waiveDialog = reactive({ visible: false, fine: null, reason: '', error: '' });

async function fetchData() {
  loading.value = true;
  try {
    const res = await getPendingFines();
    fines.value = res.data || [];
  } catch (e) {
    console.error(e);
    alert('加载失败');
  } finally {
    loading.value = false;
  }
}

function formatDate(d) {
  if (!d) return '-';
  return new Date(d).toLocaleString('zh-CN');
}

function getStatusBadge(status) {
  return {
    '未缴纳': 'badge badge-red',
    '部分缴纳': 'badge badge-orange',
    '已缴纳': 'badge badge-green',
    '已减免': 'badge badge-gray'
  }[status] || 'badge badge-gray';
}

function showPayDialog(fine) {
  payDialog.fine = fine;
  payDialog.amount = fine.unpaidAmount;
  payDialog.method = '现金';
  payDialog.error = '';
  payDialog.visible = true;
}

function showWaiveDialog(fine) {
  waiveDialog.fine = fine;
  waiveDialog.reason = '';
  waiveDialog.error = '';
  waiveDialog.visible = true;
}

async function submitPay() {
  if (!payDialog.amount || payDialog.amount <= 0) {
    payDialog.error = '请输入正确的缴纳金额';
    return;
  }
  if (payDialog.amount > payDialog.fine.unpaidAmount) {
    payDialog.error = '缴纳金额不能超过待缴金额';
    return;
  }
  try {
    paying.value = true;
    await payFine({ fineId: payDialog.fine.fineId, payAmount: payDialog.amount, payMethod: payDialog.method });
    payDialog.visible = false;
    alert('缴纳成功');
    await fetchData();
  } catch (e) {
    payDialog.error = e.response?.data?.message || '操作失败';
  } finally {
    paying.value = false;
  }
}

async function submitWaive() {
  if (!waiveDialog.reason.trim()) {
    waiveDialog.error = '请输入减免原因';
    return;
  }
  try {
    waiving.value = true;
    await waiveFine({ fineId: waiveDialog.fine.fineId, reason: waiveDialog.reason });
    waiveDialog.visible = false;
    alert('减免成功');
    await fetchData();
  } catch (e) {
    waiveDialog.error = e.response?.data?.message || '操作失败';
  } finally {
    waiving.value = false;
  }
}

onMounted(fetchData);

function exportExcel() {
  if (fines.value.length === 0) {
    alert('暂无数据可导出');
    return;
  }
  const headers = [
    { key: 'fineId', label: '罚款编号' },
    { key: 'readerId', label: '读者ID' },
    { key: 'bookTitle', label: '图书标题' },
    { key: 'ISBN', label: 'ISBN' },
    { key: 'fineType', label: '罚款类型' },
    { key: 'amount', label: '罚款金额' },
    { key: 'paidAmount', label: '已缴金额' },
    { key: 'unpaidAmount', label: '待缴金额' },
    { key: 'fineDate', label: '生成时间' },
    { key: 'payStatus', label: '支付状态' },
    { key: 'remark', label: '备注' }
  ];
  const data = fines.value.map(f => ({
    ...f,
    amount: f.amount?.toFixed(2),
    paidAmount: f.paidAmount?.toFixed(2),
    unpaidAmount: f.unpaidAmount?.toFixed(2),
    fineDate: f.fineDate ? new Date(f.fineDate).toLocaleString('zh-CN') : ''
  }));
  exportToExcel(data, headers, '罚款管理', '罚款记录列表');
}
</script>

<style scoped>
.badge { padding: 2px 10px; border-radius: 999px; font-size: 12px; font-weight: 600; }
.badge-red { background: #fee2e2; color: #dc2626; }
.badge-orange { background: #fed7aa; color: #c2410c; }
.badge-green { background: #d1fae5; color: #047857; }
.badge-gray { background: #f3f4f6; color: #6b7280; }
.btn-primary { background: #2563eb; color: white; padding: 6px 16px; border-radius: 6px; font-weight: 600; border: none; cursor: pointer; transition: 0.2s; }
.btn-primary:hover:not(:disabled) { background: #1d4ed8; }
.btn-primary:disabled { background: #9ca3af; cursor: not-allowed; }
.btn-export { background: #059669; color: white; padding: 6px 16px; border-radius: 6px; font-weight: 600; border: none; cursor: pointer; transition: 0.2s; }
.btn-export:hover { background: #047857; }
.btn-refresh { background: #4da6ff; color: white; padding: 6px 16px; border-radius: 6px; font-weight: 600; border: none; cursor: pointer; transition: 0.2s; }
.btn-refresh:hover { background: #3399ff; }
.btn-secondary { background: #6b7280; color: white; padding: 6px 16px; border-radius: 6px; font-weight: 600; border: none; cursor: pointer; transition: 0.2s; }
.btn-secondary:hover { background: #4b5563; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-content { background: white; padding: 2rem; border-radius: 12px; width: 90%; max-width: 480px; position: relative; }
.close { position: absolute; top: 1rem; right: 1rem; font-size: 1.5rem; background: none; border: none; cursor: pointer; color: #9ca3af; }
.info-row { display: flex; justify-content: space-between; font-size: 14px; }
.form-group { margin-bottom: 0.75rem; }
.form-group label { display: block; font-weight: 500; margin-bottom: 4px; font-size: 14px; }
.form-input { width: 100%; padding: 8px 12px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; box-sizing: border-box; }
</style>

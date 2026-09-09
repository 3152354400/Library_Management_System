<template>
  <div>
    <h2 class="text-2xl font-bold mb-6 text-gray-700">💡 荐购处理</h2>

    <div v-if="loading" class="text-center text-gray-500 py-10">正在加载...</div>
    <div v-else-if="recommends.length === 0" class="text-center bg-white p-10 rounded-lg shadow">
      <p class="text-lg text-gray-500">太棒了！当前没有待处理的荐购 🎉</p>
    </div>
    <div v-else class="space-y-4">
      <div v-for="r in recommends" :key="r.recommendId" class="bg-white rounded-lg shadow p-5 border-l-4 border-purple-400">
        <div class="flex justify-between items-start">
          <div class="flex-1">
            <div class="flex items-center gap-3 mb-2">
              <h4 class="font-bold text-gray-800 text-lg">{{ r.title }}</h4>
              <span class="badge badge-purple">待审核</span>
            </div>
            <div class="grid grid-cols-2 md:grid-cols-3 gap-2 text-sm text-gray-600">
              <div>ISBN: <span class="text-gray-800">{{ r.ISBN || '-' }}</span></div>
              <div>作者: <span class="text-gray-800">{{ r.author || '-' }}</span></div>
              <div>出版社: <span class="text-gray-800">{{ r.publisher || '-' }}</span></div>
              <div>荐购读者: <span class="text-gray-800">{{ r.readerName || r.readerId }}</span></div>
              <div>提交时间: <span class="text-gray-800">{{ formatDate(r.recommendTime) }}</span></div>
              <div>出版年份: <span class="text-gray-800">{{ r.publishYear || '-' }}</span></div>
            </div>
            <div v-if="r.reason" class="mt-3 p-3 bg-gray-50 rounded text-sm text-gray-700">
              <strong>推荐理由：</strong>{{ r.reason }}
            </div>
          </div>
        </div>

        <div class="mt-4 pt-3 border-t flex gap-3 flex-wrap">
          <button @click="showAcceptDialog(r)" class="btn-primary">✅ 采纳</button>
          <button @click="showRejectDialog(r)" class="btn-danger">❌ 拒绝</button>
        </div>
      </div>
    </div>

    <!-- 采纳对话框 -->
    <div v-if="acceptDialog.visible" class="modal-overlay" @click.self="acceptDialog.visible = false">
      <div class="modal-content">
        <button @click="acceptDialog.visible = false" class="close">&times;</button>
        <h3 class="text-xl font-bold mb-4">✅ 采纳荐购</h3>
        <div class="form-group">
          <label>书名：</label>
          <div class="text-gray-800 font-semibold">{{ acceptDialog.recommend?.title }}</div>
        </div>
        <div class="form-group">
          <label>采购价格（元）</label>
          <input v-model.number="acceptDialog.price" type="number" min="0" step="0.01" class="form-input" placeholder="请输入采购价格" />
        </div>
        <div class="form-group">
          <label>处理说明</label>
          <textarea v-model="acceptDialog.result" rows="3" class="form-input" placeholder="请输入处理说明..." maxlength="300"></textarea>
        </div>
        <div class="flex gap-3 mt-4">
          <button @click="submitAccept" :disabled="handling" class="btn-primary flex-1">{{ handling ? '处理中...' : '确认采纳' }}</button>
          <button @click="acceptDialog.visible = false" class="btn-secondary flex-1">取消</button>
        </div>
        <p v-if="acceptDialog.error" class="text-red-600 text-sm mt-2">{{ acceptDialog.error }}</p>
      </div>
    </div>

    <!-- 拒绝对话框 -->
    <div v-if="rejectDialog.visible" class="modal-overlay" @click.self="rejectDialog.visible = false">
      <div class="modal-content">
        <button @click="rejectDialog.visible = false" class="close">&times;</button>
        <h3 class="text-xl font-bold mb-4">❌ 拒绝荐购</h3>
        <div class="form-group">
          <label>拒绝原因（必填）</label>
          <textarea v-model="rejectDialog.result" rows="4" class="form-input" placeholder="请说明拒绝原因，例如：该书不适合馆藏、已有类似图书..." maxlength="300"></textarea>
        </div>
        <div class="flex gap-3 mt-4">
          <button @click="submitReject" :disabled="handling" class="btn-danger flex-1">{{ handling ? '处理中...' : '确认拒绝' }}</button>
          <button @click="rejectDialog.visible = false" class="btn-secondary flex-1">取消</button>
        </div>
        <p v-if="rejectDialog.error" class="text-red-600 text-sm mt-2">{{ rejectDialog.error }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import { getPendingRecommends, handleRecommend } from '@/modules/admin/api.js';

const recommends = ref([]);
const loading = ref(true);
const handling = ref(false);

const acceptDialog = reactive({ visible: false, recommend: null, price: null, result: '', error: '' });
const rejectDialog = reactive({ visible: false, recommend: null, result: '', error: '' });

async function fetchData() {
  loading.value = true;
  try {
    const res = await getPendingRecommends();
    recommends.value = res.data || [];
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
}

function formatDate(d) {
  if (!d) return '-';
  return new Date(d).toLocaleString('zh-CN');
}

function showAcceptDialog(r) {
  acceptDialog.recommend = r;
  acceptDialog.price = null;
  acceptDialog.result = '';
  acceptDialog.error = '';
  acceptDialog.visible = true;
}

function showRejectDialog(r) {
  rejectDialog.recommend = r;
  rejectDialog.result = '';
  rejectDialog.error = '';
  rejectDialog.visible = true;
}

async function submitAccept() {
  try {
    handling.value = true;
    await handleRecommend({
      recommendId: acceptDialog.recommend.recommendId,
      action: '采纳',
      handleResult: acceptDialog.result,
      purchasePrice: acceptDialog.price
    });
    acceptDialog.visible = false;
    alert('采纳成功');
    await fetchData();
  } catch (e) {
    acceptDialog.error = e.response?.data?.message || '操作失败';
  } finally {
    handling.value = false;
  }
}

async function submitReject() {
  if (!rejectDialog.result.trim()) {
    rejectDialog.error = '请输入拒绝原因';
    return;
  }
  try {
    handling.value = true;
    await handleRecommend({
      recommendId: rejectDialog.recommend.recommendId,
      action: '拒绝',
      handleResult: rejectDialog.result
    });
    rejectDialog.visible = false;
    alert('已拒绝');
    await fetchData();
  } catch (e) {
    rejectDialog.error = e.response?.data?.message || '操作失败';
  } finally {
    handling.value = false;
  }
}

onMounted(fetchData);
</script>

<style scoped>
.badge { padding: 2px 10px; border-radius: 999px; font-size: 12px; font-weight: 600; }
.badge-purple { background: #ede9fe; color: #6d28d9; }
.btn-primary { background: #2563eb; color: white; padding: 6px 16px; border-radius: 6px; font-weight: 600; border: none; cursor: pointer; transition: 0.2s; }
.btn-primary:hover:not(:disabled) { background: #1d4ed8; }
.btn-primary:disabled { background: #9ca3af; cursor: not-allowed; }
.btn-danger { background: #dc2626; color: white; padding: 6px 16px; border-radius: 6px; font-weight: 600; border: none; cursor: pointer; transition: 0.2s; }
.btn-danger:hover:not(:disabled) { background: #b91c1c; }
.btn-danger:disabled { background: #9ca3af; cursor: not-allowed; }
.btn-secondary { background: #6b7280; color: white; padding: 6px 16px; border-radius: 6px; font-weight: 600; border: none; cursor: pointer; transition: 0.2s; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-content { background: white; padding: 2rem; border-radius: 12px; width: 90%; max-width: 520px; position: relative; }
.close { position: absolute; top: 1rem; right: 1rem; font-size: 1.5rem; background: none; border: none; cursor: pointer; color: #9ca3af; }
.form-group { margin-bottom: 0.75rem; }
.form-group label { display: block; font-weight: 500; margin-bottom: 4px; font-size: 14px; }
.form-input { width: 100%; padding: 8px 12px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; box-sizing: border-box; font-family: inherit; }
</style>

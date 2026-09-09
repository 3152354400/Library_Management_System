<template>
  <Layout>
    <h1 class="title">💡 我的图书荐购</h1>

    <div class="actions">
      <button class="btn-add" @click="showModal = true">
        ➕ 提交荐购
      </button>
      <button class="btn-refresh" @click="loadRecommends">🔄 刷新</button>
    </div>

    <div class="card" v-if="recommends.length">
      <table class="styled-table">
        <thead>
          <tr>
            <th>书名</th>
            <th>作者</th>
            <th>出版社</th>
            <th>ISBN</th>
            <th>推荐理由</th>
            <th>提交时间</th>
            <th>状态</th>
            <th>处理结果</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in recommends" :key="r.recommendId">
            <td class="text-left">{{ r.title }}</td>
            <td>{{ r.author || '-' }}</td>
            <td>{{ r.publisher || '-' }}</td>
            <td>{{ r.ISBN || '-' }}</td>
            <td class="text-left text-sm max-w-xs">{{ r.reason || '-' }}</td>
            <td>{{ formatDate(r.recommendTime) }}</td>
            <td>
              <span :class="getStatusClass(r.status)">{{ r.status }}</span>
            </td>
            <td class="text-sm">{{ r.handleResult || '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="empty-state" v-else>
      <p>暂无荐购记录 📚</p>
    </div>

    <div class="notice">
      <p>📌 <strong>荐购说明：</strong></p>
      <ul>
        <li>如果您想阅读的图书不在图书馆馆藏中，可以提交荐购</li>
        <li>每月最多提交 <strong>5次</strong> 荐购</li>
        <li>同一本书不能重复荐购</li>
        <li>管理员审核后会通过消息通知您结果</li>
      </ul>
    </div>

    <!-- 荐购弹窗 -->
    <RecommendPurchaseModal
      v-if="showModal"
      @close="showModal = false"
      @recommend-success="onRecommendSuccess"
    />
  </Layout>

  <teleport to="body">
    <div v-if="showToast" class="toast">{{ toastMsg }}</div>
  </teleport>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getMyRecommends } from '@/modules/reader/api.js';
import RecommendPurchaseModal from '@/modules/reader/components/RecommendPurchaseModal.vue';
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';

const recommends = ref([]);
const showModal = ref(false);
const showToast = ref(false);
const toastMsg = ref('');

const loadRecommends = async () => {
  try {
    const res = await getMyRecommends();
    recommends.value = res.data || [];
  } catch (error) {
    console.error('获取荐购记录失败', error);
  }
};

function formatDate(date) {
  if (!date) return '-';
  return new Date(date).toLocaleString('zh-CN');
}

function getStatusClass(status) {
  return {
    'badge-pending': status === '待审核',
    'badge-accepted': status === '已采纳',
    'badge-purchased': status === '已购买',
    'badge-rejected': status === '被拒绝'
  }[status] || '';
}

function showToastMsg(msg) {
  toastMsg.value = msg;
  showToast.value = true;
  setTimeout(() => { showToast.value = false; }, 2000);
}

async function onRecommendSuccess() {
  showToastMsg('荐购提交成功！');
  await loadRecommends();
}

onMounted(loadRecommends);
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
.btn-refresh {
  background: #3498db;
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 15px;
  transition: 0.3s;
}
.btn-refresh:hover { background: #2980b9; }
.btn-add {
  background: #7c3aed;
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 15px;
  transition: 0.3s;
}
.btn-add:hover { background: #6d28d9; }
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
  background-color: #7c3aed;
  color: white;
  text-align: center;
  padding: 12px 8px;
}
.styled-table td {
  padding: 12px 8px;
  border-bottom: 1px solid #e5e7eb;
  text-align: center;
}
.styled-table tbody tr:nth-child(even) {
  background-color: #faf5ff;
}
.styled-table tbody tr:hover {
  background-color: #ede9fe;
}
.text-left { text-align: left; }
.text-sm { font-size: 13px; color: #6b7280; }
.max-w-xs { max-width: 160px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.badge-pending { background: #fef3c7; color: #92400e; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.badge-accepted { background: #dbeafe; color: #1e40af; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.badge-purchased { background: #d1fae5; color: #047857; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.badge-rejected { background: #f3f4f6; color: #6b7280; padding: 3px 10px; border-radius: 999px; font-size: 13px; }
.empty-state {
  text-align: center;
  padding: 60px;
  background: #fff;
  border-radius: 10px;
  color: #999;
  font-size: 18px;
  box-shadow: 0px 4px 12px rgba(0,0,0,0.08);
}
.notice {
  margin-top: 20px;
  padding: 16px 20px;
  background: #f0f9ff;
  border-left: 4px solid #7c3aed;
  border-radius: 6px;
  font-size: 14px;
  color: #3730a3;
}
.notice p { margin: 0 0 6px; font-weight: 600; }
.notice ul { margin: 0; padding-left: 20px; }
.notice li { margin: 2px 0; }
.toast {
  position: fixed;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  background: #2ecc71;
  color: #fff;
  padding: 10px 20px;
  border-radius: 6px;
  animation: fadeInOut 2s ease forwards;
  z-index: 9999;
}
@keyframes fadeInOut {
  0% { opacity: 0; transform: translate(-50%, -20px); }
  20% { opacity: 1; transform: translate(-50%, 0); }
  80% { opacity: 1; }
  100% { opacity: 0; transform: translate(-50%, -20px); }
}
</style>

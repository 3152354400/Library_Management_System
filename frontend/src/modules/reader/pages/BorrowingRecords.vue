<template>
  <Layout>
    <h1 class="title">📚 我的借阅记录</h1>

    <div class="actions">
      <button class="btn-refresh" @click="loadRecords">🔄 刷新记录</button>
    </div>

    <div class="card">
      <BorrowingRecordTable :records="records" :pageSize="7" />
    </div>
  </Layout>

  <!-- 使用 Teleport 把弹窗渲染到 body -->
  <teleport to="body">
    <div v-if="showToast" class="toast">刷新成功 ✅</div>
  </teleport>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import BorrowingRecordTable from '@/modules/reader/components/BorrowingRecordTable.vue';
import { getAllBorrowingRecords } from '@/modules/reader/api.js';
import {getBorrowingRecordByReaderId} from '@/modules/reader/api.js';
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue';


const records = ref([]);
const showToast = ref(false);

const loadRecords = async () => {
  try {
    const res = await getBorrowingRecordByReaderId();
    records.value = res.data || [];

    // 显示提示框
    showToast.value = true;
    setTimeout(() => {
      showToast.value = false;
    }, 1000); // 1秒后自动消失
  } catch (error) {
    console.error('获取借阅记录失败', error);
    alert('获取失败，请检查登录状态或接口地址');
  }
};

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
  text-align: right;
  margin-bottom: 15px;
}

.btn-refresh {
  background: #3498db;
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 18px;
  transition: 0.3s;
}
.btn-refresh:hover {
  background: #2980b9;
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
  z-index: 9999; /* 确保在最上层 */
}

/* 渐入渐出动画 */
@keyframes fadeInOut {
  0% { opacity: 0; transform: translate(-50%, -20px); }
  20% { opacity: 1; transform: translate(-50%, 0); }
  80% { opacity: 1; }
  100% { opacity: 0; transform: translate(-50%, -20px); }
}
</style>

<template>
  <layout>
    <div class="page-body">
      <!-- 用户信息卡片 -->
      <div class="user-card">
        <!-- 左侧头像 -->
        <div class="avatar-section">
          <img :src="avatar" class="avatar" />
        </div>

        <!-- 中间用户资料 -->
        <div class="info-section">
          <div class="info-row">
            <span class="label">用户名：</span>
            <span class="value">{{ userName }}</span>
          </div>
          <div class="info-row">
            <span class="label">真实姓名：</span>
            <span class="value">{{ fullName }}</span>
          </div>
          <div class="info-row">
            <span class="label">昵称：</span>
            <span class="value">{{ nickName }}</span>
          </div>
          <div class="info-row">
            <span class="label">信誉分：</span>
            <span class="value credit-score">{{ creditScore }}</span>
          </div>
          <div class="info-row">
            <span class="label">账户状态：</span>
            <span :class="['value', statusClass]">{{ accountStatus }}</span>
          </div>
          <div class="info-row">
            <span class="label">权限：</span>
            <span class="value">{{ permission }}</span>
          </div>
        </div>

        <!-- 右侧统计数据 -->
        <div class="stats-section">
          <DashBoardInfoCard title="当前借阅" :count="unreturnedCount" class="card-blue small-card" />
          <DashBoardInfoCard title="当前逾期" :count="overdueCount" class="card-red small-card" />
          <DashBoardInfoCard title="总逾期数" :count="allOverdueCount" class="card-green small-card" />
        </div>
      </div>

      <!-- 通知列表 -->
      <DashBoardNotificationList />
    </div>
  </layout>
</template>

<script setup>
import layout from '@/modules/reader/reader_DashBoard_layout/layout.vue'
import DashBoardInfoCard from '../components/DashBoardInfoCard.vue'
import DashBoardNotificationList from '../components/DashBoardNotificationList.vue'
import { getUnreturnedCount, getOverdueUnreturnedCount, getAllOverdueUnreturnedCountByReader } from '@/modules/reader/api.js'
import { useUserStore } from '@/stores/user.js'
import { computed, ref, onMounted } from 'vue'

const unreturnedCount = ref(0)
const overdueCount = ref(0)
const allOverdueCount = ref(0)

const userStore = useUserStore()
const baseAvatarUrl = import.meta.env.VITE_BASE_AVATAR_URL
const avatar = computed(() => baseAvatarUrl + userStore.avatar)

const userName = computed(() => userStore.userName)
const fullName = computed(() => userStore.fullName)
const nickName = computed(() => userStore.nickName)
const creditScore = computed(() => userStore.creditScore)
const accountStatus = computed(() => userStore.accountStatus)
const permission = computed(() => userStore.permission)

const statusClass = computed(() => accountStatus.value === '正常' ? 'status-normal' : 'status-blocked')

onMounted(async () => {
  try {
    const res1 = await getUnreturnedCount()
    unreturnedCount.value = res1.data

    const res2 = await getOverdueUnreturnedCount()
    overdueCount.value = res2.data

    const res3 = await getAllOverdueUnreturnedCountByReader()
    allOverdueCount.value = res3.data
  } catch (error) {
    console.error("获取借阅信息失败：", error)
  }
})
</script>

<style scoped>
/* 页面整体背景 */
.page-body {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 32px;
  background: #f4f7fa;
  min-height: 100vh;
}

/* 用户信息卡片 */
.user-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: linear-gradient(135deg, #ffffff, #f0f4ff);
  border-radius: 20px;
  box-shadow: 0 8px 25px rgba(0,0,0,0.1);
  padding: 32px;
  transition: transform 0.3s, box-shadow 0.3s;
}
.user-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 30px rgba(0,0,0,0.15);
}

/* 左侧头像 */
.avatar-section {
  margin-right: 40px;
}
.avatar {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  border: 4px solid #4da6ff;
  object-fit: cover;
}

/* 中间用户资料 */
.info-section {
  display: flex;
  flex-direction: column;
  gap: 14px;
  flex: 1; /* 占据中间剩余空间 */
}
.info-row {
  display: flex;
  gap: 12px;
  font-size: 18px;
  align-items: center;
}
.label {
  font-weight: bold;
  color: #555;
  width: 100px;
}
.value {
  color: #222;
}
.credit-score {
  color: #4da6ff;
  font-weight: bold;
}
.status-normal {
  color: #28a745;
  font-weight: bold;
}
.status-blocked {
  color: #dc3545;
  font-weight: bold;
}

/* 右侧统计数据 */
.stats-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 160px;
}
.small-card {
  padding: 16px;
  font-size: 14px;
}

/* 卡片颜色 */
.card-blue {
  background: linear-gradient(135deg, #4da6ff, #1a73e8);
  color: white;
}
.card-red {
  background: linear-gradient(135deg, #ff6b6b, #e63946);
  color: white;
}
.card-green {
  background: linear-gradient(135deg, #51cf66, #2f9e44);
  color: white;
}

/* 小卡片悬停效果 */
DashBoardInfoCard {
  transition: transform 0.3s, box-shadow 0.3s;
}
DashBoardInfoCard:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 20px rgba(0,0,0,0.15);
}
</style>

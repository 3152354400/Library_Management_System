<template>
  <div class="reservation-page">
    <!-- 1. 筛选器区域 -->
    <div class="filters-container">
      <h1 class="page-title">座位预约</h1>
      <div class="filter-group">
        <label for="building">选择场馆</label>
        <select id="building" v-model="selectedBuilding" @change="onBuildingChange">
          <option :value="null" disabled>请选择场馆</option>
          <option v-for="b in buildings" :key="b.id" :value="b.id">{{ b.name }}</option>
        </select>
      </div>
      <div class="filter-group">
        <label for="floor">选择楼层</label>
        <select id="floor" v-model="selectedFloor" :disabled="!selectedBuilding">
          <option :value="null" disabled>请选择楼层</option>
          <option v-for="f in availableFloors" :key="f" :value="f">{{ f }} 层</option>
        </select>
      </div>
      <div class="legend">
        <span class="legend-item"><span class="box available"></span>空闲</span>
        <span class="legend-item"><span class="box occupied"></span>使用中</span>
      </div>
    </div>

    <!-- 2. 座位地图展示区 -->
    <div class="map-container">
      <div v-if="loading" class="status-message">正在加载座位图...</div>
      <div v-else-if="!selectedFloor" class="status-message">请选择场馆和楼层以查看座位图。</div>
      <div v-else-if="error" class="status-message error">{{ error }}</div>
      <SeatMap 
        v-else 
        :seats="seats" 
        :layout-type="layoutType"
        @seat-selected="handleSeatSelection" 
      />
    </div>

    <!-- 3. 预约弹窗 -->
    <ReservationModal 
      v-if="selectedSeat" 
      :seat="selectedSeat" 
      @close="selectedSeat = null"
      @reserve-success="handleReserveSuccess"
    />
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { getSeatLayout } from '@/modules/reader/api.js';
import SeatMap from '../components/SeatMap.vue';
import ReservationModal from '../components/ReservationModal.vue';

// --- 数据定义 ---
const buildings = [
  // 总图楼层保持不变
  { id: 21, name: '总图书馆', floors: [3, 4, 5] }, 
  // 为德文图书馆添加 1 楼
  { id: 22, name: '德文图书馆', floors: [1, 2] } 
];

const selectedBuilding = ref(buildings[0].id); // 默认选中总图
const selectedFloor = ref(buildings[0].floors[0]); // 默认选中第一个楼层
const loading = ref(false);
const error = ref('');
const seats = ref([]);
const selectedSeat = ref(null);

// --- 计算属性 ---
const availableFloors = computed(() => {
  const building = buildings.find(b => b.id === selectedBuilding.value);
  return building ? building.floors : [];
});

// 为不同楼层设计不同布局
const layoutType = computed(() => {
    if (selectedBuilding.value === 21 && selectedFloor.value === 3) return 'large-room';
    if (selectedBuilding.value === 21 && selectedFloor.value === 4) return 'reading-hall';
    if (selectedBuilding.value === 22) return 'small-room';
    return 'default'; // 默认布局
});

// --- 方法 ---
function onBuildingChange() {
  selectedFloor.value = availableFloors.value[0] || null;
}

async function fetchSeatLayout() {
  if (!selectedBuilding.value || !selectedFloor.value) return;
  
  loading.value = true;
  error.value = '';
  seats.value = [];

  try {
    const response = await getSeatLayout(selectedBuilding.value, selectedFloor.value);
    seats.value = response.data;
  } catch (err) {
    console.error('Failed to fetch seat layout:', err);
    error.value = '无法加载座位信息，请稍后重试。';
  } finally {
    loading.value = false;
  }
}

function handleSeatSelection(seat) {
  if (seat.CurrentStatus === '空闲') {
    selectedSeat.value = seat;
  } else {
    alert('该座位当前正在使用中，请选择其他空闲座位。');
  }
}

function handleReserveSuccess() {
    selectedSeat.value = null;
    // 增加一个 100 毫秒的延迟，等待后端数据同步
    setTimeout(() => {
      fetchSeatLayout(); 
    }, 100); 
}

// --- 侦听器 ---
watch([selectedBuilding, selectedFloor], fetchSeatLayout, { immediate: true });
</script>

<style scoped>
.reservation-page { display: flex; height: calc(100vh - 3.5rem); /* 减去导航栏高度 */ }
.filters-container { width: 280px; padding: 1.5rem; background-color: #f8fafc; border-right: 1px solid #e2e8f0; display: flex; flex-direction: column; }
.page-title { font-size: 1.75rem; font-weight: bold; margin-bottom: 2rem; }
.filter-group { margin-bottom: 1.5rem; }
.filter-group label { display: block; font-weight: 500; margin-bottom: 0.5rem; }
.filter-group select { width: 100%; padding: 0.5rem; border: 1px solid #cbd5e1; border-radius: 6px; }
.map-container { flex-grow: 1; padding: 2rem; overflow: auto; background-color: #fff; }
.status-message { height: 100%; display: flex; justify-content: center; align-items: center; color: #64748b; }
.status-message.error { color: #dc2626; }
.legend { margin-top: auto; padding-top: 1.5rem; border-top: 1px solid #e2e8f0; }
.legend-item { display: flex; align-items: center; margin-bottom: 0.5rem; }
.legend-item .box { width: 1rem; height: 1rem; margin-right: 0.5rem; border-radius: 2px; }
.legend-item .available { background-color: #22c55e; }
.legend-item .occupied { background-color: #ef4444; }
</style>
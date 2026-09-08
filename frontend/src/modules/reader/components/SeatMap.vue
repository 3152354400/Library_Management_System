<template>
  <div class="seat-map-wrapper">
    <!-- 我们为每种布局都创建一个独立的容器，用 v-if 来切换 -->
    
    <!-- 布局1: 大型阅览室 (总图3楼) -->
    <div v-if="layoutType === 'large-room'" class="seat-map layout-large-room">
      <div v-for="seat in seats" :key="seat.SeatID" :class="getSeatClasses(seat)" @click="$emit('seat-selected', seat)">
        {{ seat.SeatNumber }}
      </div>
    </div>

    <!-- 布局2: 现代阅览大厅 (总图4楼) -->
    <div v-else-if="layoutType === 'reading-hall'" class="seat-map layout-reading-hall">
      <div v-for="seat in seats" :key="seat.SeatID" :class="getSeatClasses(seat)" @click="$emit('seat-selected', seat)">
        {{ seat.SeatNumber }}
      </div>
    </div>
    
    <!-- 布局3: 十字形布局 (总图5楼) -->
    <div v-else-if="layoutType === 'cross-layout'" class="seat-map layout-cross-layout">
      <div v-for="seat in seats" :key="seat.SeatID" :class="getSeatClasses(seat)" @click="$emit('seat-selected', seat)">
        {{ seat.SeatNumber }}
      </div>
    </div>
    
    <!-- 布局4: 休闲区布局 (德图1楼) -->
    <div v-else-if="layoutType === 'lounge-area'" class="seat-map layout-lounge-area">
      <div v-for="seat in seats" :key="seat.SeatID" :class="getSeatClasses(seat)" @click="$emit('seat-selected', seat)">
        {{ seat.SeatNumber }}
      </div>
    </div>
    
    <!-- 布局5: 学习舱布局 (德图2楼) -->
    <div v-else-if="layoutType === 'study-pods'" class="seat-map layout-study-pods">
      <div v-for="seat in seats" :key="seat.SeatID" :class="getSeatClasses(seat)" @click="$emit('seat-selected', seat)">
        {{ seat.SeatNumber }}
      </div>
    </div>

    <!-- 默认布局 -->
    <div v-else class="seat-map layout-default">
      <div v-for="seat in seats" :key="seat.SeatID" :class="getSeatClasses(seat)" @click="$emit('seat-selected', seat)">
        {{ seat.SeatNumber }}
      </div>
    </div>

  </div>
</template>

<script setup>
defineProps({
  seats: { type: Array, required: true },
  layoutType: { type: String, default: 'default' }
});
defineEmits(['seat-selected']);

// 一个简单的辅助函数，用于组合 class
function getSeatClasses(seat) {
  return ['seat', seat.CurrentStatus.toLowerCase(), `zone-${seat.Zone.toLowerCase()}`];
}
</script>

<style scoped>
/* --- 基础样式 --- */
.seat-map-wrapper { display: flex; justify-content: center; align-items: flex-start; padding: 2rem; width: 100%; }
.seat { width: 50px; height: 50px; display: flex; justify-content: center; align-items: center; border-radius: 6px; font-weight: bold; cursor: pointer; transition: all 0.2s ease; border: 1px solid transparent; font-size: 14px; }
.seat.空闲 { background-color: #ecfdf5; border-color: #10b981; color: #065f46; }
.seat.空闲:hover { background-color: #d1fae5; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.1); }
.seat.使用中 { background-color: #fee2e2; border-color: #ef4444; color: #991b1b; cursor: not-allowed; opacity: 0.8; }

/* --- 布局容器通用样式 --- */
.seat-map {
  display: grid;
  gap: 12px;
  width: fit-content;
}

/* --- 独立布局样式 --- */

/* 默认 & 总图5楼 & 德图2楼 (都使用简单网格，不再搞复杂布局) */
.layout-default,
.layout-cross-layout,
.layout-study-pods {
  grid-template-columns: repeat(8, 50px); /* 固定为8列，确保排列整齐 */
}

/* 总图3楼 (长桌 + 散座) */
.layout-large-room {
  display: flex;
  flex-direction: column;
  gap: 20px;
}
.room-section {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}
.room-label {
  writing-mode: vertical-rl;
  text-orientation: mixed;
  font-weight: bold;
  color: #9ca3af;
  margin-right: 1rem;
}

/* 总图4楼 (圆形 + 方形) */
.layout-reading-hall {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 20px;
  width: 70%;
}
.layout-reading-hall .seat.zone-c {
  border-radius: 50%;
}
.layout-reading-hall .seat.zone-d {
  width: 100px;
  border-radius: 8px;
}

/* 德图1楼 (休闲区) */
.layout-lounge-area {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 25px;
  width: 400px;
}
.layout-lounge-area .seat.zone-a {
  border-radius: 50%;
  width: 60px; height: 60px;
}
.layout-lounge-area .seat.zone-b {
  border-radius: 12px;
  width: 120px; height: 60px;
}
</style>
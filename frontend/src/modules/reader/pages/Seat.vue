<template>
  <div class="rd-page">
    <PageHeader title="座位预约" subtitle="选择场馆和楼层,预约空闲座位">
      <template #actions>
        <el-button @click="loadSeats" :icon="Refresh">刷新座位</el-button>
      </template>
    </PageHeader>

    <div class="layout">
      <div class="control-panel card">
        <el-form label-position="top" size="default">
          <el-form-item label="场馆">
            <el-select v-model="buildingId" @change="loadSeats" style="width:100%">
              <el-option v-for="b in buildings" :key="b.id" :label="b.name" :value="b.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="楼层">
            <el-radio-group v-model="floor" @change="loadSeats" style="width:100%; display:flex; flex-wrap:wrap; gap:6px;">
              <el-radio-button v-for="f in [1,2,3,4,5]" :key="f" :value="f">{{ f }}F</el-radio-button>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="时段">
            <el-time-picker v-model="timeRange" is-range range-separator="至" format="HH:mm" start-placeholder="开始" end-placeholder="结束" style="width:100%" />
          </el-form-item>
        </el-form>

        <div class="legend">
          <div><span class="dot free" /> 空闲</div>
          <div><span class="dot occupied" /> 已预约</div>
          <div><span class="dot mine" /> 我的预约</div>
        </div>
      </div>

      <div class="seat-area card">
        <div v-if="loading" class="loading"><el-skeleton :rows="6" animated /></div>
        <template v-else>
          <div class="seat-banner">
            <div class="bb-side"></div>
            <div class="bb-zone">{{ buildingName }} · {{ floor }}F · {{ zones.join(' / ') }}</div>
          </div>
          <div class="seat-grid">
            <button
              v-for="s in seats"
              :key="s.SeatID"
              :disabled="s.CurrentStatus === '已预约'"
              :class="['seat', s.CurrentStatus === '已预约' ? 'occupied' : 'free']"
              @click="onSeatClick(s)"
            >
              <span class="sn">{{ s.SeatNumber }}</span>
            </button>
          </div>
        </template>
      </div>

      <div class="my-res card">
        <h4>我的预约</h4>
        <EmptyState v-if="!reservations.length" title="暂无预约" />
        <div v-else class="my-list">
          <div v-for="r in reservations" :key="r.ReservationID" class="my-item">
            <div>
              <div class="seat-name">{{ r.BuildingName }} {{ r.Floor }}F · {{ r.SeatNumber }}</div>
              <div class="time">{{ formatDate(r.StartTime) }} ~ {{ formatDate(r.EndTime) }}</div>
            </div>
            <el-button size="small" type="danger" text @click="onCancel(r)">取消</el-button>
          </div>
        </div>
      </div>
    </div>

    <el-dialog v-model="reserveDialog" title="预约确认" width="420px">
      <div v-if="reserveTarget">
        <div class="reserve-info">
          <div><strong>座位</strong>{{ reserveTarget.SeatNumber }}</div>
          <div><strong>位置</strong>{{ buildingName }} {{ floor }}F · {{ reserveTarget.Zone }}</div>
          <div><strong>时段</strong>{{ formatDate(timeRange?.[0]) }} ~ {{ formatDate(timeRange?.[1]) }}</div>
        </div>
      </div>
      <template #footer>
        <el-button @click="reserveDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmReserve">确认预约</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Refresh } from '@element-plus/icons-vue'
import { spaceApi } from '@/api/space'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const buildings = [
  { id: 1, name: '主图书馆' },
  { id: 2, name: '理工分馆' },
  { id: 21, name: '文学分馆' }
]
const buildingId = ref(1)
const floor = ref(3)
const timeRange = ref([new Date(), new Date(Date.now() + 3 * 3600 * 1000)])
const seats = ref([])
const reservations = ref([])
const loading = ref(false)
const reserveDialog = ref(false)
const reserveTarget = ref(null)

const buildingName = computed(() => buildings.find((b) => b.id === buildingId.value)?.name || '')
const zones = computed(() => [...new Set(seats.value.map((s) => s.Zone).filter(Boolean))])

async function loadSeats() {
  loading.value = true
  try {
    seats.value = await spaceApi.getSeatLayout(buildingId.value, floor.value)
  } finally { loading.value = false }
}

async function loadReservations() {
  reservations.value = await spaceApi.myReservations()
}

function onSeatClick(seat) {
  if (seat.CurrentStatus === '已预约') return ElMessage.warning('该座位已被预约')
  reserveTarget.value = seat
  reserveDialog.value = true
}

async function confirmReserve() {
  if (!reserveTarget.value) return
  await spaceApi.createSeatReservation({
    SeatID: reserveTarget.value.SeatID,
    StartTime: timeRange.value[0],
    EndTime: timeRange.value[1]
  })
  ElMessage.success('预约成功!')
  reserveDialog.value = false
  loadSeats()
  loadReservations()
}

async function onCancel(r) {
  try {
    await ElMessageBox.confirm(`确认取消座位 ${r.SeatNumber} 的预约?`, '提示', { type: 'warning' })
  } catch { return }
  try {
    await spaceApi.cancelReservation(r.ReservationID)
    ElMessage.success('已取消')
    loadSeats()
    loadReservations()
  } catch { /* 接口错误已由统一提示处理 */ }
}

function formatDate(t) {
  if (!t) return ''
  return new Date(t).toLocaleString('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}

onMounted(() => { loadSeats(); loadReservations() })
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
.layout {
  display: grid;
  grid-template-columns: 280px 1fr 320px;
  gap: 16px;
  align-items: flex-start;
}
.control-panel h4, .my-res h4 { margin: 0 0 12px; }
.legend { display: flex; gap: 16px; padding-top: 12px; border-top: 1px solid var(--color-border-soft); font-size: var(--fs-sm); }
.legend > div { display: flex; align-items: center; gap: 6px; }
.dot { width: 14px; height: 14px; border-radius: 4px; display: inline-block; }
.dot.free { background: #fff; border: 2px solid var(--color-accent-green); }
.dot.occupied { background: var(--color-text-muted); }
.dot.mine { background: var(--color-primary-500); }

.seat-banner {
  background: linear-gradient(180deg, var(--color-primary-100), transparent);
  padding: 32px 16px 12px;
  border-radius: var(--radius-md);
  margin-bottom: 24px;
  position: relative;
  text-align: center;
}
.bb-side {
  position: absolute;
  left: 16px;
  top: 16px;
  width: 60px;
  height: 16px;
  background: var(--color-primary-300);
  border-radius: 4px;
}
.bb-zone { font-weight: 600; color: var(--color-primary-700); }

.seat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(60px, 1fr));
  gap: 12px;
}
.seat {
  aspect-ratio: 1;
  border: 2px solid var(--color-accent-green);
  background: #fff;
  border-radius: var(--radius-sm);
  font-size: var(--fs-xs);
  font-weight: 600;
  cursor: pointer;
  color: var(--color-accent-green);
  transition: all .15s;
}
.seat:hover:not(:disabled) { background: var(--color-primary-50); border-color: var(--color-primary-500); color: var(--color-primary-600); }
.seat.occupied {
  background: var(--color-bg);
  border-color: var(--color-text-muted);
  color: var(--color-text-muted);
  cursor: not-allowed;
}
.sn { display: block; padding: 6px 0; }

.my-list { display: flex; flex-direction: column; gap: 8px; }
.my-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 12px;
  background: var(--color-bg);
  border-radius: var(--radius-sm);
}
.seat-name { font-weight: 600; font-size: var(--fs-sm); }
.time { font-size: var(--fs-xs); color: var(--color-text-muted); margin-top: 2px; }

.reserve-info > div { display: flex; gap: 12px; padding: 6px 0; }
.reserve-info strong { min-width: 48px; color: var(--color-text-muted); }

@media (max-width: 1100px) {
  .layout { grid-template-columns: 1fr; }
}
</style>
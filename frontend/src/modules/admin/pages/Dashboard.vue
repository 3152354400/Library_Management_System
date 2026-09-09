<template>
  <div class="ad-page">
    <PageHeader title="管理仪表盘" subtitle="实时掌握图书馆运营全貌" />

    <!-- 概览统计 -->
    <div class="stat-row">
      <StatCard label="馆藏图书" :value="stats.totalBooks || 0" suffix="册" icon="Reading" accent="primary" hint="馆藏副本总数" />
      <StatCard label="注册读者" :value="stats.totalReaders || 0" suffix="人" icon="UserFilled" accent="cyan" />
      <StatCard label="今日借阅" :value="stats.todayBorrows || 0" suffix="次" icon="Tickets" accent="green" />
      <StatCard label="今日归还" :value="stats.todayReturns || 0" suffix="次" icon="RefreshRight" accent="purple" />
    </div>

    <div class="stat-row">
      <StatCard label="逾期图书" :value="stats.overdueCount || 0" suffix="本" icon="WarningFilled" accent="red" hint="需提醒读者" />
      <StatCard label="待缴罚款" :value="stats.pendingFines || 0" suffix="元" icon="Money" accent="orange" />
      <StatCard label="可预约座位" :value="stats.seatAvailable || 0" suffix="个" icon="OfficeBuilding" accent="primary" />
      <StatCard label="本周新书" :value="stats.newBooksThisWeek || 0" suffix="册" icon="Star" accent="green" />
    </div>

    <!-- 待处理 -->
    <div class="card">
      <div class="card-head">
        <h3>待办事项</h3>
        <span class="hint">需要您尽快处理的工作</span>
      </div>
      <div class="todo-grid">
        <div class="todo-item" @click="$router.push('/admin/recommends')">
          <div class="ti-icon" style="background:#E3F8FA;color:#15AABF"><el-icon :size="20"><Promotion /></el-icon></div>
          <div class="ti-body">
            <div class="ti-title">待审核荐购 <span class="badge danger">{{ stats.todoRecommends || 0 }}</span></div>
            <div class="ti-desc">读者推荐的图书等待审核</div>
          </div>
        </div>
        <div class="todo-item" @click="$router.push('/admin/reports')">
          <div class="ti-icon" style="background:#FFE3E3;color:#E03131"><el-icon :size="20"><WarningFilled /></el-icon></div>
          <div class="ti-body">
            <div class="ti-title">待处理举报 <span class="badge danger">{{ stats.todoReports || 0 }}</span></div>
            <div class="ti-desc">违规评论等待审核</div>
          </div>
        </div>
        <div class="todo-item" @click="$router.push('/admin/fines')">
          <div class="ti-icon" style="background:#FFF4E6;color:#F59F00"><el-icon :size="20"><Money /></el-icon></div>
          <div class="ti-body">
            <div class="ti-title">待处理罚款 <span class="badge warning">{{ stats.todoFineAppeals || 0 }}</span></div>
            <div class="ti-desc">读者申请减免的罚款</div>
          </div>
        </div>
        <div class="todo-item" @click="$router.push('/admin/book-loss')">
          <div class="ti-icon" style="background:#F3F0FF;color:#7950F2"><el-icon :size="20"><Document /></el-icon></div>
          <div class="ti-body">
            <div class="ti-title">丢书上报 <span class="badge danger">{{ stats.todoBookLoss || 0 }}</span></div>
            <div class="ti-desc">读者上报的遗失/损坏报告</div>
          </div>
        </div>
      </div>
    </div>

    <div class="dual-col">
      <div class="card">
        <div class="card-head">
          <h3>借阅趋势(近7天)</h3>
        </div>
        <div class="chart-placeholder">
          <svg viewBox="0 0 600 200" preserveAspectRatio="none" class="chart-svg">
            <defs>
              <linearGradient id="trendG" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0" stop-color="#3B5BDB" stop-opacity="0.4"/>
                <stop offset="1" stop-color="#3B5BDB" stop-opacity="0"/>
              </linearGradient>
            </defs>
            <path v-if="trendPath.area" :d="trendPath.area" fill="url(#trendG)"/>
            <path v-if="trendPath.line" :d="trendPath.line" stroke="#3B5BDB" stroke-width="2.5" fill="none"/>
            <g v-if="trendPath.pts.length">
              <circle v-for="(p, i) in trendPath.pts" :key="i" :cx="p[0]" :cy="p[1]" r="4" fill="#3B5BDB"/>
            </g>
          </svg>
          <div class="chart-x">
            <span v-for="d in trendDays" :key="d">{{ d }}</span>
          </div>
        </div>
      </div>

      <div class="card">
        <div class="card-head">
          <h3>热门图书 TOP5</h3>
        </div>
        <ul class="rank-list">
          <li v-for="(b, i) in hotBooks" :key="i">
            <span class="rank" :class="`rank-${i + 1}`">{{ i + 1 }}</span>
            <div class="info">
              <div class="t">{{ b.Title }}</div>
              <div class="a">{{ b.Author }}</div>
            </div>
            <span class="cnt">{{ b.BorrowCount || b.count }}</span>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { adminApi } from '@/api/admin'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatCard from '@/shared/components/StatCard.vue'

const stats = reactive({})
const hotBooks = ref([])

// 借阅趋势(近7天)动态折线
const trendDays = computed(() => (stats.trend || []).map((d) => d.Date || ''))
const trendPath = computed(() => {
  const data = stats.trend || []
  if (!data.length) return { line: '', area: '', pts: [] }
  const max = Math.max(1, ...data.map((d) => d.Count || 0))
  const w = 600
  const h = 200
  const pad = 8
  const step = data.length > 1 ? (w - pad * 2) / (data.length - 1) : 0
  const pts = data.map((d, i) => {
    const x = pad + i * step
    const y = h - pad - ((d.Count || 0) / max) * (h - pad * 2)
    return [Math.round(x), Math.round(y)]
  })
  const line = pts.map((p, i) => (i === 0 ? `M${p[0]},${p[1]}` : `L${p[0]},${p[1]}`)).join(' ')
  const area = `${line} L${pts[pts.length - 1][0]},${h} L${pts[0][0]},${h} Z`
  return { line, area, pts }
})

onMounted(async () => {
  try {
    const d = await adminApi.dashboard()
    Object.assign(stats, {
      totalBooks: d.TotalBooks ?? d.totalBooks ?? 0,
      totalReaders: d.TotalReaders ?? d.totalReaders ?? 0,
      todayBorrows: d.TodayBorrows ?? d.todayBorrows ?? 0,
      todayReturns: d.TodayReturns ?? d.todayReturns ?? 0,
      overdueCount: d.OverdueCount ?? d.overdueCount ?? 0,
      pendingFines: d.PendingFines ?? d.pendingFines ?? 0,
      seatAvailable: d.SeatAvailable ?? d.seatAvailable ?? 0,
      newBooksThisWeek: d.NewBooksThisWeek ?? d.newBooksThisWeek ?? 0,
      todoRecommends: d.TodoRecommends ?? d.todoRecommends ?? 0,
      todoReports: d.TodoReports ?? d.todoReports ?? 0,
      todoFineAppeals: d.TodoFineAppeals ?? d.todoFineAppeals ?? 0,
      todoBookLoss: d.TodoBookLoss ?? d.todoBookLoss ?? 0,
      trend: d.Trend ?? d.trend ?? []
    })
    hotBooks.value = d.HotBooks ?? d.hotBooks ?? []
  } catch (e) { console.warn(e) }
})
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.stat-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
.card-head { display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 16px; }
.card-head h3 { margin: 0; font-size: var(--fs-md); }
.hint { font-size: var(--fs-xs); color: var(--color-text-muted); }
.todo-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 12px;
}
.todo-item {
  display: flex;
  gap: 12px;
  padding: 14px;
  background: var(--color-bg);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: background .2s;
}
.todo-item:hover { background: var(--color-primary-50); }
.ti-icon { width: 40px; height: 40px; border-radius: var(--radius-md); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.ti-title { font-size: var(--fs-sm); font-weight: 600; }
.ti-desc { font-size: var(--fs-xs); color: var(--color-text-muted); margin-top: 2px; }
.badge { font-size: 10px; padding: 1px 8px; border-radius: 999px; margin-left: 6px; vertical-align: middle; }
.badge.danger { background: #FFE3E3; color: var(--color-accent-red); }
.badge.warning { background: #FFF4E6; color: var(--color-accent-orange); }

.dual-col { display: grid; grid-template-columns: 1.6fr 1fr; gap: 16px; }
.chart-placeholder { padding: 16px 0; }
.chart-svg { width: 100%; height: 200px; display: block; }
.chart-x { display: flex; justify-content: space-between; padding: 8px 16px 0; font-size: var(--fs-xs); color: var(--color-text-muted); }
.rank-list { display: flex; flex-direction: column; gap: 12px; }
.rank-list li { display: flex; align-items: center; gap: 12px; padding: 8px 0; }
.rank {
  width: 28px; height: 28px; border-radius: 999px;
  display: flex; align-items: center; justify-content: center;
  background: var(--color-bg);
  color: var(--color-text-soft);
  font-weight: 700;
  font-size: var(--fs-xs);
  flex-shrink: 0;
}
.rank-1 { background: linear-gradient(135deg, #FFD43B, #F59F00); color: #fff; }
.rank-2 { background: linear-gradient(135deg, #CED4DA, #868E96); color: #fff; }
.rank-3 { background: linear-gradient(135deg, #DAA66D, #B06838); color: #fff; }
.info { flex: 1; min-width: 0; }
.info .t { font-size: var(--fs-sm); font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.info .a { font-size: var(--fs-xs); color: var(--color-text-muted); }
.cnt { font-weight: 600; color: var(--color-primary-600); font-size: var(--fs-sm); font-feature-settings: 'tnum'; }
@media (max-width: 1100px) {
  .stat-row { grid-template-columns: repeat(2, 1fr); }
  .dual-col { grid-template-columns: 1fr; }
}
</style>
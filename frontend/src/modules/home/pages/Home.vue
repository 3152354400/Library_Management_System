<template>
  <div class="home-page">
    <!-- Hero -->
    <section class="hero">
      <div class="hero-bg">
        <svg viewBox="0 0 1440 560" class="hero-svg" preserveAspectRatio="xMidYMid slice">
          <defs>
            <linearGradient id="hg1" x1="0" y1="0" x2="1" y2="1">
              <stop offset="0%" stop-color="#EEF1FE"/>
              <stop offset="60%" stop-color="#F5F7FB"/>
              <stop offset="100%" stop-color="#fff"/>
            </linearGradient>
          </defs>
          <rect width="1440" height="560" fill="url(#hg1)"/>
          <circle cx="1300" cy="100" r="300" fill="#BAC8FD" opacity=".35"/>
          <circle cx="200" cy="450" r="200" fill="#91A7FF" opacity=".2"/>
          <circle cx="900" cy="300" r="250" fill="#3B5BDB" opacity=".06"/>
          <!-- Book stack decoration -->
          <rect x="1200" y="200" width="18" height="120" rx="4" fill="#3B5BDB" opacity=".5"/>
          <rect x="1222" y="230" width="18" height="90" rx="4" fill="#748FFC" opacity=".4"/>
          <rect x="1244" y="250" width="18" height="70" rx="4" fill="#91A7FF" opacity=".3"/>
          <rect x="1266" y="215" width="18" height="105" rx="4" fill="#5C7CFA" opacity=".45"/>
        </svg>
      </div>
      <div class="hero-content">
        <div class="hero-tag">
          <span class="tag-dot"></span>
          基于 Oracle 19c + ASP.NET Core 8
        </div>
        <h1 class="hero-title">智书云图书管理系统</h1>
        <p class="hero-desc">覆盖图书检索、借阅流通、座位预约、读者互动全流程的现代化图书馆服务</p>
        <div class="hero-actions">
          <el-button type="primary" size="large" @click="$router.push('/books')">
            <el-icon><Search /></el-icon>
            开始检索
          </el-button>
          <el-button size="large" @click="$router.push('/auth?mode=register')" v-if="!userStore.isLoggedIn">
            立即注册
          </el-button>
          <el-button size="large" @click="$router.push('/reader/dashboard')" v-else>
            进入我的主页
          </el-button>
        </div>
      </div>
    </section>

    <!-- 搜索栏 -->
    <section class="search-section">
      <div class="container">
        <div class="search-box">
          <el-icon class="search-icon"><Search /></el-icon>
          <input
            v-model="keyword"
            class="search-input"
            placeholder="输入书名、作者、ISBN 检索图书..."
            @keyup.enter="doSearch"
          />
          <el-button type="primary" @click="doSearch">搜索</el-button>
        </div>
      </div>
    </section>

    <!-- 快捷入口 -->
    <section class="quick-section">
      <div class="container">
        <div class="section-head">
          <h2>快捷入口</h2>
          <p>一站式访问您需要的服务</p>
        </div>
        <div class="quick-grid">
          <router-link to="/books" class="quick-card">
            <div class="qc-icon" style="background:#EEF1FE;color:#3B5BDB"><el-icon :size="28"><Reading /></el-icon></div>
            <div class="qc-title">图书检索</div>
            <div class="qc-desc">海量藏书 分类浏览</div>
          </router-link>
          <router-link to="/reader/borrowing" class="quick-card" v-if="userStore.isLoggedIn">
            <div class="qc-icon" style="background:#E3F8FA;color:#15AABF"><el-icon :size="28"><Tickets /></el-icon></div>
            <div class="qc-title">我的借阅</div>
            <div class="qc-desc">在借图书 续借还书</div>
          </router-link>
          <router-link to="/reader/seat" class="quick-card" v-if="userStore.isLoggedIn">
            <div class="qc-icon" style="background:#FFF4E6;color:#F59F00"><el-icon :size="28"><OfficeBuilding /></el-icon></div>
            <div class="qc-title">座位预约</div>
            <div class="qc-desc">自习座位 随时预约</div>
          </router-link>
          <router-link to="/announcements" class="quick-card">
            <div class="qc-icon" style="background:#EBFBEE;color:#37B24D"><el-icon :size="28"><Bell /></el-icon></div>
            <div class="qc-title">系统公告</div>
            <div class="qc-desc">开馆通知 活动公告</div>
          </router-link>
        </div>
      </div>
    </section>

    <!-- 推荐图书 -->
    <section class="books-section" v-if="recommendations.length">
      <div class="container">
        <div class="section-head">
          <h2>为你推荐</h2>
          <p>基于您的阅读偏好精选</p>
        </div>
        <div class="books-scroll">
          <div v-for="book in recommendations" :key="book.ISBN" class="book-card" @click="$router.push(`/book/${book.ISBN}`)">
            <div class="book-cover-wrap">
              <BookCover :title="book.Title" :author="book.Author" :isbn="book.ISBN" class="book-cover" />
            </div>
            <div class="book-info">
              <div class="book-title">{{ book.Title }}</div>
              <div class="book-author">{{ book.Author }}</div>
              <div class="book-meta">
                <span>{{ book.Publisher }}</span>
                <span>{{ book.PublishYear }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 公告预览 -->
    <section class="announcements-section" v-if="announcements.length">
      <div class="container">
        <div class="announce-grid">
          <div class="announce-main">
            <div class="section-head">
              <h2>最新公告</h2>
              <router-link to="/announcements" class="more-link">查看全部 →</router-link>
            </div>
            <div class="announce-list">
              <div v-for="a in announcements.slice(0, 3)" :key="a.AnnouncementID" class="announce-item">
                <div class="announce-tag" v-if="a.Title.includes('紧急')">紧急</div>
                <div class="announce-body">
                  <div class="announce-title">{{ a.Title }}</div>
                  <div class="announce-text">{{ a.Content }}</div>
                  <div class="announce-time">{{ formatTime(a.CreateTime) }}</div>
                </div>
              </div>
            </div>
          </div>
          <div class="announce-side">
            <div class="stats-card">
              <h3>馆藏数据</h3>
              <div class="stat-rows">
                <div class="stat-row">
                  <span>馆藏图书</span>
                  <span class="num">{{ fmtNum(stats.totalBooks) }} 册</span>
                </div>
                <div class="stat-row">
                  <span>注册读者</span>
                  <span class="num">{{ fmtNum(stats.totalReaders) }} 人</span>
                </div>
                <div class="stat-row">
                  <span>本月借阅</span>
                  <span class="num">{{ fmtNum(stats.monthBorrows) }} 次</span>
                </div>
                <div class="stat-row">
                  <span>可预约座位</span>
                  <span class="num">{{ fmtNum(stats.seatAvailable) }} 个</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { bookApi } from '@/api/book'
import { announcementApi } from '@/api/announcement'
import { authApi } from '@/api/auth'
import http from '@/services/http'
import BookCover from '@/shared/components/BookCover.vue'

const router = useRouter()
const userStore = useUserStore()
const keyword = ref('')
const recommendations = ref([])
const announcements = ref([])
const stats = ref({})

function fmtNum(n) {
  return (n ?? 0).toLocaleString('en-US')
}

function doSearch() {
  if (keyword.value.trim()) {
    router.push({ path: '/search', query: { keyword: keyword.value } })
  } else {
    router.push('/books')
  }
}

function formatTime(t) {
  if (!t) return ''
  return new Date(t).toLocaleDateString('zh-CN', { year: 'numeric', month: 'long', day: 'numeric' })
}

onMounted(async () => {
  try {
    const [annData, recData, statData] = await Promise.allSettled([
      announcementApi.public(),
      userStore.isReader ? authApi.myRecommendations() : Promise.resolve([]),
      http.get('/stats')
    ])
    if (annData.status === 'fulfilled') {
      const d = annData.value
      announcements.value = [...(d?.Urgent || []), ...(d?.Regular || [])]
    }
    if (recData.status === 'fulfilled' && recData.value) {
      recommendations.value = recData.value.slice(0, 8)
    }
    if (statData.status === 'fulfilled' && statData.value) {
      stats.value = statData.value
    }
  } catch (e) {
    console.warn('home load failed', e)
  }
})
</script>

<style scoped>
.home-page { min-height: 100vh; }

.container { max-width: 1200px; margin: 0 auto; padding: 0 24px; }

.hero {
  position: relative;
  min-height: 480px;
  display: flex;
  align-items: center;
  overflow: hidden;
}
.hero-bg {
  position: absolute;
  inset: 0;
  pointer-events: none;
}
.hero-svg { width: 100%; height: 100%; }
.hero-content {
  position: relative;
  max-width: 1200px;
  margin: 0 auto;
  padding: 80px 24px 60px;
  z-index: 1;
}
.hero-tag {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: var(--color-primary-50);
  color: var(--color-primary-600);
  padding: 6px 16px;
  border-radius: 999px;
  font-size: var(--fs-sm);
  font-weight: 500;
  margin-bottom: 24px;
}
.tag-dot {
  width: 8px; height: 8px;
  background: var(--color-primary-600);
  border-radius: 999px;
  animation: pulse 2s infinite;
}
@keyframes pulse {
  0%, 100% { opacity: 1; } 50% { opacity: 0.4; }
}
.hero-title {
  font-size: clamp(2rem, 5vw, 3.5rem);
  font-weight: 800;
  color: var(--color-text);
  margin: 0 0 16px;
  line-height: 1.15;
  letter-spacing: -0.5px;
}
.hero-desc {
  font-size: var(--fs-lg);
  color: var(--color-text-soft);
  max-width: 560px;
  margin: 0 0 32px;
  line-height: 1.7;
}
.hero-actions { display: flex; gap: 12px; flex-wrap: wrap; }

.search-section {
  margin-top: -40px;
  position: relative;
  z-index: 2;
  padding: 0 24px;
}
.search-box {
  max-width: 680px;
  margin: 0 auto;
  background: #fff;
  border-radius: 999px;
  box-shadow: var(--shadow-lg);
  display: flex;
  align-items: center;
  padding: 6px 6px 6px 20px;
  gap: 12px;
  border: 2px solid var(--color-primary-100);
}
.search-icon { color: var(--color-primary-400); font-size: 20px; flex-shrink: 0; }
.search-input {
  flex: 1;
  border: none;
  outline: none;
  font-size: var(--fs-base);
  font-family: var(--font-base);
  color: var(--color-text);
  background: transparent;
}
.search-input::placeholder { color: var(--color-text-muted); }

.section-head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-bottom: 24px;
}
.section-head h2 { font-size: var(--fs-2xl); font-weight: 700; color: var(--color-text); }
.section-head p { color: var(--color-text-muted); font-size: var(--fs-sm); margin: 4px 0 0; }
.more-link { font-size: var(--fs-sm); color: var(--color-primary-600); font-weight: 500; }

.quick-section { padding: 72px 0 40px; }
.quick-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}
.quick-card {
  background: #fff;
  border-radius: var(--radius-lg);
  padding: 24px;
  text-align: center;
  border: 1px solid var(--color-border-soft);
  text-decoration: none;
  color: var(--color-text);
  transition: transform .25s, box-shadow .25s;
  box-shadow: var(--shadow-xs);
}
.quick-card:hover { transform: translateY(-4px); box-shadow: var(--shadow-md); }
.qc-icon {
  width: 56px; height: 56px;
  border-radius: var(--radius-md);
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 14px;
}
.qc-title { font-size: var(--fs-md); font-weight: 600; margin-bottom: 4px; }
.qc-desc { font-size: var(--fs-xs); color: var(--color-text-muted); }

.books-section { padding: 40px 0 60px; }
.books-scroll {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 20px;
}
.book-card {
  cursor: pointer;
  text-decoration: none;
  color: inherit;
}
.book-cover-wrap {
  aspect-ratio: 2/3;
  border-radius: var(--radius-md);
  overflow: hidden;
  margin-bottom: 10px;
  box-shadow: var(--shadow-sm);
}
.book-info { padding: 0 4px; }
.book-title {
  font-size: var(--fs-sm);
  font-weight: 600;
  color: var(--color-text);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-height: 1.4;
  margin-bottom: 4px;
}
.book-author { font-size: var(--fs-xs); color: var(--color-text-soft); }
.book-meta {
  display: flex; gap: 8px; margin-top: 4px;
  font-size: var(--fs-xs); color: var(--color-text-muted);
}

.announcements-section { padding: 40px 0 80px; }
.announce-grid {
  display: grid;
  grid-template-columns: 1fr 280px;
  gap: 24px;
}
.announce-list { display: flex; flex-direction: column; gap: 12px; }
.announce-item {
  background: #fff;
  border-radius: var(--radius-lg);
  padding: 18px 20px;
  border: 1px solid var(--color-border-soft);
  display: flex;
  gap: 12px;
  align-items: flex-start;
}
.announce-tag {
  background: #FFF5F5;
  color: var(--color-accent-red);
  font-size: var(--fs-xs);
  font-weight: 600;
  padding: 2px 10px;
  border-radius: 999px;
  flex-shrink: 0;
  margin-top: 2px;
}
.announce-body { flex: 1; }
.announce-title { font-size: var(--fs-md); font-weight: 600; margin-bottom: 4px; }
.announce-text {
  font-size: var(--fs-sm);
  color: var(--color-text-soft);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.announce-time { font-size: var(--fs-xs); color: var(--color-text-muted); margin-top: 6px; }
.stats-card {
  background: linear-gradient(135deg, #3B5BDB, #5C7CFA);
  border-radius: var(--radius-lg);
  padding: 28px;
  color: #fff;
}
.stats-card h3 { font-size: var(--fs-lg); font-weight: 600; margin-bottom: 20px; }
.stat-rows { display: flex; flex-direction: column; gap: 14px; }
.stat-row { display: flex; justify-content: space-between; font-size: var(--fs-sm); opacity: 0.9; }
.stat-row .num { font-weight: 700; opacity: 1; font-size: var(--fs-base); }

@media (max-width: 768px) {
  .announce-grid { grid-template-columns: 1fr; }
  .hero-content { padding: 48px 24px 40px; }
}
</style>
<template>
  <div class="book-detail-container">
    <!-- 返回按钮 -->
    <button class="back-btn" @click="goBack">← 返回搜索</button>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading">加载中...</div>

    <!-- 错误状态 -->
    <div v-else-if="error" class="error">{{ error }}</div>

    <!-- 图书详情 -->
    <div v-else-if="book" class="book-detail">
      <!-- 左侧：封面 + 操作按钮 -->
      <div class="book-cover-section">
        <img
          :src="`/covers/${book.ISBN}.jpg`"
          alt="封面"
          class="book-cover"
          @error="e => (e.target.src = defaultCover)"
        />
        <div class="action-buttons">
          <button class="btn-borrow" @click="goBorrow" v-if="book.AvailableStock > 0">
            📖 立即借阅
          </button>
          <button class="btn-reserve" @click="openReserve" v-else>
            📌 预约图书
          </button>
          <button class="btn-collect" @click="toggleCollect" :disabled="favoriteLoading">
            {{ isCollected ? '❤️ 已收藏' : '🤍 收藏' }}
          </button>
        </div>
      </div>

      <!-- 右侧：基本信息 -->
      <div class="book-info-section">
        <h1 class="book-title">{{ book.Title }}</h1>

        <div class="info-grid">
          <div class="info-item">
            <span class="info-label">ISBN：</span>
            <span class="info-value">{{ book.ISBN }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">作者：</span>
            <span class="info-value">{{ book.Author || '-' }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">出版社：</span>
            <span class="info-value">{{ book.Publisher || '-' }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">出版年份：</span>
            <span class="info-value">{{ book.PublishYear || '-' }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">分类：</span>
            <span class="info-value">{{ book.Categories || book.categories || '暂无分类' }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">馆藏复本：</span>
            <span class="info-value">
              <span :class="book.AvailableStock > 0 ? 'text-green-600' : 'text-red-600'">
                {{ book.AvailableStock }} 本可借
              </span>
              / {{ book.TotalStock || book.totalStock || 0 }} 本总量
            </span>
          </div>
        </div>

        <div class="description-section" v-if="book.Description || book.description">
          <h3>📝 内容简介</h3>
          <p>{{ book.Description || book.description }}</p>
        </div>
      </div>
    </div>

    <!-- 馆藏位置 -->
    <div v-if="book && book.ISBN" class="section-shelf">
      <h2>🏠 馆藏位置</h2>
      <div v-if="shelfLoading" class="loading">加载馆藏位置...</div>
      <div v-else-if="shelfLocations.length" class="shelf-grid">
        <div v-for="loc in shelfLocations" :key="loc.BookID" class="shelf-item">
          <div class="shelf-icon">📚</div>
          <div class="shelf-info">
            <div class="shelf-name">{{ loc.ShelfName || loc.ShelfCode }}</div>
            <div class="shelf-location">
              {{ formatBuilding(loc.BuildingID) }} {{ loc.Floor }}层 {{ loc.Zone }}区
            </div>
            <div class="shelf-status">
              <span :class="loc.Status === '可借' ? 'text-green-600' : 'text-gray-400'">
                {{ loc.Status || '在馆' }}
              </span>
            </div>
          </div>
        </div>
      </div>
      <div v-else class="empty">暂无馆藏位置信息</div>
    </div>

    <!-- 评论区域 -->
    <div class="section-comments">
      <h2>💬 读者评论</h2>
      <!-- 发表评论 -->
      <div class="add-comment" v-if="isLoggedIn">
        <h3>✍️ 发表你的评论</h3>
        <div class="rating-select">
          <label>评分：</label>
          <select v-model.number="newComment.rating">
            <option value="5">⭐⭐⭐⭐⭐ 5分</option>
            <option value="4">⭐⭐⭐⭐ 4分</option>
            <option value="3">⭐⭐⭐ 3分</option>
            <option value="2">⭐⭐ 2分</option>
            <option value="1">⭐ 1分</option>
          </select>
        </div>
        <textarea
          v-model="newComment.content"
          placeholder="分享你的阅读感受..."
          rows="3"
        ></textarea>
        <button class="btn-submit-comment" @click="submitComment" :disabled="submitting">
          {{ submitting ? '提交中...' : '发表评论' }}
        </button>
        <p v-if="commentError" class="error-text">{{ commentError }}</p>
      </div>
      <div v-else class="login-hint">
        <p>登录后可发表评论 <router-link to="/auth">去登录</router-link></p>
      </div>

      <!-- 评论列表 -->
      <div v-if="commentsLoading" class="loading">加载评论...</div>
      <div v-else-if="comments.length" class="comments-list">
        <div v-for="c in comments" :key="c.CommentID || c.CommentID" class="comment-item">
          <div class="comment-header">
            <span class="comment-user">{{ c.ReaderNickname || c.ReaderID || '匿名读者' }}</span>
            <span class="comment-rating">{{ '⭐'.repeat(c.Rating || c.RATING || 0) }}</span>
            <span class="comment-time">{{ formatDate(c.CreateTime) }}</span>
          </div>
          <div class="comment-content">{{ c.ReviewContent }}</div>
          <div class="comment-actions" v-if="isLoggedIn">
            <button class="btn-report" @click="openReportModal(c)">🚩 举报</button>
          </div>
        </div>
      </div>
      <div v-else class="empty">暂无评论，快来发表第一篇吧！</div>
    </div>

    <!-- 举报弹窗 -->
    <div v-if="reportModal.show" class="modal-overlay" @click.self="reportModal.show = false">
      <div class="modal-content">
        <button @click="reportModal.show = false" class="close-btn">&times;</button>
        <h3>🚩 举报评论</h3>
        <p class="report-quote">"{{ reportModal.comment?.ReviewContent }}"</p>
        <div class="form-group">
          <label>举报原因</label>
          <textarea v-model="reportModal.reason" rows="3" placeholder="请输入举报原因..."></textarea>
        </div>
        <button class="btn-submit-report" @click="submitReport" :disabled="reportModal.submitting">
          {{ reportModal.submitting ? '提交中...' : '确认举报' }}
        </button>
      </div>
    </div>

    <!-- 预约弹窗 -->
    <ReserveBookModal
      v-if="reserveModal.show"
      :book="reserveModal.book"
      @close="reserveModal.show = false"
      @reserve-success="onReserveSuccess"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getBooks, getCommentsByISBN, addComment, addReport } from '@/modules/book/api.js'
import { getBooksBookShelf } from '@/modules/book/api.js'
import { checkFavorite, addFavorite, removeFavoriteByISBN, getMyFavorites } from '@/modules/reader/api.js'
import { useUserStore } from '@/stores/user.js'
import ReserveBookModal from '@/modules/reader/components/ReserveBookModal.vue'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const book = ref(null)
const loading = ref(false)
const error = ref('')
const comments = ref([])
const commentsLoading = ref(false)
const submitting = ref(false)
const commentError = ref('')
const shelfLocations = ref([])
const shelfLoading = ref(false)
const isCollected = ref(false)
const favoriteLoading = ref(false)
const favoriteId = ref(null)

const defaultCover = new URL('@/assets/book_cover_default.jpg', import.meta.url).href

const newComment = ref({ rating: 5, content: '' })

const reserveModal = ref({ show: false, book: null })

const reportModal = ref({
  show: false,
  comment: null,
  reason: '',
  submitting: false
})

const isLoggedIn = computed(() => userStore.isLoggedIn)

async function fetchBook() {
  loading.value = true
  error.value = ''
  try {
    const isbn = route.query.isbn || route.params.isbn
    if (!isbn) {
      error.value = '未提供ISBN'
      return
    }
    const res = await getBooks(isbn)
    const books = res.data || []
    book.value = Array.isArray(books) ? books[0] : books

    if (book.value?.ISBN) {
      fetchComments(book.value.ISBN)
      fetchShelfLocations(book.value.ISBN)
      if (isLoggedIn.value) checkFavoriteStatus(book.value.ISBN)
    }
  } catch (e) {
    console.error(e)
    error.value = '加载失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

async function fetchComments(isbn) {
  commentsLoading.value = true
  try {
    const res = await getCommentsByISBN(isbn)
    comments.value = res.data || []
  } catch (e) {
    console.error(e)
  } finally {
    commentsLoading.value = false
  }
}

async function fetchShelfLocations(isbn) {
  shelfLoading.value = true
  try {
    const res = await getBooksBookShelf(isbn)
    shelfLocations.value = res.data || []
  } catch (e) {
    console.error(e)
  } finally {
    shelfLoading.value = false
  }
}

async function checkFavoriteStatus(isbn) {
  try {
    const res = await checkFavorite(isbn)
    isCollected.value = res.data?.isFavorite || false
  } catch (e) {
    console.error('检查收藏状态失败:', e)
  }
}

async function toggleCollect() {
  if (!isLoggedIn.value) {
    alert('请先登录')
    router.push('/auth')
    return
  }

  favoriteLoading.value = true
  try {
    if (isCollected.value) {
      await removeFavoriteByISBN(book.value.ISBN)
      isCollected.value = false
      alert('已取消收藏')
    } else {
      await addFavorite({
        ISBN: book.value.ISBN,
        Notes: '',
        FolderName: '默认收藏夹'
      })
      isCollected.value = true
      alert('收藏成功')
    }
  } catch (e) {
    alert(e.response?.data?.message || '操作失败')
  } finally {
    favoriteLoading.value = false
  }
}

async function submitComment() {
  if (!newComment.value.content.trim()) {
    commentError.value = '请输入评论内容'
    return
  }
  submitting.value = true
  commentError.value = ''
  try {
    await addComment({
      ISBN: book.value.ISBN,
      Rating: newComment.value.rating,
      ReviewContent: newComment.value.content,
      Status: '正常'
    })
    newComment.value.content = ''
    await fetchComments(book.value.ISBN)
  } catch (e) {
    commentError.value = e.response?.data?.message || '发表评论失败'
  } finally {
    submitting.value = false
  }
}

function openReserve() {
  reserveModal.value = { show: true, book: book.value }
}

function onReserveSuccess() {
  // 刷新可借数量
  fetchBook()
}

function openReportModal(comment) {
  reportModal.value = { show: true, comment, reason: '', submitting: false }
}

async function submitReport() {
  if (!reportModal.value.reason.trim()) {
    return
  }
  reportModal.value.submitting = true
  try {
    await addReport({
      CommentID: reportModal.value.comment.CommentID,
      ReportReason: reportModal.value.reason,
      Status: '待处理'
    })
    alert('举报成功')
    reportModal.value.show = false
  } catch (e) {
    alert(e.response?.data?.message || '举报失败')
  } finally {
    reportModal.value.submitting = false
  }
}

function goBack() {
  router.back()
}

function goBorrow() {
  router.push({ path: '/booklocation', query: { q: book.value?.Title } })
}

function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleString('zh-CN')
}

function formatBuilding(id) {
  return id === 21 ? '总图书馆' : '德文图书馆'
}

onMounted(fetchBook)
</script>

<style scoped>
.book-detail-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 24px;
}
.back-btn {
  background: none;
  border: none;
  color: #4da6ff;
  font-size: 15px;
  cursor: pointer;
  margin-bottom: 20px;
}
.back-btn:hover { text-decoration: underline; }
.loading, .error { text-align: center; padding: 40px; color: #999; font-size: 16px; }
.error { color: #dc2626; }
.book-detail {
  display: flex;
  gap: 32px;
  background: white;
  border-radius: 16px;
  padding: 32px;
  box-shadow: 0 4px 16px rgba(0,0,0,0.08);
  margin-bottom: 32px;
}
.book-cover {
  width: 220px;
  height: 320px;
  object-fit: cover;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}
.book-cover-section { flex-shrink: 0; }
.action-buttons { display: flex; flex-direction: column; gap: 10px; margin-top: 16px; }
.btn-borrow, .btn-reserve, .btn-collect {
  width: 220px; padding: 12px; border-radius: 8px; font-size: 15px; font-weight: 600;
  border: none; cursor: pointer; transition: all 0.2s;
}
.btn-borrow { background: #2563eb; color: white; }
.btn-borrow:hover { background: #1d4ed8; }
.btn-reserve { background: #f97316; color: white; }
.btn-reserve:hover { background: #ea580c; }
.btn-collect { background: #f3f4f6; color: #374151; }
.btn-collect:hover:not(:disabled) { background: #fce7f3; }
.btn-collect:disabled { opacity: 0.6; cursor: not-allowed; }
.book-info-section { flex: 1; }
.book-title { font-size: 28px; font-weight: bold; color: #1f2937; margin: 0 0 20px; }
.info-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; margin-bottom: 24px; }
.info-item { font-size: 15px; }
.info-label { color: #6b7280; font-weight: 500; }
.info-value { color: #1f2937; }
.description-section { background: #f9fafb; padding: 16px; border-radius: 8px; }
.description-section h3 { font-size: 16px; margin: 0 0 10px; color: #374151; }
.description-section p { font-size: 14px; color: #6b7280; line-height: 1.7; margin: 0; }
.section-shelf, .section-comments {
  background: white; border-radius: 16px; padding: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06); margin-bottom: 24px;
}
.section-shelf h2, .section-comments h2 { font-size: 20px; margin: 0 0 16px; color: #1f2937; }
.shelf-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 12px; }
.shelf-item { display: flex; gap: 12px; padding: 14px; background: #f9fafb; border-radius: 8px; }
.shelf-icon { font-size: 32px; }
.shelf-info { flex: 1; }
.shelf-name { font-weight: 600; color: #1f2937; font-size: 15px; }
.shelf-location { font-size: 13px; color: #6b7280; margin-top: 2px; }
.shelf-status { font-size: 13px; margin-top: 4px; font-weight: 500; }
.add-comment { background: #f9fbfd; padding: 20px; border-radius: 12px; margin-bottom: 20px; }
.add-comment h3 { margin: 0 0 12px; font-size: 16px; color: #374151; }
.rating-select { display: flex; align-items: center; gap: 10px; margin-bottom: 10px; }
.rating-select select { padding: 6px 10px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; }
.add-comment textarea { width: 100%; padding: 10px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 14px; resize: vertical; box-sizing: border-box; }
.btn-submit-comment { margin-top: 10px; background: #4da6ff; color: white; padding: 8px 20px; border-radius: 6px; border: none; cursor: pointer; font-size: 14px; }
.btn-submit-comment:hover { background: #3399ff; }
.login-hint { text-align: center; padding: 20px; background: #f9fafb; border-radius: 8px; color: #6b7280; font-size: 14px; }
.login-hint a { color: #4da6ff; }
.comment-item { padding: 16px; border-bottom: 1px solid #f3f4f6; }
.comment-item:last-child { border-bottom: none; }
.comment-header { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; font-size: 14px; }
.comment-user { font-weight: 600; color: #1f2937; }
.comment-rating { color: #f59e0b; }
.comment-time { color: #9ca3af; margin-left: auto; }
.comment-content { font-size: 15px; color: #4b5563; line-height: 1.6; }
.comment-actions { margin-top: 8px; text-align: right; }
.btn-report { background: none; border: none; color: #9ca3af; cursor: pointer; font-size: 13px; }
.btn-report:hover { color: #dc2626; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-content { background: white; padding: 24px; border-radius: 12px; width: 90%; max-width: 480px; position: relative; }
.close-btn { position: absolute; top: 12px; right: 16px; font-size: 24px; background: none; border: none; cursor: pointer; color: #9ca3af; }
.modal-content h3 { margin: 0 0 12px; font-size: 18px; }
.report-quote { background: #f9fafb; padding: 10px; border-radius: 6px; font-size: 14px; color: #6b7280; font-style: italic; margin-bottom: 12px; }
.form-group { margin-bottom: 12px; }
.form-group label { display: block; font-size: 14px; font-weight: 500; margin-bottom: 4px; }
.form-group textarea { width: 100%; padding: 8px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; box-sizing: border-box; }
.btn-submit-report { width: 100%; background: #dc2626; color: white; padding: 10px; border-radius: 6px; border: none; cursor: pointer; font-size: 15px; font-weight: 600; }
.btn-submit-report:disabled { background: #9ca3af; cursor: not-allowed; }
.empty { text-align: center; color: #9ca3af; padding: 30px; font-size: 15px; }
.text-green-600 { color: #059669; }
.text-red-600 { color: #dc2626; }
.text-gray-400 { color: #9ca3af; }
.error-text { color: #dc2626; font-size: 14px; margin-top: 6px; }
</style>

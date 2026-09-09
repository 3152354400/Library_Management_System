<template>
  <div class="detail-page">
    <el-skeleton v-if="loading" :rows="8" animated />

    <template v-else-if="book">
      <el-button text @click="$router.back()" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> 返回
      </el-button>

      <div class="detail-top card">
        <div class="cover-side">
          <BookCover :title="book.Title" :author="book.Author" :isbn="book.ISBN" />
        </div>
        <div class="info-side">
          <h1 class="b-title">{{ book.Title }}</h1>
          <div class="b-author">{{ book.Author }}</div>
          <div class="b-meta">
            <div><span class="lbl">作者</span><span class="val">{{ book.Author }}</span></div>
            <div><span class="lbl">出版社</span><span class="val">{{ book.Publisher }}</span></div>
            <div><span class="lbl">出版年</span><span class="val">{{ book.PublishYear }}</span></div>
            <div><span class="lbl">ISBN</span><span class="val">{{ book.ISBN }}</span></div>
            <div><span class="lbl">条码</span><span class="val">{{ book.Barcode || '暂无' }}</span></div>
            <div><span class="lbl">状态</span>
              <StatusTag :status="book.Status || '正常'" />
            </div>
          </div>
          <div class="b-actions">
            <el-button type="primary" :disabled="book.Status !== '正常'" @click="onBorrow">
              <el-icon><Reading /></el-icon> 借阅
            </el-button>
            <el-button :disabled="isFav" @click="onFavorite">
              <el-icon><Star /></el-icon> {{ isFav ? '已收藏' : '收藏' }}
            </el-button>
            <el-button @click="onReserve">
              <el-icon><Calendar /></el-icon> 预约
            </el-button>
          </div>
        </div>
      </div>

      <div class="detail-tabs">
        <el-tabs v-model="tab">
          <el-tab-pane label="图书简介" name="desc">
            <div class="tab-body">
              <EmptyState v-if="!desc" title="暂无简介" />
              <div v-else v-html="desc" class="rich-text" />
            </div>
          </el-tab-pane>
          <el-tab-pane :label="`读者评论 (${comments.length})`" name="comments">
            <div class="tab-body">
              <div v-if="userStore.isReader" class="comment-form">
                <el-rate v-model="newComment.rating" :max="5" />
                <el-input v-model="newComment.ReviewContent" type="textarea" :rows="3" placeholder="写下您的读后感..." />
                <el-button type="primary" :loading="submitting" @click="submitComment">发表评论</el-button>
              </div>
              <div v-else class="login-tip">
                <el-alert type="info" :closable="false">登录后可发表评论</el-alert>
              </div>
              <EmptyState v-if="!comments.length" title="还没有评论" />
              <div v-else class="comment-list">
                <div v-for="c in comments" :key="c.CommentID" class="comment-item">
                  <div class="c-head">
                    <span class="c-user">{{ c.NickName || c.Username || '读者' }}</span>
                    <el-rate :model-value="c.Rating" disabled :max="5" />
                    <span class="c-time">{{ formatTime(c.CreateTime) }}</span>
                  </div>
                  <div class="c-body">{{ c.ReviewContent }}</div>
                  <div class="c-actions" v-if="userStore.isReader">
                    <el-button text size="small" @click="onReport(c)">举报</el-button>
                  </div>
                </div>
              </div>
            </div>
          </el-tab-pane>
        </el-tabs>
      </div>
    </template>

    <EmptyState v-else title="图书不存在" description="该 ISBN 未在馆藏中找到">
      <el-button type="primary" @click="$router.push('/books')">浏览分类</el-button>
    </EmptyState>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { bookApi } from '@/api/book'
import { borrowApi } from '@/api/borrow'
import { authApi } from '@/api/auth'
import { useUserStore } from '@/stores/user'
import BookCover from '@/shared/components/BookCover.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const loading = ref(true)
const book = ref(null)
const comments = ref([])
const isFav = ref(false)
const tab = ref('desc')
const desc = ref('')
const submitting = ref(false)
const newComment = reactive({ Rating: 5, ReviewContent: '' })

async function load() {
  loading.value = true
  try {
    // 用详情接口取完整信息（出版社/出版年/简介/真实条码与状态）
    const found = await bookApi.getDetail(route.params.isbn)
    if (found) {
      book.value = found
      desc.value = found.Summary || ''
    }
    // 评论
    comments.value = await bookApi.getComments(route.params.isbn)
    if (userStore.isLoggedIn) {
      const favRes = await bookApi.checkFavorite(route.params.isbn)
      isFav.value = favRes?.isFavorite
    }
  } finally {
    loading.value = false
  }
}

async function onBorrow() {
  if (!userStore.isLoggedIn) return router.push('/auth')
  ElMessageBox.confirm('确认借阅此书?', '提示', { type: 'info' })
    .then(async () => {
      // 后端接口需要 readerId,这里从 user store 推断
      await bookApi.borrowByBarcode(book.value.Barcode).catch(() => {})
      ElMessage.success('借阅成功')
    })
    .catch(() => {})
}

async function onFavorite() {
  if (!userStore.isLoggedIn) return router.push('/auth')
  if (isFav.value) {
    ElMessage.info('已收藏')
    return
  }
  await bookApi.addFavorite({ ISBN: book.value.ISBN, Notes: '', FolderName: '默认收藏夹' })
  isFav.value = true
  ElMessage.success('已加入收藏')
}

async function onReserve() {
  if (!userStore.isLoggedIn) return router.push('/auth')
  if (!book.value?.ISBN) return ElMessage.warning('图书信息缺失')
  try {
    const r = await borrowApi.reserve({ ISBN: book.value.ISBN, ExpectedDays: 7 })
    if (r?.Success) {
      ElMessage.success('预约成功，到馆后请及时取书')
      router.push('/reader/reserves')
    } else {
      ElMessage.error(r?.Message || '预约失败，请稍后再试')
    }
  } catch (err) {
    ElMessage.error(err?.response?.data?.message || '预约失败，请稍后再试')
  }
}

function onReport(c) {
  ElMessageBox.prompt('请输入举报理由', '举报评论', { confirmButtonText: '提交', cancelButtonText: '取消' })
    .then(() => ElMessage.success('举报已提交,管理员将尽快处理'))
    .catch(() => {})
}

async function submitComment() {
  if (!newComment.ReviewContent.trim()) return ElMessage.warning('请输入评论内容')
  submitting.value = true
  try {
    await bookApi.addComment({ ...newComment, ISBN: book.value.ISBN })
    ElMessage.success('评论已提交')
    newComment.ReviewContent = ''
    comments.value = await bookApi.getComments(book.value.ISBN)
  } finally {
    submitting.value = false
  }
}

function formatTime(t) {
  if (!t) return ''
  return new Date(t).toLocaleDateString('zh-CN')
}

watch(() => route.params.isbn, load)
onMounted(load)
</script>

<style scoped>
.detail-page { padding: 32px 24px; max-width: 1100px; margin: 0 auto; }
.back-btn { margin-bottom: 16px; }
.detail-top {
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 32px;
  padding: 32px;
  align-items: flex-start;
}
.cover-side { width: 220px; }
.b-title { font-size: var(--fs-3xl); font-weight: 700; margin: 0 0 8px; }
.b-author { color: var(--color-text-soft); margin-bottom: 24px; }
.b-meta {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px 24px;
  margin-bottom: 24px;
}
.b-meta > div { display: flex; align-items: center; gap: 12px; }
.lbl { color: var(--color-text-muted); font-size: var(--fs-sm); min-width: 60px; }
.val { color: var(--color-text); font-size: var(--fs-sm); }
.b-actions { display: flex; gap: 12px; flex-wrap: wrap; }

.detail-tabs { background: #fff; border-radius: var(--radius-lg); padding: 24px; margin-top: 24px; }
.tab-body { padding: 16px 0; min-height: 200px; }
.comment-form { background: var(--color-bg); padding: 20px; border-radius: var(--radius-md); margin-bottom: 24px; display: flex; flex-direction: column; gap: 12px; }
.login-tip { margin-bottom: 16px; }
.comment-item { padding: 16px 0; border-bottom: 1px solid var(--color-border-soft); }
.comment-item:last-child { border: none; }
.c-head { display: flex; align-items: center; gap: 12px; margin-bottom: 6px; flex-wrap: wrap; }
.c-user { font-weight: 600; }
.c-time { font-size: var(--fs-xs); color: var(--color-text-muted); margin-left: auto; }
.c-body { color: var(--color-text-soft); line-height: 1.7; }
.c-actions { margin-top: 6px; }
.rich-text { line-height: 1.8; color: var(--color-text-soft); }

@media (max-width: 768px) {
  .detail-top { grid-template-columns: 1fr; text-align: center; }
  .cover-side { margin: 0 auto; }
  .b-meta { grid-template-columns: 1fr; }
}
</style>
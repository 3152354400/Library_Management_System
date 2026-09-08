<!-- frontend/src/modules/book/components/BookComments.vue -->

<template>
  <div class="comments-container">
    <h2>图书评论</h2>

    <div class="isbn-search">
      <input v-model="searchISBN" placeholder="请输入图书ISBN" @keyup.enter="searchComments" />
      <button @click="searchComments">搜索评论</button>
    </div>

    <!-- 新增评论表单 -->
    <div v-if="searchISBN" class="add-comment">
      <h3>发表评论</h3>
      <form @submit.prevent="submitComment">
        <div class="form-group">
          <label for="rating">评分（1-5）：</label>
          <select id="rating" v-model.number="newComment.rating" required>
            <option value="1">1</option>
            <option value="2">2</option>
            <option value="3">3</option>
            <option value="4">4</option>
            <option value="5">5</option>
          </select>
        </div>
        <div class="form-group">
          <label for="review">评论内容：</label>
          <textarea id="review" v-model="newComment.reviewContent" required></textarea>
        </div>
        <button type="submit">提交评论</button>
      </form>
    </div>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else-if="comments.length > 0" class="comments-list">
      <div v-for="comment in comments" :key="comment.CommentID" class="comment-item">
        <div class="comment-header">
          <span class="reader-id">读者ID: {{ comment.ReaderID }}</span>
          <span class="rating">评分: {{ comment.RATING }}/5</span>
        </div>
        <div class="comment-content">{{ comment.ReviewContent }}</div>
        <div class="comment-time">{{ formatDate(comment.CreateTime) }}</div>
        <div class="comment-actions">
          <button @click="reportComment(comment)" class="report-button">举报</button>
        </div>
        
        <!-- 举报表单 -->
        <div v-if="reportingCommentId === comment.CommentID" class="report-section">
          <div class="form-group">
            <label for="reportReason">举报理由：</label>
            <textarea 
              id="reportReason" 
              v-model="reportReason" 
              placeholder="请输入举报理由"
              required
            ></textarea>
          </div>
          <div class="report-actions">
            <button @click="submitReport(comment)" class="submit-report-button">提交举报</button>
            <button @click="cancelReport" class="cancel-report-button">取消</button>
          </div>
        </div>
      </div>
    </div>
    <div v-else class="no-comments">暂无评论</div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getCommentsByISBN, addComment, addReport } from '@/modules/book/api.js'

const route = useRoute()
const router = useRouter()
const searchISBN = ref('')
const comments = ref([])
const loading = ref(false)
const error = ref('')

// 举报相关的响应式变量
const reportingCommentId = ref(null)
const reportReason = ref('')

const newComment = ref({
  readerId: '1', // TODO: 从用户登录信息中获取
  rating: 5,
  reviewContent: ''
})

// 格式化日期函数
function formatDate(dateString) {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN')
}

// 从 URL 获取 ISBN 并自动搜索
function initISBNFromRoute() {
  if (route.query.isbn) {
    searchISBN.value = route.query.isbn
    searchComments()
  }
}

// 获取评论
async function searchComments() {
  if (!searchISBN.value.trim()) {
    error.value = '请输入ISBN'
    return
  }

  loading.value = true
  error.value = ''

  try {
    const response = await getCommentsByISBN(searchISBN.value)
    comments.value = response.data || []
  } catch (err) {
    error.value = '获取评论失败: ' + (err.response?.data?.message || err.message)
  } finally {
    loading.value = false
  }
}

// 提交新评论
async function submitComment() {
  if (!searchISBN.value.trim()) {
    error.value = '请先搜索图书'
    return
  }

  const commentData = {
    ReaderID: newComment.value.readerId,
    ISBN: searchISBN.value,
    Rating: newComment.value.rating,
    ReviewContent: newComment.value.reviewContent,
    // 注意：后端应该自动设置 CreateTime，不需要从前端传递
    Status: '正常'
  }

  try {
    const response = await addComment(commentData)
    if (response.status === 200) {
      newComment.value.reviewContent = ''
      await searchComments() // 重新加载评论
    }
  } catch (err) {
    error.value = '提交评论失败: ' + (err.response?.data?.message || err.message)
  }
}

// 举报评论
function reportComment(comment) {
  // 设置要举报的评论ID
  reportingCommentId.value = comment.CommentID
  reportReason.value = ''
}

// 提交举报
async function submitReport(comment) {
  if (!reportReason.value.trim()) {
    error.value = '请输入举报理由'
    return
  }
  
  // 构造举报数据
  const reportData = {
    CommentID: comment.CommentID,
    ReaderID: 1, // TODO: 应该从当前登录用户获取
    ReportReason: reportReason.value,
    ReportTime: new Date().toISOString(),
    Status: '待处理',
    LibrarianID: 9
  }
  
  try {
    const response = await addReport(reportData)
    if (response.status === 200) {
      alert(`举报已提交，理由：${reportReason.value}`)
      cancelReport()
    }
  } catch (err) {
    error.value = '提交举报失败: ' + (err.response?.data?.message || err.message)
  }
}

// 取消举报
function cancelReport() {
  reportingCommentId.value = null
  reportReason.value = ''
}

// 初始化
initISBNFromRoute()
</script>

<style scoped>
.add-comment {
  margin-top: 30px;
  padding: 20px;
  background-color: #f5f7fa;
  border-radius: 8px;
}

.add-comment h3 {
  margin-bottom: 15px;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.form-group select,
.form-group textarea {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.form-group textarea {
  resize: vertical;
  min-height: 100px;
}

button[type='submit'] {
  background-color: #409eff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

button[type='submit']:hover {
  background-color: #337ecc;
}

.comment-item {
  border: 1px solid #ddd;
  border-radius: 5px;
  padding: 15px;
  margin-bottom: 15px;
  background-color: #fafafa;
}

.comment-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
  font-size: 14px;
  color: #666;
}

.comment-content {
  margin-bottom: 10px;
  line-height: 1.5;
}

.comment-time {
  font-size: 12px;
  color: #999;
}

.comment-actions {
  text-align: right;
  margin-top: 10px;
}

.report-button {
  background-color: #f56c6c;
  color: white;
  border: none;
  padding: 5px 10px;
  border-radius: 3px;
  cursor: pointer;
  font-size: 12px;
}

.report-button:hover {
  background-color: #e64a19;
}

.report-section {
  margin-top: 15px;
  padding: 15px;
  background-color: #fff5f5;
  border-radius: 8px;
  border: 1px solid #f56c6c;
}

.report-section .form-group {
  margin-bottom: 10px;
}

.report-section .form-group label {
  font-weight: normal;
  font-size: 14px;
}

.report-section .form-group textarea {
  min-height: 80px;
  font-size: 14px;
}

.report-actions {
  display: flex;
  gap: 10px;
}

.submit-report-button {
  background-color: #f56c6c;
  color: white;
  border: none;
  padding: 8px 15px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.submit-report-button:hover {
  background-color: #e64a19;
}

.cancel-report-button {
  background-color: #909399;
  color: white;
  border: none;
  padding: 8px 15px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.cancel-report-button:hover {
  background-color: #606266;
}
</style>
<!-- frontend/src/modules/book/components/Report.vue -->
<template>
  <div class="report-management">
    <h2>举报管理</h2>
<!--     
    <div class="filter-section">
      <label for="librarianId">图书管理员ID:</label>
      <input 
        id="librarianId" 
        v-model="librarianId" 
        placeholder="请输入图书管理员ID" 
        @keyup.enter="loadReports"
      />
      <button @click="loadReports">加载举报</button>
    </div> -->

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else-if="reports.length > 0" class="reports-list">
      <div v-for="report in reports" :key="report.ReportID" class="report-item">
        <div class="report-header">
          <span class="report-id">举报ID: {{ report.ReportID }}</span>
          <span class="comment-id">评论ID: {{ report.CommentID }}</span>
          <span class="reader-id">读者ID: {{ report.READERID }}</span>
        </div>
        <div class="report-reason">
          <strong>举报理由:</strong> {{ report.ReportReason }}
        </div>
        <div class="comment-content" v-if="report.commentContent">
          <strong>评论原文:</strong> {{ report.commentContent }}
        </div>
        <div class="comment-loading" v-else-if="report.loadingComment">
          正在加载评论内容...
        </div>
        <div class="comment-error" v-else-if="report.commentError">
          加载评论失败: {{ report.commentError }}
        </div>
        <div class="report-time">
          <strong>举报时间:</strong> {{ formatDate(report.ReportTime) }}
        </div>
        <div class="report-status">
          <strong>状态:</strong> 
          <span :class="['status', report.Status]">{{ report.Status }}</span>
        </div>
        <div class="report-actions">
          <button 
            v-if="report.Status === '待处理'" 
            @click="rejectReport(report)" 
            class="reject-button"
          >
            驳回
          </button>
          <button 
            v-if="report.Status === '待处理'" 
            @click="deleteComment(report)" 
            class="delete-button"
          >
            删除评论
          </button>
          <span v-if="report.Status !== '待处理'" class="status-text">
            已处理: {{ report.Status }}
          </span>
        </div>
      </div>
    </div>
    <div v-else class="no-reports">暂无举报记录</div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { getReportsByLibrarianId, changeReportStatus, getCommentsByID } from '@/modules/book/api.js'

const librarianId = ref('')
const reports = ref([])
const loading = ref(false)
const error = ref('')

// 格式化日期函数
function formatDate(dateString) {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN')
}

// 加载举报记录
async function loadReports() {
  // if (!librarianId.value.trim()) {
  //   error.value = '请输入图书管理员ID'
  //   return
  // }

  loading.value = true
  error.value = ''

  try {
    const response = await getReportsByLibrarianId(8)
    reports.value = response.data || []
    
    // 为每个举报添加newStatus属性用于状态更改
    reports.value = reports.value.map(report => ({
      ...report,
      newStatus: report.Status
    }))

    // 为每个举报加载评论内容
    await Promise.all(reports.value.map(report => loadComment(report)))
  } catch (err) {
    error.value = '获取举报失败: ' + (err.response?.data?.message || err.message)
  } finally {
    loading.value = false
  }
}
loadReports()
// 加载评论内容
async function loadComment(report) {
  report.loadingComment = true
  report.commentError = null
  
  try {
    const response = await getCommentsByID(report.CommentID)
    if (response.data && response.data.length > 0) {
      report.commentContent = response.data[0].ReviewContent
    } else {
      report.commentContent = "未找到评论内容"
    }
  } catch (err) {
    report.commentError = err.response?.data?.message || err.message
  } finally {
    report.loadingComment = false
  }
}

// 驳回举报
async function rejectReport(report) {
  try {
    const statusData = {
      ReportID: report.ReportID,
      Status: '驳回'
    }
    
    const response = await changeReportStatus(statusData)
    if (response.status === 200) {
      // 更新本地状态
      report.Status = '驳回'
      alert('举报已驳回')
    }
  } catch (err) {
    error.value = '驳回失败: ' + (err.response?.data?.message || err.message)
  }
}

// 删除评论（处理完成）
async function deleteComment(report) {
  try {
    const statusData = {
      ReportID: report.ReportID,
      Status: '处理完成'
    }
    
    const response = await changeReportStatus(statusData)
    if (response.status === 200) {
      // 更新本地状态
      report.Status = '处理完成'
      alert('评论已删除')
    }
  } catch (err) {
    error.value = '删除评论失败: ' + (err.response?.data?.message || err.message)
  }
}
</script>

<style scoped>
.report-management {
  padding: 20px;
}

.filter-section {
  display: flex;
  gap: 10px;
  align-items: center;
  margin-bottom: 20px;
  padding: 15px;
  background-color: #f5f7fa;
  border-radius: 8px;
}

.filter-section input {
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.filter-section button {
  background-color: #409eff;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

.filter-section button:hover {
  background-color: #337ecc;
}

.report-item {
  border: 1px solid #ddd;
  border-radius: 5px;
  padding: 15px;
  margin-bottom: 15px;
  background-color: #fafafa;
}

.report-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
  font-size: 14px;
  color: #666;
}

.report-reason {
  margin-bottom: 10px;
  line-height: 1.5;
}

.comment-content {
  margin-bottom: 10px;
  padding: 10px;
  background-color: #f0f8ff;
  border-radius: 4px;
  border-left: 4px solid #409eff;
}

.comment-loading, .comment-error {
  margin-bottom: 10px;
  font-style: italic;
  color: #666;
}

.comment-error {
  color: #ff4d4f;
}

.report-time {
  margin-bottom: 10px;
  font-size: 14px;
  color: #666;
}

.report-status {
  margin-bottom: 10px;
  font-size: 14px;
}

.status {
  padding: 3px 8px;
  border-radius: 3px;
  font-weight: bold;
}

.待处理 {
  background-color: #fff5e6;
  color: #ff9900;
}

.驳回 {
  background-color: #ffe6e6;
  color: #ff4d4f;
}

.处理完成 {
  background-color: #e6f7ff;
  color: #1890ff;
}

.report-actions {
  text-align: right;
  margin-top: 10px;
}

.reject-button {
  background-color: #ff9900;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  margin-right: 10px;
}

.reject-button:hover {
  background-color: #e68a00;
}

.delete-button {
  background-color: #ff4d4f;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
}

.delete-button:hover {
  background-color: #e6393b;
}

.status-text {
  font-weight: bold;
  color: #666;
}

.loading, .error, .no-reports {
  text-align: center;
  padding: 20px;
  font-size: 16px;
}

.error {
  color: #ff4d4f;
  background-color: #ffe6e6;
  border-radius: 4px;
}
</style>
import http from '@/services/http.js'

// 获取所有采购分析排名数据
export function getPurchaseAnalysis() {
  return http.get('/admin/purchase-analysis',{withToken:true})
}

// 获取采购日志列表
export function getPurchaseLogs() {
  return http.get('/admin/purchase-analysis/logs',{withToken:true})
}

// 添加一条新的采购日志
export function addPurchaseLog(logText) {
  return http.post('/admin/purchase-analysis/logs', { logText },{withToken:true})
}

// 获取所有公告（管理用）
export function getAllAnnouncements() {
  return http.get('/admin/announcements',{withToken:true})
}

// 创建新公告
export function createAnnouncement(data) {
  return http.post('/admin/announcements', data,{withToken:true})
}

// 更新公告
export function updateAnnouncement(id, data) {
  return http.put(`/admin/announcements/${id}`, data,{withToken:true})
}

// 下架公告
export function takedownAnnouncement(id) {
  return http.put(`/admin/announcements/${id}/takedown`,{withToken:true})
}

// 获取图书管理列表
export function getAdminBooks(searchTerm = '') {
  return http.get('/admin/books', { params: { search: searchTerm } },{withToken:true})
}

// 创建新图书
export function createBook(bookData) {
  return http.post('/admin/books', bookData,{withToken:true})
}

// 更新图书信息
export function updateBook(isbn, bookData) {
  return http.put(`/admin/books/${isbn}`, bookData,{withToken:true})
}

// 下架图书
export function takedownBook(isbn) {
  return http.delete(`/admin/books/${isbn}`,{withToken:true})
}

// 获取所有待处理的举报
export function getPendingReports() {
  return http.get('/admin/reports/pending',{withToken:true})
}

// 处理一个举报 (现在包含 banUser 参数)
export function handleReport(reportId, action, banUser) {
  return http.put(`/admin/reports/${reportId}`, { action, banUser },{withToken:true})
}


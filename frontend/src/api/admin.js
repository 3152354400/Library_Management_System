/**
 * api/admin.js —— 管理员端通用接口
 */
import http from '@/services/http'

export const adminApi = {
  // 仪表盘
  dashboard: () => http.get('/admin/dashboard'),

  // 图书管理
  books: (search) => http.get('/admin/books', { params: { search }, mock: 'admin.dashboard' }),
  createBook: (data) => http.post('/admin/books', data),
  updateBook: (isbn, data) => http.put(`/admin/books/${isbn}`, data),
  takedownBook: (isbn) => http.delete(`/admin/books/${isbn}`),
  addBookCopies: (data) => http.post('/admin/books/copies', data),
  importBooks: (items) => http.post('/admin/books/import', items),

  // 读者管理
  readers: () => http.get('/reader/list', { mock: 'reader.list' }),
  addReader: (data) => http.post('/reader', data),
  updateReader: (data) => http.put('/reader', data),
  deleteReader: (id) => http.delete(`/reader/${id}`),
  resetReaderPassword: (username, newPwd) => http.put(`/reader/resetPwd?userName=${username}&NewPwd=${newPwd}`),

  // 分类管理
  addCategory: (data) => http.post('/Category', data),
  updateCategory: (data) => http.put('/Category', data),
  deleteCategory: (id, operatorId) => http.delete(`/Category/${id}?operatorId=${operatorId}`),

  // 举报处理
  pendingReports: () => http.get('/admin/reports/pending', { mock: 'report.pending' }),
  handleReport: (id, data) => http.put(`/admin/reports/${id}`, data, { mock: 'report.handle' }),

  // 丢书上报
  bookLossList: (status) => http.get('/admin/book-loss/list', { params: { status }, mock: 'bookLoss.list' }),
  confirmBookLoss: (id, data) => http.put(`/admin/book-loss/${id}/confirm`, data),
  compensateBookLoss: (id) => http.put(`/admin/book-loss/${id}/compensate`),

  // 采购分析
  purchaseAnalysis: () => http.get('/admin/purchase-analysis', { mock: 'purchaseAnalysis.summary' }),
  purchaseLogs: () => http.get('/admin/purchase-analysis/logs', { mock: 'purchaseAnalysis.logs' }),
  addPurchaseLog: (data) => http.post('/admin/purchase-analysis/logs', data)
}
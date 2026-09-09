/**
 * api/fine.js —— 罚款
 */
import http from '@/services/http'

export const fineApi = {
  // 读者端
  myList: () => http.get('/reader/extension/fines', { mock: 'fine.myList' }),
  myPaged: (params) => http.get('/reader/extension/fines/paged', { params, mock: 'fine.myList' }),
  mySummary: () => http.get('/reader/extension/fines/summary', { mock: 'fine.mySummary' }),
  // 管理端
  pending: () => http.get('/admin/extension/fines/pending', { mock: 'fine.pendingAll' }),
  pay: (data) => http.post('/admin/extension/fines/pay', data, { mock: 'fine.pay' }),
  waive: (data) => http.post('/admin/extension/fines/waive', data, { mock: 'fine.waive' })
}
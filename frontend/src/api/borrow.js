/**
 * api/borrow.js —— 借阅
 */
import http from '@/services/http'

export const borrowApi = {
  myPaged: (params) => http.get('/borrowing/reader/paged', { params, mock: 'borrowing.myPaged' }),
  all: () => http.get('/borrowing', { mock: 'borrowing.all' }),
  borrow: (bookId) => http.post('/borrowing/borrow', null, { params: { bookId }, mock: 'borrowing.borrow' }),
  returnBook: (bookId) => http.post('/borrowing/return', null, { params: { bookId }, mock: 'borrowing.return' }),
  unreturnedCount: (readerId) => http.get(`/borrowing/unreturned-count/${readerId}`),
  overdueCount: (readerId) => http.get(`/borrowing/overdue-unreturned-count/${readerId}`),
  // 续借走扩展接口
  renew: (bookId) => http.post('/reader/extension/renew', { BookId: bookId }),
  // 图书预约(占位)
  reserve: (data) => http.post('/reader/extension/reserve', data, { mock: 'reserve.create' }),
  myReserves: () => http.get('/reader/extension/reserves', { mock: 'reserve.list' }),
  cancelReserve: (reserveId) => http.delete(`/reader/extension/reserve?reserveId=${reserveId}`, { mock: 'reserve.cancel' })
}
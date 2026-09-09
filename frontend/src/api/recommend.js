/**
 * api/recommend.js —— 荐购
 */
import http from '@/services/http'

export const recommendApi = {
  // 读者端
  myList: () => http.get('/reader/extension/recommends', { mock: 'recommend.myList' }),
  create: (data) => http.post('/reader/extension/recommend', data, { mock: 'recommend.create' }),
  // 管理端
  pending: () => http.get('/admin/extension/recommends/pending', { mock: 'recommend.pending' }),
  all: () => http.get('/admin/extension/recommends/all', { mock: 'recommend.all' }),
  handle: (data) => http.post('/admin/extension/recommends/handle', data, { mock: 'recommend.handle' })
}
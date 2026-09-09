/**
 * api/announcement.js —— 公告
 */
import http from '@/services/http'

export const announcementApi = {
  public: () => http.get('/announcements/public', { mock: 'announcement.public' }),
  all: () => http.get('/admin/announcements', { mock: 'announcement.all' }),
  create: (data) => http.post('/admin/announcements', data, { mock: 'announcement.create' }),
  update: (id, data) => http.put(`/admin/announcements/${id}`, data),
  takedown: (id) => http.put(`/admin/announcements/${id}/takedown`, null, { mock: 'announcement.takedown' })
}
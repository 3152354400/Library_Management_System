/**
 * api/auth.js —— 认证相关
 */
import http from '@/services/http'

export const authApi = {
  login: (data) => http.post('/login', data, { mock: 'auth.login' }),
  register: (data) => http.post('/register', data),
  logout: () => http.post('/logout', null),
  myProfile: () => http.get('/reader/me/info', { mock: 'auth.profile' }),
  updateMyProfile: (data) => http.put('/reader/me/info', data),
  resetMyPassword: (oldPwd, newPwd) => http.put(`/reader/me/resetPwd?OldPwd=${encodeURIComponent(oldPwd)}&NewPwd=${encodeURIComponent(newPwd)}`),
  uploadAvatar: (file) => {
    const fd = new FormData()
    fd.append('file', file)
    return http.post('/reader/me/upload/avatar', fd)
  },
  myRecommendations: () => http.get('/reader/me/recommendations')
}
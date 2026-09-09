import http from '@/services/http'



// ——————————————————认证相关接口————————————————
export const login = (data) => http.post('/login', data)
export const logout = () => http.post('/logout',null,{withToken:true})
export const getCaptcha = () => http.get('/captcha')

export const register = (data) => http.post('/register', data)


// ——————————————————Reader相关接口————————————————
export const getReaders = (params) => http.get('/reader/list', { withToken:true,params })//params为可选参数，一般用于分页查询等
export const getReaderById = (id) => http.get(`/reader/${id}`,{withToken:true})
export const addReader = (data) => http.post('/reader', data,{withToken:true})
export const updateReader = (data) => http.put('/reader', data,{withToken:true})
export const updateReaderPartial = (data) => http.put('/reader/updatePartial', data,{withToken:true})
export const deleteReader = (id) => http.delete(`/reader/${id}`,{withToken:true})


export const resetPassword = () => http.put(`/reader/me/resetPwd`,{withToken:true})
export const reset = (username) =>
  http.put(`/reader/resetPwd?userName=${encodeURIComponent(username)}`, null, { withToken: true })

export const getMyProfile = () => http.get('/reader/me/info',{withToken:true})
export const getAvatar = (avatarUrl) => http.get(`/reader/me/avatar/${avatarUrl}`,{withToken:true})
export const uploadAvatar = (file) => {
  const formData = new FormData()
  formData.append('file', file)

  return http.post('/reader/me/upload/avatar', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    },
    withToken:true
  })
}

export const updateAvatar = (avatarUrl) =>
  http.put(`/reader/me/avatar?avatarUrl=${encodeURIComponent(avatarUrl)}`, null, { withToken: true })

export const updateMyProfile = (data) => http.put('/reader/me/info', data,{withToken:true})

export const getAllBorrowingRecords = (params) => http.get('/borrowing', { params , withToken:true});
export const getBorrowingRecordByReaderId = (id) => http.get(`/borrowing/reader/`,{withToken:true});
export const addBorrowingRecord = (data) => http.post('/borrowing', data,{withToken:true});
export const updateBorrowingRecord = (data) => http.put('/borrowing', data,{withToken:true});
export const deleteBorrowingRecord = (id) => http.delete(`/borrowing/${id}`,{withToken:true});
export const returnBook = (id) => http.put(`/borrowing/${id}/return`,{withToken:true});
export const renewBorrowing = (id) => http.put(`/borrowing/${id}/renew`,{withToken:true});
export const getUnreturnedCount = (id) => http.get('/borrowing/unreturned-count/placeholder', { withToken: true });
export const getOverdueUnreturnedCount = (id) => http.get('/borrowing/overdue-unreturned-count/placeholder', { withToken: true });
export const getAllOverdueUnreturnedCountByReader = (id) => http.get('/borrowing/all-overdue-unreturned-count/placeholder', { withToken: true });

// ——————————————————Librarian相关接口————————————————

export const getLibrarians = (params) => http.get('/librarian/list', { withToken:true,params })//params为可选参数，一般用于分页查询等
export const getLibrarianById = (id) => http.get(`/librarian/${id}`,{withToken:true})
export const addLibrarian = (data) => http.post('/librarian', data,{withToken:true})
export const updateLibrarian = (data) => http.put('/librarian', data,{withToken:true})
export const deleteLibrarian = (id) => http.delete(`/librarian/${id}`,{withToken:true})

export const getLibrarianProfile = () => http.get('/librarian/info',{withToken:true})


export function getSeatLayout(buildingId, floor) {
  return http.get('/space/seats', { params: { buildingId, floor } });
}

export function createSeatReservation(data) {
  return http.post('/space/reservations/seat', data);
}

export function getMyReservations() {
  return http.get('/space/my-reservations');
}

export function cancelReservation(id) {
    return http.put(`/space/reservations/${id}/cancel`);
}
// ——————————————————书籍个性化推荐接口相关接口————————————————
export const getRecommendations = () => http.get('/reader/me/recommendations',{withToken:true})

// ——————————————————图书续借接口————————————————
export const renewBook = (data) => http.post('/reader/extension/renew', data, { withToken: true })

// ——————————————————罚款管理接口————————————————
export const getMyFines = () => http.get('/reader/extension/fines', { withToken: true })
export const getMyFineSummary = () => http.get('/reader/extension/fines/summary', { withToken: true })

// ——————————————————图书预约接口————————————————
export const reserveBook = (data) => http.post('/reader/extension/reserve', data, { withToken: true })
// 取消预约（通过URL参数传递reserveId）
export const cancelReserve = (reserveId) => http.delete(`/reader/extension/reserve?reserveId=${reserveId}`, { withToken: true })
export const getMyBookReserves = () => http.get('/reader/extension/reserves', { withToken: true })

// ——————————————————图书荐购接口————————————————
export const recommendPurchase = (data) => http.post('/reader/extension/recommend', data, { withToken: true })
export const getMyRecommends = () => http.get('/reader/extension/recommends', { withToken: true })

// ——————————————————消息通知接口————————————————
export const getNotifications = (params) => http.get('/reader/extension/notifications', { params, withToken: true })
export const getUnreadCount = () => http.get('/reader/extension/notifications/unread/count', { withToken: true })
export const markNotificationRead = (notificationId) => http.put(`/reader/extension/notifications/${notificationId}/read`, null, { withToken: true })
export const markAllNotificationsRead = () => http.put('/reader/extension/notifications/read/all', null, { withToken: true })

// ——————————————————图书收藏接口————————————————
export const getMyFavorites = (folderName) => http.get('/reader/favorite/list', { params: folderName ? { folderName } : {}, withToken: true })
export const checkFavorite = (isbn) => http.get('/reader/favorite/check', { params: { isbn }, withToken: true })
export const addFavorite = (data) => http.post('/reader/favorite', data, { withToken: true })
export const removeFavorite = (favoriteId) => http.delete(`/reader/favorite/${favoriteId}`, { withToken: true })
export const removeFavoriteByISBN = (isbn) => http.delete(`/reader/favorite/by-isbn`, { params: { isbn }, withToken: true })
export const updateFavoriteNotes = (favoriteId, data) => http.put(`/reader/favorite/${favoriteId}/notes`, data, { withToken: true })
export const getFavoriteFolders = () => http.get('/reader/favorite/folders', { withToken: true })

// ——————————————————分页接口————————————————
export const getBorrowingRecordPaged = (params) => http.get('/borrowing/reader/paged', { params, withToken: true })
export const getMyFinesPaged = (params) => http.get('/reader/extension/fines/paged', { params, withToken: true })

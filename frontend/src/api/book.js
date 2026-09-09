/**
 * api/book.js —— 图书、分类、书架、书单、评论、收藏
 */
import http from '@/services/http'

export const bookApi = {
  // 检索/详情
  search: (keyword) => http.get('/Book/search', { params: { keyword }, mock: 'book.search' }),
  getById: (id) => http.get(`/Book/${id}`, { mock: 'book.byId' }),
  getByBarcode: (barcode) => http.get(`/Book/by-barcode/${barcode}`),
  getDetail: (isbn) => http.get(`/Book/detail/${isbn}`),

  // 状态流转(管理员)
  borrowById: (id) => http.patch(`/Book/${id}/borrow`),
  offShelfById: (id) => http.patch(`/Book/${id}/off-shelf`),
  returnById: (id) => http.patch(`/Book/${id}/return`),
  onShelfById: (id) => http.patch(`/Book/${id}/on-shelf`),
  borrowByBarcode: (bc) => http.patch(`/Book/by-barcode/${bc}/borrow`, { mock: 'borrowing.borrow' }),
  returnByBarcode: (bc) => http.patch(`/Book/by-barcode/${bc}/return`, { mock: 'borrowing.return' }),

  // 分类树
  getCategoryTree: () => http.get('/Category/tree', { mock: 'category.tree' }),

  // 图书-分类关联
  bindCategories: (data) => http.post('/BookCategory/bind', data),
  getLeafCategories: () => http.get('/BookCategory/leaf-categories'),
  getBookCategories: (isbn) => http.get(`/BookCategory/book/${isbn}`),

  // 书架
  searchBooksOnShelf: (keyword) => http.get('/bookshelf/search_book_which_shelf', { params: { keyword } }),
  searchShelves: (keyword) => http.get('/bookshelf/search_bookshelf', { params: { keyword } }),
  addShelf: (data) => http.post('/bookshelf/add_bookshelf', data),
  deleteShelf: (id) => http.delete(`/bookshelf/delete/${id}`),
  shelfBooks: (id) => http.get(`/bookshelf/shelf-books/${id}`),

  // 评论
  getComments: (isbn) => http.get('/comment/search', { params: { ISBN: isbn }, mock: 'comment.byISBN' }),
  addComment: (data) => http.post('/comment/add', data, { mock: 'comment.add' }),

  // 书单
  createBooklist: (data) => http.post('/book/booklists', data, { mock: 'booklist.create' }),
  deleteBooklist: (id) => http.delete(`/book/booklists/${id}`),
  getBooklist: (id) => http.get(`/book/booklists/${id}`),
  myBooklists: () => http.get('/book/booklists/reader/0', { mock: 'booklist.byReader' }),
  addBookToBooklist: (booklistId, data) => http.post(`/book/booklists/${booklistId}/books`, data),
  removeBookFromBooklist: (booklistId, isbn) => http.delete(`/book/booklists/${booklistId}/books/${isbn}`),
  collectBooklist: (id, data) => http.post(`/book/booklists/${id}/collect`, data),
  cancelCollect: (id) => http.delete(`/book/booklists/${id}/collect`),
  recommendBooklists: (id, limit) => http.get(`/book/booklists/${id}/recommend`, { params: { limit } }),
  updateBooklistName: (id, data) => http.put(`/book/booklists/${id}/name`, data),
  updateCollectNotes: (id, data) => http.put(`/book/booklists/${id}/collect/notes`, data),
  updateBooklistIntro: (id, data) => http.put(`/book/booklists/${id}/intro`, data),

  // 收藏(图书)
  favorites: (folderName) => http.get('/reader/favorite/list', { params: folderName ? { folderName } : {}, mock: 'favorite.list' }),
  favoriteFolders: () => http.get('/reader/favorite/folders', { mock: 'favorite.folders' }),
  checkFavorite: (isbn) => http.get('/reader/favorite/check', { params: { isbn }, mock: 'favorite.check' }),
  addFavorite: (data) => http.post('/reader/favorite', data, { mock: 'favorite.add' }),
  removeFavorite: (id) => http.delete(`/reader/favorite/${id}`),
  removeFavoriteByISBN: (isbn) => http.delete('/reader/favorite/by-isbn', { params: { isbn } })
}

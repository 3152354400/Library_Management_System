import http from '@/services/http.js'

// ---------- 图书相关 ----------
export function getBooks(keyword) {
  return http.get('/book/search', {
    params: { keyword }
  })
}


export function getBookByBarcode(barcode) {
  return http.get(`/Book/by-barcode/${encodeURIComponent(barcode)}`, { withToken: true })
}

// ---------- 评论 ----------
export function getCommentsByISBN(ISBN) {
  return http.get('/comment/search', {
    params: { ISBN },
    withToken: true
  })
}

export function getCommentsByID(id) {
  return http.get('/comment/search-id', {
    params: { id }
  })
}
export function addComment(commentData) {
  return http.post('/comment/add', commentData, { withToken: true })
}


export function getReportsByReaderId(readerId) {
  return http.get(`/report/by-reader/${readerId}`)
}

export function getReportsByLibrarianId(librarianId) {
  return http.get(`/report/by-librarian/${librarianId}`, { withToken: true })
}

export function addReport(reportData) {
  return http.post('/admin/reports/add', reportData, { withToken: true })
}



export function changeReportStatus(statusData) {
  return http.post('/report/change-status', statusData, { withToken: true })
}

// ---------- 分类 ----------
export function getCategoryTree() {

  return http.get('/Category/tree', {
    withToken: true // 确保每个请求都带上 Token
  });
}

export function addCategory(data) {
  return http.post('/Category', data, { withToken: true })
}

export function updateCategory(data) {
  return http.put('/Category', data, { withToken: true })
}

export function deleteCategory(id, operatorId) {
  return http.delete(`/Category/${id}?operatorId=${operatorId}`, { withToken: true })
}

// ---------- 图书-分类 关联 ----------
// 绑定图书到多个分类
export function bindBookToCategories(data) {
  // data: { isbn: string, categoryIds: string[], relationNote?: string|null }
  return http.post('/BookCategory/bind', data, { withToken: true })
}

// 获取某本书已绑定的分类
export function getBookCategories(isbn) {
  return http.get(`/BookCategory/book/${encodeURIComponent(isbn)}`, { withToken: true })
}

// 获取某分类下的图书
export function getCategoryBooks(categoryId) {
  return http.get(`/BookCategory/category/${encodeURIComponent(categoryId)}`, { withToken: true })
}

// 获取所有叶子分类（用于绑定选择）
export function getLeafCategories() {
  return http.get('/BookCategory/leaf-categories', { withToken: true })
}

// 获取图书-分类关联统计
export function getBookCategoryStats() {
  return http.get('/BookCategory/stats', { withToken: true })
}

// 移除某书与某分类的关联
export function removeBookCategory(isbn, categoryId) {
  return http.delete(`/BookCategory/${encodeURIComponent(isbn)}/${encodeURIComponent(categoryId)}`, { withToken: true })
}

// ---------- 书架 ----------
export function getBooksBookShelf(keyword) {
  return http.get('/bookshelf/search_book_which_shelf', {
    params: { keyword },
    withToken: true
  })
}

export function getShelf(keyword) {
  return http.get('/bookshelf/search_bookshelf', {
    params: { keyword },
    withToken: true
  })
}

export function addShelf(buildingid, shelfcode, floor, zone) {
  return http.post('/bookshelf/add_bookshelf', {
    buildingid: Number(buildingid),
    shelfcode: shelfcode,
    floor: Number(floor),
    zone: zone
  }, { withToken: true })
}

export function deleteShelf(shelfId) {
  return http.delete(`/bookshelf/delete/${shelfId}`, { withToken: true });
}

export function checkShelfHasBooks(shelfId) {
  return http.get(`/bookshelf/has-books/${shelfId}`, { withToken: true })
}

export function checkShelfExists(buildingId, shelfCode, floor, zone) {
  return http.get('/bookshelf/check-shelf-exists', {
    params: { buildingId, shelfCode, floor, zone },
    withToken: true
  })
}

export function returnBook(bookId, shelfId) {
  return http.post('/bookshelf/return-book', {
    bookId: Number(bookId),
    shelfId: Number(shelfId)
  }, { withToken: true });
}

export function findShelfId(buildingId, shelfCode, floor, zone) {
  return http.get('/bookshelf/find-shelf-id', {
    params: { buildingId, shelfCode, floor, zone },
    withToken: true
  })
}

export function borrowBook(bookId) {
  return http.post('/bookshelf/borrow-book', { bookId }, { withToken: true })
}

// ---------- 查询单册 ----------
export function getBookById(bookId) {
  return http.get(`/Book/${bookId}`)
}

// export function getBookByBarcode(barcode) {
//   return http.get(`/Book/by-barcode/${encodeURIComponent(barcode)}`)
// }

// ---------- 状态流转：按 BookID ----------
export function borrowBookById(bookId) {
  return http.patch(`/Book/${bookId}/borrow`, {}, { withToken: true })
}

export function offShelfBookById(bookId) {
  return http.patch(`/Book/${bookId}/off-shelf`, {}, { withToken: true })
}

export function returnBookById(bookId) {
  return http.patch(`/Book/${bookId}/return`, {}, { withToken: true })
}

export function onShelfBookById(bookId) {
  return http.patch(`/Book/${bookId}/on-shelf`, {}, { withToken: true })
}

// ---------- 状态流转：按条码 ----------
export function borrowBookByBarcode(barcode) {
  return http.patch(`/Book/by-barcode/${encodeURIComponent(barcode)}/borrow`, {}, { withToken: true })
}

export function offShelfBookByBarcode(barcode) {
  return http.patch(`/Book/by-barcode/${encodeURIComponent(barcode)}/off-shelf`, {}, { withToken: true })
}

export function returnBookByBarcode(barcode) {
  return http.patch(`/Book/by-barcode/${encodeURIComponent(barcode)}/return`, {}, { withToken: true })
}

export function onShelfBookByBarcode(barcode) {
  return http.patch(`/Book/by-barcode/${encodeURIComponent(barcode)}/on-shelf`)
}

//创建书单
export function createBooklist(data) {
  return http.post('/book/booklists', data, { withToken: true })
}

//删除书单
export function deleteBooklist(booklistId) {
  return http.delete(`/book/booklists/${booklistId}`, { withToken: true })
}

//获取书单详情
export function getBooklistDetails(booklistId) {
  return http.get(`/book/booklists/${booklistId}`, { withToken: true })
}

//推荐书单
export function recommendBooklists(booklistId, limit = 10) {
  return http.get(`/book/booklists/${booklistId}/recommend`, {
    params: { limit }, withToken: true
  })
}

//添加图书到书单
export function addBookToBooklist(booklistId, data) {
  return http.post(`/book/booklists/${booklistId}/books`, data, { withToken: true })
}

//从书单移除图书
export function removeBookFromBooklist(booklistId, isbn) {
  return http.delete(`/book/booklists/${booklistId}/books/${isbn}`, { withToken: true })
}

//收藏书单
export function collectBooklist(booklistId, data) {
  return http.post(`/book/booklists/${booklistId}/collect`, data, { withToken: true })
}

//取消收藏书单
export function cancelCollectBooklist(booklistId) {
  return http.delete(`/book/booklists/${booklistId}/collect`, { withToken: true })
}

//更新收藏备注
export function updateCollectNotes(booklistId, data) {
  return http.put(`/book/booklists/${booklistId}/collect/notes`, data, { withToken: true })
}

//查询某个用户的书单（创建 + 收藏）
export function searchBooklistsByReader(readerId) {
  return http.get(`/book/booklists/reader/${readerId}`, { withToken: true })
}

//修改书单名称
export function updateBooklistName(booklistId, data) {
  return http.put(`/book/booklists/${booklistId}/name`, data, { withToken: true })
}

//修改书单简介
export function updateBooklistIntro(booklistId, data) {
  return http.put(`/book/booklists/${booklistId}/intro`, data, { withToken: true })
}

// 获取书架上的书籍
export function GetShelfBooks(shelfId) {
  return http.get(`/bookshelf/shelf-books/${shelfId}`, { withToken: true });
}

export function addBookCopies(data) {
  // data should be { ISBN, NumberOfCopies, ShelfID }
  return http.post('/admin/books/copies', data)
}
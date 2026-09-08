import BookCommentsPage from '@/modules/book/pages/BookCommentsPage.vue'

export default [
  // 图书搜索结果页
  {
    path: '/search',
    name: 'SearchResult',
    component: () =>
      import(
        /* webpackChunkName: "book-search" */
        '@/modules/book/pages/BookSearchResultPage.vue'
      ),
    meta: { requiresAuth: false, title: '图书搜索' }
  },

  // 图书分类浏览
  {
    path: '/books',
    name: 'BookCategories',
    component: () => import('@/modules/book/pages/CategoryDisplayPage.vue'),
    meta: { 
      requiresAuth: false,
      title: '图书分类'
    }
  },

  // 分类下的图书
  {
    path: '/books/category/:categoryId',
    name: 'CategoryBooks',
    component: () => import('@/modules/book/pages/CategoryBooksPage.vue'),
    props: true,
    meta: { 
      requiresAuth: false,
      title: '分类图书'
    }
  },
  // 添加首页头部导航栏 "我的图书馆" 中 "我的书单主页面" 路由
  {
    path: '/user/booklist',
    name: 'Booklist',
    component: () =>
      import(
        /* webpackChunkName: "book-booklist" */
        '@/modules/book/pages/BooklistPage.vue'
      ),
    meta: {
      requiresAuth: true, title: '我的书单' }
  },

  {
    path: '/user/booklist/created/:id',
    name: 'BooklistCreated',
    component: () =>
      import(
        /* webpackChunkName: "book-booklist" */
        '@/modules/book/pages/BooklistDetailCreated.vue'
      ),
    meta: {
      requiresAuth: true, title: '我创建的书单' }
  },

  {
    path: '/user/booklist/collected/:id',
    name: 'BooklistCollected',
    component: () =>
      import(
        /* webpackChunkName: "book-booklist" */
        '@/modules/book/pages/BooklistDetailCollected.vue'
      ),
    meta: {
      requiresAuth: true, title: '我收藏的书单' }
  },

  {
    path: '/user/booklist/recommend',
    name: 'BooklistRecommend',
    component: () =>
      import(
        /* webpackChunkName: "book-booklist" */
        '@/modules/book/pages/BooklistRecommend.vue'
      ),
    meta: {
      requiresAuth: true, title: '推荐书单' }
  },
  
  // 评论页面路由
  {
    path: '/comments',
    name: 'BookComments',
    component: BookCommentsPage,
    meta: {
      requiresAuth: false,
      title: '图书评论'
    }
  },

  //用条码借还图书
  {
    path: '/books/circulation',
    name: 'BookCirculation',
    component: () => import('@/modules/book/pages/BookCirculationPage.vue'),
    meta: { title: '借还服务（条码）' }
  },
  
  // 举报评论页面
  {
    path: '/report-comment',
    name: 'ReportComment',
    component: () => import('@/modules/book/pages/ReportPage.vue'),
    meta: { 
      requiresAuth: false,
      title: '举报评论'
    }
  },

  // 图书分类管理
  {
    path: '/books/category-manage',
    name: 'BookCategoryManage',
    component: () => import('@/modules/book/pages/BookCategoryManagePage.vue'),
    meta: { 
      requiresAuth: true,
      title: '图书分类管理',
      role: 'librarian' // 仅管理员可见
    }
  },
  
  //搜索的实体书页面
  {
    path: '/booklocation',
    name: 'BookSearchLocation',
    component: () =>
    import(
        /* webpackChunkName: "book-search" */
        '@/modules/book/pages/BookLocationPage.vue'
    ),
    meta: {
      requiresAuth: true,     
      title: '实体书位置',
    }
  },
  {
    path: '/admin/category2',
    name: 'BookSearchLocation',
    component: () =>
    import(
        /* webpackChunkName: "book-search" */
        '@/modules/book/pages/BookCategoryManagePage.vue'
    ),
    meta: {       
      layout: 'Admin', 
      requiresAuth: true,
      requiresAdmin: true,
      title: '书籍分类' }

  },
  // ========== 已删除旧的 /bookshelf 路由 ==========
]
import AdminDashboardPage from '@/modules/admin/pages/AdminDashboardPage.vue'
import CategoryManagePage from '@/modules/book/pages/CategoryManagePage.vue'
import AnnouncementManagePage from '@/modules/admin/pages/AnnouncementManagePage.vue'
import BookManagePage from '@/modules/admin/pages/BookManagePage.vue'
import ShelfManagePage from '@/modules/admin/pages/ShelfManagePage.vue' // 确保这个新页面被引入
import ReportHandlingPage from '@/modules/admin/pages/ReportHandlingPage.vue'
import ReaderMagnagePage from '@/modules/admin/pages/ReaderMagnagePage.vue'
import BookBindCategoryPage from '@/modules/admin/pages/BookBindCategoryPage.vue'
import FineManagePage from '@/modules/admin/pages/FineManagePage.vue'
import RecommendPurchaseManagePage from '@/modules/admin/pages/RecommendPurchaseManagePage.vue'
import BookLossManagePage from '@/modules/admin/pages/BookLossManagePage.vue'
import BorrowingManagePage from '@/modules/admin/pages/BorrowingManagePage.vue'

export default [
  // 管理员仪表盘
  {
    path: '/admin/dashboard',
    name: 'AdminDashboard',
    component: AdminDashboardPage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '管理员仪表盘'
    }
  },

  // 分类管理
  {
    path: '/admin/category',
    name: 'CategoryManage',
    component: CategoryManagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '分类管理'
    }
  },


  // 公告发布
  {
    path: '/admin/announcements',
    name: 'AnnouncementManage',
    component: AnnouncementManagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '公告发布'
    }
  },

  // 图书管理
  {
    path: '/admin/books',
    name: 'BookManage',
    component: BookManagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '图书管理'
    }
  },

  // 书架管理 (新的、正确的路由)
  {
    path: '/admin/shelves',
    name: 'ShelfManage',
    component: ShelfManagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '书架管理'
    }
  },

  {
    path: '/admin/reports',
    name: 'ReportHandling',
    component: ReportHandlingPage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '举报处理' }
  },

  {
    path: '/admin/readers',
    name: 'ReaderManage',
    component: ReaderMagnagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '读者管理' }
  },

  {
    path: '/admin/book-category',
    name: 'BookBindCategory',
    component: BookBindCategoryPage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '书籍分类'
    }
  },

  // ———————————————— 新增路由 ————————————————
  {
    path: '/admin/fines',
    name: 'FineManage',
    component: FineManagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '罚款管理'
    }
  },

  {
    path: '/admin/recommends',
    name: 'RecommendPurchaseManage',
    component: RecommendPurchaseManagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '荐购处理'
    }
  },

  {
    path: '/admin/book-loss',
    name: 'BookLossManage',
    component: BookLossManagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '赔偿管理'
    }
  },

  {
    path: '/admin/borrowing',
    name: 'BorrowingManage',
    component: BorrowingManagePage,
    meta: {
      layout: 'Admin',
      requiresAuth: true,
      requiresAdmin: true,
      title: '借阅管理'
    }
  },
  // ———————————————— 路由添加完成 ————————————————
]

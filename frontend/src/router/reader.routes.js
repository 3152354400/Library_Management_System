import  BorrowingTest  from '@/modules/reader/components/BorrowingTest.vue';

export default [
  //导出我的图书馆页面路由
  {
    path: '/my/home/dashboard',
    name: 'my-home-dashboard',
    component: () => import('@/modules/reader/pages/DashBoardHome_Page.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/my/borrowingRecords',
    name: 'BorrowingRecords',
    component: () => import('@/modules/reader/pages/BorrowingRecords.vue'),
  },
 // 其他reader模块路由...
  {
    path: '/reader/BorrowingTest',
    name: 'BorrowingTest',
    component: BorrowingTest,
    meta: {
      title: '借阅记录管理',
      //requiresAuth: true // 如果需要登录验证
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

  // ———————————————— 新增页面路由 ————————————————
  // 罚款管理
  {
    path: '/my/fines',
    name: 'MyFines',
    component: () => import('@/modules/reader/pages/MyFinesPage.vue'),
    meta: { requiresAuth: true, title: '我的罚款' }
  },
  // 图书预约
  {
    path: '/my/book-reserves',
    name: 'MyBookReserves',
    component: () => import('@/modules/reader/pages/MyBookReservesPage.vue'),
    meta: { requiresAuth: true, title: '我的图书预约' }
  },
  // 图书荐购
  {
    path: '/my/recommends',
    name: 'MyRecommends',
    component: () => import('@/modules/reader/pages/MyRecommendPurchasePage.vue'),
    meta: { requiresAuth: true, title: '我的图书荐购' }
  },
  // 消息通知
  {
    path: '/my/notifications',
    name: 'MyNotifications',
    component: () => import('@/modules/reader/pages/MyNotificationsPage.vue'),
    meta: { requiresAuth: true, title: '我的消息' }
  },
  // 我的收藏
  {
    path: '/my/favorites',
    name: 'MyFavorites',
    component: () => import('@/modules/reader/pages/MyFavoritesPage.vue'),
    meta: { requiresAuth: true, title: '我的收藏' }
  },
  // ———————————————— 路由添加完成 ————————————————
];

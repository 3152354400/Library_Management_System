// router/index.js

import { createRouter, createWebHistory } from 'vue-router';
import readerRoutes from './reader.routes.js';
import {jwtDecode} from 'jwt-decode';
// import adminRoutes from './admin.routes.js';

import HomeView from '@/modules/home/pages/HomeView.vue'

import adminRoutes  from '@/router/admin.routes.js'
import bookRoutes   from '@/router/book.routes.js'
// import readerRoutes from '@/router/reader.routes.js'

import AboutPage from '@/shared/pages/AboutPage.vue'; // 1. 引入新页面

const routes = [
  { path: '/', name: 'HomeView', component: HomeView },
  ...adminRoutes,
  ...bookRoutes,
  {
    path: '/auth',
    name: 'AuthPage',
    component: () => import('@/shared/pages/AuthPage.vue')
  },

  {
    path: '/about',
    name: 'About',
    component: AboutPage, // <-- 必须是组件本身，而不是一个字符串
    meta: { title: '关于我们' }
  },
  ...readerRoutes,
  //...adminRoutes,
]

const router = createRouter({
  history: createWebHistory(),
  routes
});

//判断token是否存在且未过有效期
function isLoggedIn() {
  const token = localStorage.getItem('token');
  if (!token) return false;

    const { exp } = jwtDecode(token);
    const now = Math.floor(Date.now() / 1000);
    return exp && exp > now;
}

router.beforeEach((to, from, next) => {
  if (to.meta.requiresAuth && !isLoggedIn()) {
    next({
      path: '/auth',
      query: { redirect: to.fullPath }
    });
  } else {
    next();
  }
});

export default router

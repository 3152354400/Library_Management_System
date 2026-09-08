<template>
  <header :class="['navbar', { scrolled: isScrolled }]">
    <div class="navbar-container">
      <router-link to="/" class="logo">Library System</router-link>

      <nav class="nav-links">
        <router-link to="/" class="nav-item">首页</router-link>
        <router-link to="/books" class="nav-item">图书资源</router-link>

        <div class="nav-item dropdown">
          <span>空间服务</span> <!-- 修改名称 -->
          <div class="dropdown-menu">
            <router-link to="/space/seats">座位预约</router-link> <!-- 修改链接 -->
          </div>
        </div>

        <router-link to="/my/home/dashboard" class="nav-item">读者控制台</router-link>



        <router-link to="/about" class="nav-item">关于我们</router-link>
      </nav>

      <!-- 登录按钮 / 欢迎语 -->
      <div class="auth-btn" v-if="!isLoggedIn">
        <router-link to="/auth" class="login">登录</router-link>
      </div>
      <div class="nav-item" v-else>
        <span class="welcome-text">Hi，{{ displayName }}~</span>
        <button class="logout-inline" @click="handleLogout">退出</button>
      </div>


    </div>
  </header>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { getMyProfile, logout } from '@/modules/reader/api.js'
import router from '@/router/index.js'
import { useRoute } from 'vue-router'

const isScrolled = ref(false)
let   scrollEl     = null   // 实际滚动的容器（#app）

const isLoggedIn = ref(false)
const displayName = ref('')
const route = useRoute()


function handleScroll () {
  if (scrollEl) {
    isScrolled.value = scrollEl.scrollTop > 0
  }
}

onMounted(() => {
  // Vue 在挂载后，#app 已经存在于 DOM
  scrollEl = document.getElementById('app') || window
  // 只在 #app 上监听
  scrollEl.addEventListener('scroll', handleScroll, { passive: true })
  handleScroll()            // 初始化一次，保证刷新后状态正确
})

onUnmounted(() => {
  scrollEl && scrollEl.removeEventListener('scroll', handleScroll)
})

// === 登录状态检测：封装为函数，并在挂载与路由变化时调用 ===
async function loadProfile () {
  try {
    const res = await getMyProfile() // 要求拦截器自动带 token
    const u = res?.data ?? res
    isLoggedIn.value = true
    displayName.value = u.fullName || u.nickName || u.userName || ''
    localStorage.setItem('user', JSON.stringify(u))
  } catch (e) {
    isLoggedIn.value = false
    displayName.value = ''
  }
}

onMounted(loadProfile)

watch(() => route.fullPath, () => {
  loadProfile()
})


// === 退出登录 ===
async function handleLogout() {
  try {
    await logout()   // 通知后端作废 token（可选）
  } catch (e) {
    console.warn('后端登出失败，前端仍会清理本地状态', e)
  } finally {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    isLoggedIn.value = false
    displayName.value = ''
    router.push('/') // 跳转到主页
  }
}
</script>


<style>
/* ---------- NAVBAR 基础 ---------- */
.navbar {
  position: fixed;
  top: 0;
  width: 100%;
  z-index: 1000;
  background-color: transparent;
  transition:
    background 0.3s ease,
    box-shadow 0.3s ease,
    height 1s ease;
  box-shadow: none;
  height: 4rem;
}

/* 顶部渐变遮罩增强可读性 */
.navbar::before {
  content: "";
  position: absolute;
  inset: 0;
  background: linear-gradient(rgba(0,0,0,0.6), rgba(0,0,0,0));
  pointer-events: none;
  opacity: 1;
  transition: opacity .3s ease;
}

.navbar.scrolled {
  background-color: #004b8d;
  box-shadow: 0 2px 6px rgba(0,0,0,.1);
  height: 3.5rem;
}
.navbar.scrolled::before { opacity: 0; }

.navbar-container {
  max-width: 1200px;
  margin: auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
}

.logo {
  font-size: 1.5rem;
  font-weight: 700;
  color: #fff;
  text-decoration: none;
}

/* ---------- 链接区 ---------- */
.nav-links {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.nav-item,
.nav-item span {
  width: 120px;
  text-align: center;
  color: #fff;
  font-size: 0.85rem;
  font-weight: 700;
  text-decoration: none;
  padding: 0.3rem 0.5rem;
  border-radius: 6px;
  position: relative;
  cursor: pointer;
  line-height: 1;
  text-shadow: 0 0 4px rgba(0,0,0,0.6);
}

/* 下划线动画 */
.nav-item::after {
  content: "";
  position: absolute;
  left: 50%;                  /* 居中起点 */
  transform: translateX(-50%) scaleX(0);  /* 居中 + 缩放隐藏 */
  bottom: 0.1rem;
  height: 2px;
  width: 50%;                 /* 控制下划线宽度（比如 50%） */
  background: #ffffff;
  opacity: 0;
  transition: transform 0.25s ease, opacity 0.25s ease;
  transform-origin: center;
  pointer-events: auto;       /* 允许伪元素捕获鼠标事件 */
}

.nav-item:hover::after {
  transform: translateX(-50%) scaleX(1);
  opacity: 1;
}


.nav-item:hover { background: transparent; }

/* ---------- 下拉菜单 ---------- */
.dropdown {
  position: relative;
}

.dropdown:hover .dropdown-menu {
  display: flex;
  flex-direction: column;
}

.dropdown-menu {
  display: none;
  position: absolute;
  top: calc(100% + 0.1rem);
  left: 0%;
  transform: translateX(-50%);
  background: #deeaff;
  border: 1px solid #dce6f1;
  text-shadow: 0 0 1px rgba(0, 0, 0, 0.08);

  flex-direction: column;
  min-width: 120px;
  z-index: 10;
  text-align: center;
}

.dropdown-menu a {
  color: #000;
  text-decoration: none;
  padding: 0.8rem 0.75rem;
  font-size: 0.85rem;
  font-weight: 600;
  line-height: 1.2;
  width: 100%;              /* 强制每个链接占满整行 */
  box-sizing: border-box;   /* 使 padding 不影响 width */
}

.dropdown-menu a:hover {
  background: #ffffff;
}

/* ---------- 登录按钮 ---------- */
.auth-btn .login {
  color: #004b8d;
  background: #fff;
  padding: 0.4rem 1rem;
  border-radius: 6px;
  text-decoration: none;
  font-weight: 600;
  font-size: 0.9rem;
}

.auth-btn .login:hover { background: #f0f0f0; }

/* ---------- 欢迎语+退出样式 ---------- */
/* 欢迎语样式 */
.welcome-text {
  color: #fff;
  font-size: 0.9rem;
  font-weight: 600;
  text-shadow: 0 0 4px rgba(0,0,0,0.6);
}

/* 退出按钮样式：小胶囊蓝色 */
.logout-inline {
  border: none;
  color: hsl(221, 98%, 79%);
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0.35rem 0.8rem;
  border-radius: 6px;
  transition: background 0.2s ease;
}

</style>

<template>
  <div class="sidebar">
    <div class="logo">我的图书馆</div>
    <ul class="nav-list">
      <li>
        <router-link 
          to="/" 
          class="nav-item" 
          active-class="nav-item-active"
        >
          图书馆首页
        </router-link>
      </li>
      <li>
        <router-link 
          to="/my/home/dashboard" 
          class="nav-item" 
          active-class="nav-item-active"
        >
          个人主页
        </router-link>
      </li>
      <li>
        <router-link 
          to="/user/favorites" 
          class="nav-item" 
          active-class="nav-item-active"
        >
          收藏图书
        </router-link>
      </li>
      <li>
        <router-link 
          to="/user/booklist" 
          class="nav-item" 
          active-class="nav-item-active"
        >
          我的书单
        </router-link>
      </li>
      <li>
        <router-link 
          to="/my/borrowingRecords" 
          class="nav-item" 
          active-class="nav-item-active"
        >
          借阅记录
        </router-link>
      </li>

      <!-- ========== 修改部分开始 ========== -->
      <li>
        <router-link 
          to="/my/reservations" 
          class="nav-item" 
          active-class="nav-item-active"
        >
          我的预约
        </router-link>
      </li>
      <!-- ========== 修改部分结束 ========== -->

    </ul>
  </div>
</template>

<script setup>
</script>

<style scoped>
/* 整个侧边栏 */
.sidebar {
  width: 220px;
  height: 100vh;
  background: #1e293b;
  color: white;
  padding: 24px 16px;
  box-sizing: border-box;
}

/* Logo */
.logo {
  font-size: 20px;
  font-weight: bold;
  margin-bottom: 32px;
}

/* 菜单列表 */
.nav-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

/* 单个菜单项 */
.nav-item {
  display: block;
  padding: 12px 16px;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.3s, box-shadow 0.3s;
  color: #fff;
  text-decoration: none;
}

/* 悬停效果 */
.nav-item:hover {
  background: #334155;
}

/* 当前路由选中效果 */
.nav-item-active {
  background: #475569;
  font-weight: bold;
}

/* 您之前的 ::before 样式可能会导致布局问题，我将其注释掉，
   因为 nav-item-active 已经有背景色和粗体来标示选中状态了 */
/*
.nav-item-active::before {
  content: '';
  position: absolute;
  left: -2px;
  top: 0;
  height: 100%;
  width: 4px;
  border-radius: 2px;
}
*/
</style>
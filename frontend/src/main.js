// 文件: frontend/src/main.js
// 这是集成了 Element Plus 的、与您项目结构完全匹配的最终版本

import './assets/main.css'


import { createApp } from 'vue'
import { createPinia } from 'pinia'

// --- 1. 引入 Element Plus ---
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css' // 引入 Element Plus 的核心 CSS 样式

import App from './App.vue'
import router from './router'

// --- 创建 Vue 应用实例 ---
const app = createApp(App)

// --- 按顺序注册插件 ---
app.use(createPinia())
app.use(router)
app.use(ElementPlus) // <-- 2. 将 Element Plus 注册为全局插件

// --- 最终挂载应用 ---
app.mount('#app')
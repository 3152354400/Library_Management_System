<template>
  <div class="login-container">
    <div class="login-box">
      <h2>{{ isLogin ? '欢迎登录' : '创建账户' }}</h2>
      <form @submit.prevent="onSubmit">
        <!-- 登录身份reader/librarian -->
        <!-- 注册时身份必须为reader -->
        <div v-if="isLogin" class="input-group">
          <div class="role-inline flex items-center gap-4 text-sm">
            <span class="opacity-90">登录身份：</span>
            <label class="inline-flex items-center cursor-pointer gap-1">
              <input type="radio" value="reader" v-model="loginType" />
              <span>读者登录</span>
            </label>
            <label class="inline-flex items-center cursor-pointer gap-1">
              <input type="radio" value="librarian" v-model="loginType" />
              <span>管理员登录</span>
            </label>
          </div>
        </div>

        <!-- 用户名 -->
        <div class="input-group">
          <label>用户名</label>
          <input
            type="text"
            v-model.trim="form.username"
            placeholder="请输入用户名"
            :class="{ error: errors.username }"
          />
          <p v-if="errors.username" class="error-msg">{{ errors.username }}</p>
        </div>

        <!-- 密码 -->
        <div class="input-group">
          <label>密码</label>
          <input
            type="password"
            v-model.trim="form.password"
            placeholder="请输入密码"
            :class="{ error: errors.password }"
          />
          <p v-if="errors.password" class="error-msg">{{ errors.password }}</p>
        </div>

        <!-- 确认密码（注册时显示） -->
        <div class="input-group" v-if="!isLogin">
          <label>确认密码</label>
          <input
            type="password"
            v-model.trim="form.confirmPassword"
            placeholder="请再次输入密码"
            :class="{ error: errors.confirmPassword }"
          />
          <p v-if="errors.confirmPassword" class="error-msg">{{ errors.confirmPassword }}</p>
        </div>

        <!-- 记住我（登录时显示） -->
        <div v-if="isLogin" class="flex justify-between items-center mb-4">
          <label><input type="checkbox" v-model="rememberMe" /> 记住我</label>
          <!-- <a href="#" class="forgot-link">忘记密码？</a> -->
        </div>

        <!-- 提交按钮 -->
        <button type="submit" :disabled="loading">
          {{ loading ? (isLogin ? '登录中...' : '注册中...') : (isLogin ? '登录' : '注册') }}
        </button>
      </form>

      <!-- 切换模式按钮 -->
      <p class="toggle-text">
        {{ isLogin ? '没有账号？' : '已有账号？' }}
        <!-- 切换模式时强制设置为读者登录 -->
        <button @click="toggleMode" class="toggle-btn">
          {{ isLogin ? '点击注册' : '返回登录' }}
        </button>
      </p>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { getLibrarianProfile, getMyProfile, login, register } from '@/modules/reader/api.js'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

const isLogin = ref(true)
const loading = ref(false)
const rememberMe = ref(false)

const loginType = ref("reader")  // 默认读者登录

const form = reactive({
  username: '1234567',
  password: '1234567',
  confirmPassword: '',
})

const errors = reactive({
  username: '',
  password: '',
  confirmPassword: '',
})

// 读取记住的用户名
onMounted(() => {
  const remembered = localStorage.getItem('rememberedUser')
  if (remembered) {
    form.username = JSON.parse(remembered).username
    rememberMe.value = true
  }
})

function validate() {
  errors.username = ''
  errors.password = ''
  errors.confirmPassword = ''

  if (!form.username) {
    errors.username = '请输入用户名'
  } else if (form.username.length < 2) {
    errors.username = '用户名至少2个字符'
  } else if (!/^[a-zA-Z0-9]+$/.test(form.username)) {
    errors.username = '用户名只能包含字母和数字'
  }

  if (!form.password) {
    errors.password = '请输入密码'
  } else if (form.password.length < 5) {
    errors.password = '密码至少5个字符'
  }

  if (!isLogin.value) {
    if (!form.confirmPassword) {
      errors.confirmPassword = '请确认密码'
    } else if (form.confirmPassword !== form.password) {
      errors.confirmPassword = '两次密码输入不一致'
    }
  }

  return !errors.username && !errors.password && (isLogin.value || !errors.confirmPassword)
}

async function onSubmit() {
  if (!validate()) return

  loading.value = true
  try {
    // === 登录 ===
    if (isLogin.value) {
        const res = await login({ username: form.username, password: form.password ,usertype:loginType.value})
        console.log(res)
        if (res.status === 200) {
          if (rememberMe.value) {
            localStorage.setItem('rememberedUser', JSON.stringify({ username: form.username }))
          } else {
            localStorage.removeItem('rememberedUser')
          }
          localStorage.setItem('token', res.data)

          if(loginType.value === 'reader') {
            const user = (await getMyProfile()).data
            localStorage.setItem('user', JSON.stringify(user))

            const redirectPath = route.query.redirect || '/' //读者登录后跳转到home页面
            await router.push(redirectPath)
          }else if(loginType.value === 'librarian'){
            const user = (await getLibrarianProfile()).data
            localStorage.setItem('user', JSON.stringify(user))
            //跳转到管理员操作页面
            await router.push('/admin/dashboard')
          }
        } else {
          alert(res.msg || '登录失败')
        }
    }
    // === 注册 ===
    else {
      if (loginType.value === 'reader') {
        const res = await register({ username: form.username, password: form.password })
        console.log(res)
        if (res.status === 200) {
          alert('注册成功，请登录')
          toggleMode()
        } else {
          alert(res.msg || '注册失败')
        }
      }
      // 管理员不能在公开位置注册，只能被其他管理员添加
    }
  } catch (e) {
    alert('请求失败')
  } finally {
    loading.value = false
  }
}

function toggleMode() {
  isLogin.value = !isLogin.value
  form.password = ''
  form.confirmPassword = ''
  if (!rememberMe.value) {
    form.username = ''
  }

  // 切换模式时强制设置为读者登录
  loginType.value = 'reader'

  clearErrors()
}

function clearErrors() {
  errors.username = ''
  errors.password = ''
  errors.confirmPassword = ''
}
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: linear-gradient(135deg, #667eea, #764ba2);
  padding: 20px;
}
.login-box {
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  padding: 32px 36px;
  width: 100%;
  max-width: 400px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
  color: #fff;
  text-align: center;
}
.login-box h2 {
  margin-bottom: 24px;
  font-size: 28px;
  font-weight: 600;
}
.input-group {
  margin-bottom: 18px;
  text-align: left;
}
.input-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: 600;
  font-size: 14px;
}
.input-group input {
  width: 100%;
  padding: 10px 12px;
  border: none;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.8);
  color: #333;
  font-size: 14px;
  transition: background 0.3s ease;
}
.input-group input:focus {
  outline: 2px solid #667eea;
  background: #fff;
}
.error {
  border-color: #f87171 !important;
}
.error-msg {
  color: #f87171;
  font-size: 12px;
  margin-top: 4px;
}
button[type="submit"] {
  width: 100%;
  padding: 12px 0;
  background: #667eea;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 700;
  color: white;
  cursor: pointer;
  transition: background 0.3s ease;
}
button[type="submit"]:disabled {
  background: #a3bffa;
  cursor: not-allowed;
}
button[type="submit"]:hover:not(:disabled) {
  background: #5a67d8;
}
.toggle-text {
  margin-top: 20px;
  font-size: 14px;
  color: #d1d5db;
}
.toggle-btn {
  background: none;
  border: none;
  color: #a5b4fc;
  font-weight: 600;
  cursor: pointer;
  margin-left: 6px;
}
.toggle-btn:hover {
  text-decoration: underline;
}
.forgot-link {
  color: #cbd5e1;
  font-size: 13px;
  text-decoration: none;
}
.forgot-link:hover {
  text-decoration: underline;
}

/* 圆圈和文字在一行，并去掉点击蓝框 */
input[type="radio"] {
  vertical-align: middle;
  cursor: pointer;
  accent-color: #667eea; /* 自定义选中颜色 (可选) */
  outline: none;         /* 去掉默认的蓝色外框 */
}

input[type="radio"]:focus {
  outline: none;  /* 阻止浏览器默认 focus 高亮 */
  box-shadow: none;
}

</style>

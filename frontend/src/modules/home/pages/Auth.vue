<template>
  <div class="auth-page">
    <div class="auth-left">
      <LoginBg />
      <div class="hero-content">
        <div class="brand">
          <svg width="48" height="48" viewBox="0 0 64 64" aria-hidden="true">
            <defs>
              <linearGradient id="lbg" x1="0" y1="0" x2="1" y2="1">
                <stop offset="0" stop-color="#fff"/>
                <stop offset="1" stop-color="#BAC8FD"/>
              </linearGradient>
            </defs>
            <rect x="6" y="10" width="6" height="44" rx="2" fill="url(#lbg)"/>
            <rect x="14" y="14" width="6" height="40" rx="2" fill="url(#lbg)" opacity=".85"/>
            <rect x="22" y="18" width="6" height="36" rx="2" fill="url(#lbg)" opacity=".7"/>
            <rect x="30" y="14" width="6" height="40" rx="2" fill="url(#lbg)" opacity=".85"/>
            <rect x="38" y="10" width="6" height="44" rx="2" fill="url(#lbg)"/>
            <rect x="46" y="16" width="6" height="38" rx="2" fill="url(#lbg)" opacity=".7"/>
          </svg>
          <div>
            <div class="brand-name">智书云</div>
            <div class="brand-tag">Library Management</div>
          </div>
        </div>
        <h1>智慧图书馆<br/>触手可及</h1>
        <p>10万+ 馆藏图书在线检索,座位实时预约,借阅进度一键掌握。</p>
        <ul class="features">
          <li><el-icon><Check /></el-icon> 个性化推荐,发现下一本好书</li>
          <li><el-icon><Check /></el-icon> 扫码借还,无需排队</li>
          <li><el-icon><Check /></el-icon> 自习座位实时预约</li>
          <li><el-icon><Check /></el-icon> 通知实时推送,逾期不再错过</li>
        </ul>
      </div>
    </div>

    <div class="auth-right">
      <div class="auth-card">
        <div class="auth-tabs">
          <button :class="{ active: mode === 'login' }" @click="mode = 'login'">登录</button>
          <button :class="{ active: mode === 'register' }" @click="mode = 'register'" v-if="loginType === 'reader'">注册</button>
        </div>

        <h2 class="auth-title">{{ mode === 'login' ? '欢迎回来' : '创建读者账号' }}</h2>

        <div v-if="mode === 'login'" class="role-toggle">
          <label :class="{ active: loginType === 'reader' }">
            <input v-model="loginType" type="radio" value="reader" /> 读者
          </label>
          <label :class="{ active: loginType === 'librarian' }">
            <input v-model="loginType" type="radio" value="librarian" /> 管理员
          </label>
        </div>

        <el-form ref="formRef" :model="form" :rules="rules" label-position="top" @submit.prevent="submit">
          <el-form-item label="用户名" prop="username">
            <el-input v-model="form.username" placeholder="请输入用户名" :prefix-icon="User" size="large" />
          </el-form-item>

          <el-form-item label="密码" prop="password">
            <el-input v-model="form.password" type="password" placeholder="请输入密码" :prefix-icon="Lock" size="large" show-password />
          </el-form-item>

          <el-form-item v-if="mode === 'register'" label="确认密码" prop="confirmPassword">
            <el-input v-model="form.confirmPassword" type="password" placeholder="请再次输入密码" :prefix-icon="Lock" size="large" show-password />
          </el-form-item>

          <div v-if="mode === 'login'" class="form-extra">
            <el-checkbox v-model="rememberMe">记住我</el-checkbox>
            <el-link :underline="false" type="primary" @click="forgotPwd">忘记密码?</el-link>
          </div>

          <el-button type="primary" size="large" :loading="loading" style="width:100%;margin-top:8px" @click="submit">
            {{ mode === 'login' ? '登录' : '注册并登录' }}
          </el-button>
        </el-form>

        <div class="demo-tip" v-if="mode === 'login'">
          <div class="tip-title">演示账号(已配置 mock)</div>
          <div class="ac-row">
            <span>读者</span>
            <code>reader001</code> / <code>123456</code>
          </div>
          <div class="ac-row">
            <span>管理员</span>
            <code>LIB001</code> / <code>123456</code>
          </div>
        </div>

        <div class="switch-mode" v-if="loginType === 'reader'">
          {{ mode === 'login' ? '还没有账号?' : '已有账号?' }}
          <el-link type="primary" :underline="false" @click="mode = mode === 'login' ? 'register' : 'login'">
            {{ mode === 'login' ? '立即注册' : '返回登录' }}
          </el-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import { authApi } from '@/api/auth'
import { useUserStore } from '@/stores/user'
import LoginBg from '../components/LoginBg.vue'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

const mode = ref(route.query.mode === 'register' ? 'register' : 'login')
const loginType = ref('reader')
const rememberMe = ref(false)
const loading = ref(false)
const formRef = ref(null)
const form = reactive({ username: '', password: '', confirmPassword: '' })

const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, min: 5, message: '密码至少 5 位', trigger: 'blur' }],
  confirmPassword: [
    { required: true, message: '请再次输入密码', trigger: 'blur' },
    {
      validator: (rule, value, cb) => value === form.password ? cb() : cb(new Error('两次输入不一致')),
      trigger: 'blur'
    }
  ]
}

onMounted(() => {
  const remembered = localStorage.getItem('rememberedUser')
  if (remembered) {
    form.username = JSON.parse(remembered).username
    rememberMe.value = true
  }
})

watch(mode, () => {
  form.password = ''
  form.confirmPassword = ''
})

async function submit() {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (!valid) return
    loading.value = true
    try {
      if (mode.value === 'login') {
        const token = await authApi.login({
          username: form.username,
          password: form.password,
          usertype: loginType.value
        })
        localStorage.setItem('token', token)
        if (rememberMe.value) localStorage.setItem('rememberedUser', JSON.stringify({ username: form.username }))
        else localStorage.removeItem('rememberedUser')

        let user
        let role = loginType.value
        if (loginType.value === 'reader') {
          user = await authApi.myProfile()
        } else {
          user = await fetch('/api/librarian/info', { headers: { Authorization: `Bearer ${token}` } }).then((r) => r.ok ? r.json() : { userName: form.username, fullName: '管理员' }).catch(() => ({ userName: form.username }))
        }
        userStore.setLogin({ token, user, role })

        const redirect = route.query.redirect
        if (loginType.value === 'librarian') router.push(redirect || '/admin/dashboard')
        else router.push(redirect || '/reader/dashboard')
      } else {
        if (loginType.value !== 'reader') return ElMessage.warning('管理员账号请联系系统开通')
        await authApi.register({ username: form.username, password: form.password })
        ElMessage.success('注册成功,请登录')
        mode.value = 'login'
      }
    } finally {
      loading.value = false
    }
  })
}

function forgotPwd() {
  ElMessage.info('请联系系统管理员重置密码')
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 1fr 1fr;
  background: var(--color-bg);
}

.auth-left {
  position: relative;
  background: linear-gradient(135deg, #1E2A6E 0%, #3B5BDB 60%, #5C7CFA 100%);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  overflow: hidden;
}
.hero-content { position: relative; max-width: 480px; z-index: 1; }
.brand { display: flex; align-items: center; gap: 12px; margin-bottom: 48px; }
.brand-name { font-size: var(--fs-2xl); font-weight: 700; }
.brand-tag { font-size: var(--fs-xs); opacity: 0.7; }
.hero-content h1 { font-size: 3rem; line-height: 1.15; margin: 0 0 16px; font-weight: 800; }
.hero-content p { opacity: 0.85; font-size: var(--fs-lg); margin-bottom: 32px; }
.features { display: flex; flex-direction: column; gap: 12px; }
.features li { display: flex; align-items: center; gap: 10px; font-size: var(--fs-base); }
.features li .el-icon { background: rgba(255,255,255,.2); padding: 4px; border-radius: 999px; }

.auth-right {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
}
.auth-card { width: 100%; max-width: 420px; }
.auth-tabs {
  display: flex;
  gap: 24px;
  margin-bottom: 32px;
  border-bottom: 1px solid var(--color-border-soft);
}
.auth-tabs button {
  background: none;
  border: none;
  font-size: var(--fs-base);
  padding: 8px 0;
  color: var(--color-text-muted);
  cursor: pointer;
  position: relative;
  font-weight: 500;
}
.auth-tabs button.active {
  color: var(--color-primary-600);
}
.auth-tabs button.active::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 2px;
  background: var(--color-primary-600);
}
.auth-title { font-size: var(--fs-2xl); margin: 0 0 24px; }

.role-toggle {
  display: flex;
  background: var(--color-bg);
  border-radius: var(--radius-md);
  padding: 4px;
  margin-bottom: 16px;
}
.role-toggle label {
  flex: 1;
  text-align: center;
  padding: 8px;
  border-radius: 6px;
  cursor: pointer;
  font-size: var(--fs-sm);
  transition: all .2s;
}
.role-toggle label.active {
  background: #fff;
  color: var(--color-primary-600);
  box-shadow: var(--shadow-sm);
  font-weight: 600;
}
.role-toggle input { display: none; }

.form-extra {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.demo-tip {
  margin-top: 20px;
  padding: 12px 14px;
  background: var(--color-primary-50);
  border-radius: var(--radius-md);
  font-size: var(--fs-sm);
}
.tip-title { font-weight: 600; color: var(--color-primary-600); margin-bottom: 6px; }
.ac-row { display: flex; justify-content: space-between; padding: 2px 0; color: var(--color-text-soft); }
.ac-row code {
  background: #fff;
  padding: 1px 8px;
  border-radius: 4px;
  font-family: var(--font-mono);
  font-size: var(--fs-xs);
  color: var(--color-text);
}

.switch-mode {
  margin-top: 16px;
  text-align: center;
  font-size: var(--fs-sm);
  color: var(--color-text-soft);
}

@media (max-width: 900px) {
  .auth-page { grid-template-columns: 1fr; }
  .auth-left { padding: 32px; min-height: 220px; }
  .hero-content h1 { font-size: 2rem; }
  .hero-content p { font-size: var(--fs-base); margin-bottom: 16px; }
  .features { display: none; }
  .brand { margin-bottom: 16px; }
}
</style>
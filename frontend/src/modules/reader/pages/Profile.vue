<template>
  <div class="rd-page">
    <PageHeader title="个人资料" subtitle="维护你的基本信息" />

    <div class="layout">
      <div class="card avatar-card">
        <div class="avatar-wrap">
          <img :src="avatarUrl" class="big-avatar" />
        </div>
        <div class="meta">
          <div class="name">{{ userStore.fullName || userStore.userName }}</div>
          <div class="sub">{{ userStore.nickName }}</div>
          <el-tag :type="userStore.accountStatus === '正常' ? 'success' : 'danger'" size="small" effect="light">
            {{ userStore.accountStatus }}
          </el-tag>
          <div class="credit">
            <span>信用分</span>
            <span class="score">{{ userStore.creditScore }}</span>
          </div>
        </div>
        <el-upload :show-file-list="false" :before-upload="beforeUpload" accept="image/*">
          <el-button type="primary" :loading="uploading">更换头像</el-button>
        </el-upload>
      </div>

      <div class="card form-card">
        <h3>基本信息</h3>
        <el-form :model="form" label-width="100px" style="max-width:560px">
          <el-form-item label="用户名">
            <el-input v-model="form.userName" />
          </el-form-item>
          <el-form-item label="真实姓名">
            <el-input v-model="form.fullName" />
          </el-form-item>
          <el-form-item label="昵称">
            <el-input v-model="form.nickName" />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" :loading="saving" @click="onSave">保存修改</el-button>
          </el-form-item>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { authApi } from '@/api/auth'
import { useUserStore } from '@/stores/user'
import PageHeader from '@/shared/components/PageHeader.vue'

const userStore = useUserStore()
const baseAvatarUrl = import.meta.env.VITE_BASE_AVATAR_URL || '/avatars/'
const avatarUrl = ref(baseAvatarUrl + (userStore.avatar || 'system_0.png'))
const saving = ref(false)
const uploading = ref(false)
const form = reactive({ userName: '', fullName: '', nickName: '' })

onMounted(() => {
  form.userName = userStore.userName || ''
  form.fullName = userStore.fullName || ''
  form.nickName = userStore.nickName || ''
  avatarUrl.value = baseAvatarUrl + (userStore.avatar || 'system_0.png')
})

async function onSave() {
  saving.value = true
  try {
    await authApi.updateMyProfile({ ...form })
    userStore.setUser({ ...form })
    ElMessage.success('保存成功')
  } finally { saving.value = false }
}

async function beforeUpload(file) {
  if (file.size > 1024 * 1024) { ElMessage.error('头像文件不能超过 1MB'); return false }
  uploading.value = true
  try {
    const url = await authApi.uploadAvatar(file)
    avatarUrl.value = baseAvatarUrl + url
    userStore.setUser({ avatar: url })
    ElMessage.success('头像已更新')
  } finally { uploading.value = false }
  return false
}
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
.layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 16px;
  align-items: flex-start;
}
.avatar-card { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 14px; }
.avatar-wrap {
  width: 120px;
  height: 120px;
  border-radius: 999px;
  overflow: hidden;
  background: var(--color-primary-100);
  box-shadow: var(--shadow-md);
}
.big-avatar { width: 100%; height: 100%; object-fit: cover; }
.meta .name { font-size: var(--fs-lg); font-weight: 600; }
.meta .sub { color: var(--color-text-soft); margin: 4px 0 8px; }
.credit {
  margin-top: 12px;
  padding: 10px 16px;
  background: var(--color-bg);
  border-radius: var(--radius-md);
  display: flex;
  justify-content: space-between;
  width: 100%;
}
.credit .score { color: var(--color-accent-orange); font-weight: 700; font-size: var(--fs-lg); }
.form-card h3 { margin: 0 0 16px; }
@media (max-width: 768px) { .layout { grid-template-columns: 1fr; } }
</style>
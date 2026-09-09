<template>
  <div class="rd-page">
    <PageHeader title="账号安全" subtitle="修改密码、保障账号安全" />

    <div class="card" style="max-width: 520px">
      <h3>修改密码</h3>
      <p class="hint text-muted">建议使用 8 位以上,包含字母与数字的强密码</p>

      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="当前密码" prop="oldPwd">
          <el-input v-model="form.oldPwd" type="password" show-password />
        </el-form-item>
        <el-form-item label="新密码" prop="newPwd">
          <el-input v-model="form.newPwd" type="password" show-password />
        </el-form-item>
        <el-form-item label="确认新密码" prop="confirmPwd">
          <el-input v-model="form.confirmPwd" type="password" show-password />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="saving" @click="onSubmit">修改密码</el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { authApi } from '@/api/auth'
import { useUserStore } from '@/stores/user'
import { useRouter } from 'vue-router'
import PageHeader from '@/shared/components/PageHeader.vue'

const userStore = useUserStore()
const router = useRouter()
const formRef = ref(null)
const saving = ref(false)
const form = reactive({ oldPwd: '', newPwd: '', confirmPwd: '' })

const rules = {
  oldPwd: [{ required: true, message: '请输入当前密码', trigger: 'blur' }],
  newPwd: [{ required: true, min: 6, message: '密码至少 6 位', trigger: 'blur' }],
  confirmPwd: [
    { required: true, message: '请再次输入新密码', trigger: 'blur' },
    {
      validator: (rule, value, cb) => value === form.newPwd ? cb() : cb(new Error('两次密码不一致')),
      trigger: 'blur'
    }
  ]
}

async function onSubmit() {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (!valid) return
    saving.value = true
    try {
      await authApi.resetMyPassword(form.oldPwd, form.newPwd)
      ElMessage.success('密码修改成功,请重新登录')
      userStore.logout()
      router.push('/auth')
    } finally { saving.value = false }
  })
}
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
.hint { margin: 0 0 24px; }
h3 { margin: 0 0 8px; }
</style>
<template>
  <div class="ad-page">
    <PageHeader title="读者管理" subtitle="查看 / 新增 / 编辑读者账号,重置密码" />

    <div class="card">
      <div class="filter-bar">
        <el-input v-model="keyword" placeholder="搜索用户名 / 姓名" clearable style="width:280px" :prefix-icon="Search" @keyup.enter="load" />
        <el-button type="primary" @click="load">查询</el-button>
        <el-button type="primary" :icon="Plus" @click="onCreate">新增读者</el-button>
      </div>

      <el-table :data="filtered" v-loading="loading" stripe>
        <el-table-column prop="ReaderID" label="ID" width="100" />
        <el-table-column prop="Name" label="姓名" width="120" />
        <el-table-column prop="UserName" label="用户名" width="160" />
        <el-table-column label="类型" width="100">
          <template #default="{ row }"><el-tag size="small">{{ row.ReaderType }}</el-tag></template>
        </el-table-column>
        <el-table-column label="信用分" width="120">
          <template #default="{ row }">
            <span :class="row.CreditScore >= 90 ? 'text-success' : 'text-warning'">{{ row.CreditScore }}</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <StatusTag :status="row.AccountStatus" />
          </template>
        </el-table-column>
        <el-table-column label="权限" prop="Permission" width="100" />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button text type="primary" @click="onEdit(row)"><el-icon><Edit /></el-icon> 编辑</el-button>
            <el-button text type="warning" @click="onResetPwd(row)"><el-icon><Key /></el-icon> 重置密码</el-button>
            <el-button text type="danger" @click="onDelete(row)"><el-icon><Delete /></el-icon></el-button>
          </template>
        </el-table-column>
      </el-table>
      <EmptyState v-if="!loading && !filtered.length" title="暂无读者" />
    </div>

    <el-dialog v-model="editDialog" :title="editing ? '编辑读者' : '新增读者'" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="用户名"><el-input v-model="form.UserName" :disabled="!!editing" /></el-form-item>
        <el-form-item label="姓名"><el-input v-model="form.Name" /></el-form-item>
        <el-form-item label="类型">
          <el-radio-group v-model="form.ReaderType">
            <el-radio value="学生">学生</el-radio>
            <el-radio value="教师">教师</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="状态">
          <el-radio-group v-model="form.AccountStatus">
            <el-radio value="正常">正常</el-radio>
            <el-radio value="冻结">冻结</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="权限">
          <el-radio-group v-model="form.Permission">
            <el-radio value="普通">普通</el-radio>
            <el-radio value="高级">高级</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="信用分"><el-input-number v-model="form.CreditScore" :min="0" :max="100" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Plus, Edit, Delete, Key } from '@element-plus/icons-vue'
import { adminApi } from '@/api/admin'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const keyword = ref('')
const list = ref([])
const loading = ref(false)
const editDialog = ref(false)
const editing = ref(null)
const form = reactive({ UserName: '', Name: '', ReaderType: '学生', AccountStatus: '正常', Permission: '普通', CreditScore: 100 })

const filtered = computed(() => {
  if (!keyword.value) return list.value
  const kw = keyword.value.toLowerCase()
  return list.value.filter((r) => (r.Name || '').toLowerCase().includes(kw) || (r.UserName || '').toLowerCase().includes(kw))
})

async function load() {
  loading.value = true
  try { list.value = await adminApi.readers() }
  finally { loading.value = false }
}

function onCreate() {
  editing.value = null
  Object.assign(form, { UserName: '', Name: '', ReaderType: '学生', AccountStatus: '正常', Permission: '普通', CreditScore: 100 })
  editDialog.value = true
}

function onEdit(row) {
  editing.value = row
  Object.assign(form, row)
  editDialog.value = true
}

async function confirmSave() {
  if (editing.value) {
    await adminApi.updateReader(form)
    ElMessage.success('已更新')
  } else {
    await adminApi.addReader(form)
    ElMessage.success('已新增')
  }
  editDialog.value = false
  load()
}

function onResetPwd(row) {
  ElMessageBox.confirm(`重置 ${row.Name} 的密码为 123456?`, '提示', { type: 'warning' })
    .then(async () => {
      await adminApi.resetReaderPassword(row.UserName, '123456')
      ElMessage.success('密码已重置为 123456')
    }).catch(() => {})
}

function onDelete(row) {
  ElMessageBox.confirm(`确认删除读者 ${row.Name}?`, '提示', { type: 'danger' })
    .then(async () => {
      await adminApi.deleteReader(row.ReaderID)
      ElMessage.success('已删除')
      load()
    }).catch(() => {})
}

onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.filter-bar { display: flex; gap: 12px; margin-bottom: 16px; }
</style>
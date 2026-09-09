<template>
  <div class="reader-management">
    <!-- 搜索栏 -->
    <div class="search-bar">
      <el-input
          v-model="query.keyword"
          placeholder="搜索用户名"
          clearable
          @keyup.enter="searchReaders"
      />
      <el-button type="primary" @click="searchReaders">搜索</el-button>
      <el-button type="success" @click="openAddDialog">新增读者</el-button>
    </div>

    <!-- 读者表格 -->
    <el-table :data="readers" stripe border style="width: 100%">
      <el-table-column prop="username" label="用户名" width="150" />
      <el-table-column prop="nickname" label="昵称" width="150" />
      <el-table-column prop="realName" label="真实姓名" width="150" />
      <el-table-column prop="credit" label="信誉分" width="100" />
      <el-table-column prop="status" label="账户状态" width="120">
        <template #default="{ row }">
          <el-tag :type="row.status === 'ACTIVE' ? 'success' : 'danger'">
            {{ row.status === 'ACTIVE' ? '正常' : '冻结' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="role" label="权限" width="120" />
      <el-table-column label="操作" width="400">
        <template #default="{ row }">
          <div class="operation-cell">
            <el-button type="primary" size="small" @click="openEditDialog(row.id)">编辑</el-button>
            <el-button
                :type="row.status === 'ACTIVE' ? 'warning' : 'success'"
                size="small"
                @click="toggleFreeze(row)"
            >
              {{ row.status === 'ACTIVE' ? '冻结' : '解冻' }}
            </el-button>
            <el-button type="danger" size="small" @click="removeReader(row.id)">删除</el-button>
            <el-button type="info" size="small" @click="confirmResetPassword(row.username)">重置密码</el-button>
          </div>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分页 -->
    <div class="pagination">
      <el-pagination
          background
          layout="prev, pager, next, jumper, sizes"
          :total="total"
          v-model:current-page="query.pageNum"
          v-model:page-size="query.pageSize"
          @current-change="updatePageData"
          @size-change="updatePageData"
      />
    </div>

    <!-- 新增弹窗 -->
    <el-dialog v-model="addDialogVisible" title="新增读者" width="500px">
      <el-form :model="addForm" label-width="90px">
        <el-form-item label="用户名">
          <el-input v-model="addForm.username" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="addForm.password" type="password" show-password />
        </el-form-item>
        <el-form-item label="昵称">
          <el-input v-model="addForm.nickname" />
        </el-form-item>
        <el-form-item label="真实姓名">
          <el-input v-model="addForm.realName" />
        </el-form-item>
        <el-form-item label="信誉分">
          <el-input-number v-model="addForm.credit" :min="0" />
        </el-form-item>
        <el-form-item label="权限">
          <el-select v-model="addForm.role" placeholder="请选择权限">
            <el-option label="普通" value="普通" />
            <el-option label="高级" value="高级" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="addDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveNewReader">确定</el-button>
      </template>
    </el-dialog>

    <!-- 编辑弹窗 -->
    <el-dialog v-model="editDialogVisible" title="编辑读者" width="500px">
      <el-form :model="editForm" label-width="90px">
        <el-form-item label="用户名">
          <el-input v-model="editForm.username" disabled />
        </el-form-item>
        <el-form-item label="昵称">
          <el-input v-model="editForm.nickname" />
        </el-form-item>
        <el-form-item label="真实姓名">
          <el-input v-model="editForm.realName" />
        </el-form-item>
        <el-form-item label="信誉分">
          <el-input-number v-model="editForm.credit" :min="0" />
        </el-form-item>
        <el-form-item label="权限">
          <el-select v-model="editForm.role" placeholder="请选择权限">
            <el-option label="普通" value="普通" />
            <el-option label="高级" value="高级" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveEditedReader">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getReaders, getReaderById, addReader, updateReaderPartial, deleteReader,reset } from '@/modules/reader/api.js'

const readers = ref([]) // 当前页显示数据
const allReaders = ref([]) // 存储全部数据
const total = ref(0)

const query = reactive({
  keyword: '',
  pageNum: 1,
  pageSize: 10
})

// 新增弹窗
const addDialogVisible = ref(false)
const addForm = reactive({
  username: '',
  password: '',
  nickname: '',
  realName: '',
  credit: 100,
  role: '普通',
  status: 'ACTIVE'
})

// 编辑弹窗
const editDialogVisible = ref(false)
const editForm = reactive({
  id: null,
  username: '',
  nickname: '',
  realName: '',
  credit: 100,
  role: '普通',
  status: 'ACTIVE'
})

// 获取所有读者（前端分页模式）
const fetchReaders = async () => {
  try {
    const res = await getReaders()
    const records = res.data.records || res.data || []

    allReaders.value = records.map(r => ({
      id: r.ReaderID,
      username: r.UserName,
      nickname: r.NickName,
      realName: r.FullName,
      credit: r.CreditScore,
      status: r.AccountStatus === '正常' ? 'ACTIVE' : 'FROZEN',
      role: r.Permission
    }))

    total.value = allReaders.value.length
    query.pageNum = 1
    updatePageData()
  } catch (e) {
    console.error(e)
  }
}

// 更新当前页显示数据
const updatePageData = () => {
  const start = (query.pageNum - 1) * query.pageSize
  const end = start + query.pageSize
  readers.value = allReaders.value.slice(start, end)
}

// 前端搜索
const searchReaders = () => {
  const keyword = query.keyword.trim().toLowerCase()
  if (!keyword) {
    fetchReaders()
    return
  }
  const filtered = allReaders.value.filter(r => r.username.toLowerCase().includes(keyword))
  total.value = filtered.length
  readers.value = filtered.slice(0, query.pageSize)
  query.pageNum = 1
}

// 打开新增弹窗
const openAddDialog = () => {
  Object.assign(addForm, {
    username: '',
    password: '',
    nickname: '',
    realName: '',
    credit: 100,
    role: '普通',
    status: 'ACTIVE'
  })
  addDialogVisible.value = true
}

// 打开编辑弹窗
const openEditDialog = async (id) => {
  const res = await getReaderById(id)
  Object.assign(editForm, {
    id: res.data.ReaderID,
    username: res.data.UserName,
    nickname: res.data.NickName,
    realName: res.data.FullName,
    credit: res.data.CreditScore,
    role: res.data.Permission,
    status: res.data.AccountStatus === '正常' ? 'ACTIVE' : 'FROZEN'
  })
  editDialogVisible.value = true
}

// 保存新增
const saveNewReader = async () => {
  if (!addForm.username) {
    ElMessage.warning('请输入用户名')
    return
  }

  const exists = allReaders.value.some(r => r.username === addForm.username)
  if (exists) {
    ElMessage.error('用户名已存在，请使用其他用户名')
    return
  }

  const payload = {
    userName: addForm.username,
    password: addForm.password,
    nickName: addForm.nickname,
    fullName: addForm.realName,
    creditScore: addForm.credit,
    permission: addForm.role,
    accountStatus: addForm.status === 'ACTIVE' ? '正常' : '冻结'
  }

  await addReader(payload)
  ElMessage.success('新增成功')
  addDialogVisible.value = false
  await fetchReaders()
}

// 保存编辑
const saveEditedReader = async () => {
  if (!editForm.username) {
    ElMessage.warning('用户名不能为空')
    return
  }

  const exists = allReaders.value.some(r => r.username === editForm.username && r.id !== editForm.id)
  if (exists) {
    ElMessage.error('用户名已存在，请使用其他用户名')
    return
  }

  await updateReaderPartial({
    readerID: editForm.id,
    userName: editForm.username,
    nickName: editForm.nickname,
    fullName: editForm.realName,
    creditScore: editForm.credit,
    permission: editForm.role,
    accountStatus: editForm.status === 'ACTIVE' ? '正常' : '冻结'
  })
  ElMessage.success('编辑成功')
  editDialogVisible.value = false
  await fetchReaders()
}

// 冻结/解冻
const toggleFreeze = async (row) => {
  await updateReaderPartial({
    readerID: row.id,
    accountStatus: row.status === 'ACTIVE' ? '冻结' : '正常'
  })
  ElMessage.success(row.status === 'ACTIVE' ? '已冻结' : '已解冻')
  await fetchReaders()
}

// 删除读者
const removeReader = (id) => {
  ElMessageBox.confirm('确认删除该读者吗？', '提示', { type: 'warning' })
      .then(async () => {
        try {
          await deleteReader(id)
          ElMessage.success('删除成功')
        } catch (e) {
          ElMessage.error('违反完整性约束，不可删除')
        }
        await fetchReaders()
      })
      .catch(() => {})
}

// 带确认框的重置密码
const confirmResetPassword = (username) => {
  ElMessageBox.confirm(`确定要重置用户 ${username} 的密码吗？`, '重置密码', {
    type: 'warning'
  })
      .then(async () => {
        try {
          await reset(username)
          const defaultPassword = '1234567890'
          ElMessage.success(`用户 ${username} 的密码已重置成功，默认密码为 ${defaultPassword}`)
        } catch (e) {
          console.error(e)
          ElMessage.error(`用户 ${username} 的密码重置失败`)
        }
      })
      .catch(() => {})
}

onMounted(fetchReaders)
</script>

<style scoped>
.reader-management {
  padding: 20px;
}
.search-bar {
  margin-bottom: 15px;
  display: flex;
  gap: 10px;
}
.pagination {
  margin-top: 15px;
  text-align: right;
}

.operation-cell {
  display: flex;
  justify-content: space-between; /* 平均分布 */
  gap: 5px; /* 按钮间距 */
}
.operation-cell .el-button {
  flex: 1; /* 每个按钮等宽，占满整个单元格 */
  min-width: 60px; /* 可选，防止按钮太小 */
}

</style>

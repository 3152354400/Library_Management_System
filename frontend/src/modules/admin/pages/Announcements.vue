<template>
  <div class="ad-page">
    <PageHeader title="公告管理" subtitle="发布与维护图书馆公告" />

    <div class="card">
      <div class="filter-bar">
        <el-radio-group v-model="filter" size="default">
          <el-radio-button value="all">全部</el-radio-button>
          <el-radio-button value="发布中">发布中</el-radio-button>
          <el-radio-button value="已撤回">已撤回</el-radio-button>
        </el-radio-group>
        <el-button type="primary" :icon="Plus" style="margin-left:auto" @click="onCreate">发布公告</el-button>
      </div>

      <el-table :data="filtered" v-loading="loading" stripe>
        <el-table-column prop="Title" label="标题" min-width="240" />
        <el-table-column label="面向" prop="TargetGroup" width="100" />
        <el-table-column label="内容预览" min-width="320" show-overflow-tooltip>
          <template #default="{ row }">{{ row.Content }}</template>
        </el-table-column>
        <el-table-column label="发布时间" width="160">
          <template #default="{ row }">{{ formatDate(row.CreateTime) }}</template>
        </el-table-column>
        <el-table-column label="状态" width="120">
          <template #default="{ row }"><StatusTag :status="row.Status" /></template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.Status === '发布中'" text type="warning" @click="onTakedown(row)">撤回</el-button>
          </template>
        </el-table-column>
      </el-table>
      <EmptyState v-if="!loading && !filtered.length" title="暂无公告" />
    </div>

    <el-dialog v-model="dialog" title="发布公告" width="560px">
      <el-form :model="form" label-width="80px">
        <el-form-item label="标题"><el-input v-model="form.Title" /></el-form-item>
        <el-form-item label="面向">
          <el-radio-group v-model="form.TargetGroup">
            <el-radio value="所有人">所有人</el-radio>
            <el-radio value="读者">读者</el-radio>
            <el-radio value="管理员">管理员</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="内容">
          <el-input v-model="form.Content" type="textarea" :rows="6" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialog = false">取消</el-button>
        <el-button type="primary" @click="onSubmit">发布</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { announcementApi } from '@/api/announcement'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const filter = ref('all')
const list = ref([])
const loading = ref(false)
const dialog = ref(false)
const form = reactive({ Title: '', TargetGroup: '所有人', Content: '' })

const filtered = computed(() => {
  if (filter.value === 'all') return list.value
  return list.value.filter((a) => a.Status === filter.value)
})

async function load() {
  loading.value = true
  try { list.value = await announcementApi.all() }
  finally { loading.value = false }
}

function onCreate() {
  Object.assign(form, { Title: '', TargetGroup: '所有人', Content: '' })
  dialog.value = true
}

async function onSubmit() {
  if (!form.Title || !form.Content) return ElMessage.warning('请填写完整')
  await announcementApi.create({ ...form })
  ElMessage.success('已发布')
  dialog.value = false
  load()
}

async function onTakedown(row) {
  await ElMessageBox.confirm(`确认撤回《${row.Title}》?`, '提示', { type: 'warning' }).catch(() => {})
  await announcementApi.takedown(row.AnnouncementID)
  ElMessage.success('已撤回')
  load()
}

function formatDate(t) { return t ? new Date(t).toLocaleString('zh-CN') : '' }
onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.filter-bar { display: flex; margin-bottom: 16px; }
</style>